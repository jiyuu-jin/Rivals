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
    
    [Tooltip("Reference to the GoalManager component")]
    public GoalManager goalManager;
    
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
        
        if (goalManager == null)
        {
            goalManager = FindFirstObjectByType<GoalManager>();
            Debug.Log("GameManager: Found GoalManager automatically");
        }
        
        // Connect the button to the game systems
        if (continueButton != null && zombieSpawner != null)
        {
            // Clear any existing listeners first
            continueButton.onClick.RemoveAllListeners();
            
            // Add our listener
            continueButton.onClick.AddListener(() => {
                Debug.Log("GameManager: Continue button clicked!");
                
                // Enable zombie spawning
                zombieSpawner.EnableZombieSpawning();
                
                // Enable Create Button directly (skip coaching screens)
                if (goalManager != null)
                {
                    // Enable Create Button without starting coaching flow
                    if (goalManager.createButton != null)
                    {
                        goalManager.createButton.SetActive(true);
                        Debug.Log("GameManager: Enabled Create Button via GoalManager");
                    }
                    
                    // Enable Options Button as well
                    if (goalManager.optionsButton != null)
                    {
                        goalManager.optionsButton.SetActive(true);
                        Debug.Log("GameManager: Enabled Options Button via GoalManager");
                    }
                    
                    // Enable the menu manager
                    if (goalManager.menuManager != null)
                    {
                        goalManager.menuManager.enabled = true;
                        Debug.Log("GameManager: Enabled Menu Manager via GoalManager");
                    }
                }
                else
                {
                    // Fallback: Find Create Button directly by name
                    GameObject createButtonObj = GameObject.Find("Create Button");
                    if (createButtonObj != null)
                    {
                        createButtonObj.SetActive(true);
                        Debug.Log("GameManager: Enabled Create Button directly by name");
                    }
                    
                    // Find and enable ARTemplateMenuManager
                    ARTemplateMenuManager menuManager = FindFirstObjectByType<ARTemplateMenuManager>();
                    if (menuManager != null)
                    {
                        menuManager.enabled = true;
                        Debug.Log("GameManager: Enabled ARTemplateMenuManager directly");
                    }
                }
                
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
