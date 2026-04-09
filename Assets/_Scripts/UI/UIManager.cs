using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Reflection;
using Unity.VisualScripting;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Panels")]
    [SerializeField] private GameObject _mainMenuPanel;
    [SerializeField] private GameObject _levelsPanel;
    [SerializeField] private GameObject _gameOverPanel;
    [SerializeField] private GameObject _gameWinPanel;
    [SerializeField] private GameObject _inGameHUD;

    [Header("Game Over/Win UI")]
    [SerializeField] private TextMeshProUGUI _gameWinScoreText;
    [SerializeField] private GameObject _gameWinOneStar;
    [SerializeField] private GameObject _gameWinTwoStar;
    [SerializeField] private GameObject _gameWinThreeStar;
    [SerializeField] private Button _nextLevelButton;

    [Header("In-Game HUD")]
    [SerializeField] private TextMeshProUGUI _timerText;

    [Header("Levels Menu")]
    [SerializeField] private RectTransform _levelGridContent;
    [SerializeField] private GameObject _levelButtonPrefab;
    [SerializeField] private TextMeshProUGUI _totalStarsText;
    // Stars sprites (1/3, 2/3, 3/3)
    [SerializeField] private Sprite _star1;
    [SerializeField] private Sprite _star2;
    [SerializeField] private Sprite _star3;
    // Locked and Unlocked level sprites
    [SerializeField] private Sprite _lockedLevel;
    [SerializeField] private Sprite _unlockedLevel;




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

        GameEvents.TriggerShowMainMenu();
    }

    private void OnEnable()
    {
        GameEvents.OnShowMainMenu += ShowMainMenu;
        GameEvents.OnShowLevelsMenu += ShowLevelsMenu;
        GameEvents.OnGameOver += ShowGameOver;
        GameEvents.OnGameWin += ShowGameWin;
        GameEvents.OnShowInGameHUD += ShowInGameHUD;
    }

    private void OnDisable()
    {
        GameEvents.OnShowMainMenu -= ShowMainMenu;
        GameEvents.OnShowLevelsMenu -= ShowLevelsMenu;
        GameEvents.OnGameOver -= ShowGameOver;
        GameEvents.OnGameWin -= ShowGameWin;
        GameEvents.OnShowInGameHUD -= ShowInGameHUD;
    }

    private void Update()
    {
        // Update timer text when HUD is visible
        if (_inGameHUD != null && _inGameHUD.activeSelf && _timerText != null && GameManager.Instance != null)
        {
            _timerText.text = FormatTime(GameManager.Instance.GetTimeLeft());
        }
    }

    private string FormatTime(float timeSeconds)
    {
        if (timeSeconds < 0) timeSeconds = 0;
        int minutes = Mathf.FloorToInt(timeSeconds / 60f);
        int seconds = Mathf.FloorToInt(timeSeconds % 60f);
        return $"{minutes:00}:{seconds:00}";
    }

    private void HidePanel(GameObject panel)
    {
        if (panel != null) panel.SetActive(false);
    }

    private void ShowOnly(GameObject panelToShow)
    {
        if (_mainMenuPanel) _mainMenuPanel.SetActive(false);
        if (_levelsPanel) _levelsPanel.SetActive(false);
        if (_gameOverPanel) _gameOverPanel.SetActive(false);
        if (_gameWinPanel) _gameWinPanel.SetActive(false);
        if (_inGameHUD) _inGameHUD.SetActive(false);
        if (panelToShow) panelToShow.SetActive(true);
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
        int totalLevels = GameManager.Instance.GetLevelsCount();
        int totalCollectedStars = GameManager.Instance.GetTotalCollectedStars();
        _totalStarsText.text = $"{totalCollectedStars} / {totalLevels * 3}";
        RebuildLevelsGrid();
        ShowOnly(_levelsPanel);
    }

    public void ShowGameOver()
    {
        ShowOnly(_gameOverPanel);
    }

    public void ShowGameWin(int score, int stars)
    {
        if (_gameWinScoreText) _gameWinScoreText.text = $"Score - {score}";
        if (stars == 1)
        {
            // Enable the _gameWinOneStar gameobject
            if (_gameWinOneStar) _gameWinOneStar.SetActive(true);
            if (_gameWinTwoStar) _gameWinTwoStar.SetActive(false);
            if (_gameWinThreeStar) _gameWinThreeStar.SetActive(false);
        }
        else if (stars == 2)
        {
            if (_gameWinOneStar) _gameWinOneStar.SetActive(true);
            if (_gameWinTwoStar) _gameWinTwoStar.SetActive(true);
            if (_gameWinThreeStar) _gameWinThreeStar.SetActive(false);
        }
        else if (stars == 3)
        {
            if (_gameWinOneStar) _gameWinOneStar.SetActive(true);
            if (_gameWinTwoStar) _gameWinTwoStar.SetActive(true);
            if (_gameWinThreeStar) _gameWinThreeStar.SetActive(true);
        }
        else
        {
            if (_gameWinOneStar) _gameWinOneStar.SetActive(false);
            if (_gameWinTwoStar) _gameWinTwoStar.SetActive(false);
            if (_gameWinThreeStar) _gameWinThreeStar.SetActive(false);
        }
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
        if (!_levelGridContent || _levelButtonPrefab == null) return;

        foreach (Transform child in _levelGridContent)
        {
            Destroy(child.gameObject);
        }
        _levelButtons.Clear();

        int count = GameManager.Instance.GetLevelsCount();
        for (int i = 0; i < count; i++)
        {
            int levelIndex = i; // capture correctly for closures

            var item = Instantiate(_levelButtonPrefab, _levelGridContent);
            item.name = $"Level_{levelIndex}";
            
            bool unlocked = GameManager.Instance.IsLevelUnlocked(levelIndex);
            int best = GameManager.Instance.GetBestScore(levelIndex);
            int stars = GameManager.Instance.GetBestStars(levelIndex);

            // Find components in the prefab instance
            // var title = item.transform.Find("TitleText")?.GetComponent<TextMeshProUGUI>();
            // var bestText = item.transform.Find("BestScoreText")?.GetComponent<TextMeshProUGUI>();
            // var btn = item.transform.Find("PlayButton")?.GetComponent<Button>();
            // var btnText = btn?.transform.Find("Text")?.GetComponent<TextMeshProUGUI>();
            // var bg = item.GetComponent<Image>();
            // var outline = item.GetComponent<Outline>();
            var title = item.transform.Find("TitleText")?.GetComponent<TextMeshProUGUI>();
            var itemStars = item.transform.Find("Stars")?.GetComponent<Image>();
            var btn = item.GetComponent<Button>();            
            
            item.GetComponent<Image>().sprite = unlocked ? _unlockedLevel : _lockedLevel;
            if (title)
            {
                title.enabled = unlocked;
                title.text = $"{levelIndex + 1}";
            }

            if (itemStars)
            {
                itemStars.enabled = unlocked;
                if (stars == 3)
                {
                    itemStars.sprite = _star3;
                }
                else if (stars == 2)
                {
                    itemStars.sprite = _star2;
                }
                else if (stars == 1)
                {
                    itemStars.sprite = _star1;
                }
                else itemStars.enabled = false;
            }

            if (btn)
            {                
                btn.interactable = unlocked;
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() =>
                {
                    GameEvents.TriggerStartGameRequested(levelIndex);
                });
                _levelButtons.Add(btn);
            }
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(_levelGridContent);
    }

    // --- Public Methods for Button OnClick Events ---

    public void RetryLevel()
    {
        GameEvents.TriggerRetryLevelRequested();
    }

    public void PlayNextLevel()
    {
        GameEvents.TriggerPlayNextLevelRequested();
    }

    public void GoToLevelsMenuFromGame()
    {
        GameEvents.TriggerGoToLevelsMenuRequested();
    }
}
