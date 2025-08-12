using UnityEngine;
using System;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] int maxHP = 3;
    [SerializeField] float hitCooldown = 0.5f;

    public int Current { get; private set; }
    public int Max => maxHP;

    public event Action<int, int> Changed;
    public event Action Died;

    float lastHit = -999f;

    void Awake() { Current = maxHP; }

    public void TakeDamage(int amount)
    {
        if (Time.time - lastHit < hitCooldown || Current <= 0) return;
        lastHit = Time.time;
        Current = Mathf.Max(0, Current - amount);
        Changed?.Invoke(Current, maxHP);
        if (Current == 0) Died?.Invoke();
    }

    public void Heal(int amount)
    {
        if (Current <= 0) return;
        Current = Mathf.Min(maxHP, Current + amount);
        Changed?.Invoke(Current, maxHP);
    }
    public void ResetHP()
    {
        Current = Max;
        Changed?.Invoke(Current, Max);
    }

}
