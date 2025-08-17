import { Navigation } from '@/app/components';
import { ComingSoon } from '@/app/components/ComingSoon/ComingSoon';

export default function HomePage() {
  return (
    <>
      <Navigation />
      <ComingSoon 
        pageName="HOME"
        description="The main dashboard where you'll access all game features, view your recent matches, and get quick access to game modes."
      />
    </>
  );
}
