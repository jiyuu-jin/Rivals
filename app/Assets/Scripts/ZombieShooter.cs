using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets;
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
    public int damage = 25;
    
    [Header("Visual Effects")]
    [Tooltip("Bullet prefab to spawn and animate")]
    public GameObject bulletPrefab;
    
    [Tooltip("Optional muzzle flash effect")]
    public GameObject muzzleFlashPrefab;
    
    [Tooltip("Optional hit effect (will be used by bullet)")]
    public GameObject hitEffectPrefab;
    
    [Tooltip("Sound to play when shooting")]
    public AudioClip shootSound;
    
    // Components
    private Camera arCamera;
    private AudioSource audioSource;
    private CrosshairController crosshair;
    private PlayerHealth playerHealth;
    
    // Shooting control
    private bool canShoot = true;
    private float lastShotTime = 0f;
    
    void Start()
    {
        arCamera = Camera.main;
        
        // Get PlayerHealth component
        playerHealth = GetComponent<PlayerHealth>();
        
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
        
        // Debug: Check for input - use new Input System
        bool mouseInput = Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;
        bool touchInput = Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame;
        
        if (mouseInput || touchInput)
        {
            Debug.Log($"ZombieShooter: Input detected! Mouse: {mouseInput}, Touch: {touchInput}, CanShoot: {canShoot}");
        }
        
        // Use legacy Input for better compatibility
        if (canShoot && (mouseInput || touchInput))
        {
            // Check if ObjectSpawner is enabled (mine placement mode)
            ObjectSpawner objectSpawner = FindFirstObjectByType<ObjectSpawner>();
            
            Debug.Log($"ZombieShooter: ObjectSpawner found: {objectSpawner != null}, Enabled: {(objectSpawner != null ? objectSpawner.enabled : false)}");
            
            // Only shoot if ObjectSpawner is disabled (not in mine placement mode)
            if (objectSpawner == null || !objectSpawner.enabled)
            {
                Debug.Log("ZombieShooter: Attempting to shoot!");
                Shoot();
            }
            else
            {
                Debug.Log("ZombieShooter: Not shooting because ObjectSpawner is active (mine placement mode)");
            }
        }
    }
    
    // Check if pointer is over UI
    bool IsPointerOverUI()
    {
        return UnityEngine.EventSystems.EventSystem.current != null && 
               UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject(-1);
    }
    
    void Shoot()
    {
        Debug.Log("=== ZombieShooter: SHOOT METHOD CALLED ===");
        
        // Don't shoot if player is dead
        if (playerHealth != null && playerHealth.IsDead)
        {
            Debug.Log("ZombieShooter: Cannot shoot - player is dead");
            return;
        }
        
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
        
        // Calculate shooting direction from the center of the screen
        Ray ray = arCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
        Vector3 shootDirection = ray.direction;
        
        Debug.Log($"ZombieShooter: Shooting bullet from camera center");
        Debug.Log($"ZombieShooter: Ray origin: {ray.origin}, direction: {shootDirection}");
        
        // Spawn bullet if we have a bullet prefab
        if (bulletPrefab != null)
        {
            // Spawn bullet slightly in front of camera to avoid clipping
            Vector3 spawnPosition = ray.origin + shootDirection * 0.3f;
            GameObject bullet = Instantiate(bulletPrefab, spawnPosition, Quaternion.LookRotation(shootDirection));
            
            // Initialize the bullet with shooting parameters
            Bullet bulletScript = bullet.GetComponent<Bullet>();
            if (bulletScript != null)
            {
                bulletScript.Initialize(shootDirection, damage, shootLayerMask, maxShootDistance);
                
                // Pass hit effect to bullet
                if (hitEffectPrefab != null)
                {
                    bulletScript.hitEffectPrefab = hitEffectPrefab;
                }
                
                Debug.Log($"ZombieShooter: Spawned bullet at {spawnPosition} with direction {shootDirection}");
            }
            else
            {
                Debug.LogError("ZombieShooter: Bullet prefab doesn't have Bullet script component!");
            }
        }
        else
        {
            Debug.LogWarning("ZombieShooter: No bullet prefab assigned - falling back to instant raycast");
            // Fallback to instant raycast if no bullet prefab
            PerformInstantRaycast(ray);
        }
        
        // ALWAYS show what zombies are in the scene for debugging
        GameObject[] allZombies = GameObject.FindGameObjectsWithTag("Zombie");
        Debug.Log($"ZombieShooter: Total zombies in scene with 'Zombie' tag: {allZombies.Length}");
        foreach (var zombie in allZombies)
        {
            Collider zombieCollider = zombie.GetComponent<Collider>();
            float distance = Vector3.Distance(ray.origin, zombie.transform.position);
            Debug.Log($"  - {zombie.name}: Collider={zombieCollider != null}, Distance={distance:F2}m, Layer={zombie.layer}");
        }
        
        // Also check for any object named "Parasite"
        GameObject parasite = GameObject.Find("Parasite");
        if (parasite != null)
        {
            Collider parasiteCollider = parasite.GetComponent<Collider>();
            float distance = Vector3.Distance(ray.origin, parasite.transform.position);
            Debug.Log($"ZombieShooter: Found Parasite object - Tag: {parasite.tag}, Collider: {parasiteCollider != null}, Distance: {distance:F2}m, Layer: {parasite.layer}");
        }
        else
        {
            Debug.Log("ZombieShooter: No object named 'Parasite' found in scene");
        }
        
        Debug.Log("=== ZombieShooter: SHOOT METHOD COMPLETE ===");
    }
    
    void PerformInstantRaycast(Ray ray)
    {
        Debug.Log("ZombieShooter: Performing fallback instant raycast");
        
        // Try multiple raycasts to find zombies, ignoring ARPlanes
        RaycastHit[] allHits = Physics.RaycastAll(ray, maxShootDistance, shootLayerMask);
        
        // Sort hits by distance
        System.Array.Sort(allHits, (hit1, hit2) => hit1.distance.CompareTo(hit2.distance));
        
        // Find the first hit that's not an ARPlane
        RaycastHit hitInfo = new RaycastHit();
        bool foundValidHit = false;
        
        foreach (var hit in allHits)
        {
            // Skip ARPlanes
            if (hit.collider.name.Contains("ARPlane"))
                continue;
            
            // Use this hit
            hitInfo = hit;
            foundValidHit = true;
            break;
        }
        
        if (foundValidHit)
        {
            Debug.Log($"ZombieShooter: Instant raycast hit {hitInfo.collider.name}");
            
            // Check if we hit a zombie
            if (hitInfo.collider.CompareTag("Zombie"))
            {
                OnZombieHit(hitInfo);
            }
            else
            {
                // Show hit effect if available
                if (hitEffectPrefab != null)
                {
                    Instantiate(hitEffectPrefab, hitInfo.point, Quaternion.LookRotation(hitInfo.normal));
                }
            }
        }
        else
        {
            Debug.Log("ZombieShooter: Instant raycast missed");
        }
    }
    
    void OnZombieHit(RaycastHit hit)
    {
        // Show hit effect
        if (hitEffectPrefab != null)
        {
            Instantiate(hitEffectPrefab, hit.point, Quaternion.LookRotation(hit.normal));
        }
        
        Debug.Log($"=== ZombieShooter: OnZombieHit called ===");
        Debug.Log($"Hit zombie '{hit.collider.name}' at {hit.point}, distance {hit.distance:F2}m");
        Debug.Log($"ZombieShooter damage setting: {damage}");
        
        // Apply damage to the zombie's health component
        ZombieHealth health = hit.collider.GetComponentInParent<ZombieHealth>();
        if (health != null)
        {
            Debug.Log($"Found ZombieHealth! Before damage: {health.currentHealth}/{health.maxHealth}");
            health.TakeDamage(damage);
            Debug.Log($"Applied {damage} damage to zombie. After damage: {health.currentHealth}/{health.maxHealth}");
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
