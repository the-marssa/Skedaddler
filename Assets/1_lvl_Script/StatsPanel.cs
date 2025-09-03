using TMPro;
using UnityEngine;

public class StatsPanelController : MonoBehaviour
{
    [Header("Lifetime totals (from save)")]
    [SerializeField] private TMP_Text totalStarsText;   
    [SerializeField] private TMP_Text totalLettersText; 

    [Header("Last run (runtime cache)")]
    [SerializeField] private TMP_Text lastScoreText;    
    [SerializeField] private TMP_Text lastStarsText;    
    [SerializeField] private TMP_Text lastLettersText;  
    [SerializeField] private TMP_Text lastDistanceText; 
    [SerializeField] private TMP_Text lastCpsText;      

    private void OnEnable()
    {
        
        var save = (StatsManager.Instance != null) ? StatsManager.Instance.Save : SaveSystem.LoadOrCreate();
        if (totalStarsText) totalStarsText.text = $"Всього: {save.lifetime.totalStars}";
        if (totalLettersText) totalLettersText.text = $"Всього: {save.lifetime.totalLetters}";

        
        RunSnapshot last = null;

        if (StatsManager.Instance != null)
        {
           
            if (StatsManager.Instance.CurrentRun != null)
                last = StatsManager.Instance.CurrentRun;
            
            else
                last = StatsManager.Instance.LastRunCached;
        }

        if (last != null)
        {
            if (lastScoreText) lastScoreText.text = $"Очки: {last.score}";
            if (lastStarsText) lastStarsText.text = $"Зірки: {last.stars}";
            if (lastLettersText) lastLettersText.text = $"Листи: {last.letters}";
            if (lastDistanceText) lastDistanceText.text = $"Дистанція: {Mathf.FloorToInt(last.distance)} м";
            if (lastCpsText) lastCpsText.text = $"Чекпоінти: {last.checkpointsReached}";
        }
        else
        {
           
            if (lastScoreText) lastScoreText.text = "Очки: —";
            if (lastStarsText) lastStarsText.text = "Зірки: —";
            if (lastLettersText) lastLettersText.text = "Листи: —";
            if (lastDistanceText) lastDistanceText.text = "Дистанція: —";
            if (lastCpsText) lastCpsText.text = "Чекпоінти: —";
        }
    }
}
