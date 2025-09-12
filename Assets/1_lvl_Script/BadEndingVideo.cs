using UnityEngine;
using UnityEngine.Video;

[DisallowMultipleComponent]
public class BadEndingVideo : MonoBehaviour
{
    [SerializeField] private VideoPlayer player;   
    [SerializeField] private GameObject overlay;   

    void Awake()
    {
        if (overlay) overlay.SetActive(false);
        if (player) player.playOnAwake = false;
    }

    void OnEnable()
    {
        if (player) player.loopPointReached += OnVideoEnd;
        if (GameCore.Instance) GameCore.Instance.onBadEnding.AddListener(OnBadEnding);
    }

    void OnDisable()
    {
        if (player) player.loopPointReached -= OnVideoEnd;
        if (GameCore.Instance) GameCore.Instance.onBadEnding.RemoveListener(OnBadEnding);
    }

    void OnBadEnding()
    {
        if (overlay) overlay.SetActive(true);
        if (player) player.Play();
    }

    void OnVideoEnd(VideoPlayer _)
    {
        if (overlay) overlay.SetActive(false);
        if (GameCore.Instance) GameCore.Instance.HomeButton();
    }
}
