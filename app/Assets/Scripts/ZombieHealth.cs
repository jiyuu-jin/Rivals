using System.Collections;
using UnityEngine;
using System;

public class ZombieHealth : MonoBehaviour
{
    [Tooltip("Maximum health points")]
    public int maxHealth = 100;
    
    [Tooltip("Current health points")]
    [HideInInspector] // Hide from inspector to avoid manual setting
    public int currentHealth;
    
    [Tooltip("Optional death effect prefab")]
    public GameObject deathEffectPrefab;
    
    [Tooltip("Optional hit effect prefab")]
    public GameObject hitEffectPrefab;
    
    [Tooltip("Sound when zombie is hit")]
    public AudioClip hitSound;
    
    [Tooltip("Sound when zombie dies")]
    public AudioClip deathSound;
    
    [Tooltip("Time in seconds before removing dead zombie")]
    public float removeDelay = 3f;
    
    [Tooltip("Flash color when hit")]
    public Color hitFlashColor = new Color(1f, 0.3f, 0.3f, 1f);
    
    [Tooltip("Duration of hit flash in seconds")]
    public float hitFlashDuration = 0.2f;
    
    [HideInInspector]
    public bool isDead = false; // Made public for AI access
    
    // Events for AI system
    public event Action OnDamageTaken;
    public event Action OnDeath;
    private Renderer[] renderers;
    private AudioSource audioSource;
    private Color[] originalColors;
    private Animator animator;
    
    void Start()
    {
        // Reset dead status - important to ensure zombie is alive when spawned
        isDead = false;
        
        // Initialize health
        currentHealth = maxHealth;
        
        Debug.Log($"ZombieHealth: Initialized with {currentHealth}/{maxHealth} health");
        
        // Get renderers for visual effects
        renderers = GetComponentsInChildren<Renderer>();
        originalColors = new Color[renderers.Length];
        
        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] != null && renderers[i].material != null && renderers[i].material.HasProperty("_Color"))
            {
                originalColors[i] = renderers[i].material.color;
            }
        }
        
        // Add audio source if needed
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && (hitSound != null || deathSound != null))
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        // Get animator for animation control
        animator = GetComponent<Animator>();
        if (animator == null)
        {
            Debug.LogWarning($"ZombieHealth: No Animator found on {gameObject.name}. Animations will not play.");
        }
        
        // Add the Zombie tag if not already tagged
        if (gameObject.tag != "Zombie")
        {
            gameObject.tag = "Zombie";
        }
    }
    
    public void TakeDamage(int damage)
    {
        if (isDead) return;
        
        Debug.Log($"=== ZombieHealth: TakeDamage called on {gameObject.name} ===");
        Debug.Log($"ZombieHealth: Taking {damage} damage. Current health before: {currentHealth}/{maxHealth}");
        
        currentHealth -= damage;
        
        Debug.Log($"ZombieHealth: Health after damage: {currentHealth}/{maxHealth}");
        
        // Trigger hit animation
        if (animator != null)
        {
            animator.SetBool("IsHit", true);
            StartCoroutine(ResetHitTrigger());
        }
        
        // Notify AI system
        OnDamageTaken?.Invoke();
        
        // Visual feedback
        StartCoroutine(FlashOnHit());
        
        // Spawn hit effect
        if (hitEffectPrefab != null)
        {
            Instantiate(hitEffectPrefab, transform.position + Vector3.up, Quaternion.identity);
        }
        
        // Play hit sound
        if (audioSource != null && hitSound != null)
        {
            audioSource.PlayOneShot(hitSound);
        }
        
        // Check if dead
        if (currentHealth <= 0)
        {
            Die();
        }
    }
    
    void Die()
    {
        if (isDead) return;
        
        isDead = true;
        
        // Trigger death animation
        if (animator != null)
        {
            animator.SetBool("IsDead", true);
            // Also stop other animations
            animator.SetBool("IsHit", false);
            animator.SetFloat("Speed", 0f);
        }
        
        // Notify AI system
        OnDeath?.Invoke();
        
        // Add to score
        GameScore.AddKill();
        
        // Play death sound
        if (audioSource != null && deathSound != null)
        {
            audioSource.PlayOneShot(deathSound);
        }
        
        // Spawn death effect
        if (deathEffectPrefab != null)
        {
            Instantiate(deathEffectPrefab, transform.position, transform.rotation);
        }
        
        // Disable colliders
        Collider[] colliders = GetComponentsInChildren<Collider>();
        foreach (Collider col in colliders)
        {
            col.enabled = false;
        }
        
        // Disable any movement scripts/AI
        // Example: GetComponent<ZombieAI>().enabled = false;
        
        // Start fade out and destroy
        StartCoroutine(FadeOutAndDestroy());
    }
    
    IEnumerator FlashOnHit()
    {
        // Change material color to hit flash color
        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] != null && renderers[i].material.HasProperty("_Color"))
            {
                renderers[i].material.color = hitFlashColor;
            }
        }
        
        // Wait for flash duration
        yield return new WaitForSeconds(hitFlashDuration);
        
        // Restore original colors
        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] != null && renderers[i].material.HasProperty("_Color"))
            {
                renderers[i].material.color = originalColors[i];
            }
        }
    }
    
    IEnumerator FadeOutAndDestroy()
    {
        // Wait a moment before fading
        yield return new WaitForSeconds(1f);
        
        float duration = removeDelay - 1f; // Total fade time
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            float t = elapsed / duration;
            
            // Fade out materials
            for (int i = 0; i < renderers.Length; i++)
            {
                if (renderers[i] != null && renderers[i].material.HasProperty("_Color"))
                {
                    Color c = originalColors[i];
                    c.a = 1f - t;
                    renderers[i].material.color = c;
                }
            }
            
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        // Destroy the zombie
        Destroy(gameObject);
    }
    
    IEnumerator ResetHitTrigger()
    {
        yield return new WaitForSeconds(0.1f);
        if (animator != null)
        {
            animator.SetBool("IsHit", false);
        }
    }
}
