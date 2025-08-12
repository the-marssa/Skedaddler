using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class HUDController : MonoBehaviour
{
    [Header("Score")]
    [SerializeField] TextMeshProUGUI scoreText;

    [Header("HP")]
    [SerializeField] Slider hpSlider;         
    [SerializeField] TextMeshProUGUI hpText; 

    PlayerHealth health;

    void Start()
    {
        var player = GameObject.FindGameObjectWithTag("Player");
        health = player.GetComponent<PlayerHealth>();

        hpSlider.minValue = 0;
        hpSlider.maxValue = health.Max;
        hpSlider.value = health.Current;
        UpdateHpText(health.Current, health.Max);

        health.Changed += OnHealthChanged;

        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.Changed += OnScoreChanged;
            OnScoreChanged(ScoreManager.Instance.Score);
        }
    }

    void OnDestroy()
    {
        if (health != null) health.Changed -= OnHealthChanged;
        if (ScoreManager.Instance != null) ScoreManager.Instance.Changed -= OnScoreChanged;
    }

    void OnHealthChanged(int current, int max)
    {
        if (hpSlider.maxValue != max) hpSlider.maxValue = max;
        hpSlider.value = current;
        UpdateHpText(current, max);
    }

    void UpdateHpText(int current, int max)
    {
        if (hpText != null) hpText.text = $"{current}/{max}";
    }

    void OnScoreChanged(int value)
    {
        if (scoreText != null) scoreText.text = $"x {value}";
    }
}
