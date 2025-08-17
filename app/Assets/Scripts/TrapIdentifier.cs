using UnityEngine;

/// <summary>
/// Component that stores the server trap ID for a spawned trap object and handles player collision
/// </summary>
public class TrapIdentifier : MonoBehaviour
{
    [SerializeField] private int trapId = -1;
    [SerializeField] private bool isPlayerTrap = false;
    
    /// <summary>
    /// The server-assigned trap ID
    /// </summary>
    public int TrapId 
    { 
        get => trapId; 
        set => trapId = value; 
    }
    
    /// <summary>
    /// Whether this trap was placed by the player (vs discovered from server)
    /// </summary>
    public bool IsPlayerTrap 
    { 
        get => isPlayerTrap; 
        set => isPlayerTrap = value; 
    }
    
    /// <summary>
    /// Initialize the trap with server data
    /// </summary>
    /// <param name="serverId">Server-assigned trap ID</param>
    /// <param name="playerPlaced">Whether this trap was placed by the player</param>
    public void Initialize(int serverId, bool playerPlaced = false)
    {
        trapId = serverId;
        isPlayerTrap = playerPlaced;
        
        // Update the GameObject name to include the ID for easier debugging
        if (serverId > 0)
        {
            gameObject.name = $"{gameObject.name}_ID{serverId}";
        }
        
        Debug.Log($"Trap initialized: ID={serverId}, PlayerTrap={playerPlaced}");
    }
    
    /// <summary>
    /// Check if this trap has a valid server ID
    /// </summary>
    public bool HasValidId()
    {
        return trapId > 0;
    }
    
    void Start()
    {
        // Ensure the trap has a collider set as trigger for player detection
        Collider col = GetComponent<Collider>();
        if (col == null)
        {
            // Add a sphere collider if none exists
            col = gameObject.AddComponent<SphereCollider>();
            Debug.Log($"TrapIdentifier: Added SphereCollider to {gameObject.name}");
        }
        col.isTrigger = true;
        
        Debug.Log($"TrapIdentifier: Trap {gameObject.name} ready for collision detection");
    }
    
    void OnTriggerEnter(Collider other)
    {
        // Check if the player entered the trap trigger
        if (other.CompareTag("Player"))
        {
            Debug.Log($"TrapIdentifier: Player triggered trap {gameObject.name} (ID: {trapId})");
            
            // Find the player health component
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            if (playerHealth == null)
            {
                // Try to find it on the camera (main camera approach)
                playerHealth = Camera.main?.GetComponent<PlayerHealth>();
            }
            
            if (playerHealth != null && HasValidId())
            {
                // Deal lethal damage from trap
                int trapDamage = 100; // Instant kill
                playerHealth.TakeDamageFromTrap(trapDamage, trapId);
                Debug.Log($"TrapIdentifier: Dealt {trapDamage} damage to player from trap {trapId}");
                
                // Optionally destroy the trap after triggering
                Destroy(gameObject, 0.5f); // Small delay to allow death animation
            }
            else if (!HasValidId())
            {
                Debug.LogWarning($"TrapIdentifier: Trap {gameObject.name} has no valid ID, cannot report death");
            }
            else
            {
                Debug.LogError("TrapIdentifier: Could not find PlayerHealth component");
            }
        }
    }
}
