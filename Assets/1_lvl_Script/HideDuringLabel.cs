using UnityEngine;

public class HideDuringLabel : MonoBehaviour
{
    [SerializeField] private GameObject[] disableSetActive;   
    [SerializeField] private GameObject[] hideCanvasGroup;   

    void OnEnable() { Set(true); }
    void OnDisable() { Set(false); }

    void Set(bool active)
    {
        if (disableSetActive != null)
            foreach (var go in disableSetActive)
                if (go) go.SetActive(!active);

        if (hideCanvasGroup != null)
            foreach (var go in hideCanvasGroup)
                if (go)
                {
                    var cg = go.GetComponent<CanvasGroup>() ?? go.AddComponent<CanvasGroup>();
                    bool visible = !active;
                    cg.alpha = visible ? 1f : 0f;
                    cg.interactable = visible;
                    cg.blocksRaycasts = visible;
                }
    }
}
