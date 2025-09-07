using System.Collections;
using UnityEngine;
using Dreamteck.Forever;

public sealed class GameResetter : MonoBehaviour
{
    [Header("Forever")]
    [SerializeField] private LevelGenerator generator;
    [SerializeField] private Runner runner; 

    [Header("UI")]
    [SerializeField] private StartPanelController startPanel; 
    [SerializeField] private GameObject hudRoot;              
    [SerializeField] private GameObject pausePanel;           
    [SerializeField] private GameObject gameOverPanel;        
    [SerializeField] private GameObject pauseButton;          
    [SerializeField] private GameObject mainMenuRoot;        

    [Header("Session (optional)")]
    [SerializeField] private RunSessionProvider provider;

    
    public void RestartRun() => StartCoroutine(RestartRunRoutine());

    private IEnumerator RestartRunRoutine()
    {
        
        AudioListener.pause = false;
        Time.timeScale = 1f;

        if (mainMenuRoot) mainMenuRoot.SetActive(false);
        if (gameOverPanel) gameOverPanel.SetActive(false);
        if (pausePanel) pausePanel.SetActive(false);

        if (hudRoot) hudRoot.SetActive(true);
        if (pauseButton) pauseButton.SetActive(true);

        if (startPanel) startPanel.ApplyRunState();

        provider?.StartNew();

        generator.Restart();                
        yield return null;                   
        while (!generator.ready) yield return null; 

        runner.StartFollow();
    }

    public void ResetToStartPanel()
    {
        AudioListener.pause = false;
        Time.timeScale = 1f;

        if (gameOverPanel) gameOverPanel.SetActive(false);
        if (pausePanel) pausePanel.SetActive(false);
        if (hudRoot) hudRoot.SetActive(false);
        if (pauseButton) pauseButton.SetActive(false);
        if (mainMenuRoot) mainMenuRoot.SetActive(false);

        generator.Clear();

        if (startPanel)
        {
            startPanel.gameObject.SetActive(true);
            startPanel.PrepareStart();
        }
    }

    public void ReturnToMenu()
    {
        AudioListener.pause = false;
        Time.timeScale = 1f;

        if (gameOverPanel) gameOverPanel.SetActive(false);
        if (pausePanel) pausePanel.SetActive(false);
        if (hudRoot) hudRoot.SetActive(false);
        if (pauseButton) pauseButton.SetActive(false);

        generator.Clear();

        if (startPanel) startPanel.gameObject.SetActive(false);
        if (mainMenuRoot) mainMenuRoot.SetActive(true);
    }
}
