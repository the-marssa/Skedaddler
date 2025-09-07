using UnityEngine;
using TMPro;
using System.Collections;
using VContainer;

[DisallowMultipleComponent]
public class CheckpointToastUI : MonoBehaviour
{
    [SerializeField] private CheckpointManager checkpointManager;
    [SerializeField] private TMP_Text title;
    [SerializeField] private TMP_Text starsText;
    [SerializeField] private TMP_Text lettersText;
    [SerializeField] private CanvasGroup group;
    [SerializeField] private float fadeIn = 0.2f;
    [SerializeField] private float hold = 1.2f;
    [SerializeField] private float fadeOut = 0.25f;

    private IRunSessionProvider _provider;

    [Inject] public void Construct(IRunSessionProvider provider) => _provider = provider;

    void Awake()
    {
        if (!group) group = GetComponent<CanvasGroup>() ?? gameObject.AddComponent<CanvasGroup>();
        group.alpha = 0f;
    }

    void OnEnable() { if (checkpointManager) checkpointManager.onCheckpointReached.AddListener(OnCP); }
    void OnDisable() { if (checkpointManager) checkpointManager.onCheckpointReached.RemoveListener(OnCP); }

    void OnCP(int cp, int score)
    {
        StopAllCoroutines();
        var s = _provider?.Current;
        if (title) title.text = $"Checkpoint {cp}";
        if (starsText) starsText.text = $"Stars: {s?.Stars ?? score}";
        if (lettersText) lettersText.text = $"Letters: {s?.Letters ?? 0}";
        StartCoroutine(Show());
    }

    IEnumerator Show()
    {
        float t = 0f;
        while (t < fadeIn) { t += Time.unscaledDeltaTime; group.alpha = Mathf.Clamp01(t / fadeIn); yield return null; }
        float h = 0f;
        while (h < hold) { h += Time.unscaledDeltaTime; yield return null; }
        t = 0f;
        while (t < fadeOut) { t += Time.unscaledDeltaTime; group.alpha = 1f - Mathf.Clamp01(t / fadeOut); yield return null; }
        group.alpha = 0f;
    }
}
