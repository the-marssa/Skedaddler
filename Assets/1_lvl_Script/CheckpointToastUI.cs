using System.Collections;
using TMPro;
using UnityEngine;

[DisallowMultipleComponent]
public class CheckpointToastSimpleUI : MonoBehaviour
{
    [SerializeField] private CheckpointManager checkpointManager;
    [SerializeField] private TMP_Text title;
    [SerializeField] private TMP_Text starsText;
    [SerializeField] private TMP_Text lettersText;
    [SerializeField] private CanvasGroup group;

    [Header("Timing (unscaled time)")]
    [SerializeField, Min(0f)] private float fadeIn = 0.3f;
    [SerializeField, Min(0f)] private float hold = 1.5f;
    [SerializeField, Min(0f)] private float fadeOut = 0.3f;

    private Coroutine _cr;

    private void Awake()
    {
        if (!group) group = GetComponent<CanvasGroup>();
        if (group)
        {
            group.alpha = 0f;               
            group.interactable = false;
            group.blocksRaycasts = false;
        }
        
    }

    private void OnEnable()
    {
        if (checkpointManager != null)
            checkpointManager.onCheckpointReached.AddListener(OnCheckpoint);
    }

    private void OnDisable()
    {
        if (checkpointManager != null)
            checkpointManager.onCheckpointReached.RemoveListener(OnCheckpoint);
        if (_cr != null) { StopCoroutine(_cr); _cr = null; }
    }

    private void OnCheckpoint(int index, int score)
    {
        if (title) title.text = $"опнцпея гаепефемн";

        int stars = StatsManager.Instance != null ? StatsManager.Instance.CurrentRun.stars : score;
        int letters = StatsManager.Instance != null ? StatsManager.Instance.CurrentRun.letters : 0;

        if (starsText) starsText.text = stars.ToString();
        if (lettersText) lettersText.text = letters.ToString();

        if (_cr != null) StopCoroutine(_cr);
        _cr = StartCoroutine(ShowRoutine());
    }

    private IEnumerator ShowRoutine()
    {
      
        float t = 0f;
        while (t < fadeIn)
        {
            t += Time.unscaledDeltaTime;
            if (group) group.alpha = Mathf.Clamp01(t / Mathf.Max(0.0001f, fadeIn));
            yield return null;
        }
        if (group) group.alpha = 1f;

       
        yield return new WaitForSecondsRealtime(hold);

       
        t = 0f;
        while (t < fadeOut)
        {
            t += Time.unscaledDeltaTime;
            if (group) group.alpha = 1f - Mathf.Clamp01(t / Mathf.Max(0.0001f, fadeOut));
            yield return null;
        }
        if (group) group.alpha = 0f;

        _cr = null;
    }
}
