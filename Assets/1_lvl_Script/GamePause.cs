using UnityEngine;
using UnityEngine.SceneManagement;

public class GamePause : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject hudRoot;

    [Header("While Paused")]
    [SerializeField] private Behaviour[] disableComponents;
    [SerializeField] private GameObject[] disableObjects;
    [SerializeField] private bool muteAudio = true;

    [Header("Exit to Menu")]
    [SerializeField] private string MainMenu = "MainMenu";
    [SerializeField] private int MainMenuBuildIndex = -1;

    private bool paused;

    private void Awake()
    {
        if (pausePanel) pausePanel.SetActive(false);
        SetHudVisible(true);
    }

    public void Toggle() { if (paused) Resume(); else Pause(); }

    public void Pause()
    {
        if (paused) return; paused = true;

        Time.timeScale = 0f;
        if (muteAudio) AudioListener.pause = true;

        if (disableComponents != null) foreach (var c in disableComponents) if (c) c.enabled = false;
        if (disableObjects != null) foreach (var go in disableObjects) if (go) go.SetActive(false);

        SetHudVisible(false);
        if (pausePanel) pausePanel.SetActive(true);
    }

    public void Resume()
    {
        if (!paused) return; paused = false;

        if (pausePanel) pausePanel.SetActive(false);
        SetHudVisible(true);

        if (disableComponents != null) foreach (var c in disableComponents) if (c) c.enabled = true;
        if (disableObjects != null) foreach (var go in disableObjects) if (go) go.SetActive(true);

        Time.timeScale = 1f;
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

        Time.timeScale = 1f;
        if (muteAudio) AudioListener.pause = false;

        if (MainMenuBuildIndex >= 0) SceneManager.LoadScene(MainMenuBuildIndex);
        else if (!string.IsNullOrEmpty(MainMenu)) SceneManager.LoadScene(MainMenu);
        else Debug.LogWarning("GamePause: MainMenu scene not set");
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
