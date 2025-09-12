using UnityEngine;

/// <summary>
/// Configuration ScriptableObject for AppKit settings
/// </summary>
[CreateAssetMenu(fileName = "AppKitConfig", menuName = "Game/AppKit Config")]
public class AppKitConfig : ScriptableObject
{
    [Header("AppKit Configuration")]
    [Tooltip("Your AppKit project ID")]
    public string projectId = "YOUR PROJECT ID";
    
    [Header("Metadata")]
    [Tooltip("Name of your game")]
    public string gameName = "My Game";
    
    [Tooltip("Short description of your game")]
    [TextArea(3, 5)]
    public string gameDescription = "Short description";
    
    [Tooltip("URL for your game website")]
    public string gameUrl = "https://example.com";
    
    [Tooltip("URL for your game icon")]
    public string iconUrl = "https://example.com/logo.png";
}

