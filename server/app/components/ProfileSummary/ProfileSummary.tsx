'use client';

import { Stack, Text, Progress, Group } from '@mantine/core';
import styles from './ProfileSummary.module.css';

export function ProfileSummary() {
  return (
    <div className={styles.container}>
      <Stack gap={32}>
        {/* Profile Stats */}
        <div className={styles.statsSection}>
          <Text className={styles.sectionTitle}>PROFILE SUMMARY</Text>
          <div className={styles.statsGrid}>
            <div className={styles.statItem}>
              <Text className={styles.statValue}>45</Text>
              <Text className={styles.statLabel}>Kills</Text>
            </div>
            <div className={styles.statItem}>
              <Text className={styles.statValue}>19</Text>
              <Text className={styles.statLabel}>Traps Set</Text>
            </div>
          </div>
          <div className={styles.timeItem}>
            <Text className={styles.timeValue}>1h 32m</Text>
            <Text className={styles.timeLabel}>Time Survived</Text>
          </div>
        </div>

        {/* Challenges */}
        <div className={styles.challengesSection}>
          <Text className={styles.sectionTitle}>CHALLENGES</Text>
          <Stack gap={16}>
            <div className={styles.challenge}>
              <Text className={styles.challengeTitle}>STEAL 15 TOKENS</Text>
              <Progress 
                value={80} 
                color="orange" 
                className={styles.progressBar}
                radius="xs"
                size="sm"
              />
            </div>
            <div className={styles.challenge}>
              <Text className={styles.challengeTitle}>DEFEAT 10 PLAYERS</Text>
              <Progress 
                value={100} 
                color="orange" 
                className={styles.progressBar}
                radius="xs"
                size="sm"
              />
            </div>
          </Stack>
        </div>

        {/* Achievements */}
        <div className={styles.achievementsSection}>
          <Text className={styles.sectionTitle}>ACHEIVEMENTS</Text>
          <Stack gap={12}>
            <Group className={styles.achievement}>
              <div className={styles.achievementIcon}>🏆</div>
              <Text className={styles.achievementText}>Top 10 Finish</Text>
            </Group>
            <Group className={styles.achievement}>
              <div className={styles.achievementIcon}>💎</div>
              <Text className={styles.achievementText}>2,725 Tokens</Text>
            </Group>
          </Stack>
        </div>
      </Stack>
    </div>
  );
}
