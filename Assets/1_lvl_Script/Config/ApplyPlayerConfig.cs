using UnityEngine;

[DisallowMultipleComponent]
public class ApplyPlayerConfig : MonoBehaviour
{
    [SerializeField] private PlayerController player;

    private void Reset()
    {
        if (player) return;
#if UNITY_2023_1_OR_NEWER
        player = FindFirstObjectByType<PlayerController>();
#else
        player = FindObjectOfType<PlayerController>();
#endif
    }

    private void Awake()
    {
        var cfg = ConfigProvider.I;
        if (!cfg || !player) return;

        player.ApplyConfig(
            cfg.runner.lateralSpeed,
            cfg.runner.xSmoothTime,
            cfg.runner.inputDeadZone,
            cfg.runner.jumpHeight
        );
    }
}
