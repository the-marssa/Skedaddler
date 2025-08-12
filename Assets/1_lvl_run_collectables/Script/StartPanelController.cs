using UnityEngine;

public class StartPanelController : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] GameObject startPanel;        

    [Header("Disable these components until Start")]
    [SerializeField] MonoBehaviour[] disableUntilStart; 

    [Header("Hide these GameObjects until Start (optional)")]
    [SerializeField] GameObject[] hideUntilStart;       

    [Header("Options")]
    [SerializeField] bool pauseWithTimeScale = true;   

    void Awake()
    {
      
        if (startPanel != null) startPanel.SetActive(true);
        foreach (var c in disableUntilStart) if (c) c.enabled = false;
        foreach (var go in hideUntilStart) if (go) go.SetActive(false);
        if (pauseWithTimeScale) Time.timeScale = 0f;
    }

    public void OnStart()
    {
        
        if (ScoreManager.Instance != null) ScoreManager.Instance.ResetScore();
        var player = GameObject.FindGameObjectWithTag("Player");
        var hp = player ? player.GetComponent<PlayerHealth>() : null;
        if (hp != null) hp.ResetHP(); 

       
        foreach (var c in disableUntilStart) if (c) c.enabled = true;
        foreach (var go in hideUntilStart) if (go) go.SetActive(true);

        if (pauseWithTimeScale) Time.timeScale = 1f;
        if (startPanel != null) startPanel.SetActive(false);
    }

    void OnDestroy()
    {
    
        if (pauseWithTimeScale) Time.timeScale = 1f;
    }
}
