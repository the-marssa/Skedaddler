using Dreamteck.Forever;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private Runner _runner;
    [SerializeField] private float _slideSpeed = 3f;
    [SerializeField] private float _joystickSensitivity = 2f;
    [SerializeField] private float _maxOffset = 5f;

    [Header("Animator")]
    [SerializeField] private Animator _anim;
    [SerializeField] private string _jumpTrigger = "Jump";
    [SerializeField] private string _hitTrigger = "Hit";

    [Header("Jump")]
    [SerializeField] private float _jumpHeight = 2.4f;
    [SerializeField] private float _jumpDuration = 0.55f;

    [Header("Hit / Pause forward")]
    [SerializeField] private float _hitLockTime = 1.2f;
    [SerializeField] private Runner _forwardDriver;

    private RunController _inputController;
    private float _targetX;
    private float _addValue;
    private float _verticalOffset;
    private bool _isJumping;
    private bool _controlsLocked;
    private bool _inHit;

    private void Awake()
    {
        if (_runner == null) _runner = GetComponent<Runner>();
        if (_anim == null) _anim = GetComponent<Animator>();
        if (_forwardDriver == null) _forwardDriver = GetComponent<Runner>();

        _inputController = new RunController();
        SubscribeEvents();

        _targetX = _runner ? _runner.motion.offset.x : 0f;
    }

    private void OnEnable() => _inputController.Enable();
    private void OnDisable() => _inputController.Disable();
    private void OnDestroy() { UnsubscribeEvents(); _inputController.Dispose(); }

    private void SubscribeEvents()
    {
        _inputController.Default.Move.performed += OnMovePerformed;
        _inputController.Default.Move.canceled += OnMoveCanceled;
        _inputController.Default.Jump.performed += OnJumpPerformed;
    }

    private void UnsubscribeEvents()
    {
        _inputController.Default.Move.performed -= OnMovePerformed;
        _inputController.Default.Move.canceled -= OnMoveCanceled;
        _inputController.Default.Jump.performed -= OnJumpPerformed;
    }

    private void OnMovePerformed(InputAction.CallbackContext ctx)
    {
        if (_controlsLocked) return;
        _addValue = ctx.ReadValue<Vector2>().x / _joystickSensitivity;
    }

    private void OnMoveCanceled(InputAction.CallbackContext ctx)
    {
        _addValue = 0f;
        if (_runner) _targetX = _runner.motion.offset.x;
    }

    private void OnJumpPerformed(InputAction.CallbackContext ctx)
    {
        if (_anim) _anim.SetTrigger(_jumpTrigger);
        if (!_isJumping) StartCoroutine(JumpRoutine());
    }

    private IEnumerator JumpRoutine()
    {
        _isJumping = true;
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / Mathf.Max(0.01f, _jumpDuration);
            _verticalOffset = Mathf.Sin(t * Mathf.PI) * _jumpHeight;
            yield return null;
        }
        _verticalOffset = 0f;
        _isJumping = false;
    }

    private void Update()
    {
        _targetX = Mathf.Clamp(_targetX + _addValue, -_maxOffset, _maxOffset);
        float newX = Mathf.MoveTowards(_runner.motion.offset.x, _targetX, _slideSpeed * Time.deltaTime);
        _runner.motion.offset = new Vector2(newX, _verticalOffset);
    }

    private void OnCollisionEnter(Collision c)
    {
        if (c.collider.CompareTag("Obstacle")) TryHit();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Obstacle")) TryHit();
    }

    private void TryHit()
    {
        if (_isJumping || _inHit) return;
        if (_anim) _anim.SetTrigger(_hitTrigger);
        StartCoroutine(HitLock());
    }

    private IEnumerator HitLock()
    {
        _inHit = true;
        _controlsLocked = true;
        _addValue = 0f;

        if (_forwardDriver) _forwardDriver.enabled = false;

        yield return new WaitForSeconds(_hitLockTime);

        if (_forwardDriver) _forwardDriver.enabled = true;

        _controlsLocked = false;
        _inHit = false;
    }

    public void ResetOffset() { _targetX = 0f; }
}