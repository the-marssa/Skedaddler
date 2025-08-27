using UnityEngine;
using JSAM; 

[RequireComponent(typeof(Collider))]
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

    [Header("Sound (JSAM Sound Library)")]
    [Tooltip("Obstacle sound from JSAM Sound Library")]
    [SerializeField] private Run_audiolibrarySounds hitJsamSound = Run_audiolibrarySounds.Bin_sfx;
    [Tooltip("3D")]
    [SerializeField] private bool playJsam3DFromThis = true;
    [Tooltip("Play JSAM-sound ")]
    [SerializeField] private bool playJsamSound = true;

    [Header("Lifecycle")]
    [SerializeField] private bool disableAfterHit = true;
    [SerializeField] private float removeDelay = 1.2f;
    [SerializeField] private Collider[] collidersToDisable;

    private bool consumed;

    private void OnTriggerEnter(Collider other) { Handle(other); }
    private void OnCollisionEnter(Collision col) { Handle(col.collider); }

    private void Handle(Collider col)
    {
        if (consumed) return;
        if (!col.CompareTag(targetTag)) return;

        bool shieldActive = (GameRefs.PlayerShield != null) && GameRefs.PlayerShield.IsActive;
        consumed = true;

        if (!shieldActive)
        {
            if (GameRefs.PlayerController != null) GameRefs.PlayerController.TryHit();
            if (GameRefs.PlayerHealth != null) GameRefs.PlayerHealth.TakeDamage(damage);
        }

        
        if (animator != null && !string.IsNullOrEmpty(triggerName)) animator.SetTrigger(triggerName);
        else if (legacyAnimation != null)
        {
            if (!string.IsNullOrEmpty(legacyClipName)) legacyAnimation.Play(legacyClipName);
            else legacyAnimation.Play();
        }
        if (vfx != null) vfx.Play();
        if (sfx != null) sfx.Play();

     
        if (playJsamSound)
        {
            if (playJsam3DFromThis) AudioManager.PlaySound(hitJsamSound, transform);
            else AudioManager.PlaySound(hitJsamSound);
        }

        
        if (disableAfterHit)
        {
            if (collidersToDisable != null && collidersToDisable.Length > 0)
            {
                for (int i = 0; i < collidersToDisable.Length; i++)
                    if (collidersToDisable[i] != null) collidersToDisable[i].enabled = false;
            }
            else
            {
                Collider selfCol = GetComponent<Collider>();
                if (selfCol != null) selfCol.enabled = false;
            }

            if (removeDelay > 0f) Destroy(gameObject, removeDelay);
        }
    }
}
