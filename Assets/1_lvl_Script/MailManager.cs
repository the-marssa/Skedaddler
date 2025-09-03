using System;
using UnityEngine;

public class MailManager : MonoBehaviour
{
    public static MailManager Instance { get; private set; }

    public int Letters { get; private set; }
    public event Action<int> Changed;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void ResetCount()
    {
        Letters = 0;
        Changed?.Invoke(Letters);
    }

    public void Add(int amount)
    {
        int add = Mathf.Max(0, amount);
        Letters += add;
        Changed?.Invoke(Letters);
        if (add > 0 && StatsManager.Instance != null)
            StatsManager.Instance.AddLetters(add);
    }
}
