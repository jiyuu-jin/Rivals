using UnityEngine;
using Reown.AppKit.Unity;
using System;

/// <summary>
/// Wallet status button that shows connection state and allows wallet management
/// Integrates with Reown AppKit to display wallet address and connection controls
/// </summary>
public class WalletStatusButton : MonoBehaviour
{
    [Header("Configuration")]
    [Tooltip("Reference to the AppKit initializer")]
    public AppKitInitializer appKitInitializer;
    
    [Header("UI Positioning")]
    [Tooltip("Button position from screen edge")]
    public float marginFromEdge = 20f;
    
    [Tooltip("Button width")]
    public float buttonWidth = 200f;
    
    [Tooltip("Button height")]
    public float buttonHeight = 50f;
    
    [Header("Styling")]
    [Tooltip("Font size for the button text")]
    public int fontSize = 16;
    
    // State tracking
    private bool isWalletConnected = false;
    private string connectedAddress = "";
    private bool isInitialized = false;
    
    void Start()
    {
        // Auto-find AppKit initializer if not assigned
        if (appKitInitializer == null)
        {
            appKitInitializer = FindFirstObjectByType<AppKitInitializer>();
            if (appKitInitializer == null)
            {
                Debug.LogWarning("WalletStatusButton: No AppKitInitializer found in scene!");
                return;
            }
        }
        
        // Subscribe to wallet events
        SubscribeToWalletEvents();
        
        // Check initial state
        UpdateWalletStatus();
    }
    
    void OnDestroy()
    {
        // Unsubscribe from events to prevent memory leaks
        UnsubscribeFromWalletEvents();
    }
    
    void SubscribeToWalletEvents()
    {
        try
        {
            AppKit.AccountConnected += OnWalletConnected;
            AppKit.AccountDisconnected += OnWalletDisconnected;
            AppKit.AccountChanged += OnWalletChanged;
        }
        catch (Exception e)
        {
            Debug.LogWarning($"WalletStatusButton: Could not subscribe to wallet events (AppKit may not be available): {e.Message}");
        }
    }
    
    void UnsubscribeFromWalletEvents()
    {
        try
        {
            AppKit.AccountConnected -= OnWalletConnected;
            AppKit.AccountDisconnected -= OnWalletDisconnected;
            AppKit.AccountChanged -= OnWalletChanged;
        }
        catch (Exception e)
        {
            Debug.LogWarning($"WalletStatusButton: Could not unsubscribe from wallet events: {e.Message}");
        }
    }
    
    void OnWalletConnected(object sender, EventArgs e)
    {
        Debug.Log("WalletStatusButton: Wallet connected event received");
        UpdateWalletStatus();
    }
    
    void OnWalletDisconnected(object sender, EventArgs e)
    {
        Debug.Log("WalletStatusButton: Wallet disconnected event received");
        UpdateWalletStatus();
    }
    
    void OnWalletChanged(object sender, EventArgs e)
    {
        Debug.Log("WalletStatusButton: Wallet changed event received");
        UpdateWalletStatus();
    }
    
    void UpdateWalletStatus()
    {
        if (appKitInitializer == null) return;
        
        isInitialized = appKitInitializer.IsInitialized;
        isWalletConnected = appKitInitializer.IsWalletConnected;
        connectedAddress = appKitInitializer.ConnectedWalletAddress;
    }
    
    void OnGUI()
    {
        // Update status periodically
        UpdateWalletStatus();
        
        // Create button style
        GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
        buttonStyle.fontSize = fontSize;
        buttonStyle.fontStyle = FontStyle.Bold;
        buttonStyle.alignment = TextAnchor.MiddleCenter;
        
        // Determine button text and color
        string buttonText;
        Color buttonColor;
        
        if (!isInitialized)
        {
            buttonText = "Initializing...";
            buttonColor = Color.gray;
            buttonStyle.normal.textColor = Color.white;
        }
        else if (isWalletConnected && !string.IsNullOrEmpty(connectedAddress))
        {
            buttonText = ShortenAddress(connectedAddress);
            buttonColor = new Color(0.2f, 0.8f, 0.2f, 0.9f); // Green for connected
            buttonStyle.normal.textColor = Color.white;
        }
        else
        {
            buttonText = "Connect Wallet";
            buttonColor = new Color(0.2f, 0.6f, 1f, 0.9f); // Blue for connect
            buttonStyle.normal.textColor = Color.white;
        }
        
        // Set button background color
        Texture2D backgroundTexture = MakeTex(2, 2, buttonColor);
        buttonStyle.normal.background = backgroundTexture;
        buttonStyle.hover.background = MakeTex(2, 2, Color.Lerp(buttonColor, Color.white, 0.1f));
        buttonStyle.active.background = MakeTex(2, 2, Color.Lerp(buttonColor, Color.black, 0.1f));
        
        // Position button in top-left corner
        Rect buttonRect = new Rect(
            marginFromEdge, 
            marginFromEdge, 
            buttonWidth, 
            buttonHeight
        );
        
        // Draw the button
        if (GUI.Button(buttonRect, buttonText, buttonStyle))
        {
            OnWalletButtonClicked();
        }
        
        // Cleanup texture
        if (backgroundTexture != null)
        {
            DestroyImmediate(backgroundTexture);
        }
    }
    
    void OnWalletButtonClicked()
    {
        if (appKitInitializer == null)
        {
            Debug.LogWarning("WalletStatusButton: AppKitInitializer not assigned!");
            return;
        }
        
        if (!isInitialized)
        {
            Debug.Log("WalletStatusButton: Initializing and connecting wallet...");
            _ = appKitInitializer.InitializeAndConnectWallet();
        }
        else if (isWalletConnected)
        {
            Debug.Log("WalletStatusButton: Opening wallet modal (connected - user can manage/disconnect)");
            appKitInitializer.OpenWalletModal();
        }
        else
        {
            Debug.Log("WalletStatusButton: Opening wallet connection modal");
            appKitInitializer.OpenWalletModal();
        }
    }
    
    /// <summary>
    /// Shortens a wallet address for display (e.g., 0x1234...5678)
    /// </summary>
    string ShortenAddress(string address)
    {
        if (string.IsNullOrEmpty(address) || address.Length < 10)
            return address;
        
        // Standard format: 0x1234...5678
        return $"{address.Substring(0, 6)}...{address.Substring(address.Length - 4)}";
    }
    
    /// <summary>
    /// Creates a solid color texture for button styling
    /// </summary>
    Texture2D MakeTex(int width, int height, Color color)
    {
        Color[] pix = new Color[width * height];
        for (int i = 0; i < pix.Length; i++)
            pix[i] = color;
        
        Texture2D result = new Texture2D(width, height);
        result.SetPixels(pix);
        result.Apply();
        return result;
    }
    
    /// <summary>
    /// Get the current wallet connection status
    /// </summary>
    public bool IsWalletConnected => isWalletConnected;
    
    /// <summary>
    /// Get the current connected wallet address
    /// </summary>
    public string ConnectedAddress => connectedAddress;
}
