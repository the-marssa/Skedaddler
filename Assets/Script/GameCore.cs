using System;
using System.IO;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.Video;
using Dreamteck.Forever;

[DisallowMultipleComponent]
public class GameCore : MonoBehaviour
{
    public static GameCore Instance { get; private set; }

    #region Data
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
    #endregion

    #region Refs
    [Header("Forever")]
    [SerializeField] private LevelGenerator generator;
    [SerializeField] private Runner runner;

    [Header("UI Roots")]
    [SerializeField] private GameObject mainMenuRoot;
    [SerializeField] private GameObject startPanelRoot;
    [SerializeField] private GameObject hudRoot;
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject gameOverPanel;

    [Header("Optional UI roots")]
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

    [Header("Intro Settings")]
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
    #endregion

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
    float _baseRunnerSpeed = -1f;
    float _speedFactor = 1f;
    bool _paused;
    bool _introActive;
    float _introTimer;

    #region Unity
    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        Load();
    }

    void Start()
    {
        DisableRunSystems();
        hudRoot?.SetActive(false);
        pauseIcon?.SetActive(false);
        pausePanel?.SetActive(false);
        gameOverPanel?.SetActive(false);

        if (playIntroAtStart && introPlayer && introOverlay)
        {
            mainMenuRoot?.SetActive(false);
            startPanelRoot?.SetActive(false);
            introOverlay.SetActive(true);
            _introActive = true;
            _introTimer = 0f;

            try { introPlayer.loopPointReached += _ => EndIntro(); introPlayer.Play(); }
            catch { EndIntro(); }
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

        if (CurrentRun != null && !_paused) TickTime(Time.deltaTime);
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
        if (introOverlay) introOverlay.SetActive(false);
        ShowMainMenu();
    }
    #endregion

    #region Menu
    void HideAllMenuPanels()
    {
        shopPanel?.SetActive(false);
        settingsPanel?.SetActive(false);
        statsPanel?.SetActive(false);
    }

    void EnsureHomeButtonsVisible()
    {
        if (playButton) playButton.SetActive(true);
        if (shopButton) shopButton.SetActive(true);
        if (settingsButton) settingsButton.SetActive(true);
        if (statsButton) statsButton.SetActive(true);
    }

    public void ShowMainMenu()
    {
        DisableRunSystems();
        _paused = false;
        Time.timeScale = 1f;

        hudRoot?.SetActive(false);
        pauseIcon?.SetActive(false);
        pausePanel?.SetActive(false);
        gameOverPanel?.SetActive(false);
        HideAllMenuPanels();

        if (mainMenuRoot) mainMenuRoot.SetActive(true);
        if (startPanelRoot) startPanelRoot.SetActive(false);

        EnsureHomeButtonsVisible();

        Save.lifetime.lastPlayedTicks = DateTime.UtcNow.Ticks;
        SaveToDisk();
    }

    
    public void PlayButton()
    {
        
        DisableRunSystems();
        _paused = false;
        Time.timeScale = 1f;

        HideAllMenuPanels();
        if (mainMenuRoot) mainMenuRoot.SetActive(false);
        if (startPanelRoot) startPanelRoot.SetActive(true);

        hudRoot?.SetActive(false);
        pauseIcon?.SetActive(false);
        pausePanel?.SetActive(false);
        gameOverPanel?.SetActive(false);

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void StartRunButton()
    {
        startPanelRoot?.SetActive(false);
        BeginRun();
    }

    public void OpenShopButton() { HideAllMenuPanels(); shopPanel?.SetActive(true); }
    public void BackFromShopButton() { shopPanel?.SetActive(false); ShowMainMenu(); }

    public void OpenSettingsButton() { HideAllMenuPanels(); settingsPanel?.SetActive(true); }
    public void BackFromSettingsButton() { settingsPanel?.SetActive(false); ShowMainMenu(); }

    public void OpenStatsButton()
    {
        HideAllMenuPanels();
        statsPanel?.SetActive(true);
        var ctrl = statsPanel ? statsPanel.GetComponentInChildren<StatsPanelController>(true) : null;
        if (ctrl) ctrl.Refresh();
    }
    public void BackFromStatsButton() { statsPanel?.SetActive(false); ShowMainMenu(); }

    public void ResetTotalScoreButton()
    {
        Save.lifetime.totalScore = 0;
        SaveToDisk();
        var ctrl = statsPanel ? statsPanel.GetComponentInChildren<StatsPanelController>(true) : null;
        if (ctrl) ctrl.Refresh();
    }

    public void PauseButton()
    {
        if (CurrentRun == null) return;
        _paused = true;
        Time.timeScale = 0f;
        pauseIcon?.SetActive(false);
        pausePanel?.SetActive(true);
    }

    public void ResumeButton()
    {
        if (CurrentRun == null) return;
        _paused = false;
        Time.timeScale = 1f;
        pausePanel?.SetActive(false);
        pauseIcon?.SetActive(true);
    }

    public void TogglePause()
    {
        if (CurrentRun == null) return;
        if (_paused) ResumeButton(); else PauseButton();
    }

    public void RestartFromGameOverButton()
    {
        gameOverPanel?.SetActive(false);
        ShowMainMenu();
        PlayButton();
    }

    public void HomeButton()
    {
        gameOverPanel?.SetActive(false);
        pausePanel?.SetActive(false);
        ShowMainMenu();
    }
    public void ExitToMenuButton() => HomeButton();
    #endregion

    #region Run Loop
    float GetRunnerSpeed()
    {
        if (!runner) runner = FindFirstObjectByType<Runner>(FindObjectsInactive.Include);
        if (runner)
        {
            var t = runner.GetType();
            var f = t.GetField("moveSpeed") ?? t.GetField("speed") ?? t.GetField("Speed") ?? t.GetField("MoveSpeed");
            if (f != null) return Convert.ToSingle(f.GetValue(runner));
            var p = t.GetProperty("moveSpeed") ?? t.GetProperty("speed") ?? t.GetProperty("Speed") ?? t.GetProperty("MoveSpeed");
            if (p != null) return Convert.ToSingle(p.GetValue(runner));
        }

        if (!generator) generator = FindFirstObjectByType<LevelGenerator>(FindObjectsInactive.Include);
        if (generator)
        {
            var t = generator.GetType();
            var f = t.GetField("moveSpeed") ?? t.GetField("speed") ?? t.GetField("Speed") ?? t.GetField("MoveSpeed");
            if (f != null) return Convert.ToSingle(f.GetValue(generator));
            var p = t.GetProperty("moveSpeed") ?? t.GetProperty("speed") ?? t.GetProperty("Speed") ?? t.GetProperty("MoveSpeed");
            if (p != null) return Convert.ToSingle(p.GetValue(generator));
        }
        return -1f;
    }

    void SetRunnerSpeed(float v)
    {
        bool applied = false;

        if (!runner) runner = FindFirstObjectByType<Runner>(FindObjectsInactive.Include);
        if (runner)
        {
            var t = runner.GetType();
            var f = t.GetField("moveSpeed") ?? t.GetField("speed") ?? t.GetField("Speed") ?? t.GetField("MoveSpeed");
            if (f != null) { f.SetValue(runner, v); applied = true; }
            else
            {
                var p = t.GetProperty("moveSpeed") ?? t.GetProperty("speed") ?? t.GetProperty("Speed") ?? t.GetProperty("MoveSpeed");
                if (p != null && p.CanWrite) { p.SetValue(runner, v); applied = true; }
            }
        }

        if (!applied)
        {
            if (!generator) generator = FindFirstObjectByType<LevelGenerator>(FindObjectsInactive.Include);
            if (generator)
            {
                var t = generator.GetType();
                var f = t.GetField("moveSpeed") ?? t.GetField("speed") ?? t.GetField("Speed") ?? t.GetField("MoveSpeed");
                if (f != null) { f.SetValue(generator, v); applied = true; }
                else
                {
                    var p = t.GetProperty("moveSpeed") ?? t.GetProperty("speed") ?? t.GetProperty("Speed") ?? t.GetProperty("MoveSpeed");
                    if (p != null && p.CanWrite) { p.SetValue(generator, v); applied = true; }
                }
            }
        }
    }

    void ResetPlayerForNewRun()
    {
        try
        {
            var death = FindFirstObjectByType<PlayerDeath>(FindObjectsInactive.Include);
            if (death != null)
            {
                var m = death.GetType().GetMethod("ResetAlive") ?? death.GetType().GetMethod("Reset");
                if (m != null) m.Invoke(death, null);
            }
        }
        catch { }

        try
        {
            var pc = FindFirstObjectByType<PlayerController>(FindObjectsInactive.Include);
            if (pc)
            {
                var t = pc.GetType();
                var m = t.GetMethod("ResetForRun", new[] { typeof(float) }) ?? t.GetMethod("ResetForRun", Type.EmptyTypes);
                if (m != null)
                {
                    if (m.GetParameters().Length == 1) m.Invoke(pc, new object[] { 0f });
                    else m.Invoke(pc, null);
                }

                var rb = pc.GetComponent<Rigidbody>();
                if (rb)
                {
#if UNITY_6000_0_OR_NEWER
                    rb.linearVelocity = Vector3.zero;
#else
                    rb.velocity = Vector3.zero;
#endif
                    rb.angularVelocity = Vector3.zero;
                    rb.isKinematic = false;
                }

                var anim = pc.GetComponentInChildren<Animator>(true);
                if (anim) { anim.Rebind(); anim.Update(0f); }
            }
        }
        catch { }
    }

    void BeginRun()
    {
        mainMenuRoot?.SetActive(false);
        startPanelRoot?.SetActive(false);
        hudRoot?.SetActive(true);
        pauseIcon?.SetActive(true);
        pausePanel?.SetActive(false);
        gameOverPanel?.SetActive(false);

        ResetPlayerForNewRun();

        if (!generator) generator = FindFirstObjectByType<LevelGenerator>(FindObjectsInactive.Include);
        if (!runner) runner = FindFirstObjectByType<Runner>(FindObjectsInactive.Include);
        if (generator) generator.enabled = true;
        if (runner) runner.enabled = true;

        try
        {
            var genType = generator ? generator.GetType() : null;
            var restartM = genType?.GetMethod("Restart") ?? genType?.GetMethod("Reset");
            if (generator && restartM != null) restartM.Invoke(generator, null);
        }
        catch { }

        if (_baseRunnerSpeed < 0f) _baseRunnerSpeed = GetRunnerSpeed();
        if (_baseRunnerSpeed >= 0f) SetRunnerSpeed(_baseRunnerSpeed);
        _speedFactor = 1f;

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

    public void TickTime(float delta) { if (CurrentRun != null && !_paused) CurrentRun.seconds += Mathf.Max(0f, delta); }
    public void TickDistance(float delta) { if (CurrentRun != null && !_paused) CurrentRun.distance += Mathf.Max(0f, delta); }

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
        hudRoot?.SetActive(false);
        pauseIcon?.SetActive(false);
        pausePanel?.SetActive(false);

        if (badEnding)
        {
            onBadEnding?.Invoke();
        }
        else
        {
            gameOverPanel?.SetActive(true);
            gameOverLabel?.SetActive(true);
            if (gameOverButtons) gameOverButtons.SetActive(false);
            StopAllCoroutines();
            StartCoroutine(ShowGameOverButtonsAfter(gameOverButtonsDelay));
        }

        onRunEnded?.Invoke();
        CurrentRun = null;
    }

    IEnumerator ShowGameOverButtonsAfter(float delay)
    {
        if (delay > 0f) yield return new WaitForSecondsRealtime(delay);
        gameOverButtons?.SetActive(true);
    }

    IEnumerator EndRunAfter(float delay)
    {
        if (delay > 0f) yield return new WaitForSecondsRealtime(delay);
        EndRun(died: true);
    }
    #endregion

    #region Gameplay API
    public void ApplyHit(int dmg = 1)
    {
        if (CurrentRun == null) return;
        int d = Mathf.Max(1, dmg);
        _hp = Mathf.Max(0, _hp - d);
        CurrentRun.hpLost += d;

        if (_hp <= 0)
        {
            DisableRunSystems();
            Time.timeScale = 1f;
            _paused = false;

            float delay = 0.2f;
            try
            {
                var death = FindFirstObjectByType<PlayerDeath>(FindObjectsInactive.Include);
                if (death != null)
                {
                    var play = death.GetType().GetMethod("Play");
                    if (play != null) play.Invoke(death, null);

                    var durProp = death.GetType().GetProperty("Duration");
                    if (durProp != null) delay = Mathf.Max(0.05f, Convert.ToSingle(durProp.GetValue(death)));
                }
            }
            catch { }

            StopAllCoroutines();
            StartCoroutine(EndRunAfter(delay));
        }
    }

    public void AddHeart(int amount = 1)
    {
        if (CurrentRun == null) return;
        CurrentRun.heartsPicked += Mathf.Max(0, amount);
        _hp = Mathf.Clamp(_hp + amount, 0, maxHP);
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

    public void SetSpeedFactor(float factor)
    {
        if (_baseRunnerSpeed < 0f) _baseRunnerSpeed = GetRunnerSpeed();
        if (_baseRunnerSpeed < 0f) return;

        _speedFactor = Mathf.Max(0.05f, factor);
        SetRunnerSpeed(_baseRunnerSpeed * _speedFactor);
    }
    #endregion

    #region Save/Load
    public string SavePath => Path.Combine(Application.persistentDataPath, "save.json");

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
    #endregion
}
