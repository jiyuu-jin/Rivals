import { Navigation } from '@/app/components';
import { Leaderboard } from './Leaderboard';
import styles from './page.module.css';

export default function LeaderboardPage() {
  return (
    <>
      <Navigation />
      <main className={styles.main}>
        <div className={styles.container}>
          <Leaderboard />
        </div>
      </main>
    </>
  );
}
