using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets;
using UnityEngine.InputSystem;
using System.Collections.Generic;

/// <summary>
/// Handles direct input for mine placement when in mine placement mode
/// This bypasses the complex XR Input System and provides direct touch/mouse handling
/// </summary>
public class MinePlacementHandler : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Reference to the ObjectSpawner component")]
    public ObjectSpawner objectSpawner;
    
    [Tooltip("Reference to the ARRaycastManager for plane detection")]
    public ARRaycastManager raycastManager;
    
    [Header("Settings")]
    [Tooltip("Only place mines on horizontal surfaces")]
    public bool requireHorizontalSurface = true;
    
    [Tooltip("Enable debug logging")]
    public bool enableDebugLogs = true;
    
    void Start()
    {
        // Auto-find components if not assigned
        if (objectSpawner == null)
        {
            objectSpawner = FindFirstObjectByType<ObjectSpawner>();
            if (objectSpawner != null) 
            {
                Debug.Log($"MinePlacementHandler: Found ObjectSpawner automatically. Enabled: {objectSpawner.enabled}");
            }
            else
            {
                Debug.LogError("MinePlacementHandler: Failed to find ObjectSpawner!");
            }
        }
        
        if (raycastManager == null)
        {
            raycastManager = FindFirstObjectByType<ARRaycastManager>();
            if (enableDebugLogs && raycastManager != null) 
                Debug.Log("MinePlacementHandler: Found ARRaycastManager automatically");
        }
        
        if (enableDebugLogs) Debug.Log("MinePlacementHandler: Initialized");
    }
    
    void Update()
    {
        // Only handle input when in mine placement mode
        if (InputModeManager.Instance == null || !InputModeManager.Instance.IsMinePlacementMode())
        {
            if (Time.frameCount % 60 == 0 && enableDebugLogs) // Log every second
                Debug.Log($"MinePlacementHandler: Not in mine mode. Current mode: {InputModeManager.Instance?.currentMode}");
            return;
        }
            
        // Only process if ObjectSpawner exists
        if (objectSpawner == null)
        {
            if (Time.frameCount % 60 == 0 && enableDebugLogs) // Log every second
                Debug.Log("MinePlacementHandler: ObjectSpawner is null!");
            return;
        }
        
        // Force enable ObjectSpawner in mine placement mode
        if (!objectSpawner.enabled)
        {
            objectSpawner.enabled = true;
            Debug.Log("MinePlacementHandler: Force-enabled ObjectSpawner for mine placement");
        }
            
        // Check for input (mouse or touch)
        bool inputDetected = false;
        Vector2 screenPosition = Vector2.zero;
        
        // Mouse input
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            inputDetected = true;
            screenPosition = Mouse.current.position.ReadValue();
            if (enableDebugLogs) Debug.Log("MinePlacementHandler: Mouse input detected");
        }
        
        // Touch input  
        if (!inputDetected && Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
        {
            inputDetected = true;
            screenPosition = Touchscreen.current.primaryTouch.position.ReadValue();
            if (enableDebugLogs) Debug.Log("MinePlacementHandler: Touch input detected");
        }
        
        if (inputDetected)
        {
            HandleMinePlacementInput(screenPosition);
        }
    }
    
    void HandleMinePlacementInput(Vector2 screenPosition)
    {
        if (raycastManager == null)
        {
            if (enableDebugLogs) Debug.LogWarning("MinePlacementHandler: No ARRaycastManager available");
            return;
        }
        
        // Don't place mines if pointer is over UI
        if (IsPointerOverUI())
        {
            if (enableDebugLogs) Debug.Log("MinePlacementHandler: Input over UI, ignoring");
            return;
        }
        
        // Perform AR raycast to find placement position
        var hits = new List<ARRaycastHit>();
        if (raycastManager.Raycast(screenPosition, hits, TrackableType.PlaneWithinPolygon))
        {
            if (hits.Count > 0)
            {
                var hit = hits[0];
                
                // Check if we hit an AR plane
                if (hit.trackable is ARPlane arPlane)
                {
                    // Check surface requirement
                    if (requireHorizontalSurface && arPlane.alignment != PlaneAlignment.HorizontalUp)
                    {
                        if (enableDebugLogs) Debug.Log("MinePlacementHandler: Not a horizontal surface, skipping");
                        return;
                    }
                    
                    // Debug ObjectSpawner state before spawning
                    if (enableDebugLogs)
                    {
                        Debug.Log($"MinePlacementHandler: About to spawn at {hit.pose.position}");
                        Debug.Log($"MinePlacementHandler: ObjectSpawner enabled: {objectSpawner.enabled}");
                        Debug.Log($"MinePlacementHandler: ObjectSpawner prefab count: {objectSpawner.objectPrefabs.Count}");
                        Debug.Log($"MinePlacementHandler: ObjectSpawner spawn index: {objectSpawner.spawnOptionIndex}");
                        Debug.Log($"MinePlacementHandler: ObjectSpawner camera: {(objectSpawner.cameraToFace != null ? objectSpawner.cameraToFace.name : "NULL")}");
                        
                        if (objectSpawner.objectPrefabs.Count > 0)
                        {
                            int actualIndex = objectSpawner.isSpawnOptionRandomized ? 0 : objectSpawner.spawnOptionIndex;
                            if (actualIndex >= 0 && actualIndex < objectSpawner.objectPrefabs.Count)
                            {
                                var prefab = objectSpawner.objectPrefabs[actualIndex];
                                Debug.Log($"MinePlacementHandler: Will spawn prefab: {(prefab != null ? prefab.name : "NULL PREFAB")}");
                            }
                            else
                            {
                                Debug.LogWarning($"MinePlacementHandler: Invalid spawn index {actualIndex} for {objectSpawner.objectPrefabs.Count} prefabs");
                            }
                        }
                        else
                        {
                            Debug.LogError("MinePlacementHandler: NO PREFABS assigned to ObjectSpawner!");
                        }
                    }
                    
                    // Try to spawn the mine
                    if (objectSpawner.TrySpawnObject(hit.pose.position, hit.pose.up))
                    {
                        if (enableDebugLogs) Debug.Log($"MinePlacementHandler: ✅ Successfully placed mine at {hit.pose.position}");
                    }
                    else
                    {
                        if (enableDebugLogs) Debug.LogError("MinePlacementHandler: ❌ ObjectSpawner.TrySpawnObject failed - check prefabs, camera, and validation settings");
                    }
                }
                else
                {
                    if (enableDebugLogs) Debug.Log("MinePlacementHandler: Hit trackable is not an AR plane");
                }
            }
        }
        else
        {
            if (enableDebugLogs) Debug.Log("MinePlacementHandler: No AR planes hit by raycast");
        }
    }
    
    /// <summary>
    /// Check if the pointer is over UI elements
    /// </summary>
    bool IsPointerOverUI()
    {
        return UnityEngine.EventSystems.EventSystem.current != null && 
               UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject(-1);
    }
    
    /// <summary>
    /// Manually trigger mine placement at screen center (useful for testing)
    /// </summary>
    [ContextMenu("Test Mine Placement")]
    public void TestMinePlacement()
    {
        Vector2 screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);
        HandleMinePlacementInput(screenCenter);
    }
}
