# Zombie AI System Setup Guide

## Overview
This guide creates a complete AI system where zombies patrol randomly, detect the player, scream when they see them, chase aggressively, and attack when close. Uses Unity's latest NavMesh system and our existing animation controller.

## Prerequisites
- Completed Mixamo animation setup with state machine
- ZombieHealth.cs and ZombieMovement.cs already implemented
- Unity NavMesh system available (built-in)

---

## Phase 1: AI Behavior Design

### **AI State Machine:**
1. **Patrol** → Random wandering around spawn area
2. **Alert** → Player detected, scream animation plays
3. **Chase** → Aggressive pursuit of player
4. **Attack** → Close combat when in range
5. **Hit** → Temporary stun when damaged
6. **Death** → Final state

### **Detection System:**
- **Vision Cone**: 120° field of view
- **Detection Range**: 8-12 meters
- **Attack Range**: 1.5 meters
- **Hearing**: Respond to gunshots/noise

---

## Phase 2: Dynamic AR NavMesh Setup

### Step 1: AR Foundation Integration
**No static NavMesh baking needed!** Instead, we'll build navigation dynamically from detected AR planes.

1. **Skip traditional NavMesh baking** (no "Navigation Static" objects needed)
2. **Use AR plane detection** for walkable surfaces
3. **Build NavMesh at runtime** based on ARPlaneManager data
4. **Dynamic obstacle creation** from detected walls (vertical AR planes)

### Step 2: Install NavMesh Components Package
1. **Window > Package Manager**
2. **Search for "AI Navigation"** or "NavMesh Components"
3. **Install** the package for runtime NavMesh building
4. **This enables `NavMeshSurface` component** for dynamic building

### Step 3: Zombie NavMesh Agent Setup
1. **Select your zombie prefab**
2. **Add Component**: `NavMeshAgent`
3. **Configure NavMeshAgent**:
   - **Speed**: 1.5 (walking speed)
   - **Angular Speed**: 120
   - **Acceleration**: 8
   - **Stopping Distance**: 1.5 (attack range)
   - **Auto Braking**: ✅
   - **Radius**: 0.5 (zombie width)
   - **Height**: 1.8 (zombie height)
   - **Area Mask**: Default (walkable)
   - **Base Offset**: 0 (ground level)

---

## Phase 3: Core AI Scripts

### Script 1: ZombieAI.cs (Main Controller)
**Purpose**: Controls all AI behavior states and decision making

**Key Features**:
- State machine management
- Player detection and tracking
- Dynamic NavMesh pathfinding integration
- Animation trigger coordination
- AR plane-based navigation

### Script 4: ARNavMeshBuilder.cs (Dynamic Navigation)
**Purpose**: Builds NavMesh surfaces dynamically from detected AR planes

**Key Features**:
- Real-time NavMesh generation from AR planes
- Wall obstacle creation from vertical planes
- NavMesh updates when new planes detected
- Performance-optimized rebuilding

### Script 2: ZombieVision.cs (Detection System)
**Purpose**: Handles player detection using raycasting and field-of-view

**Key Features**:
- 120° vision cone
- Raycast-based line-of-sight checking
- Distance-based detection
- Noise detection (gunshots)

### Script 3: ZombieAttack.cs (Combat System)
**Purpose**: Manages attack behavior and damage dealing

**Key Features**:
- Attack range checking
- Attack cooldown management
- Damage dealing to player
- Attack animation coordination

---

## Phase 4: Animation Integration

### **New Animator Parameters:**
Add these to your existing animator controller (you already have `IsAttacking` from previous setup):
- **AIState** (Int): 0=Patrol, 1=Alert, 2=Chase, 3=Attack
- **IsDetecting** (Bool): Triggers scream/alert state  
- **IsChasing** (Bool): High-speed movement
- ~~**IsAttacking** (Bool): Attack animations~~ ✅ **Already exists**

### **Animation States Breakdown:**

#### **States You Already Have (✅ Complete):**
- **Idle** - Standing still animation
- **Walk** - Normal walking (Speed > 0.1)
- **Run** - Fast movement (Speed > 2.0) 
- **Attack** - Existing attack animation (IsAttacking = true)
- **Hit** - Damage reaction (IsHit = true)
- **Death** - Death animation (IsDead = true)

#### **New States You Need to Add:**
- **Scream/Alert** - Zombie roars when detecting player (download "Yelling" or "Zombie Scream" from Mixamo)

### **Updated State Transitions:**

#### **Basic Movement (Already Working):**
- **Idle → Walk**: `Speed > 0.1`
- **Walk → Idle**: `Speed < 0.1`
- **Walk → Run**: `Speed > 2.0`
- **Run → Walk**: `Speed < 2.0`

#### **New AI Behavior Transitions:**
- **Any State → Scream**: `IsDetecting == true` (player spotted from any state)
- **Scream → Run**: Has Exit Time ✅ (auto-transition after scream finishes)
- **Run → Attack**: `IsAttacking == true` (when close to player)
- **Attack → Run**: `IsAttacking == false` (continue chasing)

#### **Interrupt Transitions (Any State):**
- **Any State → Hit**: `IsHit == true` (existing)
- **Any State → Death**: `IsDead == true` (existing)
- **Any State → Scream**: `IsDetecting == true` (new - player detection)

---

## Phase 5: Player Detection & Targeting

### **AR Camera Integration:**
The AI will target the **AR Camera** (player's viewpoint):
- **Automatic detection** of `Camera.main` or tagged "MainCamera"
- **Dynamic targeting** as player moves around
- **Distance-based behavior** changes
- **Real-world movement** tracking in AR space

### **Detection Layers:**
- **Player Layer**: AR Camera and XR Origin
- **Obstacle Layer**: Detected AR walls (vertical planes)
- **Ground Layer**: Detected AR floors (horizontal planes)
- **Dynamic Updates**: Layers update as AR detection improves

---

## Phase 6: Behavior Specifications

### **Patrol Behavior:**
- **Random destination** within detected AR plane boundaries
- **Stay on walkable surfaces**: Only move on detected AR floors
- **Walk speed**: 1.5 m/s
- **Pause duration**: 2-4 seconds at each point
- **Look around**: Random rotation while paused
- **Boundary respect**: Don't walk off detected planes

### **Alert Behavior:**
- **Scream animation**: 1-2 second duration
- **Face player**: Rotate to look at detected player
- **Transition**: Automatically to chase after scream

### **Chase Behavior:**
- **Run speed**: 4.0 m/s (increase NavMeshAgent speed)
- **Direct pursuit**: Dynamic NavMesh pathfinding to player position
- **Update frequency**: Every 0.2 seconds
- **Obstacle avoidance**: AR wall detection + NavMesh obstacles
- **Smart pathfinding**: Navigate around detected furniture/walls

### **Attack Behavior:**
- **Attack range**: 1.5 meters
- **Attack cooldown**: 1.5 seconds
- **Damage per attack**: 25 HP
- **Animation duration**: ~1 second
- **Knockback**: Optional slight player push

---

## Phase 7: Advanced Features

### **Smart Spawning:**
- **Spawn behind player**: Outside of view cone
- **Minimum distance**: 3+ meters from player
- **Maximum active**: Limit simultaneous attacking zombies

### **Group Behavior:**
- **Spread out**: Avoid clustering around player
- **Flanking**: Some zombies approach from sides
- **Communication**: Alert nearby zombies when player detected

### **Performance Optimization:**
- **Distance culling**: Disable AI for far zombies
- **Update frequency**: Reduce tick rate for distant zombies
- **NavMesh optimization**: Use simplified paths for distant targets

---

## Phase 8: Integration Points

### **With Existing Systems:**
- **ZombieHealth**: AI pauses when hit, stops when dead
- **ZombieMovement**: Speed parameters sync with NavMeshAgent
- **ZombieSpawner**: Auto-add AI components to new zombies
- **Bullet System**: Zombies react to being shot

### **UI Integration:**
- **Health indicators**: Show player health when attacked
- **Zombie counter**: Display active zombie count
- **Damage feedback**: Screen effects when hit

---

## Phase 9: Testing & Tuning

### **Debug Features:**
- **Gizmos**: Show detection range, vision cone, NavMesh path
- **Debug logs**: State changes, detection events
- **Visual indicators**: Color-coded zombie states

### **Balance Parameters:**
- **Detection range**: 8-12 meters (adjustable)
- **Chase speed**: 3-5 m/s (tunable)
- **Attack damage**: 20-30 HP (configurable)
- **Spawn frequency**: Based on difficulty

---

## Phase 10: Code Architecture

### **Main Classes:**
```csharp
public class ZombieAI : MonoBehaviour
{
    public enum AIState { Patrol, Alert, Chase, Attack, Hit, Dead }
    
    // State management
    // Dynamic NavMesh integration
    // Animation coordination
    // Player tracking
    // AR plane boundary checking
}

public class ARNavMeshBuilder : MonoBehaviour
{
    // AR plane detection integration
    // Dynamic NavMesh surface creation
    // Wall obstacle generation
    // Performance-optimized rebuilding
    // NavMeshSurface management
}

public class ZombieVision : MonoBehaviour
{
    // Field of view detection
    // Raycast line-of-sight
    // Player visibility checking
    // Noise detection
    // AR obstacle awareness
}

public class ZombieAttack : MonoBehaviour
{
    // Attack range checking
    // Damage dealing
    // Attack cooldown
    // Animation triggers
}
```

### **Event System:**
- **OnPlayerDetected**: Triggered when zombie sees player
- **OnPlayerLost**: When player escapes detection
- **OnAttackHit**: When attack connects with player
- **OnZombieDeath**: Cleanup and score events

---

## Phase 11: AR-Specific Advantages

### **Dynamic World Understanding:**
- **Real-time adaptation**: NavMesh updates as player scans more of the room
- **Realistic boundaries**: Zombies respect actual room layout
- **Wall detection**: Automatic obstacle creation from detected walls
- **Floor boundaries**: Zombies won't walk off detected surfaces

### **Enhanced Immersion:**
- **Real-world pathfinding**: Zombies navigate actual furniture/obstacles
- **Believable AI**: Behavior adapts to player's actual environment
- **Room-scale gameplay**: Works in any size room
- **Persistent layout**: NavMesh remembers discovered areas

### **Performance Benefits:**
- **Efficient NavMesh**: Only builds where needed (detected areas)
- **Selective updates**: Only rebuilds when new planes detected
- **Smaller memory footprint**: No large pre-baked NavMesh data
- **Mobile-optimized**: Designed for AR device constraints

---

## Implementation Priority

### **Phase 1 (Core):**
1. Basic patrol behavior
2. Player detection
3. Simple chase mechanics

### **Phase 2 (Enhancement):**
1. Attack system
2. Animation integration
3. Sound effects

### **Phase 3 (Polish):**
1. Group behavior
2. Performance optimization
3. Advanced AI features

This AI system will create engaging, dynamic zombie encounters that respond intelligently to player movement and create genuine tension during gameplay!
