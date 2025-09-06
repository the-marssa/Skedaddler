using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using VContainer;

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
    [SerializeField] private float deathFxDuration = 0.8f; 

    [Header("Power-up Timers")]
    [SerializeField] private TextMeshProUGUI shieldTimerText;
    [SerializeField] private GameObject shieldTimerGroup;
    [SerializeField] private TextMeshProUGUI magnetTimerText;
    [SerializeField] private GameObject magnetTimerGroup;

    private const int HpMin = 0;
    private const int HpMax = 20;

    private IRunSessionProvider _provider;
    private IPlayerPowerups _powerups;

    private int _lastHp = int.MinValue;
    private int _lastStars = int.MinValue;
    private int _lastLetters = int.MinValue;

    private bool _gameOverStarted;
    private Coroutine _gameOverCR;

    [Inject]
    public void Construct(IRunSessionProvider provider, IPlayerPowerups powerups)
    {
        _provider = provider;
        _powerups = powerups;
    }

    private void Start()
    {
        if (hpSlider != null) { hpSlider.minValue = HpMin; hpSlider.maxValue = HpMax; }

        
        if (health != null)
        {
            int startHp = Mathf.Clamp(health.Current, HpMin, HpMax);
            _lastHp = startHp;
            if (hpSlider != null) hpSlider.value = startHp;
            if (hpText != null) hpText.text = $"{startHp}/{HpMax}";
        }

        var s = _provider?.Current;
        if (s != null)
        {
            _lastStars = s.Stars;
            _lastLetters = s.Letters;
            if (scoreText) scoreText.text = $"x {s.Stars}";
            if (lettersText) lettersText.text = $"x {s.Letters}";
        }

        if (gameOverLabel != null) gameOverLabel.gameObject.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (gameOverLabelGroup != null) gameOverLabelGroup.alpha = 0f;
    }

    private void OnDisable()
    {
        if (_gameOverCR != null) { StopCoroutine(_gameOverCR); _gameOverCR = null; }
    }

    private void Update()
    {
        if (health != null)
        {
            int hp = Mathf.Clamp(health.Current, HpMin, HpMax);
            if (hp != _lastHp)
            {
                _lastHp = hp;
                if (hpSlider) hpSlider.value = hp;
                if (hpText) hpText.text = $"{hp}/{HpMax}";

                if (!_gameOverStarted && hp <= HpMin)
                {
                    if (isActiveAndEnabled && _gameOverCR == null)
                        _gameOverCR = StartCoroutine(GameOverSequence());
                }
            }
        }

        var s = _provider?.Current;
        if (s != null)
        {
            if (s.Stars != _lastStars)
            {
                _lastStars = s.Stars;
                if (scoreText) scoreText.text = $"x {s.Stars}";
            }
            if (s.Letters != _lastLetters)
            {
                _lastLetters = s.Letters;
                if (lettersText) lettersText.text = $"x {s.Letters}";
            }
        }

        if (_powerups != null)
        {
            
            if (_powerups.IsShieldActive)
            {
                if (shieldTimerGroup) shieldTimerGroup.SetActive(true);
                if (shieldTimerText) shieldTimerText.text = Mathf.CeilToInt(_powerups.ShieldRemaining).ToString();
            }
            else if (shieldTimerGroup) shieldTimerGroup.SetActive(false);

           
            if (_powerups.IsMagnetActive)
            {
                if (magnetTimerGroup) magnetTimerGroup.SetActive(true);
                if (magnetTimerText) magnetTimerText.text = Mathf.CeilToInt(_powerups.MagnetRemaining).ToString();
            }
            else if (magnetTimerGroup) magnetTimerGroup.SetActive(false);
        }
    }

    private IEnumerator GameOverSequence()
    {
        _gameOverStarted = true;

        yield return new WaitForSeconds(deathFxDuration);

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
}
