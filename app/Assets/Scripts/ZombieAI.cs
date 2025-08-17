using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class ZombieAI : MonoBehaviour
{
    [Header("AI Behavior Settings")]
    [Tooltip("Detection range for finding the player")]
    public float detectionRange = 10f;
    
    [Tooltip("Attack range for melee combat")]
    public float attackRange = 1.5f;
    
    [Tooltip("Patrol radius around spawn point")]
    public float patrolRadius = 5f;
    
    [Tooltip("Time to wait at each patrol point")]
    public float patrolWaitTime = 3f;
    
    [Tooltip("Walking speed during patrol")]
    public float walkSpeed = 1.5f;
    
    [Tooltip("Running speed during chase")]
    public float runSpeed = 4f;
    
    [Header("Animation Settings")]
    [Tooltip("Time for scream animation before chasing")]
    public float screamDuration = 1.5f;
    
    // AI States
    public enum AIState
    {
        Patrol,     // Random wandering
        Alert,      // Player detected, screaming
        Chase,      // Pursuing player
        Attack,     // Close combat
        Hit,        // Reaction to damage
        Dead        // Death state
    }
    
    [Header("Debug")]
    [Tooltip("Show current AI state in inspector")]
    public AIState currentState = AIState.Patrol;
    
    // Components
    private NavMeshAgent navMeshAgent;
    private Animator animator;
    private ZombieVision vision;
    private ZombieAttack attack;
    private ZombieHealth health;
    private ZombieMovement movement;
    
    // State tracking
    private Vector3 spawnPoint;
    private Vector3 currentPatrolTarget;
    private float lastStateChangeTime;
    private float lastNavUpdateTime = 0f; // Timer for NavMesh updates
    private bool hasTarget = false;
    private Transform playerTarget;
    private bool lastCanSeePlayer = false; // For vision debugging
    
    // Coroutines
    private Coroutine currentBehaviorCoroutine;
    
    void Start()
    {
        InitializeComponents();
        InitializeAI();
    }
    
    void InitializeComponents()
    {
        // Get required components
        navMeshAgent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        vision = GetComponent<ZombieVision>();
        attack = GetComponent<ZombieAttack>();
        health = GetComponent<ZombieHealth>();
        movement = GetComponent<ZombieMovement>();
        
        // Add missing components if needed
        if (navMeshAgent == null)
        {
            navMeshAgent = gameObject.AddComponent<NavMeshAgent>();
            Debug.LogWarning("ZombieAI: Added missing NavMeshAgent component");
        }
        
        if (vision == null)
        {
            vision = gameObject.AddComponent<ZombieVision>();
            Debug.Log("ZombieAI: Added ZombieVision component");
        }
        
        if (attack == null)
        {
            attack = gameObject.AddComponent<ZombieAttack>();
            Debug.Log("ZombieAI: Added ZombieAttack component");
        }
        
        // Configure NavMeshAgent
        navMeshAgent.speed = walkSpeed;
        navMeshAgent.stoppingDistance = attackRange;
        navMeshAgent.autoBraking = true;
        navMeshAgent.updateRotation = true;
        navMeshAgent.updatePosition = true;
        navMeshAgent.angularSpeed = 120f; // Reasonable turn speed
        navMeshAgent.acceleration = 8f; // Quick acceleration
        
        // Try to place on NavMesh if not already
        if (!navMeshAgent.isOnNavMesh)
        {
            NavMeshHit navHit;
            if (NavMesh.SamplePosition(transform.position, out navHit, 2.0f, NavMesh.AllAreas))
            {
                navMeshAgent.Warp(navHit.position);
                Debug.Log($"ZombieAI: Initially placed on NavMesh at {navHit.position}");
            }
            else
            {
                Debug.LogError($"ZombieAI: Failed to place on NavMesh at start! Position: {transform.position}");
            }
        }
    }
    
    void InitializeAI()
    {
        // Set spawn point
        spawnPoint = transform.position;
        
        // Find player target (AR Camera)
        FindPlayerTarget();
        
        // Start with patrol behavior
        ChangeState(AIState.Patrol);
        
        // Subscribe to vision events
        if (vision != null)
        {
            vision.OnPlayerDetected += HandlePlayerDetected;
            vision.OnPlayerLost += HandlePlayerLost;
        }
        
        // Subscribe to health events
        if (health != null)
        {
            health.OnDamageTaken += HandleDamageTaken;
            health.OnDeath += HandleDeath;
        }
        
        Debug.Log($"ZombieAI: Initialized at {spawnPoint} with target: {(playerTarget != null ? playerTarget.name : "None")}");
    }
    
    void FindPlayerTarget()
    {
        // Look for AR Camera (Main Camera)
        Camera mainCamera = Camera.main;
        if (mainCamera != null)
        {
            playerTarget = mainCamera.transform;
            Debug.Log($"ZombieAI: Found player target: {playerTarget.name}");
            return;
        }
        
        // Fallback: Look for camera tagged as MainCamera
        GameObject cameraObj = GameObject.FindWithTag("MainCamera");
        if (cameraObj != null)
        {
            playerTarget = cameraObj.transform;
            Debug.Log($"ZombieAI: Found player target by tag: {playerTarget.name}");
            return;
        }
        
        Debug.LogWarning("ZombieAI: Could not find player target (Main Camera)");
    }
    
    void Update()
    {
        // Don't process AI if dead
        if (currentState == AIState.Dead || health != null && health.isDead)
            return;
            
        // TEMPORARY FIX: If zombie is stuck in patrol with scream animation, force alert
        if (currentState == AIState.Patrol && animator != null)
        {
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            if (stateInfo.IsName("Scream") || stateInfo.IsName("Zombie Scream"))
            {
                Debug.LogError("ZombieAI: STUCK IN PATROL WITH SCREAM! Forcing Alert state");
                ChangeState(AIState.Alert);
            }
        }
            
        // Update vision system
        if (vision != null && playerTarget != null)
        {
            vision.UpdateVision(playerTarget.position);
            
            // Debug: Log vision state changes
            if (vision.CanSeePlayer != lastCanSeePlayer)
            {
                Debug.Log($"ZombieAI: Vision changed - CanSeePlayer: {vision.CanSeePlayer}, Distance: {Vector3.Distance(transform.position, playerTarget.position):F1}m");
                lastCanSeePlayer = vision.CanSeePlayer;
            }
        }
        else if (vision == null)
        {
            Debug.LogError("ZombieAI: Vision component is null!");
        }
        else if (playerTarget == null)
        {
            Debug.LogWarning("ZombieAI: Player target is null! Attempting to find camera...");
            FindPlayerTarget();
        }
        
        // Handle state-specific updates
        UpdateCurrentState();
        
        // Update animator parameters
        UpdateAnimations();
    }
    
    void UpdateCurrentState()
    {
        switch (currentState)
        {
            case AIState.Patrol:
                UpdatePatrolState();
                break;
            case AIState.Alert:
                UpdateAlertState();
                break;
            case AIState.Chase:
                UpdateChaseState();
                break;
            case AIState.Attack:
                UpdateAttackState();
                break;
            case AIState.Hit:
                UpdateHitState();
                break;
        }
    }
    
    void UpdatePatrolState()
    {
        // Check if agent is on NavMesh
        if (!navMeshAgent.isOnNavMesh)
        {
            Debug.LogWarning("ZombieAI: Not on NavMesh in patrol state!");
            return;
        }
        
        // Check if we have a valid path or if path failed
        if (!navMeshAgent.hasPath && !navMeshAgent.pathPending)
        {
            // No path, set a new patrol target
            SetNewPatrolTarget();
            return;
        }
        
        // Check if we've reached our patrol destination
        if (!navMeshAgent.pathPending && navMeshAgent.remainingDistance < 0.5f)
        {
            // We've reached the destination, wait or find new target
            if (currentBehaviorCoroutine == null)
            {
                currentBehaviorCoroutine = StartCoroutine(PatrolWaitCoroutine());
            }
        }
    }
    
    void UpdateAlertState()
    {
        // Face the player during alert
        if (playerTarget != null)
        {
            Vector3 lookDirection = (playerTarget.position - transform.position).normalized;
            lookDirection.y = 0; // Keep level
            if (lookDirection != Vector3.zero)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(lookDirection), Time.deltaTime * 2f);
            }
        }
        
        // Automatically transition to chase after scream duration
        if (Time.time - lastStateChangeTime >= screamDuration)
        {
            Debug.Log("ZombieAI: Alert state timeout, transitioning to Chase");
            ChangeState(AIState.Chase);
        }
        
        // Additional failsafe: if we've been in alert too long, force transition
        if (Time.time - lastStateChangeTime >= screamDuration * 2f)
        {
            Debug.LogWarning("ZombieAI: Alert state STUCK! Emergency transition to Chase");
            // Force animator state
            if (animator != null)
            {
                animator.SetBool("IsDetecting", false);
                animator.SetBool("IsChasing", true);
            }
            ChangeState(AIState.Chase);
        }
    }
    
    void UpdateChaseState()
    {
        if (playerTarget == null) return;
        
        float distanceToPlayer = Vector3.Distance(transform.position, playerTarget.position);
        
        // Debug NavMesh status
        if (Time.frameCount % 30 == 0) // Every half second
        {
            Debug.Log($"ZombieAI Chase Debug - Distance: {distanceToPlayer:F1}m, " +
                     $"HasPath: {navMeshAgent.hasPath}, PathPending: {navMeshAgent.pathPending}, " +
                     $"PathStatus: {navMeshAgent.pathStatus}, Velocity: {navMeshAgent.velocity.magnitude:F1}m/s, " +
                     $"IsOnNavMesh: {navMeshAgent.isOnNavMesh}");
        }
        
        // If no valid path after some time, use direct movement
        if (!navMeshAgent.hasPath && !navMeshAgent.pathPending && Time.time - lastNavUpdateTime > 0.5f)
        {
            // Direct movement toward player
            Vector3 direction = (playerTarget.position - transform.position).normalized;
            direction.y = 0; // Keep on horizontal plane
            
            // Move using NavMeshAgent.Move
            navMeshAgent.Move(direction * runSpeed * Time.deltaTime);
            
            // Rotate toward player
            if (direction != Vector3.zero)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), Time.deltaTime * 5f);
            }
            
            if (Time.frameCount % 60 == 0)
            {
                Debug.Log("ZombieAI: Using direct movement fallback due to invalid path");
            }
        }
        
        // Check if close enough to attack
        if (distanceToPlayer <= attackRange)
        {
            ChangeState(AIState.Attack);
            return;
        }
        
        // Update chase target every 0.2 seconds for performance
        // Use a separate timer to avoid messing with state change time
        if (Time.time - lastNavUpdateTime > 0.2f)
        {
            // Only update destination if player has moved significantly
            Vector3 currentDestination = navMeshAgent.destination;
            Vector3 targetPosition = playerTarget.position;
            
            // Project the player position onto the NavMesh
            NavMeshHit navHit;
            if (NavMesh.SamplePosition(targetPosition, out navHit, 5.0f, NavMesh.AllAreas))
            {
                targetPosition = navHit.position;
            }
            else
            {
                // If can't find NavMesh near player, try projecting down
                Vector3 downPosition = targetPosition;
                downPosition.y -= 10f; // Look far down
                if (NavMesh.SamplePosition(downPosition, out navHit, 15.0f, NavMesh.AllAreas))
                {
                    targetPosition = navHit.position;
                    Debug.Log($"ZombieAI: Found NavMesh below player at {targetPosition}");
                }
                else
                {
                    Debug.LogWarning($"ZombieAI: Cannot find NavMesh near player position {playerTarget.position}!");
                }
            }
            
            float destDist = Vector3.Distance(currentDestination, targetPosition);
            
            if (destDist > 0.5f) // Player moved more than 0.5m
            {
                navMeshAgent.SetDestination(targetPosition);
                Debug.Log($"ZombieAI: Updating chase target to {targetPosition}, distance: {distanceToPlayer:F1}m");
                
                // Debug path status
                if (!navMeshAgent.hasPath && !navMeshAgent.pathPending)
                {
                    Debug.LogWarning($"ZombieAI: Failed to create path! Status: {navMeshAgent.pathStatus}");
                    
                    // Try fallback movement
                    if (movement != null)
                    {
                        Vector3 direction = (targetPosition - transform.position).normalized;
                        navMeshAgent.Move(direction * runSpeed * Time.deltaTime);
                        Debug.Log("ZombieAI: Using fallback movement");
                    }
                }
            }
            lastNavUpdateTime = Time.time;
        }
    }
    
    void UpdateAttackState()
    {
        if (playerTarget == null) return;
        
        float distanceToPlayer = Vector3.Distance(transform.position, playerTarget.position);
        
        // If player moved away, go back to chasing
        if (distanceToPlayer > attackRange * 1.5f)
        {
            ChangeState(AIState.Chase);
            return;
        }
        
        // Face the player
        Vector3 lookDirection = (playerTarget.position - transform.position).normalized;
        lookDirection.y = 0;
        if (lookDirection != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(lookDirection);
        }
        
        // Let ZombieAttack component handle the actual attacking
        if (attack != null)
        {
            attack.TryAttack(playerTarget);
        }
    }
    
    void UpdateHitState()
    {
        // Hit state is temporary, automatically return to previous behavior
        if (Time.time - lastStateChangeTime > 0.5f)
        {
            // Return to appropriate state based on player visibility
            if (vision != null && vision.CanSeePlayer)
            {
                ChangeState(AIState.Chase);
            }
            else
            {
                ChangeState(AIState.Patrol);
            }
        }
    }
    
    public void ChangeState(AIState newState)
    {
        if (currentState == newState) return;
        
        Debug.Log($"ZombieAI: Changing state from {currentState} to {newState}");
        
        // Exit current state
        ExitState(currentState);
        
        // Update state
        AIState previousState = currentState;
        currentState = newState;
        lastStateChangeTime = Time.time;
        
        // Enter new state
        EnterState(newState, previousState);
    }
    
    void ExitState(AIState state)
    {
        // Stop any running coroutines
        if (currentBehaviorCoroutine != null)
        {
            StopCoroutine(currentBehaviorCoroutine);
            currentBehaviorCoroutine = null;
        }
        
        switch (state)
        {
            case AIState.Patrol:
                break;
            case AIState.Alert:
                break;
            case AIState.Chase:
                break;
            case AIState.Attack:
                break;
        }
    }
    
    void EnterState(AIState state, AIState previousState)
    {
        switch (state)
        {
            case AIState.Patrol:
                EnterPatrolState();
                break;
            case AIState.Alert:
                EnterAlertState();
                break;
            case AIState.Chase:
                EnterChaseState();
                break;
            case AIState.Attack:
                EnterAttackState();
                break;
            case AIState.Hit:
                EnterHitState();
                break;
            case AIState.Dead:
                EnterDeadState();
                break;
        }
    }
    
    void EnterPatrolState()
    {
        navMeshAgent.speed = walkSpeed;
        navMeshAgent.stoppingDistance = 0.5f;
        SetNewPatrolTarget();
    }
    
    void EnterAlertState()
    {
        // Stop moving and face player
        navMeshAgent.ResetPath();
        navMeshAgent.speed = 0;
        
        Debug.Log($"ZombieAI: Entering Alert state, will scream for {screamDuration}s");
        
        // Add fallback timeout in case animation gets stuck
        if (currentBehaviorCoroutine != null)
        {
            StopCoroutine(currentBehaviorCoroutine);
        }
        currentBehaviorCoroutine = StartCoroutine(AlertTimeoutCoroutine());
    }
    
    void EnterChaseState()
    {
        navMeshAgent.speed = runSpeed;
        navMeshAgent.stoppingDistance = attackRange;
        navMeshAgent.updateRotation = true; // Ensure agent controls rotation
        navMeshAgent.isStopped = false; // Make sure agent isn't stopped
        
        if (playerTarget != null)
        {
            // Project player position onto NavMesh
            Vector3 targetPosition = playerTarget.position;
            NavMeshHit navHit;
            if (NavMesh.SamplePosition(targetPosition, out navHit, 5.0f, NavMesh.AllAreas))
            {
                targetPosition = navHit.position;
            }
            else
            {
                // Try projecting down
                Vector3 downPosition = targetPosition;
                downPosition.y -= 10f;
                if (NavMesh.SamplePosition(downPosition, out navHit, 15.0f, NavMesh.AllAreas))
                {
                    targetPosition = navHit.position;
                }
            }
            
            navMeshAgent.SetDestination(targetPosition);
            Debug.Log($"ZombieAI: Entering Chase state - Target: {targetPosition}, Agent enabled: {navMeshAgent.enabled}, IsOnNavMesh: {navMeshAgent.isOnNavMesh}");
        }
        else
        {
            Debug.LogWarning("ZombieAI: Entering Chase state but no player target!");
        }
    }
    
    void EnterAttackState()
    {
        navMeshAgent.ResetPath();
        navMeshAgent.speed = 0;
    }
    
    void EnterHitState()
    {
        navMeshAgent.ResetPath();
    }
    
    void EnterDeadState()
    {
        navMeshAgent.enabled = false;
        this.enabled = false; // Disable this AI script
    }
    
    void SetNewPatrolTarget()
    {
        if (!navMeshAgent.isOnNavMesh)
        {
            Debug.LogWarning("ZombieAI: Not on NavMesh, cannot set patrol target");
            return;
        }
        // Generate random patrol point within radius
        Vector3 randomDirection = Random.insideUnitSphere * patrolRadius;
        randomDirection += spawnPoint;
        randomDirection.y = spawnPoint.y; // Keep at same height
        
        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomDirection, out hit, patrolRadius, NavMesh.AllAreas))
        {
            currentPatrolTarget = hit.position;
            navMeshAgent.SetDestination(currentPatrolTarget);
            Debug.Log($"ZombieAI: New patrol target set to {currentPatrolTarget}");
        }
        else
        {
            // Fallback to spawn point if no valid position found
            currentPatrolTarget = spawnPoint;
            navMeshAgent.SetDestination(currentPatrolTarget);
            Debug.LogWarning("ZombieAI: Could not find valid patrol point, returning to spawn");
        }
    }
    
    IEnumerator PatrolWaitCoroutine()
    {
        Debug.Log("ZombieAI: Waiting at patrol point");
        yield return new WaitForSeconds(patrolWaitTime);
        SetNewPatrolTarget();
        currentBehaviorCoroutine = null;
    }
    
    IEnumerator AlertTimeoutCoroutine()
    {
        Debug.Log($"ZombieAI: Alert timeout coroutine started, waiting {screamDuration * 1.5f}s");
        yield return new WaitForSeconds(screamDuration * 1.5f); // 50% longer than normal scream
        
        // Force transition to chase if still in alert state
        if (currentState == AIState.Alert)
        {
            Debug.LogWarning("ZombieAI: Alert state timeout! Forcing transition to Chase");
            ChangeState(AIState.Chase);
        }
        currentBehaviorCoroutine = null;
    }
    
    void UpdateAnimations()
    {
        if (animator == null) return;
        
        // Set AI state parameter
        animator.SetInteger("AIState", (int)currentState);
        
        // Get current animation state info for debugging
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        
        // Check for animation state mismatch
        if (currentState == AIState.Patrol && (stateInfo.IsName("Scream") || stateInfo.IsName("Zombie Scream")))
        {
            Debug.LogError($"ZombieAI: ANIMATION MISMATCH! State is {currentState} but playing Scream animation!");
            // Force animator back to idle
            animator.Play("Idle", 0, 0f);
        }
        
        // PURE BOOL-BASED ANIMATION CONTROL - No Speed dependency!
        // Clear all states first
        animator.SetBool("IsDetecting", false);
        animator.SetBool("IsChasing", false);
        animator.SetBool("IsAttacking", false);
        
        // Set the appropriate state
        switch (currentState)
        {
            case AIState.Patrol:
                // All bools are false - will use Idle/Walk based on Speed (which is fine for patrol)
                break;
            case AIState.Alert:
                animator.SetBool("IsDetecting", true);
                // NOTE: We don't set IsChasing here initially - let the scream play first
                
                // Debug: Check if we're in scream state and log progress
                if (stateInfo.IsName("Scream") || stateInfo.IsName("Zombie Scream"))
                {
                    Debug.Log($"ZombieAI: In Scream animation - Progress: {stateInfo.normalizedTime:F2}, Loop: {stateInfo.loop}");
                    
                    // After scream starts, enable chasing to allow transition
                    if (stateInfo.normalizedTime > 0.1f) // 10% into scream animation
                    {
                        animator.SetBool("IsChasing", true);
                    }
                    
                    // Force transition if animation is stuck or looping
                    if (stateInfo.normalizedTime > 0.9f || stateInfo.loop) // 90% through or looping
                    {
                        Debug.LogWarning("ZombieAI: Scream animation complete/looping! Forcing chase state");
                        ChangeState(AIState.Chase);
                    }
                }
                else
                {
                    // If we're in Alert state but not playing scream, something's wrong
                    Debug.LogWarning($"ZombieAI: In Alert state but not playing scream animation (hash: {stateInfo.shortNameHash}), forcing chase");
                    ChangeState(AIState.Chase);
                }
                break;
            case AIState.Chase:
                animator.SetBool("IsChasing", true);
                break;
            case AIState.Attack:
                animator.SetBool("IsAttacking", true);
                break;
        }
        
        // Set speed based on actual movement
        float actualSpeed = navMeshAgent.velocity.magnitude;
        
        // For chase state, ensure we show running animation even if using fallback movement
        if (currentState == AIState.Chase && actualSpeed < 0.1f)
        {
            // If we're supposed to be chasing but not moving, force run animation
            actualSpeed = runSpeed * 0.8f; // Show run animation
        }
        
        animator.SetFloat("Speed", actualSpeed);
        
        if (movement != null)
        {
            movement.SetAnimationSpeed(actualSpeed);
        }
        
        // Debug animation state
        if (Time.frameCount % 60 == 0) // Every 60 frames (about once per second)
        {
            float currentSpeed = animator.GetFloat("Speed");
            Debug.Log($"ZombieAI: State={currentState}, AnimState={stateInfo.shortNameHash}, Speed={currentSpeed:F2}, IsChasing={animator.GetBool("IsChasing")}, IsDetecting={animator.GetBool("IsDetecting")}");
        }
    }
    
    // Event Handlers
    void HandlePlayerDetected()
    {
        Debug.Log($"ZombieAI: Player detected! Current state: {currentState}");
        if (currentState == AIState.Patrol)
        {
            Debug.Log("ZombieAI: Transitioning from Patrol to Alert");
            ChangeState(AIState.Alert);
        }
        else
        {
            Debug.Log($"ZombieAI: Already in state {currentState}, not transitioning to Alert");
        }
    }
    
    void HandlePlayerLost()
    {
        Debug.Log($"ZombieAI: Player lost from state {currentState}");
        if (currentState == AIState.Chase || currentState == AIState.Attack || currentState == AIState.Alert)
        {
            ChangeState(AIState.Patrol);
        }
    }
    
    void HandleDamageTaken()
    {
        Debug.Log("ZombieAI: Took damage, entering hit state");
        if (currentState != AIState.Dead)
        {
            ChangeState(AIState.Hit);
        }
    }
    
    void HandleDeath()
    {
        Debug.Log("ZombieAI: Zombie died");
        ChangeState(AIState.Dead);
    }
    
    // Public method to manually trigger player detection (for debugging)
    public void ForceDetectPlayer()
    {
        Debug.Log("ZombieAI: Manually forcing player detection");
        if (playerTarget == null)
        {
            FindPlayerTarget();
        }
        
        if (currentState == AIState.Patrol)
        {
            ChangeState(AIState.Alert);
        }
        else if (currentState == AIState.Alert)
        {
            ChangeState(AIState.Chase);
        }
    }
    
    // Public method to fix stuck zombies
    public void FixStuckZombie()
    {
        Debug.Log("ZombieAI: Attempting to fix stuck zombie");
        
        // Try to place on NavMesh
        if (navMeshAgent != null)
        {
            NavMeshHit navHit;
            if (NavMesh.SamplePosition(transform.position, out navHit, 3.0f, NavMesh.AllAreas))
            {
                navMeshAgent.Warp(navHit.position);
                Debug.Log($"ZombieAI: Warped to NavMesh position {navHit.position}, IsOnNavMesh: {navMeshAgent.isOnNavMesh}");
                
                // Reset state to patrol
                ChangeState(AIState.Patrol);
            }
            else
            {
                Debug.LogError("ZombieAI: Failed to find NavMesh near zombie!");
            }
        }
    }
    
    // Debug visualization
    void OnDrawGizmos()
    {
        if (navMeshAgent != null && navMeshAgent.enabled && navMeshAgent.hasPath)
        {
            // Draw the path
            Gizmos.color = Color.yellow;
            var path = navMeshAgent.path;
            for (int i = 0; i < path.corners.Length - 1; i++)
            {
                Gizmos.DrawLine(path.corners[i], path.corners[i + 1]);
            }
            
            // Draw destination
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(navMeshAgent.destination, 0.2f);
        }
        
        // Draw detection range
        if (currentState == AIState.Patrol)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, detectionRange);
        }
    }
    
    void OnDestroy()
    {
        // Unsubscribe from events
        if (vision != null)
        {
            vision.OnPlayerDetected -= HandlePlayerDetected;
            vision.OnPlayerLost -= HandlePlayerLost;
        }
        
        if (health != null)
        {
            health.OnDamageTaken -= HandleDamageTaken;
            health.OnDeath -= HandleDeath;
        }
    }
    
    // Debug visualization
    void OnDrawGizmosSelected()
    {
        // Draw detection range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        
        // Draw attack range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        
        // Draw patrol radius
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(spawnPoint, patrolRadius);
        
        // Draw current patrol target
        if (currentPatrolTarget != Vector3.zero)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(currentPatrolTarget, 0.3f);
            Gizmos.DrawLine(transform.position, currentPatrolTarget);
        }
    }
}
