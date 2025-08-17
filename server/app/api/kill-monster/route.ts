import { NextRequest, NextResponse } from "next/server";
import { createPublicClient, createWalletClient, http, parseUnits } from "viem";
import { privateKeyToAccount } from "viem/accounts";
import { anvil } from "viem/chains";
import { z } from "zod";
import * as RivalToken from "../../../RivalToken.json";
import { pg } from "@/app/pg";

const schema = z.object({
    username: z.string(),
});

export async function POST(request: NextRequest) {
    const body = await request.json();
    const parsed = schema.safeParse(body);
    if (!parsed.success) {
        return NextResponse.json({ error: parsed.error.message }, { status: 400 });
    }

    const movement = parsed.data;
    console.log(movement);

    const db = pg();
    const results = await db`SELECT id, evm_address FROM users WHERE username = ${movement.username} LIMIT 1`;
    if (results.length === 0) {
        return NextResponse.json({ error: "User not found" }, { status: 404 });
    }
    const address = results[0].evm_address;
    const userId = results[0].id;

    await db`UPDATE users SET kill_count = kill_count + 1, last_active = CURRENT_TIMESTAMP WHERE id = ${userId}`;

    const client = createWalletClient({
        chain: anvil,
        transport: http(),
        account: privateKeyToAccount(process.env.PRIVATE_KEY as `0x${string}`),
    });
    const publicClient = createPublicClient({
        chain: anvil,
        transport: http(),
    });

    const hash = await client.writeContract({
        address: process.env.CONTRACT_ADDRESS as `0x${string}`,
        abi: RivalToken.abi,
        functionName: "killMonster",
        args: [address, parseUnits("1", 18)],
    });
    const receipt = await publicClient.waitForTransactionReceipt({ hash });
    console.log({ receipt });
    if (receipt.status === "success") {
        return NextResponse.json({});
    } else {
        return NextResponse.json({ error: "Transaction failed" }, { status: 500 });
    }
}
