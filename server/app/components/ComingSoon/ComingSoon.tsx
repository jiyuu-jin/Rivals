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
        <div className={styles.skullIcon}>
          <svg width="120" height="120" viewBox="0 0 120 120" fill="none" xmlns="http://www.w3.org/2000/svg">
            {/* Horns */}
            <path d="M25 20C20 10 15 5 10 5C10 5 15 15 20 30" stroke="#FF4444" strokeWidth="3" fill="none"/>
            <path d="M95 20C100 10 105 5 110 5C110 5 105 15 100 30" stroke="#FF4444" strokeWidth="3" fill="none"/>
            
            {/* Main skull */}
            <path d="M60 25C45 25 33 37 33 52C33 60 36 67 41 72C41 76 40 84 37 90C37 93 40 96 43 96H77C80 96 83 93 83 90C80 84 79 76 79 72C84 67 87 60 87 52C87 37 75 25 60 25Z" fill="#FF4444"/>
            
            {/* Eye sockets */}
            <ellipse cx="45" cy="50" rx="8" ry="10" fill="#0F0F0F"/>
            <ellipse cx="75" cy="50" rx="8" ry="10" fill="#0F0F0F"/>
            
            {/* Nose */}
            <path d="M60 65L55 75H65L60 65Z" fill="#0F0F0F"/>
            
            {/* Teeth */}
            <rect x="48" y="80" width="4" height="8" fill="#0F0F0F"/>
            <rect x="54" y="80" width="4" height="8" fill="#0F0F0F"/>
            <rect x="60" y="80" width="4" height="8" fill="#0F0F0F"/>
            <rect x="66" y="80" width="4" height="8" fill="#0F0F0F"/>
            <rect x="72" y="80" width="4" height="8" fill="#0F0F0F"/>
            
            {/* Jaw line */}
            <path d="M45 85C45 85 50 92 60 92C70 92 75 85 75 85" stroke="#0F0F0F" strokeWidth="2" fill="none"/>
          </svg>
        </div>
        
        <Text className={styles.title}>{pageName}</Text>
        <Text className={styles.comingSoonText}>COMING SOON</Text>
        
        {description && (
          <Text className={styles.description}>{description}</Text>
        )}
        
        <div className={styles.buttonContainer}>
          <Link href="/leaderboard" passHref legacyBehavior>
            <Button component="a" className={styles.backButton}>
              VIEW LEADERBOARD
            </Button>
          </Link>
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
