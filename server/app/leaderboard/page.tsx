import { Navigation } from '@/app/components';
import { IntegratedLeaderboard } from './IntegratedLeaderboard';
import styles from './page.module.css';

export default function LeaderboardPage() {
  return (
    <>
      <Navigation />
      <main className={styles.main}>
        <div className={styles.container}>
          <IntegratedLeaderboard />
        </div>
      </main>
    </>
  );
}
