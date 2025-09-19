using UnityEngine;
using System.Threading.Tasks;
using Reown.AppKit.Unity;

/// <summary>
/// Handles Reown AppKit initialization using configuration from ScriptableObject
/// Based on: https://docs.reown.com/appkit/unity/core/installation
/// </summary>
public class AppKitInitializer : MonoBehaviour
{
    [Header("Configuration")]
    [Tooltip("Reference to the Reown AppKit configuration asset")]
    public ReownAppKitConfig appKitConfig;
    
    [Header("Auto Initialize")]
    [Tooltip("Whether to automatically initialize AppKit on Start")]
    public bool autoInitialize = true;
    
    [Tooltip("Whether to automatically attempt wallet connection after initialization")]
    public bool autoConnectWallet = true;
    
    [Header("Status")]
    [Tooltip("Shows the current initialization status")]
    [SerializeField] private bool isInitialized = false;
    
    async void Start()
    {
        if (autoInitialize)
        {
            if (autoConnectWallet)
            {
                await InitializeAndConnectWallet();
            }
            else
            {
                await InitializeAppKit();
            }
        }
    }
    
    /// <summary>
    /// Initialize AppKit using the configured settings
    /// </summary>
    public async Task InitializeAppKit()
    {
        if (isInitialized)
        {
            Debug.LogWarning("AppKitInitializer: AppKit is already initialized");
            return;
        }
        
        if (appKitConfig == null)
        {
            Debug.LogError("AppKitInitializer: AppKit configuration is not assigned! Please assign an AppKitConfig asset.");
            return;
        }
        
        if (string.IsNullOrEmpty(appKitConfig.projectId) || appKitConfig.projectId == "YOUR PROJECT ID")
        {
            Debug.LogError("AppKitInitializer: Project ID is not set in the AppKit configuration! Please set a valid project ID.");
            return;
        }
        
        try
        {
            Debug.Log("AppKitInitializer: Starting Reown AppKit initialization...");
            
            var config = new AppKitConfig(
                projectId: appKitConfig.projectId,
                new Metadata(
                    name: appKitConfig.gameName,
                    description: appKitConfig.gameDescription,
                    url: appKitConfig.gameUrl,
                    iconUrl: appKitConfig.iconUrl
                )
            );
            
            await AppKit.InitializeAsync(config);
            
            // SIWE will be handled manually after wallet connection
            if (appKitConfig.enableSIWE)
            {
                Debug.Log("AppKitInitializer: SIWE authentication will be handled manually after wallet connection");
            }
            
            isInitialized = true;
            Debug.Log("AppKitInitializer: Reown AppKit initialization completed successfully!");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"AppKitInitializer: Failed to initialize AppKit: {e.Message}");
            Debug.LogException(e);
        }
    }
    
    /// <summary>
    /// Manually trigger AppKit initialization
    /// </summary>
    [ContextMenu("Initialize AppKit")]
    public void InitializeAppKitManual()
    {
        if (Application.isPlaying)
        {
            _ = InitializeAppKit();
        }
        else
        {
            Debug.LogWarning("AppKitInitializer: Cannot initialize AppKit outside of play mode");
        }
    }
    
    /// <summary>
    /// Manually trigger wallet connection
    /// </summary>
    [ContextMenu("Connect Wallet")]
    public void ConnectWalletManual()
    {
        if (Application.isPlaying)
        {
            _ = InitializeAndConnectWallet();
        }
        else
        {
            Debug.LogWarning("AppKitInitializer: Cannot connect wallet outside of play mode");
        }
    }
    
    /// <summary>
    /// Manually open wallet modal
    /// </summary>
    [ContextMenu("Open Wallet Modal")]
    public void OpenWalletModalManual()
    {
        if (Application.isPlaying)
        {
            OpenWalletModal();
        }
        else
        {
            Debug.LogWarning("AppKitInitializer: Cannot open wallet modal outside of play mode");
        }
    }
    
    /// <summary>
    /// Check if AppKit is initialized
    /// </summary>
    public bool IsInitialized => isInitialized;
    
    /// <summary>
    /// Check if a wallet is currently connected
    /// </summary>
    public bool IsWalletConnected
    {
        get
        {
            if (!isInitialized) return false;
            
            try
            {
                var controller = AppKit.ConnectorController;
                return controller != null && controller.IsAccountConnected;
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"AppKitInitializer: Could not check wallet connection status: {e.Message}");
                return false;
            }
        }
    }
    
    /// <summary>
    /// Get the currently connected wallet address
    /// </summary>
    public string ConnectedWalletAddress
    {
        get
        {
            if (!isInitialized || !IsWalletConnected) return "";
            
            try
            {
                var controller = AppKit.ConnectorController;
                if (controller != null && controller.IsAccountConnected)
                {
                    var account = controller.Account;
                    return account.Address ?? "";
                }
                return "";
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"AppKitInitializer: Could not get wallet address: {e.Message}");
                return "";
            }
        }
    }
    
    /// <summary>
    /// Try to resume a previous wallet session
    /// </summary>
    public async Task<bool> TryResumeSession()
    {
        if (!isInitialized)
        {
            Debug.LogWarning("AppKitInitializer: Cannot resume session - AppKit is not initialized");
            return false;
        }
        
        try
        {
            Debug.Log("AppKitInitializer: Attempting to resume previous wallet session...");
            var resumed = await AppKit.ConnectorController.TryResumeSessionAsync();
            
            if (resumed)
            {
                Debug.Log("AppKitInitializer: Successfully resumed previous wallet session");
            }
            else
            {
                Debug.Log("AppKitInitializer: No previous session found to resume");
            }
            
            return resumed;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"AppKitInitializer: Failed to resume session: {e.Message}");
            Debug.LogException(e);
            return false;
        }
    }
    
    /// <summary>
    /// Open the wallet connection modal
    /// </summary>
    public void OpenWalletModal()
    {
        if (!isInitialized)
        {
            Debug.LogWarning("AppKitInitializer: Cannot open wallet modal - AppKit is not initialized");
            return;
        }
        
        try
        {
            Debug.Log("AppKitInitializer: Opening wallet connection modal...");
            AppKit.OpenModal();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"AppKitInitializer: Failed to open wallet modal: {e.Message}");
            Debug.LogException(e);
        }
    }
    
    /// <summary>
    /// Initialize AppKit and attempt to resume or connect wallet
    /// </summary>
    public async Task InitializeAndConnectWallet()
    {
        await InitializeAppKit();
        
        if (!isInitialized) return;
        
        // Try to resume previous session
        var resumed = await TryResumeSession();
        
        if (!resumed)
        {
            // If no previous session, open connection modal
            Debug.Log("AppKitInitializer: No previous session found, opening wallet connection modal");
            
            // Subscribe to account connected event
            AppKit.AccountConnected += OnAccountConnected;
            OpenWalletModal();
        }
        else
        {
            OnAccountConnected(this, System.EventArgs.Empty);
        }
    }
    
    /// <summary>
    /// Handle account connection
    /// </summary>
    private void OnAccountConnected(object sender, System.EventArgs e)
    {
        Debug.Log("AppKitInitializer: Wallet account connected successfully!");
        
        // Unsubscribe to prevent multiple calls
        AppKit.AccountConnected -= OnAccountConnected;
        
        // Add your custom logic here for when wallet is connected
        OnWalletConnected();
    }
    
    /// <summary>
    /// Override this method to add custom logic when wallet is connected
    /// </summary>
    protected virtual void OnWalletConnected()
    {
        Debug.Log("AppKitInitializer: Wallet connected - ready for blockchain interactions!");
        // Add your game-specific logic here
    }
    
    /// <summary>
    /// Get SIWE configuration from the app config
    /// </summary>
    public bool IsSIWEEnabled => appKitConfig != null && appKitConfig.enableSIWE;
    
    /// <summary>
    /// Get server base URL for SIWE authentication
    /// </summary>
    public string GetServerBaseUrl => appKitConfig?.serverBaseUrl ?? "https://rivals.nyc";
}
