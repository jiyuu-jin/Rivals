using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Android;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using System;
using UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets;

public class LocationMonitor : MonoBehaviour
{
    private IEnumerator coroutine;

    ObjectSpawner m_ObjectSpawner;

    Dictionary<int, GameObject> spawnedTrapObjects = new Dictionary<int, GameObject>();
    
    // UI Components for balance display
    private string currentBalance = "0.00";

    void Start()
    {
        coroutine = Routine();
        StartCoroutine(coroutine);

        if (m_ObjectSpawner == null)
#if UNITY_2023_1_OR_NEWER
            m_ObjectSpawner = FindAnyObjectByType<ObjectSpawner>();
#else
            m_ObjectSpawner = FindObjectOfType<ObjectSpawner>();
#endif
            
        // Find any existing trap objects in the scene and register them
        RefreshSpawnedTrapObjects();
    }

    IEnumerator Routine()
    {
        Debug.Log("LocationMonitor: Starting");
        if (!Permission.HasUserAuthorizedPermission(Permission.FineLocation))
        {
            Permission.RequestUserPermission(Permission.FineLocation);
        }

        // Check if the user has location service enabled.
        if (!Input.location.isEnabledByUser)
            Debug.Log("Location not enabled on device or app does not have permission to access location");

        // Starts the location service.

        float desiredAccuracyInMeters = 10f;
        float updateDistanceInMeters = 10f;

        Input.location.Start(desiredAccuracyInMeters, updateDistanceInMeters);
        Debug.Log("Location service started");

        // Waits until the location service initializes
        int maxWait = 20;
        while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0)
        {
            Debug.Log("status: " + Input.location.status);
            yield return new WaitForSeconds(1);
            maxWait--;
        }

        // If the service didn't initialize in 20 seconds this cancels location service use.
        if (maxWait < 1)
        {
            Debug.Log("Timed out");
            yield break;
        }

        // If the connection failed this cancels location service use.
        if (Input.location.status == LocationServiceStatus.Failed)
        {
            Debug.LogError("Unable to determine device location");
            yield break;
        }
        else
        {
            while (true)
            {
                // If the connection succeeded, this retrieves the device's current location and displays it in the Console window.
                Debug.Log("Location: " + Input.location.lastData.latitude + " " + Input.location.lastData.longitude + " " + Input.location.lastData.altitude + " " + Input.location.lastData.horizontalAccuracy + " " + Input.location.lastData.timestamp);

                string json_body = "{ \"username\": \"player1\", \"latitude\": " + Input.location.lastData.latitude + ", \"longitude\": " + Input.location.lastData.longitude + " }";
                using (UnityWebRequest www = UnityWebRequest.Post("http://10.1.9.21:3000/api/movement", json_body, "application/json"))
                {
                    yield return www.SendWebRequest();

                    if (www.result != UnityWebRequest.Result.Success)
                    {
                        Debug.LogError(www.error);
                    }
                    else
                    {
                        MovementResponse response = JsonUtility.FromJson<MovementResponse>(www.downloadHandler.text);
                        
                        // Update balance display
                        UpdateBalanceDisplay(response.balance);
                        
                        foreach (Trap trap in response.traps)
                        {
                            // Check if we already have this trap object spawned
                            if (!spawnedTrapObjects.ContainsKey(trap.id))
                            {
                                Debug.Log($"TRAP DEBUG: Spawning trap from server: {trap.id} at ({trap.latitude}, {trap.longitude})");

                                // Instead of using AR raycast hit, spawn in front of player
                                Vector3 spawnPosition = GetGroundPositionInFrontOfPlayer();
                                if (spawnPosition != Vector3.zero) // Check if we found a valid ground position
                                {
                                    Debug.Log($"TRAP DEBUG: Attempting to spawn trap {trap.id} at position {spawnPosition}");
                                    if (m_ObjectSpawner.TrySpawnObject(spawnPosition, Vector3.up))
                                    {
                                        Debug.Log($"TRAP DEBUG: Successfully spawned object for trap {trap.id}, starting initialization");
                                        // Find and initialize the spawned object immediately
                                        InitializeDiscoveredTrapImmediate(trap.id, spawnPosition);
                                    }
                                    else
                                    {
                                        Debug.LogWarning($"TRAP DEBUG: Failed to spawn object for trap {trap.id}");
                                    }
                                }
                                else
                                {
                                    Debug.LogWarning($"TRAP DEBUG: Could not find valid ground position for trap {trap.id}");
                                }
                            }
                            else
                            {
                                Debug.Log($"TRAP DEBUG: Skipping trap {trap.id} - already spawned");
                            }
                        }
                    }
                }

                yield return new WaitForSeconds(1);
            }
        }

        // Stops the location service if there is no need to query location updates continuously.
        // Input.location.Stop();
    }

    // Add this method to calculate spawn position in front of player
    private Vector3 GetGroundPositionInFrontOfPlayer()
    {
        Camera playerCamera = Camera.main;
        if (playerCamera == null) return Vector3.zero;

        // Get the forward direction from the camera
        Vector3 forward = playerCamera.transform.forward;

        // Project forward direction onto the horizontal plane (ignore Y component)
        forward.y = 0;
        forward.Normalize();

        // Set spawn distance (how far in front of player)
        float spawnDistance = 2.0f; // Adjust this value as needed

        // Calculate the target position in front of player
        Vector3 targetPosition = playerCamera.transform.position + (forward * spawnDistance);

        // Raycast downward from above the target position to find ground
        Vector3 rayStart = targetPosition + Vector3.up * 10f; // Start 10 units above
        RaycastHit hit;

        if (Physics.Raycast(rayStart, Vector3.down, out hit, 20f, LayerMask.GetMask("Default")))
        {
            return hit.point;
        }

        // Fallback: if no ground found, use the target position at ground level
        return new Vector3(targetPosition.x, 0, targetPosition.z);
    }

    public IEnumerator PlaceTrap(System.Action<int> onTrapPlaced = null)
    {
        // If the connection succeeded, this retrieves the device's current location and displays it in the Console window.
        Debug.Log("Location: " + Input.location.lastData.latitude + " " + Input.location.lastData.longitude + " " + Input.location.lastData.altitude + " " + Input.location.lastData.horizontalAccuracy + " " + Input.location.lastData.timestamp);

        string json_body = "{ \"owner_username\": \"player1\", \"latitude\": " + Input.location.lastData.latitude + ", \"longitude\": " + Input.location.lastData.longitude + " }";
        using (UnityWebRequest www = UnityWebRequest.Post("http://10.1.9.21:3000/api/place-trap", json_body, "application/json"))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError(www.error);
                onTrapPlaced?.Invoke(-1); // Signal failure
            }
            else
            {
                PlaceTrapResponse response = JsonUtility.FromJson<PlaceTrapResponse>(www.downloadHandler.text);
                if (response != null && response.trap != null)
                {
                    Debug.Log("Trap placed: " + response.trap.id + " at " + response.trap.location);
                    onTrapPlaced?.Invoke(response.trap.id); // Return the trap ID
                }
                else
                {
                    Debug.LogError("Failed to parse trap placement response");
                    onTrapPlaced?.Invoke(-1); // Signal failure
                }
            }
        }
    }
    
    void InitializeDiscoveredTrapImmediate(int trapId, Vector3 spawnPosition)
    {
        StartCoroutine(InitializeDiscoveredTrapDelayed(trapId, spawnPosition));
    }
    
    IEnumerator InitializeDiscoveredTrapDelayed(int trapId, Vector3 spawnPosition)
    {
        // Wait a frame to ensure object is fully spawned and positioned
        yield return null;
        yield return null; // Wait an extra frame to be sure
        
        Debug.Log($"TRAP DEBUG: Starting search for trap {trapId} - looking for uninitialized TrapTrigger components");
        
        // Find ALL TrapTrigger components in the scene
        TrapTrigger[] allTrapTriggers = FindObjectsByType<TrapTrigger>(FindObjectsSortMode.None);
        Debug.Log($"TRAP DEBUG: Found {allTrapTriggers.Length} total TrapTrigger components in scene");
        
        GameObject foundTrapObject = null;
        
        for (int i = 0; i < allTrapTriggers.Length; i++)
        {
            TrapTrigger trapTrigger = allTrapTriggers[i];
            TrapIdentifier existingId = trapTrigger.GetComponent<TrapIdentifier>();
            
            Debug.Log($"TRAP DEBUG: TrapTrigger {i}: {trapTrigger.name}, HasTrapId={existingId != null}, ValidId={existingId?.HasValidId()}, enabled={trapTrigger.enabled}");
            
            // Look for TrapTrigger that doesn't have a valid ID yet (newly spawned)
            if (existingId == null || !existingId.HasValidId())
            {
                Debug.Log($"TRAP DEBUG: Found uninitialized TrapTrigger on {trapTrigger.name}, initializing for trap {trapId}");
                
                // This looks like a newly spawned trap object that needs initialization
                foundTrapObject = trapTrigger.gameObject;
                
                // Disable the TrapTrigger component IMMEDIATELY to prevent any server calls
                trapTrigger.enabled = false;
                Debug.Log($"TRAP DEBUG: Disabled TrapTrigger for discovered trap {trapId} on object {trapTrigger.name}");
                
                // Get or add TrapIdentifier
                TrapIdentifier trapIdentifier = existingId;
                if (trapIdentifier == null)
                {
                    trapIdentifier = trapTrigger.gameObject.AddComponent<TrapIdentifier>();
                    Debug.Log($"TRAP DEBUG: Added TrapIdentifier component to {trapTrigger.name}");
                }
                
                // Initialize this as a discovered trap (not player-placed)
                trapIdentifier.Initialize(trapId, false);
                spawnedTrapObjects[trapId] = trapTrigger.gameObject;
                Debug.Log($"TRAP DEBUG: Successfully initialized discovered trap object with ID: {trapId} on {trapTrigger.name}");
                break;
            }
            else
            {
                Debug.Log($"TRAP DEBUG: Skipping {trapTrigger.name} - already has valid ID {existingId.TrapId}");
            }
        }
        
        if (foundTrapObject == null)
        {
            Debug.LogWarning($"TRAP DEBUG: Could not find uninitialized TrapTrigger to initialize for trap ID {trapId} - searched {allTrapTriggers.Length} TrapTrigger components");
        }
    }
    
    IEnumerator InitializeDiscoveredTrap(int trapId, Vector3 spawnPosition)
    {
        // Wait a frame for the object to be fully spawned
        yield return null;
        
        // Find the newly spawned object near the spawn position
        Collider[] nearbyObjects = Physics.OverlapSphere(spawnPosition, 1f);
        foreach (Collider col in nearbyObjects)
        {
            // Check if this object has a TrapTrigger
            TrapTrigger trapTrigger = col.GetComponent<TrapTrigger>();
            if (trapTrigger != null)
            {
                // Mark this as a discovered trap to prevent server placement
                trapTrigger.MarkAsDiscoveredTrap();
                Debug.Log("Marked TrapTrigger as discovered trap to prevent duplication");
            }
            
            // Get or add TrapIdentifier
            TrapIdentifier trapIdentifier = col.GetComponent<TrapIdentifier>();
            if (trapIdentifier == null)
            {
                trapIdentifier = col.gameObject.AddComponent<TrapIdentifier>();
            }
            
            if (!trapIdentifier.HasValidId())
            {
                // Initialize this as a discovered trap (not player-placed)
                trapIdentifier.Initialize(trapId, false);
                spawnedTrapObjects[trapId] = col.gameObject;
                Debug.Log($"Initialized discovered trap object with ID: {trapId}");
                break;
            }
        }
    }
    
    public void RegisterPlayerTrap(int trapId, GameObject trapObject)
    {
        if (trapId > 0 && trapObject != null)
        {
            spawnedTrapObjects[trapId] = trapObject;
            Debug.Log($"Registered player trap with ID: {trapId}");
        }
    }
    
    void RefreshSpawnedTrapObjects()
    {
        // Find all existing trap objects in the scene and register them
        spawnedTrapObjects.Clear();
        
        TrapIdentifier[] existingTraps = FindObjectsByType<TrapIdentifier>(FindObjectsSortMode.None);
        foreach (TrapIdentifier trapId in existingTraps)
        {
            if (trapId.HasValidId())
            {
                spawnedTrapObjects[trapId.TrapId] = trapId.gameObject;
                Debug.Log($"Found existing trap in scene: ID={trapId.TrapId}");
            }
        }
        
        Debug.Log($"Refreshed trap objects: {spawnedTrapObjects.Count} traps registered");
    }
    
    void UpdateBalanceDisplay(string balance)
    {
        currentBalance = balance;
        Debug.Log($"Balance updated: {balance} tokens");
    }
    
    void OnGUI()
    {
        // Display balance in top-right corner
        GUILayout.BeginArea(new Rect(Screen.width - 200, 10, 190, 60));
        GUILayout.BeginVertical("box");
        
        GUILayout.Label($"💰 Balance: {currentBalance}", GUILayout.Height(25));
        GUILayout.Label("Rivals Tokens", GUILayout.Height(20));
        
        GUILayout.EndVertical();
        GUILayout.EndArea();
    }
}

[Serializable]
public class MovementResponse
{
    [SerializeField] public List<Trap> traps;
    [SerializeField] public string balance;
}

[Serializable]
public class Traps
{
    [SerializeField] public List<Trap> traps;
}

[Serializable]
public class Trap
{
    [SerializeField] public int id;
    [SerializeField] public float latitude;
    [SerializeField] public float longitude;
}

[Serializable]
public class PlaceTrapResponse
{
    [SerializeField] public string message;
    [SerializeField] public TrapData trap;
}

[Serializable]
public class TrapData
{
    [SerializeField] public int id;
    [SerializeField] public string location;
}
