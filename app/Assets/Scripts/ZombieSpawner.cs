using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

/// <summary>
/// Randomly spawns zombies on detected AR floor planes
/// Auto-wires to existing AR system without breaking functionality
/// </summary>
public class ZombieSpawner : MonoBehaviour
{
    [Header("Zombie Settings")]
    [Tooltip("The zombie prefab to spawn (assign your Parasite prefab here)")]
    public GameObject zombiePrefab;
    
    [Tooltip("Maximum number of zombies alive at once")]
    public int maxZombies = 3;
    
    [Tooltip("Chance to spawn zombie when new floor plane detected (0-1)")]
    [Range(0f, 1f)]
    public float spawnChance = 0.3f;
    
    [Tooltip("Minimum time between zombie spawns (seconds)")]
    public float spawnCooldown = 5f;
    
    [Header("Floor Detection")]
    [Tooltip("Minimum floor area to spawn on (square meters)")]
    public float minFloorArea = 1.0f;
    
    [Tooltip("Minimum height below camera to consider floor (meters)")]
    public float minFloorHeight = 0.8f;
    
    [Tooltip("Maximum height below camera to consider floor (meters)")]
    public float maxFloorHeight = 2.5f;
    
    [Header("Spawn Positioning")]
    [Tooltip("How far from camera center to spawn zombies (meters)")]
    public float spawnDistance = 2f;
    
    [Tooltip("Random offset range for spawn position (meters)")]
    public float spawnRandomOffset = 1f;
    
    // Auto-wired components
    private ARPlaneManager planeManager;
    private Camera arCamera;
    
    // Spawn management
    private List<GameObject> spawnedZombies = new List<GameObject>();
    private float lastSpawnTime = 0f;
    private bool systemReady = false;
    private HashSet<TrackableId> processedPlanes = new HashSet<TrackableId>();
    private float lowestFloorHeight = float.MaxValue; // Track the lowest floor detected
    private bool hasFoundFloor = false;
    private bool floorHeightLocked = false; // Once set, don't change the floor height
    private bool spawningEnabled = false; // Only start spawning after user clicks "Ready to Start"
    
    // References to other components
    private GoalManager goalManager;
    private UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets.ObjectSpawner objectSpawner;
    
    void Start()
    {
        // Check if zombie prefab is assigned
        if (zombiePrefab != null)
        {
            Debug.Log($"ZombieSpawner: Zombie prefab assigned: {zombiePrefab.name}");
            zombiePrefab.SetActive(false);
            Debug.Log("ZombieSpawner: Hiding the zombie prefab until spawning is enabled");
        }
        else
        {
            Debug.LogError("ZombieSpawner: No zombie prefab assigned! Please assign a prefab in the inspector.");
        }
        
        StartCoroutine(InitializeSystem());
    }
    
    IEnumerator InitializeSystem()
    {
        // Wait a frame to ensure AR system is initialized
        yield return null;
        
        AutoWireComponents();
        
        if (systemReady)
        {
            StartCoroutine(MonitorPlanes());
            Debug.Log("ZombieSpawner: System initialized successfully!");
            
            // Find and connect to the Continue button
            ConnectToContinueButton();
        }
    }
    
    void ConnectToContinueButton()
    {
        // Find the Continue button in the UI
        GameObject continueButtonObj = GameObject.Find("Continue Button");
        if (continueButtonObj != null)
        {
            UnityEngine.UI.Button continueButton = continueButtonObj.GetComponent<UnityEngine.UI.Button>();
            if (continueButton != null)
            {
                continueButton.onClick.AddListener(EnableZombieSpawning);
                Debug.Log("ZombieSpawner: Connected to Continue button successfully!");
            }
            else
            {
                Debug.LogWarning("ZombieSpawner: Continue Button found but it doesn't have a Button component!");
            }
        }
        else
        {
            Debug.LogWarning("ZombieSpawner: Couldn't find Continue Button in the scene!");
        }
    }
    
    void AutoWireComponents()
    {
        // Find AR components automatically
        planeManager = FindFirstObjectByType<ARPlaneManager>();
        arCamera = Camera.main;
        
        // Find ObjectSpawner to detect mine placement
        objectSpawner = FindFirstObjectByType<UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets.ObjectSpawner>();
        if (objectSpawner != null)
        {
            // Subscribe to object spawned event to detect mine placement
            objectSpawner.objectSpawned += OnObjectSpawned;
            Debug.Log("ZombieSpawner: Successfully connected to ObjectSpawner for mine detection");
        }
        else
        {
            Debug.LogWarning("ZombieSpawner: ObjectSpawner not found - mine detection disabled");
        }
        
        // Validate setup
        if (planeManager == null)
        {
            Debug.LogError("ZombieSpawner: No ARPlaneManager found in scene!");
            return;
        }
        
        if (arCamera == null)
        {
            Debug.LogError("ZombieSpawner: No main camera found!");
            return;
        }
        
        if (zombiePrefab == null)
        {
            Debug.LogWarning("ZombieSpawner: No zombie prefab assigned!");
            return;
        }
        
        // Add required components to the camera
        AddRequiredCameraComponents();
        
        systemReady = true;
    }
    
    void AddRequiredCameraComponents()
    {
        Debug.Log($"ZombieSpawner: AddRequiredCameraComponents called. Camera: {(arCamera != null ? arCamera.name : "NULL")}");
        
        if (arCamera != null)
        {
            // Add CrosshairController if it doesn't exist
            if (arCamera.GetComponent<CrosshairController>() == null)
            {
                arCamera.gameObject.AddComponent<CrosshairController>();
                Debug.Log("ZombieSpawner: Added CrosshairController to Main Camera");
            }
            else
            {
                Debug.Log("ZombieSpawner: CrosshairController already exists on Main Camera");
            }
            
            // Add ZombieShooter if it doesn't exist
            if (arCamera.GetComponent<ZombieShooter>() == null)
            {
                ZombieShooter shooter = arCamera.gameObject.AddComponent<ZombieShooter>();
                Debug.Log("ZombieSpawner: Added ZombieShooter to Main Camera");
                
                // Connect the ZombieShooter to the ZombieHealth components
                shooter.damage = 25; // Set a reasonable damage value
            }
            else
            {
                Debug.Log("ZombieSpawner: ZombieShooter already exists on Main Camera");
            }
        }
        else
        {
            Debug.LogError("ZombieSpawner: arCamera is null! Cannot add components.");
        }
    }
    
    IEnumerator MonitorPlanes()
    {
        int planeCheckCounter = 0;
        
        while (systemReady)
        {
            // Check for new planes every half second
            yield return new WaitForSeconds(0.5f);
            
            planeCheckCounter++;
            if (planeCheckCounter % 10 == 0) // Log every 5 seconds
            {
                Debug.Log($"ZombieSpawner: Checking for AR planes. Current status: hasFoundFloor={hasFoundFloor}, spawningEnabled={spawningEnabled}, planeManager={planeManager != null}");
                if (planeManager != null)
                {
                    Debug.Log($"ZombieSpawner: Number of tracked planes: {planeManager.trackables.count}");
                }
            }
            
            if (planeManager != null)
            {
                if (planeManager.trackables.count > 0 && planeCheckCounter % 10 == 0)
                {
                    Debug.Log("ZombieSpawner: AR planes detected:");
                    foreach (var plane in planeManager.trackables)
                    {
                        Debug.Log($"ZombieSpawner: Plane {plane.trackableId} - Alignment: {plane.alignment}, Center: {plane.center}, Size: {plane.size}");
                    }
                }
                
                foreach (var plane in planeManager.trackables)
                {
                    // Check if we've already processed this plane
                    if (!processedPlanes.Contains(plane.trackableId))
                    {
                        processedPlanes.Add(plane.trackableId);
                        Debug.Log($"ZombieSpawner: Processing new plane {plane.trackableId} - Alignment: {plane.alignment}, Center: {plane.center}");
                        ProcessNewPlane(plane);
                    }
                }
            }
        }
    }
    
    void Update()
    {
        // Continuously check if we need to spawn more zombies
        if (spawningEnabled && systemReady && hasFoundFloor)
        {
            // Clean up destroyed zombies and check if we need more
            CleanupDestroyedZombies();
            
            // If we're below max zombies and cooldown has passed, try to spawn
            if (spawnedZombies.Count < maxZombies && Time.time - lastSpawnTime >= spawnCooldown)
            {
                TrySpawnZombieOnAnyValidPlane();
            }
        }
    }
    
    void TrySpawnZombieOnAnyValidPlane()
    {
        if (planeManager == null) return;
        
        // Find any valid horizontal plane to spawn on
        foreach (var plane in planeManager.trackables)
        {
            if (IsFloorPlane(plane))
            {
                Debug.Log("ZombieSpawner: Found valid plane for respawning, attempting spawn...");
                TrySpawnZombie(plane);
                break; // Only spawn one zombie per attempt
            }
        }
    }
    
    void OnDestroy()
    {
        // Clean up spawned zombies
        CleanupAllZombies();
        
        // Unsubscribe from object spawner events
        if (objectSpawner != null)
        {
            objectSpawner.objectSpawned -= OnObjectSpawned;
        }
    }
    
    void OnObjectSpawned(GameObject spawnedObject)
    {
        Debug.Log($"ZombieSpawner: Object spawned: {spawnedObject.name}");
        
        // We no longer need special handling for mines
        // Original zombie prefab position will be set when spawning is enabled
    }
    
    void ProcessNewPlane(ARPlane plane)
    {
        // Check if we should attempt to spawn
        if (!ShouldAttemptSpawn())
            return;
            
        // Check if this is a valid floor plane
        if (!IsFloorPlane(plane))
            return;
            
        // Check spawn conditions
        if (!CanSpawnZombie())
            return;
            
        // Attempt to spawn zombie
        TrySpawnZombie(plane);
    }
    
    bool ShouldAttemptSpawn()
    {
        // Check if spawning has been enabled by the ready button
        if (!spawningEnabled)
        {
            return false;
        }
        
        // Don't spawn zombies until we've detected a floor plane
        if (!hasFoundFloor)
        {
            Debug.LogWarning("ZombieSpawner: Waiting for floor detection before spawning zombies");
            return false;
        }
        
        // For initial spawning (new plane detected), check random chance
        // For respawning (Update method), we skip random chance
        if (Random.value > spawnChance)
            return false;
            
        Debug.Log("ZombieSpawner: All spawn conditions met, attempting spawn");
        return true;
    }
    
    bool IsFloorPlane(ARPlane plane)
    {
        // Must be horizontal up surface (floor, not ceiling)
        if (plane.alignment != PlaneAlignment.HorizontalUp)
        {
            Debug.Log($"ZombieSpawner: Plane rejected - not horizontal up (alignment: {plane.alignment})");
            return false;
        }
            
        // Check if the plane is large enough
        float planeArea = plane.size.x * plane.size.y;
        if (planeArea < minFloorArea)
        {
            Debug.Log($"ZombieSpawner: Plane rejected - too small ({planeArea:F2}m² < {minFloorArea}m²)");
            return false;
        }
        
        // Always update the lowest floor height when we find a lower horizontal plane
        if (!hasFoundFloor || plane.center.y < lowestFloorHeight)
        {
            float previousHeight = lowestFloorHeight;
            lowestFloorHeight = plane.center.y;
            hasFoundFloor = true;
            Debug.Log($"ZombieSpawner: New lowest floor found at height {lowestFloorHeight:F3}m");
            
            // Update all existing zombies to the new floor height
            UpdateAllZombieHeights(previousHeight);
            
            // Update the original zombie prefab height if it's active
            if (zombiePrefab != null && zombiePrefab.activeSelf)
            {
                Vector3 zombiePosition = zombiePrefab.transform.position;
                zombiePosition.y = lowestFloorHeight;
                zombiePrefab.transform.position = zombiePosition;
                Debug.Log($"ZombieSpawner: Updated original zombie prefab to new floor height Y={zombiePosition.y:F3}m");
            }
        }
        
        // Check if this plane is at a reasonable height difference from our current floor
        float heightDifference = Mathf.Abs(plane.center.y - lowestFloorHeight);
        if (heightDifference > 0.3f) // Only same-level planes within 30cm
        {
            Debug.Log($"ZombieSpawner: Plane rejected - too high above floor. Height diff: {heightDifference:F3}m");
            return false;
        }
        
        // Check if this plane is at a reasonable height below the camera
        float heightFromCamera = arCamera.transform.position.y - plane.center.y;
        if (heightFromCamera < minFloorHeight || heightFromCamera > maxFloorHeight)
        {
            Debug.Log($"ZombieSpawner: Plane rejected - unusual height from camera: {heightFromCamera:F2}m");
            return false;
        }
        
        Debug.Log($"ZombieSpawner: Valid floor plane found - {planeArea:F2}m², at floor level");
        return true;
    }
    
    bool CanSpawnZombie()
    {
        // Check if we're at max zombie limit
        CleanupDestroyedZombies();
        return spawnedZombies.Count < maxZombies;
    }
    
    void TrySpawnZombie(ARPlane plane)
    {
        // Don't spawn if we haven't found the floor yet
        if (!hasFoundFloor)
        {
            Debug.Log("ZombieSpawner: Cannot spawn - no floor detected yet");
            return;
        }
        
        // Don't spawn if we're at max capacity
        CleanupDestroyedZombies();
        if (spawnedZombies.Count >= maxZombies)
        {
            Debug.Log($"ZombieSpawner: Cannot spawn - at max capacity ({spawnedZombies.Count}/{maxZombies})");
            return;
        }
        
        Debug.Log($"ZombieSpawner: Attempting to spawn zombie using lowest floor height: {lowestFloorHeight:F3}m");
        
        // Check if zombie prefab is available
        if (zombiePrefab == null)
        {
            Debug.LogError("ZombieSpawner: Cannot spawn zombie - zombiePrefab is null!");
            return;
        }
        
        // Get the XZ position only, we'll set Y separately
        Vector3 spawnPosition = GetSpawnPosition(plane);
        
        if (spawnPosition == Vector3.zero)
        {
            Debug.Log("ZombieSpawner: Failed to find valid spawn position");
            return; // Failed to find valid position
        }
        
        // Final validation - ensure spawn position is reasonable
        if (!IsSpawnPositionValid(spawnPosition, plane))
        {
            Debug.Log($"ZombieSpawner: Invalid spawn position {spawnPosition}, skipping");
            return;
        }
        
        // When positioning the zombie, use the lowest detected floor height
        Vector3 finalPosition = spawnPosition;
        finalPosition.y = lowestFloorHeight; // Use lowest floor height detected
        
        Debug.Log($"ZombieSpawner: About to instantiate zombie at position {finalPosition}");
        
        try
        {
                    // Now instantiate the zombie at the position
        GameObject zombie = Instantiate(zombiePrefab, finalPosition, GetSpawnRotation(plane));
        
        // Ensure the ZombieHealth component is properly initialized
        ZombieHealth health = zombie.GetComponent<ZombieHealth>();
        if (health != null)
        {
            // Initialize health manually without disabling/enabling (which would reset it)
            if (health.currentHealth <= 0)
            {
                health.currentHealth = health.maxHealth;
                Debug.Log("ZombieSpawner: Manually initialized zombie health to avoid reset bug");
            }
        }
        
        // Add animation components if they don't exist
        AddAnimationComponents(zombie);
            
            if (zombie != null)
            {
                Debug.Log($"ZombieSpawner: Successfully spawned zombie at Y={finalPosition.y:F3}m (floor height)");
                
                // Make sure it's active
                zombie.SetActive(true);
        
        // Track spawned zombie
        spawnedZombies.Add(zombie);
        lastSpawnTime = Time.time;
            }
            else
            {
                Debug.LogError("ZombieSpawner: Failed to instantiate zombie - returned null");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"ZombieSpawner: Error spawning zombie: {e.Message}\n{e.StackTrace}");
        }
    }
    
    Vector3 GetSpawnPosition(ARPlane plane)
    {
        // Try multiple random positions to avoid overlapping
        for (int attempts = 0; attempts < 10; attempts++)
        {
            Vector3 candidatePosition = GetRandomPositionOnPlane(plane);
            
            // Check if position is clear of other zombies
            if (IsPositionClear(candidatePosition))
            {
                return candidatePosition;
            }
        }
        
        // If we couldn't find a clear position, don't spawn
        Debug.Log("ZombieSpawner: Could not find clear position on plane");
        return Vector3.zero;
    }
    
    Vector3 GetRandomPositionOnPlane(ARPlane plane)
    {
        // Get camera forward direction (projected on plane)
        Vector3 cameraForward = arCamera.transform.forward;
        cameraForward.y = 0; // Project to horizontal plane
        cameraForward.Normalize();
        
        // Calculate base spawn position in front of camera
        Vector3 cameraPos = arCamera.transform.position;
        Vector3 basePosition = cameraPos + cameraForward * spawnDistance;
        
        // We're only concerned with XZ positioning here
        // Y will be set separately in TrySpawnZombie
        
        // Add small random offset for variety
        Vector2 randomOffset = Random.insideUnitCircle * spawnRandomOffset;
        
        // Create initial spawn position (XZ only)
        Vector3 spawnPosition = new Vector3(
            basePosition.x + randomOffset.x,
            plane.center.y, // Temporary Y, will be replaced later
            basePosition.z + randomOffset.y
        );
        
        // Verify the position is actually on the plane bounds
        Vector3 planeLocalPos = spawnPosition - plane.center;
        planeLocalPos.y = 0; // Ignore Y for bounds check
        float maxX = plane.size.x * 0.4f; // Stay well within bounds
        float maxZ = plane.size.y * 0.4f;
        
        // Clamp to plane bounds if necessary
        planeLocalPos.x = Mathf.Clamp(planeLocalPos.x, -maxX, maxX);
        planeLocalPos.z = Mathf.Clamp(planeLocalPos.z, -maxZ, maxZ);
        
        // Get final position (XZ only)
        Vector3 finalPosition = plane.center + planeLocalPos;
        
        Debug.Log($"ZombieSpawner: XZ position calculated at ({finalPosition.x:F3}, {finalPosition.z:F3})");
        
        return finalPosition;
    }
    
    float GetAccurateSurfaceHeight(Vector3 position, ARPlane plane)
    {
        // SIMPLIFIED: Always use the plane's exact height
        // This ensures zombies are always exactly on the detected floor plane
        Debug.Log($"ZombieSpawner: Using exact plane height {plane.center.y}");
        return plane.center.y;
    }
    
    bool IsSpawnPositionValid(Vector3 position, ARPlane plane)
    {
        // Check if position height is reasonable relative to camera
        float heightFromCamera = arCamera.transform.position.y - position.y;
        // More lenient height check - AR floor detection can vary
        if (heightFromCamera < 0.3f || heightFromCamera > 4f)
        {
            Debug.Log($"ZombieSpawner: Position height invalid - {heightFromCamera:F2}m from camera");
            return false;
        }
        
        // Check if position is within plane bounds
        Vector3 planeLocalPos = position - plane.center;
        planeLocalPos.y = 0; // Ignore Y for distance check
        
        float maxX = plane.size.x * 0.45f;
        float maxZ = plane.size.y * 0.45f;
        
        if (Mathf.Abs(planeLocalPos.x) > maxX || Mathf.Abs(planeLocalPos.z) > maxZ)
        {
            Debug.Log($"ZombieSpawner: Position outside plane bounds");
            return false;
        }
        
        return true;
    }
    
    bool IsPositionClear(Vector3 position)
    {
        // Check distance from existing zombies
        foreach (var zombie in spawnedZombies)
        {
            if (zombie != null)
            {
                float distance = Vector3.Distance(position, zombie.transform.position);
                if (distance < 1.5f) // Minimum 1.5 meter spacing
                {
                    return false;
                }
            }
        }
        return true;
    }
    
    bool IsPositionOnPlane(Vector3 position, ARPlane plane)
    {
        // Simple bounds check (could be improved with actual plane mesh)
        Vector3 localPos = position - plane.center;
        localPos.y = 0; // Ignore height for this check
        
        return Mathf.Abs(localPos.x) <= plane.size.x * 0.5f && 
               Mathf.Abs(localPos.z) <= plane.size.y * 0.5f;
    }
    
    Quaternion GetSpawnRotation(ARPlane plane)
    {
        // Make zombie face the camera
        if (arCamera == null)
            return Quaternion.identity;
            
        Vector3 lookDirection = arCamera.transform.position - new Vector3(plane.center.x, 0, plane.center.z);
        lookDirection.y = 0; // Make sure the rotation is only around the Y axis
        lookDirection.Normalize();
            
        return Quaternion.LookRotation(lookDirection);
    }
    
    void CleanupDestroyedZombies()
    {
        // Count how many zombies were destroyed
        int beforeCount = spawnedZombies.Count;
        
        // Remove null references from our list
        spawnedZombies.RemoveAll(zombie => zombie == null);
        
        int afterCount = spawnedZombies.Count;
        int destroyedCount = beforeCount - afterCount;
        
        if (destroyedCount > 0)
        {
            Debug.Log($"ZombieSpawner: {destroyedCount} zombies were destroyed. Current count: {afterCount}/{maxZombies}");
            // Note: Respawning is now handled by the Update() method
        }
    }
    
    void CleanupAllZombies()
    {
        foreach (var zombie in spawnedZombies)
        {
            if (zombie != null)
                Destroy(zombie);
        }
        spawnedZombies.Clear();
    }
    
    // Update the height of all existing zombies when a new lowest floor is found
    void UpdateAllZombieHeights(float previousHeight)
    {
        // Calculate the height difference
        float heightDifference = lowestFloorHeight - previousHeight;
        
        if (Mathf.Approximately(heightDifference, 0f))
            return; // No change needed
            
        foreach (var zombie in spawnedZombies)
        {
            if (zombie != null)
            {
                // Get current position
                Vector3 position = zombie.transform.position;
                
                // Apply the height adjustment
                position.y += heightDifference;
                
                // Update position
                zombie.transform.position = position;
                
                Debug.Log($"ZombieSpawner: Adjusted zombie height by {heightDifference:F3}m to Y={position.y:F3}m");
            }
        }
        
        Debug.Log($"ZombieSpawner: Updated {spawnedZombies.Count} zombies to new floor height");
    }
    
    // Simple method for button to force set the floor height
    public void SetFloorHeightFromCurrentPosition()
    {
        if (arCamera != null)
        {
            // Set floor height to camera height minus 1.5 meters (approx average human height)
            lowestFloorHeight = arCamera.transform.position.y - 1.5f;
            hasFoundFloor = true;
            floorHeightLocked = true;
            Debug.Log($"ZombieSpawner: Floor height manually set to {lowestFloorHeight:F3}m and locked");
        }
        else
        {
            Debug.LogWarning("ZombieSpawner: No surface found below camera to set as floor");
        }
    }
    
    // Method to manually set floor height from AR planes
    public void SetFloorHeightFromARPlanes()
    {
        // Always scan for the lowest floor plane, don't lock the height
        
        if (planeManager == null || planeManager.trackables.count == 0)
        {
            Debug.LogWarning("ZombieSpawner: No AR planes detected yet");
            return;
        }
        
        // Find the lowest horizontal plane
        float lowestPlaneY = float.MaxValue;
        bool foundPlane = false;
        
        foreach (var plane in planeManager.trackables)
        {
            if (plane.alignment == PlaneAlignment.HorizontalUp && plane.center.y < lowestPlaneY)
            {
                lowestPlaneY = plane.center.y;
                foundPlane = true;
            }
        }
        
        if (foundPlane)
        {
            lowestFloorHeight = lowestPlaneY;
            hasFoundFloor = true;
            // Don't lock the floor height, allow it to update
            Debug.Log($"ZombieSpawner: Floor height set from AR planes at Y={lowestFloorHeight:F3}m");
        }
        else
        {
            Debug.LogWarning("ZombieSpawner: No suitable horizontal planes found to set floor height");
        }
    }
    
    /// <summary>
    /// Public method to enable zombie spawning when the "Ready to Start" button is clicked
    /// </summary>
    public void EnableZombieSpawning()
    {
        Debug.Log("ZombieSpawner: EnableZombieSpawning called");
        
        // Check if we have detected a floor plane first
        if (!hasFoundFloor)
        {
            Debug.LogWarning("ZombieSpawner: No floor detected yet. Trying to find AR planes...");
            
            // Try to automatically find a floor plane
            SetFloorHeightFromARPlanes();
            
            // If we still don't have a floor, set a default floor height
            if (!hasFoundFloor)
            {
                Debug.LogWarning("ZombieSpawner: No AR planes detected. Setting default floor height.");
                SetDefaultFloorHeight();
            }
        }
        
        // We should have a valid floor now, enable spawning
        spawningEnabled = true;
        
        // Position the original zombie prefab at the floor height
        if (zombiePrefab != null && hasFoundFloor)
        {
            Vector3 zombiePosition = zombiePrefab.transform.position;
            zombiePosition.y = lowestFloorHeight;
            zombiePrefab.transform.position = zombiePosition;
            
            // Initialize the ZombieHealth component without resetting it
            ZombieHealth health = zombiePrefab.GetComponent<ZombieHealth>();
            if (health != null)
            {
                // Only initialize if health is 0 or uninitialized
                if (health.currentHealth <= 0)
                {
                    health.currentHealth = health.maxHealth;
                    Debug.Log("ZombieSpawner: Manually initialized original zombie health");
                }
            }
            
            // Add animation components to the original prefab too
            AddAnimationComponents(zombiePrefab);
            
            zombiePrefab.SetActive(true);
            Debug.Log($"ZombieSpawner: Showing and positioning original zombie prefab at Y={zombiePosition.y:F3}m");
        }
        else if (zombiePrefab == null)
        {
            Debug.LogError("ZombieSpawner: Cannot show zombie prefab - it is null!");
        }
        
        Debug.Log("ZombieSpawner: Floor height set to Y=" + lowestFloorHeight + "m. Zombie spawning ENABLED!");
        
        // Force spawn a zombie for testing
        ForceSpawnZombie();
    }
    
    /// <summary>
    /// Sets a default floor height when AR plane detection fails
    /// </summary>
    private void SetDefaultFloorHeight()
    {
        if (arCamera != null)
        {
            // Set floor height to camera height minus 1.5 meters (approx average human height)
            lowestFloorHeight = arCamera.transform.position.y - 1.5f;
            hasFoundFloor = true;
            Debug.Log($"ZombieSpawner: Default floor height set to {lowestFloorHeight:F3}m (camera height - 1.5m)");
        }
        else
        {
            // Absolute fallback
            lowestFloorHeight = -1.5f;
            hasFoundFloor = true;
            Debug.Log($"ZombieSpawner: Default floor height set to {lowestFloorHeight:F3}m (absolute fallback)");
        }
    }
    
    /// <summary>
    /// Forces a zombie to spawn for testing purposes
    /// </summary>
    private void ForceSpawnZombie()
    {
        if (zombiePrefab == null)
        {
            Debug.LogError("ZombieSpawner: Cannot force spawn zombie - zombiePrefab is null!");
            return;
        }
        
        if (!hasFoundFloor)
        {
            Debug.LogError("ZombieSpawner: Cannot force spawn zombie - no floor height set!");
            return;
        }
        
        Debug.Log("ZombieSpawner: Forcing zombie spawn for testing...");
        
        // Create a position in front of the camera
        Vector3 spawnPosition = arCamera.transform.position + arCamera.transform.forward * 2f;
        spawnPosition.y = lowestFloorHeight; // Set to floor height
        
        try
        {
            // Instantiate the zombie
            GameObject zombie = Instantiate(zombiePrefab, spawnPosition, Quaternion.LookRotation(-arCamera.transform.forward));
            
            if (zombie != null)
            {
                Debug.Log($"ZombieSpawner: Successfully forced spawn of zombie at {spawnPosition}");
                zombie.SetActive(true);
                spawnedZombies.Add(zombie);
                lastSpawnTime = Time.time;
            }
            else
            {
                Debug.LogError("ZombieSpawner: Failed to force spawn zombie - returned null");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"ZombieSpawner: Error force spawning zombie: {e.Message}\n{e.StackTrace}");
        }
    }
    
    /// <summary>
    /// Add required animation components to a zombie
    /// </summary>
    void AddAnimationComponents(GameObject zombie)
    {
        if (zombie == null) return;
        
        // Add Animator if it doesn't exist
        Animator animator = zombie.GetComponent<Animator>();
        if (animator == null)
        {
            animator = zombie.AddComponent<Animator>();
            Debug.Log("ZombieSpawner: Added Animator component to zombie");
        }
        
        // Add ZombieMovement if it doesn't exist
        ZombieMovement movement = zombie.GetComponent<ZombieMovement>();
        if (movement == null)
        {
            movement = zombie.AddComponent<ZombieMovement>();
            Debug.Log("ZombieSpawner: Added ZombieMovement component to zombie");
        }
        
        // Note: The Animator Controller should be assigned manually in the prefab
        // or through the Unity Inspector for proper animation setup
    }
    
    /// <summary>
    /// Public method to disable zombie spawning
    /// </summary>
    public void DisableZombieSpawning()
    {
        spawningEnabled = false;
        Debug.Log("ZombieSpawner: Zombie spawning DISABLED!");
    }
    
    // Debug info
    void OnGUI()
    {
        if (!systemReady) return;
        
        // Don't show debug UI during greeting
        if (goalManager == null)
        {
            goalManager = FindFirstObjectByType<GoalManager>();
        }
        
        // Skip showing debug UI if greeting prompt is active
        if (goalManager != null && goalManager.greetingPrompt.activeSelf)
        {
            return;
        }
        
        GUILayout.BeginArea(new Rect(10, Screen.height - 200, 300, 200));
        GUILayout.Label("ZombieSpawner Debug:");
        
        if (hasFoundFloor)
            {
                GUILayout.Label($"Floor height: {lowestFloorHeight:F3}m {(floorHeightLocked ? "(LOCKED)" : "")}");
                GUILayout.Label($"Height above floor: {arCamera.transform.position.y - lowestFloorHeight:F2}m");
            }
        else
        {
            GUILayout.Label("No floor detected yet");
        }
        
        if (GUI.Button(new Rect(10, 100, 150, 40), "Set Floor Height"))
            {
                SetFloorHeightFromCurrentPosition();
        }
        
        if (GUI.Button(new Rect(10, 150, 150, 40), "Reset Floor Lock"))
            {
                floorHeightLocked = false;
                Debug.Log("ZombieSpawner: Floor height unlocked");
        }
        
        GUILayout.EndArea();
    }
}