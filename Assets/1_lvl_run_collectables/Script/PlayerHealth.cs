using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health")]
    [SerializeField, Min(1)] private int maxHealth = 5;
    [SerializeField, Tooltip("Current health for debugging/UI.")] private int current;

    public int MaxHealth => maxHealth;
    public int Current => current;

    private void Awake()
    {
        current = Mathf.Clamp(current, 0, maxHealth);
        if (current == 0) current = maxHealth; // start full if not set
    }

    public void AddHealth(int amount)
    {
        current = Mathf.Clamp(current + amount, 0, maxHealth);
    }
}
