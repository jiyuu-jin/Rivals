# SIWE (Sign In With Ethereum) Integration Setup Guide

This guide explains how to set up wallet-based authentication using SIWE (Sign In With Ethereum) in your Unity game with Next.js backend.

**Based on:** [Reown AppKit SIWE Documentation](https://docs.reown.com/appkit/next/core/siwe#getnonce)

## 🎯 Overview

Your game now uses wallet addresses for authentication instead of usernames. Players must connect their wallet and sign a message to access game features.

### ✅ What's Already Implemented

1. **Server-side SIWE authentication** with NextAuth
2. **Protected API routes** requiring wallet authentication
3. **Unity wallet integration** with Reown AppKit
4. **Authenticated API client** for Unity requests
5. **Updated game logic** to use wallet addresses

## 🔧 Server Setup (Already Done)

### 1. Installed Packages
- `@reown/appkit-siwe` - SIWE authentication
- `next-auth` - Session management

### 2. Created Authentication Files
- `app/api/auth/[...nextauth]/route.ts` - NextAuth configuration
- `app/siwe-config.ts` - SIWE client configuration  
- `app/lib/auth.ts` - Authentication utilities

### 3. Updated API Routes
All API routes now require wallet authentication:
- `/api/movement` - Get nearby traps and balance
- `/api/place-trap` - Place new trap
- `/api/die` - Handle player death
- `/api/kill-monster` - Kill monster and get rewards

## 🎮 Unity Setup (Already Done)

### 1. Enhanced AppKit Integration
- `AppKitInitializer.cs` - Manages wallet connection
- `WalletStatusButton.cs` - Shows wallet status and controls

### 2. New Authentication System
- `AuthenticatedAPIClient.cs` - Handles authenticated requests
- Updated `LocationMonitor.cs` - Uses wallet authentication

## ⚙️ Environment Configuration

### 1. Server Environment Variables (.env.local)

Create `/server/.env.local` with:

```bash
# NextAuth configuration
NEXTAUTH_SECRET=your-secret-key-here-generate-a-strong-random-string
NEXTAUTH_URL=http://localhost:3000

# Reown AppKit Project ID (get from https://cloud.reown.com)
NEXT_PUBLIC_PROJECT_ID=your-project-id-here
```

### 2. Generate NEXTAUTH_SECRET

```bash
openssl rand -base64 32
```

### 3. Get Project ID
1. Visit [Reown Dashboard](https://cloud.reown.com)
2. Create a new project
3. Copy your Project ID

## 🚀 Unity Scene Setup

### 1. Add Required Components

Add these components to your scene:

```
GameObject: "AppKit Manager"
├── AppKitInitializer
├── AuthenticatedAPIClient  
└── WalletStatusButton

GameObject: "Location Manager" 
├── LocationMonitor (updated)
└── Reference to AuthenticatedAPIClient
```

### 2. Configure Components

#### AppKitInitializer
- Create and assign `ReownAppKitConfig` asset
- Set your Project ID in the config
- Enable `autoConnectWallet` for automatic connection

#### AuthenticatedAPIClient
- Set `serverBaseUrl` to your backend URL
- Reference will be auto-found by other components

#### LocationMonitor
- Assign reference to `AuthenticatedAPIClient`
- Remove any hardcoded usernames

### 3. Create AppKit Config Asset

1. Right-click in Project → Create → Game → Reown AppKit Config
2. Set your Project ID and game metadata
3. Assign to `AppKitInitializer` component

## 🔄 Authentication Flow

### 1. Game Startup
```
1. Unity starts → AppKitInitializer initializes
2. Attempts to resume previous wallet session
3. If no session: Shows wallet connection modal
4. User connects wallet and signs SIWE message
5. Session stored, APIs become accessible
```

### 2. API Requests
```
1. LocationMonitor checks if wallet connected
2. Uses AuthenticatedAPIClient for all requests
3. Requests include authentication headers/cookies
4. Server validates wallet signature
5. API processes request with wallet address
```

### 3. Session Management
```
- Sessions persist across game restarts
- Automatically attempt reconnection
- Handle disconnection gracefully
- Show wallet status in UI
```

## 🛠️ Database Schema Updates

The authentication system automatically creates users based on wallet addresses:

```sql
-- Users are created/found by wallet address
-- Username defaults to wallet address (can be updated)
INSERT INTO users (evm_address, username)
VALUES ('0x123...abc', '0x123...abc')
ON CONFLICT (evm_address) DO NOTHING;
```

## 🧪 Testing the Integration

### 1. Server Testing
```bash
cd server
pnpm dev
```

Visit `http://localhost:3000` and test wallet connection.

### 2. Unity Testing
1. Add all components to scene
2. Configure AppKit with valid Project ID
3. Play scene
4. Wallet button should appear in top-left
5. Click to connect wallet
6. Sign SIWE message
7. Game APIs should work

### 3. API Testing
Test protected endpoints with proper authentication:

```bash
# This will fail without authentication
curl -X POST http://localhost:3000/api/movement \
  -H "Content-Type: application/json" \
  -d '{"latitude": 40.7128, "longitude": -74.0060}'

# Should return 401 Unauthorized
```

## 🚨 Troubleshooting

### Common Issues

#### 1. "Authentication required" errors
- Check wallet is connected in Unity
- Verify Project ID is correct
- Ensure NextAuth session is active

#### 2. AppKit not initializing
- Verify Reown AppKit prefab is in scene
- Check Project ID is valid
- Install required packages on server

#### 3. API requests failing
- Check server is running on correct port
- Verify `serverBaseUrl` in AuthenticatedAPIClient
- Check browser console for CORS errors

#### 4. SIWE signature verification failing
- Ensure wallet supports signing
- Check network ID matches
- Verify message format is correct

### Debug Logging

Enable debug logs in Unity:
```csharp
// Look for these log prefixes:
// "AppKitInitializer:"
// "AuthenticatedAPIClient:"
// "WalletStatusButton:"
// "LocationMonitor:"
```

Enable debug logs on server:
```bash
# Check server console for:
# NextAuth logs
# API route authentication logs
# SIWE verification logs
```

## 🔐 Security Notes

### Production Considerations

1. **Environment Variables**
   - Use strong random `NEXTAUTH_SECRET`
   - Never commit secrets to git
   - Use different Project IDs for dev/prod

2. **HTTPS Required**
   - SIWE requires HTTPS in production
   - Configure proper SSL certificates
   - Update NEXTAUTH_URL for production

3. **Rate Limiting**
   - Add rate limiting to API routes
   - Protect against spam requests
   - Monitor wallet signature attempts

4. **Session Security**
   - Sessions expire automatically
   - Force re-authentication on suspicious activity
   - Monitor for unusual patterns

## 📚 Additional Resources

- [Reown AppKit Documentation](https://docs.reown.com/appkit/unity/core/installation)
- [SIWE Specification (EIP-4361)](https://eips.ethereum.org/EIPS/eip-4361)
- [NextAuth.js Documentation](https://next-auth.js.org/)
- [SIWX Migration Guide](https://docs.reown.com/appkit/next/core/siwx)

## 🎉 Next Steps

Your game now has secure wallet-based authentication! Consider adding:

1. **User profiles** linked to wallet addresses
2. **NFT integration** for in-game items
3. **Token gating** for premium features
4. **Multichain support** using SIWX
5. **Social features** with wallet-based identity

---

**Note:** This integration provides a secure, decentralized authentication system that puts players in control of their identity and data.
