using UnityEngine;

/// <summary>
/// Configuration ScriptableObject for Reown AppKit settings
/// Based on: https://docs.reown.com/appkit/unity/core/installation
/// </summary>
[CreateAssetMenu(fileName = "ReownAppKitConfig", menuName = "Game/Reown AppKit Config")]
public class ReownAppKitConfig : ScriptableObject
{
    [Header("AppKit Configuration")]
    [Tooltip("Your AppKit project ID from Reown Dashboard (https://cloud.reown.com)")]
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

