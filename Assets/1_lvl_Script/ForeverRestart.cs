using System.Collections;
using UnityEngine;
using Dreamteck.Forever;

public class ForeverRestart : MonoBehaviour
{
    [SerializeField] private Runner runner;

    public void OnTryAgain()
    {
        Time.timeScale = 1f;
        LevelGenerator.instance.Restart();
        StartCoroutine(StartWhenReady());
    }

    private IEnumerator StartWhenReady()
    {
        yield return new WaitUntil(() => LevelGenerator.instance.ready);
        if (runner) runner.StartFollow();
    }

    public void HardRestart()
    {
        Time.timeScale = 1f;
        var gen = LevelGenerator.instance;
        gen.Clear();
        gen.StartGeneration();
        StartCoroutine(StartWhenReady());
    }
}
