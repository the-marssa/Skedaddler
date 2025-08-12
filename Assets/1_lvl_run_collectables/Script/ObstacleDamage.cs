using UnityEngine;

public class ObstacleDamage : MonoBehaviour
{
    [SerializeField] int damage = 1;
    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        var hp = other.GetComponent<PlayerHealth>();
        if (hp != null) hp.TakeDamage(damage);
    }
}
