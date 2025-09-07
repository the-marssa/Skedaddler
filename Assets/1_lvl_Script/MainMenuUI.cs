using UnityEngine;

public class MainMenuUI : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject homePanel;
    [SerializeField] private GameObject characterPanel;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject statsPanel;
    [SerializeField] private GameResetter resetter;

    [Header("Roots")]
    [SerializeField] private GameObject mainMenuRoot;
    [SerializeField] private GameObject startScreenPanel;

    [Header("Off while Menu open")]
    [SerializeField] private GameObject[] disableWhileMenu;
    [SerializeField] private Behaviour[] disableComponentsMenu;

    private void Awake()
    {
        Show(homePanel);
        ApplyMenuState(true);
    }

    public void OnPlay()
    {
        resetter?.ResetToStartPanel();
        if (startScreenPanel) startScreenPanel.SetActive(true);
        if (mainMenuRoot) mainMenuRoot.SetActive(false);
    }

    public void OpenCharacter() => ShowAndPause(characterPanel);
    public void OpenSettings() => ShowAndPause(settingsPanel);
    public void OpenStats() => ShowAndPause(statsPanel);
    public void BackToHome() => ShowAndPause(homePanel);

    private void ShowAndPause(GameObject target)
    {
        Show(target);
        ApplyMenuState(true);
    }

    private void Show(GameObject target)
    {
        if (homePanel) homePanel.SetActive(false);
        if (characterPanel) characterPanel.SetActive(false);
        if (settingsPanel) settingsPanel.SetActive(false);
        if (statsPanel) statsPanel.SetActive(false);
        if (startScreenPanel) startScreenPanel.SetActive(false);

        if (mainMenuRoot && !mainMenuRoot.activeSelf) mainMenuRoot.SetActive(true);
        if (target == startScreenPanel && mainMenuRoot) mainMenuRoot.SetActive(false);

        if (target) target.SetActive(true);
    }

    private void ApplyMenuState(bool menuVisible)
    {
        Time.timeScale = menuVisible ? 0f : 1f;

        if (disableWhileMenu != null)
            foreach (var go in disableWhileMenu) if (go) go.SetActive(!menuVisible);

        if (disableComponentsMenu != null)
            foreach (var c in disableComponentsMenu) if (c) c.enabled = !menuVisible;
    }
}
