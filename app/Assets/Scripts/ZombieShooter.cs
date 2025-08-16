using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class ZombieShooter : MonoBehaviour
{
    [Header("Shooting Settings")]
    [Tooltip("Time between shots in seconds")]
    public float fireRate = 0.5f;
    
    [Tooltip("Maximum distance for shots")]
    public float maxShootDistance = 50f;
    
    [Tooltip("Layer mask for shooting")]
    public LayerMask shootLayerMask = -1; // Default to everything
    
    [Tooltip("Damage per hit")]
    public int damage = 10;
    
    [Header("Visual Effects")]
    [Tooltip("Optional muzzle flash effect")]
    public GameObject muzzleFlashPrefab;
    
    [Tooltip("Optional hit effect")]
    public GameObject hitEffectPrefab;
    
    [Tooltip("Sound to play when shooting")]
    public AudioClip shootSound;
    
    // Components
    private Camera arCamera;
    private AudioSource audioSource;
    private CrosshairController crosshair;
    
    // Shooting control
    private bool canShoot = true;
    private float lastShotTime = 0f;
    
    void Start()
    {
        arCamera = Camera.main;
        
        // Add audio source if needed
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && shootSound != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        // Find or create crosshair
        crosshair = FindObjectOfType<CrosshairController>();
        if (crosshair == null)
        {
            GameObject crosshairObj = new GameObject("Crosshair Controller");
            crosshair = crosshairObj.AddComponent<CrosshairController>();
        }
    }
    
    void Update()
    {
        // Check if can shoot (based on time elapsed)
        if (Time.time - lastShotTime >= fireRate)
        {
            canShoot = true;
        }
        
        // Use InputSystem instead of legacy Input
        if (canShoot && Pointer.current != null && 
            Pointer.current.press.wasPressedThisFrame)
        {
            Shoot();
        }
    }
    
    void Shoot()
    {
        // Mark that we've shot
        canShoot = false;
        lastShotTime = Time.time;
        
        // Play sound effect
        if (audioSource != null && shootSound != null)
        {
            audioSource.PlayOneShot(shootSound);
        }
        
        // Show muzzle flash effect if available
        if (muzzleFlashPrefab != null)
        {
            GameObject flash = Instantiate(muzzleFlashPrefab, arCamera.transform.position + arCamera.transform.forward * 0.5f, arCamera.transform.rotation);
            Destroy(flash, 0.1f); // Destroy after a short time
        }
        
        // Perform the raycast from the center of the screen
        RaycastHit hitInfo;
        Ray ray = arCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
        
        if (Physics.Raycast(ray, out hitInfo, maxShootDistance, shootLayerMask))
        {
            // Check if we hit a zombie
            // Assuming zombies have a tag or component to identify them
            if (hitInfo.collider.CompareTag("Zombie"))
            {
                OnZombieHit(hitInfo);
            }
            else
            {
                // Hit something else
                Debug.Log($"Shot hit {hitInfo.collider.name} at distance {hitInfo.distance:F2}m");
                
                // Show hit effect if available
                if (hitEffectPrefab != null)
                {
                    Instantiate(hitEffectPrefab, hitInfo.point, Quaternion.LookRotation(hitInfo.normal));
                }
            }
        }
    }
    
    void OnZombieHit(RaycastHit hit)
    {
        // Show hit effect
        if (hitEffectPrefab != null)
        {
            Instantiate(hitEffectPrefab, hit.point, Quaternion.LookRotation(hit.normal));
        }
        
        Debug.Log($"Hit zombie at {hit.point}, distance {hit.distance:F2}m");
        
        // Apply damage to the zombie's health component
        ZombieHealth health = hit.collider.GetComponentInParent<ZombieHealth>();
        if (health != null)
        {
            health.TakeDamage(damage);
            Debug.Log($"Applied {damage} damage to zombie. Remaining health: {health.currentHealth}");
        }
        else
        {
            Debug.LogWarning("Hit a zombie but couldn't find ZombieHealth component!");
            
            // As a fallback, try to find the component in children
            health = hit.collider.GetComponentInChildren<ZombieHealth>();
            if (health != null)
            {
                health.TakeDamage(damage);
                Debug.Log($"Applied {damage} damage to zombie (found in children). Remaining health: {health.currentHealth}");
            }
            else
            {
                // For testing, destroy the zombie directly if no health component is found
                Debug.LogWarning("No ZombieHealth component found - destroying zombie directly");
                Destroy(hit.collider.gameObject);
            }
        }
    }
}
