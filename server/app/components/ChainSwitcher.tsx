'use client';

import { useSwitchChain, useChainId } from 'wagmi';
import { Select } from '@mantine/core';
import { supportedChains, getFaucetUrl } from '@/app/lib/chains';
import { useState } from 'react';
import styles from './ChainSwitcher.module.css';

export function ChainSwitcher() {
  const chainId = useChainId();
  const { switchChain, isPending } = useSwitchChain();
  const [isLoading, setIsLoading] = useState(false);

  const chainOptions = supportedChains.map(chain => ({
    value: chain.id.toString(),
    label: chain.name,
  }));

  const handleChainSwitch = async (value: string | null) => {
    if (!value) return;
    
    const newChainId = parseInt(value);
    if (newChainId === chainId) return;

    try {
      setIsLoading(true);
      await switchChain({ chainId: newChainId });
    } catch (error) {
      console.error('Failed to switch chain:', error);
    } finally {
      setIsLoading(false);
    }
  };

  const currentChain = supportedChains.find(chain => chain.id === chainId);

  return (
    <div className={styles.container}>
      <Select
        data={chainOptions}
        value={chainId?.toString() || ''}
        onChange={handleChainSwitch}
        placeholder="Select Network"
        className={styles.select}
        disabled={isPending || isLoading}
        size="sm"
      />
      
      {currentChain && currentChain.testnet && (
        <div className={styles.faucetInfo}>
          <text className={styles.faucetText}>
            Need test tokens? 
            {getFaucetUrl(chainId) && (
              <a 
                href={getFaucetUrl(chainId)} 
                target="_blank" 
                rel="noopener noreferrer"
                className={styles.faucetLink}
              >
                Get from faucet
              </a>
            )}
          </text>
        </div>
      )}
    </div>
  );
}
