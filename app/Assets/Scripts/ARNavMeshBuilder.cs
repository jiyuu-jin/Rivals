using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.AI;
using Unity.AI.Navigation;

[RequireComponent(typeof(ARPlaneManager))]
public class ARNavMeshBuilder : MonoBehaviour
{
    [Header("NavMesh Settings")]
    [Tooltip("How often to rebuild the NavMesh (seconds)")]
    public float rebuildInterval = 2f;
    
    [Tooltip("Minimum plane area to include in NavMesh (square meters)")]
    public float minPlaneArea = 0.5f;
    
    [Tooltip("Height offset for NavMesh surface above detected planes")]
    public float surfaceHeight = 0.02f;
    
    [Tooltip("Build NavMesh for vertical planes as obstacles")]
    public bool createWallObstacles = true;
    
    [Tooltip("Wall obstacle height")]
    public float wallObstacleHeight = 2f;
    
    [Header("Performance")]
    [Tooltip("Maximum number of planes to process per frame")]
    public int maxPlanesPerFrame = 5;
    
    [Tooltip("Enable debug logging")]
    public bool enableDebugLogs = true;
    
    // Components
    private ARPlaneManager planeManager;
    private NavMeshSurface navMeshSurface;
    
    // State tracking
    private HashSet<TrackableId> processedPlanes = new HashSet<TrackableId>();
    private List<GameObject> navMeshObjects = new List<GameObject>();
    private List<NavMeshObstacle> wallObstacles = new List<NavMeshObstacle>();
    private float lastRebuildTime;
    private bool isRebuilding = false;
    
    // NavMesh data
    private GameObject navMeshParent;
    
    void Start()
    {
        InitializeComponents();
        SetupNavMeshParent();
        StartCoroutine(NavMeshUpdateCoroutine());
    }
    
    void InitializeComponents()
    {
        planeManager = GetComponent<ARPlaneManager>();
        if (planeManager == null)
        {
            Debug.LogError("ARNavMeshBuilder: ARPlaneManager not found!");
            enabled = false;
            return;
        }
        
        // Create NavMeshSurface if it doesn't exist
        navMeshSurface = GetComponent<NavMeshSurface>();
        if (navMeshSurface == null)
        {
            navMeshSurface = gameObject.AddComponent<NavMeshSurface>();
            if (enableDebugLogs)
                Debug.Log("ARNavMeshBuilder: Added NavMeshSurface component");
        }
        
        // Configure NavMeshSurface
        navMeshSurface.collectObjects = CollectObjects.Children;
        navMeshSurface.useGeometry = NavMeshCollectGeometry.PhysicsColliders;
        
        // Configure build settings for better connectivity
        var buildSettings = navMeshSurface.GetBuildSettings();
        buildSettings.agentRadius = 0.3f; // Smaller radius for tighter spaces
        buildSettings.agentHeight = 1.8f;
        buildSettings.agentSlope = 45f;
        buildSettings.agentClimb = 0.4f;
        buildSettings.minRegionArea = 0.1f; // Smaller regions allowed
        navMeshSurface.overrideTileSize = true;
        navMeshSurface.tileSize = 128; // Smaller tiles for better connectivity
        
        // Subscribe to plane events
        planeManager.planesChanged += OnPlanesChanged;
        
        if (enableDebugLogs)
            Debug.Log("ARNavMeshBuilder: Initialized successfully");
    }
    
    void SetupNavMeshParent()
    {
        navMeshParent = new GameObject("NavMesh Surfaces");
        navMeshParent.transform.SetParent(transform);
        
        if (enableDebugLogs)
            Debug.Log("ARNavMeshBuilder: Created NavMesh parent object");
    }
    
    void OnPlanesChanged(ARPlanesChangedEventArgs eventArgs)
    {
        // Process new planes
        foreach (var plane in eventArgs.added)
        {
            ProcessNewPlane(plane);
        }
        
        // Update existing planes
        foreach (var plane in eventArgs.updated)
        {
            UpdateExistingPlane(plane);
        }
        
        // Remove deleted planes
        foreach (var plane in eventArgs.removed)
        {
            RemovePlane(plane);
        }
        
        // Schedule NavMesh rebuild if we have changes
        if (eventArgs.added.Count > 0 || eventArgs.updated.Count > 0 || eventArgs.removed.Count > 0)
        {
            ScheduleNavMeshRebuild();
        }
    }
    
    void ProcessNewPlane(ARPlane plane)
    {
        if (processedPlanes.Contains(plane.trackableId))
            return;
            
        processedPlanes.Add(plane.trackableId);
        
        // Check if plane is large enough
        float area = plane.size.x * plane.size.y;
        if (area < minPlaneArea)
        {
            if (enableDebugLogs)
                Debug.Log($"ARNavMeshBuilder: Skipping small plane {plane.trackableId} (area: {area:F2}m²)");
            return;
        }
        
        if (plane.alignment == PlaneAlignment.HorizontalUp)
        {
            CreateNavMeshSurface(plane);
            if (enableDebugLogs)
                Debug.Log($"ARNavMeshBuilder: Created NavMesh surface for floor plane {plane.trackableId}");
        }
        else if (plane.alignment == PlaneAlignment.Vertical && createWallObstacles)
        {
            CreateWallObstacle(plane);
            if (enableDebugLogs)
                Debug.Log($"ARNavMeshBuilder: Created wall obstacle for plane {plane.trackableId}");
        }
    }
    
    void UpdateExistingPlane(ARPlane plane)
    {
        if (!processedPlanes.Contains(plane.trackableId))
        {
            ProcessNewPlane(plane);
            return;
        }
        
        // Update existing NavMesh objects based on plane changes
        if (plane.alignment == PlaneAlignment.HorizontalUp)
        {
            UpdateNavMeshSurface(plane);
        }
        else if (plane.alignment == PlaneAlignment.Vertical && createWallObstacles)
        {
            UpdateWallObstacle(plane);
        }
    }
    
    void RemovePlane(ARPlane plane)
    {
        processedPlanes.Remove(plane.trackableId);
        
        // Remove associated NavMesh objects
        RemoveNavMeshObjectsForPlane(plane.trackableId);
        
        if (enableDebugLogs)
            Debug.Log($"ARNavMeshBuilder: Removed plane {plane.trackableId}");
    }
    
    void CreateNavMeshSurface(ARPlane plane)
    {
        // Create a collider object for NavMesh generation
        GameObject navMeshObj = new GameObject($"NavMesh_Floor_{plane.trackableId}");
        navMeshObj.transform.SetParent(navMeshParent.transform);
        
        // Position and scale the object to match the plane
        navMeshObj.transform.position = plane.transform.position + Vector3.up * surfaceHeight;
        navMeshObj.transform.rotation = plane.transform.rotation;
        navMeshObj.transform.localScale = new Vector3(plane.size.x, 0.1f, plane.size.y);
        
        // Add a box collider for NavMesh generation
        BoxCollider collider = navMeshObj.AddComponent<BoxCollider>();
        collider.size = Vector3.one;
        
        // Set layer for NavMesh generation
        navMeshObj.layer = 0; // Default layer
        
        // Store reference
        navMeshObjects.Add(navMeshObj);
        
        // Store plane ID for future reference
        PlaneReference planeRef = navMeshObj.AddComponent<PlaneReference>();
        planeRef.planeId = plane.trackableId;
    }
    
    void CreateWallObstacle(ARPlane plane)
    {
        // Create NavMesh obstacle for walls
        GameObject obstacleObj = new GameObject($"NavMesh_Wall_{plane.trackableId}");
        obstacleObj.transform.SetParent(navMeshParent.transform);
        
        // Position and scale
        obstacleObj.transform.position = plane.transform.position;
        obstacleObj.transform.rotation = plane.transform.rotation;
        
        // Add NavMesh obstacle
        NavMeshObstacle obstacle = obstacleObj.AddComponent<NavMeshObstacle>();
        obstacle.shape = NavMeshObstacleShape.Box;
        obstacle.size = new Vector3(plane.size.x, wallObstacleHeight, 0.2f);
        obstacle.carving = true;
        
        wallObstacles.Add(obstacle);
        
        // Store plane ID
        PlaneReference planeRef = obstacleObj.AddComponent<PlaneReference>();
        planeRef.planeId = plane.trackableId;
    }
    
    void UpdateNavMeshSurface(ARPlane plane)
    {
        // Find and update the corresponding NavMesh object
        foreach (var obj in navMeshObjects)
        {
            if (obj == null) continue;
            
            PlaneReference planeRef = obj.GetComponent<PlaneReference>();
            if (planeRef != null && planeRef.planeId == plane.trackableId)
            {
                // Update position and scale
                obj.transform.position = plane.transform.position + Vector3.up * surfaceHeight;
                obj.transform.rotation = plane.transform.rotation;
                obj.transform.localScale = new Vector3(plane.size.x, 0.1f, plane.size.y);
                break;
            }
        }
    }
    
    void UpdateWallObstacle(ARPlane plane)
    {
        // Find and update the corresponding wall obstacle
        foreach (var obstacle in wallObstacles)
        {
            if (obstacle == null) continue;
            
            PlaneReference planeRef = obstacle.GetComponent<PlaneReference>();
            if (planeRef != null && planeRef.planeId == plane.trackableId)
            {
                // Update position and size
                obstacle.transform.position = plane.transform.position;
                obstacle.transform.rotation = plane.transform.rotation;
                obstacle.size = new Vector3(plane.size.x, wallObstacleHeight, 0.2f);
                break;
            }
        }
    }
    
    void RemoveNavMeshObjectsForPlane(TrackableId planeId)
    {
        // Remove NavMesh objects
        for (int i = navMeshObjects.Count - 1; i >= 0; i--)
        {
            if (navMeshObjects[i] == null) continue;
            
            PlaneReference planeRef = navMeshObjects[i].GetComponent<PlaneReference>();
            if (planeRef != null && planeRef.planeId == planeId)
            {
                Destroy(navMeshObjects[i]);
                navMeshObjects.RemoveAt(i);
            }
        }
        
        // Remove wall obstacles
        for (int i = wallObstacles.Count - 1; i >= 0; i--)
        {
            if (wallObstacles[i] == null) continue;
            
            PlaneReference planeRef = wallObstacles[i].GetComponent<PlaneReference>();
            if (planeRef != null && planeRef.planeId == planeId)
            {
                Destroy(wallObstacles[i].gameObject);
                wallObstacles.RemoveAt(i);
            }
        }
    }
    
    void ScheduleNavMeshRebuild()
    {
        // Don't rebuild too frequently
        if (Time.time - lastRebuildTime < rebuildInterval || isRebuilding)
            return;
            
        StartCoroutine(RebuildNavMeshCoroutine());
    }
    
    // Public method to force rebuild for debugging
    public void ForceRebuildNavMesh()
    {
        Debug.Log("ARNavMeshBuilder: Forcing NavMesh rebuild");
        StartCoroutine(RebuildNavMeshCoroutine());
    }
    
    IEnumerator RebuildNavMeshCoroutine()
    {
        isRebuilding = true;
        lastRebuildTime = Time.time;
        
        if (enableDebugLogs)
            Debug.Log("ARNavMeshBuilder: Starting NavMesh rebuild...");
        
        // Wait a frame to ensure all plane updates are complete
        yield return null;
        
        // Clean up null references
        navMeshObjects.RemoveAll(obj => obj == null);
        wallObstacles.RemoveAll(obstacle => obstacle == null);
        
        // Rebuild the NavMesh
        if (navMeshSurface != null)
        {
            navMeshSurface.BuildNavMesh();
            
            if (enableDebugLogs)
                Debug.Log($"ARNavMeshBuilder: NavMesh rebuilt with {navMeshObjects.Count} surfaces and {wallObstacles.Count} obstacles");
        }
        
        isRebuilding = false;
    }
    
    IEnumerator NavMeshUpdateCoroutine()
    {
        while (enabled)
        {
            yield return new WaitForSeconds(rebuildInterval);
            
            // Periodic rebuild to ensure NavMesh stays current
            if (navMeshObjects.Count > 0 && !isRebuilding)
            {
                ScheduleNavMeshRebuild();
            }
        }
    }
    
    void OnDestroy()
    {
        // Unsubscribe from events
        if (planeManager != null)
        {
            planeManager.planesChanged -= OnPlanesChanged;
        }
        
        // Clean up NavMesh objects
        if (navMeshParent != null)
        {
            Destroy(navMeshParent);
        }
    }
    
    // Helper component to track plane IDs
    private class PlaneReference : MonoBehaviour
    {
        public TrackableId planeId;
    }
    
    // Debug information
    void OnGUI()
    {
        if (!enableDebugLogs) return;
        
        GUILayout.BeginArea(new Rect(35, 220, 300, 200));
        GUILayout.BeginVertical("box");
        
        GUILayout.Label("AR NavMesh Builder Debug");
        GUILayout.Label($"Processed Planes: {processedPlanes.Count}");
        GUILayout.Label($"NavMesh Surfaces: {navMeshObjects.Count}");
        GUILayout.Label($"Wall Obstacles: {wallObstacles.Count}");
        GUILayout.Label($"Is Rebuilding: {isRebuilding}");
        GUILayout.Label($"Last Rebuild: {Time.time - lastRebuildTime:F1}s ago");
        
        if (GUILayout.Button("Force Rebuild"))
        {
            StartCoroutine(RebuildNavMeshCoroutine());
        }
        
        GUILayout.EndVertical();
        GUILayout.EndArea();
    }
}
