using UnityEngine;
using System;

[DisallowMultipleComponent]
public class PlayerHealth : MonoBehaviour
{
    private const int ABS_MAX_HP = 20;

    [SerializeField, Range(1, ABS_MAX_HP)] private int maxHP = ABS_MAX_HP;
    [SerializeField, Min(0f)] private float hitCooldown = 0.5f;

    public int Current { get; private set; }
    public int Max => Mathf.Min(maxHP, ABS_MAX_HP);

    public event Action<int, int> Changed;
    public event Action Died;

    private float lastHit = -999f;

    private void Awake()
    {
        Current = Max;
        GameRefs.PlayerHealth = this;
    }

    private void OnDestroy()
    {
        if (GameRefs.PlayerHealth == this) GameRefs.PlayerHealth = null;
    }

    public bool TakeDamage(int amount)
    {
        if (amount <= 0) return false;
        if (Time.time - lastHit < hitCooldown || Current <= 0) return false;

        lastHit = Time.time;
        Current = Mathf.Max(0, Current - amount);
        Changed?.Invoke(Current, Max);
        if (Current == 0) Died?.Invoke();
        return true;
    }

    public bool Heal(int amount)
    {
        if (amount <= 0 || Current <= 0) return false;

        int prev = Current;
        Current = Mathf.Min(Max, Current + amount); 
        if (Current != prev) Changed?.Invoke(Current, Max);
        return Current != prev;
    }

    public void ResetHP()
    {
        Current = Max;       
        lastHit = -999f;
        Changed?.Invoke(Current, Max);
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (maxHP < 1) maxHP = 1;
        if (maxHP > ABS_MAX_HP) maxHP = ABS_MAX_HP;
        if (hitCooldown < 0f) hitCooldown = 0f;
    }
#endif
}
