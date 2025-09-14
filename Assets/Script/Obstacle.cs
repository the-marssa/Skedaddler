using UnityEngine;

[RequireComponent(typeof(Collider))]
[DisallowMultipleComponent]
public class Obstacle : MonoBehaviour
{
    [SerializeField] int damage = 1;

    [Header("Filter")]
    [SerializeField] string targetTag = "Player";

    [Header("FX")]
    [SerializeField] Animator animator;
    [SerializeField] string triggerName = "Hit";
    [SerializeField] Animation legacyAnimation;
    [SerializeField] string legacyClipName = "";
    [SerializeField] ParticleSystem vfx;
    [SerializeField] AudioSource sfx;

    [Header("Lifecycle")]
    [SerializeField] bool disableAfterHit = true;
    [SerializeField] float removeDelay = 1.2f;
    [SerializeField] Collider[] collidersToDisable;

    bool consumed;

    void Reset()
    {
        var c = GetComponent<Collider>();
        if (c) c.isTrigger = true;      
    }

    void OnTriggerEnter(Collider other) => Handle(other);
    void OnCollisionEnter(Collision collision) => Handle(collision.collider);

    void Handle(Collider col)
    {
        if (consumed) return;

        var go = col.attachedRigidbody ? col.attachedRigidbody.gameObject : col.gameObject;
        if (!go || !go.activeInHierarchy || !go.CompareTag(targetTag)) return;

        consumed = true;

        var pp = go.GetComponentInParent<PlayerPowers>();
        if (pp != null) pp.TryObstacleHit(Mathf.Max(1, damage));
        else GameCore.Instance?.ApplyHit(Mathf.Max(1, damage));

        
        if (animator && !string.IsNullOrEmpty(triggerName)) animator.SetTrigger(triggerName);
        else if (legacyAnimation)
        {
            if (!string.IsNullOrEmpty(legacyClipName)) legacyAnimation.Play(legacyClipName);
            else legacyAnimation.Play();
        }
        if (vfx) vfx.Play();
        if (sfx) sfx.Play();

        if (disableAfterHit)
        {
            if (collidersToDisable != null && collidersToDisable.Length > 0)
                foreach (var c in collidersToDisable) if (c) c.enabled = false;
                    else
                    {
                        var self = GetComponent<Collider>();
                        if (self) self.enabled = false;
                    }

            if (removeDelay > 0f) Destroy(gameObject, removeDelay);
        }
    }
}
