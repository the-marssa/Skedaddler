using UnityEngine;
using System.Collections;
using VContainer;
using Dreamteck.Forever;

public class StartRunHook : MonoBehaviour
{
    [SerializeField] private GameObject startOverlay, pausePanel, hudRoot;
    [SerializeField] private Runner runner;

    private IRunSessionProvider _provider;
    [Inject] public void Construct(IRunSessionProvider p) => _provider = p;

    public void StartRun()
    {
        Time.timeScale = 1f;
        StartCoroutine(CoStartWhenReady());
        if (startOverlay) startOverlay.SetActive(false);
        if (pausePanel) pausePanel.SetActive(false);
        if (hudRoot) hudRoot.SetActive(true);
    }

    private IEnumerator CoStartWhenReady()
    {
        var gen = LevelGenerator.instance;
        if (gen == null)
        {
            Debug.LogError("LevelGenerator not found in scene");
            yield break;
        }

        
        if (!gen.ready) gen.StartGeneration();

        
        yield return new WaitUntil(() => gen.ready);

        
        _provider?.StartNew();

        if (runner) runner.StartFollow();
    }
}
