using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

[DisallowMultipleComponent]
public sealed class BadEndingVideo : MonoBehaviour
{
    [Header("UI / Video")]
    [SerializeField] private GameObject overlay;
    [SerializeField] private VideoPlayer player;
    [SerializeField] private Button skipButton;

    [Header("Hide these when playing (optional)")]
    [SerializeField] private GameObject[] toHide;

    void OnEnable()
    {
        if (GameCore.Instance) GameCore.Instance.onBadEnding.AddListener(OnBadEnding);
        if (skipButton) skipButton.onClick.AddListener(Skip);
        if (overlay) overlay.SetActive(false);
    }

    void OnDisable()
    {
        if (GameCore.Instance) GameCore.Instance.onBadEnding.RemoveListener(OnBadEnding);
        if (skipButton) skipButton.onClick.RemoveListener(Skip);

        if (player)
        {
            player.loopPointReached -= OnVideoEnd;
            player.errorReceived -= OnVideoError;
            player.prepareCompleted -= OnPrepared;
        }
    }

    void OnBadEnding()
    {
        if (toHide != null) foreach (var go in toHide) if (go) go.SetActive(false);
        if (overlay) overlay.SetActive(true);

        if (!player) return;
        player.playOnAwake = false;
        player.isLooping = false;

        player.loopPointReached -= OnVideoEnd;
        player.errorReceived -= OnVideoError;
        player.prepareCompleted -= OnPrepared;

        player.loopPointReached += OnVideoEnd;
        player.errorReceived += OnVideoError;
        player.prepareCompleted += OnPrepared;

        try { player.Stop(); } catch { }
        player.Prepare();
    }

    void OnPrepared(VideoPlayer vp)
    {
        try { vp.Play(); }
        catch { OnVideoEnd(vp); }
    }

    void OnVideoEnd(VideoPlayer _)
    {
        if (overlay) overlay.SetActive(false);
        GameCore.Instance?.HomeButton();
    }

    void OnVideoError(VideoPlayer vp, string message)
    {
        Debug.LogError("BadEndingVideo error: " + message);
        OnVideoEnd(vp);
    }

    void Skip()
    {
        if (player && player.isPlaying) player.Stop();
        OnVideoEnd(player);
    }
}
