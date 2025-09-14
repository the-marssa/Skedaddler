using UnityEngine;
using TMPro;

[DisallowMultipleComponent]
public class HUDController : MonoBehaviour
{
    [Header("Text")]
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text hpText;
    [SerializeField] private TMP_Text defermentText;

    [Header("Timers UI (groups)")]
    [SerializeField] private GameObject letterTimerGroup;
    [SerializeField] private TMP_Text letterTimerText;
    [SerializeField] private GameObject magnetTimerGroup;
    [SerializeField] private TMP_Text magnetTimerText;
    [SerializeField] private GameObject shieldTimerGroup;
    [SerializeField] private TMP_Text shieldTimerText;

    private PlayerPowers powers;

    void Awake() { if (!powers) powers = FindFirstObjectByType<PlayerPowers>(); }
    void OnEnable() => RefreshAll();

    void Update()
    {
        var gc = GameCore.Instance;
        if (gc != null)
        {
            var run = gc.CurrentRun;
            if (run != null)
            {
                if (scoreText) scoreText.text = run.score.ToString();
                if (hpText) hpText.text = $"{gc.HP}/{gc.MaxHP}";
            }
            else
            {
                if (scoreText) scoreText.text = "0";
                if (hpText) hpText.text = $"{gc.MaxHP}/{gc.MaxHP}";
            }

            if (defermentText) defermentText.text = gc.Save.lifetime.deferments.ToString();
        }

        if (powers)
        {
            if (letterTimerGroup) letterTimerGroup.SetActive(powers.LetterSlowActive);
            if (magnetTimerGroup) magnetTimerGroup.SetActive(powers.MagnetActive);
            if (shieldTimerGroup) shieldTimerGroup.SetActive(powers.ShieldActive);

            if (letterTimerText && powers.LetterSlowActive)
                letterTimerText.text = Mathf.CeilToInt(powers.LetterRemaining).ToString();
            if (magnetTimerText && powers.MagnetActive)
                magnetTimerText.text = Mathf.CeilToInt(powers.MagnetRemaining).ToString();
            if (shieldTimerText && powers.ShieldActive)
                shieldTimerText.text = Mathf.CeilToInt(powers.ShieldRemaining).ToString();
        }
    }

    public void RefreshAll() => Update();
}
