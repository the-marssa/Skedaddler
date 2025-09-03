using UnityEngine;
using UnityEngine.Events;

[DefaultExecutionOrder(10)]
[DisallowMultipleComponent]
public class CheckpointManager : MonoBehaviour
{
    [Header("Score thresholds (per run)")]
    [SerializeField] private int[] scoreThresholds = new int[] { 10, 50, 100, 150 };

    [Header("Events")]
    [SerializeField] public UnityEvent<int, int> onCheckpointReached;

    private int nextIndex;                
    private long lastRunStartTicks = -1;   

    private void OnEnable()
    {
        TrySubscribe();
        ResetForCurrentRun(); 
    }

    private void OnDisable()
    {
        if (StatsManager.Instance != null)
        {
            StatsManager.Instance.OnScoreChanged -= HandleScoreChanged;
           
        }
    }

    private void TrySubscribe()
    {
        if (StatsManager.Instance == null) return;
        StatsManager.Instance.OnScoreChanged -= HandleScoreChanged;
        StatsManager.Instance.OnScoreChanged += HandleScoreChanged;
    }

    private void ResetForCurrentRun()
    {
        nextIndex = 0;
        var sm = StatsManager.Instance;
        lastRunStartTicks = (sm != null && sm.CurrentRun != null) ? sm.CurrentRun.startedAtTicks : -1;
    }

    private void HandleScoreChanged(int newScore)
    {
        var sm = StatsManager.Instance;
        if (sm == null || sm.CurrentRun == null) return;

        
        if (sm.CurrentRun.startedAtTicks != lastRunStartTicks)
        {
            ResetForCurrentRun();
        }

        if (nextIndex >= scoreThresholds.Length) return;

        
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
