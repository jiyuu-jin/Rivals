import { Navigation } from '@/app/components';
import { ComingSoon } from '@/app/components/ComingSoon/ComingSoon';

export default function ProfilePage() {
  return (
    <>
      <Navigation />
      <ComingSoon 
        pageName="PROFILE"
        description="View and edit your player profile, achievements, loadouts, and customize your appearance."
      />
    </>
  );
}
