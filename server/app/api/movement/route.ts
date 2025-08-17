import { pg } from "@/app/pg";
import { NextRequest, NextResponse } from "next/server";
import { z } from "zod";
import { formatUnits } from "viem";
import * as RivalsToken from "../../../RivalsToken.json";
import { getClientsByChainId, SupportedChainId } from "@/app/clients";

const schema = z.object({
    username: z.string(),
    latitude: z.number(),
    longitude: z.number(),
    chainId: z.string().optional() as z.ZodOptional<z.ZodType<SupportedChainId>>,
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
    
    // Get user info and balance
    let userBalance = "0";
    const userResults = await db`SELECT id, evm_address FROM users WHERE username = ${movement.username} LIMIT 1`;
    if (userResults.length > 0) {
        const userAddress = userResults[0].evm_address as `0x${string}`;
        
        // Get balance from smart contract
        const { publicClient, contractAddress } = getClientsByChainId(movement.chainId);
        
        try {
            const balance = await publicClient.readContract({
                address: contractAddress as `0x${string}`,
                abi: RivalsToken.abi,
                functionName: "balanceOf",
                args: [userAddress],
            }) as bigint;
            
            // Convert from wei to tokens using viem's formatUnits
            userBalance = parseFloat(formatUnits(balance, 18)).toFixed(2);
        } catch (error) {
            console.error("Error fetching balance:", error);
            userBalance = "0";
        }
    }
    
    // First, let's get all traps and compute distances to debug
    const allTraps = await db`
        SELECT *, 
               location <@> point(${movement.longitude}, ${movement.latitude}) as distance_miles
        FROM traps
        ORDER BY distance_miles ASC
    `;
    console.log("All traps with distances: ", JSON.stringify(allTraps, null, 2));

    const range = 0.004;
    
    // Now filter with a more liberal range (1 mile = 1609.34 meters)
    const traps = await db`
        SELECT * FROM traps
        WHERE location <@> point(${movement.longitude}, ${movement.latitude}) <= ${range}
    `;
    console.log("Traps within threshold: ", JSON.stringify(traps));

    return NextResponse.json({ 
        traps,
        balance: userBalance 
    });
}
