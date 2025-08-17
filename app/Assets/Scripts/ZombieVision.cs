using UnityEngine;
using System;

public class ZombieVision : MonoBehaviour
{
    [Header("Detection Settings")]
    [Tooltip("Maximum distance to detect the player")]
    public float maxDetectionRange = 12f;
    
    [Tooltip("Field of view angle in degrees")]
    [Range(30f, 180f)]
    public float fieldOfViewAngle = 120f;
    
    [Tooltip("How often to check for player visibility (seconds)")]
    public float visionUpdateRate = 0.2f;
    
    [Tooltip("Layers that block line of sight")]
    public LayerMask obstacleLayerMask = -1;
    
    [Tooltip("Height offset for vision raycast (from zombie's feet)")]
    public float eyeHeight = 1.6f;
    
    [Header("Noise Detection")]
    [Tooltip("Range for detecting gunshots and other noise")]
    public float hearingRange = 15f;
    
    [Tooltip("Time to remember last heard noise")]
    public float noiseMemoryTime = 3f;
    
    // Events
    public event Action OnPlayerDetected;
    public event Action OnPlayerLost;
    
    // State tracking
    public bool CanSeePlayer { get; private set; } = false;
    public bool HasHeardNoise { get; private set; } = false;
    
    private Vector3 lastKnownPlayerPosition;
    private Vector3 lastHeardNoisePosition;
    private float lastNoiseTime;
    private float lastVisionUpdateTime;
    private bool playerWasVisible = false;
    
    // Debug
    [Header("Debug")]
    public bool showDebugRays = true;
    public bool showFieldOfView = true;
    
    void Start()
    {
        // Subscribe to shooting events for noise detection
        SubscribeToNoiseEvents();
    }
    
    void SubscribeToNoiseEvents()
    {
        // Try to find and subscribe to shooting events
        ZombieShooter shooter = FindObjectOfType<ZombieShooter>();
        if (shooter != null)
        {
            // We'll need to add an event to ZombieShooter for this
            Debug.Log("ZombieVision: Found ZombieShooter for noise detection");
        }
    }
    
    public void UpdateVision(Vector3 playerPosition)
    {
        // Throttle vision updates for performance
        if (Time.time - lastVisionUpdateTime < visionUpdateRate)
            return;
            
        lastVisionUpdateTime = Time.time;
        
        // Store player position for reference
        lastKnownPlayerPosition = playerPosition;
        
        // Check if player is in detection range
        float distanceToPlayer = Vector3.Distance(transform.position, playerPosition);
        if (distanceToPlayer > maxDetectionRange)
        {
            if (CanSeePlayer)
            {
                LosePlayer();
            }
            return;
        }
        
        // Check if player is in field of view
        if (!IsInFieldOfView(playerPosition))
        {
            if (CanSeePlayer)
            {
                LosePlayer();
            }
            return;
        }
        
        // Check line of sight
        if (HasClearLineOfSight(playerPosition))
        {
            if (!CanSeePlayer)
            {
                DetectPlayer();
            }
        }
        else
        {
            if (CanSeePlayer)
            {
                LosePlayer();
            }
        }
    }
    
    bool IsInFieldOfView(Vector3 targetPosition)
    {
        Vector3 directionToTarget = (targetPosition - GetEyePosition()).normalized;
        Vector3 forward = transform.forward;
        
        float angle = Vector3.Angle(forward, directionToTarget);
        return angle <= fieldOfViewAngle / 2f;
    }
    
    bool HasClearLineOfSight(Vector3 targetPosition)
    {
        Vector3 eyePosition = GetEyePosition();
        Vector3 directionToTarget = targetPosition - eyePosition;
        float distanceToTarget = directionToTarget.magnitude;
        
        // Perform raycast to check for obstacles
        RaycastHit hit;
        if (Physics.Raycast(eyePosition, directionToTarget.normalized, out hit, distanceToTarget, obstacleLayerMask))
        {
            // Check if we hit an AR plane or obstacle
            if (hit.collider.name.Contains("ARPlane"))
            {
                // Hit an AR plane, this blocks line of sight
                if (showDebugRays)
                {
                    Debug.DrawRay(eyePosition, directionToTarget.normalized * hit.distance, Color.red, 0.1f);
                }
                return false;
            }
        }
        
        // Clear line of sight
        if (showDebugRays)
        {
            Debug.DrawRay(eyePosition, directionToTarget.normalized * distanceToTarget, Color.green, 0.1f);
        }
        return true;
    }
    
    Vector3 GetEyePosition()
    {
        return transform.position + Vector3.up * eyeHeight;
    }
    
    void DetectPlayer()
    {
        CanSeePlayer = true;
        playerWasVisible = true;
        Debug.Log($"ZombieVision: Player detected at distance {Vector3.Distance(transform.position, lastKnownPlayerPosition):F1}m");
        OnPlayerDetected?.Invoke();
    }
    
    void LosePlayer()
    {
        CanSeePlayer = false;
        Debug.Log("ZombieVision: Lost sight of player");
        OnPlayerLost?.Invoke();
    }
    
    /// <summary>
    /// Call this when a gunshot or other noise occurs
    /// </summary>
    public void OnNoiseHeard(Vector3 noisePosition)
    {
        float distanceToNoise = Vector3.Distance(transform.position, noisePosition);
        
        if (distanceToNoise <= hearingRange)
        {
            lastHeardNoisePosition = noisePosition;
            lastNoiseTime = Time.time;
            HasHeardNoise = true;
            
            Debug.Log($"ZombieVision: Heard noise at distance {distanceToNoise:F1}m");
            
            // If we can't see the player but heard a noise, become alert
            if (!CanSeePlayer)
            {
                OnPlayerDetected?.Invoke();
            }
        }
    }
    
    void Update()
    {
        // Update noise memory
        if (HasHeardNoise && Time.time - lastNoiseTime > noiseMemoryTime)
        {
            HasHeardNoise = false;
            Debug.Log("ZombieVision: Forgot about heard noise");
        }
    }
    
    /// <summary>
    /// Get the last known position of the player (for investigation)
    /// </summary>
    public Vector3 GetLastKnownPlayerPosition()
    {
        return lastKnownPlayerPosition;
    }
    
    /// <summary>
    /// Get the position where noise was last heard
    /// </summary>
    public Vector3 GetLastHeardNoisePosition()
    {
        return lastHeardNoisePosition;
    }
    
    /// <summary>
    /// Check if the zombie should be alert based on vision or hearing
    /// </summary>
    public bool ShouldBeAlert()
    {
        return CanSeePlayer || HasHeardNoise;
    }
    
    // Debug visualization
    void OnDrawGizmosSelected()
    {
        if (!showFieldOfView) return;
        
        Vector3 eyePosition = GetEyePosition();
        
        // Draw detection range
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(transform.position, maxDetectionRange);
        
        // Draw hearing range
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, hearingRange);
        
        // Draw field of view
        Vector3 leftBoundary = Quaternion.Euler(0, -fieldOfViewAngle / 2f, 0) * transform.forward * maxDetectionRange;
        Vector3 rightBoundary = Quaternion.Euler(0, fieldOfViewAngle / 2f, 0) * transform.forward * maxDetectionRange;
        
        Gizmos.color = CanSeePlayer ? Color.red : Color.yellow;
        Gizmos.DrawLine(eyePosition, eyePosition + leftBoundary);
        Gizmos.DrawLine(eyePosition, eyePosition + rightBoundary);
        
        // Draw arc for field of view
        int segments = 20;
        float angleStep = fieldOfViewAngle / segments;
        float startAngle = -fieldOfViewAngle / 2f;
        
        for (int i = 0; i < segments; i++)
        {
            float angle1 = startAngle + (angleStep * i);
            float angle2 = startAngle + (angleStep * (i + 1));
            
            Vector3 point1 = Quaternion.Euler(0, angle1, 0) * transform.forward * maxDetectionRange;
            Vector3 point2 = Quaternion.Euler(0, angle2, 0) * transform.forward * maxDetectionRange;
            
            Gizmos.DrawLine(eyePosition + point1, eyePosition + point2);
        }
        
        // Draw last known player position
        if (lastKnownPlayerPosition != Vector3.zero)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(lastKnownPlayerPosition, 0.3f);
            Gizmos.DrawLine(eyePosition, lastKnownPlayerPosition);
        }
        
        // Draw last heard noise position
        if (HasHeardNoise)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawSphere(lastHeardNoisePosition, 0.2f);
            Gizmos.DrawLine(transform.position, lastHeardNoisePosition);
        }
    }
}
