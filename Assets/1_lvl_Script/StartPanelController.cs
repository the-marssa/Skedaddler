using UnityEngine;
using JSAM;

public class StartPanelController : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject startPanel;

    [Header("HUD root (CanvasGroup will be toggled)")]
    [SerializeField] private GameObject hudRoot;

    [Header("Off initially")]
    [SerializeField] private GameObject[] mustBeOff;

    [Header("Disabled until Start")]
    [SerializeField] private GameObject[] disableGOs;
    [SerializeField] private Behaviour[] disableComponents;

    [Header("Also turn OFF on Start (e.g. PausePanel)")]
    [SerializeField] private GameObject[] offOnStart;

    [Header("Music on Start")]
    [SerializeField] private bool switchToRunMusic = true;
    [SerializeField] private Run_audiolibraryMusic runTrack = Run_audiolibraryMusic.Play_sfx;

    [SerializeField] private bool pauseWithTimeScale = true;
    private bool prepared;

    void OnEnable() => PrepareStart();

    public void PrepareStart()
    {
        if (mustBeOff != null) foreach (var go in mustBeOff) if (go) go.SetActive(false);
        if (startPanel) startPanel.SetActive(true);

        if (pauseWithTimeScale) Time.timeScale = 0f;

        if (disableGOs != null) foreach (var go in disableGOs) if (go) go.SetActive(false);
        if (disableComponents != null) foreach (var c in disableComponents) if (c) c.enabled = false;

        SetHudVisible(false);
        prepared = true;
    }

    public void StartGame()
    {
        if (!prepared) return;

        if (offOnStart != null) foreach (var go in offOnStart) if (go) go.SetActive(false);

        if (disableGOs != null) foreach (var go in disableGOs) if (go) go.SetActive(true);
        if (disableComponents != null) foreach (var c in disableComponents) if (c) c.enabled = true;

        SetHudVisible(true);

        if (pauseWithTimeScale) Time.timeScale = 1f;
        if (startPanel) startPanel.SetActive(false);

        if (disableGOs != null)
            foreach (var go in disableGOs) if (go) go.transform.SetAsLastSibling();

        if (switchToRunMusic)
        {
            if (!AudioManager.IsMusicPlaying(runTrack))  
            {
                AudioManager.FadeMainMusicOut(0.25f);     
                AudioManager.FadeMusicIn(runTrack, 0.35f, true); 
            }
        }

        prepared = false;
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
