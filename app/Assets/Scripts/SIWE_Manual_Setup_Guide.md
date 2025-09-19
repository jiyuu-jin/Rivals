# Manual SIWE Authentication Setup Guide

✅ **Fixed Compilation Errors** - Manual SIWE implementation ready!

## 🎯 Current Status

The Unity AppKit doesn't have built-in SIWE APIs, so I've implemented a **manual SIWE authentication flow** that integrates seamlessly with your game.

## 🔧 **Unity Scene Setup**

Add these components to your scene in this order:

### **1. Main Authentication Manager**
```
GameObject: "Authentication Manager"
├── AppKitInitializer          (connect wallet)
├── SIWEAuthentication         (handle SIWE flow)  
├── AuthenticatedAPIClient     (manage API requests)
└── WalletStatusButton         (UI control)
```

### **2. Component Configuration**

#### **AppKitInitializer**
- ✅ Create and assign `ReownAppKitConfig` asset
- ✅ Set **Project ID** from Reown Dashboard
- ✅ Enable **"Enable SIWE"**: `true`
- ✅ Set **"Server Base URL"**: `https://rivals.nyc`

#### **SIWEAuthentication**
- ✅ Auto-finds AppKitInitializer
- ✅ Set **Server Base URL**: `https://rivals.nyc`

#### **AuthenticatedAPIClient**
- ✅ Auto-finds both AppKitInitializer and SIWEAuthentication
- ✅ Set **Server Base URL**: `https://rivals.nyc`

#### **WalletStatusButton**
- ✅ Auto-finds all required components
- ✅ Configure UI positioning as desired

### **3. LocationMonitor Integration**
- ✅ Assign **AuthenticatedAPIClient** reference
- ✅ Already updated to use authenticated requests

## 🔄 **Authentication Flow**

### **Step 1: Wallet Connection**
```
1. User clicks wallet button
2. AppKit modal opens
3. User connects wallet
4. Wallet connection successful
```

### **Step 2: SIWE Authentication**
```
1. SIWEAuthentication detects wallet connection
2. Gets nonce from server (/api/auth/csrf)
3. Creates SIWE message (EIP-4361 format)
4. Requests signature from wallet
5. Sends signature to server for verification
6. Authentication complete!
```

### **Step 3: API Access**
```
1. AuthenticatedAPIClient checks authentication status
2. All API requests now include proper authentication
3. Server validates wallet signature
4. Game APIs work normally
```

## 🚨 **Current Limitation**

The **signature request step** needs Unity AppKit's signing API, which may not be available. Here's what happens:

### **What You'll See:**
1. ✅ Wallet connects successfully
2. ✅ SIWE message is created
3. ⚠️ **Signature request shows warning**: `"AppKit signing not yet implemented"`
4. ❌ Authentication fails without signature

### **Debug Logs to Look For:**
```
SIWEAuthentication: Starting authentication for address 0x...
SIWEAuthentication: Created SIWE message: rivals.nyc wants you to sign in...
SIWEAuthentication: Requesting signature from wallet...
SIWEAuthentication: AppKit signing not yet implemented
```

## 🛠️ **Next Steps to Complete SIWE**

### **Option 1: Find Unity AppKit Signing API**
Research the Unity AppKit documentation for:
- `SignPersonalMessage()`
- `SignMessage()`
- `RequestSignature()`
- Any wallet interaction methods

### **Option 2: Alternative Signing Methods**
If Unity AppKit doesn't support signing:
- Use WalletConnect Unity SDK directly
- Implement web-based signing in WebView
- Use platform-specific wallet integrations

### **Option 3: Server-Side Verification Only**
For testing, you could temporarily:
- Skip signature verification
- Use wallet address for authentication
- Add signature requirement later

## 🧪 **Testing Instructions**

### **1. Test Current Implementation**
1. ✅ Update your `ReownAppKitConfig` asset settings
2. ✅ Add all components to scene
3. ✅ Run Unity game
4. ✅ Connect wallet
5. ⚠️ Check debug logs for SIWE flow

### **2. Expected Behavior**
```
# Successful wallet connection:
AppKitInitializer: SIWE authentication will be handled manually
SIWEAuthentication: Wallet connected, starting SIWE authentication...
SIWEAuthentication: Starting authentication for address 0x...

# Current limitation:
SIWEAuthentication: AppKit signing not yet implemented
AuthenticatedAPIClient: SIWE authentication failed
```

### **3. Environment Setup**
Make sure your server is running with:
```bash
# Server .env.local
NEXTAUTH_SECRET=your-generated-secret
NEXT_PUBLIC_PROJECT_ID=your-reown-project-id
```

## 🎯 **What Works Right Now**

✅ **Server-side SIWE**: Complete authentication system  
✅ **Unity integration**: All components connected properly  
✅ **Wallet connection**: AppKit wallet connection works  
✅ **SIWE message creation**: EIP-4361 compliant messages  
✅ **API protection**: All endpoints require authentication  
✅ **UI integration**: Wallet status button with SIWE awareness  

## ❌ **What Needs Implementation**

❌ **Wallet signature request**: Unity AppKit signing API  
❌ **Signature verification flow**: Complete authentication  

## 🆘 **Support Options**

### **Research Unity AppKit Signing**
- Check latest Unity AppKit documentation
- Look for signature/signing examples
- Contact Reown support for Unity guidance

### **Alternative Implementation**
- Research WalletConnect Unity SDK
- Consider web-based authentication
- Evaluate other Unity Web3 libraries

### **Temporary Workaround**
For development testing, I can create a temporary bypass that:
- Uses wallet address for authentication
- Skips signature verification
- Allows testing of game functionality

## 📋 **Immediate Action Items**

1. ✅ **Set up scene** with all components
2. ✅ **Configure settings** in Unity
3. ✅ **Test wallet connection** (should work)
4. ⚠️ **Check SIWE logs** (will show limitation)
5. 🔍 **Research signing API** or contact support

The foundation is complete - we just need the final piece for wallet signing! 🚀
