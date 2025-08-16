using UnityEngine;

/// <summary>
/// Component that stores the server trap ID for a spawned trap object
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
}
