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
    
    [Header("SIWE Authentication")]
    [Tooltip("Enable SIWE (Sign In With Ethereum) authentication")]
    public bool enableSIWE = true;
    
    [Tooltip("Base URL of your backend server (e.g., https://rivals.nyc or http://localhost:3000)")]
    public string serverBaseUrl = "https://rivals.nyc";
    
    [Header("Metadata")]
    [Tooltip("Name of your game")]
    public string gameName = "Rivals";
    
    [Tooltip("Short description of your game")]
    [TextArea(3, 5)]
    public string gameDescription = "Please sign with your account to authenticate with Rivals";
    
    [Tooltip("URL for your game website")]
    public string gameUrl = "https://rivals.nyc";
    
    [Tooltip("URL for your game icon")]
    public string iconUrl = "https://rivals.nyc/logo.png";
}

