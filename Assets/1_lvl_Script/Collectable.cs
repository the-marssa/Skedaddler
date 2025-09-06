using UnityEngine;

[RequireComponent(typeof(Collider))]
[DisallowMultipleComponent]
public class Collectable : MonoBehaviour
{
    public enum Type { Heart, Star, Letter, Magnet, Shield }
    [SerializeField] private Type type = Type.Star;
    [SerializeField] private string targetTag = "Player";
    [SerializeField] private int amountOverride = -1;     
    [SerializeField] private float durationOverride = -1; 

    private IRunSessionProvider _provider;
    private RunRewardsConfig _rewards;
    private IPlayerHealth _hp;
    private IPlayerPowerups _powerups;

    [VContainer.Inject]
    public void Construct(IRunSessionProvider provider, RunRewardsConfig rewards,
                          IPlayerHealth hp, IPlayerPowerups powerups)
    { _provider = provider; _rewards = rewards; _hp = hp; _powerups = powerups; }

    private void Reset()
    {
        var col = GetComponent<Collider>(); col.isTrigger = true;
        if (!TryGetComponent<Rigidbody>(out var rb)) rb = gameObject.AddComponent<Rigidbody>();
        rb.isKinematic = true; rb.useGravity = false;
    }

    private bool _consumed;
    private void OnTriggerEnter(Collider other)
    {
        if (_consumed || !other.CompareTag(targetTag)) return;

        switch (type)
        {
            case Type.Heart:
                int heal = amountOverride > 0 ? amountOverride :
                           (_rewards ? _rewards.heartHeal : 1);
                if (!(_hp != null && _hp.Heal(heal))) return;
                break;

            case Type.Star:
                int s = amountOverride > 0 ? amountOverride :
                        (_rewards ? _rewards.starScore : 1);
                _provider?.Current?.AddStars(s);
                break;

            case Type.Letter:
                int l = amountOverride > 0 ? amountOverride :
                        (_rewards ? _rewards.letterValue : 1);
                _provider?.Current?.AddLetters(l);
                break;

            case Type.Magnet:
                float m = durationOverride > 0 ? durationOverride :
                          (_rewards ? _rewards.magnetDuration : 5f);
                _powerups?.ActivateMagnet(m);
                break;

            case Type.Shield:
                float sh = durationOverride > 0 ? durationOverride :
                           (_rewards ? _rewards.shieldDuration : 5f);
                _powerups?.ActivateShield(sh);
                break;
        }

        _consumed = true;
        Destroy(gameObject);
    }
}
