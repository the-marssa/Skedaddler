using UnityEngine;
using VContainer;
using JSAM;

[RequireComponent(typeof(Collider))]
[DisallowMultipleComponent]
public class Obstacle : MonoBehaviour
{
    [SerializeField] private int damage = 1;

    [Header("Filter")]
    [SerializeField] private string targetTag = "Player";

    [Header("FX")]
    [SerializeField] private Animator animator;
    [SerializeField] private string triggerName = "Hit";
    [SerializeField] private Animation legacyAnimation;
    [SerializeField] private string legacyClipName = "";
    [SerializeField] private ParticleSystem vfx;
    [SerializeField] private AudioSource sfx;

    [Header("Sound (JSAM)")]
    [SerializeField] private Run_audiolibrarySounds hitJsamSound = Run_audiolibrarySounds.Bin_sfx;
    [SerializeField] private bool playJsam3DFromThis = true;
    [SerializeField] private bool playJsamSound = true;

    [Header("Lifecycle")]
    [SerializeField] private bool disableAfterHit = true;
    [SerializeField] private float removeDelay = 1.2f;
    [SerializeField] private Collider[] collidersToDisable;

    private bool consumed;

    private IPlayerPowerups _powerups;
    private IPlayerHealth _health;

    [Inject]
    public void Construct(IPlayerPowerups powerups, IPlayerHealth health)
    {
        _powerups = powerups;
        _health = health;
    }

    private void OnTriggerEnter(Collider other) => Handle(other);
    private void OnCollisionEnter(Collision col) => Handle(col.collider);

    private void Handle(Collider col)
    {
        if (consumed || !col.CompareTag(targetTag)) return;
        consumed = true;

        if (!(_powerups?.IsShieldActive ?? false))
            _health?.TakeDamage(Mathf.Max(1, damage));

        if (animator && !string.IsNullOrEmpty(triggerName)) animator.SetTrigger(triggerName);
        else if (legacyAnimation)
        {
            if (!string.IsNullOrEmpty(legacyClipName)) legacyAnimation.Play(legacyClipName);
            else legacyAnimation.Play();
        }
        if (vfx) vfx.Play();
        if (sfx) sfx.Play();

        if (playJsamSound)
        {
            if (playJsam3DFromThis) AudioManager.PlaySound(hitJsamSound, transform);
            else AudioManager.PlaySound(hitJsamSound);
        }

        if (disableAfterHit)
        {
            if (collidersToDisable != null && collidersToDisable.Length > 0)
                foreach (var c in collidersToDisable) if (c) c.enabled = false;
                    else
                    {
                        var selfCol = GetComponent<Collider>();
                        if (selfCol) selfCol.enabled = false;
                    }

            if (removeDelay > 0f) Destroy(gameObject, removeDelay);
        }
    }
}
