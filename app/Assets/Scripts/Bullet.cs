using System.Collections;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [Header("Bullet Settings")]
    [Tooltip("Speed of the bullet in meters per second")]
    public float speed = 20f;
    
    [Tooltip("How long the bullet exists before auto-destroying")]
    public float lifetime = 5f;
    
    [Tooltip("Effect to spawn when bullet hits something")]
    public GameObject hitEffectPrefab;
    
    [Tooltip("Effect to spawn when bullet is destroyed (missed)")]
    public GameObject missEffectPrefab;
    
    // Private variables
    private Vector3 direction;
    private float damage;
    private LayerMask layerMask;
    private float distanceTraveled = 0f;
    private float maxDistance;
    private Vector3 startPosition;
    
    void Start()
    {
        startPosition = transform.position;
        
        // Auto-destroy after lifetime
        Destroy(gameObject, lifetime);
    }
    
    void Update()
    {
        MoveBullet();
        CheckForHits();
    }
    
    public void Initialize(Vector3 shootDirection, float bulletDamage, LayerMask shootLayerMask, float shootMaxDistance)
    {
        direction = shootDirection.normalized;
        damage = bulletDamage;
        layerMask = shootLayerMask;
        maxDistance = shootMaxDistance;
        
        // Orient the bullet to face the direction it's traveling
        if (direction != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(direction);
        }
    }
    
    void MoveBullet()
    {
        // Move the bullet forward
        float moveDistance = speed * Time.deltaTime;
        transform.Translate(Vector3.forward * moveDistance);
        
        distanceTraveled += moveDistance;
        
        // Destroy if traveled too far
        if (distanceTraveled >= maxDistance)
        {
            OnMiss();
        }
    }
    
    void CheckForHits()
    {
        // Cast a ray from the bullet's position in the direction it's moving
        float checkDistance = speed * Time.deltaTime * 1.5f; // Check a bit ahead
        
        RaycastHit[] hits = Physics.RaycastAll(transform.position, direction, checkDistance, layerMask);
        
        // Sort hits by distance
        System.Array.Sort(hits, (hit1, hit2) => hit1.distance.CompareTo(hit2.distance));
        
        foreach (RaycastHit hit in hits)
        {
            // Skip ARPlanes
            if (hit.collider.name.Contains("ARPlane"))
                continue;
                
            // We hit something valid
            OnHit(hit);
            return;
        }
    }
    
    void OnHit(RaycastHit hit)
    {
        Debug.Log($"Bullet: Hit {hit.collider.name} at distance {hit.distance:F2}m");
        
        // Spawn hit effect
        if (hitEffectPrefab != null)
        {
            Instantiate(hitEffectPrefab, hit.point, Quaternion.LookRotation(hit.normal));
        }
        
        // Check if we hit a zombie
        if (hit.collider.CompareTag("Zombie"))
        {
            Debug.Log("Bullet: Hit a zombie!");
            
            // Apply damage to the zombie's health component
            ZombieHealth health = hit.collider.GetComponentInParent<ZombieHealth>();
            if (health != null)
            {
                Debug.Log($"Bullet: Applying {damage} damage to zombie. Health before: {health.currentHealth}/{health.maxHealth}");
                health.TakeDamage((int)damage);
                Debug.Log($"Bullet: Zombie health after damage: {health.currentHealth}/{health.maxHealth}");
            }
            else
            {
                // Fallback - try to find health component in children
                health = hit.collider.GetComponentInChildren<ZombieHealth>();
                if (health != null)
                {
                    health.TakeDamage((int)damage);
                    Debug.Log($"Bullet: Applied {damage} damage to zombie (found in children). Remaining health: {health.currentHealth}");
                }
                else
                {
                    Debug.LogWarning("Bullet: Hit zombie but couldn't find ZombieHealth component!");
                }
            }
        }
        
        // Destroy the bullet
        Destroy(gameObject);
    }
    
    void OnMiss()
    {
        Debug.Log("Bullet: Missed target (max distance reached)");
        
        // Spawn miss effect
        if (missEffectPrefab != null)
        {
            Instantiate(missEffectPrefab, transform.position, transform.rotation);
        }
        
        // Destroy the bullet
        Destroy(gameObject);
    }
    
    // Optional: Add a trail renderer or particle effect to make the bullet more visible
    void OnDrawGizmos()
    {
        // Draw the bullet's path for debugging
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 0.05f);
        
        if (direction != Vector3.zero)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawRay(transform.position, direction * 2f);
        }
    }
}
