using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    private Canvas _canvas;
    private RectTransform _mainMenuPanel;
    private RectTransform _levelsPanel;
    private RectTransform _gameOverPanel;
    private RectTransform _gameWinPanel;
    private RectTransform _inGameHUD;

    private Text _gameOverScoreText;
    private Text _gameWinScoreText;
    private Button _nextLevelButton;
    private Text _timerText;

    private readonly List<Button> _levelButtons = new();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        EnsureEventSystem();
        BuildCanvas();
        BuildMainMenu();
        BuildLevelsMenu();
        BuildGameOver();
        BuildGameWin();
        BuildInGameHUD();

        ShowMainMenu();
    }

    private void Update()
    {
        // Update timer text when HUD is visible
        if (_inGameHUD != null && _inGameHUD.gameObject.activeSelf && _timerText != null && GameManager.Instance != null)
        {
            _timerText.text = FormatTime(GameManager.Instance.GetTimeLeft());
        }
    }

    private void EnsureEventSystem()
    {
        if (FindObjectOfType<EventSystem>() != null) return;
        var es = new GameObject("EventSystem", typeof(EventSystem));
        // Try to add InputSystemUIInputModule if available, else fallback to StandaloneInputModule
        var inputSystemType = Type.GetType("UnityEngine.InputSystem.UI.InputSystemUIInputModule, Unity.InputSystem", false);
        if (inputSystemType != null)
        {
            es.AddComponent(inputSystemType);
        }
        else
        {
            es.AddComponent<StandaloneInputModule>();
        }
        DontDestroyOnLoad(es);
    }

    private void BuildCanvas()
    {
        var go = new GameObject("RuntimeCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        _canvas = go.GetComponent<Canvas>();
        _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = go.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;
        DontDestroyOnLoad(go);
    }

    private RectTransform CreateFullPanel(string name)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(_canvas.transform, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        return rt;
    }

    private static Font DefaultFont
    {
        get
        {
            var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (font == null)
            {
                // Fallback to an OS font if builtin not found
                try { font = Font.CreateDynamicFontFromOSFont("Arial", 16); } catch { }
            }
            return font;
        }
    }

    private Text CreateText(Transform parent, string text, int size, TextAnchor align = TextAnchor.MiddleCenter)
    {
        var go = new GameObject("Text", typeof(Text));
        go.transform.SetParent(parent, false);
        var t = go.GetComponent<Text>();
        t.font = DefaultFont;
        t.text = text;
        t.fontSize = size;
        t.alignment = align;
        t.color = Color.white;
        var rt = t.rectTransform;
        rt.sizeDelta = new Vector2(600, 100);
        return t;
    }

    private Button CreateButton(Transform parent, string label, Action onClick, ButtonStyle style = ButtonStyle.Default)
    {
        var go = new GameObject("Button", typeof(Image), typeof(Button));
        go.transform.SetParent(parent, false);
        var img = go.GetComponent<Image>();
        var btn = go.GetComponent<Button>();
        btn.targetGraphic = img;
        btn.onClick.AddListener(() => onClick?.Invoke());

        // Apply style-specific properties
        Vector2 buttonSize;
        Color normalColor, highlightColor;
        int fontSize;

        switch (style)
        {
            case ButtonStyle.MainMenu:
                buttonSize = new Vector2(320, 70);
                normalColor = new Color(0.2f, 0.6f, 0.9f, 0.9f); // Nice blue
                highlightColor = new Color(0.3f, 0.7f, 1f, 1f);
                fontSize = 32;
                break;
            case ButtonStyle.Small:
                buttonSize = new Vector2(160, 50);
                normalColor = new Color(0.25f, 0.25f, 0.25f, 0.9f);
                highlightColor = new Color(0.35f, 0.35f, 0.35f, 1f);
                fontSize = 24;
                break;
            case ButtonStyle.Level:
                buttonSize = new Vector2(280, 50);
                normalColor = new Color(0.15f, 0.7f, 0.4f, 0.9f); // Green
                highlightColor = new Color(0.2f, 0.8f, 0.5f, 1f);
                fontSize = 26;
                break;
            case ButtonStyle.Locked:
                buttonSize = new Vector2(280, 50);
                normalColor = new Color(0.4f, 0.2f, 0.2f, 0.8f); // Dark red
                highlightColor = new Color(0.4f, 0.2f, 0.2f, 0.8f);
                fontSize = 26;
                break;
            default:
                buttonSize = new Vector2(280, 60);
                normalColor = new Color(0.25f, 0.25f, 0.25f, 0.9f);
                highlightColor = new Color(0.35f, 0.35f, 0.35f, 1f);
                fontSize = 28;
                break;
        }

        img.color = normalColor;
        
        // Add rounded corners effect with shadow
        var shadow = go.AddComponent<Shadow>();
        shadow.effectColor = new Color(0, 0, 0, 0.5f);
        shadow.effectDistance = new Vector2(2, -2);

        // Set up color transitions
        var colors = btn.colors;
        colors.normalColor = normalColor;
        colors.highlightedColor = highlightColor;
        colors.pressedColor = new Color(normalColor.r * 0.8f, normalColor.g * 0.8f, normalColor.b * 0.8f, normalColor.a);
        colors.disabledColor = new Color(0.3f, 0.3f, 0.3f, 0.6f);
        colors.colorMultiplier = 1f;
        colors.fadeDuration = 0.15f;
        btn.colors = colors;

        // Layout element
        var le = go.GetComponent<LayoutElement>() ?? go.AddComponent<LayoutElement>();
        le.preferredWidth = buttonSize.x;
        le.preferredHeight = buttonSize.y;
        le.minHeight = buttonSize.y * 0.8f;

        var text = CreateText(go.transform, label, fontSize);
        text.color = Color.white;
        text.alignment = TextAnchor.MiddleCenter;
        text.raycastTarget = false;
        text.rectTransform.anchorMin = Vector2.zero;
        text.rectTransform.anchorMax = Vector2.one;
        text.rectTransform.offsetMin = new Vector2(8, 4);
        text.rectTransform.offsetMax = new Vector2(-8, -4);

        var rt = go.GetComponent<RectTransform>();
        rt.sizeDelta = buttonSize;
        return btn;
    }

    private enum ButtonStyle
    {
        Default,
        MainMenu,
        Small,
        Level,
        Locked
    }

    private void BuildMainMenu()
    {
        _mainMenuPanel = CreateFullPanel("MainMenu");
        
        // Add gradient background
        var bg = _mainMenuPanel.gameObject.AddComponent<Image>();
        bg.color = new Color(0.05f, 0.1f, 0.2f, 0.95f); // Dark blue gradient feel
        
        var layout = _mainMenuPanel.gameObject.AddComponent<VerticalLayoutGroup>();
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.spacing = 40;
        layout.padding = new RectOffset(60, 60, 120, 120);

        // Title with better styling
        var title = CreateText(_mainMenuPanel, "Draw The Path", 72);
        title.rectTransform.sizeDelta = new Vector2(800, 120);
        title.color = new Color(0.9f, 0.95f, 1f, 1f); // Slightly blue-tinted white
        title.fontStyle = FontStyle.Bold;
        
        // Add title shadow
        var titleShadow = title.gameObject.AddComponent<Shadow>();
        titleShadow.effectColor = new Color(0, 0, 0, 0.7f);
        titleShadow.effectDistance = new Vector2(3, -3);

        // Spacer
        var spacer = new GameObject("Spacer", typeof(RectTransform));
        spacer.transform.SetParent(_mainMenuPanel, false);
        var spacerRT = spacer.GetComponent<RectTransform>();
        spacerRT.sizeDelta = new Vector2(0, 60);

        CreateButton(_mainMenuPanel, "Play", ShowLevelsMenu, ButtonStyle.MainMenu);

        HidePanel(_mainMenuPanel);
    }

    private void BuildLevelsMenu()
    {
        _levelsPanel = CreateFullPanel("LevelsMenu");
        
        // Add background
        var bg = _levelsPanel.gameObject.AddComponent<Image>();
        bg.color = new Color(0.08f, 0.12f, 0.18f, 0.95f);

        // Header area
        var header = new GameObject("Header", typeof(RectTransform));
        header.transform.SetParent(_levelsPanel, false);
        var headerRT = header.GetComponent<RectTransform>();
        headerRT.anchorMin = new Vector2(0, 1);
        headerRT.anchorMax = new Vector2(1, 1);
        headerRT.pivot = new Vector2(0.5f, 1f);
        headerRT.sizeDelta = new Vector2(0, 140);
        headerRT.anchoredPosition = new Vector2(0, 0);

        var headerLayout = header.AddComponent<HorizontalLayoutGroup>();
        headerLayout.childAlignment = TextAnchor.MiddleCenter;
        headerLayout.padding = new RectOffset(40, 40, 30, 30);
        headerLayout.spacing = 40;

        var title = CreateText(header.transform, "Select Level", 52);
        title.rectTransform.sizeDelta = new Vector2(600, 100);
        title.color = new Color(0.9f, 0.95f, 1f, 1f);
        title.fontStyle = FontStyle.Bold;
        
        // Add title shadow
        var titleShadow = title.gameObject.AddComponent<Shadow>();
        titleShadow.effectColor = new Color(0, 0, 0, 0.6f);
        titleShadow.effectDistance = new Vector2(2, -2);

        var backBtn = CreateButton(header.transform, "Back", ShowMainMenu, ButtonStyle.Small);

        // Scroll area with better margins
        var scrollGO = new GameObject("ScrollView", typeof(Image), typeof(Mask), typeof(ScrollRect));
        scrollGO.transform.SetParent(_levelsPanel, false);
        var scrollRT = scrollGO.GetComponent<RectTransform>();
        scrollRT.anchorMin = new Vector2(0, 0);
        scrollRT.anchorMax = new Vector2(1, 1);
        scrollRT.offsetMin = new Vector2(60, 60); // More generous margins
        scrollRT.offsetMax = new Vector2(-60, -160);

        var scrollImg = scrollGO.GetComponent<Image>();
        scrollImg.color = new Color(0, 0, 0, 0.1f); // Subtle background

        var scroll = scrollGO.GetComponent<ScrollRect>();
        scroll.horizontal = false;

        var viewport = new GameObject("Viewport", typeof(RectTransform));
        viewport.transform.SetParent(scrollGO.transform, false);
        var viewportRT = viewport.GetComponent<RectTransform>();
        viewportRT.anchorMin = Vector2.zero;
        viewportRT.anchorMax = Vector2.one;
        viewportRT.offsetMin = Vector2.zero;
        viewportRT.offsetMax = Vector2.zero;
        scroll.viewport = viewportRT;

        var content = new GameObject("Content", typeof(RectTransform));
        content.transform.SetParent(viewportRT, false);
        var contentRT = content.GetComponent<RectTransform>();
        contentRT.anchorMin = new Vector2(0, 1);
        contentRT.anchorMax = new Vector2(1, 1);
        contentRT.pivot = new Vector2(0.5f, 1);
        contentRT.anchoredPosition = new Vector2(0, 0);
        contentRT.offsetMin = new Vector2(0, contentRT.offsetMin.y); // Ensure content doesn't go outside viewport
        contentRT.offsetMax = new Vector2(0, contentRT.offsetMax.y);

        var grid = content.AddComponent<GridLayoutGroup>();
        grid.cellSize = new Vector2(320, 180); // Better proportions
        grid.spacing = new Vector2(30, 30); // More generous spacing
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = 4;
        grid.padding = new RectOffset(40, 40, 30, 30); // More left/right padding to prevent cutoff
        grid.childAlignment = TextAnchor.UpperCenter; // Center align the grid items

        scroll.content = contentRT;

        HidePanel(_levelsPanel);
    }

    private void BuildGameOver()
    {
        _gameOverPanel = CreateFullPanel("GameOver");
        
        // Add background
        var bg = _gameOverPanel.gameObject.AddComponent<Image>();
        bg.color = new Color(0.15f, 0.05f, 0.05f, 0.95f); // Dark red tint
        
        var layout = _gameOverPanel.gameObject.AddComponent<VerticalLayoutGroup>();
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.spacing = 30;
        layout.padding = new RectOffset(60, 60, 100, 100);

        var title = CreateText(_gameOverPanel, "Game Over", 64);
        title.color = new Color(1f, 0.6f, 0.6f, 1f); // Light red
        title.fontStyle = FontStyle.Bold;
        
        var titleShadow = title.gameObject.AddComponent<Shadow>();
        titleShadow.effectColor = new Color(0, 0, 0, 0.8f);
        titleShadow.effectDistance = new Vector2(3, -3);
        
        _gameOverScoreText = CreateText(_gameOverPanel, "Score: 0", 40);
        _gameOverScoreText.color = new Color(0.9f, 0.9f, 0.9f, 1f);

        CreateButton(_gameOverPanel, "Retry", () => { GameManager.Instance.RetryLevel(); ShowInGameHUD(); });
        CreateButton(_gameOverPanel, "Level Select", ShowLevelsMenu);

        HidePanel(_gameOverPanel);
    }

    private void BuildGameWin()
    {
        _gameWinPanel = CreateFullPanel("GameWin");
        
        // Add background
        var bg = _gameWinPanel.gameObject.AddComponent<Image>();
        bg.color = new Color(0.05f, 0.15f, 0.05f, 0.95f); // Dark green tint
        
        var layout = _gameWinPanel.gameObject.AddComponent<VerticalLayoutGroup>();
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.spacing = 30;
        layout.padding = new RectOffset(60, 60, 100, 100);

        var title = CreateText(_gameWinPanel, "Level Complete!", 64);
        title.color = new Color(0.6f, 1f, 0.6f, 1f); // Light green
        title.fontStyle = FontStyle.Bold;
        
        var titleShadow = title.gameObject.AddComponent<Shadow>();
        titleShadow.effectColor = new Color(0, 0, 0, 0.8f);
        titleShadow.effectDistance = new Vector2(3, -3);
        
        _gameWinScoreText = CreateText(_gameWinPanel, "Score: 0", 40);
        _gameWinScoreText.color = new Color(0.9f, 0.9f, 0.9f, 1f);

        _nextLevelButton = CreateButton(_gameWinPanel, "Play Next Level", () =>
        {
            if (GameManager.Instance.TryPlayNextLevel())
            {
                ShowInGameHUD();
            }
        });
        CreateButton(_gameWinPanel, "Level Select", ShowLevelsMenu);

        HidePanel(_gameWinPanel);
    }

    private void BuildInGameHUD()
    {
        _inGameHUD = CreateFullPanel("InGameHUD");

        // Back button (top-left) with better styling
        var backBtn = CreateButton(_inGameHUD, "Back", () =>
        {
            if (LevelManager.Instance != null) LevelManager.Instance.EndLevel();
            if (GameManager.Instance != null) GameManager.Instance.IsGameRunning = false;
            ShowLevelsMenu();
        }, ButtonStyle.Small);
        
        var backRT = backBtn.GetComponent<RectTransform>();
        backRT.anchorMin = new Vector2(0, 1);
        backRT.anchorMax = new Vector2(0, 1);
        backRT.pivot = new Vector2(0, 1);
        backRT.anchoredPosition = new Vector2(25, -25);
        backRT.sizeDelta = new Vector2(140, 55);

        // Timer (top-right) with better styling
        _timerText = CreateText(_inGameHUD, "00:00", 40, TextAnchor.MiddleRight);
        _timerText.color = new Color(1f, 1f, 1f, 0.95f);
        _timerText.fontStyle = FontStyle.Bold;
        
        // Add timer background
        var timerBG = new GameObject("TimerBG", typeof(Image));
        timerBG.transform.SetParent(_inGameHUD, false);
        var timerBGImg = timerBG.GetComponent<Image>();
        timerBGImg.color = new Color(0, 0, 0, 0.6f);
        var timerBGRT = timerBG.GetComponent<RectTransform>();
        timerBGRT.anchorMin = new Vector2(1, 1);
        timerBGRT.anchorMax = new Vector2(1, 1);
        timerBGRT.pivot = new Vector2(1, 1);
        timerBGRT.anchoredPosition = new Vector2(-25, -25);
        timerBGRT.sizeDelta = new Vector2(140, 55);
        
        // Add shadow to timer background
        var timerBGShadow = timerBG.AddComponent<Shadow>();
        timerBGShadow.effectColor = new Color(0, 0, 0, 0.5f);
        timerBGShadow.effectDistance = new Vector2(2, -2);
        
        var timerRT = _timerText.rectTransform;
        timerRT.anchorMin = new Vector2(1, 1);
        timerRT.anchorMax = new Vector2(1, 1);
        timerRT.pivot = new Vector2(1, 1);
        timerRT.anchoredPosition = new Vector2(-25, -25);
        timerRT.sizeDelta = new Vector2(140, 55);
        
        // Move timer to front
        _timerText.transform.SetAsLastSibling();

        HidePanel(_inGameHUD);
    }

    private string FormatTime(float timeSeconds)
    {
        if (timeSeconds < 0) timeSeconds = 0;
        int minutes = Mathf.FloorToInt(timeSeconds / 60f);
        int seconds = Mathf.FloorToInt(timeSeconds % 60f);
        return $"{minutes:00}:{seconds:00}";
    }

    private void HidePanel(RectTransform panel)
    {
        if (panel != null) panel.gameObject.SetActive(false);
    }

    private void ShowOnly(RectTransform panel)
    {
        if (_mainMenuPanel) _mainMenuPanel.gameObject.SetActive(false);
        if (_levelsPanel) _levelsPanel.gameObject.SetActive(false);
        if (_gameOverPanel) _gameOverPanel.gameObject.SetActive(false);
        if (_gameWinPanel) _gameWinPanel.gameObject.SetActive(false);
        if (_inGameHUD) _inGameHUD.gameObject.SetActive(false);
        if (panel) panel.gameObject.SetActive(true);
    }

    public void HideAll()
    {
        if (_mainMenuPanel) _mainMenuPanel.gameObject.SetActive(false);
        if (_levelsPanel) _levelsPanel.gameObject.SetActive(false);
        if (_gameOverPanel) _gameOverPanel.gameObject.SetActive(false);
        if (_gameWinPanel) _gameWinPanel.gameObject.SetActive(false);
        if (_inGameHUD) _inGameHUD.gameObject.SetActive(false);
    }

    public void ShowMainMenu()
    {
        ShowOnly(_mainMenuPanel);
    }

    public void ShowLevelsMenu()
    {
        RebuildLevelsGrid();
        ShowOnly(_levelsPanel);
    }

    public void ShowGameOver()
    {
        if (_gameOverScoreText) _gameOverScoreText.text = $"Score: {GameManager.Instance.GetScore()}";
        ShowOnly(_gameOverPanel);
    }

    public void ShowGameWin()
    {
        if (_gameWinScoreText) _gameWinScoreText.text = $"Score: {GameManager.Instance.GetScore()}";
        if (_nextLevelButton)
        {
            int next = GameManager.Instance.GetCurrentLevelIndex() + 1;
            bool canPlayNext = next < GameManager.Instance.GetLevelsCount() && GameManager.Instance.IsLevelUnlocked(next);
            _nextLevelButton.interactable = canPlayNext;
        }
        ShowOnly(_gameWinPanel);
    }

    public void ShowInGameHUD()
    {
        ShowOnly(_inGameHUD);
    }

    private void RebuildLevelsGrid()
    {
        // Find the content RectTransform under LevelsPanel
        var content = _levelsPanel.Find("ScrollView/Viewport/Content") as RectTransform;
        if (!content) return;

        foreach (Transform child in content)
        {
            Destroy(child.gameObject);
        }
        _levelButtons.Clear();

        int count = GameManager.Instance.GetLevelsCount();
        for (int i = 0; i < count; i++)
        {
            int levelIndex = i; // capture correctly for closures

            var item = new GameObject($"Level_{levelIndex}", typeof(Image));
            item.transform.SetParent(content, false);
            var bg = item.GetComponent<Image>();
            
            bool unlocked = GameManager.Instance.IsLevelUnlocked(levelIndex);
            int best = GameManager.Instance.GetBestScore(levelIndex);

            // Set background color based on unlock status
            if (unlocked)
            {
                bg.color = new Color(0.12f, 0.18f, 0.25f, 0.95f); // Dark blue for unlocked
            }
            else
            {
                bg.color = new Color(0.15f, 0.1f, 0.1f, 0.9f); // Dark red for locked
            }
            
            // Add subtle border effect
            var border = item.AddComponent<Outline>();
            border.effectColor = unlocked ? new Color(0.3f, 0.5f, 0.8f, 0.8f) : new Color(0.5f, 0.2f, 0.2f, 0.6f);
            border.effectDistance = new Vector2(2, 2);

            var layout = item.AddComponent<VerticalLayoutGroup>();
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.spacing = 8;
            layout.padding = new RectOffset(15, 15, 15, 15); // More generous padding

            var title = CreateText(item.transform, $"Level {levelIndex + 1}", 30); // Make it 1-indexed for display
            title.color = unlocked ? new Color(0.9f, 0.95f, 1f, 1f) : new Color(0.7f, 0.7f, 0.7f, 0.8f);
            title.fontStyle = FontStyle.Bold;
            title.rectTransform.sizeDelta = new Vector2(280, 40);

            var bestText = CreateText(item.transform, best > 0 ? $"Best: {best}" : "Not completed", 20);
            bestText.color = unlocked ? new Color(0.7f, 0.9f, 0.7f, 0.9f) : new Color(0.6f, 0.6f, 0.6f, 0.7f);
            bestText.rectTransform.sizeDelta = new Vector2(280, 30);

            var btn = CreateButton(item.transform, unlocked ? "Play" : "Locked", () =>
            {
                // Re-check lock at click time
                if (GameManager.Instance.IsLevelUnlocked(levelIndex))
                {
                    GameManager.Instance.NewGame(levelIndex);
                    ShowInGameHUD();
                }
            }, unlocked ? ButtonStyle.Level : ButtonStyle.Locked);
            
            btn.interactable = unlocked;
            _levelButtons.Add(btn);
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(content);
    }
}
