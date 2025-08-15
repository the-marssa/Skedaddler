using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Obstacle : MonoBehaviour
{
    [SerializeField] private int damage = 1;

    [Header("Animator (Manual)")]
    [SerializeField] private Animator animator;
    [SerializeField] private string triggerName = "Hit";

    [Header("Legacy Animation (optional)")]
    [SerializeField] private Animation legacyAnimation;
    [SerializeField] private string legacyClipName = "";

    [Header("FX (optional)")]
    [SerializeField] private ParticleSystem vfx;
    [SerializeField] private AudioSource sfx;

    [SerializeField] private bool disableAfterHit = true;
    [SerializeField] private float removeDelay = 1.2f;

    private bool consumed;

    private void Awake()
    {
        if (!animator) animator = GetComponentInChildren<Animator>();
        if (!legacyAnimation) legacyAnimation = GetComponentInChildren<Animation>();
    }

    private void OnTriggerEnter(Collider other) => Handle(other);
    private void OnCollisionEnter(Collision col) => Handle(col.collider);

    private void Handle(Collider col)
    {
        if (consumed) return;

        
        var ctrl = col.GetComponentInParent<PlayerController>();
        if (!ctrl) return;

        bool shieldActive = GameRefs.PlayerShield != null && GameRefs.PlayerShield.IsActive;

        consumed = true;

        
        if (!shieldActive)
        {
            ctrl.TryHit();

            var hp = GameRefs.PlayerHealth
                     ?? ctrl.GetComponent<PlayerHealth>()
                     ?? ctrl.GetComponentInParent<PlayerHealth>();

            if (hp) hp.TakeDamage(damage);
        }

       
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
