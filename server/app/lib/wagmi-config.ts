'use client';

import { createConfig, http } from 'wagmi';
import { supportedChains, flowTestnet, chilizSpicy, anvil } from './chains';

// Create wagmi configuration
export const wagmiConfig = createConfig({
  chains: supportedChains,
  transports: {
    [flowTestnet.id]: http(),
    [chilizSpicy.id]: http(),
    [anvil.id]: http(),
  },
});

// Re-export for convenience
export { supportedChains };
