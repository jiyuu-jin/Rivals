# Reown AppKit Setup Guide

This guide explains how to set up Reown AppKit (formerly WalletConnect) in your Unity project according to the official documentation.

**Documentation Reference:** [https://docs.reown.com/appkit/unity/core/installation](https://docs.reown.com/appkit/unity/core/installation)

## Prerequisites

Before starting, ensure your Unity project meets these requirements:

- **Unity 2022.3 or above**
- **IL2CPP code stripping level: Minimal (or lower)**
- **Target platform:** Android, iOS, Windows, macOS, WebGL
- **Gamma color space** (if you need Linear color space, please open a GitHub issue)

## Installation Steps

### 1. Install the Package

You need to install the Reown AppKit package via OpenUPM. You have two options:

#### Option A: OpenUPM CLI (Recommended)

1. Install Node.js if you haven't already
2. Install OpenUPM CLI:
   ```bash
   npm install -g openupm-cli
   ```
3. Navigate to your Unity project root directory
4. Run the installation command:
   ```bash
   openupm add com.reown.appkit.unity
   ```

#### Option B: Package Manager with OpenUPM

1. Open Unity Package Manager (Window → Package Manager)
2. Click the "+" button and select "Add package from git URL"
3. Add the OpenUPM registry and install the package manually

### 2. Get Your Project ID

1. Visit the [Reown Dashboard](https://cloud.reown.com)
2. Create a new project or use an existing one
3. Copy your Project ID - you'll need this for configuration

### 3. Create Configuration Asset

1. In your Unity project, right-click in the Project window
2. Go to Create → Game → Reown AppKit Config
3. Name the asset (e.g., "MyGameAppKitConfig")
4. Select the asset and configure it:
   - **Project ID**: Paste your Project ID from the Reown Dashboard
   - **Game Name**: Your game's name
   - **Game Description**: Short description of your game
   - **Game URL**: Your game's website URL
   - **Icon URL**: URL to your game's icon image

### 4. Add AppKit Prefab to Scene

1. In the Project window, navigate to `Packages/Reown.AppKit.Unity/Prefabs`
2. Find the "Reown AppKit" prefab
3. Drag it into your scene hierarchy

### 5. Setup AppKit Initializer

1. Create an empty GameObject in your scene (or use an existing one)
2. Add the `AppKitInitializer` component to it
3. Configure the component:
   - **AppKit Config**: Assign your ReownAppKitConfig asset
   - **Auto Initialize**: Check if you want automatic initialization on Start
   - **Auto Connect Wallet**: Check if you want automatic wallet connection

## Usage Examples

### Basic Initialization

```csharp
public class MyGameManager : MonoBehaviour
{
    public AppKitInitializer appKitInitializer;
    
    async void Start()
    {
        // Initialize AppKit
        await appKitInitializer.InitializeAppKit();
        
        // Try to resume previous session or connect new wallet
        await appKitInitializer.InitializeAndConnectWallet();
    }
}
```

### Manual Wallet Connection

```csharp
public void OnConnectWalletButtonClick()
{
    if (appKitInitializer.IsInitialized)
    {
        appKitInitializer.OpenWalletModal();
    }
}
```

### Handling Wallet Connection Events

```csharp
public class WalletHandler : AppKitInitializer
{
    protected override void OnWalletConnected()
    {
        base.OnWalletConnected();
        
        // Your custom logic when wallet is connected
        Debug.Log("Wallet connected! Ready for blockchain interactions.");
        
        // Enable game features that require wallet
        EnableBlockchainFeatures();
    }
    
    private void EnableBlockchainFeatures()
    {
        // Add your game-specific blockchain functionality here
    }
}
```

## Context Menu Actions

The `AppKitInitializer` component provides helpful context menu actions for testing:

- **Initialize AppKit**: Manually initialize AppKit (Play mode only)
- **Connect Wallet**: Initialize and attempt wallet connection (Play mode only)
- **Open Wallet Modal**: Open the wallet connection modal (Play mode only)

Right-click on the component in the Inspector to access these actions.

## Build Settings

### Code Stripping Level

1. Go to Edit → Project Settings → Player
2. Expand "Publishing Settings" (or "Other Settings" depending on platform)
3. Set "Managed Stripping Level" to "Minimal" or lower

### Platform-Specific Notes

#### Android
- Ensure you have the necessary permissions in your AndroidManifest.xml
- Test on real devices for best wallet app integration

#### iOS
- Make sure your deployment target supports the required iOS version
- Test wallet integration with actual wallet apps

#### WebGL
- Some wallet features may be limited in WebGL builds
- Test thoroughly in various browsers

## Troubleshooting

### Common Issues

1. **"Project ID not set" error**: Make sure you've assigned a valid Project ID in your ReownAppKitConfig asset
2. **AppKit not initializing**: Check that the Reown AppKit prefab is in your scene
3. **Wallet modal not opening**: Ensure AppKit is initialized before trying to open the modal
4. **Build errors**: Verify your code stripping level is set to Minimal or lower

### Debug Logging

The `AppKitInitializer` script provides detailed debug logging. Look for log messages prefixed with "AppKitInitializer:" to track initialization and connection status.

## Additional Resources

- [Reown AppKit Documentation](https://docs.reown.com/appkit/unity/core/installation)
- [Unity AppKit Example](https://github.com/reown-com/appkit-unity) 
- [Reown Dashboard](https://cloud.reown.com)
- [WebGL Sample](https://appkit-lab.reown.com/unity_webgl/)

## Support

If you encounter issues:

1. Check the official documentation
2. Review the example projects
3. Open an issue on the official GitHub repository
4. Visit the Reown community forums

---

**Note:** This setup creates a complete wallet connection system for your Unity game, enabling users to connect their crypto wallets and interact with blockchain features.
