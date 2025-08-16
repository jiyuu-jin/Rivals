using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Simple game manager to handle UI connections and game flow
/// </summary>
public class GameManager : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("The Continue button in the greeting UI")]
    public Button continueButton;
    
    [Header("Game Systems")]
    [Tooltip("Reference to the ZombieSpawner component")]
    public ZombieSpawner zombieSpawner;
    
    void Start()
    {
        // Try to auto-find components if not assigned
        if (continueButton == null)
        {
            // Look for the Continue button by name
            GameObject buttonObj = GameObject.Find("Continue Button");
            if (buttonObj != null)
            {
                continueButton = buttonObj.GetComponent<Button>();
                Debug.Log("GameManager: Found Continue Button automatically");
            }
        }
        
        if (zombieSpawner == null)
        {
            zombieSpawner = FindFirstObjectByType<ZombieSpawner>();
            Debug.Log("GameManager: Found ZombieSpawner automatically");
        }
        
        // Connect the button to the zombie spawner
        if (continueButton != null && zombieSpawner != null)
        {
            // Clear any existing listeners first
            continueButton.onClick.RemoveAllListeners();
            
            // Add our listener
            continueButton.onClick.AddListener(() => {
                Debug.Log("GameManager: Continue button clicked!");
                zombieSpawner.EnableZombieSpawning();
                
                // Hide the greeting UI
                GameObject greetingPrompt = GameObject.Find("Greeting Prompt");
                if (greetingPrompt != null)
                {
                    greetingPrompt.SetActive(false);
                    Debug.Log("GameManager: Hidden greeting prompt");
                }
            });
            
            Debug.Log("GameManager: Successfully connected Continue button to ZombieSpawner.EnableZombieSpawning()");
        }
        else
        {
            if (continueButton == null) Debug.LogWarning("GameManager: Could not find Continue Button!");
            if (zombieSpawner == null) Debug.LogWarning("GameManager: Could not find ZombieSpawner!");
        }
    }
}
