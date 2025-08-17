using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets;
using UnityEngine.InputSystem;
using System.Collections.Generic;

/// <summary>
/// Direct mine placement test - bypasses all mode switching
/// </summary>
public class DirectMinePlacementTest : MonoBehaviour
{
    private ObjectSpawner objectSpawner;
    private ARRaycastManager raycastManager;
    private float lastPlacementTime = 0f;
    
    void Start()
    {
        // Find components
        objectSpawner = FindFirstObjectByType<ObjectSpawner>();
        raycastManager = FindFirstObjectByType<ARRaycastManager>();
        
        if (objectSpawner == null)
        {
            Debug.LogError("DirectMinePlacementTest: No ObjectSpawner found!");
            return;
        }
        
        if (raycastManager == null)
        {
            Debug.LogError("DirectMinePlacementTest: No ARRaycastManager found!");
            return;
        }
        
        // Force enable ObjectSpawner
        objectSpawner.enabled = true;
        Debug.Log("DirectMinePlacementTest: Found components and enabled ObjectSpawner");
        Debug.Log($"DirectMinePlacementTest: ObjectSpawner has {objectSpawner.objectPrefabs.Count} prefabs");
        Debug.Log($"DirectMinePlacementTest: Spawn index: {objectSpawner.spawnOptionIndex}");
    }
    
    void Update()
    {
        // Simple cooldown
        if (Time.time - lastPlacementTime < 0.5f) return;
        
        // Check for any input
        bool hasInput = false;
        Vector2 inputPosition = Vector2.zero;
        
        // Mouse
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            hasInput = true;
            inputPosition = Mouse.current.position.ReadValue();
            Debug.Log("DirectMinePlacementTest: Mouse input detected!");
        }
        
        // Touch
        if (!hasInput && Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
        {
            hasInput = true;
            inputPosition = Touchscreen.current.primaryTouch.position.ReadValue();
            Debug.Log("DirectMinePlacementTest: Touch input detected!");
        }
        
        if (hasInput)
        {
            TryPlaceMine(inputPosition);
            lastPlacementTime = Time.time;
        }
    }
    
    void TryPlaceMine(Vector2 screenPosition)
    {
        Debug.Log($"DirectMinePlacementTest: Attempting mine placement at screen position {screenPosition}");
        
        // Make sure ObjectSpawner is enabled
        if (!objectSpawner.enabled)
        {
            objectSpawner.enabled = true;
            Debug.Log("DirectMinePlacementTest: Re-enabled ObjectSpawner");
        }
        
        // AR Raycast
        var hits = new List<ARRaycastHit>();
        if (raycastManager.Raycast(screenPosition, hits, UnityEngine.XR.ARSubsystems.TrackableType.PlaneWithinPolygon))
        {
            Debug.Log($"DirectMinePlacementTest: Raycast hit {hits.Count} planes");
            
            if (hits.Count > 0)
            {
                var hit = hits[0];
                Debug.Log($"DirectMinePlacementTest: Hit position: {hit.pose.position}");
                
                // Try to spawn
                bool success = objectSpawner.TrySpawnObject(hit.pose.position, hit.pose.up);
                
                if (success)
                {
                    Debug.Log("🎉 DirectMinePlacementTest: MINE PLACED SUCCESSFULLY!");
                }
                else
                {
                    Debug.LogError("❌ DirectMinePlacementTest: TrySpawnObject FAILED!");
                    
                    // Force spawn a mine directly to test
                    if (objectSpawner.objectPrefabs.Count > 0)
                    {
                        var minePrefab = objectSpawner.objectPrefabs[0];
                        if (minePrefab != null)
                        {
                            var mine = Instantiate(minePrefab, hit.pose.position, Quaternion.identity);
                            Debug.Log("🔧 DirectMinePlacementTest: Force-spawned mine directly!");
                        }
                    }
                }
            }
        }
        else
        {
            Debug.LogWarning("DirectMinePlacementTest: No AR planes hit - scan the floor first!");
        }
    }
}
