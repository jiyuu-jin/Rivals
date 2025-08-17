using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Samples.ARStarterAssets;
using UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Readers;

/// <summary>
/// Automatically configures ARInteractorSpawnTrigger for proper mine placement
/// </summary>
public class ARSpawnerSetup : MonoBehaviour
{
    [Header("Debug")]
    [Tooltip("Show setup information in console")]
    public bool enableDebugLogs = true;
    
    void Start()
    {
        SetupARSpawnerInput();
    }
    
    void SetupARSpawnerInput()
    {
        // Find the ARInteractorSpawnTrigger component
        ARInteractorSpawnTrigger spawnerTrigger = FindFirstObjectByType<ARInteractorSpawnTrigger>();
        
        if (spawnerTrigger == null)
        {
            if (enableDebugLogs) Debug.LogWarning("ARSpawnerSetup: No ARInteractorSpawnTrigger found in scene!");
            return;
        }
        
        if (enableDebugLogs) Debug.Log("ARSpawnerSetup: Found ARInteractorSpawnTrigger, configuring...");
        
        // Set the spawn trigger type to InputAction (instead of SelectAttempt)
        spawnerTrigger.spawnTriggerType = ARInteractorSpawnTrigger.SpawnTriggerType.InputAction;
        
        // Configure the spawn object input to use touch/mouse
        var spawnInput = spawnerTrigger.spawnObjectInput;
        spawnInput.inputSourceMode = XRInputButtonReader.InputSourceMode.InputActionReference;
        
        // Configure the input action (name is read-only, so we just log it)
        var inputAction = spawnInput.inputActionPerformed;
        
        if (enableDebugLogs) 
        {
            Debug.Log($"ARSpawnerSetup: Configured spawn trigger type to {spawnerTrigger.spawnTriggerType}");
            Debug.Log($"ARSpawnerSetup: Current input action name: {inputAction.name}");
        }
        
        // Verify ObjectSpawner is connected
        ObjectSpawner objectSpawner = FindFirstObjectByType<ObjectSpawner>();
        if (objectSpawner != null)
        {
            spawnerTrigger.objectSpawner = objectSpawner;
            if (enableDebugLogs) Debug.Log("ARSpawnerSetup: Connected ObjectSpawner to ARInteractorSpawnTrigger");
        }
        else
        {
            if (enableDebugLogs) Debug.LogWarning("ARSpawnerSetup: No ObjectSpawner found in scene!");
        }
        
        if (enableDebugLogs) Debug.Log("ARSpawnerSetup: Configuration complete!");
    }
    
    /// <summary>
    /// Manually trigger setup (useful for testing)
    /// </summary>
    [ContextMenu("Setup AR Spawner")]
    public void ManualSetup()
    {
        SetupARSpawnerInput();
    }
}
