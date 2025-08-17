import { createPublicClient, createWalletClient, http } from "viem";
import { privateKeyToAccount } from "viem/accounts";
import { anvil } from "viem/chains";

export function getClients() {
    return {
        publicClient: createPublicClient({
            chain: anvil,
            transport: http(),
        }),
        walletClient: createWalletClient({
            chain: anvil,
            transport: http(),
            account: privateKeyToAccount(process.env.PRIVATE_KEY as `0x${string}`),
        }),
    }
}