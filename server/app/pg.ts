import "server-only";
import postgres from "postgres";

export type Pg = ReturnType<typeof postgres>;

let instance: Pg | null = null;

export function pg() {
  if (instance) return instance;
  instance = postgres(process.env.POSTGRES_URI!, {
    max: 1,
  });
  return instance;
}
