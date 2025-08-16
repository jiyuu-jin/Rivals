# Mixamo Animation Setup Guide for Unity

## Overview
This guide walks you through importing Mixamo animations for your rigged zombie model and setting up a Unity Animator State Machine for smooth animation transitions.

## Prerequisites
- Rigged 3D model (your Parasite zombie)
- Mixamo account (free at mixamo.com)
- Unity project with the model already imported

---

## Phase 1: Downloading Animations from Mixamo

### Step 1: Upload Your Model to Mixamo
1. **Go to mixamo.com** and sign in
2. **Click "Upload Character"**
3. **Upload your rigged Parasite model** (.fbx file)
4. **Let Mixamo auto-rig** if it's not already rigged
5. **Verify the rigging** looks correct in the preview

### Step 2: Download Key Animations
Download these essential zombie animations:

#### **Locomotion Animations:**
- **Idle** - "Zombie Idle" or "Standing Idle"
- **Walk** - "Zombie Walk" or "Walking"
- **Run** - "Zombie Run" or "Fast Run"

#### **Action Animations:**
- **Attack** - "Zombie Punching" or "Zombie Crawl Attack"
- **Hit Reaction** - "Hit Reaction" or "Being Hit"
- **Death** - "Zombie Death" or "Dying"

#### **Essential Animations (continued):**
- **Scream/Roar** - "Yelling", "Zombie Scream", or "Roaring" (plays before running)

#### **Optional Animations:**
- **Spawn/Rise** - "Getting Up" or "Zombie Rising"

### Step 3: Download Settings
For each animation:
- **Format**: FBX for Unity (.fbx)
- **Skin**: With Skin (if you want the model included)
- **Keyframe Reduction**: None (for best quality)
- **FPS**: 30
- **In Place**: ✅ Check this for locomotion (walk/run)

---

## Phase 2: Importing into Unity

### Step 1: Import Animation Files
1. **Create an "Animations" folder** in Assets/
2. **Drag all downloaded .fbx files** into this folder
3. **Wait for Unity to process** the imports

### Step 2: Configure Import Settings
For each animation file:
1. **Select the .fbx file** in Project window
2. **Go to Inspector > Rig tab**
3. **Set Animation Type**: Humanoid
4. **Set Avatar Definition**: Create From This Model (for first) or Copy From Other Avatar
5. **Click Apply**

6. **Go to Animation tab**
7. **For each animation clip**:
   - **Loop Time**: ✅ (for idle, walk, run)
   - **Loop Pose**: ✅ (for looping animations)
   - **Root Transform Rotation**: ✅ Bake Into Pose (for in-place movement)
   - **Root Transform Position (Y)**: ✅ Bake Into Pose
   - **Root Transform Position (XZ)**: ✅ Bake Into Pose (for in-place)
8. **Click Apply**

---

## Phase 3: Setting Up the Animator Controller

### Step 1: Create Animator Controller
1. **Right-click in Project** window
2. **Create > Animator Controller**
3. **Name it** "ZombieAnimatorController"

### Step 2: Assign to Your Model
1. **Select your zombie model** in the scene
2. **Add Animator component** if not present
3. **Assign your controller** to the Controller field

### Step 3: Set Up State Machine

#### **Create States:**
1. **Open Animator window** (Window > Animation > Animator)
2. **Right-click in grid** > Create State > Empty
3. **Create these states**:
   - **Idle** (set as default - right-click > Set as Layer Default State)
   - **Walk**
   - **Scream** (plays before running)
   - **Run**
   - **Attack**
   - **Hit**
   - **Death**

#### **Assign Animation Clips:**
1. **Select each state**
2. **In Inspector**, assign the corresponding animation clip to **Motion**

### Step 4: Create Parameters
In the Animator window, click **Parameters tab** and add:
- **Speed** (Float) - for movement speed
- **IsAttacking** (Bool) - for attack trigger
- **IsHit** (Bool) - for hit reaction
- **IsDead** (Bool) - for death state

### Step 5: Create Transitions

#### **Movement Transitions:**
- **Idle → Walk**: Condition `Speed > 0.1`
- **Walk → Idle**: Condition `Speed < 0.1`
- **Walk → Scream**: Condition `Speed > 2.0` (triggers scream before running)
- **Scream → Run**: Has Exit Time ✅ (automatically transitions when scream finishes)
- **Run → Walk**: Condition `Speed < 2.0`
- **Run → Idle**: Condition `Speed < 0.1`

#### **Action Transitions:**
- **Any State → Attack**: Condition `IsAttacking = true`
- **Attack → Idle**: Has Exit Time ✅, IsAttacking = false
- **Any State → Hit**: Condition `IsHit = true`
- **Hit → Idle**: Has Exit Time ✅, IsHit = false
- **Any State → Death**: Condition `IsDead = true`

### Step 6: Configure Transition Settings
For smooth transitions:
- **Transition Duration**: 0.1-0.3 seconds
- **Interruption Source**: Current State
- **Ordered Interruption**: ✅

#### **Special Settings for Scream State:**
- **Scream → Run transition**: 
  - **Has Exit Time**: ✅ (let scream finish)
  - **Exit Time**: 0.8-0.9 (transition near end of scream)
  - **Transition Duration**: 0.1 (quick transition to running)

---

## Phase 4: Script Integration

### Step 1: Update ZombieHealth.cs
Add animator control to the zombie health script:

```csharp
public class ZombieHealth : MonoBehaviour
{
    private Animator animator;
    
    void Start()
    {
        animator = GetComponent<Animator>();
        // ... existing code
    }
    
    public void TakeDamage(int damage)
    {
        // ... existing damage code
        
        // Trigger hit animation
        if (animator != null)
        {
            animator.SetBool("IsHit", true);
            StartCoroutine(ResetHitTrigger());
        }
        
        if (currentHealth <= 0)
        {
            Die();
        }
    }
    
    void Die()
    {
        // ... existing death code
        
        // Trigger death animation
        if (animator != null)
        {
            animator.SetBool("IsDead", true);
        }
    }
    
    IEnumerator ResetHitTrigger()
    {
        yield return new WaitForSeconds(0.1f);
        if (animator != null)
        {
            animator.SetBool("IsHit", false);
        }
    }
}
```

### Step 2: Create ZombieMovement.cs (Optional)
For movement-based animation:

```csharp
public class ZombieMovement : MonoBehaviour
{
    private Animator animator;
    private Vector3 lastPosition;
    public float speedMultiplier = 1f;
    
    void Start()
    {
        animator = GetComponent<Animator>();
        lastPosition = transform.position;
    }
    
    void Update()
    {
        // Calculate movement speed
        float speed = Vector3.Distance(transform.position, lastPosition) / Time.deltaTime;
        lastPosition = transform.position;
        
        // Update animator
        if (animator != null)
        {
            animator.SetFloat("Speed", speed * speedMultiplier);
        }
    }
}
```

---

## Phase 5: Testing and Tuning

### Step 1: Test Basic States
1. **Enter Play Mode**
2. **In Animator window**, watch state transitions
3. **Use Inspector** to manually toggle parameters for testing

### Step 2: Fine-Tune Transitions
- **Adjust transition durations** for smoothness
- **Modify parameter thresholds** for better responsiveness
- **Test edge cases** (rapid state changes)

### Step 3: Optimize Performance
- **Use Culling Mode**: Based on Renderers for offscreen zombies
- **Disable unnecessary muscle groups** in Avatar configuration
- **Use Animation Compression** for smaller file sizes

---

## Troubleshooting

### Common Issues:
- **Animations not playing**: Check if Animator Controller is assigned
- **Jerky transitions**: Reduce transition duration or check Root Motion settings
- **Floating/sliding**: Verify Root Transform Position settings
- **Wrong scale**: Check model import scale and Mixamo export settings

### Performance Tips:
- **Use Animation Events** for precise timing (attack hit frames, etc.)
- **Implement Animation Layers** for additive animations (hit reactions)
- **Cache Animator references** instead of GetComponent calls

---

## Next Steps
1. **Download and import** your chosen Mixamo animations
2. **Follow the setup guide** step by step
3. **Test with simple parameter changes** first
4. **Integrate with your existing ZombieHealth script**
5. **Add movement-based animations** as needed

This setup will give you professional-looking animated zombies that react to damage, move naturally, and die dramatically!
