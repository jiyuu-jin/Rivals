import { pg } from "@/app/pg";
import { NextRequest, NextResponse } from "next/server";
import { z } from "zod";

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
    const result = await db`
      INSERT INTO traps (owner, location)
      SELECT u.id, point(${longitude}, ${latitude})
      FROM users u
      WHERE u.username = ${owner_username}
      RETURNING id, location
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
