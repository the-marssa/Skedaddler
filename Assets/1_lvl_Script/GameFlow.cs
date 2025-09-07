using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Dreamteck.Forever;

public sealed class GameFlow : MonoBehaviour
{
    [Header("Forever")]
    [SerializeField] private LevelGenerator generator;
    [SerializeField] private Runner runner;

    [Header("Player")]
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private PlayerDeath playerDeath;
    [SerializeField] private PlayerController playerController;
    [SerializeField] private Animator animator;
    [SerializeField] private Rigidbody rb;

    [Header("Panels / Roots")]
    [SerializeField] private GameObject startPanel;
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject mainMenuRoot;

    [Header("HUD (visible)")]
    [SerializeField] private GameObject hudRoot;
    [SerializeField] private GameObject stick;
    [SerializeField] private GameObject jump;
    [SerializeField] private GameObject hp;
    [SerializeField] private GameObject score;
    [SerializeField] private GameObject letter;
    [SerializeField] private GameObject magnetTimerGroup;
    [SerializeField] private GameObject shieldTimerGroup;
    [SerializeField] private GameObject gamePauseButton;

    [Header("Game Over UI")]
    [SerializeField] private GameObject gameOverLabel;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private float gameOverLabelTime = 0.8f;

    [Header("Session (optional)")]
    [SerializeField] private RunSessionProvider provider;

    [Header("Pause behaviour")]
    [SerializeField] private bool pauseUsesTimeScale = true;
    [SerializeField] private bool muteAudio = true;
    [SerializeField] private Behaviour[] disableComponentsWhilePaused;
    [SerializeField] private GameObject[] disableObjectsWhilePaused;

    [Header("Exit to Menu")]
    [SerializeField] private bool useInSceneMenu = true;
    [SerializeField] private string mainMenuSceneName = "MainMenu";
    [SerializeField] private int mainMenuBuildIndex = -1;

    private void OnEnable()
    {
        if (playerHealth) playerHealth.Died += OnPlayerDied;
    }

    private void OnDisable()
    {
        if (playerHealth) playerHealth.Died -= OnPlayerDied;
    }

    public void StartRun() => StartCoroutine(StartRunRoutine());
    public void RestartRun() => StartCoroutine(RestartRunRoutine());

    public void ReturnToMenu()
    {
        Unpause();
        Hide(pausePanel, gameOverLabel, gameOverPanel);
        HideRunHUD();
        generator.Clear();
        if (startPanel) startPanel.SetActive(false);
        if (mainMenuRoot) mainMenuRoot.SetActive(true);
    }

    public void ResetToStartPanel()
    {
        Unpause();
        Hide(pausePanel, gameOverLabel, gameOverPanel);
        HideRunHUD();
        generator.Clear();
        if (mainMenuRoot) mainMenuRoot.SetActive(false);
        if (startPanel) startPanel.SetActive(true);
    }

    public void TogglePause()
    {
        if (pausePanel && pausePanel.activeSelf) Resume();
        else Pause();
    }

    public void Pause()
    {
        if (pauseUsesTimeScale) Time.timeScale = 0f;
        if (muteAudio) AudioListener.pause = true;

        if (disableComponentsWhilePaused != null)
            foreach (var c in disableComponentsWhilePaused) if (c) c.enabled = false;
        if (disableObjectsWhilePaused != null)
            foreach (var go in disableObjectsWhilePaused) if (go) go.SetActive(false);

        SetHudVisible(false);
        if (pausePanel) pausePanel.SetActive(true);
        if (gamePauseButton) gamePauseButton.SetActive(false);
    }

    public void Resume()
    {
        if (pausePanel) pausePanel.SetActive(false);
        if (gamePauseButton) gamePauseButton.SetActive(true);

        if (disableComponentsWhilePaused != null)
            foreach (var c in disableComponentsWhilePaused) if (c) c.enabled = true;
        if (disableObjectsWhilePaused != null)
            foreach (var go in disableObjectsWhilePaused) if (go) go.SetActive(true);

        SetHudVisible(true);
        if (pauseUsesTimeScale) Time.timeScale = 1f;
        if (muteAudio) AudioListener.pause = false;
    }

    public void SaveAndExitToMenu()
    {
        try
        {
            StatsManager.Instance?.CacheLastRunForMenu();
            StatsManager.Instance?.EndRun(false);
        }
        catch { }

        if (pauseUsesTimeScale) Time.timeScale = 1f;
        if (muteAudio) AudioListener.pause = false;

        if (useInSceneMenu && mainMenuRoot != null)
        {
            Hide(pausePanel, gameOverLabel, gameOverPanel);
            HideRunHUD();
            generator.Clear();
            mainMenuRoot.SetActive(true);
            return;
        }

        if (mainMenuBuildIndex >= 0) SceneManager.LoadScene(mainMenuBuildIndex);
        else if (!string.IsNullOrEmpty(mainMenuSceneName)) SceneManager.LoadScene(mainMenuSceneName);
        else Debug.LogWarning("GameFlow: menu scene is not set (scene name/index).");
    }

    public void OnPlayerDied() => StartCoroutine(GameOverSequence());

    private IEnumerator StartRunRoutine()
    {
        Unpause();
        Hide(pausePanel, gameOverLabel, gameOverPanel, mainMenuRoot);
        if (startPanel) startPanel.SetActive(false);

        ShowRunHUD();
        provider?.StartNew();
        if (playerHealth) playerHealth.ResetFull();

        generator.Clear();
        yield return null;
        generator.StartGeneration();
        while (!generator.ready) yield return null;

        ResetPlayerStateForRun();
        StartRunnerGuaranteed();
    }

    private IEnumerator RestartRunRoutine()
    {
        Unpause();
        Hide(pausePanel, gameOverLabel, gameOverPanel, mainMenuRoot);
        if (startPanel) startPanel.SetActive(false);

        ShowRunHUD();
        provider?.StartNew();
        if (playerHealth) playerHealth.ResetFull();

        generator.Restart();
        yield return null;
        while (!generator.ready) yield return null;

        ResetPlayerStateForRun();
        StartRunnerGuaranteed();
    }

    private IEnumerator GameOverSequence()
    {
        Hide(pausePanel);
        if (gamePauseButton) gamePauseButton.SetActive(false);

        if (gameOverLabel) gameOverLabel.SetActive(true);
        yield return new WaitForSecondsRealtime(gameOverLabelTime);

        if (gameOverPanel) gameOverPanel.SetActive(true);
        Time.timeScale = 0f;
        AudioListener.pause = true;
    }

    private void ShowRunHUD()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;

        if (hudRoot)
        {
            hudRoot.SetActive(true);

            var cg = hudRoot.GetComponent<CanvasGroup>() ?? hudRoot.AddComponent<CanvasGroup>();
            cg.alpha = 1f;
            cg.interactable = true;
            cg.blocksRaycasts = true;

            var canvas = hudRoot.GetComponent<Canvas>();
            if (canvas) canvas.enabled = true;
        }

        Show(stick, jump, hp, score, letter, magnetTimerGroup, shieldTimerGroup, gamePauseButton);

        Canvas.ForceUpdateCanvases();
    }

    private void HideRunHUD()
    {
        if (hudRoot)
        {
            var cg = hudRoot.GetComponent<CanvasGroup>() ?? hudRoot.AddComponent<CanvasGroup>();
            cg.alpha = 0f;
            cg.interactable = false;
            cg.blocksRaycasts = false;
        }

        Hide(stick, jump, hp, score, letter, magnetTimerGroup, shieldTimerGroup, gamePauseButton);
    }

    private void SetHudVisible(bool v)
    {
        if (!hudRoot) return;
        var cg = hudRoot.GetComponent<CanvasGroup>() ?? hudRoot.AddComponent<CanvasGroup>();
        cg.alpha = v ? 1f : 0f;
        cg.interactable = v;
        cg.blocksRaycasts = v;
    }

    private static void Hide(params GameObject[] gos)
    {
        if (gos == null) return;
        foreach (var go in gos) if (go) go.SetActive(false);
    }

    private static void Show(params GameObject[] gos)
    {
        if (gos == null) return;
        foreach (var go in gos) if (go) go.SetActive(true);
    }

    private static void Unpause()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;
    }

    private void ResetPlayerStateForRun()
    {
        if (playerDeath) playerDeath.ResetAlive();
        if (playerController) playerController.ResetForRun(0f);

        if (rb)
        {
#if UNITY_6000_0_OR_NEWER
            rb.linearVelocity = Vector3.zero;
#else
            rb.velocity = Vector3.zero;
#endif
            rb.angularVelocity = Vector3.zero;

            bool transformMode = !runner || runner.physicsMode == Runner.PhysicsMode.Transform;
            rb.isKinematic = transformMode;
            rb.collisionDetectionMode = transformMode
                ? CollisionDetectionMode.ContinuousSpeculative
                : CollisionDetectionMode.Continuous;
            rb.detectCollisions = true;
        }

        if (animator)
        {
            animator.applyRootMotion = false;
            animator.Rebind();
            animator.Update(0f);
            animator.speed = 1f;
        }
    }

    private void StartRunnerGuaranteed()
    {
        if (!runner) return;

        runner.enabled = true;
        runner.startMode = Runner.StartMode.Percent;
        runner.startPercent = 0.0;

        runner.StartFollow();
        StartCoroutine(StartFollowNextFrame());
    }

    private IEnumerator StartFollowNextFrame()
    {
        yield return null;
        if (runner) runner.StartFollow();
    }
}
