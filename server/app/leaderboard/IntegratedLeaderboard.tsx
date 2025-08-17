'use client';

import { Table, Text, Button, Progress, Group, Stack } from '@mantine/core';
import styles from './IntegratedLeaderboard.module.css';

interface LeaderboardEntry {
  rank: number;
  username: string;
  kills: number;
  tokens: number;
  lastActive: string;
}

const leaderboardData: LeaderboardEntry[] = [
  { rank: 1, username: 'ShadowFang', kills: 125, tokens: 8810, lastActive: '4 mins ago' },
  { rank: 2, username: 'CryptWrath', kills: 195, tokens: 8810, lastActive: '4 mins ago' },
  { rank: 3, username: 'DarkReaper', kills: 113, tokens: 7828, lastActive: '6 mins ago' },
  { rank: 4, username: 'SpectreSlayer', kills: 104, tokens: 7834, lastActive: '5 mins ago' },
  { rank: 5, username: 'VenomTwist', kills: 102, tokens: 7312, lastActive: '7 mins ago' },
  { rank: 6, username: 'NightStalker', kills: 87, tokens: 7687, lastActive: '3 mins ago' },
  { rank: 7, username: 'ScarletBlade', kills: 79, tokens: 7655, lastActive: '3 mins ago' },
  { rank: 8, username: 'IronGhoul', kills: 79, tokens: 7889, lastActive: '4 mins ago' },
  { rank: 9, username: 'BoneSlicer', kills: 67, tokens: 969, lastActive: '1 min ago' },
  { rank: 10, username: 'SkullHunter', kills: 17, tokens: 4107, lastActive: '4 mins ago' },
];

export function IntegratedLeaderboard() {
  return (
    <div className={styles.container}>
      {/* Left Panel - Player Profile */}
      <div className={styles.leftPanel}>
        <div className={styles.profileSection}>
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
          
          <Text className={styles.username}>RivalHunter01</Text>
          <Text className={styles.rank}>#17</Text>
          
          <div className={styles.tokensBadge}>
            <span className={styles.tokenIcon}>+</span>
            <span className={styles.tokenAmount}>4,580</span>
          </div>
          
          <Button className={styles.loadoutButton} fullWidth>
            EDIT LOADOUT
          </Button>
        </div>
      </div>

      {/* Center - Leaderboard */}
      <div className={styles.centerPanel}>
        <div className={styles.header}>
          <div className={styles.titleSection}>
            <div className={styles.titleIcon}>
              <svg width="60" height="60" viewBox="0 0 60 60" fill="none" xmlns="http://www.w3.org/2000/svg">
                {/* Simplified skull for header */}
                <path d="M30 10C20 10 12 18 12 28C12 34 14 39 17 42C17 45 16 50 14 54C14 56 16 58 18 58H42C44 58 46 56 46 54C44 50 43 45 43 42C46 39 48 34 48 28C48 18 40 10 30 10Z" fill="#FF4444"/>
                <ellipse cx="22" cy="26" rx="4" ry="5" fill="#141414"/>
                <ellipse cx="38" cy="26" rx="4" ry="5" fill="#141414"/>
                <path d="M30 35L27 40H33L30 35Z" fill="#141414"/>
              </svg>
            </div>
            <Text className={styles.title}>
              RIVALS <span className={styles.subtitle}>- GLOBAL LEADERBOARD</span>
            </Text>
          </div>
        </div>
        
        <Table className={styles.table}>
          <thead>
            <tr>
              <th className={styles.headerRank}>RANK</th>
              <th className={styles.headerName}></th>
              <th className={styles.headerKills}>KILLS</th>
              <th className={styles.headerTokens}>TOKENS</th>
              <th className={styles.headerLastActive}>LAST ACTIVE</th>
            </tr>
          </thead>
          <tbody>
            {leaderboardData.map((entry) => (
              <tr key={entry.rank} className={styles.row}>
                <td className={styles.rankCell}>
                  <span className={`${styles.rankNumber} ${entry.rank <= 3 ? styles.topRank : ''}`}>
                    {entry.rank}.
                  </span>
                </td>
                <td className={styles.usernameCell}>{entry.username}</td>
                <td className={styles.killsCell}>{entry.kills}</td>
                <td className={styles.tokensCell}>{entry.tokens.toLocaleString()}</td>
                <td className={styles.lastActiveCell}>{entry.lastActive}</td>
              </tr>
            ))}
          </tbody>
        </Table>
      </div>

      {/* Right Panel - Profile Summary */}
      <div className={styles.rightPanel}>
        <div className={styles.summarySection}>
          <Text className={styles.sectionTitle}>PROFILE SUMMARY</Text>
          
          <div className={styles.statsRow}>
            <div className={styles.statItem}>
              <Text className={styles.statValue}>45</Text>
              <Text className={styles.statLabel}>KILLS</Text>
            </div>
            <div className={styles.statItem}>
              <Text className={styles.statValue}>19</Text>
              <Text className={styles.statLabel}>TRAPS SET</Text>
            </div>
          </div>
          
          <div className={styles.timeItem}>
            <Text className={styles.timeValue}>1h 32m</Text>
            <Text className={styles.timeLabel}>TIME SURVIVED</Text>
          </div>
        </div>

        <div className={styles.challengesSection}>
          <Text className={styles.sectionTitle}>CHALLENGES</Text>
          
          <div className={styles.challenge}>
            <Text className={styles.challengeTitle}>STEAL 15 TOKENS</Text>
            <Progress 
              value={80} 
              className={styles.progressBar}
              radius="xs"
              size={6}
              color="red"
            />
          </div>
          
          <div className={styles.challenge}>
            <Text className={styles.challengeTitle}>DEFEAT 10 PLAYERS</Text>
            <Progress 
              value={100} 
              className={styles.progressBar}
              radius="xs"
              size={6}
              color="red"
            />
          </div>
        </div>

        <div className={styles.achievementsSection}>
          <Text className={styles.sectionTitle}>ACHEIVEMENTS</Text>
          
          <div className={styles.achievement}>
            <span className={styles.achievementIcon}>🏆</span>
            <Text className={styles.achievementText}>Top 10 Finish</Text>
          </div>
          
          <div className={styles.achievement}>
            <span className={styles.achievementIcon}>💎</span>
            <Text className={styles.achievementText}>2,725 Tokens</Text>
          </div>
        </div>
      </div>
    </div>
  );
}
