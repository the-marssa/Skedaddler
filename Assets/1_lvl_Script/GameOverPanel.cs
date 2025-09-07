using UnityEngine;

public sealed class GameOverPanel : MonoBehaviour
{
    [SerializeField] private GameObject panel;
    [SerializeField] private GameResetter resetter;

    private void Awake()
    {
        if (panel) panel.SetActive(false);
    }

    public void Show(bool show)
    {
        if (panel) panel.SetActive(show);
        Time.timeScale = show ? 0f : 1f;   
        AudioListener.pause = show;
    }


    public void OnRestart() => resetter?.RestartRun();


    public void OnMenu() => resetter?.ReturnToMenu();
}
