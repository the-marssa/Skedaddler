using UnityEngine;
using UnityEngine.InputSystem;
using Dreamteck.Forever;

[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody))]
public sealed class PlayerController : MonoBehaviour
{
    [Header("Forever")]
    [SerializeField] private Runner runner;

    [Header("Movement (strafe X)")]
    [SerializeField] private float maxOffset = 2.2f;
    [SerializeField] private float xSmoothTime = 0.08f;
    [SerializeField] private float xMaxSpeed = 12f;

    [Header("Input System (New)")]
    [Tooltip("Use 1D Axis action (A/D, Left/Right, keys) if you have it")]
    [SerializeField] private InputActionReference move1D;   
    [Tooltip("Use 2D action (stick/dpad). X is used for strafing")]
    [SerializeField] private InputActionReference move2D;   
    [SerializeField] private InputActionReference jumpAction; 

    [Header("On-screen joystick (optional)")]
    [SerializeField] private Joystick joystick;             
    [SerializeField] private float joystickScale = 1.0f;    

    [Header("Jump (parabola over offset.y)")]
    [SerializeField] private float jumpHeight = 1.2f;
    [SerializeField] private float jumpDuration = 0.7f;
    [SerializeField] private AnimationCurve jumpEase = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Hit-lock")]
    [SerializeField] private float hitLockTime = 1.2f;

    [Header("Visuals")]
    [SerializeField] private Animator animator;

    public bool RunActive { get; private set; } = true;

    Rigidbody _rb;
    float _targetX, _xVel;
    bool _isJumping;
    float _jumpT;
    bool _controlsLocked;
    float _hitLockTimer;

    void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        if (!runner) runner = GetComponent<Runner>();
    }

    void OnEnable()
    {
        EnableAction(move1D);
        EnableAction(move2D);
        EnableAction(jumpAction);
        ResetStateHard();
    }

    void OnDisable()
    {
        DisableAction(move1D); 
        DisableAction(move2D);
        DisableAction(jumpAction);
    }

    void Update()
    {
        if (!runner || !RunActive) return;

        if (_controlsLocked && _hitLockTimer > 0f)
        {
            _hitLockTimer -= Time.deltaTime;
            if (_hitLockTimer <= 0f) _controlsLocked = false;
        }

        float inputX = 0f;

        if (move1D && move1D.action != null)
            inputX += move1D.action.ReadValue<float>();           

        if (move2D && move2D.action != null)
            inputX += move2D.action.ReadValue<Vector2>().x;      

        if (joystick) inputX += joystick.Horizontal * joystickScale;

        
        if (!_controlsLocked)
            _targetX = Mathf.Clamp(_targetX + inputX * Time.deltaTime, -maxOffset, maxOffset);

        
        var motion = runner.motion;
        float curX = motion.offset.x;
        float y = motion.offset.y;

        float newX = Mathf.SmoothDamp(curX, _targetX, ref _xVel, xSmoothTime, xMaxSpeed);

        if (_isJumping)
        {
            _jumpT += Time.deltaTime / Mathf.Max(0.01f, jumpDuration);
            float t = Mathf.Clamp01(_jumpT);
            float e = jumpEase.Evaluate(t);
            y = 4f * jumpHeight * e * (1f - e);
            if (t >= 1f) { _isJumping = false; _jumpT = 0f; }
        }
        else y = 0f;

        motion.offset = new Vector2(newX, y);

        if (!_controlsLocked && !_isJumping && WasJumpPressedThisFrame())
            Jump();

        if (animator)
        {
            animator.SetFloat("SpeedX", Mathf.Abs(_xVel));
            animator.SetBool("Jump", _isJumping);
            animator.SetBool("Locked", _controlsLocked);
        }
    }

    bool WasJumpPressedThisFrame()
    {
        return jumpAction && jumpAction.action != null && jumpAction.action.WasPressedThisFrame();
    }

    public void SetRunActive(bool active) => RunActive = active;

    public void Jump()
    {
        if (_isJumping || _controlsLocked || !RunActive) return;
        _isJumping = true;
        _jumpT = 0f;
    }

    public void LockControlsOnHit()
    {
        _controlsLocked = true;
        _hitLockTimer = hitLockTime;
        if (animator) animator.SetTrigger("Hit");
    }

    public void ResetStateSoft()
    {
        _controlsLocked = false;
        _hitLockTimer = 0f;
        _isJumping = false;
        _jumpT = 0f;
    }

    public void ResetStateHard()
    {
        ResetStateSoft();
        _targetX = 0f;
        _xVel = 0f;
        if (runner)
        {
            var m = runner.motion;
            m.offset = Vector2.zero;
        }
    }

    static void EnableAction(InputActionReference r)
    {
        if (r != null && r.action != null && !r.action.enabled) r.action.Enable();
    }
    static void DisableAction(InputActionReference r)
    {
        if (r != null && r.action != null && r.action.enabled) r.action.Disable();
    }
}
