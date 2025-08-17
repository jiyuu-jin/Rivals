import { Navigation } from '@/app/components';
import { ComingSoon } from '@/app/components/ComingSoon/ComingSoon';

export default function StatsPage() {
  return (
    <>
      <Navigation />
      <ComingSoon 
        pageName="STATS"
        description="Detailed statistics and analytics about your gameplay, including kill/death ratios, survival times, and performance metrics."
      />
    </>
  );
}
