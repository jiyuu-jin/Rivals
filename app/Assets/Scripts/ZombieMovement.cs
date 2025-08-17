using UnityEngine;

public class ZombieMovement : MonoBehaviour
{
    [Header("Animation Settings")]
    [Tooltip("Multiplier for speed calculations")]
    public float speedMultiplier = 1f;
    
    [Tooltip("Speed threshold for walking animation")]
    public float walkThreshold = 0.1f;
    
    [Tooltip("Speed threshold for running animation")]
    public float runThreshold = 2f;
    
    [Tooltip("How often to update movement speed (seconds)")]
    public float updateInterval = 0.1f;
    
    // Private variables
    private Animator animator;
    private Vector3 lastPosition;
    private float lastUpdateTime;
    private float currentSpeed;
    
    void Start()
    {
        // Get components
        animator = GetComponent<Animator>();
        if (animator == null)
        {
            Debug.LogWarning($"ZombieMovement: No Animator found on {gameObject.name}. Movement animations will not work.");
        }
        
        // Initialize tracking
        lastPosition = transform.position;
        lastUpdateTime = Time.time;
        currentSpeed = 0f;
    }
    
    void Update()
    {
        // Check if ZombieAI is present and controlling animations
        ZombieAI zombieAI = GetComponent<ZombieAI>();
        if (zombieAI != null)
        {
            // Let ZombieAI handle animation parameters
            return;
        }
        
        // Only update at specified intervals for performance
        if (Time.time - lastUpdateTime >= updateInterval)
        {
            UpdateMovementAnimation();
            lastUpdateTime = Time.time;
        }
    }
    
    void UpdateMovementAnimation()
    {
        if (animator == null) return;
        
        // Calculate movement speed
        float distance = Vector3.Distance(transform.position, lastPosition);
        float deltaTime = Time.time - (lastUpdateTime - updateInterval);
        currentSpeed = (distance / deltaTime) * speedMultiplier;
        
        // Update last position
        lastPosition = transform.position;
        
        // Set animator speed parameter
        animator.SetFloat("Speed", currentSpeed);
        
        // Debug logging (can be removed in final version)
        if (currentSpeed > 0.01f) // Only log when moving
        {
            Debug.Log($"ZombieMovement: Speed = {currentSpeed:F2}, Distance = {distance:F2}, DeltaTime = {deltaTime:F2}");
        }
    }
    
    /// <summary>
    /// Manually set the animation speed (useful for AI or scripted movement)
    /// </summary>
    public void SetAnimationSpeed(float speed)
    {
        currentSpeed = speed;
        if (animator != null)
        {
            animator.SetFloat("Speed", currentSpeed);
        }
    }
    
    /// <summary>
    /// Get the current calculated speed
    /// </summary>
    public float GetCurrentSpeed()
    {
        return currentSpeed;
    }
    
    /// <summary>
    /// Trigger the attack animation
    /// </summary>
    public void TriggerAttack()
    {
        if (animator != null)
        {
            animator.SetBool("IsAttacking", true);
            StartCoroutine(ResetAttackTrigger());
        }
    }
    
    /// <summary>
    /// Stop all movement animations (useful when zombie dies)
    /// </summary>
    public void StopMovement()
    {
        if (animator != null)
        {
            animator.SetFloat("Speed", 0f);
            animator.SetBool("IsAttacking", false);
        }
    }
    
    System.Collections.IEnumerator ResetAttackTrigger()
    {
        // Wait for attack animation to finish (adjust time as needed)
        yield return new WaitForSeconds(1f);
        if (animator != null)
        {
            animator.SetBool("IsAttacking", false);
        }
    }
    
    void OnDisable()
    {
        // Stop movement when disabled
        StopMovement();
    }
}
