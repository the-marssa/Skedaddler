using System.Collections;
using UnityEngine;
using JSAM;

[DefaultExecutionOrder(1000)]
public class SceneMusic : MonoBehaviour
{
    [SerializeField] private Run_audiolibraryMusic track = Run_audiolibraryMusic.Menu_sfx;
    [SerializeField] private bool fadeIn = true;
    [SerializeField] private float fadeInTime = 0.35f;
    [SerializeField] private int warmupFrames = 2;
    [SerializeField] private float extraDelay = 0.05f;

    private void OnEnable() => StartCoroutine(PlayWhenReady());

    private IEnumerator PlayWhenReady()
    {
        while (AudioManager.Instance == null) yield return null;
        for (int i = 0; i < warmupFrames; i++) yield return null;
        if (extraDelay > 0f) yield return new WaitForSecondsRealtime(extraDelay);

        if (AudioManager.TryGetPlayingMusic(track, out _)) yield break;

      
        if (fadeIn && fadeInTime > 0f && Time.timeScale > 0f)
            AudioManager.FadeMusicIn(track, fadeInTime, true);
        else
            AudioManager.PlayMusic(track, true);
    }
}
