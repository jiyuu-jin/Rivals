'use server';

import { pg } from '@/app/pg';
import { getTokenHoldersFromEvents, type TokenHolder } from '@/app/lib/token-events';

export interface LeaderboardEntry {
  rank: number;
  username: string;
  kills: number;
  tokens: number;
  lastActive: string;
  evmAddress: string;
}

export interface PlayerProfile {
  username: string;
  rank: number;
  kills: number;
  tokens: number;
  trapsSet: number;
  timeSurvived: string;
}

export async function getLeaderboardData(chainId?: number): Promise<{
  leaderboard: LeaderboardEntry[];
  playerProfile: PlayerProfile | null;
  currentPlayerUsername?: string;
}> {
  try {
    console.log('[LEADERBOARD DEBUG] Starting getLeaderboardData with chainId:', chainId);
    const db = pg();

    // Get all users with their kill counts and last active times
    const users = await db`
      SELECT 
        id,
        username, 
        evm_address,
        kill_count,
        last_active,
        (SELECT COUNT(*) FROM traps WHERE owner = users.id) as traps_set
      FROM users 
      ORDER BY kill_count DESC, last_active DESC
    `;

    console.log('[LEADERBOARD DEBUG] Found users in database:', users.length);

    // Get token holders from blockchain events (last 2 months)
    console.log('[LEADERBOARD DEBUG] Fetching token balances from Transfer events...');
    const tokenHolders = await getTokenHoldersFromEvents(chainId);
    console.log('[LEADERBOARD DEBUG] Found', tokenHolders.length, 'token holders from events');

    // Create a map for quick balance lookups
    const balanceMap = new Map<string, TokenHolder>();
    tokenHolders.forEach(holder => {
      balanceMap.set(holder.address.toLowerCase(), holder);
    });

    // Combine database users with blockchain balances
    const leaderboardData = users.map((user, index) => {
      const userAddress = user.evm_address?.toLowerCase();
      const holderData = userAddress ? balanceMap.get(userAddress) : null;
      const tokenBalance = holderData ? Math.floor(parseFloat(holderData.formattedBalance)) : 0;

      // Use blockchain-based last active time if available, otherwise fall back to database
      let lastActiveString: string;
      if (holderData?.lastActiveString) {
        lastActiveString = holderData.lastActiveString;
        console.log('[LEADERBOARD DEBUG] Using blockchain data for', user.username, '- transfers:', holderData.transferCount, 'last active:', lastActiveString);
      } else {
        // Fallback to database timestamp if no blockchain activity
        const lastActiveDate = new Date(user.last_active);
        const now = new Date();
        const diffMinutes = Math.floor((now.getTime() - lastActiveDate.getTime()) / (1000 * 60));

        if (diffMinutes < 1) {
          lastActiveString = 'just now';
        } else if (diffMinutes < 60) {
          lastActiveString = `${diffMinutes} min${diffMinutes === 1 ? '' : 's'} ago`;
        } else {
          const diffHours = Math.floor(diffMinutes / 60);
          if (diffHours < 24) {
            lastActiveString = `${diffHours} hour${diffHours === 1 ? '' : 's'} ago`;
          } else {
            const diffDays = Math.floor(diffHours / 24);
            lastActiveString = `${diffDays} day${diffDays === 1 ? '' : 's'} ago`;
          }
        }
        console.log('[LEADERBOARD DEBUG] Using database last active for', user.username, ':', lastActiveString);
      }

      return {
        rank: index + 1,
        username: user.username,
        kills: holderData?.transferCount || 0, // Use blockchain transfer count instead of database kills
        tokens: tokenBalance,
        lastActive: lastActiveString,
        evmAddress: user.evm_address,
        trapsSet: user.traps_set || 0,
      };
    });

    // Also include token holders who aren't in our users database
    const knownAddresses = new Set(users.map(u => u.evm_address?.toLowerCase()).filter(Boolean));
    tokenHolders.forEach(holder => {
      if (!knownAddresses.has(holder.address.toLowerCase())) {
        const tokenBalance = Math.floor(parseFloat(holder.formattedBalance));
        if (tokenBalance > 0) {
          leaderboardData.push({
            rank: 0, // Will be set after sorting
            username: `${holder.address.slice(0, 6)}...${holder.address.slice(-4)}`, // Shortened address
            kills: holder.transferCount, // Use blockchain transfer count
            tokens: tokenBalance,
            lastActive: holder.lastActiveString || 'unknown', // Use blockchain last active
            evmAddress: holder.address,
            trapsSet: 0,
          });
        }
      }
    });

    // Sort by a combination of tokens and kills (prioritize tokens, then kills as tiebreaker)
    leaderboardData.sort((a, b) => {
      if (a.tokens !== b.tokens) {
        return b.tokens - a.tokens; // Higher tokens first
      }
      return b.kills - a.kills; // Then higher kills
    });

    // Update ranks after sorting
    leaderboardData.forEach((entry, index) => {
      entry.rank = index + 1;
    });

    // Filter out entries with 0 tokens for cleaner display
    const filteredLeaderboard = leaderboardData.filter(entry => entry.tokens > 0);

    // For demo purposes, let's assume the first player is the current player
    // In a real app, you'd get this from authentication/session
    const currentPlayer = filteredLeaderboard[0];
    const playerProfile: PlayerProfile | null = currentPlayer ? {
      username: currentPlayer.username,
      rank: currentPlayer.rank,
      kills: currentPlayer.kills, // Now uses blockchain transfer count
      tokens: currentPlayer.tokens,
      trapsSet: currentPlayer.trapsSet,
      timeSurvived: '1h 32m', // This would be calculated from game session data
    } : null;

    console.log('[LEADERBOARD DEBUG] Final leaderboard has', filteredLeaderboard.length, 'entries');

    return {
      leaderboard: filteredLeaderboard.slice(0, 10), // Top 10 for leaderboard display
      playerProfile,
      currentPlayerUsername: currentPlayer?.username,
    };

  } catch (error) {
    console.error('Error fetching leaderboard data:', error);

    // Return empty data - UI will show appropriate loading/empty state
    return {
      leaderboard: [],
      playerProfile: null,
      currentPlayerUsername: undefined,
    };
  }
}

export async function getPlayerChallenges(username: string): Promise<{
  stealTokensProgress: number;
  defeatPlayersProgress: number;
  achievements: Array<{ icon: string; text: string; }>;
}> {
  try {
    const db = pg();

    // Get player data for challenges
    const playerData = await db`
      SELECT 
        kill_count,
        (SELECT COUNT(*) FROM traps WHERE owner = users.id) as traps_set
      FROM users 
      WHERE username = ${username}
      LIMIT 1
    `;

    if (playerData.length === 0) {
      return {
        stealTokensProgress: 0,
        defeatPlayersProgress: 0,
        achievements: [],
      };
    }

    const player = playerData[0];

    // Calculate challenge progress
    const stealTokensProgress = Math.min(100, (player.kill_count * 5) / 15 * 100); // Assume 5 tokens per kill
    const defeatPlayersProgress = Math.min(100, (player.kill_count / 10) * 100);

    // Generate achievements based on performance
    const achievements = [];
    if (player.kill_count >= 10) {
      achievements.push({ icon: '🏆', text: 'Top 10 Finish' });
    }
    if (player.kill_count >= 50) {
      achievements.push({ icon: '💎', text: '2,725 Tokens' });
    }

    return {
      stealTokensProgress,
      defeatPlayersProgress,
      achievements,
    };

  } catch (error) {
    console.error('Error fetching player challenges:', error);

    // Return fallback data
    return {
      stealTokensProgress: 80,
      defeatPlayersProgress: 100,
      achievements: [
        { icon: '🏆', text: 'Top 10 Finish' },
        { icon: '💎', text: '2,725 Tokens' },
      ],
    };
  }
}
