using UnityEngine;

[DisallowMultipleComponent]
public class ApplyPlayerConfig : MonoBehaviour
{
    [SerializeField] private PlayerController _player;              
    [SerializeField] private PlayerMovementConfig _movement;       

    private void Reset()
    {
        if (!_player)
            _player = GetComponentInParent<PlayerController>() ?? FindPlayerInScene();
    }

    private void Awake()
    {
        if (!_player)
            _player = FindPlayerInScene();

        Apply();
    }

    private void Apply()
    {
        if (_player == null || _movement == null) return;

        _player.joystickSensitivity = _movement.lateralSpeed;
        _player.xSmoothTime = Mathf.Max(0.01f, _movement.xSmoothTime);
        _player.inputDeadZone = Mathf.Clamp01(_movement.inputDeadZone);
        _player.jumpHeight = Mathf.Max(0f, _movement.jumpHeight);
    }

    
    private static PlayerController FindPlayerInScene()
    {
#if UNITY_2023_1_OR_NEWER
        return Object.FindFirstObjectByType<PlayerController>(FindObjectsInactive.Include);
#else
        
        var all = Resources.FindObjectsOfTypeAll<PlayerController>();
        foreach (var c in all)
        {
            if (c && c.gameObject.scene.IsValid()) return c;
        }
        return null;
#endif
    }
}
