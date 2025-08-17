using System.Collections;
using UnityEngine;

public class ZombieAttack : MonoBehaviour
{
    [Header("Attack Settings")]
    [Tooltip("Damage dealt per attack")]
    public int attackDamage = 25;
    
    [Tooltip("Time between attacks in seconds")]
    public float attackCooldown = 1.5f;
    
    [Tooltip("Maximum range for attacks")]
    public float attackRange = 1.5f;
    
    [Tooltip("Duration of attack animation")]
    public float attackAnimationDuration = 1f;
    
    [Tooltip("When in the animation to deal damage (0-1)")]
    [Range(0f, 1f)]
    public float damagePoint = 0.6f;
    
    [Header("Effects")]
    [Tooltip("Effect to spawn when attack hits")]
    public GameObject attackHitEffect;
    
    [Tooltip("Sound to play when attacking")]
    public AudioClip attackSound;
    
    [Tooltip("Sound to play when attack hits")]
    public AudioClip hitSound;
    
    // State tracking
    private bool canAttack = true;
    private bool isAttacking = false;
    private float lastAttackTime;
    private AudioSource audioSource;
    private Animator animator;
    
    // Target tracking
    private Transform currentTarget;
    private bool hasDeltDamage = false;
    
    void Start()
    {
        // Get components
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        animator = GetComponent<Animator>();
        if (animator == null)
        {
            Debug.LogWarning("ZombieAttack: No Animator found. Attack animations will not play.");
        }
    }
    
    void Update()
    {
        // Update attack cooldown
        if (!canAttack && Time.time - lastAttackTime >= attackCooldown)
        {
            canAttack = true;
            Debug.Log("ZombieAttack: Attack cooldown complete, can attack again");
        }
        
        // Reset attack state when animation ends
        if (isAttacking && Time.time - lastAttackTime >= attackAnimationDuration)
        {
            EndAttack();
        }
    }
    
    /// <summary>
    /// Attempt to attack the target if in range and ready
    /// </summary>
    public bool TryAttack(Transform target)
    {
        if (!canAttack || isAttacking || target == null)
            return false;
            
        float distanceToTarget = Vector3.Distance(transform.position, target.position);
        if (distanceToTarget > attackRange)
            return false;
            
        StartAttack(target);
        return true;
    }
    
    void StartAttack(Transform target)
    {
        Debug.Log($"ZombieAttack: Starting attack on {target.name}");
        
        currentTarget = target;
        isAttacking = true;
        canAttack = false;
        hasDeltDamage = false;
        lastAttackTime = Time.time;
        
        // Play attack sound
        if (audioSource != null && attackSound != null)
        {
            audioSource.PlayOneShot(attackSound);
        }
        
        // Trigger attack animation
        if (animator != null)
        {
            animator.SetBool("IsAttacking", true);
            animator.SetTrigger("Attack");
        }
        
        // Schedule damage dealing
        StartCoroutine(DealDamageAtPoint());
    }
    
    IEnumerator DealDamageAtPoint()
    {
        // Wait for the damage point in the animation
        float damageTime = attackAnimationDuration * damagePoint;
        yield return new WaitForSeconds(damageTime);
        
        // Deal damage if we haven't already and target is still valid
        if (!hasDeltDamage && currentTarget != null)
        {
            DealDamage();
        }
    }
    
    void DealDamage()
    {
        if (hasDeltDamage || currentTarget == null) return;
        
        // Check if target is still in range
        float distanceToTarget = Vector3.Distance(transform.position, currentTarget.position);
        if (distanceToTarget > attackRange)
        {
            Debug.Log("ZombieAttack: Target moved out of range, attack missed");
            return;
        }
        
        hasDeltDamage = true;
        
        Debug.Log($"ZombieAttack: Dealing {attackDamage} damage to {currentTarget.name}");
        
        // Try to damage the player (assuming it's the AR camera/player controller)
        PlayerHealth playerHealth = currentTarget.GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(attackDamage);
            OnSuccessfulHit();
        }
        else
        {
            // Try to find PlayerHealth in parent or children
            playerHealth = currentTarget.GetComponentInParent<PlayerHealth>();
            if (playerHealth == null)
                playerHealth = currentTarget.GetComponentInChildren<PlayerHealth>();
                
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(attackDamage);
                OnSuccessfulHit();
            }
            else
            {
                Debug.LogWarning($"ZombieAttack: Could not find PlayerHealth component on {currentTarget.name}");
                // For testing, just log the damage
                Debug.Log($"ZombieAttack: Would deal {attackDamage} damage to player");
                OnSuccessfulHit();
            }
        }
    }
    
    void OnSuccessfulHit()
    {
        Debug.Log("ZombieAttack: Successfully hit player!");
        
        // Play hit sound
        if (audioSource != null && hitSound != null)
        {
            audioSource.PlayOneShot(hitSound);
        }
        
        // Spawn hit effect
        if (attackHitEffect != null && currentTarget != null)
        {
            Vector3 effectPosition = currentTarget.position + Vector3.up * 0.5f;
            Instantiate(attackHitEffect, effectPosition, Quaternion.identity);
        }
        
        // Add screen shake or other player feedback here
        AddPlayerFeedback();
    }
    
    void AddPlayerFeedback()
    {
        // Here you could add:
        // - Screen shake
        // - Red damage overlay
        // - Controller vibration
        // - Sound effects
        
        // For now, just log
        Debug.Log("ZombieAttack: Player hit feedback triggered");
    }
    
    void EndAttack()
    {
        if (!isAttacking) return;
        
        Debug.Log("ZombieAttack: Attack sequence complete");
        
        isAttacking = false;
        currentTarget = null;
        
        // Reset attack animation
        if (animator != null)
        {
            animator.SetBool("IsAttacking", false);
        }
    }
    
    /// <summary>
    /// Force stop the current attack (useful when zombie is hit or dies)
    /// </summary>
    public void StopAttack()
    {
        if (isAttacking)
        {
            StopAllCoroutines();
            EndAttack();
            Debug.Log("ZombieAttack: Attack forcibly stopped");
        }
    }
    
    /// <summary>
    /// Check if zombie is currently attacking
    /// </summary>
    public bool IsAttacking()
    {
        return isAttacking;
    }
    
    /// <summary>
    /// Check if zombie can attack right now
    /// </summary>
    public bool CanAttack()
    {
        return canAttack && !isAttacking;
    }
    
    /// <summary>
    /// Get remaining cooldown time
    /// </summary>
    public float GetRemainingCooldown()
    {
        if (canAttack) return 0f;
        return Mathf.Max(0f, attackCooldown - (Time.time - lastAttackTime));
    }
    
    // Called when zombie takes damage or dies
    public void OnZombieHit()
    {
        // Interrupt attack if hit
        if (isAttacking)
        {
            StopAttack();
        }
    }
    
    public void OnZombieDeath()
    {
        // Stop all attacks when zombie dies
        StopAttack();
        this.enabled = false;
    }
    
    // Debug visualization
    void OnDrawGizmosSelected()
    {
        // Draw attack range
        Gizmos.color = isAttacking ? Color.red : Color.orange;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        
        // Draw line to current target
        if (currentTarget != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position + Vector3.up, currentTarget.position + Vector3.up);
        }
    }
}
