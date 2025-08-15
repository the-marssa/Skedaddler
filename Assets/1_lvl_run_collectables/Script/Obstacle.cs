using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Obstacle : MonoBehaviour
{
    [SerializeField] int damage = 1;

    [Header("Animator (Manual)")]
    [SerializeField] Animator animator;
    [SerializeField] string triggerName = "Hit";

    [Header("Legacy Animation (optional)")]
    [SerializeField] Animation legacyAnimation;
    [SerializeField] string legacyClipName = "";

    [Header("FX (optional)")]
    [SerializeField] ParticleSystem vfx;
    [SerializeField] AudioSource sfx;

    [SerializeField] bool disableAfterHit = true;
    [SerializeField] float removeDelay = 1.2f;

    bool consumed;

    void Awake()
    {
        if (!animator) animator = GetComponentInChildren<Animator>();
        if (!legacyAnimation) legacyAnimation = GetComponentInChildren<Animation>();
    }

    void OnTriggerEnter(Collider other) { Handle(other); }
    void OnCollisionEnter(Collision c) { Handle(c.collider); }

    void Handle(Collider col)
    {
        if (consumed) return;

        var ctrl = col.GetComponentInParent<PlayerController>();
        if (!ctrl) return;

        consumed = true;

        ctrl.TryHit();
        var hp = ctrl.GetComponent<PlayerHealth>() ?? ctrl.GetComponentInParent<PlayerHealth>();
        if (hp) hp.TakeDamage(damage);

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
            foreach (var c in GetComponentsInChildren<Collider>()) c.enabled = false;
            if (removeDelay > 0f) Destroy(gameObject, removeDelay);
        }
    }
}
