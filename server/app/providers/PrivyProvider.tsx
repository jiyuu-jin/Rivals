'use client';

import { createAppKit } from '@reown/appkit/react';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { WagmiProvider, cookieToInitialState } from 'wagmi';
import { wagmiAdapter, projectId, metadata } from '@/app/config/appkit';
import { supportedChains } from '@/app/lib/chains';
import { ReactNode, useState } from 'react';

interface AppKitProviderProps {
  children: ReactNode;
  cookies?: string;
}

// Create the AppKit instance
const modal = createAppKit({
  adapters: [wagmiAdapter],
  projectId: projectId!,
  networks: [...supportedChains],
  defaultNetwork: supportedChains[0],
  metadata,
  features: {
    analytics: true,
  },
  themeMode: 'dark',
  themeVariables: {
    '--w3m-accent': '#FF4444',
    '--w3m-color-mix': '#FF4444',
    '--w3m-color-mix-strength': 20,
  },
});

export function AppKitProvider({ children, cookies }: AppKitProviderProps) {
  const [queryClient] = useState(() => new QueryClient());
  const initialState = cookieToInitialState(wagmiAdapter.wagmiConfig, cookies);

  return (
    <WagmiProvider config={wagmiAdapter.wagmiConfig} initialState={initialState}>
      <QueryClientProvider client={queryClient}>
        {children}
      </QueryClientProvider>
    </WagmiProvider>
  );
}

// Keep the old name as an alias for backward compatibility
export const PrivyProvider = AppKitProvider;
