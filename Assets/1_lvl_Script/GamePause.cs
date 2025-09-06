using UnityEngine;
using UnityEngine.SceneManagement;

public class GamePause : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject hudRoot;

    [Header("Off while Pause")]
    [SerializeField] private Behaviour[] disableComponents;
    [SerializeField] private GameObject[] disableObjects;
    [SerializeField] private bool muteAudio = true;

    [Header("Optional")]
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
    }

    public void Restart()
    {
        
        if (resetter)
        {
            Resume();               
            resetter.RestartFromBeginning();
            return;
        }

        
        Time.timeScale = 1f;
        if (muteAudio) AudioListener.pause = false;

        var scene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(scene.buildIndex);
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
