using UnityEngine;

public class StartRunHook : MonoBehaviour
{
    public void StartRun()
    {
        if (StatsManager.Instance != null)
            StatsManager.Instance.StartRun();
    }
}
