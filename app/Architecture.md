# Unity AR Zombie Shooter - Architecture Document

## 1. Core Systems

### 1.1 AR Foundation Integration
The application uses Unity's AR Foundation for AR functionality, with key components:
- **ARPlaneManager**: Detects and manages AR planes (floors, walls, etc.)
- **ARCameraManager**: Manages AR camera functionality
- **ARRaycastManager**: Handles raycasting against AR planes
- **ARFeatheredPlaneMeshVisualizerCompanion**: Visualizes detected AR planes with feathered edges
- **ARNavMeshBuilder**: Dynamically builds NavMesh surfaces from AR planes for AI navigation

### 1.2 Input System
- Uses the new Unity Input System package for mobile compatibility
- **Mouse and Touchscreen**: Cross-platform input handling for shooting
- **Pointer detection**: Screen tap/touch detection with UI overlap prevention
- **Event-driven input**: Decoupled input handling from game logic

### 1.3 XR Interaction Toolkit
- **ObjectSpawner**: Handles spawning objects in the AR environment (disabled by default)
- **ARGestureInteractor**: Manages AR gestures for object interaction
- **XR Origin**: Root of the AR camera rig hierarchy
- **ARInteractorSpawnTrigger**: Respects ObjectSpawner state for mine placement

## 2. Game Systems

### 2.1 Player Systems

#### 2.1.1 PlayerHealth
- **Purpose**: Manages player health, damage feedback, and death detection
- **Key Features**:
  - Visual damage feedback with screen flash overlay
  - Health bar UI display with proper mobile positioning
  - Death detection and death screen triggering
  - Respawn functionality with event system
  - Audio feedback for damage and death
- **Events**: OnHealthChanged, OnPlayerDeath, OnPlayerRespawn
- **Dependencies**: DeathScreen, GameScore

#### 2.1.2 CrosshairController
- **Purpose**: Creates and manages the on-screen crosshair for aiming
- **Key Features**:
  - Dynamically creates UI canvas with circular crosshair
  - Responsive positioning (center screen)
  - Color changes based on game mode (shooting vs mine placement)
  - Mobile-optimized sizing
- **Dependencies**: Unity UI system, ObjectSpawner

#### 2.1.3 ZombieShooter
- **Purpose**: Handles shooting mechanics and bullet spawning
- **Key Features**:
  - Animated bullet system with projectile physics
  - Screen-center aiming with crosshair integration
  - Input handling with death state checking
  - Raycast fallback if bullet prefab not assigned
  - Audio and visual effects (muzzle flash, hit effects)
  - Configurable damage, fire rate, and distance
- **Dependencies**: Unity Input System, Bullet prefab, PlayerHealth

### 2.2 Zombie AI System

#### 2.2.1 ZombieAI (Main Controller)
- **Purpose**: Central AI state machine controlling zombie behavior
- **States**: Patrol, Alert, Chase, Attack, Hit, Dead
- **Key Features**:
  - Pure Bool-based animation control (no Speed conflicts)
  - Multiple failsafe systems for state transitions
  - Player target acquisition (AR Camera)
  - NavMeshAgent integration for pathfinding
  - Comprehensive debug logging
  - Event-driven state changes
- **Dependencies**: ZombieVision, ZombieAttack, ZombieHealth, ZombieMovement

#### 2.2.2 ZombieVision
- **Purpose**: Handles player detection using field-of-view and line-of-sight
- **Key Features**:
  - Configurable FOV angle and detection range
  - Raycast-based line-of-sight checking
  - AR plane obstacle detection
  - Noise detection system (hearing)
  - Visual debugging with gizmos
- **Events**: OnPlayerDetected, OnPlayerLost
- **Dependencies**: LayerMask configuration

#### 2.2.3 ZombieAttack
- **Purpose**: Manages zombie melee attack behavior
- **Key Features**:
  - Attack cooldown system
  - Range-based attack validation
  - Animation trigger coordination
  - Player damage application
  - Attack range visualization
- **Events**: OnAttackAnimationTrigger
- **Dependencies**: PlayerHealth, Animator

#### 2.2.4 ZombieMovement
- **Purpose**: Handles movement animation parameter setting
- **Key Features**:
  - Defers to ZombieAI when present (prevents conflicts)
  - Speed calculation based on actual movement
  - Manual animation speed setting
  - Attack animation triggering
- **Dependencies**: Animator, ZombieAI

#### 2.2.5 ZombieHealth
- **Purpose**: Manages zombie health, damage response, and death
- **Key Features**:
  - Health tracking with damage application
  - Animation integration (hit/death states)
  - Score tracking integration
  - Visual and audio feedback
  - Fade-out death sequence
  - Component cleanup on death
- **Events**: OnDamageTaken, OnDeath
- **Dependencies**: GameScore, Animator

#### 2.2.6 ZombieSpawner
- **Purpose**: Manages zombie lifecycle and AR plane-based spawning
- **Key Features**:
  - AR plane height detection and tracking
  - Dynamic height adjustment for all zombies
  - Player death detection and spawning control
  - Continuous respawning system
  - Auto-component addition (AI, Animation, Health)
  - Event-driven spawning (ready button, player death/respawn)
- **Dependencies**: ARPlaneManager, PlayerHealth, ZombieAI components

### 2.3 Game Management Systems

#### 2.3.1 DeathScreen
- **Purpose**: Manages death screen UI and respawn functionality
- **Key Features**:
  - Full-screen overlay with dark background
  - Score display with kill count and survival time
  - Respawn button with touch handling
  - Game pause/resume (Time.timeScale)
  - Auto-component finding (no manual wiring needed)
  - Mobile-optimized UI scaling
- **Dependencies**: PlayerHealth, ZombieSpawner, ZombieShooter, GameScore

#### 2.3.2 GameScore
- **Purpose**: Static score tracking system
- **Key Features**:
  - Kill count tracking
  - Survival time calculation
  - Score reset functionality
  - Formatted score display
  - Total score calculation (kills + time)
- **Dependencies**: None (static system)

#### 2.3.3 GameManager
- **Purpose**: Handles UI connections and game flow initialization
- **Key Features**:
  - Auto-wires Continue button to zombie spawning
  - Manages greeting prompt visibility
  - Centralized UI event handling
- **Dependencies**: ZombieSpawner, UI components

### 2.4 Projectile System

#### 2.4.1 Bullet
- **Purpose**: Animated projectile with physics and damage
- **Key Features**:
  - Configurable speed, damage, and range
  - Raycast-based collision detection
  - AR plane filtering (ignores ARPlanes)
  - Hit effect spawning
  - Automatic cleanup at max distance
- **Dependencies**: ZombieHealth, LayerMask configuration

### 2.5 Navigation System

#### 2.5.1 ARNavMeshBuilder
- **Purpose**: Dynamically builds NavMesh from detected AR planes
- **Key Features**:
  - Real-time NavMesh generation from AR planes
  - Obstacle creation from vertical planes
  - NavMeshSurface management
  - Debug UI with plane statistics
  - Performance optimization with update intervals
- **Dependencies**: Unity AI Navigation package, ARPlaneManager

### 2.6 Mine System (Disabled by Default)

#### 2.6.1 ObjectSpawner (Modified)
- **Purpose**: Handles mine placement when explicitly enabled
- **Key Features**:
  - Disabled by default (shooting is primary)
  - Only enabled through UI menu selection
  - Spawns objects at raycast hit points
  - Managed by ARTemplateMenuManager
- **Dependencies**: ARRaycastManager, ARTemplateMenuManager

#### 2.6.2 ARTemplateMenuManager (Modified)
- **Purpose**: Manages object creation menu and spawning mode
- **Key Features**:
  - Enables ObjectSpawner only for mine-related objects
  - Disables ObjectSpawner when menu hidden
  - Shooting remains default mode
- **Dependencies**: ObjectSpawner

## 3. Animation System

### 3.1 Animation Controller Structure
- **Parameters**: Speed (Float), IsChasing (Bool), IsDetecting (Bool), IsAttacking (Bool), IsHit (Bool), IsDead (Bool), AIState (Int)
- **States**: Idle, Walk, Run, Scream, Attack, Hit, Death
- **Transition Logic**: Pure Bool-based for AI states, Speed-based only for Patrol (Idle ↔ Walk)

### 3.2 Animation Integration
- **ZombieAI**: Controls all Bool parameters based on AI state
- **ZombieMovement**: Handles Speed parameter (defers to AI when present)
- **Conflict Resolution**: ZombieMovement disables when ZombieAI present

## 4. Scene Structure

### 4.1 Main Components
- **XR Origin (AR Rig)**: Contains AR camera setup with PlayerHealth, ZombieShooter, CrosshairController
- **DeathScreenManager**: GameObject with DeathScreen script
- **ZombieSpawner**: Manages zombie lifecycle
- **ARNavMeshBuilder**: Handles dynamic navigation mesh
- **UI System**: Greeting prompts, menus, health display

### 4.2 Component Auto-Wiring
- **DeathScreen**: Auto-finds PlayerHealth, ZombieSpawner, ZombieShooter
- **ZombieSpawner**: Auto-adds required components to spawned zombies
- **PlayerHealth**: Auto-finds DeathScreen
- **GameManager**: Auto-connects Continue button

## 5. Event-Driven Architecture

### 5.1 Player Events
- **PlayerHealth**: OnHealthChanged, OnPlayerDeath, OnPlayerRespawn
- **ZombieShooter**: Responds to player death state

### 5.2 AI Events
- **ZombieVision**: OnPlayerDetected, OnPlayerLost
- **ZombieHealth**: OnDamageTaken, OnDeath
- **ZombieAttack**: OnAttackAnimationTrigger

### 5.3 Game Flow Events
- **Continue Button**: Triggers zombie spawning
- **Player Death**: Stops spawning, shows death screen
- **Player Respawn**: Resets spawning, clears zombies

## 6. Debug & Development Features

### 6.1 Comprehensive Logging
- **State transitions**: All AI state changes logged
- **Animation monitoring**: Real-time animation state tracking
- **Vision system**: Player detection/loss events
- **Performance metrics**: NavMesh update frequency

### 6.2 Visual Debug Tools
- **Gizmos**: FOV cones, attack ranges, patrol areas
- **On-screen UI**: Health bars, NavMesh statistics
- **Animation progress**: Scream animation completion tracking

### 6.3 Failsafe Systems
- **Multiple timeout layers**: Animation stuck prevention
- **Emergency state transitions**: Force state changes when stuck
- **Component auto-addition**: Missing components automatically added

## 7. Performance Optimizations

### 7.1 Update Frequency Control
- **Vision updates**: 0.2s intervals
- **NavMesh updates**: 0.2s intervals  
- **Animation updates**: 0.1s intervals
- **Debug logging**: 60-frame intervals

### 7.2 Mobile-Specific Optimizations
- **UI scaling**: Responsive to different screen sizes
- **Input handling**: Cross-platform touch/mouse support
- **Raycast optimization**: Layered hit detection

## 8. Current Implementation Status

### 8.1 Completed Systems ✅
- **Core shooting mechanics** with animated bullets
- **AI state machine** with pure Bool-based animations
- **Death screen system** with respawn functionality
- **Score tracking** and display
- **Dynamic NavMesh** from AR planes
- **Comprehensive debug systems**

### 8.2 Known Issues & Fixes Applied
- **Animation conflicts**: Resolved with Bool-based system
- **Scream loop bug**: Fixed with multiple failsafe layers
- **Input conflicts**: Shooting default, mines optional
- **Build compatibility**: Input System integration completed

### 8.3 Future Enhancements
- **Mixamo animation integration**: Planned animation upgrade
- **Advanced AI behaviors**: Enhanced zombie intelligence
- **Additional weapon types**: Planned feature expansion
- **Multiplayer support**: Future consideration