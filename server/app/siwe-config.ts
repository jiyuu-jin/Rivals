import { getCsrfToken, signIn, signOut, getSession } from "next-auth/react";
import type {
  SIWEVerifyMessageArgs,
  SIWECreateMessageArgs,
  SIWESession,
} from "@reown/appkit-siwe";
import { createSIWEConfig, formatMessage } from "@reown/appkit-siwe";
import { mainnet, sepolia, arbitrum, polygon } from "@reown/appkit/networks";

export const siweConfig = createSIWEConfig({
  getMessageParams: async () => ({
    domain: typeof window !== "undefined" ? window.location.host : "",
    uri: typeof window !== "undefined" ? window.location.origin : "",
    chains: [mainnet.id, sepolia.id, arbitrum.id, polygon.id],
    statement: "Please sign with your account to authenticate with Rivals",
  }),
  createMessage: ({ address, ...args }: SIWECreateMessageArgs) =>
    formatMessage(args, address),
  getNonce: async () => {
    const nonce = await getCsrfToken();
    if (!nonce) {
      throw new Error("Failed to get nonce!");
    }

    return nonce;
  },
  getSession: async () => {
    const session = await getSession();
    if (!session) {
      return null;
    }

    // Validate address and chainId types
    if (
      typeof session.address !== "string" ||
      typeof session.chainId !== "number"
    ) {
      return null;
    }

    return {
      address: session.address,
      chainId: session.chainId,
    } satisfies SIWESession;
  },
  verifyMessage: async ({ message, signature }: SIWEVerifyMessageArgs) => {
    try {
      const success = await signIn("credentials", {
        message,
        redirect: false,
        signature,
        callbackUrl: "/",
      });

      return Boolean(success?.ok);
    } catch (error) {
      console.error("SIWE verification error:", error);
      return false;
    }
  },
  signOut: async () => {
    try {
      await signOut({
        redirect: false,
      });

      return true;
    } catch (error) {
      console.error("SIWE sign out error:", error);
      return false;
    }
  },
});
