using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using JSAM;

public class GameOverController : MonoBehaviour
{
    [Header("Restart without scene reload")]
    [SerializeField] private GameResetter resetter;

    [Tooltip("Fallback scene name from Build Settings")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    [Header("Hide visually while Game Over is visible (kept active)")]
    [SerializeField] private GameObject[] hideWhileOpen;

    [Header("Disable while Game Over is visible (SetActive)")]
    [SerializeField] private GameObject[] disableWhileOpen;

    [Header("Also auto-disable")]
    [SerializeField] private GameObject[] autoDisableWhileOpen; 

    [Header("Behaviour")]
    [SerializeField] private bool pauseOnEnable = true;
    [SerializeField] private bool muteAudioOnPause = false;

    [Header("Audio")]
    [SerializeField] private AudioClip deathClip;
    [SerializeField, Range(0f, 1f)] private float deathVolume = 1f;

    private readonly List<GameObject> autoDisabled = new();

    private void OnEnable()
    {
        if (pauseOnEnable) Time.timeScale = 0f;
        if (muteAudioOnPause) AudioListener.pause = true;

        SetHidden(true);
        SetDisabled(true);

        
        autoDisabled.Clear();
        if (autoDisableWhileOpen != null)
        {
            foreach (var go in autoDisableWhileOpen)
            {
                if (!go || !go.scene.IsValid()) continue;
                if (go.activeSelf) { go.SetActive(false); autoDisabled.Add(go); }
            }
        }

        
        try { AudioManager.StopAllMusic(); } catch { }

        if (deathClip) AudioSource.PlayClipAtPoint(deathClip, GetAudioPos(), deathVolume);
    }

    private void OnDisable()
    {
        foreach (var go in autoDisabled) if (go) go.SetActive(true);
        autoDisabled.Clear();

        SetDisabled(false);
        SetHidden(false);

        if (muteAudioOnPause) AudioListener.pause = false;
        if (pauseOnEnable) Time.timeScale = 1f;
    }

    public void TryAgain()
    {
        if (muteAudioOnPause) AudioListener.pause = false;
        Time.timeScale = 1f;

        if (resetter) resetter.RestartFromBeginning();
        else SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void ReturnToMainMenu()
    {
        
        if (StatsManager.Instance != null) StatsManager.Instance.CacheLastRunForMenu();

        if (muteAudioOnPause) AudioListener.pause = false;
        Time.timeScale = 1f;

        AppFlow.SkipNextIntro = true;
        if (resetter)
        {
            resetter.ReturnToMenu();
        }
        else
        {
            if (!string.IsNullOrEmpty(mainMenuSceneName))
                SceneManager.LoadScene(mainMenuSceneName);
            else
                SceneManager.LoadScene(0);
        }
    }

    private void SetHidden(bool hide)
    {
        if (hideWhileOpen == null) return;
        foreach (var go in hideWhileOpen)
        {
            if (!go) continue;
            var cg = go.GetComponent<CanvasGroup>() ?? go.AddComponent<CanvasGroup>();
            bool visible = !hide;
            cg.alpha = visible ? 1f : 0f;
            cg.interactable = visible;
            cg.blocksRaycasts = visible;
        }
    }

    private void SetDisabled(bool disabled)
    {
        if (disableWhileOpen == null) return;
        foreach (var go in disableWhileOpen)
            if (go) go.SetActive(!disabled);
    }

    private static Vector3 GetAudioPos()
    {
        if (Camera.main) return Camera.main.transform.position;
        var listener = Object.FindFirstObjectByType<AudioListener>(); 
        return listener ? listener.transform.position : Vector3.zero;
    }
}
