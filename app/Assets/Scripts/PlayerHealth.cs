using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    [Header("Player Health Settings")]
    [Tooltip("Maximum player health")]
    public int maxHealth = 100;
    
    [Tooltip("Current player health")]
    public int currentHealth;
    
    [Tooltip("Screen flash color when damaged")]
    public Color damageFlashColor = new Color(1f, 0f, 0f, 0.3f);
    
    [Tooltip("Duration of damage flash")]
    public float damageFlashDuration = 0.2f;
    
    [Header("Audio")]
    [Tooltip("Sound when player takes damage")]
    public AudioClip damageSound;
    
    [Header("Death Screen")]
    [Tooltip("Reference to death screen controller")]
    public DeathScreen deathScreen;
    
    [Tooltip("Sound when player dies")]
    public AudioClip deathSound;
    
    [Header("API Settings")]
    [Tooltip("Server URL for API calls")]
    public string serverUrl = "http://10.1.9.21:3000";
    
    [Tooltip("Player username for API calls")]
    public string username = "player1";
    
    // Events
    public event Action<int> OnHealthChanged;
    public event Action OnPlayerDeath;
    public event Action OnPlayerRespawn;
    
    // Components
    private AudioSource audioSource;
    private Canvas damageOverlay;
    
    // State
    public bool IsDead { get; private set; } = false;
    private int causingTrapId = -1; // Track which trap caused death, -1 means no trap
    
    void Start()
    {
        // Initialize health
        currentHealth = maxHealth;
        
        // Get audio source
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        // Find death screen if not assigned
        if (deathScreen == null)
        {
            deathScreen = FindFirstObjectByType<DeathScreen>();
        }
        
        // Start game scoring
        GameScore.StartGame();
        
        // Create damage overlay
        CreateDamageOverlay();
        
        Debug.Log($"PlayerHealth: Initialized with {currentHealth}/{maxHealth} health");
    }
    
    void CreateDamageOverlay()
    {
        // Create a canvas for damage effects
        GameObject overlayObj = new GameObject("DamageOverlay");
        overlayObj.transform.SetParent(transform);
        
        damageOverlay = overlayObj.AddComponent<Canvas>();
        damageOverlay.renderMode = RenderMode.ScreenSpaceOverlay;
        damageOverlay.sortingOrder = 1000; // Render on top
        
        // Add the actual overlay image
        GameObject imageObj = new GameObject("DamageImage");
        imageObj.transform.SetParent(overlayObj.transform);
        
        UnityEngine.UI.Image damageImage = imageObj.AddComponent<UnityEngine.UI.Image>();
        damageImage.color = Color.clear; // Start transparent
        damageImage.raycastTarget = false; // Don't block input
        
        // Make it fill the screen
        RectTransform rectTransform = damageImage.GetComponent<RectTransform>();
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;
        
        // Initially hide the overlay
        damageOverlay.gameObject.SetActive(false);
    }
    
    public void TakeDamage(int damage)
    {
        if (IsDead) return;
        
        Debug.Log($"PlayerHealth: Taking {damage} damage. Health before: {currentHealth}/{maxHealth}");
        
        currentHealth -= damage;
        currentHealth = Mathf.Max(0, currentHealth); // Don't go below 0
        
        Debug.Log($"PlayerHealth: Health after damage: {currentHealth}/{maxHealth}");
        
        // Trigger damage effects
        OnHealthChanged?.Invoke(currentHealth);
        ShowDamageEffect();
        
        // Play damage sound
        if (audioSource != null && damageSound != null)
        {
            audioSource.PlayOneShot(damageSound);
        }
        
        // Check for death
        if (currentHealth <= 0)
        {
            Die();
        }
    }
    
    public void TakeDamageFromTrap(int damage, int trapId)
    {
        if (IsDead) return;
        
        Debug.Log($"PlayerHealth: Taking {damage} damage from trap {trapId}");
        
        // Store the trap that caused this damage
        causingTrapId = trapId;
        
        // Use regular damage method
        TakeDamage(damage);
    }
    
    public void Heal(int healAmount)
    {
        if (IsDead) return;
        
        currentHealth += healAmount;
        currentHealth = Mathf.Min(maxHealth, currentHealth); // Don't exceed max
        
        OnHealthChanged?.Invoke(currentHealth);
        
        Debug.Log($"PlayerHealth: Healed {healAmount}. Current health: {currentHealth}/{maxHealth}");
    }
    
    void ShowDamageEffect()
    {
        if (damageOverlay != null)
        {
            StartCoroutine(DamageFlashCoroutine());
        }
    }
    
    System.Collections.IEnumerator DamageFlashCoroutine()
    {
        damageOverlay.gameObject.SetActive(true);
        
        UnityEngine.UI.Image damageImage = damageOverlay.GetComponentInChildren<UnityEngine.UI.Image>();
        if (damageImage != null)
        {
            // Flash to damage color
            damageImage.color = damageFlashColor;
            
            // Fade out over time
            float elapsed = 0f;
            while (elapsed < damageFlashDuration)
            {
                elapsed += Time.deltaTime;
                float alpha = Mathf.Lerp(damageFlashColor.a, 0f, elapsed / damageFlashDuration);
                damageImage.color = new Color(damageFlashColor.r, damageFlashColor.g, damageFlashColor.b, alpha);
                yield return null;
            }
            
            // Ensure it's fully transparent
            damageImage.color = Color.clear;
        }
        
        damageOverlay.gameObject.SetActive(false);
    }
    
    void Die()
    {
        if (IsDead) return;
        
        IsDead = true;
        
        Debug.Log("PlayerHealth: Player died!");
        
        // Call the API to report death
        StartCoroutine(ReportDeath());
        
        // Play death sound
        if (audioSource != null && deathSound != null)
        {
            audioSource.PlayOneShot(deathSound);
        }
        
        // Show death screen
        if (deathScreen != null)
        {
            deathScreen.ShowDeathScreen();
        }
        else
        {
            Debug.LogWarning("PlayerHealth: No death screen found!");
        }
        
        // Trigger death event
        OnPlayerDeath?.Invoke();
        
        // Here you could add:
        // - Game over screen
        // - Respawn logic
        // - Score submission
        // - Scene restart
        
        ShowGameOverEffect();
    }
    
    void ShowGameOverEffect()
    {
        // For now, just log
        Debug.Log("PlayerHealth: Game Over - implement restart logic here");
        
        // Could add:
        // - Fade to black
        // - Game over UI
        // - Restart button
        // - Score display
    }
    
    IEnumerator ReportDeath()
    {
        Debug.Log("PlayerHealth: Reporting death to server...");
        
        string json_body;
        if (causingTrapId > 0)
        {
            // Death caused by trap
            json_body = $"{{ \"username\": \"{username}\", \"trapId\": {causingTrapId} }}";
            Debug.Log($"PlayerHealth: Reporting death by trap {causingTrapId}");
        }
        else
        {
            // Death caused by monster
            json_body = $"{{ \"username\": \"{username}\" }}";
            Debug.Log("PlayerHealth: Reporting death by monster");
        }
        
        using (UnityWebRequest www = UnityWebRequest.Post($"{serverUrl}/api/die", json_body, "application/json"))
        {
            yield return www.SendWebRequest();
            
            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"PlayerHealth: Failed to report death: {www.error}");
            }
            else
            {
                Debug.Log("PlayerHealth: Death reported successfully to server");
            }
        }
        
        // Reset trap ID for next death
        causingTrapId = -1;
    }
    
    public void Respawn()
    {
        IsDead = false;
        currentHealth = maxHealth;
        OnHealthChanged?.Invoke(currentHealth);
        
        Debug.Log("PlayerHealth: Player respawned with full health");
        
        // Trigger respawn event if needed
        OnPlayerRespawn?.Invoke();
    }
    
    public float GetHealthPercentage()
    {
        return (float)currentHealth / maxHealth;
    }
    
    // Debug GUI
    void OnGUI()
    {
        // Show health below status bar and away from edges
        GUILayout.BeginArea(new Rect(35, 80, 300, 120));
        GUILayout.BeginVertical("box");
        
        GUILayout.Label($"Health: {currentHealth}/{maxHealth}", GUILayout.Height(25));
        
        // Health bar - much bigger
        Rect healthBarRect = GUILayoutUtility.GetRect(280, 50); // Much wider and taller
        GUI.DrawTexture(healthBarRect, Texture2D.whiteTexture, ScaleMode.StretchToFill, true, 0f, Color.red, 0f, 0f);
        
        float healthPercent = GetHealthPercentage();
        Rect healthFillRect = new Rect(healthBarRect.x, healthBarRect.y, healthBarRect.width * healthPercent, healthBarRect.height);
        GUI.DrawTexture(healthFillRect, Texture2D.whiteTexture, ScaleMode.StretchToFill, true, 0f, Color.green, 0f, 0f);
        
        if (IsDead)
        {
            GUILayout.Label("DEAD");
            if (GUILayout.Button("Respawn"))
            {
                Respawn();
            }
        }
        
        GUILayout.EndVertical();
        GUILayout.EndArea();
    }
}
