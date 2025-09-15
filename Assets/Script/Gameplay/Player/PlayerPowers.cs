using System;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class PlayerPowers : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private PlayerController player; 

    [Header("Durations (sec)")]
    [SerializeField] private float magnetSeconds = 7f;
    [SerializeField] private float shieldSeconds = 5f;
    [SerializeField] private float letterSlowSeconds = 6f;

  
    private float _magnetT, _shieldT, _letterT;

  
    public event Action<float> OnMagnetTime;
    public event Action<float> OnShieldTime;
    public event Action<float> OnLetterTime;

    public bool MagnetActive => _magnetT > 0f;
    public bool ShieldActive => _shieldT > 0f;
    public bool LetterSlowActive => _letterT > 0f;

    private void Awake()
    {
        if (!player) player = GetComponentInParent<PlayerController>();
    }

    private void OnEnable()
    {
        ResetAll();
    }

    private void OnDisable()
    {
        /* nothing to unsubscribe — no subscriptions exist */
    }

    private void Update()
    {
        Tick(ref _magnetT, OnMagnetTime);
        Tick(ref _shieldT, OnShieldTime);
        Tick(ref _letterT, OnLetterTime);
    }

    private static void Tick(ref float t, Action<float> evt)
    {
        if (t <= 0f) return;
        t = Mathf.Max(0f, t - Time.unscaledDeltaTime);
        evt?.Invoke(t);
    }

    public void ResetAll()
    {
        _magnetT = _shieldT = _letterT = 0f;
        OnMagnetTime?.Invoke(0f);
        OnShieldTime?.Invoke(0f);
        OnLetterTime?.Invoke(0f);
    }

    
    public void ActivateMagnet() => _magnetT = Mathf.Max(_magnetT, magnetSeconds);
    public void ActivateShield() => _shieldT = Mathf.Max(_shieldT, shieldSeconds);
    public void ActivateLetterSlow() => _letterT = Mathf.Max(_letterT, letterSlowSeconds);
}
