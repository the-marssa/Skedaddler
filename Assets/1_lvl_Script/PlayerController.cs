using Dreamteck.Forever;
using UnityEngine;
using UnityEngine.InputSystem;
using MoreMountains.Feedbacks;

[RequireComponent(typeof(Runner))]
public class PlayerController : MonoBehaviour
{
    [Header("Forever")]
    [SerializeField] private Runner runner;

    [Header("Input")]
    public float joystickSensitivity = 2f;
    [Range(0f, 0.3f)] public float inputDeadZone = 0.08f;

    [Header("Lateral Move")]
    [SerializeField] private float maxOffset = 5f;
    public float xSmoothTime = 0.10f;
    [SerializeField] private float xMaxSpeed = 100f;

    [Header("Jump")]
    [SerializeField] private Animator anim;
    [SerializeField] private string jumpTrigger = "Jump";
    public float jumpHeight = 1.9f;
    [SerializeField] private float jumpDuration = 0.8f;
    [SerializeField] private MMF_Player jumpStartFx;
    [SerializeField] private MMF_Player jumpLandFx;

    [Header("Hit lock")]
    [SerializeField] private string hitTrigger = "Hit";
    [SerializeField] private float hitLockTime = 1.2f;

    private RunController input;
    private float addValue;
    private float targetX;
    private float xVel;
    private bool controlsLocked;
    private bool isJumping;
    private float jumpT;

    public bool ControlsLocked => controlsLocked;

    private void Awake()
    {
        if (!runner) runner = GetComponent<Runner>();
        if (!anim) anim = GetComponentInChildren<Animator>();

        input = new RunController();
        input.Default.Move.performed += OnMove;
        input.Default.Move.canceled += OnMove;
        input.Default.Jump.performed += OnJump;

        targetX = runner ? runner.motion.offset.x : 0f;
    }

    private void OnEnable() => input.Enable();
    private void OnDisable() => input.Disable();

    private void OnDestroy()
    {
        input.Default.Move.performed -= OnMove;
        input.Default.Move.canceled -= OnMove;
        input.Default.Jump.performed -= OnJump;
        input.Dispose();
    }

    private void Update()
    {
        if (!runner) return;

        if (!controlsLocked)
            targetX = Mathf.Clamp(targetX + addValue * Time.deltaTime, -maxOffset, maxOffset);

        var motion = runner.motion;
        float curX = motion.offset.x;
        float y = motion.offset.y;

        float newX = Mathf.SmoothDamp(curX, targetX, ref xVel, xSmoothTime, xMaxSpeed);

        if (isJumping)
        {
            jumpT += Time.deltaTime / Mathf.Max(0.01f, jumpDuration);
            float t = Mathf.Clamp01(jumpT);
            y = 4f * jumpHeight * t * (1f - t);
            if (t >= 1f)
            {
                isJumping = false;
                jumpLandFx?.PlayFeedbacks();
            }
        }

        runner.motion.offset = new Vector2(newX, y);
    }

    private void OnMove(InputAction.CallbackContext ctx)
    {
        var x = ctx.ReadValue<Vector2>().x;
        addValue = Mathf.Abs(x) < inputDeadZone ? 0f : x * joystickSensitivity;
    }

    private void OnJump(InputAction.CallbackContext _)
    {
        if (controlsLocked || isJumping) return;
        isJumping = true;
        jumpT = 0f;
        anim?.SetTrigger(jumpTrigger);
        jumpStartFx?.PlayFeedbacks();
    }

    public void HitPause()
    {
        if (controlsLocked) return;
        controlsLocked = true;
        anim?.SetTrigger(hitTrigger);
        Invoke(nameof(UnlockControls), hitLockTime);
    }

    public void LockControls() => controlsLocked = true;
    public void UnlockControls() => controlsLocked = false;

    public void ResetForRun(float startOffsetX = 0f)
    {
        addValue = 0f;
        xVel = 0f;
        controlsLocked = false;

        isJumping = false;
        jumpT = 0f;

        targetX = Mathf.Clamp(startOffsetX, -maxOffset, maxOffset);

        
        if (anim)
        {
            anim.ResetTrigger(jumpTrigger);
            anim.ResetTrigger(hitTrigger);
            anim.applyRootMotion = false; 
            anim.Rebind();
            anim.Update(0f);
        }

        if (runner)
        {
            runner.motion.offset = new Vector2(targetX, 0f);
        }
    }
}
