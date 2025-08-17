'use client';

import { Stack, Text, Button, Image } from '@mantine/core';
import styles from './PlayerProfile.module.css';

export function PlayerProfile() {
  return (
    <div className={styles.container}>
      <Stack gap={24} align="center">
        <div className={styles.avatarContainer}>
          <div className={styles.skullIcon}>
            <svg width="80" height="80" viewBox="0 0 80 80" fill="none" xmlns="http://www.w3.org/2000/svg">
              <path d="M40 10C28 10 18 20 18 32C18 38 20 43 23 47C23 50 22 56 20 60C20 62 22 64 24 64H56C58 64 60 62 60 60C58 56 57 50 57 47C60 43 62 38 62 32C62 20 52 10 40 10Z" fill="#FF6B35"/>
              <path d="M30 28C30 31.3 27.3 34 24 34C20.7 34 18 31.3 18 28C18 24.7 20.7 22 24 22C27.3 22 30 24.7 30 28Z" fill="#1A1B1E"/>
              <path d="M62 28C62 31.3 59.3 34 56 34C52.7 34 50 31.3 50 28C50 24.7 52.7 22 56 22C59.3 22 62 24.7 62 28Z" fill="#1A1B1E"/>
              <path d="M40 40C38 40 36 42 36 44C36 46 38 48 40 48C42 48 44 46 44 44C44 42 42 40 40 40Z" fill="#1A1B1E"/>
              <path d="M32 52H48V56C48 58 46 60 44 60H36C34 60 32 58 32 56V52Z" fill="#1A1B1E"/>
            </svg>
          </div>
        </div>
        
        <Stack gap={8} align="center">
          <Text className={styles.username}>RivalHunter01</Text>
          <Text className={styles.rank}>#17</Text>
        </Stack>

        <div className={styles.tokensContainer}>
          <div className={styles.tokenIcon}>+</div>
          <Text className={styles.tokenAmount}>4,580</Text>
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
