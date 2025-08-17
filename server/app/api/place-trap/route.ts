import { pg } from "@/app/pg";
import { NextRequest, NextResponse } from "next/server";
import { createPublicClient, createWalletClient, http, parseUnits } from "viem";
import { anvil } from "viem/chains";
import { z } from "zod";
import * as RivalToken from "../../../RivalToken.json";
import { privateKeyToAccount } from "viem/accounts";
import { getClients } from "@/app/clients";

const trapSchema = z.object({
  owner_username: z.string(),
  latitude: z.number(),
  longitude: z.number(),
});

export async function POST(request: NextRequest) {
  try {
    const body = await request.json();
    const parsed = trapSchema.safeParse(body);

    if (!parsed.success) {
      return NextResponse.json({ error: parsed.error.message }, { status: 400 });
    }

    const { owner_username, latitude, longitude } = parsed.data;

    const db = pg();
    const results = await db`SELECT id, evm_address FROM users WHERE username = ${owner_username} LIMIT 1`;
    if (results.length === 0) {
      return NextResponse.json({ error: "User not found" }, { status: 404 });
    }
    const address = results[0].evm_address as `0x${string}`;

    const { publicClient, walletClient } = getClients();
    const balance = await publicClient.readContract({
      address: process.env.CONTRACT_ADDRESS as `0x${string}`,
      abi: RivalToken.abi,
      functionName: "balanceOf",
      args: [address],
    }) as bigint;
    if (balance < parseUnits("1", 18)) {
      return NextResponse.json({ error: "Insufficient balance" }, { status: 400 });
    }

    const hash = await walletClient.writeContract({
      address: process.env.CONTRACT_ADDRESS as `0x${string}`,
      abi: RivalToken.abi,
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
      VALUES (${results[0].id}, point(${longitude}, ${latitude}))
    `;

    return NextResponse.json({
      message: "Trap created successfully",
      trap: result[0]
    }, { status: 201 });

  } catch (error) {
    console.error("Error creating trap:", error);
    return NextResponse.json({
      error: "Failed to create trap"
    }, { status: 500 });
  }
}
