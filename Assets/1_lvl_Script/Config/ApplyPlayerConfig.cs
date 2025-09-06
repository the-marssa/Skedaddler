using UnityEngine;
using VContainer;


[DisallowMultipleComponent]
public class ApplyPlayerConfig : MonoBehaviour
{
    [SerializeField] private PlayerController _player;
    private PlayerMovementConfig _movement;


    [Inject] public void Construct(PlayerMovementConfig movement) => _movement = movement;


    private void Reset()
    {
#if UNITY_2023_1_OR_NEWER
        if (!_player) _player = FindFirstObjectByType<PlayerController>();
#else
if (!_player) _player = FindObjectOfType<PlayerController>();
#endif
    }


    private void Awake()
    {
        if (!_player || !_movement) return;
        _player.ApplyConfig(
        _movement.lateralSpeed,
        _movement.xSmoothTime,
        _movement.inputDeadZone,
        _movement.jumpHeight
        );
    }
}