using UnityEngine;

public class FloorButtonHelper : MonoBehaviour
{
    public ZombieSpawner zombieSpawner;
    
    // Simple method that Unity will definitely see
    public void SetFloorHeight()
    {
        if (zombieSpawner != null)
        {
            zombieSpawner.SetFloorHeightFromARPlanes();
        }
        else
        {
            Debug.LogError("FloorButtonHelper: ZombieSpawner reference is not set!");
        }
    }
}
