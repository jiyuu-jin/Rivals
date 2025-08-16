import { pg } from "@/app/pg";
import { NextRequest, NextResponse } from "next/server";
import { z } from "zod";

const schema = z.object({
    latitude: z.number(),
    longitude: z.number(),
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
    // First, let's get all traps and compute distances to debug
    const allTraps = await db`
        SELECT *, 
               location <@> point(${movement.longitude}, ${movement.latitude}) as distance_miles
        FROM traps
        ORDER BY distance_miles ASC
    `;
    console.log("All traps with distances: ", JSON.stringify(allTraps, null, 2));

    const range = 0.005;
    
    // Now filter with a more liberal range (1 mile = 1609.34 meters)
    const result = await db`
        SELECT * FROM traps
        WHERE location <@> point(${movement.longitude}, ${movement.latitude}) <= ${range}
    `;
    console.log("Traps within threshold: ", JSON.stringify(result));

    return NextResponse.json({ message: "Movement received", result });
}
