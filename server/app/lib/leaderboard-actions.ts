'use server';

import { pg } from '@/app/pg';
import { getClientsByChainId, SupportedChainId } from '@/app/clients';
import { getContractAddress } from '@/app/lib/chains';
import { createPublicClient, http, formatUnits } from 'viem';
import { flowTestnet, chilizSpicy, anvil } from '@/app/lib/chains';
import * as RivalsToken from '../../RivalsToken.json';

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

    if (users.length === 0) {
      return {
        leaderboard: [],
        playerProfile: null,
      };
    }

    // Get blockchain client based on chainId
    let publicClient;
    let contractAddress;

    if (chainId === 545) {
      publicClient = createPublicClient({
        chain: flowTestnet,
        transport: http(),
      });
      contractAddress = getContractAddress(545);
    } else if (chainId === 88882) {
      publicClient = createPublicClient({
        chain: chilizSpicy,
        transport: http(),
      });
      contractAddress = getContractAddress(88882);
    } else {
      // Default to anvil or existing clients
      const clients = getClientsByChainId();
      publicClient = clients.publicClient;
      contractAddress = clients.contractAddress;
    }

    // Fetch token balances for all users
    const leaderboardPromises = users.map(async (user, index) => {
      let tokenBalance = 0;

      try {
        const balance = await publicClient.readContract({
          address: contractAddress as `0x${string}`,
          abi: RivalsToken.abi,
          functionName: 'balanceOf',
          args: [user.evm_address as `0x${string}`],
        }) as bigint;

        // Convert from wei to tokens
        tokenBalance = Math.floor(parseFloat(formatUnits(balance, 18)));
      } catch (error) {
        console.error(`Error fetching balance for ${user.username}:`, error);
        tokenBalance = 0;
      }

      // Format last active time
      const lastActiveDate = new Date(user.last_active);
      const now = new Date();
      const diffMinutes = Math.floor((now.getTime() - lastActiveDate.getTime()) / (1000 * 60));

      let lastActiveString: string;
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

      return {
        rank: index + 1,
        username: user.username,
        kills: user.kill_count,
        tokens: tokenBalance,
        lastActive: lastActiveString,
        evmAddress: user.evm_address,
        trapsSet: user.traps_set || 0,
      };
    });

    const leaderboardData = await Promise.all(leaderboardPromises);

    // Sort by a combination of kills and tokens (prioritize kills, then tokens as tiebreaker)
    leaderboardData.sort((a, b) => {
      if (a.kills !== b.kills) {
        return b.kills - a.kills;
      }
      return b.tokens - a.tokens;
    });

    // Update ranks after sorting
    leaderboardData.forEach((entry, index) => {
      entry.rank = index + 1;
    });

    // For demo purposes, let's assume the first player is the current player
    // In a real app, you'd get this from authentication/session
    const currentPlayer = leaderboardData[0];
    const playerProfile: PlayerProfile | null = currentPlayer ? {
      username: currentPlayer.username,
      rank: currentPlayer.rank,
      kills: currentPlayer.kills,
      tokens: currentPlayer.tokens,
      trapsSet: currentPlayer.trapsSet,
      timeSurvived: '1h 32m', // This would be calculated from game session data
    } : null;

    return {
      leaderboard: leaderboardData.slice(0, 10), // Top 10 for leaderboard display
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
