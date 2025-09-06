using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
public class PlayerPowerups : MonoBehaviour, IPlayerPowerups
{
    [Header("Optional visuals")]
    [SerializeField] private GameObject shieldVisual;
    [SerializeField] private GameObject magnetAura;

    private float _magnetEnd;
    private float _shieldEnd;
    private Coroutine _magnetCo, _shieldCo;

    public bool IsMagnetActive => Time.time < _magnetEnd;
    public bool IsShieldActive => Time.time < _shieldEnd;
    public float MagnetRemaining => Mathf.Max(0f, _magnetEnd - Time.time);
    public float ShieldRemaining => Mathf.Max(0f, _shieldEnd - Time.time);

    public void ActivateMagnet(float seconds)
    {
        if (seconds <= 0f) return;
        _magnetEnd = Time.time + seconds;
        if (_magnetCo != null) StopCoroutine(_magnetCo);
        _magnetCo = StartCoroutine(CoMagnet());
    }

    public void ActivateShield(float seconds)
    {
        if (seconds <= 0f) return;
        _shieldEnd = Time.time + seconds;
        if (_shieldCo != null) StopCoroutine(_shieldCo);
        _shieldCo = StartCoroutine(CoShield());
    }

    private IEnumerator CoMagnet()
    {
        if (magnetAura) magnetAura.SetActive(true);
        while (IsMagnetActive) yield return null;
        if (magnetAura) magnetAura.SetActive(false);
        _magnetCo = null;
    }

    private IEnumerator CoShield()
    {
        if (shieldVisual) shieldVisual.SetActive(true);
        while (IsShieldActive) yield return null;
        if (shieldVisual) shieldVisual.SetActive(false);
        _shieldCo = null;
    }
}
