using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Central coordinator for UI system
/// Manages dependency injection of PlayerStatsManager to all visual feedback controllers
/// </summary>
public class UIManager : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private PlayerStatsManager statsManager;
    
    [Header("Auto-Discovery")]
    [SerializeField] private bool autoDiscoverControllers = true;
    [SerializeField] private bool initializeOnStart = true;
    
    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;
    
    private List<IVisualFeedback> visualControllers = new List<IVisualFeedback>();
    private bool isInitialized = false;
    
    private void Awake()
    {
        // Try to find PlayerStatsManager if not assigned
        if (statsManager == null)
        {
            statsManager = GetComponent<PlayerStatsManager>();
            
            if (statsManager == null)
            {
                statsManager = GetComponentInParent<PlayerStatsManager>();
            }
            
            if (statsManager == null)
            {
                statsManager = FindObjectOfType<PlayerStatsManager>();
            }
            
            if (statsManager == null)
            {
                Debug.LogError("UIManager: PlayerStatsManager not found! Assign it in the inspector or ensure it exists in the scene.");
            }
        }
    }
    
    private void Start()
    {
        if (initializeOnStart)
        {
            InitializeUI();
        }
    }
    
    /// <summary>
    /// Initialize all visual feedback controllers
    /// Call this manually if initializeOnStart is false
    /// </summary>
    public void InitializeUI()
    {
        if (isInitialized)
        {
            if (showDebugLogs)
                Debug.LogWarning("UIManager: Already initialized!");
            return;
        }
        
        if (statsManager == null)
        {
            Debug.LogError("UIManager: Cannot initialize without PlayerStatsManager!");
            return;
        }
        
        if (autoDiscoverControllers)
        {
            DiscoverControllers();
        }
        
        InjectDependencies();
        
        isInitialized = true;
        
        if (showDebugLogs)
            Debug.Log($"UIManager: Initialized {visualControllers.Count} visual controllers");
    }
    
    /// <summary>
    /// Automatically find all IVisualFeedback components in the scene
    /// </summary>
    private void DiscoverControllers()
    {
        visualControllers.Clear();
        
        // Find all IVisualFeedback components on this GameObject and children
        var monoBehaviours = GetComponentsInChildren<MonoBehaviour>(true);
        
        foreach (var mb in monoBehaviours)
        {
            if (mb is IVisualFeedback visualFeedback)
            {
                visualControllers.Add(visualFeedback);
                
                if (showDebugLogs)
                    Debug.Log($"UIManager: Discovered {mb.GetType().Name}");
            }
        }
    }
    
    /// <summary>
    /// Inject PlayerStatsManager into all controllers and initialize them
    /// </summary>
    private void InjectDependencies()
    {
        foreach (var controller in visualControllers)
        {
            try
            {
                controller.Initialize(statsManager);
                
                if (showDebugLogs)
                {
                    var mb = controller as MonoBehaviour;
                    if (mb != null)
                    {
                        Debug.Log($"UIManager: Initialized {mb.GetType().Name}");
                    }
                }
            }
            catch (System.Exception e)
            {
                var mb = controller as MonoBehaviour;
                string name = mb != null ? mb.GetType().Name : "Unknown";
                Debug.LogError($"UIManager: Failed to initialize {name}: {e.Message}");
            }
        }
    }
    
    /// <summary>
    /// Manually register a visual controller
    /// Use this if you create controllers dynamically
    /// </summary>
    public void RegisterController(IVisualFeedback controller)
    {
        if (!visualControllers.Contains(controller))
        {
            visualControllers.Add(controller);
            
            if (isInitialized)
            {
                controller.Initialize(statsManager);
            }
            
            if (showDebugLogs)
            {
                var mb = controller as MonoBehaviour;
                if (mb != null)
                {
                    Debug.Log($"UIManager: Registered {mb.GetType().Name}");
                }
            }
        }
    }
    
    /// <summary>
    /// Unregister a visual controller
    /// </summary>
    public void UnregisterController(IVisualFeedback controller)
    {
        if (visualControllers.Contains(controller))
        {
            controller.Cleanup();
            visualControllers.Remove(controller);
            
            if (showDebugLogs)
            {
                var mb = controller as MonoBehaviour;
                if (mb != null)
                {
                    Debug.Log($"UIManager: Unregistered {mb.GetType().Name}");
                }
            }
        }
    }
    
    /// <summary>
    /// Show all visual effects
    /// </summary>
    public void ShowAllVisuals()
    {
        foreach (var controller in visualControllers)
        {
            controller.Show();
        }
    }
    
    /// <summary>
    /// Hide all visual effects
    /// </summary>
    public void HideAllVisuals()
    {
        foreach (var controller in visualControllers)
        {
            controller.Hide();
        }
    }
    
    /// <summary>
    /// Get a specific controller by type
    /// </summary>
    public T GetController<T>() where T : class, IVisualFeedback
    {
        foreach (var controller in visualControllers)
        {
            if (controller is T typedController)
            {
                return typedController;
            }
        }
        return null;
    }
    
    private void OnDestroy()
    {
        // Cleanup all controllers
        foreach (var controller in visualControllers)
        {
            try
            {
                controller.Cleanup();
            }
            catch (System.Exception e)
            {
                Debug.LogError($"UIManager: Error during cleanup: {e.Message}");
            }
        }
        
        visualControllers.Clear();
    }
    
    #region Debug Methods
    
    [ContextMenu("Reinitialize UI")]
    private void DebugReinitialize()
    {
        isInitialized = false;
        InitializeUI();
    }
    
    [ContextMenu("List Controllers")]
    private void DebugListControllers()
    {
        Debug.Log($"=== UI Controllers ({visualControllers.Count}) ===");
        foreach (var controller in visualControllers)
        {
            var mb = controller as MonoBehaviour;
            if (mb != null)
            {
                Debug.Log($"- {mb.GetType().Name} on {mb.gameObject.name}");
            }
        }
    }
    
    [ContextMenu("Show All")]
    private void DebugShowAll()
    {
        ShowAllVisuals();
    }
    
    [ContextMenu("Hide All")]
    private void DebugHideAll()
    {
        HideAllVisuals();
    }
    
    #endregion
}
