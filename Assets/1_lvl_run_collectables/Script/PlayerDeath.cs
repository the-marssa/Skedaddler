using UnityEngine;

public class PlayerDeath : MonoBehaviour
{
    [SerializeField] Animator animator;
    [SerializeField] string dieTrigger = "Die";
    [SerializeField] MonoBehaviour[] disableOnDeath; 
    [SerializeField] float dieAnimDuration = 1.0f;   

    public bool IsDead { get; private set; }
    public float Duration => dieAnimDuration;

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
    }
}
