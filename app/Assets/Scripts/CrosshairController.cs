using UnityEngine;
using UnityEngine.UI;

public class CrosshairController : MonoBehaviour
{
    [Header("Crosshair Settings")]
    [Tooltip("The color of the crosshair")]
    public Color crosshairColor = Color.red;
    
    [Tooltip("The size of the crosshair in screen pixels")]
    public float crosshairSize = 20f;
    
    [Header("Mode Colors")]
    [Tooltip("Color for shooting mode")]
    public Color shootingColor = Color.green;
    
    [Tooltip("Color for mine placement mode")]
    public Color minePlacementColor = new Color(1f, 0.5f, 0f, 1f); // Orange
    
    [Tooltip("The thickness of the crosshair circle")]
    [Range(1f, 10f)]
    public float thickness = 2f;
    
    [Tooltip("Optional raycast distance for shooting")]
    public float shootDistance = 100f;
    
    [Tooltip("Layer mask for raycasting")]
    public LayerMask shootLayerMask = -1; // Default to everything
    
    private RectTransform crosshairRect;
    private Image crosshairImage;
    
    void Start()
    {
        // Create the crosshair UI
        CreateCrosshair();
        
        // Subscribe to mode changes
        InputModeManager.OnModeChanged += OnModeChanged;
        
        // Set initial mode visual
        UpdateModeVisuals();
        
        Debug.Log("CrosshairController: Crosshair created and mode listeners setup");
    }
    
    void Update()
    {
        // Crosshair visuals are now handled by mode change events
        // No continuous updates needed
    }
    
    void CreateCrosshair()
    {
        // First check if we already have a canvas in the scene
        Canvas canvas = FindObjectOfType<Canvas>();
        Debug.Log($"CrosshairController: Found existing canvas: {(canvas != null ? canvas.name : "none")}");
        
        // If no canvas exists, create one
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("UI Canvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            
            // Add required components
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
            Debug.Log("CrosshairController: Created new canvas with ScreenSpaceOverlay mode");
        }
        
        // Create crosshair GameObject
        GameObject crosshairObj = new GameObject("Crosshair");
        crosshairObj.transform.SetParent(canvas.transform, false);
        Debug.Log($"CrosshairController: Created crosshair object and parented to {canvas.name}");
        
        // Add UI Image component
        crosshairImage = crosshairObj.AddComponent<Image>();
        crosshairImage.color = crosshairColor;
        Debug.Log($"CrosshairController: Added Image component with color {crosshairColor}");
        
        // Make the image a circle by using a circular sprite or by setting its shape
        // For simplicity, we'll use a circular sprite
        crosshairImage.sprite = CreateCircleSprite();
        Debug.Log("CrosshairController: Created circle sprite for crosshair");
        
        // Set the size
        crosshairRect = crosshairImage.rectTransform;
        crosshairRect.sizeDelta = new Vector2(crosshairSize, crosshairSize);
        
        // Center it on the screen
        crosshairRect.anchorMin = new Vector2(0.5f, 0.5f);
        crosshairRect.anchorMax = new Vector2(0.5f, 0.5f);
        crosshairRect.pivot = new Vector2(0.5f, 0.5f);
        crosshairRect.anchoredPosition = Vector2.zero;
        Debug.Log($"CrosshairController: Positioned crosshair at center with size {crosshairSize}px");
    }
    
    Sprite CreateCircleSprite()
    {
        // Create a simple circle texture
        int resolution = Mathf.Max(64, Mathf.CeilToInt(crosshairSize) * 4); // Higher resolution for smoother circle
        Texture2D texture = new Texture2D(resolution, resolution);
        
        Vector2 center = new Vector2(resolution / 2, resolution / 2);
        float radius = resolution / 2 - thickness / 2;
        float innerRadius = radius - thickness;
        
        // Create a circular ring
        for (int y = 0; y < resolution; y++)
        {
            for (int x = 0; x < resolution; x++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), center);
                
                // Ring shape (hollow circle)
                if (distance < radius && distance > innerRadius)
                {
                    texture.SetPixel(x, y, crosshairColor);
                }
                else
                {
                    texture.SetPixel(x, y, Color.clear);
                }
            }
        }
        
        texture.Apply();
        
        // Create sprite from texture
        return Sprite.Create(texture, new Rect(0, 0, resolution, resolution), new Vector2(0.5f, 0.5f));
    }
    
    public bool TryShoot(out RaycastHit hitInfo)
    {
        hitInfo = new RaycastHit();
        
        // Shoot ray from center of screen
        Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
        
        if (Physics.Raycast(ray, out hitInfo, shootDistance, shootLayerMask))
        {
            // Check if we hit a zombie
            if (hitInfo.collider.CompareTag("Zombie"))
            {
                Debug.Log($"Hit zombie at distance {hitInfo.distance:F2}m");
                return true;
            }
        }
        
        return false;
    }
    
    /// <summary>
    /// Handle mode changes from InputModeManager
    /// </summary>
    void OnModeChanged(InputMode newMode)
    {
        UpdateModeVisuals();
        Debug.Log($"CrosshairController: Mode changed to {newMode}");
    }
    
    /// <summary>
    /// Update visual appearance based on current mode
    /// </summary>
    void UpdateModeVisuals()
    {
        if (crosshairImage == null) return;
        
        if (InputModeManager.Instance != null)
        {
            Color targetColor = InputModeManager.Instance.IsShootingMode() ? shootingColor : minePlacementColor;
            crosshairImage.color = targetColor;
        }
        else
        {
            // Fallback to default color
            crosshairImage.color = crosshairColor;
        }
    }
    
    void OnDestroy()
    {
        // Unsubscribe from events
        InputModeManager.OnModeChanged -= OnModeChanged;
    }
}
