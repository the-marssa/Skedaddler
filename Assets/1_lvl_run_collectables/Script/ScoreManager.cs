using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    [Header("Score")]
    [SerializeField, Min(0)] private int startScore = 0;
    [SerializeField, Min(0)] private int score;

    public int Score => score;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        score = startScore;
        // DontDestroyOnLoad(gameObject); // enable if you need persistence
    }

    public void Add(int value)
    {
        score += Mathf.Max(0, value);
    }
}
