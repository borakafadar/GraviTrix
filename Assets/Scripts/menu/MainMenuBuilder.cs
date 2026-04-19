using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class MainMenuBuilder : MonoBehaviour
{
    [Header("Scene Settings")]
    [SerializeField] private string playSceneName = "MainScene";

    [Header("Fonts")]
    [SerializeField] private Font titleFont;
    [SerializeField] private Font buttonFont;

    [Header("Music")]
    [SerializeField] private AudioClip musicClip;

    [Header("Layout")]
    [SerializeField] private Vector2 referenceResolution = new Vector2(1080, 1920);

    private Canvas canvas;
    private Camera menuCamera;

    private GameObject optionsPopupObject;

    private const string MusicVolumeKey = "MusicVolume";
    private const string SfxVolumeKey = "SfxVolume";

    private Slider musicVolumeSlider;
    private Text musicVolumeText;

    private Slider sfxVolumeSlider;
    private Text sfxVolumeText;

    private float musicVolume = 0.8f;
    private float sfxVolume = 0.8f;

    [SerializeField] private string recordPlayerPrefsKey = "HighScore";

    private Text recordText;

    private void Start()
    {
        BuildMainMenu();
    }

    private void BuildMainMenu()
    {
        CreateMenuCamera();
        CreateCanvas();
        CreateBackgroundGrid();
        CreateFlyingBlocks(); // Uçan bloklar eklendi
        CreateBackgroundParticles(); // Parçacık efektleri eklendi
        CreateTitle();
        CreateRecordText();
        CreateMenuButtons();
        CreateOptionsPopup();
        CreateEventSystemIfNeeded();
        CreateMusicManager();
    }

    private void CreateMusicManager()
    {
        if (MusicManager.Instance == null)
        {
            GameObject musicManagerObject = new GameObject("MusicManager");
            MusicManager manager = musicManagerObject.AddComponent<MusicManager>();

            if (musicClip != null)
            {
                manager.PlayMusic(musicClip);
            }
        }

        if (SfxManager.Instance == null)
        {
            GameObject sfxManagerObject = new GameObject("SfxManager");
            sfxManagerObject.AddComponent<SfxManager>();
        }
    }

    private void CreateMenuCamera()
    {
        Camera existingCamera = Camera.main;

        if (existingCamera != null)
        {
            menuCamera = existingCamera;
            menuCamera.clearFlags = CameraClearFlags.SolidColor;
            menuCamera.backgroundColor = new Color32(35, 22, 70, 255);
            menuCamera.orthographic = true;
            menuCamera.orthographicSize = 5f;
            menuCamera.transform.position = new Vector3(0f, 0f, -10f);
            return;
        }

        GameObject cameraObject = new GameObject("Main Menu Camera");
        menuCamera = cameraObject.AddComponent<Camera>();

        cameraObject.tag = "MainCamera";

        menuCamera.clearFlags = CameraClearFlags.SolidColor;
        menuCamera.backgroundColor = new Color32(35, 22, 70, 255);
        menuCamera.orthographic = true;
        menuCamera.orthographicSize = 5f;
        menuCamera.nearClipPlane = 0.1f;
        menuCamera.farClipPlane = 100f;

        cameraObject.transform.position = new Vector3(0f, 0f, -10f);
    }

    private void CreateCanvas()
    {
        GameObject canvasObject = new GameObject("Main Menu Canvas");

        canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceCamera;
        canvas.worldCamera = menuCamera;
        canvas.planeDistance = 1f;
        canvas.sortingOrder = 10;

        CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = referenceResolution;
        scaler.matchWidthOrHeight = 0.5f;

        canvasObject.AddComponent<GraphicRaycaster>();
    }

    private void CreateBackgroundGrid()
    {
        GameObject backgroundObject = new GameObject("Purple Gradient 12x12 Grid Background");
        backgroundObject.transform.SetParent(canvas.transform, false);

        Image backgroundImage = backgroundObject.AddComponent<Image>();
        backgroundImage.sprite = CreatePurpleGridSprite(1200, 1200, 12);
        backgroundImage.type = Image.Type.Simple;

        RectTransform rect = backgroundObject.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        // Arkaplanı en arkaya atıyoruz
        Canvas bgCanvas = backgroundObject.AddComponent<Canvas>();
        bgCanvas.overrideSorting = true;
        bgCanvas.sortingOrder = 5;

        backgroundObject.transform.SetAsFirstSibling();
    }

    private void CreateFlyingBlocks()
    {
        GameObject container = new GameObject("Flying Blocks Container");
        container.transform.SetParent(canvas.transform, false);

        Canvas fbCanvas = container.AddComponent<Canvas>();
        fbCanvas.overrideSorting = true;
        fbCanvas.sortingOrder = 6;

        RectTransform rect = container.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        FlyingBlocksManager manager = container.AddComponent<FlyingBlocksManager>();
        manager.blockSprite = CreateSimpleBlockSprite(128, 24);

        container.transform.SetSiblingIndex(1);
    }

    private void CreateBackgroundParticles()
    {
        GameObject psObj = new GameObject("BackgroundParticles");
        psObj.transform.SetParent(menuCamera.transform, false);
        psObj.transform.localPosition = new Vector3(0, 0, 5f);

        ParticleSystem ps = psObj.AddComponent<ParticleSystem>();
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        ParticleSystemRenderer psr = ps.GetComponent<ParticleSystemRenderer>();

        psr.material = new Material(Shader.Find("Sprites/Default"));
        psr.sortingOrder = 8;

        var main = ps.main;
        main.duration = 5f;
        main.loop = true;
        main.startLifetime = new ParticleSystem.MinMaxCurve(4f, 10f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(0.1f, 0.4f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.05f, 0.2f);
        main.startColor = new Color(1f, 1f, 1f, 0.4f);
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.maxParticles = 150;

        var emission = ps.emission;
        emission.rateOverTime = 20f;

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = new Vector3(20f, 20f, 1f);

        var velOverLifetime = ps.velocityOverLifetime;
        velOverLifetime.enabled = true;
        velOverLifetime.x = new ParticleSystem.MinMaxCurve(0f);
        velOverLifetime.y = new ParticleSystem.MinMaxCurve(1.25f);
        velOverLifetime.z = new ParticleSystem.MinMaxCurve(0f);

        var colorOverLifetime = ps.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient grad = new Gradient();
        grad.SetKeys(
            new GradientColorKey[] { new GradientColorKey(Color.cyan, 0f), new GradientColorKey(Color.magenta, 1f) },
            new GradientAlphaKey[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(1f, 0.3f), new GradientAlphaKey(0f, 1f) }
        );
        colorOverLifetime.color = grad;

        ps.Play();
    }

    private void CreateTitle()
    {
        GameObject titleObject = new GameObject("Title - GRAVITRIX");
        titleObject.transform.SetParent(canvas.transform, false);

        Text titleText = titleObject.AddComponent<Text>();
        titleText.text = "GRAVITRIX";
        titleText.font = titleFont != null ? titleFont : GetDefaultFont();

        // Boyut büyütüldü
        titleText.fontSize = 145;

        titleText.fontStyle = FontStyle.Bold;
        titleText.alignment = TextAnchor.MiddleCenter;
        titleText.color = Color.white;

        HorizontalGradientText titleGradient = titleObject.AddComponent<HorizontalGradientText>();
        titleGradient.leftColor = new Color32(85, 190, 255, 255);
        titleGradient.rightColor = new Color32(255, 80, 205, 255);

        Outline outline = titleObject.AddComponent<Outline>();
        outline.effectColor = new Color32(70, 35, 130, 255);
        outline.effectDistance = new Vector2(3f, -3f);

        Shadow shadow = titleObject.AddComponent<Shadow>();
        shadow.effectColor = new Color(0.05f, 0.02f, 0.12f, 0.55f);
        shadow.effectDistance = new Vector2(6f, -6f);

        RectTransform rect = titleObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 1f);
        rect.anchorMax = new Vector2(0.5f, 1f);
        rect.pivot = new Vector2(0.5f, 0.5f);

        // Aşağı alındı ve genişletildi
        rect.anchoredPosition = new Vector2(0f, -310f);
        rect.sizeDelta = new Vector2(1300f, 250f);

        titleObject.transform.SetAsLastSibling();
    }

    private void CreateRecordText()
    {
        GameObject recordObject = new GameObject("Record Text");
        recordObject.transform.SetParent(canvas.transform, false);

        recordText = recordObject.AddComponent<Text>();

        int recordScore = PlayerPrefs.GetInt(recordPlayerPrefsKey, 0);

        // TOP RECORD olarak değiştirildi
        recordText.text = "TOP RECORD: " + recordScore;
        recordText.font = buttonFont != null ? buttonFont : GetDefaultFont();

        // Boyut büyütüldü
        recordText.fontSize = 42;

        recordText.fontStyle = FontStyle.Bold;
        recordText.alignment = TextAnchor.MiddleCenter;
        recordText.color = new Color32(255, 225, 80, 255);

        Shadow shadow = recordObject.AddComponent<Shadow>();
        shadow.effectColor = new Color(0.05f, 0.02f, 0.12f, 0.6f);
        shadow.effectDistance = new Vector2(3f, -3f);

        RectTransform rect = recordObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 1f);
        rect.anchorMax = new Vector2(0.5f, 1f);
        rect.pivot = new Vector2(0.5f, 0.5f);

        // Başlığa göre hizalandı
        rect.anchoredPosition = new Vector2(0f, -440f);
        rect.sizeDelta = new Vector2(800f, 80f);

        recordObject.transform.SetAsLastSibling();
    }

    private void CreateMenuButtons()
    {
        GameObject buttonContainer = new GameObject("Menu Buttons");
        buttonContainer.transform.SetParent(canvas.transform, false);

        RectTransform containerRect = buttonContainer.AddComponent<RectTransform>();
        containerRect.anchorMin = new Vector2(0.5f, 0.5f);
        containerRect.anchorMax = new Vector2(0.5f, 0.5f);
        containerRect.pivot = new Vector2(0.5f, 0.5f);
        containerRect.anchoredPosition = new Vector2(0f, -210f);
        containerRect.sizeDelta = new Vector2(720f, 520f);

        VerticalLayoutGroup layout = buttonContainer.AddComponent<VerticalLayoutGroup>();
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.spacing = 55f;
        layout.childControlWidth = false;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;

        CreateMenuButton(buttonContainer.transform, "PLAY", OnPlayClicked);
        CreateMenuButton(buttonContainer.transform, "OPTIONS", OnOptionsClicked);
        CreateMenuButton(buttonContainer.transform, "EXIT", OnExitClicked);

        buttonContainer.transform.SetAsLastSibling();
    }

    private void CreateMenuButton(Transform parent, string label, UnityEngine.Events.UnityAction clickAction)
    {
        GameObject buttonObject = new GameObject("Button - " + label);
        buttonObject.transform.SetParent(parent, false);

        RectTransform rect = buttonObject.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(650f, 105f);

        Image buttonImage = buttonObject.AddComponent<Image>();
        buttonImage.sprite = CreateRoundedButtonSprite(650, 105, 28);
        buttonImage.type = Image.Type.Simple;
        buttonImage.raycastTarget = true;

        Button button = buttonObject.AddComponent<Button>();
        button.targetGraphic = buttonImage;
        button.transition = Selectable.Transition.None;
        button.onClick.AddListener(clickAction);

        Shadow buttonShadow = buttonObject.AddComponent<Shadow>();
        buttonShadow.effectColor = new Color(0.15f, 0.07f, 0.02f, 0.35f);
        buttonShadow.effectDistance = new Vector2(0f, -6f);

        GameObject darkOverlayObject = new GameObject("Hover Dark Overlay");
        darkOverlayObject.transform.SetParent(buttonObject.transform, false);

        Image darkOverlay = darkOverlayObject.AddComponent<Image>();
        darkOverlay.sprite = buttonImage.sprite;
        darkOverlay.type = Image.Type.Simple;
        darkOverlay.color = new Color(0f, 0f, 0f, 0f);
        darkOverlay.raycastTarget = false;

        RectTransform overlayRect = darkOverlayObject.GetComponent<RectTransform>();
        overlayRect.anchorMin = Vector2.zero;
        overlayRect.anchorMax = Vector2.one;
        overlayRect.offsetMin = Vector2.zero;
        overlayRect.offsetMax = Vector2.zero;

        GameObject textObject = new GameObject("Text - " + label);
        textObject.transform.SetParent(buttonObject.transform, false);

        Text buttonText = textObject.AddComponent<Text>();
        buttonText.text = label;
        buttonText.font = buttonFont != null ? buttonFont : GetDefaultFont();
        buttonText.fontSize = 42;
        buttonText.fontStyle = FontStyle.Bold;
        buttonText.alignment = TextAnchor.MiddleCenter;
        buttonText.color = new Color32(75, 42, 22, 255);
        buttonText.raycastTarget = false;

        RectTransform textRect = textObject.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        MainMenuButtonEffect buttonEffect = buttonObject.AddComponent<MainMenuButtonEffect>();
        buttonEffect.SetOverlay(darkOverlay);

        darkOverlayObject.transform.SetAsLastSibling();
        textObject.transform.SetAsLastSibling();
    }

    private void CreateOptionsPopup()
    {
        optionsPopupObject = new GameObject("Options Popup");
        optionsPopupObject.transform.SetParent(canvas.transform, false);

        RectTransform popupRootRect = optionsPopupObject.AddComponent<RectTransform>();
        popupRootRect.anchorMin = Vector2.zero;
        popupRootRect.anchorMax = Vector2.one;
        popupRootRect.offsetMin = Vector2.zero;
        popupRootRect.offsetMax = Vector2.zero;

        Image darkOverlay = optionsPopupObject.AddComponent<Image>();
        darkOverlay.color = new Color(0f, 0f, 0f, 0.45f);

        GameObject panelObject = new GameObject("Options Panel");
        panelObject.transform.SetParent(optionsPopupObject.transform, false);

        RectTransform panelRect = panelObject.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.anchoredPosition = Vector2.zero;
        panelRect.sizeDelta = new Vector2(720f, 650f);

        Image panelImage = panelObject.AddComponent<Image>();
        panelImage.sprite = CreateRoundedPanelSprite(720, 650, 42);
        panelImage.type = Image.Type.Simple;

        Shadow panelShadow = panelObject.AddComponent<Shadow>();
        panelShadow.effectColor = new Color(0f, 0f, 0f, 0.45f);
        panelShadow.effectDistance = new Vector2(0f, -10f);

        CreatePopupTitle(panelObject.transform);

        musicVolume = PlayerPrefs.GetFloat(MusicVolumeKey, 0.8f);
        sfxVolume = PlayerPrefs.GetFloat(SfxVolumeKey, 0.8f);

        CreateMusicVolumeSlider(panelObject.transform, new Vector2(0f, 110f));
        CreateSfxVolumeSlider(panelObject.transform, new Vector2(0f, -55f));
        CreatePopupButton(panelObject.transform, "CLOSE", new Vector2(0f, -240f), CloseOptionsPopup, out _);

        UpdateOptionsTexts();

        optionsPopupObject.SetActive(false);
        optionsPopupObject.transform.SetAsLastSibling();
    }

    private void CreatePopupTitle(Transform parent)
    {
        GameObject titleObject = new GameObject("Options Title");
        titleObject.transform.SetParent(parent, false);

        Text titleText = titleObject.AddComponent<Text>();
        titleText.text = "OPTIONS";
        titleText.font = titleFont != null ? titleFont : GetDefaultFont();
        titleText.fontSize = 62;
        titleText.fontStyle = FontStyle.Bold;
        titleText.alignment = TextAnchor.MiddleCenter;
        titleText.color = new Color32(255, 225, 90, 255);

        Shadow shadow = titleObject.AddComponent<Shadow>();
        shadow.effectColor = new Color(0f, 0f, 0f, 0.5f);
        shadow.effectDistance = new Vector2(4f, -4f);

        RectTransform rect = titleObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = new Vector2(0f, 230f);
        rect.sizeDelta = new Vector2(600f, 100f);
    }

    private void CreatePopupButton(
        Transform parent,
        string label,
        Vector2 anchoredPosition,
        UnityEngine.Events.UnityAction clickAction,
        out Text createdText
    )
    {
        GameObject buttonObject = new GameObject("Popup Button - " + label);
        buttonObject.transform.SetParent(parent, false);

        RectTransform rect = buttonObject.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = new Vector2(500f, 95f);

        Image buttonImage = buttonObject.AddComponent<Image>();
        buttonImage.sprite = CreateRoundedButtonSprite(500, 95, 26);
        buttonImage.type = Image.Type.Simple;
        buttonImage.raycastTarget = true;

        Button button = buttonObject.AddComponent<Button>();
        button.targetGraphic = buttonImage;
        button.transition = Selectable.Transition.None;
        button.onClick.AddListener(clickAction);

        GameObject darkOverlayObject = new GameObject("Hover Dark Overlay");
        darkOverlayObject.transform.SetParent(buttonObject.transform, false);

        Image darkOverlay = darkOverlayObject.AddComponent<Image>();
        darkOverlay.sprite = buttonImage.sprite;
        darkOverlay.type = Image.Type.Simple;
        darkOverlay.color = new Color(0f, 0f, 0f, 0f);
        darkOverlay.raycastTarget = false;

        RectTransform overlayRect = darkOverlayObject.GetComponent<RectTransform>();
        overlayRect.anchorMin = Vector2.zero;
        overlayRect.anchorMax = Vector2.one;
        overlayRect.offsetMin = Vector2.zero;
        overlayRect.offsetMax = Vector2.zero;

        GameObject textObject = new GameObject("Text - " + label);
        textObject.transform.SetParent(buttonObject.transform, false);

        Text buttonText = textObject.AddComponent<Text>();
        buttonText.text = label;
        buttonText.font = buttonFont != null ? buttonFont : GetDefaultFont();
        buttonText.fontSize = 36;
        buttonText.fontStyle = FontStyle.Bold;
        buttonText.alignment = TextAnchor.MiddleCenter;
        buttonText.color = new Color32(75, 42, 22, 255);
        buttonText.raycastTarget = false;

        RectTransform textRect = textObject.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        MainMenuButtonEffect effect = buttonObject.AddComponent<MainMenuButtonEffect>();
        effect.SetOverlay(darkOverlay);

        darkOverlayObject.transform.SetAsLastSibling();
        textObject.transform.SetAsLastSibling();

        createdText = buttonText;
    }

    private void CreateMusicVolumeSlider(Transform parent, Vector2 anchoredPosition)
    {
        GameObject rootObject = new GameObject("Music Volume Slider Root");
        rootObject.transform.SetParent(parent, false);

        RectTransform rootRect = rootObject.AddComponent<RectTransform>();
        rootRect.anchorMin = new Vector2(0.5f, 0.5f);
        rootRect.anchorMax = new Vector2(0.5f, 0.5f);
        rootRect.pivot = new Vector2(0.5f, 0.5f);
        rootRect.anchoredPosition = anchoredPosition;
        rootRect.sizeDelta = new Vector2(560f, 130f);

        GameObject labelObject = new GameObject("Music Volume Label");
        labelObject.transform.SetParent(rootObject.transform, false);

        musicVolumeText = labelObject.AddComponent<Text>();
        musicVolumeText.font = buttonFont != null ? buttonFont : GetDefaultFont();
        musicVolumeText.fontSize = 30;
        musicVolumeText.fontStyle = FontStyle.Bold;
        musicVolumeText.alignment = TextAnchor.MiddleCenter;
        musicVolumeText.color = new Color32(255, 225, 90, 255);
        musicVolumeText.raycastTarget = false;

        RectTransform labelRect = labelObject.GetComponent<RectTransform>();
        labelRect.anchorMin = new Vector2(0.5f, 1f);
        labelRect.anchorMax = new Vector2(0.5f, 1f);
        labelRect.pivot = new Vector2(0.5f, 0.5f);
        labelRect.anchoredPosition = new Vector2(0f, -20f);
        labelRect.sizeDelta = new Vector2(560f, 50f);

        GameObject sliderObject = new GameObject("Music Volume Slider");
        sliderObject.transform.SetParent(rootObject.transform, false);

        RectTransform sliderRect = sliderObject.AddComponent<RectTransform>();
        sliderRect.anchorMin = new Vector2(0.5f, 0f);
        sliderRect.anchorMax = new Vector2(0.5f, 0f);
        sliderRect.pivot = new Vector2(0.5f, 0.5f);
        sliderRect.anchoredPosition = new Vector2(0f, 35f);
        sliderRect.sizeDelta = new Vector2(520f, 55f);

        musicVolumeSlider = sliderObject.AddComponent<Slider>();
        musicVolumeSlider.minValue = 0f;
        musicVolumeSlider.maxValue = 1f;
        musicVolumeSlider.wholeNumbers = false;

        GameObject backgroundObject = new GameObject("Background");
        backgroundObject.transform.SetParent(sliderObject.transform, false);

        Image backgroundImage = backgroundObject.AddComponent<Image>();
        backgroundImage.sprite = CreateSliderTrackSprite(520, 28, 14, new Color32(45, 28, 80, 255), new Color32(160, 130, 210, 255));
        backgroundImage.type = Image.Type.Simple;

        RectTransform backgroundRect = backgroundObject.GetComponent<RectTransform>();
        backgroundRect.anchorMin = new Vector2(0f, 0.5f);
        backgroundRect.anchorMax = new Vector2(1f, 0.5f);
        backgroundRect.pivot = new Vector2(0.5f, 0.5f);
        backgroundRect.anchoredPosition = Vector2.zero;
        backgroundRect.sizeDelta = new Vector2(0f, 28f);

        GameObject fillAreaObject = new GameObject("Fill Area");
        fillAreaObject.transform.SetParent(sliderObject.transform, false);

        RectTransform fillAreaRect = fillAreaObject.AddComponent<RectTransform>();
        fillAreaRect.anchorMin = new Vector2(0f, 0.5f);
        fillAreaRect.anchorMax = new Vector2(1f, 0.5f);
        fillAreaRect.pivot = new Vector2(0.5f, 0.5f);
        fillAreaRect.anchoredPosition = Vector2.zero;
        fillAreaRect.sizeDelta = new Vector2(-32f, 28f);

        GameObject fillObject = new GameObject("Fill");
        fillObject.transform.SetParent(fillAreaObject.transform, false);

        Image fillImage = fillObject.AddComponent<Image>();
        fillImage.sprite = CreateSliderTrackSprite(520, 28, 14, new Color32(255, 220, 45, 255), new Color32(235, 135, 22, 255));
        fillImage.type = Image.Type.Simple;

        RectTransform fillRect = fillObject.GetComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.offsetMin = Vector2.zero;
        fillRect.offsetMax = Vector2.zero;

        GameObject handleAreaObject = new GameObject("Handle Slide Area");
        handleAreaObject.transform.SetParent(sliderObject.transform, false);

        RectTransform handleAreaRect = handleAreaObject.AddComponent<RectTransform>();
        handleAreaRect.anchorMin = Vector2.zero;
        handleAreaRect.anchorMax = Vector2.one;
        handleAreaRect.offsetMin = new Vector2(18f, 0f);
        handleAreaRect.offsetMax = new Vector2(-18f, 0f);

        GameObject handleObject = new GameObject("Handle");
        handleObject.transform.SetParent(handleAreaObject.transform, false);

        Image handleImage = handleObject.AddComponent<Image>();
        handleImage.sprite = CreateCircleSprite(48, new Color32(255, 235, 95, 255), new Color32(145, 75, 20, 255));
        handleImage.type = Image.Type.Simple;

        RectTransform handleRect = handleObject.GetComponent<RectTransform>();
        handleRect.sizeDelta = new Vector2(48f, 48f);

        musicVolumeSlider.fillRect = fillRect;
        musicVolumeSlider.handleRect = handleRect;
        musicVolumeSlider.targetGraphic = handleImage;

        musicVolumeSlider.value = musicVolume;
        musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);

        UpdateMusicVolumeText();
    }

    private void CreateSfxVolumeSlider(Transform parent, Vector2 anchoredPosition)
    {
        GameObject rootObject = new GameObject("SFX Volume Slider Root");
        rootObject.transform.SetParent(parent, false);

        RectTransform rootRect = rootObject.AddComponent<RectTransform>();
        rootRect.anchorMin = new Vector2(0.5f, 0.5f);
        rootRect.anchorMax = new Vector2(0.5f, 0.5f);
        rootRect.pivot = new Vector2(0.5f, 0.5f);
        rootRect.anchoredPosition = anchoredPosition;
        rootRect.sizeDelta = new Vector2(560f, 130f);

        GameObject labelObject = new GameObject("SFX Volume Label");
        labelObject.transform.SetParent(rootObject.transform, false);

        sfxVolumeText = labelObject.AddComponent<Text>();
        sfxVolumeText.font = buttonFont != null ? buttonFont : GetDefaultFont();
        sfxVolumeText.fontSize = 30;
        sfxVolumeText.fontStyle = FontStyle.Bold;
        sfxVolumeText.alignment = TextAnchor.MiddleCenter;
        sfxVolumeText.color = new Color32(255, 225, 90, 255);
        sfxVolumeText.raycastTarget = false;

        RectTransform labelRect = labelObject.GetComponent<RectTransform>();
        labelRect.anchorMin = new Vector2(0.5f, 1f);
        labelRect.anchorMax = new Vector2(0.5f, 1f);
        labelRect.pivot = new Vector2(0.5f, 0.5f);
        labelRect.anchoredPosition = new Vector2(0f, -20f);
        labelRect.sizeDelta = new Vector2(560f, 50f);

        GameObject sliderObject = new GameObject("SFX Volume Slider");
        sliderObject.transform.SetParent(rootObject.transform, false);

        RectTransform sliderRect = sliderObject.AddComponent<RectTransform>();
        sliderRect.anchorMin = new Vector2(0.5f, 0f);
        sliderRect.anchorMax = new Vector2(0.5f, 0f);
        sliderRect.pivot = new Vector2(0.5f, 0.5f);
        sliderRect.anchoredPosition = new Vector2(0f, 35f);
        sliderRect.sizeDelta = new Vector2(520f, 55f);

        sfxVolumeSlider = sliderObject.AddComponent<Slider>();
        sfxVolumeSlider.minValue = 0f;
        sfxVolumeSlider.maxValue = 1f;
        sfxVolumeSlider.wholeNumbers = false;

        GameObject backgroundObject = new GameObject("Background");
        backgroundObject.transform.SetParent(sliderObject.transform, false);

        Image backgroundImage = backgroundObject.AddComponent<Image>();
        backgroundImage.sprite = CreateSliderTrackSprite(
            520,
            28,
            14,
            new Color32(45, 28, 80, 255),
            new Color32(160, 130, 210, 255)
        );
        backgroundImage.type = Image.Type.Simple;

        RectTransform backgroundRect = backgroundObject.GetComponent<RectTransform>();
        backgroundRect.anchorMin = new Vector2(0f, 0.5f);
        backgroundRect.anchorMax = new Vector2(1f, 0.5f);
        backgroundRect.pivot = new Vector2(0.5f, 0.5f);
        backgroundRect.anchoredPosition = Vector2.zero;
        backgroundRect.sizeDelta = new Vector2(0f, 28f);

        GameObject fillAreaObject = new GameObject("Fill Area");
        fillAreaObject.transform.SetParent(sliderObject.transform, false);

        RectTransform fillAreaRect = fillAreaObject.AddComponent<RectTransform>();
        fillAreaRect.anchorMin = new Vector2(0f, 0.5f);
        fillAreaRect.anchorMax = new Vector2(1f, 0.5f);
        fillAreaRect.pivot = new Vector2(0.5f, 0.5f);
        fillAreaRect.anchoredPosition = Vector2.zero;
        fillAreaRect.sizeDelta = new Vector2(-32f, 28f);

        GameObject fillObject = new GameObject("Fill");
        fillObject.transform.SetParent(fillAreaObject.transform, false);

        Image fillImage = fillObject.AddComponent<Image>();
        fillImage.sprite = CreateSliderTrackSprite(
            520,
            28,
            14,
            new Color32(255, 220, 45, 255),
            new Color32(235, 135, 22, 255)
        );
        fillImage.type = Image.Type.Simple;

        RectTransform fillRect = fillObject.GetComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.offsetMin = Vector2.zero;
        fillRect.offsetMax = Vector2.zero;

        GameObject handleAreaObject = new GameObject("Handle Slide Area");
        handleAreaObject.transform.SetParent(sliderObject.transform, false);

        RectTransform handleAreaRect = handleAreaObject.AddComponent<RectTransform>();
        handleAreaRect.anchorMin = Vector2.zero;
        handleAreaRect.anchorMax = Vector2.one;
        handleAreaRect.offsetMin = new Vector2(18f, 0f);
        handleAreaRect.offsetMax = new Vector2(-18f, 0f);

        GameObject handleObject = new GameObject("Handle");
        handleObject.transform.SetParent(handleAreaObject.transform, false);

        Image handleImage = handleObject.AddComponent<Image>();
        handleImage.sprite = CreateCircleSprite(
            48,
            new Color32(255, 235, 95, 255),
            new Color32(145, 75, 20, 255)
        );
        handleImage.type = Image.Type.Simple;

        RectTransform handleRect = handleObject.GetComponent<RectTransform>();
        handleRect.sizeDelta = new Vector2(48f, 48f);

        sfxVolumeSlider.fillRect = fillRect;
        sfxVolumeSlider.handleRect = handleRect;
        sfxVolumeSlider.targetGraphic = handleImage;

        sfxVolumeSlider.value = sfxVolume;
        sfxVolumeSlider.onValueChanged.AddListener(OnSfxVolumeChanged);

        UpdateSfxVolumeText();
    }

    private void OnMusicVolumeChanged(float value)
    {
        musicVolume = value;

        PlayerPrefs.SetFloat(MusicVolumeKey, musicVolume);
        PlayerPrefs.Save();

        UpdateMusicVolumeText();

        if (MusicManager.Instance != null)
        {
            MusicManager.Instance.SetVolume(musicVolume);
        }
    }

    private void UpdateMusicVolumeText()
    {
        if (musicVolumeText == null)
        {
            return;
        }

        int percentage = Mathf.RoundToInt(musicVolume * 100f);
        musicVolumeText.text = "MUSIC VOLUME: " + percentage + "%";
    }

    private void OnSfxVolumeChanged(float value)
    {
        sfxVolume = value;

        PlayerPrefs.SetFloat(SfxVolumeKey, sfxVolume);
        PlayerPrefs.Save();

        UpdateSfxVolumeText();

        if (SfxManager.Instance != null)
        {
            SfxManager.Instance.SetVolume(sfxVolume);
        }
    }

    private void UpdateSfxVolumeText()
    {
        if (sfxVolumeText == null)
        {
            return;
        }

        int percentage = Mathf.RoundToInt(sfxVolume * 100f);
        sfxVolumeText.text = "SFX VOLUME: " + percentage + "%";
    }

    private void UpdateOptionsTexts()
    {
        UpdateMusicVolumeText();
        UpdateSfxVolumeText();
    }

    private void CloseOptionsPopup()
    {
        if (optionsPopupObject != null)
        {
            optionsPopupObject.SetActive(false);
        }
    }

    private Sprite CreatePurpleGridSprite(int width, int height, int cellCount)
    {
        Texture2D texture = new Texture2D(width, height);
        texture.filterMode = FilterMode.Bilinear;

        Color topColor = new Color32(105, 75, 160, 255);
        Color middleColor = new Color32(75, 48, 125, 255);
        Color bottomColor = new Color32(38, 24, 72, 255);

        Color gridColor = new Color32(170, 150, 205, 255);

        int cellSizeX = width / cellCount;
        int cellSizeY = height / cellCount;

        Vector2 center = new Vector2(width / 2f, height / 2f);
        float maxDistance = Vector2.Distance(Vector2.zero, center);

        for (int y = 0; y < height; y++)
        {
            float verticalT = (float)y / (height - 1);

            Color baseColor;

            if (verticalT < 0.5f)
            {
                baseColor = Color.Lerp(bottomColor, middleColor, verticalT / 0.5f);
            }
            else
            {
                baseColor = Color.Lerp(middleColor, topColor, (verticalT - 0.5f) / 0.5f);
            }

            for (int x = 0; x < width; x++)
            {
                float distanceFromCenter = Vector2.Distance(new Vector2(x, y), center);
                float glowAmount = 1f - Mathf.Clamp01(distanceFromCenter / maxDistance);
                Color glowingColor = Color.Lerp(baseColor, new Color32(120, 85, 180, 255), glowAmount * 0.25f);

                bool isGridLineX = x % cellSizeX <= 2;
                bool isGridLineY = y % cellSizeY <= 2;

                if (isGridLineX || isGridLineY)
                {
                    texture.SetPixel(x, y, Color.Lerp(glowingColor, gridColor, 0.35f));
                }
                else
                {
                    texture.SetPixel(x, y, glowingColor);
                }
            }
        }

        texture.Apply();

        return Sprite.Create(
            texture,
            new Rect(0, 0, width, height),
            new Vector2(0.5f, 0.5f),
            100f
        );
    }

    private Sprite CreateRoundedButtonSprite(int width, int height, int radius)
    {
        Texture2D texture = new Texture2D(width, height);
        texture.filterMode = FilterMode.Bilinear;

        Color leftColor = new Color32(255, 220, 45, 255);
        Color rightColor = new Color32(235, 135, 22, 255);
        Color borderColor = new Color32(155, 82, 18, 255);
        Color highlightColor = new Color32(255, 238, 105, 255);

        int borderThickness = 5;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float distance = GetRoundedRectDistance(x, y, width, height, radius);

                if (distance > 0f)
                {
                    texture.SetPixel(x, y, Color.clear);
                    continue;
                }

                float horizontalT = (float)x / (width - 1);
                float verticalT = (float)y / (height - 1);

                Color horizontalGradient = Color.Lerp(leftColor, rightColor, horizontalT);
                Color verticalShade = Color.Lerp(new Color32(210, 105, 20, 255), horizontalGradient, verticalT);

                bool isBorder = distance > -borderThickness;
                bool isTopHighlight = y > height * 0.68f && x > width * 0.06f && x < width * 0.94f;

                if (isBorder)
                {
                    texture.SetPixel(x, y, borderColor);
                }
                else if (isTopHighlight)
                {
                    texture.SetPixel(x, y, Color.Lerp(verticalShade, highlightColor, 0.16f));
                }
                else
                {
                    texture.SetPixel(x, y, verticalShade);
                }
            }
        }

        texture.Apply();

        return Sprite.Create(
            texture,
            new Rect(0, 0, width, height),
            new Vector2(0.5f, 0.5f),
            100f
        );
    }

    private Sprite CreateRoundedPanelSprite(int width, int height, int radius)
    {
        Texture2D texture = new Texture2D(width, height);
        texture.filterMode = FilterMode.Bilinear;

        Color topColor = new Color32(95, 58, 150, 245);
        Color bottomColor = new Color32(42, 24, 82, 245);
        Color borderColor = new Color32(190, 155, 235, 255);

        int borderThickness = 6;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float distance = GetRoundedRectDistance(x, y, width, height, radius);

                if (distance > 0f)
                {
                    texture.SetPixel(x, y, Color.clear);
                    continue;
                }

                float t = (float)y / (height - 1);
                Color baseColor = Color.Lerp(bottomColor, topColor, t);

                bool isBorder = distance > -borderThickness;

                if (isBorder)
                {
                    texture.SetPixel(x, y, borderColor);
                }
                else
                {
                    texture.SetPixel(x, y, baseColor);
                }
            }
        }

        texture.Apply();

        return Sprite.Create(
            texture,
            new Rect(0, 0, width, height),
            new Vector2(0.5f, 0.5f),
            100f
        );
    }

    private Sprite CreateSliderTrackSprite(int width, int height, int radius, Color leftColor, Color rightColor)
    {
        Texture2D texture = new Texture2D(width, height);
        texture.filterMode = FilterMode.Bilinear;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float distance = GetRoundedRectDistance(x, y, width, height, radius);

                if (distance > 0f)
                {
                    texture.SetPixel(x, y, Color.clear);
                    continue;
                }

                float t = (float)x / (width - 1);
                texture.SetPixel(x, y, Color.Lerp(leftColor, rightColor, t));
            }
        }

        texture.Apply();

        return Sprite.Create(
            texture,
            new Rect(0, 0, width, height),
            new Vector2(0.5f, 0.5f),
            100f
        );
    }

    private Sprite CreateCircleSprite(int size, Color fillColor, Color borderColor)
    {
        Texture2D texture = new Texture2D(size, size);
        texture.filterMode = FilterMode.Bilinear;

        Vector2 center = new Vector2(size / 2f, size / 2f);
        float radius = size / 2f - 2f;
        float borderRadius = radius - 5f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), center);

                if (distance > radius)
                {
                    texture.SetPixel(x, y, Color.clear);
                }
                else if (distance > borderRadius)
                {
                    texture.SetPixel(x, y, borderColor);
                }
                else
                {
                    texture.SetPixel(x, y, fillColor);
                }
            }
        }

        texture.Apply();

        return Sprite.Create(
            texture,
            new Rect(0, 0, size, size),
            new Vector2(0.5f, 0.5f),
            100f
        );
    }

    private Sprite CreateSimpleBlockSprite(int size, int radius)
    {
        Texture2D texture = new Texture2D(size, size);
        texture.filterMode = FilterMode.Bilinear;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float distance = GetRoundedRectDistance(x, y, size, size, radius);

                if (distance > 0f)
                {
                    texture.SetPixel(x, y, Color.clear);
                }
                else
                {
                    float t = (x + (size - y)) / (float)(size * 2);
                    Color c = Color.Lerp(Color.white, new Color(0.8f, 0.8f, 0.8f, 1f), t);

                    if (distance > -3f) c = new Color(1f, 1f, 1f, 0.4f);

                    texture.SetPixel(x, y, c);
                }
            }
        }

        texture.Apply();
        return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
    }

    private float GetRoundedRectDistance(int x, int y, int width, int height, int radius)
    {
        float px = x - width / 2f;
        float py = y - height / 2f;

        float halfWidth = width / 2f - radius;
        float halfHeight = height / 2f - radius;

        float dx = Mathf.Abs(px) - halfWidth;
        float dy = Mathf.Abs(py) - halfHeight;

        float outsideDistance = new Vector2(Mathf.Max(dx, 0), Mathf.Max(dy, 0)).magnitude;
        float insideDistance = Mathf.Min(Mathf.Max(dx, dy), 0);

        return outsideDistance + insideDistance - radius;
    }

    private Font GetDefaultFont()
    {
        Font defaultFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        if (defaultFont == null)
        {
            defaultFont = Resources.GetBuiltinResource<Font>("Arial.ttf");
        }

        return defaultFont;
    }

    private void CreateEventSystemIfNeeded()
    {
        if (FindObjectOfType<UnityEngine.EventSystems.EventSystem>() != null)
        {
            return;
        }

        GameObject eventSystemObject = new GameObject("EventSystem");
        eventSystemObject.AddComponent<UnityEngine.EventSystems.EventSystem>();
        eventSystemObject.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
    }

    private void OnPlayClicked()
    {
        SceneManager.LoadScene(playSceneName);
    }

    private void OnOptionsClicked()
    {
        if (optionsPopupObject != null)
        {
            optionsPopupObject.SetActive(true);
        }
    }

    private void OnExitClicked()
    {
        Debug.Log("Exit clicked.");

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}

// -------------------------------------------------------------------------
// YARDIMCI VE YENİ SINIFLAR
// -------------------------------------------------------------------------

public class FlyingBlocksManager : MonoBehaviour
{
    public Sprite blockSprite;

    private float spawnTimer;

    private class BlockData
    {
        public RectTransform rect;
        public Vector2 velocity;
        public float rotationSpeed;
    }

    private List<BlockData> activeBlocks = new List<BlockData>();
    private Color[] blockColors = {
        Color.cyan,
        Color.magenta,
        Color.yellow,
        new Color(1f, 0.5f, 0f),
        Color.green,
        new Color(1f, 0.2f, 0.3f)
    };

    private void Start()
    {
        for (int i = 0; i < 8; i++)
        {
            SpawnBlock(true);
        }
    }

    private void Update()
    {
        spawnTimer -= Time.deltaTime;
        if (spawnTimer <= 0)
        {
            SpawnBlock(false);
            spawnTimer = Random.Range(1.0f, 2.5f);
        }

        for (int i = activeBlocks.Count - 1; i >= 0; i--)
        {
            BlockData b = activeBlocks[i];
            if (b.rect == null)
            {
                activeBlocks.RemoveAt(i);
                continue;
            }

            b.rect.anchoredPosition += b.velocity * Time.deltaTime;
            b.rect.Rotate(0, 0, b.rotationSpeed * Time.deltaTime);

            if (Mathf.Abs(b.rect.anchoredPosition.x) > 1200f || Mathf.Abs(b.rect.anchoredPosition.y) > 2200f)
            {
                Destroy(b.rect.gameObject);
                activeBlocks.RemoveAt(i);
            }
        }
    }

    private void SpawnBlock(bool randomInsideScreen)
    {
        GameObject go = new GameObject("FlyingBlock");
        go.transform.SetParent(transform, false);

        Image img = go.AddComponent<Image>();
        img.sprite = blockSprite;

        Color c = blockColors[Random.Range(0, blockColors.Length)];
        c.a = Random.Range(0.1f, 0.3f);
        img.color = c;

        RectTransform rect = go.GetComponent<RectTransform>();

        // Boyutlar korundu
        float size = Random.Range(90f, 280f);
        rect.sizeDelta = new Vector2(size, size);

        Vector2 startPos = Vector2.zero;
        Vector2 vel = Vector2.zero;

        float halfW = 1080f / 2f + 100f;
        float halfH = 1920f / 2f + 100f;
        float speed = Random.Range(50f, 250f);

        if (randomInsideScreen)
        {
            startPos = new Vector2(Random.Range(-halfW, halfW), Random.Range(-halfH, halfH));
            vel = new Vector2(Random.Range(-speed, speed), Random.Range(-speed, speed));
        }
        else
        {
            int side = Random.Range(0, 4);
            switch (side)
            {
                case 0:
                    startPos = new Vector2(-halfW, Random.Range(-halfH, halfH));
                    vel = new Vector2(speed, Random.Range(-speed * 0.5f, speed * 0.5f));
                    break;
                case 1:
                    startPos = new Vector2(halfW, Random.Range(-halfH, halfH));
                    vel = new Vector2(-speed, Random.Range(-speed * 0.5f, speed * 0.5f));
                    break;
                case 2:
                    startPos = new Vector2(Random.Range(-halfW, halfW), -halfH);
                    vel = new Vector2(Random.Range(-speed * 0.5f, speed * 0.5f), speed);
                    break;
                case 3:
                    startPos = new Vector2(Random.Range(-halfW, halfW), halfH);
                    vel = new Vector2(Random.Range(-speed * 0.5f, speed * 0.5f), -speed);
                    break;
            }
        }

        rect.anchoredPosition = startPos;

        BlockData bd = new BlockData();
        bd.rect = rect;
        bd.velocity = vel;
        bd.rotationSpeed = Random.Range(-150f, 150f);

        activeBlocks.Add(bd);
    }
}

public class HorizontalGradientText : BaseMeshEffect
{
    public Color leftColor = Color.cyan;
    public Color rightColor = Color.magenta;

    public override void ModifyMesh(VertexHelper vertexHelper)
    {
        if (!IsActive() || vertexHelper.currentVertCount == 0)
        {
            return;
        }

        List<UIVertex> vertices = new List<UIVertex>();
        vertexHelper.GetUIVertexStream(vertices);

        float minX = vertices[0].position.x;
        float maxX = vertices[0].position.x;

        for (int i = 1; i < vertices.Count; i++)
        {
            float x = vertices[i].position.x;

            if (x < minX)
            {
                minX = x;
            }

            if (x > maxX)
            {
                maxX = x;
            }
        }

        float width = maxX - minX;

        for (int i = 0; i < vertices.Count; i++)
        {
            UIVertex vertex = vertices[i];

            float t = width <= 0.01f ? 0f : Mathf.InverseLerp(minX, maxX, vertex.position.x);
            vertex.color = Color.Lerp(leftColor, rightColor, t);

            vertices[i] = vertex;
        }

        vertexHelper.Clear();
        vertexHelper.AddUIVertexTriangleStream(vertices);
    }
}

public class MainMenuButtonEffect : MonoBehaviour
{
    private Image darkOverlay;
    private RectTransform rectTransform;
    private Canvas parentCanvas;

    private Vector3 normalScale = Vector3.one;
    private Vector3 hoverScale = new Vector3(1.035f, 1.035f, 1f);
    private Vector3 pressedScale = new Vector3(0.94f, 0.94f, 1f);

    private float normalDarkness = 0f;
    private float hoverDarkness = 0.18f;
    private float pressedDarkness = 0.35f;

    private float animationSpeed = 14f;

    public void SetOverlay(Image overlay)
    {
        darkOverlay = overlay;
    }

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        parentCanvas = GetComponentInParent<Canvas>();
    }

    private void Update()
    {
        if (rectTransform == null || darkOverlay == null)
        {
            return;
        }

        Camera uiCamera = null;

        if (parentCanvas != null && parentCanvas.renderMode != RenderMode.ScreenSpaceOverlay)
        {
            uiCamera = parentCanvas.worldCamera;
        }

        bool isMouseOver = RectTransformUtility.RectangleContainsScreenPoint(
            rectTransform,
            Input.mousePosition,
            uiCamera
        );

        float targetDarkness = normalDarkness;
        Vector3 targetScale = normalScale;

        if (isMouseOver)
        {
            targetDarkness = hoverDarkness;
            targetScale = hoverScale;

            if (Input.GetMouseButton(0))
            {
                targetDarkness = pressedDarkness;
                targetScale = pressedScale;
            }
        }

        Color currentColor = darkOverlay.color;
        Color targetColor = new Color(0f, 0f, 0f, targetDarkness);

        darkOverlay.color = Color.Lerp(
            currentColor,
            targetColor,
            Time.unscaledDeltaTime * animationSpeed
        );

        rectTransform.localScale = Vector3.Lerp(
            rectTransform.localScale,
            targetScale,
            Time.unscaledDeltaTime * animationSpeed
        );
    }
}