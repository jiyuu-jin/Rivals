import { pg } from "@/app/pg";
import { NextRequest, NextResponse } from "next/server";
import { z } from "zod";

const schema = z.object({
  movement: z.object({
    latitude: z.number(),
    longitude: z.number(),
  }),
});

export async function POST(request: NextRequest) {
  const body = await request.json();
  const parsed = schema.safeParse(body);
  if (!parsed.success) {
    return NextResponse.json({ error: parsed.error.message }, { status: 400 });
  }

  const { movement } = parsed.data;
  console.log(movement);

  const db = pg();
  const result = await db`
    SELECT * FROM traps
    WHERE earth_distance(location, point(${movement.longitude}, ${movement.latitude})) <= 1000
  `;

  return NextResponse.json({ message: "Movement received", result });
}
