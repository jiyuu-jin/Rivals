import { NextRequest, NextResponse } from "next/server";
import { z } from "zod";
import RivalsToken from "../../../RivalsToken.json";
import { pg } from "@/app/pg";
import { getClientsByChainId, SupportedChainId } from "@/app/clients";
import { requireAuth, getOrCreateUser } from "@/app/lib/auth";

const schema = z.object({
    trapId: z.number().optional(),
    chainId: z.string().optional() as z.ZodOptional<z.ZodType<SupportedChainId>>,
});

export async function POST(request: NextRequest) {
    try {
        // Require authentication
        const session = await requireAuth(request);
        
        const body = await request.json();
        const parsed = schema.safeParse(body);
        if (!parsed.success) {
            return NextResponse.json({ error: parsed.error.message }, { status: 400 });
        }

        const data = parsed.data;

        const db = pg();
        const user = await getOrCreateUser(db, session.address);
        const address = session.address;

        const { publicClient, walletClient, contractAddress } = getClientsByChainId(data.chainId);

        if (data.trapId) {
            const trapResults = await db`SELECT owner FROM traps WHERE id = ${data.trapId} LIMIT 1`;
            if (trapResults.length === 0) {
                return NextResponse.json({ error: "Trap not found" }, { status: 404 });
            }
            const trap = trapResults[0];
            const otherUserId = trap.owner;
            const otherUser = await db`SELECT id, evm_address FROM users WHERE id = ${otherUserId} LIMIT 1`;
            await db`DELETE FROM traps WHERE id = ${data.trapId}`;
            const hash = await walletClient.writeContract({
                address: contractAddress as `0x${string}`,
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
                address: contractAddress as `0x${string}`,
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
    } catch (error) {
        console.error("Die API error:", error);
        
        if (error instanceof Error && error.message === "Authentication required") {
            return NextResponse.json({ error: "Authentication required" }, { status: 401 });
        }
        
        return NextResponse.json({ error: "Internal server error" }, { status: 500 });
    }
}
