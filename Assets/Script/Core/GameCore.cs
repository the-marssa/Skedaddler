using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.Video;
using Dreamteck.Forever;

[DisallowMultipleComponent]
public sealed class GameCore : MonoBehaviour
{
    public static GameCore Instance { get; private set; }

    [Serializable]
    public class RunSnapshot
    {
        public int score, stars, letters;
        public int heartsPicked, hpLost;
        public int checkpoints;
        public float distance, seconds;
        public long startedAtTicks;
        public int defermentsEarned;
    }

    [Serializable]
    public class LifetimeStats
    {
        public long totalStars, totalLetters, totalHearts, totalScore;
        public long totalRuns, totalDeaths;
        public long deferments;
        public int bestScore, maxCheckpoint;
        public float bestDistance;
        public long lastPlayedTicks;
        public int starRemainder;
        public int dailyAdsGrants;
        public long lastAdResetMidnight;
        public long lastAdGrantTicks;
    }

    [Serializable]
    public class SaveBlob
    {
        public LifetimeStats lifetime = new LifetimeStats();
        public RunSnapshot lastRun;
    }

    [Header("Forever")]
    [SerializeField] private LevelGenerator generator;
    [SerializeField] private Runner runner;
    [SerializeField] private PlayerController player;
    [SerializeField] private PlayerDeath playerDeath;

    [Header("UI Roots")]
    [SerializeField] private GameObject mainMenuRoot;
    [SerializeField] private GameObject startPanelRoot;
    [SerializeField] private GameObject hudRoot;
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject gameOverPanel;

    [Header("Optional UI Roots")]
    [SerializeField] private GameObject checkpointRoot;

    [Header("Game Over UI")]
    [SerializeField] private GameObject gameOverLabel;
    [SerializeField] private GameObject gameOverButtons;
    [SerializeField] private float gameOverButtonsDelay = 2f;

    [Header("Main Menu Subpanels")]
    [SerializeField] private GameObject shopPanel;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject statsPanel;

    [Header("Home Menu Buttons (optional)")]
    [SerializeField] private GameObject playButton;
    [SerializeField] private GameObject shopButton;
    [SerializeField] private GameObject settingsButton;
    [SerializeField] private GameObject statsButton;

    [Header("Intro Video (optional)")]
    [SerializeField] private VideoPlayer introPlayer;
    [SerializeField] private GameObject introOverlay;
    [SerializeField] private bool playIntroAtStart = true;
    [SerializeField] private float introMinSkipDelay = 0.35f;

    [Header("Score / HP Rules")]
    [SerializeField] private int scorePerStar = 1;
    [SerializeField] private int scorePerDeferment = 100;
    [SerializeField] private int maxHP = 10;

    [Header("Input (optional)")]
    [SerializeField] private InputActionReference pauseAction;

    [Header("HUD widgets (optional)")]
    [SerializeField] private GameObject pauseIcon;

    [Header("Events")]
    public UnityEvent onRunStarted;
    public UnityEvent onRunEnded;
    public UnityEvent onCheckpoint;
    public UnityEvent onBadEnding;

    public SaveBlob Save { get; private set; } = new SaveBlob();
    public RunSnapshot CurrentRun { get; private set; }

    public int HP => _hp;
    public int MaxHP => maxHP;
    public bool IsRunActive => CurrentRun != null && !_paused;

    int _hp;
    bool _paused;
    bool _introActive;
    float _introTimer;

    public string SavePath => Path.Combine(Application.persistentDataPath, "save.json");

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        Load();
    }

    void Start()
    {
        DisableRunSystems();
        SafeSetActive(hudRoot, false);
        SafeSetActive(pausePanel, false);
        SafeSetActive(gameOverPanel, false);
        SafeSetActive(pauseIcon, false);

        if (playIntroAtStart && introPlayer && introOverlay)
        {
            SafeSetActive(mainMenuRoot, false);
            SafeSetActive(startPanelRoot, false);
            SafeSetActive(introOverlay, true);
            _introActive = true;
            _introTimer = 0f;

            introPlayer.loopPointReached += _ => EndIntro();
            try { introPlayer.Play(); } catch { EndIntro(); }
        }
        else ShowMainMenu();

        if (pauseAction)
        {
            pauseAction.action.performed += _ => TogglePause();
            pauseAction.action.Enable();
        }
    }

    void Update()
    {
        if (_introActive)
        {
            _introTimer += Time.unscaledDeltaTime;
            if (_introTimer >= introMinSkipDelay && AnySkipPressed()) EndIntro();
            return;
        }

        if (CurrentRun != null && !_paused)
            CurrentRun.seconds += Time.deltaTime;
    }

    bool AnySkipPressed()
    {
        var k = Keyboard.current;
        if (k != null && (k.escapeKey.wasPressedThisFrame || k.spaceKey.wasPressedThisFrame || k.enterKey.wasPressedThisFrame)) return true;
        var m = Mouse.current;
        if (m != null && (m.leftButton.wasPressedThisFrame || m.rightButton.wasPressedThisFrame)) return true;
        var t = Touchscreen.current;
        if (t != null && t.primaryTouch.press.wasPressedThisFrame) return true;
        var gp = Gamepad.current;
        if (gp != null && (gp.startButton.wasPressedThisFrame || gp.selectButton.wasPressedThisFrame || gp.buttonSouth.wasPressedThisFrame)) return true;
        return false;
    }

    void EndIntro()
    {
        if (!_introActive) return;
        _introActive = false;
        try { if (introPlayer) introPlayer.Stop(); } catch { }
        SafeSetActive(introOverlay, false);
        ShowMainMenu();
    }

    public void ShowMainMenu()
    {
        DisableRunSystems();
        _paused = false;
        Time.timeScale = 1f;

        SafeSetActive(hudRoot, false);
        SafeSetActive(pausePanel, false);
        SafeSetActive(gameOverPanel, false);
        HideAllSubPanels();

        SafeSetActive(mainMenuRoot, true);
        SafeSetActive(startPanelRoot, false);

        EnsureHomeButtonsVisible();

        Save.lifetime.lastPlayedTicks = DateTime.UtcNow.Ticks;
        SaveToDisk();
    }

    public void PlayButton()
    {
        DisableRunSystems();
        _paused = false;
        Time.timeScale = 1f;

        HideAllSubPanels();
        SafeSetActive(mainMenuRoot, false);
        SafeSetActive(startPanelRoot, true);

        SafeSetActive(hudRoot, false);
        SafeSetActive(pausePanel, false);
        SafeSetActive(gameOverPanel, false);
        SafeSetActive(pauseIcon, false);

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void StartRunButton()
    {
        SafeSetActive(startPanelRoot, false);
        BeginRun();
    }

    public void OpenShopButton() { HideAllSubPanels(); SafeSetActive(shopPanel, true); }
    public void BackFromShopButton() { SafeSetActive(shopPanel, false); ShowMainMenu(); }

    public void OpenSettingsButton() { HideAllSubPanels(); SafeSetActive(settingsPanel, true); }
    public void BackFromSettingsButton() { SafeSetActive(settingsPanel, false); ShowMainMenu(); }

    public void OpenStatsButton()
    {
        HideAllSubPanels();
        SafeSetActive(statsPanel, true);
        var ctrl = statsPanel ? statsPanel.GetComponentInChildren<StatsPanelController>(true) : null;
        if (ctrl) ctrl.Refresh();
    }
    public void BackFromStatsButton() { SafeSetActive(statsPanel, false); ShowMainMenu(); }

    public void PauseButton()
    {
        if (CurrentRun == null) return;
        _paused = true;
        Time.timeScale = 0f;
        SafeSetActive(pausePanel, true);
        SafeSetActive(pauseIcon, false);
    }

    public void ResumeButton()
    {
        if (CurrentRun == null) return;
        _paused = false;
        Time.timeScale = 1f;
        SafeSetActive(pausePanel, false);
        SafeSetActive(pauseIcon, true);
    }

    public void TogglePause()
    {
        if (CurrentRun == null) return;
        if (_paused) ResumeButton(); else PauseButton();
    }

    public void RestartFromGameOverButton()
    {
        SafeSetActive(gameOverPanel, false);
        ShowMainMenu();
        PlayButton();
    }

    public void HomeButton()
    {
        SafeSetActive(gameOverPanel, false);
        SafeSetActive(pausePanel, false);
        ShowMainMenu();
    }

    void HideAllSubPanels()
    {
        SafeSetActive(shopPanel, false);
        SafeSetActive(settingsPanel, false);
        SafeSetActive(statsPanel, false);
    }

    void EnsureHomeButtonsVisible()
    {
        SafeSetActive(playButton, true);
        SafeSetActive(shopButton, true);
        SafeSetActive(settingsButton, true);
        SafeSetActive(statsButton, true);
    }


    void BeginRun()
    {
        SafeSetActive(mainMenuRoot, false);
        SafeSetActive(startPanelRoot, false);
        SafeSetActive(hudRoot, true);
        SafeSetActive(pausePanel, false);
        SafeSetActive(gameOverPanel, false);
        SafeSetActive(pauseIcon, true);

        if (player) player.ResetStateHard();
        if (playerDeath) playerDeath.ResetAlive(); 

        if (generator) { generator.enabled = true; generator.Restart(); }
        if (runner) runner.enabled = true;

        CurrentRun = new RunSnapshot
        {
            startedAtTicks = DateTime.UtcNow.Ticks,
            seconds = 0f,
            distance = 0f,
            hpLost = 0,
            checkpoints = 0,
            score = 0,
            stars = 0,
            letters = 0,
            heartsPicked = 0,
            defermentsEarned = 0
        };

        _hp = maxHP;
        _paused = false;
        Time.timeScale = 1f;

        onRunStarted?.Invoke();
    }

    void DisableRunSystems()
    {
        if (generator) generator.enabled = false;
        if (runner) runner.enabled = false;
    }

    public void EndRun(bool died, bool badEnding = false)
    {
        if (CurrentRun == null) { ShowMainMenu(); return; }

        var lt = Save.lifetime;
        lt.totalRuns++; if (died) lt.totalDeaths++;
        lt.totalScore += CurrentRun.score;
        lt.totalStars += CurrentRun.stars;
        lt.totalLetters += CurrentRun.letters;
        lt.totalHearts += CurrentRun.heartsPicked;

        if (CurrentRun.score > lt.bestScore) lt.bestScore = CurrentRun.score;
        if (CurrentRun.distance > lt.bestDistance) lt.bestDistance = CurrentRun.distance;
        if (CurrentRun.checkpoints > lt.maxCheckpoint) lt.maxCheckpoint = CurrentRun.checkpoints;

        Save.lastRun = CurrentRun;
        SaveToDisk();

        DisableRunSystems();
        SafeSetActive(hudRoot, false);
        SafeSetActive(pausePanel, false);
        SafeSetActive(pauseIcon, false);

        if (badEnding)
        {
            onBadEnding?.Invoke();
        }
        else
        {
            SafeSetActive(gameOverPanel, true);
            SafeSetActive(gameOverLabel, true);
            SafeSetActive(gameOverButtons, false);
            StopAllCoroutines();
            StartCoroutine(ShowGameOverButtonsAfter(gameOverButtonsDelay));
        }

        onRunEnded?.Invoke();
        CurrentRun = null;
    }

    IEnumerator ShowGameOverButtonsAfter(float delay)
    {
        if (delay > 0f) yield return new WaitForSecondsRealtime(delay);
        SafeSetActive(gameOverButtons, true);
    }

    

    public void ApplyHit(int dmg = 1)
    {
        if (CurrentRun == null) return;
        int d = Mathf.Max(1, dmg);
        _hp = Mathf.Max(0, _hp - d);
        CurrentRun.hpLost += d;

        if (_hp <= 0)
        {
            if (playerDeath) playerDeath.Play();
            StopAllCoroutines();
            StartCoroutine(EndRunAfterRealtime(0.2f));
        }
    }

    IEnumerator EndRunAfterRealtime(float delay)
    {
        if (delay > 0f) yield return new WaitForSecondsRealtime(delay);
        EndRun(died: true, badEnding: false);
    }

    public void AddHeart(int amount = 1)
    {
        if (CurrentRun == null) return;
        int a = Mathf.Max(0, amount);
        CurrentRun.heartsPicked += a;
        _hp = Mathf.Clamp(_hp + a, 0, maxHP);
    }

    public void AddStar(int amount = 1)
    {
        if (CurrentRun == null) return;
        int a = Mathf.Max(1, amount);
        CurrentRun.stars += a;
        CurrentRun.score += a * scorePerStar;

        if (scorePerDeferment > 0)
        {
            int pool = Save.lifetime.starRemainder + a;
            int grants = pool / scorePerDeferment;
            Save.lifetime.starRemainder = pool % scorePerDeferment;
            if (grants > 0) Save.lifetime.deferments += grants;
        }
    }

    public void AddLetter(int amount = 1)
    {
        if (CurrentRun != null) CurrentRun.letters += Mathf.Max(0, amount);
    }

    public void CommitCheckpoint()
    {
        if (CurrentRun == null) return;
        CurrentRun.checkpoints = Mathf.Max(CurrentRun.checkpoints + 1, 1);
        SaveToDisk();
        onCheckpoint?.Invoke(); 
    }

    

    void Load()
    {
        try
        {
            if (File.Exists(SavePath))
            {
                string json = File.ReadAllText(SavePath);
                Save = JsonUtility.FromJson<SaveBlob>(json);
            }
        }
        catch { Save = new SaveBlob(); }
        if (Save == null) Save = new SaveBlob();
    }

    public void SaveToDisk()
    {
        try
        {
            var json = JsonUtility.ToJson(Save, true);
            File.WriteAllText(SavePath, json);
        }
        catch { /* ignore */ }
    }

    static void SafeSetActive(GameObject go, bool state)
    {
        if (go && go.activeSelf != state) go.SetActive(state);
    }
}
