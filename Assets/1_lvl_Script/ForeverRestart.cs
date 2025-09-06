using System.Collections;
using UnityEngine;
using Dreamteck.Forever;
using VContainer;

public class ForeverRestart : MonoBehaviour
{
    [SerializeField] private Runner runner;

    private IRunSessionProvider _provider;
    [Inject] public void Construct(IRunSessionProvider p) => _provider = p;

    public void OnTryAgain()
    {
        Time.timeScale = 1f;
        LevelGenerator.instance.Restart();
        StartCoroutine(StartWhenReady());
    }

    public void HardRestart()
    {
        Time.timeScale = 1f;
        var gen = LevelGenerator.instance;
        gen.Clear();
        gen.StartGeneration();
        StartCoroutine(StartWhenReady());
    }

    private IEnumerator StartWhenReady()
    {
        yield return new WaitUntil(() => LevelGenerator.instance.ready);
        if (runner) runner.StartFollow();
        _provider?.StartNew();
    }
}
