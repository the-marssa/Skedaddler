using UnityEngine;
using UnityEngine.Events;

[DisallowMultipleComponent]
[RequireComponent(typeof(Collider))]
public sealed class Obstacle : MonoBehaviour
{
    [System.Serializable] public class IntEvent : UnityEvent<int> { }

    [Header("Hit")]
    [SerializeField] private int damage = 1;
    [SerializeField] private bool oneShot = true;

    [Header("FX")]
    [SerializeField] private GameObject hitFx;
    [SerializeField] private AudioSource hitAudio;

    [Header("Notify (optional)")]
    [Tooltip("ApplyDamage")]
    [SerializeField] private IntEvent onHit; 

    bool _consumed;

    void OnTriggerEnter(Collider other)
    {
        if (_consumed) return;

        if (!other.TryGetComponent<PlayerController>(out var ctrl)) return;

        if (hitFx) Instantiate(hitFx, transform.position, Quaternion.identity);
        if (hitAudio) hitAudio.Play();

        ctrl.LockControlsOnHit();

        onHit?.Invoke(damage);

        if (oneShot)
        {
            _consumed = true;
            DisableAllColliders();
        }
    }
    void DisableAllColliders()
    {
        var cols = GetComponentsInChildren<Collider>(includeInactive: false);
        for (int i = 0; i < cols.Length; i++)
        {
            if (cols[i] != null) cols[i].enabled = false;
        }
    }
}
