import { NextRequest, NextResponse } from "next/server";
import { z } from "zod";
import RivalsToken from "../../../RivalsToken.json";
import { pg } from "@/app/pg";
import { getClientsByChainId, SupportedChainId } from "@/app/clients";
import { requireAuth, getOrCreateUser } from "@/app/lib/auth";

const schema = z.object({
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
        console.log("Kill monster from authenticated user:", session.address, data);

        const db = pg();
        const user = await getOrCreateUser(db, session.address);
        const address = session.address;

        await db`UPDATE users SET kill_count = kill_count + 1, last_active = CURRENT_TIMESTAMP WHERE id = ${user.id}`;

        const { publicClient, walletClient, contractAddress } = getClientsByChainId(data.chainId);

        const hash = await walletClient.writeContract({
            address: contractAddress as `0x${string}`,
            abi: RivalsToken.abi,
            functionName: "killMonster",
            args: [address],
        });
        const receipt = await publicClient.waitForTransactionReceipt({ hash });
        console.log({ receipt });
        if (receipt.status === "success") {
            return NextResponse.json({});
        } else {
            return NextResponse.json({ error: "Transaction failed" }, { status: 500 });
        }
    } catch (error) {
        console.error("Kill monster API error:", error);
        
        if (error instanceof Error && error.message === "Authentication required") {
            return NextResponse.json({ error: "Authentication required" }, { status: 401 });
        }
        
        return NextResponse.json({ error: "Internal server error" }, { status: 500 });
    }
}
