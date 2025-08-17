'use client';

import { useReadContract, useAccount } from 'wagmi';
import { getContractAddress } from '@/app/lib/chains';
import { formatUnits } from 'viem';

const RIVALS_TOKEN_ABI = [
  {
    "inputs": [{ "internalType": "address", "name": "account", "type": "address" }],
    "name": "balanceOf",
    "outputs": [{ "internalType": "uint256", "name": "", "type": "uint256" }],
    "stateMutability": "view",
    "type": "function"
  },
  {
    "inputs": [],
    "name": "decimals",
    "outputs": [{ "internalType": "uint8", "name": "", "type": "uint8" }],
    "stateMutability": "view",
    "type": "function"
  }
] as const;

export function useTokenBalance() {
  const { address, chainId, isConnected } = useAccount();

  const contractAddress = chainId ? getContractAddress(chainId) : undefined;

  const { data: balance, isError, isLoading } = useReadContract({
    abi: RIVALS_TOKEN_ABI,
    address: contractAddress as `0x${string}`,
    functionName: 'balanceOf',
    args: address ? [address] : undefined,
    query: {
      enabled: !!(isConnected && address && contractAddress),
      refetchInterval: 5000, // Refetch every 5 seconds
    }
  });

  const { data: decimals } = useReadContract({
    abi: RIVALS_TOKEN_ABI,
    address: contractAddress as `0x${string}`,
    functionName: 'decimals',
    query: {
      enabled: !!contractAddress,
    }
  });

  // Format the balance to a human-readable number
  const formattedBalance = balance && decimals
    ? formatUnits(balance, decimals)
    : '0';

  // Convert to number and format with commas
  const displayBalance = parseFloat(formattedBalance).toLocaleString(undefined, {
    maximumFractionDigits: 0
  });

  return {
    balance: displayBalance,
    rawBalance: balance,
    isLoading,
    isError,
    isConnected,
    hasContract: !!contractAddress
  };
}
