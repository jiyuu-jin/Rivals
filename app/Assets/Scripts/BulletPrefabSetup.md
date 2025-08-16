# How to Create the Bullet Prefab

## Step 1: Create the Bullet GameObject
1. In Unity, create a new empty GameObject: `GameObject > Create Empty`
2. Name it "Bullet"

## Step 2: Add Visual Component (choose one)
### Option A: Simple Sphere
1. Add a Sphere: `GameObject > 3D Object > Sphere` as a child
2. Scale it down to (0.1, 0.1, 0.1) or smaller
3. Optional: Change material color to yellow/orange

### Option B: Capsule (more bullet-like)
1. Add a Capsule: `GameObject > 3D Object > Capsule` as a child  
2. Scale it to (0.05, 0.2, 0.05) for a bullet shape
3. Rotate it to point forward if needed

## Step 3: Add Required Components
1. **Add the Bullet script**: Select the root "Bullet" object and add the `Bullet.cs` script component
2. **Add a Rigidbody** (optional): Add `Rigidbody` component and set `Is Kinematic = true`
3. **Add a Collider**: Add a `Sphere Collider` or `Capsule Collider` and set `Is Trigger = true`

## Step 4: Configure the Bullet Script
- **Speed**: 20 (meters per second)
- **Lifetime**: 5 (seconds)
- **Hit Effect Prefab**: Leave empty for now (will be set by ZombieShooter)
- **Miss Effect Prefab**: Leave empty for now

## Step 5: Optional Enhancements
### Trail Renderer (for bullet trail)
1. Add `Trail Renderer` component to the Bullet object
2. Set **Time**: 0.3
3. Set **Width**: 0.02 to 0.01
4. Set **Color**: Bright yellow/white fading to transparent

### Light Component (for muzzle flash effect)
1. Add `Light` component
2. Set **Type**: Point
3. Set **Range**: 2
4. Set **Intensity**: 1
5. Set **Color**: Orange/Yellow

## Step 6: Create the Prefab
1. Drag the configured "Bullet" GameObject from the Hierarchy to the Project window (Assets folder)
2. This creates a prefab file
3. Delete the original from the scene
4. The prefab is now ready to be assigned to the `ZombieShooter.bulletPrefab` field

## Step 7: Assign to ZombieShooter
1. Select the GameObject with the `ZombieShooter` script (usually on Main Camera)
2. In the inspector, find the **Bullet Prefab** field
3. Drag your newly created bullet prefab into this field

## Testing
- The bullet should now spawn when you shoot
- It should travel in the direction you're aiming
- It should damage zombies on contact
- Debug logs will show bullet spawning and hits
