using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverController : MonoBehaviour
{
    [Tooltip("Scene name from Build Settings")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    [Header("Hide while Game Over is visible")]
    [SerializeField] private GameObject[] hideWhileOpen; 

    [Header("Behaviour")]
    [SerializeField] private bool pauseOnEnable = true;  

    private void OnEnable()
    {
        if (pauseOnEnable) Time.timeScale = 0f;
        SetHidden(true);
    }

    private void OnDisable()
    {
        SetHidden(false);
        if (pauseOnEnable) Time.timeScale = 1f;
    }

    private void SetHidden(bool hide)
    {
        if (hideWhileOpen == null) return;
        foreach (var go in hideWhileOpen)
            if (go) go.SetActive(!hide);
    }

    // Buttons
    public void TryAgain()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f;
        if (!string.IsNullOrEmpty(mainMenuSceneName))
            SceneManager.LoadScene(mainMenuSceneName);
        else
            SceneManager.LoadScene(0);
    }
}
