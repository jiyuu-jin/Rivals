'use client';

import { Stack, Text, Button, Image } from '@mantine/core';
import { useTokenBalance } from '@/app/hooks/useTokenBalance';
import styles from './PlayerProfile.module.css';

export function PlayerProfile() {
  const { balance, isLoading, isError, isConnected, hasContract } = useTokenBalance();

  return (
    <div className={styles.container}>
      <Stack gap={24} align="center">
        <div className={styles.avatarContainer}>
          <div className={styles.logoIcon}>
            <img 
              src="/logo.jpeg" 
              alt="Rivals Logo" 
              width={80} 
              height={80}
              className={styles.logoImage}
            />
          </div>
        </div>
        
        <Stack gap={8} align="center">
          <Text className={styles.username}>RivalHunter01</Text>
          <Text className={styles.rank}>#17</Text>
        </Stack>

        <div className={styles.tokensContainer}>
          <div className={styles.tokenIcon}>+</div>
          <Text className={styles.tokenAmount}>
            {!isConnected || !hasContract ? '0' : 
             isLoading ? '...' : 
             isError ? '0' : 
             balance}
          </Text>
        </div>

        <Button 
          className={styles.loadoutButton}
          variant="subtle"
          fullWidth
        >
          EDIT LOADOUT
        </Button>
      </Stack>
    </div>
  );
}
