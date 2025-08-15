using UnityEngine;

public class StartPanelController : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject startPanel;

    [Header("Disable these GameObjects until Start")]
    [SerializeField] private GameObject[] disableUntilStart;

    [Header("Hide these GameObjects until Start (optional)")]
    [SerializeField] private GameObject[] hideUntilStart;

    [Header("References")]
    [SerializeField] private GameObject player;         
    [SerializeField] private PlayerHealth playerHealth; 
    [Header("Options")]
    [SerializeField] private bool pauseWithTimeScale = true;

    private void Awake()
    {
        if (startPanel) startPanel.SetActive(true);
        foreach (var go in disableUntilStart) if (go) go.SetActive(false);
        foreach (var go in hideUntilStart) if (go) go.SetActive(false);
        if (pauseWithTimeScale) Time.timeScale = 0f;
    }

    public void OnStart()
    {
        if (ScoreManager.Instance) ScoreManager.Instance.ResetScore();
        if (playerHealth) playerHealth.ResetHP();  

        foreach (var go in disableUntilStart) if (go) go.SetActive(true);
        foreach (var go in hideUntilStart) if (go) go.SetActive(true);

        if (pauseWithTimeScale) Time.timeScale = 1f;
        if (startPanel) startPanel.SetActive(false);
      
        MailManager.Instance?.ResetCount();
    }

    private void OnDestroy()
    {
        if (pauseWithTimeScale) Time.timeScale = 1f;
    }
}
