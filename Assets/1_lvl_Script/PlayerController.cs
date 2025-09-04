using Dreamteck.Forever;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using JSAM;
using MoreMountains.Feedbacks;

[RequireComponent(typeof(Runner))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] private Runner _runner;
    [SerializeField] private float _joystickSensitivity = 2f;
    [SerializeField] private float _maxOffset = 5f;

    [Header("Smoothing")]
    [SerializeField] private float _lateralSpeed = 4f;
    [SerializeField, Range(0.02f, 0.3f)] private float _xSmoothTime = 0.10f;
    [SerializeField] private float _xMaxSpeed = 100f;
    [SerializeField, Range(0f, 0.3f)] private float _inputDeadZone = 0.08f;

    [Header("Animator")]
    [SerializeField] private Animator _anim;
    [SerializeField] private string _jumpTrigger = "Jump";
    [SerializeField] private string _hitTrigger = "Hit";

    [Header("Jump")]
    [SerializeField] private float _jumpHeight = 1.9f;
    [SerializeField] private float _jumpDuration = 0.8f;

    [Header("Hit / Pause forward")]
    [SerializeField] private float _hitLockTime = 1.2f;
    [SerializeField] private Runner _forwardDriver;

    [Header("FEEL")]
    [SerializeField] private MMF_Player jumpStartFx; 
    [SerializeField] private MMF_Player jumpLandFx; 

    private RunController _inputController;
    private float _targetX;
    private float _addValue;
    private float _verticalOffset;
    private bool _isJumping;
    private bool _controlsLocked;
    private bool _inHit;
    private float _xVel;

    private void Awake()
    {
        if (_runner == null) _runner = GetComponent<Runner>();
        if (_anim == null) _anim = GetComponent<Animator>();
        if (_forwardDriver == null) _forwardDriver = GetComponent<Runner>();

        GameRefs.PlayerController = this;

        _inputController = new RunController();
        SubscribeEvents();

        _targetX = _runner != null ? _runner.motion.offset.x : 0f;
    }

    private void OnEnable() => _inputController.Enable();
    private void OnDisable() => _inputController.Disable();
    private void OnDestroy()
    {
        UnsubscribeEvents();
        _inputController.Dispose();
        if (GameRefs.PlayerController == this) GameRefs.PlayerController = null;
    }

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
        float x = ctx.ReadValue<Vector2>().x;
        _addValue = (Mathf.Abs(x) < _inputDeadZone) ? 0f : x * _joystickSensitivity;
    }

    private void OnMoveCanceled(InputAction.CallbackContext ctx)
    {
        _addValue = 0f;
        if (_runner != null) _targetX = _runner.motion.offset.x;
    }

    private void OnJumpPerformed(InputAction.CallbackContext ctx)
    {
       
        jumpStartFx?.PlayFeedbacks();

        if (_anim != null) _anim.SetTrigger(_jumpTrigger);

        if (!_isJumping)
        {
            JSAM.AudioManager.PlaySound(Run_audiolibrarySounds.Jump_sfx, transform);
            StartCoroutine(JumpRoutine());
        }
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
        jumpLandFx?.PlayFeedbacks();
        _isJumping = false;
    }

    private void Update()
    {
        if (_runner == null) return;

        _targetX = Mathf.Clamp(
            _targetX + _addValue * _lateralSpeed * Time.deltaTime,
            -_maxOffset, _maxOffset
        );

        float newX = Mathf.SmoothDamp(
            _runner.motion.offset.x,
            _targetX,
            ref _xVel,
            _xSmoothTime,
            _xMaxSpeed
        );

        _runner.motion.offset = new Vector2(newX, _verticalOffset);
    }

    public void TryHit()
    {
        if (_isJumping || _inHit) return;
        if (_anim != null) _anim.SetTrigger(_hitTrigger);
        StartCoroutine(HitLock());
    }

    private IEnumerator HitLock()
    {
        _inHit = true;
        _controlsLocked = true;
        _addValue = 0f;

        if (_forwardDriver != null) _forwardDriver.enabled = false;

        yield return new WaitForSeconds(_hitLockTime);

        if (_forwardDriver != null) _forwardDriver.enabled = true;

        _controlsLocked = false;
        _inHit = false;
    }

    public void ResetOffset() { _targetX = 0f; }
    public void ApplyConfig(float lateralSpeed, float xSmoothTime, float inputDeadZone, float jumpHeight)
    {
        _lateralSpeed = lateralSpeed;
        _xSmoothTime = xSmoothTime;
        _inputDeadZone = inputDeadZone;
        _jumpHeight = jumpHeight;
    }

}
