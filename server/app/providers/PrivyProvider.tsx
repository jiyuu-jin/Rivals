'use client';

import { PrivyProvider as BasePrivyProvider } from '@privy-io/react-auth';
import { WagmiProvider } from '@privy-io/wagmi';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { wagmiConfig, supportedChains } from '@/app/lib/wagmi-config';
import { ReactNode, useState } from 'react';

interface PrivyProviderProps {
  children: ReactNode;
}

export function PrivyProvider({ children }: PrivyProviderProps) {
  const [queryClient] = useState(() => new QueryClient());

  return (
    <BasePrivyProvider
      appId={process.env.NEXT_PUBLIC_PRIVY_APP_ID || 'default-app-id'}
      config={{
        loginMethods: ['wallet', 'email'],
        appearance: {
          theme: 'dark',
          accentColor: '#FF4444',
          logo: '/logo.jpeg',
        },
        embeddedWallets: {
          createOnLogin: 'users-without-wallets',
        },
        supportedChains: supportedChains,
      }}
    >
      <QueryClientProvider client={queryClient}>
        <WagmiProvider config={wagmiConfig}>
          {children}
        </WagmiProvider>
      </QueryClientProvider>
    </BasePrivyProvider>
  );
}
