using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Quick toggle button to switch between shooting and mine placement modes
/// </summary>
public class ModeToggleButton : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Button component for toggling")]
    public Button toggleButton;
    
    [Tooltip("Text component to display current mode")]
    public Text buttonText;
    
    [Header("Auto-Create UI")]
    [Tooltip("Automatically create UI if references are null")]
    public bool autoCreateUI = true;
    
    [Header("Positioning")]
    [Tooltip("Position on screen (0,0 = bottom-left, 1,1 = top-right)")]
    public Vector2 screenPosition = new Vector2(0.05f, 0.15f); // Bottom-left area
    
    [Tooltip("Button size")]
    public Vector2 buttonSize = new Vector2(120, 50);
    
    [Header("Visual Settings")]
    [Tooltip("Font size for button text")]
    public int fontSize = 16;
    
    [Tooltip("Button colors for different modes")]
    public Color shootingButtonColor = new Color(0f, 0.8f, 0f, 0.8f); // Green
    public Color minePlacementButtonColor = new Color(1f, 0.5f, 0f, 0.8f); // Orange
    
    private Canvas canvas;
    
    void Start()
    {
        // Find or create UI elements
        if (autoCreateUI && (toggleButton == null || buttonText == null))
        {
            CreateToggleButton();
        }
        
        // Setup button click listener
        if (toggleButton != null)
        {
            toggleButton.onClick.AddListener(OnToggleClicked);
        }
        
        // Subscribe to mode changes
        InputModeManager.OnModeChanged += OnModeChanged;
        
        // Set initial button appearance
        UpdateButtonAppearance();
        
        Debug.Log("ModeToggleButton: Toggle button created and listeners setup");
    }
    
    void CreateToggleButton()
    {
        // Find or create canvas
        canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("Toggle Button Canvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 99; // High priority but below mode indicator
            
            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920); // Mobile resolution
            scaler.matchWidthOrHeight = 0.5f;
            
            canvasObj.AddComponent<GraphicRaycaster>();
        }
        
        // Create toggle button
        GameObject buttonObj = new GameObject("Mode Toggle Button");
        buttonObj.transform.SetParent(canvas.transform, false);
        
        toggleButton = buttonObj.AddComponent<Button>();
        Image buttonImage = buttonObj.AddComponent<Image>();
        buttonImage.color = shootingButtonColor;
        
        RectTransform buttonRect = buttonObj.GetComponent<RectTransform>();
        buttonRect.anchorMin = screenPosition;
        buttonRect.anchorMax = screenPosition;
        buttonRect.pivot = new Vector2(0f, 0f); // Bottom-left pivot
        buttonRect.anchoredPosition = Vector2.zero;
        buttonRect.sizeDelta = buttonSize;
        
        // Create button text
        GameObject textObj = new GameObject("Button Text");
        textObj.transform.SetParent(buttonObj.transform, false);
        
        buttonText = textObj.AddComponent<Text>();
        buttonText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        buttonText.fontSize = fontSize;
        buttonText.alignment = TextAnchor.MiddleCenter;
        buttonText.color = Color.white;
        buttonText.text = "MODE";
        
        RectTransform textRect = buttonText.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        
        Debug.Log("ModeToggleButton: UI elements created successfully");
    }
    
    /// <summary>
    /// Handle button click - toggle between modes
    /// </summary>
    public void OnToggleClicked()
    {
        if (InputModeManager.Instance != null)
        {
            InputModeManager.Instance.ToggleMode();
            Debug.Log("ModeToggleButton: Mode toggled via button click");
        }
        else
        {
            Debug.LogWarning("ModeToggleButton: InputModeManager not found!");
        }
    }
    
    /// <summary>
    /// Handle mode changes from InputModeManager
    /// </summary>
    void OnModeChanged(InputMode newMode)
    {
        UpdateButtonAppearance();
        Debug.Log($"ModeToggleButton: Updated appearance for mode {newMode}");
    }
    
    /// <summary>
    /// Update button appearance based on current mode
    /// </summary>
    void UpdateButtonAppearance()
    {
        if (InputModeManager.Instance == null) return;
        
        bool isShootingMode = InputModeManager.Instance.IsShootingMode();
        
        // Update button color
        if (toggleButton != null)
        {
            Image buttonImage = toggleButton.GetComponent<Image>();
            if (buttonImage != null)
            {
                buttonImage.color = isShootingMode ? shootingButtonColor : minePlacementButtonColor;
            }
        }
        
        // Update button text
        if (buttonText != null)
        {
            buttonText.text = isShootingMode ? "MINE" : "SHOOT";
            buttonText.color = Color.white;
        }
    }
    
    /// <summary>
    /// Show/hide the toggle button
    /// </summary>
    public void SetVisible(bool visible)
    {
        if (toggleButton != null)
        {
            toggleButton.gameObject.SetActive(visible);
        }
    }
    
    /// <summary>
    /// Enable/disable the toggle button
    /// </summary>
    public void SetInteractable(bool interactable)
    {
        if (toggleButton != null)
        {
            toggleButton.interactable = interactable;
        }
    }
    
    void OnDestroy()
    {
        // Unsubscribe from events
        InputModeManager.OnModeChanged -= OnModeChanged;
        
        // Remove button listener
        if (toggleButton != null)
        {
            toggleButton.onClick.RemoveListener(OnToggleClicked);
        }
    }
}
