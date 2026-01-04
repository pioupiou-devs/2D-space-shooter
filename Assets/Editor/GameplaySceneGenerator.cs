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

        // Create GameScreenManager
        GameObject gameScreenManager = CreateGameScreenManager(cameraObj.GetComponent<Camera>());

        // Create BulletPool
        GameObject bulletPool = CreateBulletPool();

        // Create Player
        GameObject player = CreatePlayer(
            healthConfig, combatConfig, defenseConfig, movementConfig, progressionConfig,
            singleShotStrategy, bulletPool, gameScreenManager);

        // Create HUD Canvas
        GameObject hudCanvas = CreateHUDCanvas(player);

        // Save scene
        EnsureDirectoryExists("Assets/Game/Scenes");
        EditorSceneManager.SaveScene(newScene, ScenePath);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"Gameplay scene generated successfully at: {ScenePath}");
        Debug.Log("Scene contains: Main Camera, GameScreenManager, BulletPool, Player (with all stat systems), HUD Canvas (with MinimalHUDController)");
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
            UIManagerType, MinimalHUDControllerType, TextMeshProUGUIType
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

        return canvasObj;
    }

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
}
