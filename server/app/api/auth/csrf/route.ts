import { NextRequest, NextResponse } from "next/server";
import { randomBytes } from "crypto";

export async function GET(request: NextRequest) {
  try {
    // Generate a random nonce for SIWE
    // This is a simple implementation - in production you might want to store and validate nonces
    const nonce = randomBytes(16).toString('hex');
    
    if (!nonce) {
      return NextResponse.json({ error: "Failed to generate nonce" }, { status: 500 });
    }

    return NextResponse.json({ csrfToken: nonce });
  } catch (error) {
    console.error("Error generating nonce:", error);
    return NextResponse.json({ error: "Internal server error" }, { status: 500 });
  }
}
