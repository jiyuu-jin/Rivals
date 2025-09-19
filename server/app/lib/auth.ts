import { NextRequest } from "next/server";
import { getToken } from "next-auth/jwt";

export interface AuthenticatedSession {
  address: string;
  chainId: number;
}

/**
 * Get the authenticated session from a NextRequest
 * Returns null if not authenticated
 */
export async function getAuthenticatedSession(
  request: NextRequest
): Promise<AuthenticatedSession | null> {
  try {
    const token = await getToken({ 
      req: request, 
      secret: process.env.NEXTAUTH_SECRET 
    });

    if (!token?.sub) {
      return null;
    }

    // Parse the token subject which contains chainId:address
    const [, chainId, address] = token.sub.split(":");
    
    if (!chainId || !address) {
      return null;
    }

    return {
      address: address.toLowerCase(), // Normalize to lowercase
      chainId: parseInt(chainId, 10),
    };
  } catch (error) {
    console.error("Error getting authenticated session:", error);
    return null;
  }
}

/**
 * Middleware function to require authentication for API routes
 * Returns the session if authenticated, throws error if not
 */
export async function requireAuth(request: NextRequest): Promise<AuthenticatedSession> {
  const session = await getAuthenticatedSession(request);
  
  if (!session) {
    throw new Error("Authentication required");
  }

  return session;
}

/**
 * Get or create user in database based on wallet address
 */
export async function getOrCreateUser(db: any, address: string) {
  // First try to find existing user
  const existingUser = await db`
    SELECT id, evm_address, username 
    FROM users 
    WHERE LOWER(evm_address) = ${address.toLowerCase()} 
    LIMIT 1
  `;

  if (existingUser.length > 0) {
    return existingUser[0];
  }

  // Create new user with address as username (can be updated later)
  const newUser = await db`
    INSERT INTO users (evm_address, username)
    VALUES (${address}, ${address})
    RETURNING id, evm_address, username
  `;

  return newUser[0];
}
