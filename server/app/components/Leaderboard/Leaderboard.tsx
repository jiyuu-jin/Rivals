'use client';

import { Table, Text } from '@mantine/core';
import styles from './Leaderboard.module.css';

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

export function Leaderboard() {
  return (
    <div className={styles.container}>
      <Text className={styles.title}>
        RIVALS <span className={styles.subtitle}>- GLOBAL LEADERBOARD</span>
      </Text>
      
      <div className={styles.tableWrapper}>
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
                <td className={styles.rank}>
                  <span className={`${styles.rankNumber} ${entry.rank <= 3 ? styles.topRank : ''}`}>
                    {entry.rank}.
                  </span>
                </td>
                <td className={styles.username}>{entry.username}</td>
                <td className={styles.kills}>{entry.kills}</td>
                <td className={styles.tokens}>{entry.tokens.toLocaleString()}</td>
                <td className={styles.lastActive}>{entry.lastActive}</td>
              </tr>
            ))}
          </tbody>
        </Table>
      </div>
    </div>
  );
}
