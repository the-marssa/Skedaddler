using UnityEngine;
using JSAM;
using MoreMountains.Feedbacks;

[DisallowMultipleComponent]
public class PlayerDeath : MonoBehaviour
{
    [Header("Animation")]
    [SerializeField] private Animator animator;
    [SerializeField] private string dieTrigger = "Die";
    [SerializeField] private float dieAnimDuration = 1.0f;
    public float Duration => dieAnimDuration;

    [Header("Disable on Death")]
    [SerializeField] private MonoBehaviour[] disableOnDeath;

    [Header("Audio (JSAM)")]
    [SerializeField] private bool stopMusicOnDeath = true;
    [SerializeField] private Run_audiolibraryMusic runMusic = Run_audiolibraryMusic.Play_sfx;
    [SerializeField] private bool playDeathSfx = true;
    [SerializeField] private Run_audiolibrarySounds deathSfx = Run_audiolibrarySounds.Death_sfx;
    [SerializeField] private bool deathSfx3DFromThis = false;

    [Header("FEEL")]
    [SerializeField] private MMF_Player deathFx;

    public bool IsDead { get; private set; }

    private void Awake()
    {
        if (!animator) animator = GetComponentInChildren<Animator>();
    }

    public void Play()
    {
        if (IsDead) return;
        IsDead = true;

        if (disableOnDeath != null)
            foreach (var c in disableOnDeath) if (c) c.enabled = false;

        if (animator) animator.SetTrigger(dieTrigger);
        deathFx?.PlayFeedbacks();

        if (stopMusicOnDeath) AudioManager.StopMusic(runMusic);
        if (playDeathSfx)
        {
            if (deathSfx3DFromThis) AudioManager.PlaySound(deathSfx, transform);
            else AudioManager.PlaySound(deathSfx);
        }
    }

    public void ResetAlive()
    {
        if (!IsDead) return;
        IsDead = false;

        if (disableOnDeath != null)
            foreach (var c in disableOnDeath) if (c) c.enabled = true;

        if (animator)
        {
            animator.ResetTrigger(dieTrigger);
            animator.applyRootMotion = false;
            animator.Rebind();
            animator.Update(0f);
            animator.speed = 1f;
        }
    }
}
