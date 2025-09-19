import { pg } from "@/app/pg";
import { NextRequest, NextResponse } from "next/server";
import { parseUnits } from "viem";
import { z } from "zod";
import RivalsToken from "../../../RivalsToken.json";
import { getClientsByChainId, SupportedChainId } from "@/app/clients";
import { requireAuth, getOrCreateUser } from "@/app/lib/auth";

const trapSchema = z.object({
  latitude: z.number(),
  longitude: z.number(),
  chainId: z.string().optional() as z.ZodOptional<z.ZodType<SupportedChainId>>,
});

export async function POST(request: NextRequest) {
  try {
    // Require authentication
    const session = await requireAuth(request);
    
    const body = await request.json();
    const parsed = trapSchema.safeParse(body);

    if (!parsed.success) {
      return NextResponse.json({ error: parsed.error.message }, { status: 400 });
    }

    const { latitude, longitude, chainId } = parsed.data;

    const db = pg();
    const user = await getOrCreateUser(db, session.address);
    const address = session.address as `0x${string}`;

    const { publicClient, walletClient, contractAddress } = getClientsByChainId(chainId);
    const balance = await publicClient.readContract({
      address: contractAddress as `0x${string}`,
      abi: RivalsToken.abi,
      functionName: "balanceOf",
      args: [address],
    }) as bigint;
    if (balance < parseUnits("1", 18)) {
      return NextResponse.json({ error: "Insufficient balance" }, { status: 400 });
    }

    const hash = await walletClient.writeContract({
      address: contractAddress as `0x${string}`,
      abi: RivalsToken.abi,
      functionName: "spend",
      args: [address, parseUnits("1", 18)],
    });
    const receipt = await publicClient.waitForTransactionReceipt({ hash });
    console.log({ receipt });
    if (receipt.status !== "success") {
      return NextResponse.json({ error: "Transaction failed" }, { status: 500 });
    }

    const result = await db`
      INSERT INTO traps (owner, location)
      VALUES (${user.id}, point(${longitude}, ${latitude}))
      RETURNING id, owner, location
    `;

    return NextResponse.json({
      message: "Trap created successfully",
      trap: result[0]
    }, { status: 201 });

  } catch (error) {
    console.error("Error creating trap:", error);
    
    if (error instanceof Error && error.message === "Authentication required") {
      return NextResponse.json({ error: "Authentication required" }, { status: 401 });
    }
    
    return NextResponse.json({
      error: "Failed to create trap"
    }, { status: 500 });
  }
}
