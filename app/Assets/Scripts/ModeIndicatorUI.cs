using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Displays the current input mode (Shooting/Mine Placement) at the top of the screen
/// </summary>
public class ModeIndicatorUI : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Text component to display the mode name")]
    public Text modeText;
    
    [Tooltip("Text component to display instructions")]
    public Text instructionText;
    
    [Header("Auto-Create UI")]
    [Tooltip("Automatically create UI if references are null")]
    public bool autoCreateUI = true;
    
    [Header("Positioning")]
    [Tooltip("Distance from top of screen")]
    public float topMargin = 100f;
    
    [Tooltip("Font size for mode text")]
    public int modeFontSize = 24;
    
    [Tooltip("Font size for instruction text")]
    public int instructionFontSize = 18;
    
    private Canvas canvas;
    private RectTransform canvasRect;
    
    void Start()
    {
        // Find or create UI elements
        if (autoCreateUI && (modeText == null || instructionText == null))
        {
            CreateModeUI();
        }
        
        // Subscribe to mode changes
        InputModeManager.OnModeChanged += OnModeChanged;
        
        // Set initial mode display
        UpdateModeDisplay();
        
        Debug.Log("ModeIndicatorUI: UI created and mode listeners setup");
    }
    
    void CreateModeUI()
    {
        // Find or create canvas
        canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("Mode UI Canvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100; // High priority to appear on top
            
            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920); // Mobile resolution
            scaler.matchWidthOrHeight = 0.5f;
            
            canvasObj.AddComponent<GraphicRaycaster>();
        }
        
        canvasRect = canvas.GetComponent<RectTransform>();
        
        // Create mode indicator panel
        GameObject modePanel = new GameObject("Mode Indicator Panel");
        modePanel.transform.SetParent(canvas.transform, false);
        
        RectTransform panelRect = modePanel.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 1f); // Top center
        panelRect.anchorMax = new Vector2(0.5f, 1f);
        panelRect.pivot = new Vector2(0.5f, 1f);
        panelRect.anchoredPosition = new Vector2(0, -topMargin);
        panelRect.sizeDelta = new Vector2(400, 80);
        
        // Add background (optional - for better readability)
        Image panelImage = modePanel.AddComponent<Image>();
        panelImage.color = new Color(0, 0, 0, 0.3f); // Semi-transparent black
        panelImage.raycastTarget = false; // Don't block touches
        
        // Create mode text
        GameObject modeTextObj = new GameObject("Mode Text");
        modeTextObj.transform.SetParent(modePanel.transform, false);
        
        modeText = modeTextObj.AddComponent<Text>();
        modeText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        modeText.fontSize = modeFontSize;
        modeText.alignment = TextAnchor.MiddleCenter;
        modeText.color = Color.white;
        modeText.raycastTarget = false;
        
        RectTransform modeTextRect = modeText.GetComponent<RectTransform>();
        modeTextRect.anchorMin = Vector2.zero;
        modeTextRect.anchorMax = Vector2.one;
        modeTextRect.offsetMin = new Vector2(10, 25);
        modeTextRect.offsetMax = new Vector2(-10, -5);
        
        // Create instruction text
        GameObject instructionTextObj = new GameObject("Instruction Text");
        instructionTextObj.transform.SetParent(modePanel.transform, false);
        
        instructionText = instructionTextObj.AddComponent<Text>();
        instructionText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        instructionText.fontSize = instructionFontSize;
        instructionText.alignment = TextAnchor.MiddleCenter;
        instructionText.color = Color.white;
        instructionText.raycastTarget = false;
        
        RectTransform instructionTextRect = instructionText.GetComponent<RectTransform>();
        instructionTextRect.anchorMin = Vector2.zero;
        instructionTextRect.anchorMax = Vector2.one;
        instructionTextRect.offsetMin = new Vector2(10, 5);
        instructionTextRect.offsetMax = new Vector2(-10, -25);
        
        Debug.Log("ModeIndicatorUI: UI elements created successfully");
    }
    
    /// <summary>
    /// Handle mode changes from InputModeManager
    /// </summary>
    void OnModeChanged(InputMode newMode)
    {
        UpdateModeDisplay();
        Debug.Log($"ModeIndicatorUI: Mode changed to {newMode}");
    }
    
    /// <summary>
    /// Update the display based on current mode
    /// </summary>
    void UpdateModeDisplay()
    {
        if (InputModeManager.Instance == null) return;
        
        if (modeText != null)
        {
            modeText.text = InputModeManager.Instance.GetModeDisplayName();
            modeText.color = InputModeManager.Instance.GetModeColor();
        }
        
        if (instructionText != null)
        {
            instructionText.text = InputModeManager.Instance.GetModeInstructions();
            instructionText.color = InputModeManager.Instance.GetModeColor();
        }
    }
    
    /// <summary>
    /// Manually refresh the display (useful for testing)
    /// </summary>
    public void RefreshDisplay()
    {
        UpdateModeDisplay();
    }
    
    void OnDestroy()
    {
        // Unsubscribe from events
        InputModeManager.OnModeChanged -= OnModeChanged;
    }
}
