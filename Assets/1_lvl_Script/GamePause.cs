using UnityEngine;
using UnityEngine.SceneManagement;
using JSAM;

public class GamePause : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject hudRoot;

    [Header("Off while Pause")]
    [SerializeField] private Behaviour[] disableComponents;
    [SerializeField] private GameObject[] disableObjects;
    [SerializeField] private bool muteAudio = true;

    [Header("Run music")]
    [SerializeField] private Run_audiolibraryMusic runMusic = Run_audiolibraryMusic.Play_sfx;
    private bool musicWasPlaying;

    [Header("Int")]
    [SerializeField] private GameResetter resetter;

    private bool paused;

    void Awake()
    {
        if (pausePanel) pausePanel.SetActive(false);
        SetHudVisible(true);
    }

    public void Pause()
    {
        if (paused) return;
        paused = true;

        musicWasPlaying = AudioManager.IsMusicPlaying(runMusic);

        Time.timeScale = 0f;
        if (muteAudio) AudioListener.pause = true;

        if (disableComponents != null) foreach (var c in disableComponents) if (c) c.enabled = false;
        if (disableObjects != null) foreach (var go in disableObjects) if (go) go.SetActive(false);

        SetHudVisible(false);
        if (pausePanel) pausePanel.SetActive(true);
    }

    public void Resume()
    {
        if (!paused) return;
        paused = false;

        Time.timeScale = 1f;
        if (muteAudio) AudioListener.pause = false;

        if (disableComponents != null) foreach (var c in disableComponents) if (c) c.enabled = true;
        if (disableObjects != null) foreach (var go in disableObjects) if (go) go.SetActive(true);

        if (pausePanel) pausePanel.SetActive(false);
        SetHudVisible(true);

        if (musicWasPlaying && !AudioManager.IsMusicPlaying(runMusic))
            AudioManager.PlayMusic(runMusic, isMainMusic: true);
    }

    public void Restart()
    {
        UnpauseCleanup();

        if (resetter) resetter.RestartFromBeginning();
        else
        {
            var scene = SceneManager.GetActiveScene();
            SceneManager.LoadScene(scene.buildIndex);
        }
    }

    public void GoToMenu()
    {
        ExitWithoutSaving();
    }

    public void ExitWithoutSaving()
    {
        
        if (StatsManager.Instance != null) StatsManager.Instance.CacheLastRunForMenu();

        UnpauseCleanup();

        if (resetter)
        {
            AppFlow.SkipNextIntro = true;
            resetter.ReturnToMenu();
            return;
        }

        AppFlow.SkipNextIntro = true;
        SceneManager.LoadScene("MainMenu");
    }

    
    public void SaveAndExit()
    {
        if (StatsManager.Instance != null && StatsManager.Instance.CurrentRun != null)
        {
            StatsManager.Instance.EndRun(false); 
        }

       
        if (StatsManager.Instance != null) StatsManager.Instance.CacheLastRunForMenu();

        UnpauseCleanup();

        if (resetter)
        {
            AppFlow.SkipNextIntro = true;
            resetter.ReturnToMenu();
            return;
        }

        AppFlow.SkipNextIntro = true;
        SceneManager.LoadScene("MainMenu");
    }

    private void UnpauseCleanup()
    {
        paused = false;

        Time.timeScale = 1f;
        if (muteAudio) AudioListener.pause = false;

        if (disableComponents != null) foreach (var c in disableComponents) if (c) c.enabled = true;
        if (disableObjects != null) foreach (var go in disableObjects) if (go) go.SetActive(true);

        if (pausePanel) pausePanel.SetActive(false);
        SetHudVisible(true);

        if (musicWasPlaying && !AudioManager.IsMusicPlaying(runMusic))
            AudioManager.PlayMusic(runMusic, isMainMusic: true);
    }

    private void SetHudVisible(bool v)
    {
        if (!hudRoot) return;
        var cg = hudRoot.GetComponent<CanvasGroup>() ?? hudRoot.AddComponent<CanvasGroup>();
        cg.alpha = v ? 1f : 0f;
        cg.interactable = v;
        cg.blocksRaycasts = v;
    }
}
