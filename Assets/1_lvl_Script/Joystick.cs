using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.OnScreen;

[RequireComponent(typeof(RectTransform))]
[DisallowMultipleComponent]
public class SimpleOnScreenStick : OnScreenControl, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    [Header("UI")]
    [SerializeField] private RectTransform stickBase;
    [SerializeField] private RectTransform stickKnob;

    [Header("Settings")]
    [SerializeField, Min(1f)] private float movementRange = 80f;

    [Tooltip("Input path the stick will drive (e.g. <Gamepad>/leftStick)")]
    [SerializeField] private string m_ControlPath = "<Gamepad>/leftStick";

    [Header("Safety")]
    [SerializeField] private bool autoDisableRaycastsWhenNotRunning = true;

    private Vector2 _startPos;
    private CanvasGroup _cg;

    
    protected override string controlPathInternal
    {
        get => m_ControlPath;
        set => m_ControlPath = value;
    }

    private void Awake()
    {
        if (stickKnob == null) stickKnob = transform.Find("Knob") as RectTransform;
        if (stickBase == null) stickBase = transform.Find("Base") as RectTransform;
        if (stickKnob) _startPos = stickKnob.anchoredPosition;

        if (autoDisableRaycastsWhenNotRunning)
        {
            _cg = GetComponent<CanvasGroup>();
            if (_cg == null) _cg = gameObject.AddComponent<CanvasGroup>();
        }
    }

    
    private void LateUpdate()
    {
        if (!_cg || !autoDisableRaycastsWhenNotRunning) return;
        bool running = GameCore.Instance != null && GameCore.Instance.CurrentRun != null;
        _cg.blocksRaycasts = running;
        _cg.interactable = running;
    }

    public void OnPointerDown(PointerEventData eventData) => UpdateStick(eventData);
    public void OnDrag(PointerEventData eventData) => UpdateStick(eventData);

    public void OnPointerUp(PointerEventData eventData)
    {
        if (stickKnob) stickKnob.anchoredPosition = _startPos;
        SendValueToControl(Vector2.zero);
    }

    private void UpdateStick(PointerEventData data)
    {
        if (!stickBase || !stickKnob) return;

        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            stickBase, data.position, data.pressEventCamera, out localPoint);

        var clamped = Vector2.ClampMagnitude(localPoint, movementRange);
        stickKnob.anchoredPosition = _startPos + clamped;
        SendValueToControl(clamped / movementRange);
    }
}
