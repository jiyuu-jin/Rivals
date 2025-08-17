using UnityEngine;
using System.Collections;

public class TrapTrigger : MonoBehaviour
{
    private TrapIdentifier trapIdentifier;
    
    private bool hasTriggeredPlacement = false;
    
    void Start()
    {
        Debug.Log($"TRAP DEBUG: TrapTrigger.Start() called on {gameObject.name}, enabled={enabled}");
        
        // Get or add TrapIdentifier component
        trapIdentifier = GetComponent<TrapIdentifier>();
        if (trapIdentifier == null)
        {
            trapIdentifier = gameObject.AddComponent<TrapIdentifier>();
            Debug.Log($"TRAP DEBUG: Added TrapIdentifier to {gameObject.name}");
        }
        else
        {
            Debug.Log($"TRAP DEBUG: Found existing TrapIdentifier on {gameObject.name}, HasValidId={trapIdentifier.HasValidId()}");
        }
        
        // When this object is spawned, check if it's a mine and trigger trap placement
        if (gameObject.name.Contains("Mine") || gameObject.name.Contains("mine"))
        {
            Debug.Log($"TRAP DEBUG: About to trigger trap placement for {gameObject.name}");
            TriggerTrapPlacement();
        }
    }
    
    // Public method that can be called to mark this trap as "discovered" and prevent server calls
    public void MarkAsDiscoveredTrap()
    {
        hasTriggeredPlacement = true; // Prevent any future server calls
        Debug.Log("TrapTrigger marked as discovered trap - will not place on server");
    }

    void TriggerTrapPlacement()
    {
        // Wait a frame to ensure TrapIdentifier is properly initialized
        StartCoroutine(DelayedTrapPlacement());
    }
    
    IEnumerator DelayedTrapPlacement()
    {
        // Wait multiple frames to ensure InitializeDiscoveredTrap has time to run
        yield return null; // Wait one frame
        yield return null; // Wait another frame to be safe
        
        Debug.Log($"TRAP DEBUG: DelayedTrapPlacement checking {gameObject.name}, enabled={enabled}");
        
        // Check if this component was disabled (meaning it's a server-discovered trap)
        if (!enabled)
        {
            Debug.Log($"TRAP DEBUG: TrapTrigger disabled on {gameObject.name} - skipping server placement");
            yield break;
        }
        
        // Check if we've already been marked as a discovered trap
        if (hasTriggeredPlacement)
        {
            Debug.Log($"TRAP DEBUG: Skipping trap placement - {gameObject.name} already marked as discovered trap");
            yield break;
        }
        
        // Only place trap if this is a new player-placed trap (not one from server discovery)
        if (trapIdentifier.HasValidId())
        {
            Debug.Log($"TRAP DEBUG: Skipping trap placement - {gameObject.name} already has server ID {trapIdentifier.TrapId}");
            yield break;
        }
        
        // Mark that we're about to trigger placement to prevent duplicate calls
        hasTriggeredPlacement = true;
        
        Debug.Log($"TRAP DEBUG: Triggered trap placement from spawned mine {gameObject.name}");
        
        // Find LocationMonitor and call PlaceTrap
        LocationMonitor locationMonitor = FindFirstObjectByType<LocationMonitor>();
        if (locationMonitor != null)
        {
            StartCoroutine(locationMonitor.PlaceTrap(OnTrapPlaced));
        }
        else
        {
            Debug.LogError($"TRAP DEBUG: LocationMonitor not found - cannot place trap for {gameObject.name}");
        }
    }
    
    void OnTrapPlaced(int trapId)
    {
        if (trapId > 0)
        {
            // Initialize this trap object with the server-returned ID
            trapIdentifier.Initialize(trapId, true);
            
            // Register this trap with LocationMonitor to avoid duplicates
            LocationMonitor locationMonitor = FindFirstObjectByType<LocationMonitor>();
            if (locationMonitor != null)
            {
                locationMonitor.RegisterPlayerTrap(trapId, gameObject);
            }
            
            Debug.Log($"Trap object initialized with server ID: {trapId}");
        }
        else
        {
            Debug.LogError("Failed to place trap on server");
            // Optionally destroy this object if server placement failed
            // Destroy(gameObject);
        }
    }
}
