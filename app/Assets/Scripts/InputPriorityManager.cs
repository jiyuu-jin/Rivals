using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets;

/// <summary>
/// Manages input priority between shooting and placing mines
/// </summary>
public class InputPriorityManager : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Reference to the ObjectSpawner component")]
    public ObjectSpawner objectSpawner;
    
    [Tooltip("Reference to the ZombieShooter component")]
    public ZombieShooter zombieShooter;
    
    [Header("Settings")]
    [Tooltip("Whether mine placement is enabled")]
    public bool minePlacementEnabled = false;
    
    // Store the original enabled state of the ObjectSpawner
    private bool originalObjectSpawnerState;
    
    void Start()
    {
        // Auto-find references if not set
        if (objectSpawner == null)
            objectSpawner = FindFirstObjectByType<ObjectSpawner>();
            
        if (zombieShooter == null)
            zombieShooter = FindFirstObjectByType<ZombieShooter>();
            
        if (objectSpawner != null)
        {
            // Store original state
            originalObjectSpawnerState = objectSpawner.enabled;
            
            // Set initial state based on minePlacementEnabled
            objectSpawner.enabled = minePlacementEnabled;
            
            Debug.Log($"InputPriorityManager: Object spawner found and set to {(minePlacementEnabled ? "enabled" : "disabled")}");
        }
        else
        {
            Debug.LogWarning("InputPriorityManager: Object spawner not found!");
        }
        
        if (zombieShooter != null)
        {
            Debug.Log("InputPriorityManager: Zombie shooter found");
        }
        else
        {
            Debug.LogWarning("InputPriorityManager: Zombie shooter not found!");
        }
    }
    
    /// <summary>
    /// Enable mine placement mode
    /// </summary>
    public void EnableMinePlacement()
    {
        if (objectSpawner != null)
        {
            minePlacementEnabled = true;
            objectSpawner.enabled = true;
            Debug.Log("InputPriorityManager: Mine placement ENABLED");
        }
    }
    
    /// <summary>
    /// Disable mine placement mode
    /// </summary>
    public void DisableMinePlacement()
    {
        if (objectSpawner != null)
        {
            minePlacementEnabled = false;
            objectSpawner.enabled = false;
            Debug.Log("InputPriorityManager: Mine placement DISABLED");
        }
    }
    
    /// <summary>
    /// Toggle mine placement mode
    /// </summary>
    public void ToggleMinePlacement()
    {
        if (minePlacementEnabled)
            DisableMinePlacement();
        else
            EnableMinePlacement();
    }
}
