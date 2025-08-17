import { Chain, createPublicClient, createWalletClient, http } from "viem";
import { privateKeyToAccount } from "viem/accounts";
import { anvil, chiliz, flowMainnet, flowTestnet, spicy } from "viem/chains";

let chain: Chain;
if (process.env.CHAIN_FLOW_MAINNET === "true") {
    chain = flowMainnet;
} else if (process.env.CHAIN_FLOW_TESTNET === "true") {
    chain = flowTestnet;
} else if (process.env.CHAIN_CHILIZ_MAINNET === "true") {
    chain = chiliz;
} else if (process.env.CHAIN_CHILIZ_TESTNET === "true") {
    chain = spicy;
} else {
    chain = anvil;
}

export function getClients() {
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
