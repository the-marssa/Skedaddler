using UnityEngine;
using UnityEngine.SceneManagement;

public class StartMenu : MonoBehaviour
{
    [SerializeField] string gameSceneName = "Project_scene"; 
    public void OnStartClicked()
    {
        SceneManager.LoadScene(gameSceneName);
    }
}
