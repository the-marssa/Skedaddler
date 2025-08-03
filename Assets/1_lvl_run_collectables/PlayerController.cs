using UnityEngine;
using Dreamteck.Forever;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private Runner _runner;
    [SerializeField] private float _slideSpeed = 3f;
    [SerializeField] private float _joystickSensitivity = 2f;
    [SerializeField] private float _maxOffset = 5f;

    private Run_controller _inputController;
    private Vector2 _targetVector;
    private float _addValue;

    private void Awake()
    {
        if (_runner == null) _runner = GetComponent<Runner>();

        _inputController = new Run_controller();
        SubscribeEvents();
    }

    private void OnEnable()
    {
        _inputController.Enable();
    }

    private void OnDisable()
    {
        _inputController.Disable();
    }

    private void OnDestroy()
    {
        UnsubscribeEvents();
        _inputController.Dispose();
    }

    private void SubscribeEvents()
    {
        _inputController.Default.Move.performed += OnMovePerformed;
        _inputController.Default.Move.canceled += OnMoveCanceled;
    }

    private void UnsubscribeEvents()
    {
        _inputController.Default.Move.performed -= OnMovePerformed;
        _inputController.Default.Move.canceled -= OnMoveCanceled;
    }

    private void OnMovePerformed(InputAction.CallbackContext context)
    {
        OnMovementReceived(context.ReadValue<Vector2>());
    }

    private void OnMoveCanceled(InputAction.CallbackContext context)
    {
        OnMovementEnded();
    }

    private void OnMovementReceived(Vector2 movement)
    {
        _addValue = movement.x / _joystickSensitivity;
    }

    private void OnMovementEnded()
    {
        _targetVector = _runner.motion.offset;
        _addValue = 0;
    }

    private void Update()
    {
        _targetVector = new Vector2(
            Mathf.Clamp(_targetVector.x + _addValue, -_maxOffset, _maxOffset),
            0f
        );

        Vector2 finalOffset = Vector2.MoveTowards(
            _runner.motion.offset,
            _targetVector,
            _slideSpeed * Time.deltaTime
        );

        _runner.motion.offset = finalOffset;
    }

    public void ResetOffset()
    {
        _targetVector = Vector2.zero;
    }
}
