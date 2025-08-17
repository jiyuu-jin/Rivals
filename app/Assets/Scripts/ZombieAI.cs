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
    private bool hasTarget = false;
    private Transform playerTarget;
    
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
            
        // Update vision system
        if (vision != null && playerTarget != null)
        {
            vision.UpdateVision(playerTarget.position);
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
            ChangeState(AIState.Chase);
        }
    }
    
    void UpdateChaseState()
    {
        if (playerTarget == null) return;
        
        float distanceToPlayer = Vector3.Distance(transform.position, playerTarget.position);
        
        // Check if close enough to attack
        if (distanceToPlayer <= attackRange)
        {
            ChangeState(AIState.Attack);
            return;
        }
        
        // Update chase target every 0.2 seconds for performance
        if (Time.time - lastStateChangeTime > 0.2f)
        {
            navMeshAgent.SetDestination(playerTarget.position);
            lastStateChangeTime = Time.time;
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
    }
    
    void EnterChaseState()
    {
        navMeshAgent.speed = runSpeed;
        navMeshAgent.stoppingDistance = attackRange;
        
        if (playerTarget != null)
        {
            navMeshAgent.SetDestination(playerTarget.position);
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
    
    void UpdateAnimations()
    {
        if (animator == null) return;
        
        // Set AI state parameter
        animator.SetInteger("AIState", (int)currentState);
        
        // Set specific animation triggers
        switch (currentState)
        {
            case AIState.Patrol:
                animator.SetBool("IsDetecting", false);
                animator.SetBool("IsChasing", false);
                animator.SetBool("IsAttacking", false);
                break;
            case AIState.Alert:
                animator.SetBool("IsDetecting", true);
                animator.SetBool("IsChasing", false);
                animator.SetBool("IsAttacking", false);
                break;
            case AIState.Chase:
                animator.SetBool("IsDetecting", false);
                animator.SetBool("IsChasing", true);
                animator.SetBool("IsAttacking", false);
                break;
            case AIState.Attack:
                animator.SetBool("IsDetecting", false);
                animator.SetBool("IsChasing", false);
                animator.SetBool("IsAttacking", true);
                break;
        }
        
        // Movement speed for existing system
        if (movement != null)
        {
            float currentSpeed = navMeshAgent.velocity.magnitude;
            movement.SetAnimationSpeed(currentSpeed);
        }
    }
    
    // Event Handlers
    void HandlePlayerDetected()
    {
        Debug.Log("ZombieAI: Player detected!");
        if (currentState == AIState.Patrol)
        {
            ChangeState(AIState.Alert);
        }
    }
    
    void HandlePlayerLost()
    {
        Debug.Log("ZombieAI: Player lost");
        if (currentState == AIState.Chase || currentState == AIState.Attack)
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
