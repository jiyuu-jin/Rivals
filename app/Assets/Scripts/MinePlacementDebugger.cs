using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets;

/// <summary>
/// Debug script to diagnose mine placement issues step by step
/// </summary>
public class MinePlacementDebugger : MonoBehaviour
{
    [Header("Debug Controls")]
    [Tooltip("Enable detailed logging")]
    public bool enableDetailedLogging = true;
    
    [Tooltip("Test mine placement at screen center")]
    public bool testPlacementOnStart = false;
    
    void Start()
    {
        if (testPlacementOnStart)
        {
            Invoke(nameof(TestMinePlacement), 2f); // Wait 2 seconds for AR to initialize
        }
        
        if (enableDetailedLogging)
        {
            DiagnoseSystem();
        }
    }
    
    void DiagnoseSystem()
    {
        Debug.Log("===== MINE PLACEMENT SYSTEM DIAGNOSIS =====");
        
        // 1. Check InputModeManager
        if (InputModeManager.Instance != null)
        {
            Debug.Log($"✅ InputModeManager found - Current mode: {InputModeManager.Instance.currentMode}");
        }
        else
        {
            Debug.LogError("❌ InputModeManager not found!");
        }
        
        // 2. Check ObjectSpawner
        ObjectSpawner objectSpawner = FindFirstObjectByType<ObjectSpawner>();
        if (objectSpawner != null)
        {
            Debug.Log($"✅ ObjectSpawner found - Enabled: {objectSpawner.enabled}");
            Debug.Log($"   Object Prefabs Count: {objectSpawner.objectPrefabs.Count}");
            Debug.Log($"   Spawn Option Index: {objectSpawner.spawnOptionIndex}");
            Debug.Log($"   Is Spawn Option Randomized: {objectSpawner.isSpawnOptionRandomized}");
            
            if (objectSpawner.objectPrefabs.Count > 0)
            {
                for (int i = 0; i < objectSpawner.objectPrefabs.Count; i++)
                {
                    var prefab = objectSpawner.objectPrefabs[i];
                    Debug.Log($"   Prefab {i}: {(prefab != null ? prefab.name : "NULL")}");
                }
            }
            else
            {
                Debug.LogError("❌ ObjectSpawner has NO prefabs assigned!");
            }
            
            Debug.Log($"   Camera To Face: {(objectSpawner.cameraToFace != null ? objectSpawner.cameraToFace.name : "NULL")}");
        }
        else
        {
            Debug.LogError("❌ ObjectSpawner not found!");
        }
        
        // 3. Check ARRaycastManager
        ARRaycastManager raycastManager = FindFirstObjectByType<ARRaycastManager>();
        if (raycastManager != null)
        {
            Debug.Log($"✅ ARRaycastManager found - Enabled: {raycastManager.enabled}");
        }
        else
        {
            Debug.LogError("❌ ARRaycastManager not found!");
        }
        
        // 4. Check ARPlaneManager
        ARPlaneManager planeManager = FindFirstObjectByType<ARPlaneManager>();
        if (planeManager != null)
        {
            Debug.Log($"✅ ARPlaneManager found - Enabled: {planeManager.enabled}");
            Debug.Log($"   Tracked Planes Count: {planeManager.trackables.count}");
        }
        else
        {
            Debug.LogError("❌ ARPlaneManager not found!");
        }
        
        // 5. Check MinePlacementHandler
        MinePlacementHandler placementHandler = FindFirstObjectByType<MinePlacementHandler>();
        if (placementHandler != null)
        {
            Debug.Log($"✅ MinePlacementHandler found - Enabled: {placementHandler.enabled}");
        }
        else
        {
            Debug.LogError("❌ MinePlacementHandler not found!");
        }
        
        Debug.Log("===== END DIAGNOSIS =====");
    }
    
    [ContextMenu("Test Mine Placement")]
    public void TestMinePlacement()
    {
        Debug.Log("===== TESTING MINE PLACEMENT =====");
        
        // Check current mode
        if (InputModeManager.Instance == null)
        {
            Debug.LogError("❌ InputModeManager not available for test");
            return;
        }
        
        Debug.Log($"Current mode: {InputModeManager.Instance.currentMode}");
        
        // Force mine placement mode if not already
        if (!InputModeManager.Instance.IsMinePlacementMode())
        {
            Debug.Log("🔄 Switching to mine placement mode for test");
            InputModeManager.Instance.SetMode(InputMode.MinePlacement);
        }
        
        // Find components
        ObjectSpawner objectSpawner = FindFirstObjectByType<ObjectSpawner>();
        ARRaycastManager raycastManager = FindFirstObjectByType<ARRaycastManager>();
        
        if (objectSpawner == null)
        {
            Debug.LogError("❌ No ObjectSpawner found for test");
            return;
        }
        
        if (raycastManager == null)
        {
            Debug.LogError("❌ No ARRaycastManager found for test");
            return;
        }
        
        // Enable ObjectSpawner if not enabled
        if (!objectSpawner.enabled)
        {
            Debug.Log("🔄 Enabling ObjectSpawner for test");
            objectSpawner.enabled = true;
        }
        
        // Test raycast from screen center
        Vector2 screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);
        Debug.Log($"Testing raycast from screen center: {screenCenter}");
        
        var hits = new System.Collections.Generic.List<ARRaycastHit>();
        if (raycastManager.Raycast(screenCenter, hits, TrackableType.PlaneWithinPolygon))
        {
            Debug.Log($"✅ AR Raycast hit {hits.Count} planes");
            
            if (hits.Count > 0)
            {
                var hit = hits[0];
                Debug.Log($"Hit position: {hit.pose.position}");
                Debug.Log($"Hit trackable: {hit.trackable}");
                
                if (hit.trackable is ARPlane arPlane)
                {
                    Debug.Log($"Hit AR Plane - Alignment: {arPlane.alignment}");
                    
                    // Try to spawn
                    Debug.Log("🎯 Attempting to spawn mine...");
                    bool success = objectSpawner.TrySpawnObject(hit.pose.position, hit.pose.up);
                    
                    if (success)
                    {
                        Debug.Log("🎉 Mine placement SUCCESS!");
                    }
                    else
                    {
                        Debug.LogError("❌ Mine placement FAILED!");
                        
                        // Additional diagnostics
                        Debug.Log($"ObjectSpawner prefab count: {objectSpawner.objectPrefabs.Count}");
                        Debug.Log($"Spawn option index: {objectSpawner.spawnOptionIndex}");
                        Debug.Log($"Camera to face: {objectSpawner.cameraToFace}");
                    }
                }
                else
                {
                    Debug.LogWarning("⚠️ Hit trackable is not an AR plane");
                }
            }
        }
        else
        {
            Debug.LogWarning("⚠️ No AR planes hit by raycast - make sure AR planes are detected first");
            
            // Check if any planes exist
            ARPlaneManager planeManager = FindFirstObjectByType<ARPlaneManager>();
            if (planeManager != null)
            {
                Debug.Log($"Total tracked planes: {planeManager.trackables.count}");
                foreach (var plane in planeManager.trackables)
                {
                    Debug.Log($"Plane {plane.trackableId}: {plane.alignment}, size: {plane.size}");
                }
            }
        }
        
        Debug.Log("===== END TEST =====");
    }
    
    void Update()
    {
        // Show continuous diagnostics in mine placement mode
        if (enableDetailedLogging && InputModeManager.Instance != null && InputModeManager.Instance.IsMinePlacementMode())
        {
            if (Time.frameCount % 120 == 0) // Every 2 seconds
            {
                ObjectSpawner spawner = FindFirstObjectByType<ObjectSpawner>();
                if (spawner != null)
                {
                    Debug.Log($"[Mine Mode] ObjectSpawner enabled: {spawner.enabled}, prefabs: {spawner.objectPrefabs.Count}");
                }
            }
        }
    }
}
