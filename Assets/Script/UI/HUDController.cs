using TMPro;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class HUDController : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private PlayerPowers powers;        
    [SerializeField] private GameCore game;              

    [Header("Timer Groups (show/hide)")]
    [SerializeField] private GameObject magnetTimerGroup;
    [SerializeField] private GameObject shieldTimerGroup;
    [SerializeField] private GameObject letterTimerGroup;

    [Header("Timer Texts")]
    [SerializeField] private TMP_Text magnetTimerText;
    [SerializeField] private TMP_Text shieldTimerText;
    [SerializeField] private TMP_Text letterTimerText;

    [Header("Score / HP / Deferment")]
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text hpText;
    [SerializeField] private TMP_Text defermentText;

    [Header("UI refresh")]
    [SerializeField, Min(0.02f)] private float statsRefreshInterval = 0.20f;

    private float _statsTimer;

    private void Awake()
    {
        if (!powers) powers = GetComponentInParent<PlayerPowers>();
        if (!game) game = GameCore.Instance;

        SetGroup(magnetTimerGroup, false);
        SetGroup(shieldTimerGroup, false);
        SetGroup(letterTimerGroup, false);

        SetText(magnetTimerText, "");
        SetText(shieldTimerText, "");
        SetText(letterTimerText, "");

        SetText(scoreText, "0");
        SetText(hpText, "");
        SetText(defermentText, "");
    }

    private void OnEnable()
    {
        if (powers)
        {
            powers.OnMagnetTime += OnMagnetTick;
            powers.OnShieldTime += OnShieldTick;
            powers.OnLetterTime += OnLetterTick;
        }

        OnMagnetTick(0f);
        OnShieldTick(0f);
        OnLetterTick(0f);
        ForceStatsRefresh();
    }

    private void OnDisable()
    {
        if (powers)
        {
            powers.OnMagnetTime -= OnMagnetTick;   
            powers.OnShieldTime -= OnShieldTick;
            powers.OnLetterTime -= OnLetterTick;
        }
    }

    private void Update()
    {
        _statsTimer += Time.unscaledDeltaTime;
        if (_statsTimer >= statsRefreshInterval)
        {
            _statsTimer = 0f;
            RefreshStats();
        }
    }

    

    private void OnMagnetTick(float secondsLeft)
    {
        bool on = secondsLeft > 0.01f;
        SetGroup(magnetTimerGroup, on);
        if (on) SetText(magnetTimerText, Mathf.CeilToInt(secondsLeft).ToString());
        else SetText(magnetTimerText, "");
    }

    private void OnShieldTick(float secondsLeft)
    {
        bool on = secondsLeft > 0.01f;
        SetGroup(shieldTimerGroup, on);
        if (on) SetText(shieldTimerText, Mathf.CeilToInt(secondsLeft).ToString());
        else SetText(shieldTimerText, "");
    }

    private void OnLetterTick(float secondsLeft)
    {
        bool on = secondsLeft > 0.01f;
        SetGroup(letterTimerGroup, on);
        if (on) SetText(letterTimerText, Mathf.CeilToInt(secondsLeft).ToString());
        else SetText(letterTimerText, "");
    }

   

    private void RefreshStats()
    {
        var g = game ? game : GameCore.Instance;
        if (g != null && g.CurrentRun != null)
        {
            if (scoreText) scoreText.text = g.CurrentRun.score.ToString();
            if (hpText) hpText.text = g.HP.ToString();

            if (defermentText)
            {
                long total = g.Save.lifetime.deferments;  
                int earnedNow = g.CurrentRun.defermentsEarned;
                defermentText.text = earnedNow > 0 ? $"{total} (+{earnedNow})" : total.ToString();
            }
        }
        else
        {
            if (scoreText) scoreText.text = "0";
            if (hpText) hpText.text = "";
            if (defermentText) defermentText.text = (g != null ? g.Save.lifetime.deferments : 0L).ToString();
        }
    }

    private void ForceStatsRefresh()
    {
        _statsTimer = statsRefreshInterval;
        RefreshStats();
    }

    

    private static void SetGroup(GameObject go, bool state)
    {
        if (go && go.activeSelf != state) go.SetActive(state);
    }

    private static void SetText(TMP_Text t, string s)
    {
        if (t && t.text != s) t.text = s;
    }
}
