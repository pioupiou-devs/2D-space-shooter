using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System;
using System.Reflection;

/// <summary>
/// Editor script to generate a complete gameplay scene with player, HUD, and all systems wired.
/// Run via menu: Tools > 2D Space Shooter > Generate Gameplay Scene
/// Or via batchmode: Unity -quit -batchmode -projectPath [PATH] -executeMethod GameplaySceneGenerator.GenerateScene
/// </summary>
public static class GameplaySceneGenerator
{
    private const string ScenePath = "Assets/Game/Scenes/GameplayScene.unity";
    private const string ConfigsPath = "Assets/Game/ScriptableObjects/Stats";
    private const string PrefabsPath = "Assets/Game/Prefabs";
    private const string SpritesPath = "Assets/Game/Art/Sprites";

    // Type references resolved at runtime
    private static Type HealthStatsConfigType => GetType("HealthStatsConfig");
    private static Type CombatStatsConfigType => GetType("CombatStatsConfig");
    private static Type DefenseStatsConfigType => GetType("DefenseStatsConfig");
    private static Type MovementStatsConfigType => GetType("MovementStatsConfig");
    private static Type ProgressionStatsConfigType => GetType("ProgressionStatsConfig");
    private static Type SingleShotStrategyType => GetType("SingleShotStrategy");
    private static Type GameScreenManagerType => GetType("GameScreenManager");
    private static Type BulletPoolType => GetType("BulletPool");
    private static Type BulletType => GetType("Bullet");
    private static Type HealthSystemType => GetType("HealthSystem");
    private static Type CombatSystemType => GetType("CombatSystem");
    private static Type DefenseSystemType => GetType("DefenseSystem");
    private static Type MovementSystemType => GetType("MovementSystem");
    private static Type ProgressionSystemType => GetType("ProgressionSystem");
    private static Type PlayerStatsManagerType => GetType("PlayerStatsManager");
    private static Type BoundedMovement2DType => GetType("BoundedMovement2D");
    private static Type WeaponControllerType => GetType("WeaponController");
    private static Type UIManagerType => GetType("UIManager");
    private static Type MinimalHUDControllerType => GetType("MinimalHUDController");
    private static Type TextMeshProUGUIType => GetType("TMPro.TextMeshProUGUI");
    
    // Pause menu types
    private static Type PauseMenuManagerType => GetType("PauseMenuManager");
    private static Type OptionsManagerType => GetType("OptionsManager");
    private static Type ConfirmDialogType => GetType("ConfirmDialog");

    // Built-in sprite names
    private const string SquareSpriteName = "Square";
    private const string TriangleSpriteName = "Triangle";

    private static Type GetType(string typeName)
    {
        // Search in all loaded assemblies
        foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            Type type = assembly.GetType(typeName);
            if (type != null)
                return type;
        }
        Debug.LogWarning($"Type not found: {typeName}");
        return null;
    }

    /// <summary>
    /// Creates a simple white square texture for use as a sprite
    /// </summary>
    private static Sprite CreateSimpleSprite(string name, int size = 32)
    {
        string spritePath = $"{SpritesPath}/{name}.png";
        
        // Check if sprite already exists
        Sprite existingSprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
        if (existingSprite != null)
            return existingSprite;

        // Ensure sprites directory exists
        EnsureDirectoryExists(SpritesPath);

        // Create a simple white texture
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        Color[] pixels = new Color[size * size];
        
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = Color.white;
        }
        
        texture.SetPixels(pixels);
        texture.Apply();

        // Save texture as PNG
        byte[] pngData = texture.EncodeToPNG();
        System.IO.File.WriteAllBytes(spritePath, pngData);
        UnityEngine.Object.DestroyImmediate(texture);

        // Import the texture
        AssetDatabase.ImportAsset(spritePath, ImportAssetOptions.ForceUpdate);

        // Configure as sprite
        TextureImporter importer = AssetImporter.GetAtPath(spritePath) as TextureImporter;
        if (importer != null)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spritePixelsPerUnit = 32;
            importer.filterMode = FilterMode.Point;
            importer.SaveAndReimport();
        }

        Debug.Log($"Created sprite: {spritePath}");
        return AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
    }

    /// <summary>
    /// Creates a triangle sprite programmatically pointing right (for horizontal gameplay)
    /// </summary>
    private static Sprite CreateTriangleSprite()
    {
        string spritePath = $"{SpritesPath}/PlayerTriangle.png";
        
        // Delete existing sprite to recreate with new orientation
        if (AssetDatabase.LoadAssetAtPath<Sprite>(spritePath) != null)
        {
            AssetDatabase.DeleteAsset(spritePath);
        }

        // Ensure sprites directory exists
        EnsureDirectoryExists(SpritesPath);

        // Create a triangle texture
        int size = 32;
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        Color[] pixels = new Color[size * size];
        
        // Fill with transparent
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = Color.clear;
        }

        // Draw a triangle pointing RIGHT (for horizontal gameplay)
        int centerY = size / 2;
        for (int x = 0; x < size; x++)
        {
            // Calculate height of triangle at this column
            float progress = (float)x / size;
            int halfHeight = Mathf.RoundToInt(progress * (size / 2));
            
            for (int y = centerY - halfHeight; y <= centerY + halfHeight; y++)
            {
                if (y >= 0 && y < size)
                {
                    pixels[y * size + x] = Color.white;
                }
            }
        }
        
        texture.SetPixels(pixels);
        texture.Apply();

        // Save texture as PNG
        byte[] pngData = texture.EncodeToPNG();
        System.IO.File.WriteAllBytes(spritePath, pngData);
        UnityEngine.Object.DestroyImmediate(texture);

        // Import the texture
        AssetDatabase.ImportAsset(spritePath, ImportAssetOptions.ForceUpdate);

        // Configure as sprite
        TextureImporter importer = AssetImporter.GetAtPath(spritePath) as TextureImporter;
        if (importer != null)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spritePixelsPerUnit = 32;
            importer.filterMode = FilterMode.Point;
            importer.SaveAndReimport();
        }

        Debug.Log($"Created triangle sprite (pointing right): {spritePath}");
        return AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
    }

    /// <summary>
    /// Gets a sprite - creates one if built-in not available
    /// </summary>
    private static Sprite GetOrCreateSprite(string spriteName)
    {
        // For triangle, skip the built-in lookup (it doesn't exist) and create directly
        if (spriteName.Equals("Triangle", StringComparison.OrdinalIgnoreCase))
        {
            return CreateTriangleSprite();
        }

        // For square/knob, use the built-in Knob sprite
        if (spriteName.Equals("Square", StringComparison.OrdinalIgnoreCase) || 
            spriteName.Equals("Knob", StringComparison.OrdinalIgnoreCase))
        {
            Sprite sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
            if (sprite != null)
                return sprite;
        }

        // Fallback: create a simple square sprite
        return CreateSimpleSprite(spriteName);
    }

    [MenuItem("Tools/2D Space Shooter/Generate Gameplay Scene")]
    public static void GenerateScene()
    {
        // Validate required types exist
        if (!ValidateTypes())
        {
            Debug.LogError("GameplaySceneGenerator: Missing required types. Ensure all scripts are compiled.");
            return;
        }

        // Ensure directories exist FIRST
        EnsureDirectoriesExist();
        AssetDatabase.Refresh();

        // Create or load configs and store their paths
        string healthConfigPath = CreateOrLoadConfigPath(HealthStatsConfigType, "HealthStats");
        string combatConfigPath = CreateOrLoadConfigPath(CombatStatsConfigType, "CombatStats");
        string defenseConfigPath = CreateOrLoadConfigPath(DefenseStatsConfigType, "DefenseStats");
        string movementConfigPath = CreateOrLoadConfigPath(MovementStatsConfigType, "MovementStats");
        string progressionConfigPath = CreateOrLoadConfigPath(ProgressionStatsConfigType, "ProgressionStats");
        string singleShotStrategyPath = CreateOrLoadConfigPath(SingleShotStrategyType, "SingleShotStrategy");

        // Force save all assets before creating scene
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        // Create new scene (this may invalidate object references, so we use paths)
        Scene newScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // Reload configs from paths AFTER scene creation
        var healthConfig = AssetDatabase.LoadAssetAtPath<ScriptableObject>(healthConfigPath);
        var combatConfig = AssetDatabase.LoadAssetAtPath<ScriptableObject>(combatConfigPath);
        var defenseConfig = AssetDatabase.LoadAssetAtPath<ScriptableObject>(defenseConfigPath);
        var movementConfig = AssetDatabase.LoadAssetAtPath<ScriptableObject>(movementConfigPath);
        var progressionConfig = AssetDatabase.LoadAssetAtPath<ScriptableObject>(progressionConfigPath);
        var singleShotStrategy = AssetDatabase.LoadAssetAtPath<ScriptableObject>(singleShotStrategyPath);

        // Validate configs were loaded
        if (healthConfig == null || combatConfig == null || defenseConfig == null || 
            movementConfig == null || progressionConfig == null)
        {
            Debug.LogError("GameplaySceneGenerator: Failed to load config assets after scene creation.");
            Debug.LogError($"Health: {healthConfig != null}, Combat: {combatConfig != null}, Defense: {defenseConfig != null}, Movement: {movementConfig != null}, Progression: {progressionConfig != null}");
            return;
        }

        // Create Main Camera
        GameObject cameraObj = CreateMainCamera();

        // Create EventSystem for UI
        CreateEventSystem();

        // Create GameScreenManager
        GameObject gameScreenManager = CreateGameScreenManager(cameraObj.GetComponent<Camera>());

        // Create BulletPool
        GameObject bulletPool = CreateBulletPool();

        // Create Player
        GameObject player = CreatePlayer(
            healthConfig, combatConfig, defenseConfig, movementConfig, progressionConfig,
            singleShotStrategy, bulletPool, gameScreenManager);

        // Create HUD Canvas with Pause Menu
        GameObject hudCanvas = CreateHUDCanvas(player);

        // Save scene
        EnsureDirectoryExists("Assets/Game/Scenes");
        EditorSceneManager.SaveScene(newScene, ScenePath);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"Gameplay scene generated successfully at: {ScenePath}");
        Debug.Log("Scene contains: Main Camera, EventSystem, GameScreenManager, BulletPool, Player (with all stat systems), HUD Canvas (with MinimalHUDController and PauseMenuManager)");
    }

    private static bool ValidateTypes()
    {
        Type[] requiredTypes = new Type[]
        {
            HealthStatsConfigType, CombatStatsConfigType, DefenseStatsConfigType,
            MovementStatsConfigType, ProgressionStatsConfigType, SingleShotStrategyType,
            GameScreenManagerType, BulletPoolType, BulletType,
            HealthSystemType, CombatSystemType, DefenseSystemType, MovementSystemType, ProgressionSystemType,
            PlayerStatsManagerType, BoundedMovement2DType, WeaponControllerType,
            UIManagerType, MinimalHUDControllerType, TextMeshProUGUIType,
            PauseMenuManagerType, OptionsManagerType, ConfirmDialogType
        };

        foreach (var type in requiredTypes)
        {
            if (type == null)
                return false;
        }
        return true;
    }

    private static void EnsureDirectoriesExist()
    {
        EnsureDirectoryExists(ConfigsPath);
        EnsureDirectoryExists(PrefabsPath);
        EnsureDirectoryExists(SpritesPath);
        EnsureDirectoryExists("Assets/Game/Scenes");
    }

    private static void EnsureDirectoryExists(string path)
    {
        if (!AssetDatabase.IsValidFolder(path))
        {
            string[] parts = path.Split('/');
            string currentPath = parts[0];

            for (int i = 1; i < parts.Length; i++)
            {
                string newPath = currentPath + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(newPath))
                {
                    AssetDatabase.CreateFolder(currentPath, parts[i]);
                }
                currentPath = newPath;
            }
        }
    }

    /// <summary>
    /// Creates or loads a config and returns the asset path (not the object, since scene creation can invalidate references)
    /// </summary>
    private static string CreateOrLoadConfigPath(Type configType, string fileName)
    {
        if (configType == null)
        {
            Debug.LogError($"CreateOrLoadConfigPath: configType is null for {fileName}");
            return null;
        }

        string assetPath = $"{ConfigsPath}/{fileName}.asset";

        // Try to load existing config
        ScriptableObject config = AssetDatabase.LoadAssetAtPath<ScriptableObject>(assetPath);

        if (config == null)
        {
            // Create new config
            config = ScriptableObject.CreateInstance(configType);
            if (config == null)
            {
                Debug.LogError($"Failed to create instance of {configType.Name}");
                return null;
            }

            AssetDatabase.CreateAsset(config, assetPath);
            AssetDatabase.SaveAssets();
            Debug.Log($"Created config: {assetPath}");
        }

        // Verify the asset exists
        if (!System.IO.File.Exists(assetPath.Replace("Assets/", Application.dataPath + "/")))
        {
            // Try alternate check using AssetDatabase
            if (AssetDatabase.LoadAssetAtPath<ScriptableObject>(assetPath) == null)
            {
                Debug.LogError($"Config file does not exist after creation: {assetPath}");
                return null;
            }
        }

        return assetPath;
    }

    // Keep the old method for backward compatibility but mark it as using the new path-based approach
    private static ScriptableObject CreateOrLoadConfig(Type configType, string fileName)
    {
        string path = CreateOrLoadConfigPath(configType, fileName);
        if (string.IsNullOrEmpty(path))
            return null;
        return AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);
    }

    private static GameObject CreateMainCamera()
    {
        GameObject cameraObj = new GameObject("Main Camera");
        cameraObj.tag = "MainCamera";

        Camera camera = cameraObj.AddComponent<Camera>();
        camera.orthographic = true;
        camera.orthographicSize = 5f;
        camera.backgroundColor = new Color(0.1f, 0.1f, 0.15f, 1f); // Dark space blue
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.nearClipPlane = -10f;
        camera.farClipPlane = 10f;

        cameraObj.AddComponent<AudioListener>();

        cameraObj.transform.position = new Vector3(0f, 0f, -10f);

        return cameraObj;
    }

    private static void CreateEventSystem()
    {
        GameObject es = new GameObject("EventSystem");
        es.AddComponent<UnityEngine.EventSystems.EventSystem>();
        
        // Use Input System UI Input Module (new Input System)
        var inputSystemModuleType = Type.GetType("UnityEngine.InputSystem.UI.InputSystemUIInputModule, Unity.InputSystem");
        if (inputSystemModuleType != null)
        {
            es.AddComponent(inputSystemModuleType);
        }
        else
        {
            es.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }
    }

    private static GameObject CreateGameScreenManager(Camera mainCamera)
    {
        GameObject managerObj = new GameObject("GameScreenManager");
        Component manager = managerObj.AddComponent(GameScreenManagerType);

        // Use SerializedObject to set serialized fields
        SerializedObject so = new SerializedObject(manager);
        so.FindProperty("mainCamera").objectReferenceValue = mainCamera;
        so.FindProperty("followCameraSize").boolValue = true;
        so.FindProperty("boundsPadding").floatValue = 0.5f;
        so.ApplyModifiedPropertiesWithoutUndo();

        return managerObj;
    }

    private static GameObject CreateBulletPool()
    {
        GameObject poolObj = new GameObject("BulletPool");
        Component pool = poolObj.AddComponent(BulletPoolType);

        // Create bullet prefab if it doesn't exist
        GameObject bulletPrefab = CreateBulletPrefab();

        // Assign bullet prefab
        SerializedObject so = new SerializedObject(pool);
        so.FindProperty("bulletPrefab").objectReferenceValue = bulletPrefab.GetComponent(BulletType);
        so.FindProperty("initialPoolSize").intValue = 50;
        so.FindProperty("maxPoolSize").intValue = 200;
        so.FindProperty("defaultSpeed").floatValue = 15f;
        so.FindProperty("defaultLifetime").floatValue = 5f;
        so.ApplyModifiedPropertiesWithoutUndo();

        return poolObj;
    }

    private static GameObject CreateBulletPrefab()
    {
        string prefabPath = $"{PrefabsPath}/Bullet.prefab";

        GameObject existingPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        if (existingPrefab != null)
        {
            return existingPrefab;
        }

        // Create bullet GameObject
        GameObject bulletObj = new GameObject("Bullet");

        // Add Bullet component (also adds Rigidbody2D via RequireComponent)
        bulletObj.AddComponent(BulletType);

        // Add sprite renderer with a simple sprite
        SpriteRenderer sr = bulletObj.AddComponent<SpriteRenderer>();
        sr.sprite = GetOrCreateSprite("Square");
        sr.color = Color.yellow;
        sr.sortingOrder = 5;

        // Scale down the bullet to appropriate size
        bulletObj.transform.localScale = new Vector3(0.2f, 0.2f, 1f);

        // Add circle collider
        CircleCollider2D collider = bulletObj.AddComponent<CircleCollider2D>();
        collider.radius = 0.1f;
        collider.isTrigger = true;

        // Configure Rigidbody2D
        Rigidbody2D rb = bulletObj.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.gravityScale = 0f;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        }

        // Save as prefab
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(bulletObj, prefabPath);
        UnityEngine.Object.DestroyImmediate(bulletObj);

        Debug.Log($"Created bullet prefab: {prefabPath}");
        return prefab;
    }

    private static GameObject CreatePlayer(
        ScriptableObject healthConfig,
        ScriptableObject combatConfig,
        ScriptableObject defenseConfig,
        ScriptableObject movementConfig,
        ScriptableObject progressionConfig,
        ScriptableObject shootingStrategy,
        GameObject bulletPoolObj,
        GameObject gameScreenManagerObj)
    {
        GameObject playerObj = new GameObject("Player");
        playerObj.tag = "Player";
        playerObj.layer = LayerMask.NameToLayer("Default");

        // Add sprite renderer with triangle sprite (pointing right for horizontal gameplay)
        SpriteRenderer sr = playerObj.AddComponent<SpriteRenderer>();
        sr.sprite = GetOrCreateSprite("Triangle");
        sr.color = Color.cyan;
        sr.sortingOrder = 10;

        // Add Rigidbody2D
        Rigidbody2D rb = playerObj.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.bodyType = RigidbodyType2D.Kinematic;

        // Add collider
        BoxCollider2D collider = playerObj.AddComponent<BoxCollider2D>();
        collider.size = new Vector2(0.8f, 0.8f);

        // Add stat systems (order matters - PlayerStatsManager requires the others)
        Component healthSystem = playerObj.AddComponent(HealthSystemType);
        Component combatSystem = playerObj.AddComponent(CombatSystemType);
        Component defenseSystem = playerObj.AddComponent(DefenseSystemType);
        Component movementSystem = playerObj.AddComponent(MovementSystemType);
        Component progressionSystem = playerObj.AddComponent(ProgressionSystemType);

        // Assign configs using SerializedObject
        AssignConfig(healthSystem, healthConfig);
        AssignConfig(combatSystem, combatConfig);
        AssignConfig(defenseSystem, defenseConfig);
        AssignConfig(movementSystem, movementConfig);
        AssignConfig(progressionSystem, progressionConfig);

        // Add PlayerStatsManager
        Component statsManager = playerObj.AddComponent(PlayerStatsManagerType);

        // Add BoundedMovement2D
        Component movement = playerObj.AddComponent(BoundedMovement2DType);
        SerializedObject movementSO = new SerializedObject(movement);
        movementSO.FindProperty("gameScreenManager").objectReferenceValue = gameScreenManagerObj.GetComponent(GameScreenManagerType);
        movementSO.FindProperty("moveSpeed").floatValue = 5f;
        movementSO.ApplyModifiedPropertiesWithoutUndo();

        // Create fire point - positioned to the RIGHT of player (horizontal gameplay)
        GameObject firePoint = new GameObject("FirePoint");
        firePoint.transform.SetParent(playerObj.transform);
        firePoint.transform.localPosition = new Vector3(0.5f, 0f, 0f);

        // Add WeaponController
        Component weaponController = playerObj.AddComponent(WeaponControllerType);
        SerializedObject weaponSO = new SerializedObject(weaponController);
        weaponSO.FindProperty("bulletPool").objectReferenceValue = bulletPoolObj.GetComponent(BulletPoolType);
        weaponSO.FindProperty("firePoint").objectReferenceValue = firePoint.transform;
        weaponSO.FindProperty("statsManager").objectReferenceValue = statsManager;
        weaponSO.FindProperty("strategyObject").objectReferenceValue = shootingStrategy;
        weaponSO.FindProperty("defaultDirection").vector2Value = Vector2.right; // Shoot RIGHT (horizontal)
        weaponSO.FindProperty("autoFire").boolValue = false;
        weaponSO.FindProperty("useMouseAim").boolValue = false;
        weaponSO.ApplyModifiedPropertiesWithoutUndo();

        // Set initial position - LEFT side of screen (horizontal gameplay)
        playerObj.transform.position = new Vector3(-6f, 0f, 0f);

        return playerObj;
    }

    private static void AssignConfig(Component component, ScriptableObject config)
    {
        if (component == null)
        {
            Debug.LogError("AssignConfig: Component is null");
            return;
        }

        if (config == null)
        {
            Debug.LogError($"AssignConfig: Config is null for component {component.GetType().Name}");
            return;
        }

        SerializedObject so = new SerializedObject(component);
        SerializedProperty configProp = so.FindProperty("config");
        if (configProp != null)
        {
            configProp.objectReferenceValue = config;
            so.ApplyModifiedPropertiesWithoutUndo();
            Debug.Log($"Assigned {config.name} to {component.GetType().Name}");
        }
        else
        {
            Debug.LogError($"AssignConfig: Could not find 'config' property on {component.GetType().Name}");
        }
    }

    private static GameObject CreateHUDCanvas(GameObject player)
    {
        Component statsManager = player.GetComponent(PlayerStatsManagerType);

        // Create Canvas
        GameObject canvasObj = new GameObject("HUD Canvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        canvasObj.AddComponent<GraphicRaycaster>();

        // Add UIManager directly to the Canvas so it can discover child controllers
        Component uiManager = canvasObj.AddComponent(UIManagerType);

        SerializedObject uiSO = new SerializedObject(uiManager);
        uiSO.FindProperty("statsManager").objectReferenceValue = statsManager;
        uiSO.FindProperty("autoDiscoverControllers").boolValue = true;
        uiSO.FindProperty("initializeOnStart").boolValue = true;
        uiSO.FindProperty("showDebugLogs").boolValue = true;
        uiSO.ApplyModifiedPropertiesWithoutUndo();

        // Create HUD Panel (child of canvas, so UIManager can discover it)
        GameObject hudPanel = CreateHUDPanel(canvasObj.transform);

        // Create Pause Menu
        CreatePauseMenu(canvasObj.transform);

        return canvasObj;
    }

    #region Pause Menu Creation

    private static void CreatePauseMenu(Transform parent)
    {
        // Create Pause Menu Panel (container for all pause UI)
        GameObject pauseMenuPanel = new GameObject("Pause Menu Panel");
        pauseMenuPanel.transform.SetParent(parent);
        
        RectTransform pauseRect = pauseMenuPanel.AddComponent<RectTransform>();
        pauseRect.anchorMin = Vector2.zero;
        pauseRect.anchorMax = Vector2.one;
        pauseRect.offsetMin = Vector2.zero;
        pauseRect.offsetMax = Vector2.zero;

        // Add PauseMenuManager
        Component pauseManager = pauseMenuPanel.AddComponent(PauseMenuManagerType);

        // Create Overlay (semi-transparent background)
        GameObject overlay = CreateOverlay(pauseMenuPanel.transform);

        // Create Pause Modal
        GameObject pauseModal = CreatePauseModal(pauseMenuPanel.transform);

        // Create Options Panel (shared component)
        GameObject optionsPanel = CreateOptionsPanel(pauseMenuPanel.transform);

        // Create Confirm Dialog (shared component)
        GameObject confirmDialog = CreateConfirmDialog(pauseMenuPanel.transform);

        // Wire up PauseMenuManager
        SerializedObject pauseSO = new SerializedObject(pauseManager);
        pauseSO.FindProperty("pauseMenuPanel").objectReferenceValue = pauseModal;
        pauseSO.FindProperty("overlayPanel").objectReferenceValue = overlay;
        pauseSO.FindProperty("optionsPanel").objectReferenceValue = optionsPanel;
        pauseSO.FindProperty("confirmDialog").objectReferenceValue = confirmDialog.GetComponent(ConfirmDialogType);
        
        // Wire buttons
        Button resumeBtn = pauseModal.transform.Find("Button Container/Resume Button")?.GetComponent<Button>();
        Button optionsBtn = pauseModal.transform.Find("Button Container/Options Button")?.GetComponent<Button>();
        Button mainMenuBtn = pauseModal.transform.Find("Button Container/Main Menu Button")?.GetComponent<Button>();
        Button quitBtn = pauseModal.transform.Find("Button Container/Quit Button")?.GetComponent<Button>();
        Button optionsBackBtn = optionsPanel.transform.Find("Back Button")?.GetComponent<Button>();

        pauseSO.FindProperty("resumeButton").objectReferenceValue = resumeBtn;
        pauseSO.FindProperty("optionsButton").objectReferenceValue = optionsBtn;
        pauseSO.FindProperty("mainMenuButton").objectReferenceValue = mainMenuBtn;
        pauseSO.FindProperty("quitButton").objectReferenceValue = quitBtn;
        pauseSO.FindProperty("optionsBackButton").objectReferenceValue = optionsBackBtn;
        pauseSO.FindProperty("mainMenuSceneName").stringValue = "MainMenuScene";
        pauseSO.ApplyModifiedPropertiesWithoutUndo();

        // Start with pause menu hidden
        overlay.SetActive(false);
        pauseModal.SetActive(false);
        optionsPanel.SetActive(false);
        confirmDialog.SetActive(false);
    }

    private static GameObject CreateOverlay(Transform parent)
    {
        GameObject overlay = new GameObject("Overlay");
        overlay.transform.SetParent(parent);

        RectTransform rect = overlay.AddComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        Image img = overlay.AddComponent<Image>();
        img.color = new Color(0f, 0f, 0f, 0.7f);

        return overlay;
    }

    private static GameObject CreatePauseModal(Transform parent)
    {
        GameObject modal = new GameObject("Pause Modal");
        modal.transform.SetParent(parent);

        RectTransform rect = modal.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(400, 450);
        rect.anchoredPosition = Vector2.zero;

        Image bg = modal.AddComponent<Image>();
        bg.color = new Color(0.08f, 0.08f, 0.13f, 0.98f);

        // Title
        GameObject title = new GameObject("Title");
        title.transform.SetParent(modal.transform);
        RectTransform titleRect = title.AddComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.5f, 1f);
        titleRect.anchorMax = new Vector2(0.5f, 1f);
        titleRect.sizeDelta = new Vector2(300, 60);
        titleRect.anchoredPosition = new Vector2(0, -50);

        Component titleText = title.AddComponent(TextMeshProUGUIType);
        SetTMPText(titleText, "PAUSED", 42, 1);
        SetTMPColor(titleText, Color.cyan);

        // Button container
        GameObject btnContainer = new GameObject("Button Container");
        btnContainer.transform.SetParent(modal.transform);
        RectTransform btnRect = btnContainer.AddComponent<RectTransform>();
        btnRect.anchorMin = new Vector2(0.5f, 0.5f);
        btnRect.anchorMax = new Vector2(0.5f, 0.5f);
        btnRect.sizeDelta = new Vector2(280, 280);
        btnRect.anchoredPosition = new Vector2(0, -20);

        VerticalLayoutGroup layout = btnContainer.AddComponent<VerticalLayoutGroup>();
        layout.spacing = 15;
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childControlWidth = true;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = true;

        // Buttons
        CreateMenuButton("Resume Button", btnContainer.transform, "RESUME", new Color(0.2f, 0.7f, 0.2f), 55);
        CreateMenuButton("Options Button", btnContainer.transform, "OPTIONS", new Color(0.3f, 0.5f, 0.8f), 55);
        CreateMenuButton("Main Menu Button", btnContainer.transform, "MAIN MENU", new Color(0.8f, 0.5f, 0.2f), 55);
        CreateMenuButton("Quit Button", btnContainer.transform, "QUIT", new Color(0.7f, 0.3f, 0.3f), 55);

        // Hint text
        GameObject hint = new GameObject("Hint");
        hint.transform.SetParent(modal.transform);
        RectTransform hintRect = hint.AddComponent<RectTransform>();
        hintRect.anchorMin = new Vector2(0.5f, 0f);
        hintRect.anchorMax = new Vector2(0.5f, 0f);
        hintRect.sizeDelta = new Vector2(300, 30);
        hintRect.anchoredPosition = new Vector2(0, 25);

        Component hintText = hint.AddComponent(TextMeshProUGUIType);
        SetTMPText(hintText, "[Press ESC to resume]", 16, 1);
        SetTMPColor(hintText, new Color(0.5f, 0.5f, 0.5f));

        return modal;
    }

    private static GameObject CreateOptionsPanel(Transform parent)
    {
        GameObject panel = new GameObject("Options Panel");
        panel.transform.SetParent(parent);

        RectTransform rect = panel.AddComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        Image bg = panel.AddComponent<Image>();
        bg.color = new Color(0.1f, 0.1f, 0.15f, 0.98f);

        // Add OptionsManager
        Component optionsManager = panel.AddComponent(OptionsManagerType);

        // Title
        GameObject title = new GameObject("Title");
        title.transform.SetParent(panel.transform);
        RectTransform titleRect = title.AddComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.5f, 1f);
        titleRect.anchorMax = new Vector2(0.5f, 1f);
        titleRect.sizeDelta = new Vector2(300, 60);
        titleRect.anchoredPosition = new Vector2(0, -50);

        Component titleText = title.AddComponent(TextMeshProUGUIType);
        SetTMPText(titleText, "OPTIONS", 42, 1);
        SetTMPColor(titleText, Color.white);

        // Create scroll view for options content
        GameObject scrollView = CreateOptionsScrollView(panel.transform);

        // Back button
        GameObject backBtn = CreateMenuButton("Back Button", panel.transform, "BACK", new Color(0.5f, 0.5f, 0.5f), 50);
        RectTransform backRect = backBtn.GetComponent<RectTransform>();
        backRect.anchorMin = new Vector2(0.5f, 0.05f);
        backRect.anchorMax = new Vector2(0.5f, 0.05f);
        backRect.sizeDelta = new Vector2(150, 50);
        backRect.anchoredPosition = Vector2.zero;

        // Wire OptionsManager
        WireOptionsManager(optionsManager, scrollView);

        return panel;
    }

    private static GameObject CreateOptionsScrollView(Transform parent)
    {
        GameObject scrollView = new GameObject("Scroll View");
        scrollView.transform.SetParent(parent);
        RectTransform scrollRect = scrollView.AddComponent<RectTransform>();
        scrollRect.anchorMin = new Vector2(0.1f, 0.12f);
        scrollRect.anchorMax = new Vector2(0.9f, 0.85f);
        scrollRect.offsetMin = Vector2.zero;
        scrollRect.offsetMax = Vector2.zero;

        ScrollRect scroll = scrollView.AddComponent<ScrollRect>();
        scroll.horizontal = false;
        scroll.vertical = true;
        scroll.movementType = ScrollRect.MovementType.Clamped;
        scroll.scrollSensitivity = 30;

        // Viewport
        GameObject viewport = new GameObject("Viewport");
        viewport.transform.SetParent(scrollView.transform);
        RectTransform viewportRect = viewport.AddComponent<RectTransform>();
        viewportRect.anchorMin = Vector2.zero;
        viewportRect.anchorMax = Vector2.one;
        viewportRect.offsetMin = Vector2.zero;
        viewportRect.offsetMax = new Vector2(-15, 0);
        
        Image viewportImage = viewport.AddComponent<Image>();
        viewportImage.color = new Color(0, 0, 0, 0.01f);
        Mask mask = viewport.AddComponent<Mask>();
        mask.showMaskGraphic = false;

        // Content
        GameObject content = new GameObject("Content");
        content.transform.SetParent(viewport.transform);
        RectTransform contentRect = content.AddComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0, 1);
        contentRect.anchorMax = new Vector2(1, 1);
        contentRect.pivot = new Vector2(0.5f, 1);
        contentRect.offsetMin = Vector2.zero;
        contentRect.offsetMax = Vector2.zero;
        contentRect.sizeDelta = new Vector2(0, 0);

        // Add layout components
        VerticalLayoutGroup vlg = content.AddComponent<VerticalLayoutGroup>();
        vlg.spacing = 5;
        vlg.padding = new RectOffset(20, 20, 10, 10);
        vlg.childAlignment = TextAnchor.UpperCenter;
        vlg.childControlWidth = true;
        vlg.childControlHeight = false;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;

        ContentSizeFitter csf = content.AddComponent<ContentSizeFitter>();
        csf.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        scroll.viewport = viewportRect;
        scroll.content = contentRect;

        // Audio Section
        CreateSectionHeader("AUDIO", content.transform);
        CreateSliderRow("Master Volume", content.transform);
        CreateSliderRow("Music Volume", content.transform);
        CreateSliderRow("SFX Volume", content.transform);
        
        CreateSpacer(content.transform, 15);

        // Display Section
        CreateSectionHeader("DISPLAY", content.transform);
        CreateToggleRow("Fullscreen", content.transform);
        CreateToggleRow("V-Sync", content.transform);

        CreateSpacer(content.transform, 15);

        // Controls Section
        CreateSectionHeader("CONTROLS", content.transform);
        CreateKeyBindRow("Move Up", "W", content.transform);
        CreateKeyBindRow("Move Down", "S", content.transform);
        CreateKeyBindRow("Move Left", "A", content.transform);
        CreateKeyBindRow("Move Right", "D", content.transform);
        CreateKeyBindRow("Fire", "Space", content.transform);
        CreateKeyBindRow("Pause", "Escape", content.transform);

        // Vertical Scrollbar
        GameObject scrollbar = new GameObject("Scrollbar Vertical");
        scrollbar.transform.SetParent(scrollView.transform);
        RectTransform scrollbarRect = scrollbar.AddComponent<RectTransform>();
        scrollbarRect.anchorMin = new Vector2(1, 0);
        scrollbarRect.anchorMax = new Vector2(1, 1);
        scrollbarRect.offsetMin = new Vector2(-12, 0);
        scrollbarRect.offsetMax = Vector2.zero;
        scrollbarRect.sizeDelta = new Vector2(12, 0);

        Image scrollbarBg = scrollbar.AddComponent<Image>();
        scrollbarBg.color = new Color(0.2f, 0.2f, 0.2f, 0.5f);

        Scrollbar scrollbarComp = scrollbar.AddComponent<Scrollbar>();
        scrollbarComp.direction = Scrollbar.Direction.BottomToTop;

        GameObject slidingArea = new GameObject("Sliding Area");
        slidingArea.transform.SetParent(scrollbar.transform);
        RectTransform slidingRect = slidingArea.AddComponent<RectTransform>();
        slidingRect.anchorMin = Vector2.zero;
        slidingRect.anchorMax = Vector2.one;
        slidingRect.offsetMin = Vector2.zero;
        slidingRect.offsetMax = Vector2.zero;

        GameObject handle = new GameObject("Handle");
        handle.transform.SetParent(slidingArea.transform);
        RectTransform handleRect = handle.AddComponent<RectTransform>();
        handleRect.anchorMin = Vector2.zero;
        handleRect.anchorMax = new Vector2(1, 1);
        handleRect.offsetMin = Vector2.zero;
        handleRect.offsetMax = Vector2.zero;
        Image handleImage = handle.AddComponent<Image>();
        handleImage.color = Color.cyan;

        scrollbarComp.handleRect = handleRect;
        scrollbarComp.targetGraphic = handleImage;
        scroll.verticalScrollbar = scrollbarComp;
        scroll.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHideAndExpandViewport;

        return scrollView;
    }

    private static void CreateSectionHeader(string title, Transform parent)
    {
        GameObject header = new GameObject($"{title} Header");
        header.transform.SetParent(parent);
        RectTransform headerRect = header.AddComponent<RectTransform>();
        headerRect.sizeDelta = new Vector2(0, 35);

        LayoutElement le = header.AddComponent<LayoutElement>();
        le.minHeight = 35;
        le.preferredHeight = 35;

        Component headerTmp = header.AddComponent(TextMeshProUGUIType);
        SetTMPText(headerTmp, title, 22, 0);
        SetTMPColor(headerTmp, Color.cyan);
    }

    private static void CreateSpacer(Transform parent, float height)
    {
        GameObject spacer = new GameObject("Spacer");
        spacer.transform.SetParent(parent);
        RectTransform spacerRect = spacer.AddComponent<RectTransform>();
        spacerRect.sizeDelta = new Vector2(0, height);

        LayoutElement le = spacer.AddComponent<LayoutElement>();
        le.minHeight = height;
        le.preferredHeight = height;
    }

    private static void CreateSliderRow(string label, Transform parent)
    {
        float rowHeight = 50;

        GameObject row = new GameObject($"{label} Row");
        row.transform.SetParent(parent);
        RectTransform rowRect = row.AddComponent<RectTransform>();
        rowRect.sizeDelta = new Vector2(0, rowHeight);

        LayoutElement le = row.AddComponent<LayoutElement>();
        le.minHeight = rowHeight;
        le.preferredHeight = rowHeight;

        // Label
        GameObject labelObj = new GameObject("Label");
        labelObj.transform.SetParent(row.transform);
        RectTransform labelRect = labelObj.AddComponent<RectTransform>();
        labelRect.anchorMin = new Vector2(0, 0);
        labelRect.anchorMax = new Vector2(0.3f, 1);
        labelRect.offsetMin = Vector2.zero;
        labelRect.offsetMax = Vector2.zero;

        Component labelTmp = labelObj.AddComponent(TextMeshProUGUIType);
        SetTMPText(labelTmp, label, 18, 0);
        SetTMPColor(labelTmp, Color.white);

        // Slider
        GameObject sliderObj = new GameObject("Slider");
        sliderObj.transform.SetParent(row.transform);
        RectTransform sliderRect = sliderObj.AddComponent<RectTransform>();
        sliderRect.anchorMin = new Vector2(0.32f, 0.2f);
        sliderRect.anchorMax = new Vector2(0.82f, 0.8f);
        sliderRect.offsetMin = Vector2.zero;
        sliderRect.offsetMax = Vector2.zero;

        Slider slider = sliderObj.AddComponent<Slider>();
        slider.minValue = 0;
        slider.maxValue = 1;
        slider.value = 1;

        // Slider Background
        GameObject sliderBg = new GameObject("Background");
        sliderBg.transform.SetParent(sliderObj.transform);
        RectTransform bgRect = sliderBg.AddComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;
        Image bgImage = sliderBg.AddComponent<Image>();
        bgImage.color = new Color(0.2f, 0.2f, 0.2f);

        // Fill Area
        GameObject fillArea = new GameObject("Fill Area");
        fillArea.transform.SetParent(sliderObj.transform);
        RectTransform fillAreaRect = fillArea.AddComponent<RectTransform>();
        fillAreaRect.anchorMin = new Vector2(0, 0.25f);
        fillAreaRect.anchorMax = new Vector2(1, 0.75f);
        fillAreaRect.offsetMin = new Vector2(5, 0);
        fillAreaRect.offsetMax = new Vector2(-5, 0);

        GameObject fill = new GameObject("Fill");
        fill.transform.SetParent(fillArea.transform);
        RectTransform fillRect = fill.AddComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = new Vector2(0, 1);
        fillRect.offsetMin = Vector2.zero;
        fillRect.offsetMax = Vector2.zero;
        Image fillImage = fill.AddComponent<Image>();
        fillImage.color = Color.cyan;
        slider.fillRect = fillRect;

        // Handle
        GameObject handleArea = new GameObject("Handle Slide Area");
        handleArea.transform.SetParent(sliderObj.transform);
        RectTransform handleAreaRect = handleArea.AddComponent<RectTransform>();
        handleAreaRect.anchorMin = Vector2.zero;
        handleAreaRect.anchorMax = Vector2.one;
        handleAreaRect.offsetMin = new Vector2(10, 0);
        handleAreaRect.offsetMax = new Vector2(-10, 0);

        GameObject handle = new GameObject("Handle");
        handle.transform.SetParent(handleArea.transform);
        RectTransform handleRect = handle.AddComponent<RectTransform>();
        handleRect.anchorMin = new Vector2(0, 0);
        handleRect.anchorMax = new Vector2(0, 1);
        handleRect.sizeDelta = new Vector2(20, 0);
        Image handleImage = handle.AddComponent<Image>();
        handleImage.color = Color.white;
        slider.handleRect = handleRect;
        slider.targetGraphic = handleImage;

        // Value text
        GameObject valueObj = new GameObject("Value");
        valueObj.transform.SetParent(row.transform);
        RectTransform valueRect = valueObj.AddComponent<RectTransform>();
        valueRect.anchorMin = new Vector2(0.85f, 0);
        valueRect.anchorMax = new Vector2(1, 1);
        valueRect.offsetMin = Vector2.zero;
        valueRect.offsetMax = Vector2.zero;

        Component valueTmp = valueObj.AddComponent(TextMeshProUGUIType);
        SetTMPText(valueTmp, "100%", 16, 1);
        SetTMPColor(valueTmp, Color.white);
    }

    private static void CreateToggleRow(string label, Transform parent)
    {
        float rowHeight = 45;

        GameObject row = new GameObject($"{label} Row");
        row.transform.SetParent(parent);
        RectTransform rowRect = row.AddComponent<RectTransform>();
        rowRect.sizeDelta = new Vector2(0, rowHeight);

        LayoutElement le = row.AddComponent<LayoutElement>();
        le.minHeight = rowHeight;
        le.preferredHeight = rowHeight;

        // Label
        GameObject labelObj = new GameObject("Label");
        labelObj.transform.SetParent(row.transform);
        RectTransform labelRect = labelObj.AddComponent<RectTransform>();
        labelRect.anchorMin = new Vector2(0, 0);
        labelRect.anchorMax = new Vector2(0.7f, 1);
        labelRect.offsetMin = Vector2.zero;
        labelRect.offsetMax = Vector2.zero;

        Component labelTmp = labelObj.AddComponent(TextMeshProUGUIType);
        SetTMPText(labelTmp, label, 18, 0);
        SetTMPColor(labelTmp, Color.white);

        // Toggle
        GameObject toggleObj = new GameObject("Toggle");
        toggleObj.transform.SetParent(row.transform);
        RectTransform toggleRect = toggleObj.AddComponent<RectTransform>();
        toggleRect.anchorMin = new Vector2(0.75f, 0.1f);
        toggleRect.anchorMax = new Vector2(0.85f, 0.9f);
        toggleRect.offsetMin = Vector2.zero;
        toggleRect.offsetMax = Vector2.zero;

        Toggle toggle = toggleObj.AddComponent<Toggle>();

        GameObject background = new GameObject("Background");
        background.transform.SetParent(toggleObj.transform);
        RectTransform bgRect = background.AddComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;
        Image bgImage = background.AddComponent<Image>();
        bgImage.color = new Color(0.25f, 0.25f, 0.25f);

        GameObject checkmark = new GameObject("Checkmark");
        checkmark.transform.SetParent(background.transform);
        RectTransform checkRect = checkmark.AddComponent<RectTransform>();
        checkRect.anchorMin = new Vector2(0.15f, 0.15f);
        checkRect.anchorMax = new Vector2(0.85f, 0.85f);
        checkRect.offsetMin = Vector2.zero;
        checkRect.offsetMax = Vector2.zero;
        Image checkImage = checkmark.AddComponent<Image>();
        checkImage.color = Color.cyan;

        toggle.targetGraphic = bgImage;
        toggle.graphic = checkImage;
        toggle.isOn = true;
    }

    private static void CreateKeyBindRow(string label, string defaultKey, Transform parent)
    {
        float rowHeight = 40;

        GameObject row = new GameObject($"{label} Row");
        row.transform.SetParent(parent);
        RectTransform rowRect = row.AddComponent<RectTransform>();
        rowRect.sizeDelta = new Vector2(0, rowHeight);

        LayoutElement le = row.AddComponent<LayoutElement>();
        le.minHeight = rowHeight;
        le.preferredHeight = rowHeight;

        // Label
        GameObject labelObj = new GameObject("Label");
        labelObj.transform.SetParent(row.transform);
        RectTransform labelRect = labelObj.AddComponent<RectTransform>();
        labelRect.anchorMin = new Vector2(0, 0);
        labelRect.anchorMax = new Vector2(0.5f, 1);
        labelRect.offsetMin = Vector2.zero;
        labelRect.offsetMax = Vector2.zero;

        Component labelTmp = labelObj.AddComponent(TextMeshProUGUIType);
        SetTMPText(labelTmp, label, 16, 0);
        SetTMPColor(labelTmp, Color.white);

        // Key Button
        GameObject keyBtn = new GameObject("KeyButton");
        keyBtn.transform.SetParent(row.transform);
        RectTransform keyRect = keyBtn.AddComponent<RectTransform>();
        keyRect.anchorMin = new Vector2(0.6f, 0.1f);
        keyRect.anchorMax = new Vector2(0.9f, 0.9f);
        keyRect.offsetMin = Vector2.zero;
        keyRect.offsetMax = Vector2.zero;

        Image btnBg = keyBtn.AddComponent<Image>();
        btnBg.color = new Color(0.25f, 0.25f, 0.3f);

        Button btn = keyBtn.AddComponent<Button>();
        ColorBlock colors = btn.colors;
        colors.highlightedColor = new Color(0.35f, 0.35f, 0.4f);
        colors.pressedColor = new Color(0.2f, 0.2f, 0.25f);
        btn.colors = colors;

        GameObject keyTextObj = new GameObject("KeyText");
        keyTextObj.transform.SetParent(keyBtn.transform);
        RectTransform keyTextRect = keyTextObj.AddComponent<RectTransform>();
        keyTextRect.anchorMin = Vector2.zero;
        keyTextRect.anchorMax = Vector2.one;
        keyTextRect.offsetMin = Vector2.zero;
        keyTextRect.offsetMax = Vector2.zero;

        Component keyTmp = keyTextObj.AddComponent(TextMeshProUGUIType);
        SetTMPText(keyTmp, defaultKey, 14, 1);
        SetTMPColor(keyTmp, Color.white);
    }

    private static void WireOptionsManager(Component manager, GameObject scrollView)
    {
        SerializedObject so = new SerializedObject(manager);
        
        Transform content = scrollView.transform.Find("Viewport/Content");
        if (content == null)
        {
            Debug.LogWarning("Could not find scroll view content for OptionsManager wiring");
            return;
        }

        // Wire sliders
        var masterRow = content.Find("Master Volume Row");
        var musicRow = content.Find("Music Volume Row");
        var sfxRow = content.Find("SFX Volume Row");

        if (masterRow != null) 
            so.FindProperty("masterVolumeSlider").objectReferenceValue = masterRow.Find("Slider")?.GetComponent<Slider>();
        if (musicRow != null) 
            so.FindProperty("musicVolumeSlider").objectReferenceValue = musicRow.Find("Slider")?.GetComponent<Slider>();
        if (sfxRow != null) 
            so.FindProperty("sfxVolumeSlider").objectReferenceValue = sfxRow.Find("Slider")?.GetComponent<Slider>();

        // Wire toggles
        var fullscreenRow = content.Find("Fullscreen Row");
        var vsyncRow = content.Find("V-Sync Row");

        if (fullscreenRow != null) 
            so.FindProperty("fullscreenToggle").objectReferenceValue = fullscreenRow.Find("Toggle")?.GetComponent<Toggle>();
        if (vsyncRow != null) 
            so.FindProperty("vsyncToggle").objectReferenceValue = vsyncRow.Find("Toggle")?.GetComponent<Toggle>();

        // Wire key binding buttons
        var moveUpRow = content.Find("Move Up Row");
        var moveDownRow = content.Find("Move Down Row");
        var moveLeftRow = content.Find("Move Left Row");
        var moveRightRow = content.Find("Move Right Row");
        var fireRow = content.Find("Fire Row");
        var pauseRow = content.Find("Pause Row");

        if (moveUpRow != null)
            so.FindProperty("moveUpButton").objectReferenceValue = moveUpRow.Find("KeyButton")?.GetComponent<Button>();
        if (moveDownRow != null)
            so.FindProperty("moveDownButton").objectReferenceValue = moveDownRow.Find("KeyButton")?.GetComponent<Button>();
        if (moveLeftRow != null)
            so.FindProperty("moveLeftButton").objectReferenceValue = moveLeftRow.Find("KeyButton")?.GetComponent<Button>();
        if (moveRightRow != null)
            so.FindProperty("moveRightButton").objectReferenceValue = moveRightRow.Find("KeyButton")?.GetComponent<Button>();
        if (fireRow != null)
            so.FindProperty("fireButton").objectReferenceValue = fireRow.Find("KeyButton")?.GetComponent<Button>();
        if (pauseRow != null)
            so.FindProperty("pauseButton").objectReferenceValue = pauseRow.Find("KeyButton")?.GetComponent<Button>();

        so.ApplyModifiedPropertiesWithoutUndo();
    }
    #endregion

    #region HUD Creation

    private static GameObject CreateHUDPanel(Transform parent)
    {
        // Create HUD Panel
        GameObject hudPanel = new GameObject("HUD Panel");
        hudPanel.transform.SetParent(parent);

        RectTransform hudRect = hudPanel.AddComponent<RectTransform>();
        hudRect.anchorMin = Vector2.zero;
        hudRect.anchorMax = Vector2.one;
        hudRect.offsetMin = Vector2.zero;
        hudRect.offsetMax = Vector2.zero;

        // Add CanvasGroup for dimming when paused
        hudPanel.AddComponent<CanvasGroup>();

        // Add MinimalHUDController
        Component hudController = hudPanel.AddComponent(MinimalHUDControllerType);

        // Create Lives Text (top-left)
        GameObject livesObj = CreateTextElement("Lives Text", hudPanel.transform, new Vector2(0, 1), new Vector2(0, 1), new Vector2(120, 50), new Vector2(80, -40));
        Component livesText = livesObj.GetComponent(TextMeshProUGUIType);
        SetTMPText(livesText, "? 3", 32, 0); // 0 = Left

        // Create Score Text (top-center)
        GameObject scoreObj = CreateTextElement("Score Text", hudPanel.transform, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(200, 50), new Vector2(0, -40));
        Component scoreText = scoreObj.GetComponent(TextMeshProUGUIType);
        SetTMPText(scoreText, "0", 36, 1); // 1 = Center

        // Create Level Text (top-right)
        GameObject levelObj = CreateTextElement("Level Text", hudPanel.transform, new Vector2(1, 1), new Vector2(1, 1), new Vector2(120, 50), new Vector2(-80, -40));
        Component levelText = levelObj.GetComponent(TextMeshProUGUIType);
        SetTMPText(levelText, "Lv 1", 28, 2); // 2 = Right

        // Create XP Text (below level)
        GameObject xpObj = CreateTextElement("XP Text", hudPanel.transform, new Vector2(1, 1), new Vector2(1, 1), new Vector2(100, 30), new Vector2(-80, -70));
        Component xpText = xpObj.GetComponent(TextMeshProUGUIType);
        SetTMPText(xpText, "[0%]", 20, 2);

        // Wire text references to HUD controller
        SerializedObject hudSO = new SerializedObject(hudController);
        hudSO.FindProperty("livesText").objectReferenceValue = livesText;
        hudSO.FindProperty("scoreText").objectReferenceValue = scoreText;
        hudSO.FindProperty("levelText").objectReferenceValue = levelText;
        hudSO.FindProperty("xpText").objectReferenceValue = xpText;
        hudSO.FindProperty("animateOnChange").boolValue = true;
        hudSO.FindProperty("popScale").floatValue = 1.2f;
        hudSO.FindProperty("popDuration").floatValue = 0.2f;
        hudSO.ApplyModifiedPropertiesWithoutUndo();

        return hudPanel;
    }

    private static void SetTMPText(Component textComponent, string text, float fontSize, int alignment)
    {
        if (textComponent == null) return;

        SerializedObject so = new SerializedObject(textComponent);
        
        // Set text
        SerializedProperty textProp = so.FindProperty("m_text");
        if (textProp != null) textProp.stringValue = text;
        
        // Set font size
        SerializedProperty sizeProp = so.FindProperty("m_fontSize");
        if (sizeProp != null) sizeProp.floatValue = fontSize;
        
        // Set alignment using the TextMeshPro alignment enum values
        // Left = 257, Center = 258, Right = 260 (TMPro.TextAlignmentOptions)
        SerializedProperty alignProp = so.FindProperty("m_textAlignment");
        if (alignProp != null)
        {
            int alignValue = alignment switch
            {
                0 => 257,  // Left
                1 => 258,  // Center
                2 => 260,  // Right
                _ => 257
            };
            alignProp.intValue = alignValue;
        }
        
        // Set color to white
        SerializedProperty colorProp = so.FindProperty("m_fontColor");
        if (colorProp != null) colorProp.colorValue = Color.white;
        
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    private static GameObject CreateTextElement(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 size, Vector2 anchoredPos)
    {
        GameObject textObj = new GameObject(name);
        textObj.transform.SetParent(parent);

        RectTransform rect = textObj.AddComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = anchorMin;
        rect.sizeDelta = size;
        rect.anchoredPosition = anchoredPos;

        textObj.AddComponent(TextMeshProUGUIType);

        return textObj;
    }

    private static GameObject CreateConfirmDialog(Transform parent)
    {
        GameObject dialog = new GameObject("Confirm Dialog");
        dialog.transform.SetParent(parent);

        RectTransform dialogRect = dialog.AddComponent<RectTransform>();
        dialogRect.anchorMin = Vector2.zero;
        dialogRect.anchorMax = Vector2.one;
        dialogRect.offsetMin = Vector2.zero;
        dialogRect.offsetMax = Vector2.zero;

        Image overlayImg = dialog.AddComponent<Image>();
        overlayImg.color = new Color(0f, 0f, 0f, 0.7f);

        GameObject modal = new GameObject("Modal");
        modal.transform.SetParent(dialog.transform);
        RectTransform modalRect = modal.AddComponent<RectTransform>();
        modalRect.anchorMin = new Vector2(0.5f, 0.5f);
        modalRect.anchorMax = new Vector2(0.5f, 0.5f);
        modalRect.sizeDelta = new Vector2(450, 220);
        modalRect.anchoredPosition = Vector2.zero;

        Image modalBg = modal.AddComponent<Image>();
        modalBg.color = new Color(0.15f, 0.15f, 0.2f);

        // Title
        GameObject title = new GameObject("Title");
        title.transform.SetParent(modal.transform);
        RectTransform titleRect = title.AddComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.5f, 1f);
        titleRect.anchorMax = new Vector2(0.5f, 1f);
        titleRect.sizeDelta = new Vector2(400, 50);
        titleRect.anchoredPosition = new Vector2(0, -40);

        Component titleText = title.AddComponent(TextMeshProUGUIType);
        SetTMPText(titleText, "CONFIRM?", 32, 1);
        SetTMPColor(titleText, Color.white);

        // Message
        GameObject message = new GameObject("Message");
        message.transform.SetParent(modal.transform);
        RectTransform msgRect = message.AddComponent<RectTransform>();
        msgRect.anchorMin = new Vector2(0.5f, 0.5f);
        msgRect.anchorMax = new Vector2(0.5f, 0.5f);
        msgRect.sizeDelta = new Vector2(400, 40);
        msgRect.anchoredPosition = new Vector2(0, 10);

        Component msgText = message.AddComponent(TextMeshProUGUIType);
        SetTMPText(msgText, "Are you sure?", 20, 1);
        SetTMPColor(msgText, new Color(0.7f, 0.7f, 0.7f));

        // Button container
        GameObject btnContainer = new GameObject("Button Container");
        btnContainer.transform.SetParent(modal.transform);
        RectTransform btnRect = btnContainer.AddComponent<RectTransform>();
        btnRect.anchorMin = new Vector2(0.5f, 0f);
        btnRect.anchorMax = new Vector2(0.5f, 0f);
        btnRect.sizeDelta = new Vector2(300, 50);
        btnRect.anchoredPosition = new Vector2(0, 40);

        HorizontalLayoutGroup hLayout = btnContainer.AddComponent<HorizontalLayoutGroup>();
        hLayout.spacing = 20;
        hLayout.childAlignment = TextAnchor.MiddleCenter;
        hLayout.childControlWidth = false;
        hLayout.childControlHeight = false;

        CreateMenuButton("Confirm Button", btnContainer.transform, "YES", new Color(0.7f, 0.2f, 0.2f), 45, 120);
        CreateMenuButton("Cancel Button", btnContainer.transform, "NO", new Color(0.3f, 0.6f, 0.3f), 45, 120);

        // Add ConfirmDialog component
        Component confirmComp = dialog.AddComponent(ConfirmDialogType);
        SerializedObject confirmSO = new SerializedObject(confirmComp);
        confirmSO.FindProperty("titleText").objectReferenceValue = titleText;
        confirmSO.FindProperty("messageText").objectReferenceValue = msgText;
        confirmSO.FindProperty("confirmButton").objectReferenceValue = btnContainer.transform.Find("Confirm Button")?.GetComponent<Button>();
        confirmSO.FindProperty("cancelButton").objectReferenceValue = btnContainer.transform.Find("Cancel Button")?.GetComponent<Button>();
        confirmSO.ApplyModifiedPropertiesWithoutUndo();

        return dialog;
    }

    private static GameObject CreateMenuButton(string name, Transform parent, string text, Color color, float height, float width = 250)
    {
        GameObject btn = new GameObject(name);
        btn.transform.SetParent(parent);

        RectTransform rect = btn.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(width, height);

        Image img = btn.AddComponent<Image>();
        img.color = color;

        Button button = btn.AddComponent<Button>();
        ColorBlock colors = button.colors;
        colors.highlightedColor = color * 1.2f;
        colors.pressedColor = color * 0.8f;
        button.colors = colors;

        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(btn.transform);

        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        Component tmp = textObj.AddComponent(TextMeshProUGUIType);
        SetTMPText(tmp, text, 22, 1);
        SetTMPColor(tmp, Color.white);

        return btn;
    }

    private static void SetTMPColor(Component textComponent, Color color)
    {
        if (textComponent == null) return;

        SerializedObject so = new SerializedObject(textComponent);
        SerializedProperty colorProp = so.FindProperty("m_fontColor");
        if (colorProp != null)
        {
            colorProp.colorValue = color;
            so.ApplyModifiedPropertiesWithoutUndo();
        }
    }

    #endregion
}
