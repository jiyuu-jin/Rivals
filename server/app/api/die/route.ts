import { NextRequest, NextResponse } from "next/server";
import { createPublicClient, createWalletClient, http, parseUnits } from "viem";
import { privateKeyToAccount } from "viem/accounts";
import { anvil } from "viem/chains";
import { z } from "zod";
import * as RivalsToken from "../../../RivalsToken.json";
import { pg } from "@/app/pg";
import { getClients } from "@/app/clients";

const schema = z.object({
    username: z.string(),
    trapId: z.number().optional(),
});

export async function POST(request: NextRequest) {
    const body = await request.json();
    const parsed = schema.safeParse(body);
    if (!parsed.success) {
        return NextResponse.json({ error: parsed.error.message }, { status: 400 });
    }

    const movement = parsed.data;

    const db = pg();
    const results = await db`SELECT id, evm_address FROM users WHERE username = ${movement.username} LIMIT 1`;
    if (results.length === 0) {
        return NextResponse.json({ error: "User not found" }, { status: 404 });
    }
    const address = results[0].evm_address;

    const { publicClient, walletClient } = getClients();

    if (movement.trapId) {
        const results = await db`SELECT owner FROM traps WHERE id = ${movement.trapId} LIMIT 1`;
        if (results.length === 0) {
            return NextResponse.json({ error: "User not found" }, { status: 404 });
        }
        const trap = results[0];
        const otherUserId = trap.owner;
        const otherUser = await db`SELECT id, evm_address FROM users WHERE id = ${otherUserId} LIMIT 1`;
        await db`DELETE FROM traps WHERE id = ${movement.trapId}`;
        const hash = await walletClient.writeContract({
            address: process.env.CONTRACT_ADDRESS as `0x${string}`,
            abi: RivalsToken.abi,
            functionName: "dieByTrap",
            args: [address, otherUser[0].evm_address],
        });
        const receipt = await publicClient.waitForTransactionReceipt({ hash });
        console.log({ receipt });
        if (receipt.status === "success") {
            return NextResponse.json({});
        } else {
            return NextResponse.json({ error: "Transaction failed" }, { status: 500 });
        }
    } else {
        const hash = await walletClient.writeContract({
            address: process.env.CONTRACT_ADDRESS as `0x${string}`,
            abi: RivalsToken.abi,
            functionName: "dieByMonster",
            args: [address],
        });
        const receipt = await publicClient.waitForTransactionReceipt({ hash });
        console.log({ receipt });
        if (receipt.status === "success") {
            return NextResponse.json({});
        } else {
            return NextResponse.json({ error: "Transaction failed" }, { status: 500 });
        }
    }
}
