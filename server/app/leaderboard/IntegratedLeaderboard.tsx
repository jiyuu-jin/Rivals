'use client';

import { Table, Text, Button, Progress, Group, Stack } from '@mantine/core';
import { useEffect, useState } from 'react';
import { useAccount } from 'wagmi';
import { getLeaderboardData, getPlayerChallenges, type LeaderboardEntry, type PlayerProfile } from '@/app/lib/leaderboard-actions';
import { useTokenBalance } from '@/app/hooks/useTokenBalance';
import styles from './IntegratedLeaderboard.module.css';

interface LeaderboardData {
  leaderboard: LeaderboardEntry[];
  playerProfile: PlayerProfile | null;
  currentPlayerUsername?: string;
}

interface ChallengeData {
  stealTokensProgress: number;
  defeatPlayersProgress: number;
  achievements: Array<{ icon: string; text: string; }>;
}

export function IntegratedLeaderboard() {
  const { chainId, isConnected } = useAccount();
  const { balance, isLoading: balanceLoading, isError: balanceError, hasContract } = useTokenBalance();
  const [data, setData] = useState<LeaderboardData | null>(null);
  const [challenges, setChallenges] = useState<ChallengeData | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    async function fetchData() {
      try {
        setLoading(true);
        const leaderboardData = await getLeaderboardData(chainId);
        setData(leaderboardData);
        
        if (leaderboardData.currentPlayerUsername) {
          const challengeData = await getPlayerChallenges(leaderboardData.currentPlayerUsername);
          setChallenges(challengeData);
        }
      } catch (err) {
        setError('Failed to load leaderboard data');
        console.error('Error loading leaderboard:', err);
      } finally {
        setLoading(false);
      }
    }

    fetchData();
  }, []);

  if (loading) {
    return (
      <div className={styles.container}>
        <div className={styles.loading}>
          <Text className={styles.loadingText}>Loading leaderboard data...</Text>
        </div>
      </div>
    );
  }

  if (error || !data) {
    return (
      <div className={styles.container}>
        <div className={styles.error}>
          <Text className={styles.errorText}>{error || 'Failed to load data'}</Text>
        </div>
      </div>
    );
  }

  const { leaderboard, playerProfile } = data;

  // Show message if no leaderboard data available
  if (leaderboard.length === 0) {
    return (
      <div className={styles.container}>
        <div className={styles.loading}>
          <Text className={styles.loadingText}>No leaderboard data available yet. Start playing to see rankings!</Text>
        </div>
      </div>
    );
  }

  return (
    <div className={styles.container}>
      {/* Left Panel - Player Profile */}
      <div className={styles.leftPanel}>
        <div className={styles.profileSection}>
          <div className={styles.logoIcon}>
            <img 
              src="/logo.jpeg" 
              alt="Rivals Logo" 
              width={120} 
              height={120}
              className={styles.logoImage}
            />
          </div>
          
          <Text className={styles.username}>{playerProfile?.username || 'RivalHunter01'}</Text>
          <Text className={styles.rank}>#{playerProfile?.rank || 17}</Text>
          
          <div className={styles.tokensBadge}>
            <span className={styles.tokenIcon}>+</span>
            <span className={styles.tokenAmount}>
              {!isConnected || !hasContract ? '0' :
               balanceLoading ? '...' :
               balanceError ? '0' :
               balance}
            </span>
          </div>
          
          <Button 
            className={styles.loadoutButton} 
            fullWidth
            onClick={() => alert('Coming Soon! Loadout customization will be available in a future update.')}
          >
            EDIT LOADOUT
          </Button>
        </div>
      </div>

      {/* Center - Leaderboard */}
      <div className={styles.centerPanel}>
        <div className={styles.header}>
          <Text className={styles.title}>
            RIVALS <span className={styles.subtitle}>- GLOBAL LEADERBOARD</span>
          </Text>
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
            {leaderboard.map((entry) => (
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
              <Text className={styles.statValue}>{playerProfile?.kills || 45}</Text>
              <Text className={styles.statLabel}>KILLS</Text>
            </div>
            <div className={styles.statItem}>
              <Text className={styles.statValue}>{playerProfile?.trapsSet || 19}</Text>
              <Text className={styles.statLabel}>TRAPS SET</Text>
            </div>
          </div>
          
          <div className={styles.timeItem}>
            <Text className={styles.timeValue}>{playerProfile?.timeSurvived || '1h 32m'}</Text>
            <Text className={styles.timeLabel}>TIME SURVIVED</Text>
          </div>
        </div>

        <div className={styles.challengesSection}>
          <Text className={styles.sectionTitle}>CHALLENGES</Text>
          
          <div className={styles.challenge}>
            <Text className={styles.challengeTitle}>STEAL 15 TOKENS</Text>
            <Progress 
              value={challenges?.stealTokensProgress || 80} 
              className={styles.progressBar}
              radius="xs"
              size={6}
              color="red"
            />
          </div>
          
          <div className={styles.challenge}>
            <Text className={styles.challengeTitle}>DEFEAT 10 PLAYERS</Text>
            <Progress 
              value={challenges?.defeatPlayersProgress || 100} 
              className={styles.progressBar}
              radius="xs"
              size={6}
              color="red"
            />
          </div>
        </div>

        <div className={styles.achievementsSection}>
          <Text className={styles.sectionTitle}>ACHEIVEMENTS</Text>
          
          {(challenges?.achievements || [{ icon: '🏆', text: 'Top 10 Finish' }, { icon: '💎', text: '2,725 Tokens' }]).map((achievement, index) => (
            <div key={index} className={styles.achievement}>
              <span className={styles.achievementIcon}>{achievement.icon}</span>
              <Text className={styles.achievementText}>{achievement.text}</Text>
            </div>
          ))}
        </div>
      </div>
    </div>
  );
}
