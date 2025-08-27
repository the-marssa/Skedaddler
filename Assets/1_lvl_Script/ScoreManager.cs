using UnityEngine;
using System;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }
    public int Score { get; private set; }
    public event Action<int> Changed;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void Add(int amount) { Score += amount; Changed?.Invoke(Score); }
    public void ResetScore() { Score = 0; Changed?.Invoke(Score); }
}
