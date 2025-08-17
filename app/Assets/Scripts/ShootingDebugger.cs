using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

/// <summary>
/// Debug script to help diagnose shooting issues
/// </summary>
public class ShootingDebugger : MonoBehaviour
{
    void Start()
    {
        Debug.Log("=== ShootingDebugger: STARTUP DIAGNOSTICS ===");
        
        // Check if Main Camera exists and has required components
        Camera mainCamera = Camera.main;
        if (mainCamera == null)
            mainCamera = FindFirstObjectByType<Camera>();
            
        if (mainCamera != null)
        {
            Debug.Log($"Main Camera found: {mainCamera.name}");
            
            // Check for ZombieShooter
            ZombieShooter shooter = mainCamera.GetComponent<ZombieShooter>();
            Debug.Log($"ZombieShooter component: {(shooter != null ? "FOUND" : "MISSING")}");
            
            // Check for CrosshairController
            CrosshairController crosshair = mainCamera.GetComponent<CrosshairController>();
            Debug.Log($"CrosshairController component: {(crosshair != null ? "FOUND" : "MISSING")}");
        }
        else
        {
            Debug.LogError("No main camera found!");
        }
        
        // Check for ZombieSpawner
        ZombieSpawner spawner = FindFirstObjectByType<ZombieSpawner>();
        Debug.Log($"ZombieSpawner: {(spawner != null ? "FOUND" : "MISSING")}");
        
        // Check for GameManager
        GameManager gameManager = FindFirstObjectByType<GameManager>();
        Debug.Log($"GameManager: {(gameManager != null ? "FOUND" : "MISSING")}");
        
        // Check for Continue Button
        GameObject continueBtn = GameObject.Find("Continue Button");
        if (continueBtn != null)
        {
            Button button = continueBtn.GetComponent<Button>();
            Debug.Log($"Continue Button found, has {button.onClick.GetPersistentEventCount()} persistent events");
        }
        else
        {
            Debug.LogWarning("Continue Button not found!");
        }
        
        // Check for zombies in scene
        GameObject[] zombies = GameObject.FindGameObjectsWithTag("Zombie");
        Debug.Log($"Found {zombies.Length} objects tagged as 'Zombie':");
        foreach (var zombie in zombies)
        {
            ZombieHealth health = zombie.GetComponent<ZombieHealth>();
            Debug.Log($"  - {zombie.name}: ZombieHealth = {(health != null ? $"YES (health: {health.currentHealth}/{health.maxHealth})" : "NO")}");
            
            // Check if it has colliders
            Collider[] colliders = zombie.GetComponents<Collider>();
            Debug.Log($"    Colliders: {colliders.Length}");
        }
        
        Debug.Log("=== ShootingDebugger: DIAGNOSTICS COMPLETE ===");
    }
    
    void Update()
    {
        // Check for input and log it - use new Input System
        bool mouseInput = Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;
        bool touchInput = Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame;
        
        if (mouseInput || touchInput)
        {
            Debug.Log("ShootingDebugger: Input detected - mouse or touch!");
        }
    }
    
    void OnGUI()
    {
        // Debug info disabled - uncomment below to re-enable
        return;
        
        // Show debug info on screen
        GUI.color = Color.green;
        GUILayout.Label("=== SHOOTING DEBUG INFO ===");
        
        Camera cam = Camera.main;
        if (cam != null)
        {
            ZombieShooter shooter = cam.GetComponent<ZombieShooter>();
            CrosshairController crosshair = cam.GetComponent<CrosshairController>();
            
            GUILayout.Label($"ZombieShooter: {(shooter != null ? "✓" : "✗")}");
            GUILayout.Label($"CrosshairController: {(crosshair != null ? "✓" : "✗")}");
        }
        
        GameObject[] zombies = GameObject.FindGameObjectsWithTag("Zombie");
        GUILayout.Label($"Zombies in scene: {zombies.Length}");
        
        ZombieSpawner spawner = FindFirstObjectByType<ZombieSpawner>();
        if (spawner != null)
        {
            GUILayout.Label($"ZombieSpawner: Found");
        }
    }
}
