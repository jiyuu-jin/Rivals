'use client';

import { usePrivy } from '@privy-io/react-auth';
import { useAccount, useDisconnect } from 'wagmi';
import { Button, Menu, Text, Group } from '@mantine/core';
import styles from './WalletConnect.module.css';

export function WalletConnect() {
  const { login, logout, authenticated, user } = usePrivy();
  const { address, isConnected } = useAccount();
  const { disconnect } = useDisconnect();

  const handleDisconnect = () => {
    disconnect();
    logout();
  };

  if (!authenticated || !isConnected) {
    return (
      <Button 
        onClick={login}
        className={styles.connectButton}
        variant="outline"
      >
        Connect Wallet
      </Button>
    );
  }

  const displayAddress = address ? `${address.slice(0, 6)}...${address.slice(-4)}` : 'Connected';

  return (
    <Menu shadow="md" width={200}>
      <Menu.Target>
        <Button 
          className={styles.connectedButton}
          variant="subtle"
        >
          <Text size="sm">{displayAddress}</Text>
        </Button>
      </Menu.Target>

      <Menu.Dropdown>
        <Menu.Label>Wallet Actions</Menu.Label>
        
        <Menu.Item
          onClick={() => {
            if (address) {
              navigator.clipboard.writeText(address);
            }
          }}
        >
          Copy Address
        </Menu.Item>
        
        <Menu.Divider />
        
        <Menu.Item
          color="red"
          onClick={handleDisconnect}
        >
          Disconnect
        </Menu.Item>
      </Menu.Dropdown>
    </Menu>
  );
}
