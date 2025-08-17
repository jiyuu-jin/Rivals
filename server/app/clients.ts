import { Chain, createPublicClient, createWalletClient, http } from "viem";
import { privateKeyToAccount } from "viem/accounts";
import { anvil, chiliz, flowMainnet, flowTestnet, spicy } from "viem/chains";

// Chain configuration with separate contract addresses and private keys
export const CHAIN_CONFIG = {
    anvil: {
        chain: anvil,
        contractAddress: process.env.ANVIL_CONTRACT_ADDRESS || process.env.CONTRACT_ADDRESS,
        privateKey: process.env.ANVIL_PRIVATE_KEY || process.env.PRIVATE_KEY,
    },
    flow_mainnet: {
        chain: flowMainnet,
        contractAddress: process.env.FLOW_MAINNET_CONTRACT_ADDRESS,
        privateKey: process.env.FLOW_MAINNET_PRIVATE_KEY,
    },
    flow_testnet: {
        chain: flowTestnet,
        contractAddress: process.env.FLOW_TESTNET_CONTRACT_ADDRESS,
        privateKey: process.env.FLOW_TESTNET_PRIVATE_KEY,
    },
    chiliz_mainnet: {
        chain: chiliz,
        contractAddress: process.env.CHILIZ_MAINNET_CONTRACT_ADDRESS,
        privateKey: process.env.CHILIZ_MAINNET_PRIVATE_KEY,
    },
    chiliz_testnet: {
        chain: spicy,
        contractAddress: process.env.CHILIZ_TESTNET_CONTRACT_ADDRESS,
        privateKey: process.env.CHILIZ_TESTNET_PRIVATE_KEY,
    },
} as const;

// Default chain based on environment variables (for backward compatibility)
let defaultChain: Chain;
if (process.env.CHAIN_FLOW_MAINNET === "true") {
    defaultChain = flowMainnet;
} else if (process.env.CHAIN_FLOW_TESTNET === "true") {
    defaultChain = flowTestnet;
} else if (process.env.CHAIN_CHILIZ_MAINNET === "true") {
    defaultChain = chiliz;
} else if (process.env.CHAIN_CHILIZ_TESTNET === "true") {
    defaultChain = spicy;
} else {
    defaultChain = anvil;
}

export type SupportedChainId = keyof typeof CHAIN_CONFIG;

// Original function for backward compatibility
export function getClients() {
    return getClientsForChain(defaultChain);
}

// New function that accepts dynamic chain selection
export function getClientsForChain(chain: Chain, privateKey?: string) {
    return {
        publicClient: createPublicClient({
            chain,
            transport: http(),
        }),
        walletClient: createWalletClient({
            chain,
            transport: http(),
            account: privateKeyToAccount((privateKey || process.env.PRIVATE_KEY) as `0x${string}`),
        }),
    }
}

// Helper function to get clients by chain ID with proper config
export function getClientsByChainId(chainId?: SupportedChainId) {
    const config = chainId ? CHAIN_CONFIG[chainId] : CHAIN_CONFIG.anvil;
    return {
        ...getClientsForChain(config.chain, config.privateKey),
        contractAddress: config.contractAddress,
    };
}
