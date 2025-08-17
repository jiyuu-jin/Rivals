# Privy Wallet Integration Setup

This document explains how to set up and use the Privy wallet integration with Flow Testnet and Chiliz Spicy Testnet.

## 🔧 Setup Instructions

### 1. Get Privy App ID
1. Visit [Privy Dashboard](https://dashboard.privy.io/)
2. Create a new application
3. Copy your App ID
4. Add it to your `.env.local` file:
   ```env
   NEXT_PUBLIC_PRIVY_APP_ID=your_privy_app_id_here
   ```

### 2. Configure Contract Addresses
Update your `.env.local` with contract addresses for each network:
```env
NEXT_PUBLIC_FLOW_TESTNET_CONTRACT_ADDRESS=0x...
NEXT_PUBLIC_CHILIZ_TESTNET_CONTRACT_ADDRESS=0x...
NEXT_PUBLIC_ANVIL_CONTRACT_ADDRESS=0x...
```

### 3. Test the Integration
1. Start the development server: `npm run dev`
2. Navigate to `http://localhost:3000/leaderboard`
3. Click "Connect Wallet" in the navigation
4. Test different chains using the chain switcher

## 🌐 Supported Networks

### Flow Testnet
- **Chain ID**: 545
- **RPC**: https://rest-testnet.onflow.org
- **Explorer**: https://testnet.flowscan.org
- **Faucet**: https://testnet-faucet.onflow.org

### Chiliz Spicy Testnet
- **Chain ID**: 88882
- **RPC**: https://chiliz-spicy.publicnode.com
- **Explorer**: https://testnet.chiliscan.com
- **Faucet**: https://spicy-faucet.chiliz.com

### Anvil (Local)
- **Chain ID**: 31337
- **RPC**: http://127.0.0.1:8545
- **For development and testing**

## 🎮 Features

### Wallet Connection
- Connect with various wallet providers
- Email-based wallet creation
- Embedded wallet support
- Multi-chain switching

### Game Integration
- Read token balances from smart contracts
- Execute game actions (kill monster, place trap, etc.)
- Real-time leaderboard updates
- Chain-specific contract interactions

### User Experience
- Dark theme matching game aesthetics
- Loading states and error handling
- Responsive design
- Faucet links for testnet tokens

## 🔍 Testing Scenarios

1. **Wallet Connection**: Test connecting with MetaMask, WalletConnect, etc.
2. **Chain Switching**: Switch between Flow Testnet and Chiliz Spicy
3. **Token Balance**: Verify balance reading from contracts
4. **Game Actions**: Test monster kills and trap placement
5. **Leaderboard**: Ensure real-time updates work

## 🛠️ Development Notes

### Components
- `WalletConnect`: Main wallet connection button
- `ChainSwitcher`: Network selection dropdown
- `PrivyProvider`: Wraps app with authentication context

### Hooks
- `useAccount`: Get connected wallet info
- `useBlockchainClients`: Access blockchain clients
- `useTokenBalance`: Read token balances
- `useGameActions`: Execute game transactions

### Configuration
- `chains.ts`: Network configurations
- `wagmi-config.ts`: Wagmi setup
- `PrivyProvider.tsx`: Privy configuration

## 🚨 Troubleshooting

### Common Issues
1. **"Connect Wallet" not showing**: Check Privy App ID in env
2. **Chain switching fails**: Verify RPC endpoints are accessible
3. **Balance shows 0**: Confirm contract addresses are correct
4. **Transactions fail**: Ensure wallet has gas tokens

### Debug Steps
1. Check browser console for errors
2. Verify environment variables are loaded
3. Test RPC endpoints directly
4. Confirm wallet is connected to correct network

## 📚 Resources

- [Privy Documentation](https://docs.privy.io/)
- [Flow Developer Docs](https://developers.flow.com/)
- [Chiliz Chain Docs](https://docs.chiliz.com/)
- [Wagmi Documentation](https://wagmi.sh/)
