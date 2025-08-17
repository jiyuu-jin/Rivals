import { Navigation } from '@/app/components';
import { ComingSoon } from '@/app/components/ComingSoon/ComingSoon';

export default function SettingsPage() {
  return (
    <>
      <Navigation />
      <ComingSoon 
        pageName="SETTINGS"
        description="Customize your game experience with audio, video, controls, and account preferences."
      />
    </>
  );
}
