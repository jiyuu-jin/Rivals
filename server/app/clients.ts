import { Chain, createPublicClient, createWalletClient, http } from "viem";
import { privateKeyToAccount } from "viem/accounts";
import { anvil, chiliz, flowMainnet, flowTestnet, spicy } from "viem/chains";

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

// Chain mapping for dynamic selection
export const SUPPORTED_CHAINS = {
    anvil: anvil,
    flow_mainnet: flowMainnet,
    flow_testnet: flowTestnet,
    chiliz_mainnet: chiliz,
    chiliz_testnet: spicy,
} as const;

export type SupportedChainId = keyof typeof SUPPORTED_CHAINS;

// Original function for backward compatibility
export function getClients() {
    return getClientsForChain(defaultChain);
}

// New function that accepts dynamic chain selection
export function getClientsForChain(chain: Chain) {
    return {
        publicClient: createPublicClient({
            chain,
            transport: http(),
        }),
        walletClient: createWalletClient({
            chain,
            transport: http(),
            account: privateKeyToAccount(process.env.PRIVATE_KEY as `0x${string}`),
        }),
    }
}

// Helper function to get clients by chain ID
export function getClientsByChainId(chainId?: SupportedChainId) {
    const chain = chainId ? SUPPORTED_CHAINS[chainId] : defaultChain;
    return getClientsForChain(chain);
}
