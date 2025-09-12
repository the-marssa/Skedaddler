using UnityEngine;
using TMPro;

[DisallowMultipleComponent]
public class StatsPanelController : MonoBehaviour
{
    [Header("Lifetime")]
    [SerializeField] private TMP_Text ltTotalScore;
    [SerializeField] private TMP_Text ltTotalLetters;
    [SerializeField] private TMP_Text ltTotalHearts;
    [SerializeField] private TMP_Text ltDeferments;

    [Header("Last Run")]
    [SerializeField] private TMP_Text lrScore;
    [SerializeField] private TMP_Text lrLetters;
    [SerializeField] private TMP_Text lrCheckpoints;
    [SerializeField] private TMP_Text lrTime;
    [SerializeField] private TMP_Text lrHP;

    void OnEnable() => Refresh();

    public void Refresh()
    {
        var gc = GameCore.Instance;
        if (!gc) return;

        var lt = gc.Save.lifetime;
        if (ltTotalScore) ltTotalScore.text = lt.totalScore.ToString();
        if (ltTotalLetters) ltTotalLetters.text = lt.totalLetters.ToString();
        if (ltTotalHearts) ltTotalHearts.text = lt.totalHearts.ToString();
        if (ltDeferments) ltDeferments.text = lt.deferments.ToString();

        var lr = gc.Save.lastRun;
        if (lr != null)
        {
            if (lrScore) lrScore.text = lr.score.ToString();
            if (lrLetters) lrLetters.text = lr.letters.ToString();
            if (lrCheckpoints) lrCheckpoints.text = lr.checkpoints.ToString();
            if (lrTime) lrTime.text = Mathf.CeilToInt(lr.seconds) + "s";
            if (lrHP) lrHP.text = lr.hpLost.ToString();
        }
        else
        {
            if (lrScore) lrScore.text = "0";
            if (lrLetters) lrLetters.text = "0";
            if (lrCheckpoints) lrCheckpoints.text = "0";
            if (lrTime) lrTime.text = "0s";
            if (lrHP) lrHP.text = "0";
        }
    }
}
