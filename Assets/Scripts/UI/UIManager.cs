using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

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
    [SerializeField] private TextMeshProUGUI _gameOverScoreText;
    [SerializeField] private TextMeshProUGUI _gameWinScoreText;
    [SerializeField] private Button _nextLevelButton;

    [Header("In-Game HUD")]
    [SerializeField] private TextMeshProUGUI _timerText;

    [Header("Levels Menu")]
    [SerializeField] private RectTransform _levelGridContent;
    [SerializeField] private GameObject _levelButtonPrefab;

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

        ShowMainMenu();
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
        // Play main menu music
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayMainMenuMusic();
        }
    }

    public void ShowLevelsMenu()
    {
        RebuildLevelsGrid();
        ShowOnly(_levelsPanel);
        // Play level select music (or main menu music if level select music is not set)
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayLevelSelectMusic();
        }
    }

    public void ShowGameOver()
    {
        if (_gameOverScoreText) _gameOverScoreText.text = $"Score: {GameManager.Instance.GetScore()}";
        ShowOnly(_gameOverPanel);
        // Play game over music
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayGameOverMusic();
        }
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
        // Play victory music
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayVictoryMusic();
        }
    }

    public void ShowInGameHUD()
    {
        ShowOnly(_inGameHUD);
        // Play gameplay music
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayGameplayMusic();
        }
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

            // Find components in the prefab instance
            var title = item.transform.Find("TitleText")?.GetComponent<TextMeshProUGUI>();
            var bestText = item.transform.Find("BestScoreText")?.GetComponent<TextMeshProUGUI>();
            var btn = item.transform.Find("PlayButton")?.GetComponent<Button>();
            var btnText = btn?.transform.Find("Text")?.GetComponent<TextMeshProUGUI>();
            var bg = item.GetComponent<Image>();
            var outline = item.GetComponent<Outline>();

            if (title)
            {
                title.text = $"Level {levelIndex + 1}";
                title.color = unlocked ? new Color(0.9f, 0.95f, 1f, 1f) : new Color(0.7f, 0.7f, 0.7f, 0.8f);
            }

            if (bestText)
            {
                bestText.text = best > 0 ? $"Best: {best}" : "Not completed";
                bestText.color = unlocked ? new Color(0.7f, 0.9f, 0.7f, 0.9f) : new Color(0.6f, 0.6f, 0.6f, 0.7f);
            }

            if (bg) bg.color = unlocked ? new Color(0.12f, 0.18f, 0.25f, 0.95f) : new Color(0.15f, 0.1f, 0.1f, 0.9f);
            if (outline) outline.effectColor = unlocked ? new Color(0.3f, 0.5f, 0.8f, 0.8f) : new Color(0.5f, 0.2f, 0.2f, 0.6f);

            if (btn)
            {
                if (btnText) btnText.text = unlocked ? "Play" : "Locked";
                
                btn.interactable = unlocked;
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() =>
                {
                    if (GameManager.Instance.IsLevelUnlocked(levelIndex))
                    {
                        GameManager.Instance.NewGame(levelIndex);
                        ShowInGameHUD();
                    }
                });
                _levelButtons.Add(btn);
            }
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(_levelGridContent);
    }

    // --- Public Methods for Button OnClick Events ---

    public void RetryLevel()
    {
        GameManager.Instance.RetryLevel();
        ShowInGameHUD();
    }

    public void PlayNextLevel()
    {
        if (GameManager.Instance.TryPlayNextLevel())
        {
            ShowInGameHUD();
        }
    }

    public void GoToLevelsMenuFromGame()
    {
        if (LevelManager.Instance != null) LevelManager.Instance.EndLevel();
        if (GameManager.Instance != null) GameManager.Instance.IsGameRunning = false;
        ShowLevelsMenu();
    }
}
