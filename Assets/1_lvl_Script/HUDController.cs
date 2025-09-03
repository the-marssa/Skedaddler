using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;
using System.Collections;

public class HUDController : MonoBehaviour
{
    [Header("Score")]
    [SerializeField] private TextMeshProUGUI scoreText;

    [Header("HP UI")]
    [SerializeField] private Slider hpSlider;
    [SerializeField] private TextMeshProUGUI hpText;

    [Header("Letters")]
    [SerializeField] private TextMeshProUGUI lettersText;

    [Header("References")]
    [SerializeField] private GameObject player;
    [SerializeField] private PlayerHealth health;

    [Header("Game Over UI")]
    [SerializeField] private TextMeshProUGUI gameOverLabel;
    [SerializeField] private CanvasGroup gameOverLabelGroup;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private bool pauseOnGameOver = true;

    [Header("Hide on Game Over (optional, kept active)")]
    [SerializeField] private GameObject[] hideOnGameOver;

    [Header("Timings (unscaled)")]
    [SerializeField] private float labelFadeInDuration = 1f;
    [SerializeField] private float labelHoldSeconds = 3f;
    [SerializeField] private float labelFadeOutDuration = 1f;

    private const int HpMin = 0;
    private const int HpMax = 20;

    private Action<int, int> _healthChangedHandler;
    private bool _gameOverStarted;
    private Coroutine _gameOverCR;

    private void Start()
    {
        if (hpSlider != null) { hpSlider.minValue = HpMin; hpSlider.maxValue = HpMax; }

        if (health != null)
        {
            int startVal = Mathf.Clamp(health.Current, HpMin, HpMax);
            if (hpSlider != null) hpSlider.value = startVal;
            UpdateHpText(startVal);
            _healthChangedHandler = (current, _) => OnHealthChanged(current);
            health.Changed += _healthChangedHandler;
        }

        if (MailManager.Instance != null)
        {
            MailManager.Instance.Changed += OnLettersChanged;
            OnLettersChanged(MailManager.Instance.Letters);
        }

        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.Changed += OnScoreChanged;
            OnScoreChanged(ScoreManager.Instance.Score);
        }

        if (gameOverLabel != null) gameOverLabel.gameObject.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (gameOverLabelGroup != null) gameOverLabelGroup.alpha = 0f;
    }

    private void OnDisable()
    {
        if (_gameOverCR != null) { StopCoroutine(_gameOverCR); _gameOverCR = null; }
    }

    private void OnDestroy()
    {
        if (health != null && _healthChangedHandler != null) health.Changed -= _healthChangedHandler;
        if (ScoreManager.Instance != null) ScoreManager.Instance.Changed -= OnScoreChanged;
        if (MailManager.Instance != null) MailManager.Instance.Changed -= OnLettersChanged;
    }

    private void OnHealthChanged(int current)
    {
        int clamped = Mathf.Clamp(current, HpMin, HpMax);
        if (hpSlider != null) hpSlider.value = clamped;
        UpdateHpText(clamped);

        if (!_gameOverStarted && current <= HpMin)
        {
            if (!isActiveAndEnabled) return;
            if (_gameOverCR == null) _gameOverCR = StartCoroutine(GameOverSequence());
        }
    }

    private void UpdateHpText(int current)
    {
        if (hpText != null) hpText.text = $"{current}/{HpMax}";
    }

    private void OnScoreChanged(int value)
    {
        if (scoreText != null) scoreText.text = $"x {value}";
    }

    private void OnLettersChanged(int count)
    {
        if (lettersText != null) lettersText.text = $"x {count}";
    }

    private IEnumerator GameOverSequence()
    {
        _gameOverStarted = true;

        var death = GameRefs.PlayerDeath;
        if (death != null) death.Play();

        yield return new WaitForSeconds(death != null ? death.Duration : 0.8f);

        SetGameplayUIVisible(false);

        if (pauseOnGameOver) Time.timeScale = 0f;

        if (gameOverPanel != null) gameOverPanel.SetActive(false);

        if (gameOverLabel != null)
        {
            gameOverLabel.gameObject.SetActive(true);

            if (gameOverLabelGroup != null)
            {
                gameOverLabelGroup.alpha = 0f;
                float t = 0f;
                while (t < labelFadeInDuration)
                {
                    t += Time.unscaledDeltaTime;
                    gameOverLabelGroup.alpha = Mathf.Clamp01(t / labelFadeInDuration);
                    yield return null;
                }
            }

            yield return new WaitForSecondsRealtime(labelHoldSeconds);

            if (gameOverLabelGroup != null)
            {
                float t = 0f;
                while (t < labelFadeOutDuration)
                {
                    t += Time.unscaledDeltaTime;
                    gameOverLabelGroup.alpha = 1f - Mathf.Clamp01(t / labelFadeOutDuration);
                    yield return null;
                }
            }

            gameOverLabel.gameObject.SetActive(false);
        }
        else
        {
            yield return new WaitForSecondsRealtime(0.6f);
        }

        

        if (gameOverPanel != null) gameOverPanel.SetActive(true);

        _gameOverCR = null;
    }

    private void SetGameplayUIVisible(bool show)
    {
        if (hideOnGameOver == null) return;
        foreach (var go in hideOnGameOver)
        {
            if (!go) continue;
            var cg = go.GetComponent<CanvasGroup>() ?? go.AddComponent<CanvasGroup>();
            cg.alpha = show ? 1f : 0f;
            cg.interactable = show;
            cg.blocksRaycasts = show;
        }
    }

    [Header("Power-up Timers")]
    [SerializeField] private TextMeshProUGUI shieldTimerText;
    [SerializeField] private GameObject shieldTimerGroup;
    [SerializeField] private TextMeshProUGUI magnetTimerText;
    [SerializeField] private GameObject magnetTimerGroup;

    private void Update()
    {
        var sh = GameRefs.PlayerShield;
        if (sh != null && sh.IsActive)
        {
            if (shieldTimerGroup) shieldTimerGroup.SetActive(true);
            if (shieldTimerText) shieldTimerText.text = Mathf.CeilToInt(sh.Remaining).ToString();
        }
        else
        {
            if (shieldTimerGroup) shieldTimerGroup.SetActive(false);
        }

        var mg = GameRefs.PlayerMagnet;
        if (mg != null && mg.IsActive)
        {
            if (magnetTimerGroup) magnetTimerGroup.SetActive(true);
            if (magnetTimerText) magnetTimerText.text = Mathf.CeilToInt(mg.Remaining).ToString();
        }
        else
        {
            if (magnetTimerGroup) magnetTimerGroup.SetActive(false);
        }
    }
}
