'use client';

import { Group, UnstyledButton } from '@mantine/core';
import Link from 'next/link';
import { usePathname } from 'next/navigation';
import { WalletConnect } from '../WalletConnect';
import { ChainSwitcher } from '../ChainSwitcher';
import styles from './Navigation.module.css';

interface NavItem {
  label: string;
  href: string;
}

const navItems: NavItem[] = [
  { label: 'HOME', href: '/home' },
  { label: 'STATS', href: '/stats' },
  { label: 'LEADERBOARD', href: '/leaderboard' },
  { label: 'SETTINGS', href: '/settings' },
  { label: 'PROFILE', href: '/profile' },
];

export function Navigation() {
  const pathname = usePathname();

  return (
    <nav className={styles.nav}>
      <Group className={styles.navGroup}>
        <Group className={styles.leftNav}>
          {navItems.slice(0, 3).map((item) => (
            <UnstyledButton 
              key={item.label}
              component={Link}
              href={item.href}
              className={`${styles.navItem} ${pathname === item.href ? styles.active : ''}`}
            >
              {item.label}
            </UnstyledButton>
          ))}
        </Group>
        <Group className={styles.rightNav}>
          {navItems.slice(3).map((item) => (
            <UnstyledButton 
              key={item.label}
              component={Link}
              href={item.href}
              className={`${styles.navItem} ${pathname === item.href ? styles.active : ''}`}
            >
              {item.label}
            </UnstyledButton>
          ))}
          <div className={styles.walletSection}>
            <ChainSwitcher />
            <WalletConnect />
          </div>
        </Group>
      </Group>
    </nav>
  );
}