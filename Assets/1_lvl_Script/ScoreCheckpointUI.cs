using System.Collections;
using UnityEngine;
using TMPro;

[DisallowMultipleComponent]
public class ScoreCheckpointUI : MonoBehaviour
{
    [Header("Thresholds (per run)")]
    [SerializeField] private int[] scoreThresholds = new int[] { 10, 50, 100, 150 };

    [Header("UI")]
    [SerializeField] private TMP_Text title;          
    [SerializeField] private TMP_Text statusText;     
    [SerializeField] private CanvasGroup group;
    [SerializeField] private float fadeIn = 0.2f;
    [SerializeField] private float hold = 1.2f;
    [SerializeField] private float fadeOut = 0.25f;

    [Header("Text")]
    [SerializeField] private string savedLabel = "Прогрес збережено";

    private int nextIndex;
    private long runStartTicks = -1;

    void Awake()
    {
        if (!group) group = GetComponent<CanvasGroup>() ?? gameObject.AddComponent<CanvasGroup>();
        group.alpha = 0f;
    }

    void OnEnable() => ResetForRun();

    void Update()
    {
        var gc = GameCore.Instance;
        if (gc == null || gc.CurrentRun == null) return;

        
        if (gc.CurrentRun.startedAtTicks != runStartTicks) ResetForRun();

        int score = gc.CurrentRun.score; 

        
        while (scoreThresholds != null &&
               nextIndex < scoreThresholds.Length &&
               score >= scoreThresholds[nextIndex])
        {
            int reachedValue = scoreThresholds[nextIndex]; 
            gc.CommitCheckpoint();
            ShowToast(reachedValue);
            nextIndex++;
        }
    }

    void ResetForRun()
    {
        var gc = GameCore.Instance;
        nextIndex = 0;
        runStartTicks = gc?.CurrentRun?.startedAtTicks ?? -1;
        if (group) group.alpha = 0f;
        if (title) title.text = "";
        if (statusText) statusText.text = "";
        StopAllCoroutines();
    }

    void ShowToast(int reachedValue)
    {
        if (title) title.text = reachedValue.ToString();            
        if (statusText) statusText.text = savedLabel;               
        StopAllCoroutines();
        StartCoroutine(FadeRoutine());
    }

    IEnumerator FadeRoutine()
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
