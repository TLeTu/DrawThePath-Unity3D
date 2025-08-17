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

    private Button CreateButton(Transform parent, string label, Action onClick)
    {
        var go = new GameObject("Button", typeof(Image), typeof(Button));
        go.transform.SetParent(parent, false);
        var img = go.GetComponent<Image>();
        img.color = new Color(0.15f, 0.15f, 0.15f, 0.95f);
        var btn = go.GetComponent<Button>();
        btn.targetGraphic = img;
        btn.onClick.AddListener(() => onClick?.Invoke());

        // Ensure layout groups give the button enough space
        var le = go.GetComponent<LayoutElement>() ?? go.AddComponent<LayoutElement>();
        le.preferredWidth = 400;
        le.preferredHeight = 64; // compact default
        le.minHeight = 44;

        var text = CreateText(go.transform, label, 28);
        text.color = Color.white;
        text.alignment = TextAnchor.MiddleCenter;
        text.raycastTarget = false; // let button receive clicks
        text.rectTransform.anchorMin = Vector2.zero;
        text.rectTransform.anchorMax = Vector2.one;
        text.rectTransform.offsetMin = Vector2.zero;
        text.rectTransform.offsetMax = Vector2.zero;

        var rt = go.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(400, 64);
        return btn;
    }

    private void BuildMainMenu()
    {
        _mainMenuPanel = CreateFullPanel("MainMenu");
        var layout = _mainMenuPanel.gameObject.AddComponent<VerticalLayoutGroup>();
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.spacing = 24;
        layout.padding = new RectOffset(20, 20, 20, 20);

        var title = CreateText(_mainMenuPanel, "Draw The Path", 64);
        title.rectTransform.sizeDelta = new Vector2(800, 120);

        CreateButton(_mainMenuPanel, "Play", ShowLevelsMenu);

        HidePanel(_mainMenuPanel);
    }

    private void BuildLevelsMenu()
    {
        _levelsPanel = CreateFullPanel("LevelsMenu");

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
        headerLayout.padding = new RectOffset(20, 20, 20, 20);
        headerLayout.spacing = 20;

        var title = CreateText(header.transform, "Select Level", 48);
        title.rectTransform.sizeDelta = new Vector2(600, 100);

        var backBtn = CreateButton(header.transform, "Back", ShowMainMenu);
        // Prefer explicit layout sizing in header
        var backLE = backBtn.GetComponent<LayoutElement>();
        if (backLE != null)
        {
            backLE.preferredWidth = 240;
            backLE.preferredHeight = 70;
        }
        backBtn.GetComponent<RectTransform>().sizeDelta = new Vector2(240, 70);

        // Scroll area
        var scrollGO = new GameObject("ScrollView", typeof(Image), typeof(Mask), typeof(ScrollRect));
        scrollGO.transform.SetParent(_levelsPanel, false);
        var scrollRT = scrollGO.GetComponent<RectTransform>();
        scrollRT.anchorMin = new Vector2(0, 0);
        scrollRT.anchorMax = new Vector2(1, 1);
        scrollRT.offsetMin = new Vector2(40, 40);
        scrollRT.offsetMax = new Vector2(-40, -160);

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

        var grid = content.AddComponent<GridLayoutGroup>();
        grid.cellSize = new Vector2(360, 160); // increased height so title + best + button fit
        grid.spacing = new Vector2(24, 24);
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = 4;

        scroll.content = contentRT;

        // Populate levels on show
        HidePanel(_levelsPanel);
    }

    private void BuildGameOver()
    {
        _gameOverPanel = CreateFullPanel("GameOver");
        var layout = _gameOverPanel.gameObject.AddComponent<VerticalLayoutGroup>();
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.spacing = 24;
        layout.padding = new RectOffset(20, 20, 20, 20);

        CreateText(_gameOverPanel, "Game Over", 64);
        _gameOverScoreText = CreateText(_gameOverPanel, "Score: 0", 36);

        CreateButton(_gameOverPanel, "Retry", () => { GameManager.Instance.RetryLevel(); ShowInGameHUD(); });
        CreateButton(_gameOverPanel, "Level Select", ShowLevelsMenu);

        HidePanel(_gameOverPanel);
    }

    private void BuildGameWin()
    {
        _gameWinPanel = CreateFullPanel("GameWin");
        var layout = _gameWinPanel.gameObject.AddComponent<VerticalLayoutGroup>();
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.spacing = 24;
        layout.padding = new RectOffset(20, 20, 20, 20);

        CreateText(_gameWinPanel, "Level Complete!", 64);
        _gameWinScoreText = CreateText(_gameWinPanel, "Score: 0", 36);

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

        // Back button (top-left)
        var backBtn = CreateButton(_inGameHUD, "Back", () =>
        {
            if (LevelManager.Instance != null) LevelManager.Instance.EndLevel();
            if (GameManager.Instance != null) GameManager.Instance.IsGameRunning = false;
            ShowLevelsMenu();
        });
        var backRT = backBtn.GetComponent<RectTransform>();
        backRT.anchorMin = new Vector2(0, 1);
        backRT.anchorMax = new Vector2(0, 1);
        backRT.pivot = new Vector2(0, 1);
        backRT.anchoredPosition = new Vector2(20, -20);
        backRT.sizeDelta = new Vector2(160, 60);
        var backLE = backBtn.GetComponent<LayoutElement>();
        if (backLE != null)
        {
            backLE.preferredWidth = 160;
            backLE.preferredHeight = 60;
        }

        // Timer (top-right)
        _timerText = CreateText(_inGameHUD, "00:00", 36, TextAnchor.MiddleRight);
        var timerRT = _timerText.rectTransform;
        timerRT.anchorMin = new Vector2(1, 1);
        timerRT.anchorMax = new Vector2(1, 1);
        timerRT.pivot = new Vector2(1, 1);
        timerRT.anchoredPosition = new Vector2(-20, -20);
        timerRT.sizeDelta = new Vector2(220, 60);

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
            bg.color = new Color(0.08f, 0.08f, 0.08f, 0.9f);

            var layout = item.AddComponent<VerticalLayoutGroup>();
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.spacing = 6;
            layout.padding = new RectOffset(12, 12, 12, 12);

            bool unlocked = GameManager.Instance.IsLevelUnlocked(levelIndex);
            int best = GameManager.Instance.GetBestScore(levelIndex);

            var title = CreateText(item.transform, $"Level {levelIndex}", 28);

            var bestText = CreateText(item.transform, $"Best: {best}", 22);
            bestText.color = new Color(0.9f, 0.9f, 0.9f, 0.9f);

            var btn = CreateButton(item.transform, unlocked ? "Play" : "Locked", () =>
            {
                // Re-check lock at click time
                if (GameManager.Instance.IsLevelUnlocked(levelIndex))
                {
                    GameManager.Instance.NewGame(levelIndex);
                    ShowInGameHUD();
                }
            });
            btn.interactable = unlocked;
            _levelButtons.Add(btn);
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(content);
    }
}
