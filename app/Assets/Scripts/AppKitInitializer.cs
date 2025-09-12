using UnityEngine;
using System.Threading.Tasks;

/// <summary>
/// Handles AppKit initialization using configuration from ScriptableObject
/// </summary>
public class AppKitInitializer : MonoBehaviour
{
    [Header("Configuration")]
    [Tooltip("Reference to the AppKit configuration asset")]
    public AppKitConfig appKitConfig;
    
    [Header("Auto Initialize")]
    [Tooltip("Whether to automatically initialize AppKit on Start")]
    public bool autoInitialize = true;
    
    [Header("Status")]
    [Tooltip("Shows the current initialization status")]
    [SerializeField] private bool isInitialized = false;
    
    async void Start()
    {
        if (autoInitialize)
        {
            await InitializeAppKit();
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
            Debug.Log("AppKitInitializer: Starting AppKit initialization...");
            
            // Note: This assumes AppKit namespace/class exists in your project
            // You may need to adjust the namespace or add using statements based on your AppKit implementation
            await AppKit.InitializeAsync(
                new AppKitConfiguration(
                    projectId: appKitConfig.projectId,
                    new Metadata(
                        name: appKitConfig.gameName,
                        description: appKitConfig.gameDescription,
                        url: appKitConfig.gameUrl,
                        iconUrl: appKitConfig.iconUrl
                    )
                )
            );
            
            isInitialized = true;
            Debug.Log("AppKitInitializer: AppKit initialization completed successfully!");
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
    /// Check if AppKit is initialized
    /// </summary>
    public bool IsInitialized => isInitialized;
}

// Note: These classes represent the expected AppKit API structure
// You may need to adjust these based on your actual AppKit implementation

/// <summary>
/// Mock AppKit class - replace with your actual AppKit implementation
/// </summary>
public static class AppKit
{
    public static async Task InitializeAsync(AppKitConfiguration config)
    {
        // Simulate async initialization
        await Task.Delay(100);
        
        // Your actual AppKit initialization logic goes here
        Debug.Log($"AppKit initialized with project ID: {config.ProjectId}");
        Debug.Log($"Game: {config.Metadata.Name}");
        Debug.Log($"Description: {config.Metadata.Description}");
        Debug.Log($"URL: {config.Metadata.Url}");
        Debug.Log($"Icon: {config.Metadata.IconUrl}");
    }
}

/// <summary>
/// AppKit configuration structure - adjust based on your actual AppKit API
/// </summary>
public class AppKitConfiguration
{
    public string ProjectId { get; }
    public Metadata Metadata { get; }
    
    public AppKitConfiguration(string projectId, Metadata metadata)
    {
        ProjectId = projectId;
        Metadata = metadata;
    }
}

/// <summary>
/// Metadata structure for AppKit - adjust based on your actual AppKit API
/// </summary>
public class Metadata
{
    public string Name { get; }
    public string Description { get; }
    public string Url { get; }
    public string IconUrl { get; }
    
    public Metadata(string name, string description, string url, string iconUrl)
    {
        Name = name;
        Description = description;
        Url = url;
        IconUrl = iconUrl;
    }
}
