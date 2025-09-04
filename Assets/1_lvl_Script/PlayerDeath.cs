using UnityEngine;
using JSAM;
using MoreMountains.Feedbacks;

[DisallowMultipleComponent]
public class PlayerDeath : MonoBehaviour
{
    [Header("Animation")]
    [SerializeField] Animator animator;
    [SerializeField] string dieTrigger = "Die";
    [SerializeField] float dieAnimDuration = 1.0f;
    public float Duration => dieAnimDuration;

    [Header("Disable on Death")]
    [SerializeField] MonoBehaviour[] disableOnDeath;

    [Header("Audio (JSAM)")]
    [SerializeField] bool stopMusicOnDeath = true;
    [SerializeField] Run_audiolibraryMusic runMusic = Run_audiolibraryMusic.Play_sfx;
    [SerializeField] bool playDeathSfx = true;
    [SerializeField] Run_audiolibrarySounds deathSfx = Run_audiolibrarySounds.Death_sfx;
    [SerializeField] bool deathSfx3DFromThis = false;

    [Header("FEEL")]
    [SerializeField] private MMF_Player deathFx;

    public bool IsDead { get; private set; }

    private void Awake()
    {
        if (!animator) animator = GetComponentInChildren<Animator>();
        GameRefs.PlayerDeath = this;
    }

    private void OnDestroy()
    {
        if (GameRefs.PlayerDeath == this) GameRefs.PlayerDeath = null;
    }

    public void Play()
    {
        if (IsDead) return;
        IsDead = true;

        foreach (var c in disableOnDeath) if (c) c.enabled = false;
        if (animator) animator.SetTrigger(dieTrigger);
        deathFx?.PlayFeedbacks();
        if (stopMusicOnDeath)
        {
            AudioManager.StopMusic(runMusic);
        }

        if (playDeathSfx)
        {
            if (deathSfx3DFromThis) AudioManager.PlaySound(deathSfx, transform);
            else AudioManager.PlaySound(deathSfx);
        }
    }
}
