'use client';

import { cookieStorage, createStorage } from 'wagmi';
import { WagmiAdapter } from '@reown/appkit-adapter-wagmi';
import { supportedChains } from '@/app/lib/chains';

// Get project ID from environment
export const projectId = process.env.NEXT_PUBLIC_REOWN_PROJECT_ID;

if (!projectId) {
  throw new Error('NEXT_PUBLIC_REOWN_PROJECT_ID is not set');
}

// Create the Wagmi adapter
export const wagmiAdapter = new WagmiAdapter({
  storage: createStorage({ storage: cookieStorage }),
  ssr: true,
  projectId: projectId!,
  networks: [...supportedChains],
});

// Export the wagmi config
export const config = wagmiAdapter.wagmiConfig;

// App metadata
export const metadata = {
  name: 'Rivals - AR Zombie Game',
  description: 'Battle zombies and compete on the global leaderboard',
  url: 'https://rivals.game', // Update with your actual URL
  icons: ['/logo.jpeg'],
};