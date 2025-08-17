# **Death Screen Implementation Plan**

## **📋 Overview**
Create a full-screen death overlay that appears when the player's health reaches zero, with a simple respawn button to restart the game.

---

## **🎯 Core Features**
- **Full-screen dark overlay** when player dies
- **"You Died" message** with dramatic styling
- **Respawn button** to restart the game
- **Pause game logic** while death screen is active
- **Reset player health** and game state on respawn

---

## **📁 Files to Create/Modify**

### **1. New Script: `DeathScreen.cs`**
- **Purpose**: Manages death screen UI and respawn logic
- **Location**: `app/Assets/Scripts/DeathScreen.cs`
- **Responsibilities**:
  - Show/hide death screen overlay
  - Handle respawn button click
  - Reset game state on respawn

### **2. Modify: `PlayerHealth.cs`**
- **Changes**: Trigger death screen when health reaches zero
- **Add**: Event or direct call to show death screen

### **3. Modify: `ZombieSpawner.cs`**
- **Changes**: Stop spawning when player is dead
- **Add**: Reset zombie spawning on respawn

---

## **🎨 UI Design Specifications**

### **Death Screen Layout:**
```
┌─────────────────────────────────┐
│                                 │
│          [Dark Overlay]         │
│                                 │
│         💀 YOU DIED 💀          │
│                                 │
│        Final Score: XXX         │
│                                 │
│       ┌─────────────────┐       │
│       │   🔄 RESPAWN    │       │
│       └─────────────────┘       │
│                                 │
└─────────────────────────────────┘
```

### **Visual Elements:**
- **Background**: Semi-transparent black overlay (80% opacity)
- **Title**: Large red "YOU DIED" text
- **Score**: Optional kill count display
- **Button**: Green respawn button with icon
- **Animation**: Fade-in effect for dramatic impact

---

## **⚙️ Implementation Steps**

### **Phase 1: Create Death Screen UI**

#### **Step 1: Create `DeathScreen.cs`**
```csharp
using UnityEngine;
using UnityEngine.UI;

public class DeathScreen : MonoBehaviour
{
    [Header("UI References")]
    public GameObject deathPanel;
    public Button respawnButton;
    public Text deathMessage;
    public Text scoreText;
    
    [Header("Settings")]
    public string deathText = "YOU DIED";
    public Color overlayColor = new Color(0, 0, 0, 0.8f);
    
    private PlayerHealth playerHealth;
    private ZombieSpawner zombieSpawner;
    private bool isDead = false;
    
    void Start()
    {
        // Find components
        // Setup respawn button
        // Hide death screen initially
    }
    
    public void ShowDeathScreen()
    {
        // Show overlay
        // Pause game logic
        // Display final score
    }
    
    public void OnRespawnClicked()
    {
        // Reset player health
        // Reset zombie spawning
        // Hide death screen
        // Resume game
    }
    
    void CreateDeathUI()
    {
        // Dynamically create Canvas and UI elements
    }
}
```

#### **Step 2: UI Creation Methods**
- **Dynamic Canvas creation** (similar to CrosshairController)
- **Full-screen overlay panel**
- **Centered text and button elements**
- **Proper UI scaling** for different screen sizes

### **Phase 2: Integrate with Game Systems**

#### **Step 3: Modify `PlayerHealth.cs`**
```csharp
// Add death screen reference
public DeathScreen deathScreen;

// In Die() method:
void Die()
{
    if (isDead) return;
    isDead = true;
    
    // Existing death logic...
    
    // Show death screen
    if (deathScreen != null)
    {
        deathScreen.ShowDeathScreen();
    }
}

// Add respawn method:
public void Respawn()
{
    isDead = false;
    currentHealth = maxHealth;
    // Reset any other player state
}
```

#### **Step 4: Modify `ZombieSpawner.cs`**
```csharp
// Add death detection
private bool isPlayerDead = false;

// In Update() - stop spawning if player dead:
void Update()
{
    if (isPlayerDead) return;
    // Existing spawning logic...
}

// Add reset method:
public void ResetSpawning()
{
    isPlayerDead = false;
    // Clear existing zombies
    // Reset spawn state
}
```

### **Phase 3: Game State Management**

#### **Step 5: Pause/Resume Logic**
- **Time.timeScale = 0** when death screen shows
- **Time.timeScale = 1** on respawn
- **Disable zombie AI** during death screen
- **Disable shooting input** during death screen

#### **Step 6: Score Tracking (Optional)**
```csharp
public class GameScore : MonoBehaviour
{
    public static int zombiesKilled = 0;
    public static float survivalTime = 0f;
    
    public static void AddKill() { zombiesKilled++; }
    public static void Reset() { zombiesKilled = 0; survivalTime = 0f; }
    public static string GetScoreText() 
    { 
        return $"Zombies Killed: {zombiesKilled}\nSurvival Time: {survivalTime:F1}s"; 
    }
}
```

---

## **🎮 User Experience Flow**

### **Death Sequence:**
1. **Player health reaches 0**
2. **Screen fades to dark overlay**
3. **"YOU DIED" text appears**
4. **Final score displays**
5. **Respawn button becomes clickable**

### **Respawn Sequence:**
1. **Player clicks respawn button**
2. **Death screen fades out**
3. **Player health resets to full**
4. **All zombies cleared/reset**
5. **Game resumes normal state**

---

## **🔧 Technical Considerations**

### **Performance:**
- **Reuse Canvas** instead of recreating
- **Object pooling** for UI elements
- **Efficient zombie cleanup** on respawn

### **Mobile Optimization:**
- **Touch-friendly button size** (minimum 44x44 points)
- **Clear visual feedback** for button presses
- **Proper UI scaling** across devices

### **Edge Cases:**
- **Multiple death triggers** (prevent duplicate death screens)
- **Rapid respawn clicks** (cooldown/disable button)
- **Memory cleanup** when resetting game state

---

## **📱 Platform-Specific Notes**

### **AR Foundation Integration:**
- **Maintain AR tracking** during death screen
- **Keep camera feed** visible in background
- **Preserve AR plane data** across respawns

### **Input Handling:**
- **Touch input** for respawn button
- **Disable shooting** during death screen
- **Handle back button** on Android (optional)

---

## **🚀 Implementation Priority**

### **Must-Have (Phase 1):**
- ✅ Basic death screen overlay
- ✅ Respawn button functionality
- ✅ Player health reset

### **Nice-to-Have (Phase 2):**
- 📊 Score display
- 🎵 Death sound effects
- ✨ Fade animations

### **Future Enhancements:**
- 🏆 High score tracking
- 📈 Statistics screen
- 🎯 Achievement system

---

**Ready to implement?** Let me know which phase you'd like to start with! 🎯
