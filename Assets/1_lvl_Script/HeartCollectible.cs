using UnityEngine;

[RequireComponent(typeof(Collider))]
public class HeartCollectible : MonoBehaviour
{
    [SerializeField] private string targetTag = "Player";
    [SerializeField] private bool consumeEvenIfNoHeal = false;

    private bool consumed;

    private void OnTriggerEnter(Collider other)
    {
        if (consumed) return;
        if (!other.CompareTag(targetTag)) return;

        var hp = GameRefs.PlayerHealth;
        var heart = ConfigProvider.I?.GetCollectible(GameConfig.CollectibleKind.Heart);
        int amount = (heart != null) ? heart.healAmount : 1;

        bool healed = (hp != null) && hp.Heal(amount);
        if (healed || consumeEvenIfNoHeal)
        {
            if (heart != null)
            {
                if (heart.sfx) AudioSource.PlayClipAtPoint(heart.sfx, transform.position);
                if (heart.vfxPrefab) Instantiate(heart.vfxPrefab, transform.position, Quaternion.identity);
            }

            consumed = true;
            Destroy(gameObject);
        }
    }
}
