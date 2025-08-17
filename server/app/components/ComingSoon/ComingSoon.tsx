'use client';

import { Text, Button } from '@mantine/core';
import Link from 'next/link';
import styles from './ComingSoon.module.css';

interface ComingSoonProps {
  pageName: string;
  description?: string;
}

export function ComingSoon({ pageName, description }: ComingSoonProps) {
  return (
    <div className={styles.container}>
      <div className={styles.content}>
        <div className={styles.logoIcon}>
          <img 
            src="/logo.jpeg" 
            alt="Rivals Logo" 
            width={120} 
            height={120}
            className={styles.logoImage}
          />
        </div>
        
        <Text className={styles.title}>{pageName}</Text>
        <Text className={styles.comingSoonText}>COMING SOON</Text>
        
        {description && (
          <Text className={styles.description}>{description}</Text>
        )}
        
        <div className={styles.buttonContainer}>
          <Button 
            component={Link} 
            href="/leaderboard" 
            className={styles.backButton}
          >
            VIEW LEADERBOARD
          </Button>
        </div>
        
        <div className={styles.decorativeElements}>
          <div className={styles.glowOrb} />
          <div className={styles.glowOrb} />
          <div className={styles.glowOrb} />
        </div>
      </div>
    </div>
  );
}
