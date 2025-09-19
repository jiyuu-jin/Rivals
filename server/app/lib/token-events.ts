'use server';

import { createPublicClient, http, parseAbiItem, formatUnits, getAddress } from 'viem';
import { getContractAddress } from '@/app/lib/chains';
import { anvil } from '@/app/lib/chains';

export interface TokenHolder {
  address: string;
  balance: bigint;
  formattedBalance: string;
  rank: number;
}

export interface TransferEvent {
  blockNumber: bigint;
  transactionHash: string;
  from: string;
  to: string;
  value: bigint;
  formattedValue: string;
  timestamp?: number;
}

/**
 * Get token holders by analyzing Transfer events from the last 2 months
 */
export async function getTokenHoldersFromEvents(chainId?: number): Promise<TokenHolder[]> {
  try {
    console.log('[TOKEN EVENTS] Starting balance calculation from Transfer events');

    // Get blockchain client based on chainId
    let publicClient;
    let contractAddress;

    if (chainId === 545) {
      // Flow Testnet - use the correct EVM RPC endpoint
      publicClient = createPublicClient({
        chain: {
          id: 545,
          name: 'Flow Testnet',
          network: 'flow-testnet',
          nativeCurrency: {
            decimals: 18,
            name: 'Flow',
            symbol: 'FLOW',
          },
          rpcUrls: {
            default: {
              http: ['https://testnet.evm.nodes.onflow.org'],
            },
            public: {
              http: ['https://testnet.evm.nodes.onflow.org'],
            },
          },
          blockExplorers: {
            default: {
              name: 'Flow Testnet Explorer',
              url: 'https://evm-testnet.flowscan.io'
            },
          },
          testnet: true,
        },
        transport: http('https://testnet.evm.nodes.onflow.org'),
      });
      contractAddress = getContractAddress(545);

      // Temporary: if environment variable is not set, use the known contract address
      if (!contractAddress) {
        console.log('[TOKEN EVENTS] No contract address from environment, using known address');
        contractAddress = '0xc0BdCb2597984D3f0e356CBb01112782A9ECBEBe';
      }

      console.log('[TOKEN EVENTS] Using Flow Testnet EVM RPC: https://testnet.evm.nodes.onflow.org');
    } else {
      // Default to anvil for local development
      publicClient = createPublicClient({
        chain: anvil,
        transport: http('http://127.0.0.1:8545'),
      });
      contractAddress = getContractAddress(31337);
      console.log('[TOKEN EVENTS] Using Anvil local RPC: http://127.0.0.1:8545');
    }

    if (!contractAddress) {
      console.error('[TOKEN EVENTS] No contract address found for chainId:', chainId);
      return [];
    }

    console.log('[TOKEN EVENTS] Using contract address:', contractAddress);
    console.log('[TOKEN EVENTS] Target contract should be: 0xc0BdCb2597984D3f0e356CBb01112782A9ECBEBe');

    // Test RPC connection first
    try {
      const currentBlock = await publicClient.getBlockNumber();
      console.log('[TOKEN EVENTS] Current block:', currentBlock);
      console.log('[TOKEN EVENTS] RPC connection successful');
    } catch (rpcError) {
      console.error('[TOKEN EVENTS] RPC connection failed:', rpcError);
      return [];
    }

    // Get current block for logging, but search from contract deployment
    const currentBlock = await publicClient.getBlockNumber();
    console.log('[TOKEN EVENTS] Current block:', currentBlock);
    console.log('[TOKEN EVENTS] Searching from contract deployment (earliest) to latest block');

    // Get all Transfer events from contract deployment
    console.log('[TOKEN EVENTS] Fetching events with params:', {
      address: contractAddress,
      fromBlock: 'earliest',
      toBlock: 'latest'
    });

    let transferEvents;
    try {
      transferEvents = await publicClient.getLogs({
        address: contractAddress as `0x${string}`,
        event: parseAbiItem('event Transfer(address indexed from, address indexed to, uint256 value)'),
        fromBlock: 'earliest',
        toBlock: 'latest'
      });
      console.log('[TOKEN EVENTS] Successfully fetched', transferEvents.length, 'Transfer events');
    } catch (eventError) {
      console.error('[TOKEN EVENTS] Error fetching Transfer events from earliest:', eventError);

      // If earliest fails, try a smaller recent range to test connectivity
      console.log('[TOKEN EVENTS] Fallback: trying last 1000 blocks to test RPC...');
      try {
        const fallbackFromBlock = currentBlock > BigInt(1000) ? currentBlock - BigInt(1000) : BigInt(0);
        transferEvents = await publicClient.getLogs({
          address: contractAddress as `0x${string}`,
          event: parseAbiItem('event Transfer(address indexed from, address indexed to, uint256 value)'),
          fromBlock: fallbackFromBlock,
          toBlock: 'latest'
        });
        console.log('[TOKEN EVENTS] Fallback successful, found', transferEvents.length, 'Transfer events in recent blocks');
      } catch (fallbackError) {
        console.error('[TOKEN EVENTS] All attempts failed:', fallbackError);
        return [];
      }
    }

    // Calculate balances from events
    const balances = new Map<string, bigint>();
    const ZERO_ADDRESS = '0x0000000000000000000000000000000000000000';

    console.log('[TOKEN EVENTS] Processing', transferEvents.length, 'Transfer events...');

    for (const event of transferEvents) {
      const { from, to, value } = event.args!;

      // Ensure value is defined
      if (!value) {
        console.warn('[TOKEN EVENTS] Skipping event with undefined value');
        continue;
      }

      console.log('[TOKEN EVENTS] Processing Transfer:', {
        from,
        to,
        value: value.toString(),
        blockNumber: event.blockNumber?.toString(),
        txHash: event.transactionHash
      });

      // Subtract from sender (skip mint events where from = 0x0)
      if (from && from.toLowerCase() !== ZERO_ADDRESS.toLowerCase()) {
        const normalizedFrom = getAddress(from); // Normalize address case
        const currentBalance = balances.get(normalizedFrom) || BigInt(0);
        const newBalance = currentBalance - value;
        balances.set(normalizedFrom, newBalance);
        console.log('[TOKEN EVENTS] Updated balance for', normalizedFrom, 'from', currentBalance.toString(), 'to', newBalance.toString(), '(subtracted', value.toString(), ')');
      }

      // Add to receiver (skip burn events where to = 0x0)
      if (to && to.toLowerCase() !== ZERO_ADDRESS.toLowerCase()) {
        const normalizedTo = getAddress(to); // Normalize address case
        const currentBalance = balances.get(normalizedTo) || BigInt(0);
        const newBalance = currentBalance + value;
        balances.set(normalizedTo, newBalance);
        console.log('[TOKEN EVENTS] Updated balance for', normalizedTo, 'from', currentBalance.toString(), 'to', newBalance.toString(), '(added', value.toString(), ')');
      }
    }

    console.log('[TOKEN EVENTS] Final balance map has', balances.size, 'entries');

    // Convert to array and filter out zero/negative balances
    const holders: TokenHolder[] = [];
    let rank = 1;

    // Convert to array first, then sort
    const balanceArray = Array.from(balances.entries())
      .filter(([, balance]) => balance > BigInt(0))
      .sort(([, a], [, b]) => {
        if (a > b) return -1;
        if (a < b) return 1;
        return 0;
      });

    // Assign ranks and format
    for (const [address, balance] of balanceArray) {
      holders.push({
        address,
        balance,
        formattedBalance: formatUnits(balance, 18),
        rank: rank++
      });
    }

    console.log('[TOKEN EVENTS] Calculated balances for', holders.length, 'token holders');

    // Log top 5 holders for debugging
    console.log('[TOKEN EVENTS] Top 5 holders:');
    holders.slice(0, 5).forEach(holder => {
      console.log(`  ${holder.rank}. ${holder.address}: ${holder.formattedBalance} tokens`);
    });

    return holders;

  } catch (error) {
    console.error('[TOKEN EVENTS] Error fetching holders from events:', error);
    return [];
  }
}

/**
 * Get recent Transfer events for activity feed
 */
export async function getRecentTransfers(chainId?: number, limit = 50): Promise<TransferEvent[]> {
  try {
    // Get blockchain client based on chainId
    let publicClient;
    let contractAddress;

    if (chainId === 545) {
      // Flow Testnet - use the correct EVM RPC endpoint
      publicClient = createPublicClient({
        chain: {
          id: 545,
          name: 'Flow Testnet',
          network: 'flow-testnet',
          nativeCurrency: {
            decimals: 18,
            name: 'Flow',
            symbol: 'FLOW',
          },
          rpcUrls: {
            default: {
              http: ['https://testnet.evm.nodes.onflow.org'],
            },
            public: {
              http: ['https://testnet.evm.nodes.onflow.org'],
            },
          },
          blockExplorers: {
            default: {
              name: 'Flow Testnet Explorer',
              url: 'https://evm-testnet.flowscan.io'
            },
          },
          testnet: true,
        },
        transport: http('https://testnet.evm.nodes.onflow.org'),
      });
      contractAddress = getContractAddress(545);

      // Temporary: if environment variable is not set, use the known contract address
      if (!contractAddress) {
        contractAddress = '0xc0BdCb2597984D3f0e356CBb01112782A9ECBEBe';
      }
    } else {
      publicClient = createPublicClient({
        chain: anvil,
        transport: http('http://127.0.0.1:8545'),
      });
      contractAddress = getContractAddress(31337);
    }

    if (!contractAddress) {
      return [];
    }

    // Get recent Transfer events (last 1000 blocks)
    const transferEvents = await publicClient.getLogs({
      address: contractAddress as `0x${string}`,
      event: parseAbiItem('event Transfer(address indexed from, address indexed to, uint256 value)'),
      fromBlock: BigInt(-1000),
      toBlock: 'latest'
    });

    // Convert to our format and limit results
    const events: TransferEvent[] = transferEvents
      .slice(-limit) // Get last N events
      .reverse() // Most recent first
      .map(event => ({
        blockNumber: event.blockNumber!,
        transactionHash: event.transactionHash!,
        from: event.args!.from!,
        to: event.args!.to!,
        value: event.args!.value!,
        formattedValue: formatUnits(event.args!.value!, 18)
      }));

    return events;

  } catch (error) {
    console.error('[TOKEN EVENTS] Error fetching recent transfers:', error);
    return [];
  }
}

/**
 * Get balance for a specific address from events
 */
export async function getAddressBalanceFromEvents(address: string, chainId?: number): Promise<bigint> {
  try {
    const holders = await getTokenHoldersFromEvents(chainId);
    const normalizedAddress = getAddress(address);
    const holder = holders.find(h => h.address.toLowerCase() === normalizedAddress.toLowerCase());
    return holder ? holder.balance : BigInt(0);
  } catch (error) {
    console.error('[TOKEN EVENTS] Error getting address balance:', error);
    return BigInt(0);
  }
}