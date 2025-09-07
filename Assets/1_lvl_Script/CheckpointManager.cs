using UnityEngine;
using UnityEngine.Events;

[DefaultExecutionOrder(10)]
[DisallowMultipleComponent]
public class CheckpointManager : MonoBehaviour
{
    [Header("Score thresholds (per run)")]
    [SerializeField] private int[] scoreThresholds = new int[] { 10, 50, 100, 150 };

    [Header("Events")]
    public UnityEvent<int, int> onCheckpointReached; 

    int nextIndex;
    long lastRunStartTicks = -1;

    void OnEnable() { TrySubscribe(); ResetForCurrentRun(); }
    void OnDisable() { if (StatsManager.Instance != null) StatsManager.Instance.OnScoreChanged -= HandleScoreChanged; }

    void TrySubscribe()
    {
        var sm = StatsManager.Instance;
        if (sm == null) return;
        sm.OnScoreChanged -= HandleScoreChanged;
        sm.OnScoreChanged += HandleScoreChanged;
    }

    void ResetForCurrentRun()
    {
        var sm = StatsManager.Instance;
        if (sm == null || sm.CurrentRun == null) { nextIndex = 0; lastRunStartTicks = -1; return; }
        if (sm.CurrentRun.startedAtTicks != lastRunStartTicks) { nextIndex = 0; lastRunStartTicks = sm.CurrentRun.startedAtTicks; }
    }

    void HandleScoreChanged(int newScore)
    {
        var sm = StatsManager.Instance;
        if (sm == null || sm.CurrentRun == null || scoreThresholds == null) return;

        while (nextIndex < scoreThresholds.Length && newScore >= scoreThresholds[nextIndex])
        {
            int cpIndex = nextIndex + 1;
            if (sm.CurrentRun.checkpointsReached < cpIndex)
            {
                sm.CommitCheckpoint(cpIndex);
                onCheckpointReached?.Invoke(cpIndex, newScore);
            }
            nextIndex++;
        }
    }
}
