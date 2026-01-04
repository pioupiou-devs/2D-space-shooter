using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System;
using System.Reflection;

/// <summary>
/// Editor script to generate a complete main menu scene with Play, Options, and Quit functionality.
/// Run via menu: Tools > 2D Space Shooter > Generate Main Menu Scene
/// </summary>
public static class MainMenuSceneGenerator
{
    private const string ScenePath = "Assets/Game/Scenes/MainMenuScene.unity";

    private static Type MainMenuManagerType => GetType("MainMenuManager");
    private static Type OptionsManagerType => GetType("OptionsManager");
    private static Type TextMeshProUGUIType => GetType("TMPro.TextMeshProUGUI");

    private static Type GetType(string typeName)
    {
        foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            Type type = assembly.GetType(typeName);
            if (type != null)
                return type;
        }
        Debug.LogWarning($"Type not found: {typeName}");
        return null;
    }

    [MenuItem("Tools/2D Space Shooter/Generate Main Menu Scene")]
    public static void GenerateScene()
    {
        if (!ValidateTypes())
        {
            Debug.LogError("MainMenuSceneGenerator: Missing required types.");
            return;
        }

        EnsureDirectoryExists("Assets/Game/Scenes");

        Scene newScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        CreateMainCamera();
        CreateEventSystem();
        
        GameObject canvas = CreateCanvas();
        
        GameObject mainPanel = CreateMainMenuPanel(canvas.transform);
        GameObject optionsPanel = CreateOptionsPanel(canvas.transform);
        GameObject quitPanel = CreateQuitConfirmPanel(canvas.transform);

        Component menuManager = canvas.AddComponent(MainMenuManagerType);
        Component optionsManager = canvas.AddComponent(OptionsManagerType);

        SerializedObject menuSO = new SerializedObject(menuManager);
        menuSO.FindProperty("mainMenuPanel").objectReferenceValue = mainPanel;
        menuSO.FindProperty("optionsPanel").objectReferenceValue = optionsPanel;
        menuSO.FindProperty("quitConfirmPanel").objectReferenceValue = quitPanel;
        menuSO.FindProperty("gameplaySceneName").stringValue = "GameplayScene";

        Button playBtn = mainPanel.transform.Find("Button Container/Play Button")?.GetComponent<Button>();
        Button optionsBtn = mainPanel.transform.Find("Button Container/Options Button")?.GetComponent<Button>();
        Button quitBtn = mainPanel.transform.Find("Button Container/Quit Button")?.GetComponent<Button>();
        Button backBtn = optionsPanel.transform.Find("Back Button")?.GetComponent<Button>();
        Button confirmBtn = quitPanel.transform.Find("Modal/Button Container/Confirm Button")?.GetComponent<Button>();
        Button cancelBtn = quitPanel.transform.Find("Modal/Button Container/Cancel Button")?.GetComponent<Button>();

        menuSO.FindProperty("playButton").objectReferenceValue = playBtn;
        menuSO.FindProperty("optionsButton").objectReferenceValue = optionsBtn;
        menuSO.FindProperty("quitButton").objectReferenceValue = quitBtn;
        menuSO.FindProperty("optionsBackButton").objectReferenceValue = backBtn;
        menuSO.FindProperty("quitConfirmButton").objectReferenceValue = confirmBtn;
        menuSO.FindProperty("quitCancelButton").objectReferenceValue = cancelBtn;
        menuSO.ApplyModifiedPropertiesWithoutUndo();

        WireOptionsManager(optionsManager, optionsPanel);

        EditorSceneManager.SaveScene(newScene, ScenePath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"Main menu scene generated at: {ScenePath}");
    }

    private static bool ValidateTypes()
    {
        return MainMenuManagerType != null && OptionsManagerType != null && TextMeshProUGUIType != null;
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
                    AssetDatabase.CreateFolder(currentPath, parts[i]);
                currentPath = newPath;
            }
        }
    }

    private static void CreateMainCamera()
    {
        GameObject cam = new GameObject("Main Camera");
        cam.tag = "MainCamera";
        Camera camera = cam.AddComponent<Camera>();
        camera.orthographic = true;
        camera.orthographicSize = 5f;
        camera.backgroundColor = new Color(0.05f, 0.05f, 0.1f);
        camera.clearFlags = CameraClearFlags.SolidColor;
        cam.AddComponent<AudioListener>();
        cam.transform.position = new Vector3(0, 0, -10);
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
            Debug.Log("EventSystem created with InputSystemUIInputModule");
        }
        else
        {
            // Fallback to legacy input only if Input System is not available
            es.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            Debug.LogWarning("InputSystemUIInputModule not found, using legacy StandaloneInputModule");
        }
    }

    private static GameObject CreateCanvas()
    {
        GameObject canvasObj = new GameObject("Main Menu Canvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;
        
        canvasObj.AddComponent<GraphicRaycaster>();
        return canvasObj;
    }

    private static GameObject CreateMainMenuPanel(Transform parent)
    {
        GameObject panel = CreateFullPanel("Main Menu Panel", parent);
        Image bg = panel.AddComponent<Image>();
        bg.color = new Color(0, 0, 0, 0.3f);

        // Title only (no subtitle)
        CreateText("Title", panel.transform, new Vector2(0.5f, 0.70f), "2D SPACE SHOOTER", 64, Color.cyan);

        // Button container
        GameObject btnContainer = new GameObject("Button Container");
        btnContainer.transform.SetParent(panel.transform);
        RectTransform containerRect = btnContainer.AddComponent<RectTransform>();
        containerRect.anchorMin = new Vector2(0.5f, 0.35f);
        containerRect.anchorMax = new Vector2(0.5f, 0.35f);
        containerRect.sizeDelta = new Vector2(300, 220);
        containerRect.anchoredPosition = Vector2.zero;

        VerticalLayoutGroup layout = btnContainer.AddComponent<VerticalLayoutGroup>();
        layout.spacing = 15;
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childControlWidth = true;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = true;

        CreateButton("Play Button", btnContainer.transform, "PLAY", new Color(0.2f, 0.7f, 0.2f), 60);
        CreateButton("Options Button", btnContainer.transform, "OPTIONS", new Color(0.3f, 0.5f, 0.8f), 60);
        CreateButton("Quit Button", btnContainer.transform, "QUIT", new Color(0.7f, 0.3f, 0.3f), 60);

        return panel;
    }

    private static GameObject CreateOptionsPanel(Transform parent)
    {
        GameObject panel = CreateFullPanel("Options Panel", parent);
        panel.SetActive(false);
        
        Image bg = panel.AddComponent<Image>();
        bg.color = new Color(0.1f, 0.1f, 0.15f, 0.98f);

        CreateText("Options Title", panel.transform, new Vector2(0.5f, 0.92f), "OPTIONS", 42, Color.white);

        GameObject scrollView = CreateScrollView(panel.transform);

        Transform contentTransform = scrollView.transform.Find("Viewport/Content");
        if (contentTransform != null)
        {
            // Add VerticalLayoutGroup to content for automatic layout
            VerticalLayoutGroup vlg = contentTransform.gameObject.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 5;
            vlg.padding = new RectOffset(20, 20, 10, 10);
            vlg.childAlignment = TextAnchor.UpperCenter;
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            // Add ContentSizeFitter to auto-size content
            ContentSizeFitter csf = contentTransform.gameObject.AddComponent<ContentSizeFitter>();
            csf.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            // Audio Section
            CreateSectionHeader("AUDIO", contentTransform);
            CreateSliderRow("Master Volume", contentTransform);
            CreateSliderRow("Music Volume", contentTransform);
            CreateSliderRow("SFX Volume", contentTransform);
            
            // Spacer
            CreateSpacer(contentTransform, 15);

            // Display Section
            CreateSectionHeader("DISPLAY", contentTransform);
            CreateToggleRow("Fullscreen", contentTransform);
            CreateToggleRow("V-Sync", contentTransform);

            // Spacer
            CreateSpacer(contentTransform, 15);

            // Controls Section
            CreateSectionHeader("CONTROLS", contentTransform);
            CreateKeyBindRow("Move Up", "W", contentTransform);
            CreateKeyBindRow("Move Down", "S", contentTransform);
            CreateKeyBindRow("Move Left", "A", contentTransform);
            CreateKeyBindRow("Move Right", "D", contentTransform);
            CreateKeyBindRow("Fire", "Space", contentTransform);
            CreateKeyBindRow("Pause", "Escape", contentTransform);
        }

        GameObject backBtn = CreateButton("Back Button", panel.transform, "BACK", new Color(0.5f, 0.5f, 0.5f), 50, 150);
        RectTransform backRect = backBtn.GetComponent<RectTransform>();
        backRect.anchorMin = new Vector2(0.5f, 0.03f);
        backRect.anchorMax = new Vector2(0.5f, 0.03f);
        backRect.anchoredPosition = Vector2.zero;

        return panel;
    }

    private static GameObject CreateScrollView(Transform parent)
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
        viewportRect.offsetMax = new Vector2(-15, 0); // Leave space for scrollbar
        
        Image viewportImage = viewport.AddComponent<Image>();
        viewportImage.color = new Color(0, 0, 0, 0.01f); // Nearly transparent, needed for masking
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
        contentRect.sizeDelta = new Vector2(0, 0); // Will be adjusted based on content

        scroll.viewport = viewportRect;
        scroll.content = contentRect;

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

        // Scrollbar Handle
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
        handleRect.anchorMax = Vector2.one;
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
        SerializedObject headerSO = new SerializedObject(headerTmp);
        headerSO.FindProperty("m_text").stringValue = title;
        headerSO.FindProperty("m_fontSize").floatValue = 22;
        headerSO.FindProperty("m_textAlignment").intValue = 257; // Left
        headerSO.FindProperty("m_fontColor").colorValue = Color.cyan;
        headerSO.ApplyModifiedPropertiesWithoutUndo();
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

        // Label (left side)
        GameObject labelObj = new GameObject("Label");
        labelObj.transform.SetParent(row.transform);
        RectTransform labelRect = labelObj.AddComponent<RectTransform>();
        labelRect.anchorMin = new Vector2(0, 0);
        labelRect.anchorMax = new Vector2(0.3f, 1);
        labelRect.offsetMin = Vector2.zero;
        labelRect.offsetMax = Vector2.zero;

        Component labelTmp = labelObj.AddComponent(TextMeshProUGUIType);
        SerializedObject labelSO = new SerializedObject(labelTmp);
        labelSO.FindProperty("m_text").stringValue = label;
        labelSO.FindProperty("m_fontSize").floatValue = 18;
        labelSO.FindProperty("m_textAlignment").intValue = 257; // Left
        labelSO.FindProperty("m_fontColor").colorValue = Color.white;
        labelSO.ApplyModifiedPropertiesWithoutUndo();

        // Slider (middle)
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

        // Background
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

        // Handle Area
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

        // Value Label (right side)
        GameObject valueObj = new GameObject("Value");
        valueObj.transform.SetParent(row.transform);
        RectTransform valueRect = valueObj.AddComponent<RectTransform>();
        valueRect.anchorMin = new Vector2(0.85f, 0);
        valueRect.anchorMax = new Vector2(1, 1);
        valueRect.offsetMin = Vector2.zero;
        valueRect.offsetMax = Vector2.zero;

        Component valueTmp = valueObj.AddComponent(TextMeshProUGUIType);
        SerializedObject valueSO = new SerializedObject(valueTmp);
        valueSO.FindProperty("m_text").stringValue = "100%";
        valueSO.FindProperty("m_fontSize").floatValue = 16;
        valueSO.FindProperty("m_textAlignment").intValue = 258; // Center
        valueSO.FindProperty("m_fontColor").colorValue = Color.white;
        valueSO.ApplyModifiedPropertiesWithoutUndo();
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

        // Label (left side)
        GameObject labelObj = new GameObject("Label");
        labelObj.transform.SetParent(row.transform);
        RectTransform labelRect = labelObj.AddComponent<RectTransform>();
        labelRect.anchorMin = new Vector2(0, 0);
        labelRect.anchorMax = new Vector2(0.7f, 1);
        labelRect.offsetMin = Vector2.zero;
        labelRect.offsetMax = Vector2.zero;

        Component labelTmp = labelObj.AddComponent(TextMeshProUGUIType);
        SerializedObject labelSO = new SerializedObject(labelTmp);
        labelSO.FindProperty("m_text").stringValue = label;
        labelSO.FindProperty("m_fontSize").floatValue = 18;
        labelSO.FindProperty("m_textAlignment").intValue = 257; // Left
        labelSO.FindProperty("m_fontColor").colorValue = Color.white;
        labelSO.ApplyModifiedPropertiesWithoutUndo();

        // Toggle (right side)
        GameObject toggleObj = new GameObject("Toggle");
        toggleObj.transform.SetParent(row.transform);
        RectTransform toggleRect = toggleObj.AddComponent<RectTransform>();
        toggleRect.anchorMin = new Vector2(0.75f, 0.1f);
        toggleRect.anchorMax = new Vector2(0.85f, 0.9f);
        toggleRect.offsetMin = Vector2.zero;
        toggleRect.offsetMax = Vector2.zero;

        Toggle toggle = toggleObj.AddComponent<Toggle>();

        // Background
        GameObject background = new GameObject("Background");
        background.transform.SetParent(toggleObj.transform);
        RectTransform bgRect = background.AddComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;
        Image bgImage = background.AddComponent<Image>();
        bgImage.color = new Color(0.25f, 0.25f, 0.25f);

        // Checkmark
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

        // Label (left side)
        GameObject labelObj = new GameObject("Label");
        labelObj.transform.SetParent(row.transform);
        RectTransform labelRect = labelObj.AddComponent<RectTransform>();
        labelRect.anchorMin = new Vector2(0, 0);
        labelRect.anchorMax = new Vector2(0.5f, 1);
        labelRect.offsetMin = Vector2.zero;
        labelRect.offsetMax = Vector2.zero;

        Component labelTmp = labelObj.AddComponent(TextMeshProUGUIType);
        SerializedObject labelSO = new SerializedObject(labelTmp);
        labelSO.FindProperty("m_text").stringValue = label;
        labelSO.FindProperty("m_fontSize").floatValue = 16;
        labelSO.FindProperty("m_textAlignment").intValue = 257; // Left
        labelSO.FindProperty("m_fontColor").colorValue = Color.white;
        labelSO.ApplyModifiedPropertiesWithoutUndo();

        // Key Button (right side)
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

        // Key Text
        GameObject keyTextObj = new GameObject("KeyText");
        keyTextObj.transform.SetParent(keyBtn.transform);
        RectTransform keyTextRect = keyTextObj.AddComponent<RectTransform>();
        keyTextRect.anchorMin = Vector2.zero;
        keyTextRect.anchorMax = Vector2.one;
        keyTextRect.offsetMin = Vector2.zero;
        keyTextRect.offsetMax = Vector2.zero;

        Component keyTmp = keyTextObj.AddComponent(TextMeshProUGUIType);
        SerializedObject keySO = new SerializedObject(keyTmp);
        keySO.FindProperty("m_text").stringValue = defaultKey;
        keySO.FindProperty("m_fontSize").floatValue = 14;
        keySO.FindProperty("m_textAlignment").intValue = 258; // Center
        keySO.FindProperty("m_fontColor").colorValue = Color.white;
        keySO.ApplyModifiedPropertiesWithoutUndo();
    }

    private static GameObject CreateQuitConfirmPanel(Transform parent)
    {
        GameObject overlay = CreateFullPanel("Quit Confirm Panel", parent);
        overlay.SetActive(false);
        
        Image overlayBg = overlay.AddComponent<Image>();
        overlayBg.color = new Color(0, 0, 0, 0.7f);

        GameObject modal = new GameObject("Modal");
        modal.transform.SetParent(overlay.transform);
        RectTransform modalRect = modal.AddComponent<RectTransform>();
        modalRect.anchorMin = new Vector2(0.5f, 0.5f);
        modalRect.anchorMax = new Vector2(0.5f, 0.5f);
        modalRect.sizeDelta = new Vector2(450, 220);
        modalRect.anchoredPosition = Vector2.zero;

        Image modalBg = modal.AddComponent<Image>();
        modalBg.color = new Color(0.15f, 0.15f, 0.2f);

        CreateText("Quit Title", modal.transform, new Vector2(0.5f, 0.75f), "QUIT GAME?", 32, Color.white);
        CreateText("Quit Message", modal.transform, new Vector2(0.5f, 0.5f), "Are you sure you want to quit?", 20, Color.gray);

        GameObject btnContainer = new GameObject("Button Container");
        btnContainer.transform.SetParent(modal.transform);
        RectTransform btnRect = btnContainer.AddComponent<RectTransform>();
        btnRect.anchorMin = new Vector2(0.5f, 0.15f);
        btnRect.anchorMax = new Vector2(0.5f, 0.15f);
        btnRect.sizeDelta = new Vector2(300, 50);
        btnRect.anchoredPosition = Vector2.zero;

        HorizontalLayoutGroup hLayout = btnContainer.AddComponent<HorizontalLayoutGroup>();
        hLayout.spacing = 20;
        hLayout.childAlignment = TextAnchor.MiddleCenter;
        hLayout.childControlWidth = false;
        hLayout.childControlHeight = false;

        CreateButton("Confirm Button", btnContainer.transform, "YES", new Color(0.7f, 0.2f, 0.2f), 45, 120);
        CreateButton("Cancel Button", btnContainer.transform, "NO", new Color(0.3f, 0.6f, 0.3f), 45, 120);

        return overlay;
    }

    private static GameObject CreateFullPanel(string name, Transform parent)
    {
        GameObject panel = new GameObject(name);
        panel.transform.SetParent(parent);
        RectTransform rect = panel.AddComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        return panel;
    }

    private static void CreateText(string name, Transform parent, Vector2 anchor, string text, float size, Color color)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent);
        RectTransform rect = obj.AddComponent<RectTransform>();
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.sizeDelta = new Vector2(600, size + 20);
        rect.anchoredPosition = Vector2.zero;

        Component tmp = obj.AddComponent(TextMeshProUGUIType);
        SerializedObject so = new SerializedObject(tmp);
        so.FindProperty("m_text").stringValue = text;
        so.FindProperty("m_fontSize").floatValue = size;
        so.FindProperty("m_textAlignment").intValue = 258; // Center
        so.FindProperty("m_fontColor").colorValue = color;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    private static GameObject CreateButton(string name, Transform parent, string text, Color color, float height, float width = 250)
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
        SerializedObject so = new SerializedObject(tmp);
        so.FindProperty("m_text").stringValue = text;
        so.FindProperty("m_fontSize").floatValue = 22;
        so.FindProperty("m_textAlignment").intValue = 258;
        so.FindProperty("m_fontColor").colorValue = Color.white;
        so.ApplyModifiedPropertiesWithoutUndo();

        return btn;
    }

    private static void WireOptionsManager(Component manager, GameObject optionsPanel)
    {
        SerializedObject so = new SerializedObject(manager);
        
        Transform content = optionsPanel.transform.Find("Scroll View/Viewport/Content");
        if (content == null)
        {
            Debug.LogWarning("Could not find scroll view content");
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
}
