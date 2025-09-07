using UnityEngine;

public class StartPanelController : MonoBehaviour
{
    [Header("Off till Start")]
    [SerializeField] private GameObject[] disableGOs;        
    [SerializeField] private Behaviour[] disableComponents; 

    [Header("Behaviour")]
    [SerializeField] private bool pauseWithTimeScale = true; 
    private bool prepared;

    void OnEnable()
    {
        PrepareStart();
    }

    public void PrepareStart()
    {
        if (prepared) return;
        prepared = true;

        if (pauseWithTimeScale) Time.timeScale = 0f;

        if (disableGOs != null)
            foreach (var go in disableGOs)
                if (go) go.SetActive(false);

        if (disableComponents != null)
            foreach (var b in disableComponents)
                if (b) b.enabled = false;

        gameObject.SetActive(true);
    }

    public void ApplyRunState()
    {
        if (pauseWithTimeScale) Time.timeScale = 1f;

        if (disableGOs != null)
            foreach (var go in disableGOs)
                if (go) go.SetActive(true);

        if (disableComponents != null)
            foreach (var b in disableComponents)
                if (b) b.enabled = true;

        gameObject.SetActive(false);
        prepared = false;
    }
}
