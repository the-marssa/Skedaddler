using UnityEngine;
using System;

public class PlayerHealth : MonoBehaviour, IPlayerHealth
{
    [SerializeField] private int max = 20;
    [SerializeField] private int start = 20;

    public int Current { get; private set; }
    public int Max => max;
    public bool IsDead { get; private set; }

    public event Action<int, int> Changed;
    public event Action Died;

    private void Awake()
    {
        max = Mathf.Max(1, max);
        Current = Mathf.Clamp(start, 0, max);
        IsDead = (Current <= 0);
        Changed?.Invoke(Current, Max);
        if (IsDead) SafeDie();
    }

    public void ResetFull()
    {
        IsDead = false;
        Current = Max;
        Changed?.Invoke(Current, Max);
    }

    public void Reset()
    {
        IsDead = false;
        Current = Mathf.Clamp(start, 0, Max);
        Changed?.Invoke(Current, Max);
    }

    public bool Heal(int amount)
    {
        if (amount <= 0) return false;
        int before = Current;
        Current = Mathf.Min(Max, Current + amount);
        if (Current != before)
        {
            if (IsDead && Current > 0) IsDead = false;
            Changed?.Invoke(Current, Max);
            return true;
        }
        return false;
    }

    public bool TakeDamage(int amount)
    {
        if (amount <= 0 || IsDead) return false;
        int before = Current;
        Current = Mathf.Max(0, Current - amount);
        if (Current != before)
        {
            Changed?.Invoke(Current, Max);
            if (Current == 0) SafeDie();
            return true;
        }
        return false;
    }

    private void SafeDie()
    {
        if (IsDead) return;
        IsDead = true;
        try { Died?.Invoke(); } catch { /* ignore */ }
    }
}
