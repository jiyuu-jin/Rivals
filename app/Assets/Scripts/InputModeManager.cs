using UnityEngine;
using System;

public enum InputMode
{
    Shooting,
    MinePlacement
}

/// <summary>
/// Manages the current input mode (shooting vs mine placement) and notifies other systems
/// </summary>
public class InputModeManager : MonoBehaviour
{
    [Header("Mode Settings")]
    [Tooltip("Starting input mode")]
    public InputMode startingMode = InputMode.Shooting;
    
    [Header("Debug")]
    [Tooltip("Show current mode in inspector")]
    public InputMode currentMode = InputMode.Shooting;
    
    // Singleton instance
    public static InputModeManager Instance { get; private set; }
    
    // Events
    public static event Action<InputMode> OnModeChanged;
    public static event Action OnShootingModeEnabled;
    public static event Action OnMinePlacementModeEnabled;
    
    void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("InputModeManager: Singleton instance created");
        }
        else
        {
            Debug.LogWarning("InputModeManager: Duplicate instance destroyed");
            Destroy(gameObject);
            return;
        }
        
        // Set initial mode
        currentMode = startingMode;
    }
    
    void Start()
    {
        // Notify initial mode
        NotifyModeChanged();
    }
    
    /// <summary>
    /// Set the current input mode
    /// </summary>
    public void SetMode(InputMode newMode)
    {
        if (currentMode == newMode)
        {
            Debug.Log($"InputModeManager: Already in {newMode} mode");
            return;
        }
        
        InputMode previousMode = currentMode;
        currentMode = newMode;
        
        Debug.Log($"InputModeManager: Mode changed from {previousMode} to {newMode}");
        
        NotifyModeChanged();
    }
    
    /// <summary>
    /// Toggle between shooting and mine placement modes
    /// </summary>
    public void ToggleMode()
    {
        InputMode newMode = currentMode == InputMode.Shooting ? InputMode.MinePlacement : InputMode.Shooting;
        SetMode(newMode);
    }
    
    /// <summary>
    /// Check if currently in shooting mode
    /// </summary>
    public bool IsShootingMode()
    {
        return currentMode == InputMode.Shooting;
    }
    
    /// <summary>
    /// Check if currently in mine placement mode
    /// </summary>
    public bool IsMinePlacementMode()
    {
        return currentMode == InputMode.MinePlacement;
    }
    
    /// <summary>
    /// Force shooting mode (useful for game events like player death)
    /// </summary>
    public void ForceShootingMode()
    {
        SetMode(InputMode.Shooting);
    }
    
    /// <summary>
    /// Get current mode as string for UI display
    /// </summary>
    public string GetModeDisplayName()
    {
        switch (currentMode)
        {
            case InputMode.Shooting:
                return "SHOOTING MODE";
            case InputMode.MinePlacement:
                return "MINE PLACEMENT MODE";
            default:
                return "UNKNOWN MODE";
        }
    }
    
    /// <summary>
    /// Get instruction text for current mode
    /// </summary>
    public string GetModeInstructions()
    {
        switch (currentMode)
        {
            case InputMode.Shooting:
                return "TAP TO SHOOT";
            case InputMode.MinePlacement:
                return "TAP TO PLACE MINE";
            default:
                return "";
        }
    }
    
    /// <summary>
    /// Get color for current mode
    /// </summary>
    public Color GetModeColor()
    {
        switch (currentMode)
        {
            case InputMode.Shooting:
                return Color.green;
            case InputMode.MinePlacement:
                return new Color(1f, 0.5f, 0f, 1f); // Orange
            default:
                return Color.white;
        }
    }
    
    void NotifyModeChanged()
    {
        // Notify all listeners
        OnModeChanged?.Invoke(currentMode);
        
        // Notify specific mode events
        switch (currentMode)
        {
            case InputMode.Shooting:
                OnShootingModeEnabled?.Invoke();
                break;
            case InputMode.MinePlacement:
                OnMinePlacementModeEnabled?.Invoke();
                break;
        }
    }
    
    void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}
