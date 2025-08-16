using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Handles the initial setup of the game, connecting UI elements to game systems
/// </summary>
public class GameSetup : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("The Continue button in the greeting UI")]
    public Button continueButton;

    [Header("Game Systems")]
    [Tooltip("Reference to the ZombieSpawner component")]
    public ZombieSpawner zombieSpawner;

    void Start()
    {
        // Add the CrosshairController and ZombieShooter to the main camera if they don't exist
        Camera mainCamera = Camera.main;
        if (mainCamera != null)
        {
            // Add CrosshairController if it doesn't exist
            if (mainCamera.GetComponent<CrosshairController>() == null)
            {
                mainCamera.gameObject.AddComponent<CrosshairController>();
                Debug.Log("GameSetup: Added CrosshairController to Main Camera");
            }

            // Add ZombieShooter if it doesn't exist
            if (mainCamera.GetComponent<ZombieShooter>() == null)
            {
                mainCamera.gameObject.AddComponent<ZombieShooter>();
                Debug.Log("GameSetup: Added ZombieShooter to Main Camera");
            }
        }
        else
        {
            Debug.LogError("GameSetup: Main Camera not found!");
        }

        // Connect the Continue button to enable zombie spawning
        if (continueButton != null && zombieSpawner != null)
        {
            continueButton.onClick.AddListener(zombieSpawner.EnableZombieSpawning);
            Debug.Log("GameSetup: Connected Continue button to ZombieSpawner.EnableZombieSpawning()");
        }
        else
        {
            if (continueButton == null)
                Debug.LogError("GameSetup: Continue button reference is missing!");
            if (zombieSpawner == null)
                Debug.LogError("GameSetup: ZombieSpawner reference is missing!");
        }
    }
}
