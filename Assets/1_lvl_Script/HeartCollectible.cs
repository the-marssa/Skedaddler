using UnityEngine;

[RequireComponent(typeof(Collider))]
public class HeartCollectible : MonoBehaviour
{
    [SerializeField] private int amount = 1;
    [SerializeField] private string targetTag = "Player";
    [SerializeField] private bool consumeEvenIfNoHeal = false;

    private bool consumed;

    private void OnTriggerEnter(Collider other)
    {
        if (consumed) return;
        if (!other.CompareTag(targetTag)) return;

        var hp = GameRefs.PlayerHealth;

        bool healed = (hp != null) && hp.Heal(amount);
        if (healed || consumeEvenIfNoHeal)
        {
            if (healed && StatsManager.Instance != null) StatsManager.Instance.AddHeartPickup(amount);
            consumed = true;
            Destroy(gameObject);
        }
    }
}
