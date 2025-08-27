using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.InputSystem.OnScreen;

public class SimpleOnScreenStick : OnScreenControl, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    [Header("UI")]
    [SerializeField] private RectTransform stickBase;
    [SerializeField] private RectTransform stickKnob;

    [Header("Settings")]
    [SerializeField] private float movementRange = 80f;

    [Tooltip("Input path the stick will drive (e.g. <Gamepad>/leftStick)")]
    [SerializeField] private string m_ControlPath = "<Gamepad>/leftStick";

    private Vector2 _startPos;

    protected override string controlPathInternal
    {
        get => controlPath;
        set => controlPath = value;
    }

    private void Awake()
    {
        if (stickKnob == null) stickKnob = transform.Find("Knob") as RectTransform;
        if (stickBase == null) stickBase = transform.Find("Base") as RectTransform;
        _startPos = stickKnob.anchoredPosition;
    }

    public void OnPointerDown(PointerEventData eventData) => UpdateStick(eventData);
    public void OnDrag(PointerEventData eventData) => UpdateStick(eventData);

    public void OnPointerUp(PointerEventData eventData)
    {
        stickKnob.anchoredPosition = _startPos;
        SendValueToControl(Vector2.zero);
    }

    private void UpdateStick(PointerEventData data)
    {
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            stickBase, data.position, data.pressEventCamera, out localPoint);

        var clamped = Vector2.ClampMagnitude(localPoint, movementRange);
        stickKnob.anchoredPosition = _startPos + clamped;

      
        var value = clamped / movementRange;
        SendValueToControl(value);
    }
}
