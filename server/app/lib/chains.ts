import { defineChain } from 'viem'

// Flow Testnet Configuration
export const flowTestnet = defineChain({
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
      http: ['https://rest-testnet.onflow.org'],
    },
    public: {
      http: ['https://rest-testnet.onflow.org'],
    },
  },
  blockExplorers: {
    default: { 
      name: 'Flow Testnet Explorer', 
      url: 'https://testnet.flowscan.org' 
    },
  },
  testnet: true,
})

// Chiliz Spicy Testnet Configuration  
export const chilizSpicy = defineChain({
  id: 88882,
  name: 'Chiliz Spicy Testnet',
  network: 'chiliz-spicy',
  nativeCurrency: {
    decimals: 18,
    name: 'Chiliz',
    symbol: 'CHZ',
  },
  rpcUrls: {
    default: {
      http: ['https://chiliz-spicy.publicnode.com'],
    },
    public: {
      http: ['https://chiliz-spicy.publicnode.com'],
    },
  },
  blockExplorers: {
    default: { 
      name: 'Chiliz Spicy Explorer', 
      url: 'https://testnet.chiliscan.com' 
    },
  },
  testnet: true,
})

// Anvil (local development)
export const anvil = defineChain({
  id: 31337,
  name: 'Anvil',
  network: 'foundry',
  nativeCurrency: {
    decimals: 18,
    name: 'Ether',
    symbol: 'ETH',
  },
  rpcUrls: {
    default: {
      http: ['http://127.0.0.1:8545'],
    },
    public: {
      http: ['http://127.0.0.1:8545'],
    },
  },
  testnet: true,
})

// Export all supported chains
export const supportedChains = [flowTestnet, chilizSpicy, anvil] as const;

// Chain utilities
export function getChainById(chainId: number) {
  return supportedChains.find(chain => chain.id === chainId);
}

export function getContractAddress(chainId: number): string {
  switch (chainId) {
    case 545: // Flow Testnet
      return process.env.NEXT_PUBLIC_FLOW_TESTNET_CONTRACT_ADDRESS || '';
    case 88882: // Chiliz Spicy
      return process.env.NEXT_PUBLIC_CHILIZ_TESTNET_CONTRACT_ADDRESS || '';
    case 31337: // Anvil
      return process.env.NEXT_PUBLIC_ANVIL_CONTRACT_ADDRESS || process.env.NEXT_PUBLIC_CONTRACT_ADDRESS || '';
    default:
      return process.env.NEXT_PUBLIC_CONTRACT_ADDRESS || '';
  }
}

// Faucet URLs
export const FAUCETS = {
  flow: 'https://testnet-faucet.onflow.org',
  chiliz: 'https://spicy-faucet.chiliz.com',
  anvil: '', // No faucet needed for local development
};

export function getFaucetUrl(chainId: number): string {
  switch (chainId) {
    case 545: return FAUCETS.flow;
    case 88882: return FAUCETS.chiliz;
    case 31337: return FAUCETS.anvil;
    default: return '';
  }
}
