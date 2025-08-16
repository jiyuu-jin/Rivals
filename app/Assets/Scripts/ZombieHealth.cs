using System.Collections;
using UnityEngine;

public class ZombieHealth : MonoBehaviour
{
    [Tooltip("Maximum health points")]
    public int maxHealth = 100;
    
    [Tooltip("Current health points")]
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
    
    private bool isDead = false;
    private Renderer[] renderers;
    private AudioSource audioSource;
    private Color[] originalColors;
    
    void Start()
    {
        // Initialize health
        currentHealth = maxHealth;
        
        // Get renderers for visual effects
        renderers = GetComponentsInChildren<Renderer>();
        originalColors = new Color[renderers.Length];
        
        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i].material.HasProperty("_Color"))
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
        
        // Add the Zombie tag if not already tagged
        if (gameObject.tag != "Zombie")
        {
            gameObject.tag = "Zombie";
        }
    }
    
    public void TakeDamage(int damage)
    {
        if (isDead) return;
        
        currentHealth -= damage;
        
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
}
