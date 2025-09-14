using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public sealed class SkinThumbnailItem: MonoBehaviour
{
    [SerializeField] private Image artwork;                         
    private FullscreenSkinViewer viewer;                        
    private SkinData data;

    private void Awake()
    {
        GetComponent<Button>().onClick.AddListener(() =>
        {
            if (viewer && data) viewer.Show(data);
        });
    }

    
    public void Bind(SkinData d, FullscreenSkinViewer v)
    {
        data = d;
        viewer = v;

        if (artwork && data)
        {
            artwork.sprite = data.preview;
            artwork.preserveAspect = data.preserveAspect;
        }
    }
}
