using UnityEngine;

public class ConfigProvider : MonoBehaviour
{
    [SerializeField] private GameConfig config;

    public static GameConfig I { get; private set; }

    private void Awake()
    {
        if (I != null) { Destroy(gameObject); return; }

        if (config == null)
        {
            config = Resources.Load<GameConfig>("Config/GameConfig");
        }

        I = config;
        DontDestroyOnLoad(gameObject);
    }
}
