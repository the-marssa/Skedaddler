using UnityEngine;

public class HeartCollectible : MonoBehaviour
{
    [SerializeField] int amount = 1;
    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        var hp = other.GetComponent<PlayerHealth>();
        if (hp != null) hp.Heal(amount);
        Destroy(gameObject);
    }
}
