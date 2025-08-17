using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class DeathScreen : MonoBehaviour
{
    [Header("Settings")]
    public string deathText = "YOU DIED";
    public Color overlayColor = new Color(0, 0, 0, 0.8f);
    public Color buttonColor = new Color(0.2f, 0.7f, 0.2f, 1f); // Green
    public Color textColor = new Color(1f, 0.2f, 0.2f, 1f); // Red
    
    private Canvas deathCanvas;
    private GameObject deathPanel;
    private Button respawnButton;
    private Text deathMessage;
    private Text scoreText;
    
    private PlayerHealth playerHealth;
    private ZombieSpawner zombieSpawner;
    private ZombieShooter zombieShooter;
    private bool isDeathScreenActive = false;
    
    void Start()
    {
        CreateDeathUI();
        FindGameComponents();
        HideDeathScreen();
    }
    
    void FindGameComponents()
    {
        // Find PlayerHealth (should be on Main Camera)
        Camera mainCamera = Camera.main;
        if (mainCamera != null)
        {
            playerHealth = mainCamera.GetComponent<PlayerHealth>();
            zombieShooter = mainCamera.GetComponent<ZombieShooter>();
        }
        
        // Find ZombieSpawner
        zombieSpawner = FindFirstObjectByType<ZombieSpawner>();
        
        Debug.Log($"DeathScreen: Found components - PlayerHealth: {playerHealth != null}, ZombieSpawner: {zombieSpawner != null}, ZombieShooter: {zombieShooter != null}");
    }
    
    void CreateDeathUI()
    {
        // Create Canvas
        GameObject canvasObject = new GameObject("DeathScreenCanvas");
        deathCanvas = canvasObject.AddComponent<Canvas>();
        deathCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        deathCanvas.sortingOrder = 1000; // Ensure it's on top
        
        // Add Canvas Scaler
        CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        scaler.matchWidthOrHeight = 0.5f;
        
        // Add GraphicRaycaster for button interactions
        canvasObject.AddComponent<GraphicRaycaster>();
        
        // Create full-screen overlay panel
        GameObject panelObject = new GameObject("DeathPanel");
        panelObject.transform.SetParent(canvasObject.transform, false);
        deathPanel = panelObject;
        
        Image panelImage = panelObject.AddComponent<Image>();
        panelImage.color = overlayColor;
        
        RectTransform panelRect = panelObject.GetComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;
        
        // Create "YOU DIED" text
        GameObject deathTextObject = new GameObject("DeathText");
        deathTextObject.transform.SetParent(panelObject.transform, false);
        
        deathMessage = deathTextObject.AddComponent<Text>();
        deathMessage.text = deathText;
        deathMessage.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        deathMessage.fontSize = 80;
        deathMessage.color = textColor;
        deathMessage.alignment = TextAnchor.MiddleCenter;
        deathMessage.fontStyle = FontStyle.Bold;
        
        RectTransform deathTextRect = deathTextObject.GetComponent<RectTransform>();
        deathTextRect.anchorMin = new Vector2(0.1f, 0.6f);
        deathTextRect.anchorMax = new Vector2(0.9f, 0.8f);
        deathTextRect.offsetMin = Vector2.zero;
        deathTextRect.offsetMax = Vector2.zero;
        
        // Create score text
        GameObject scoreTextObject = new GameObject("ScoreText");
        scoreTextObject.transform.SetParent(panelObject.transform, false);
        
        scoreText = scoreTextObject.AddComponent<Text>();
        scoreText.text = "Score: 0";
        scoreText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        scoreText.fontSize = 40;
        scoreText.color = Color.white;
        scoreText.alignment = TextAnchor.MiddleCenter;
        
        RectTransform scoreTextRect = scoreTextObject.GetComponent<RectTransform>();
        scoreTextRect.anchorMin = new Vector2(0.1f, 0.45f);
        scoreTextRect.anchorMax = new Vector2(0.9f, 0.55f);
        scoreTextRect.offsetMin = Vector2.zero;
        scoreTextRect.offsetMax = Vector2.zero;
        
        // Create respawn button
        GameObject buttonObject = new GameObject("RespawnButton");
        buttonObject.transform.SetParent(panelObject.transform, false);
        
        Image buttonImage = buttonObject.AddComponent<Image>();
        buttonImage.color = buttonColor;
        
        respawnButton = buttonObject.AddComponent<Button>();
        respawnButton.targetGraphic = buttonImage;
        respawnButton.onClick.AddListener(OnRespawnClicked);
        
        RectTransform buttonRect = buttonObject.GetComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(0.3f, 0.25f);
        buttonRect.anchorMax = new Vector2(0.7f, 0.35f);
        buttonRect.offsetMin = Vector2.zero;
        buttonRect.offsetMax = Vector2.zero;
        
        // Create button text
        GameObject buttonTextObject = new GameObject("ButtonText");
        buttonTextObject.transform.SetParent(buttonObject.transform, false);
        
        Text buttonText = buttonTextObject.AddComponent<Text>();
        buttonText.text = "🔄 RESPAWN";
        buttonText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        buttonText.fontSize = 36;
        buttonText.color = Color.white;
        buttonText.alignment = TextAnchor.MiddleCenter;
        buttonText.fontStyle = FontStyle.Bold;
        
        RectTransform buttonTextRect = buttonTextObject.GetComponent<RectTransform>();
        buttonTextRect.anchorMin = Vector2.zero;
        buttonTextRect.anchorMax = Vector2.one;
        buttonTextRect.offsetMin = Vector2.zero;
        buttonTextRect.offsetMax = Vector2.zero;
        
        Debug.Log("DeathScreen: UI created successfully");
    }
    
    public void ShowDeathScreen()
    {
        if (isDeathScreenActive) return;
        
        isDeathScreenActive = true;
        deathPanel.SetActive(true);
        
        // Update score display
        UpdateScoreDisplay();
        
        // Pause game
        Time.timeScale = 0f;
        
        // Disable shooting
        if (zombieShooter != null)
        {
            zombieShooter.enabled = false;
        }
        
        Debug.Log("DeathScreen: Death screen shown, game paused");
    }
    
    public void HideDeathScreen()
    {
        isDeathScreenActive = false;
        deathPanel.SetActive(false);
        
        // Resume game
        Time.timeScale = 1f;
        
        // Re-enable shooting
        if (zombieShooter != null)
        {
            zombieShooter.enabled = true;
        }
        
        Debug.Log("DeathScreen: Death screen hidden, game resumed");
    }
    
    public void OnRespawnClicked()
    {
        Debug.Log("DeathScreen: Respawn button clicked");
        
        // Reset player health
        if (playerHealth != null)
        {
            playerHealth.Respawn();
        }
        
        // Reset zombie spawning
        if (zombieSpawner != null)
        {
            zombieSpawner.ResetSpawning();
        }
        
        // Reset score
        GameScore.Reset();
        
        // Hide death screen
        HideDeathScreen();
    }
    
    void UpdateScoreDisplay()
    {
        if (scoreText != null)
        {
            scoreText.text = GameScore.GetScoreText();
        }
    }
    
    public bool IsDeathScreenActive()
    {
        return isDeathScreenActive;
    }
}
