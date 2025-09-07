using System.Reflection;
using UnityEngine;
using VContainer;

[RequireComponent(typeof(Collider))]
[DisallowMultipleComponent]
public class Collectable : MonoBehaviour
{
    public enum Type { Heart, Star, Letter, Magnet, Shield }

    [Header("Collectable")]
    [SerializeField] private Type type = Type.Star;
    [SerializeField] private string targetTag = "Player";

    [Tooltip("If > 0 — overrides default amount for stars/letters/heal")]
    [SerializeField] private int amountOverride = -1;

    [Tooltip("If > 0 — overrides default powerup duration (seconds)")]
    [SerializeField] private float durationOverride = -1f;

    [Tooltip("Heart only: consume the item even if Heal did nothing")]
    [SerializeField] private bool consumeEvenIfNoHeal = false;

    private IRunSessionProvider _provider;
    private RunRewardsConfig _rewards; 
    private IPlayerHealth _health;
    private IPlayerPowerups _powerups;

    private bool _consumed;

    [Inject]
    public void Construct(
        IRunSessionProvider provider,
        IPlayerPowerups powerups,
        IPlayerHealth health,
        RunRewardsConfig rewards)
    {
        _provider = provider;
        _powerups = powerups;
        _health = health;
        _rewards = rewards;
    }

    private void OnTriggerEnter(Collider other)
    {
        TryCollectFromCollider(other);
    }

    private void OnCollisionEnter(Collision collision)
    {
        
        TryCollectFromCollider(collision.collider);
    }

    private void TryCollectFromCollider(Collider other)
    {
        if (_consumed) return;

      
        var go = other.attachedRigidbody ? other.attachedRigidbody.gameObject : other.gameObject;
        var root = go.transform.root ? go.transform.root.gameObject : go;

        if (!(go.CompareTag(targetTag) || root.CompareTag(targetTag)))
            return;

        switch (type)
        {
            case Type.Star:
                {
                    int amount = amountOverride > 0 ? amountOverride
                        : TryGetInt(_rewards, new[] { "starAmount", "starsAmount", "starsPerPickup", "starPerPickup" }, 1);
                    _provider?.Current?.AddStars(amount);
                    break;
                }
            case Type.Letter:
                {
                    int amount = amountOverride > 0 ? amountOverride
                        : TryGetInt(_rewards, new[] { "letterAmount", "lettersAmount", "lettersPerPickup", "letterPerPickup" }, 1);
                    _provider?.Current?.AddLetters(amount);
                    break;
                }
            case Type.Heart:
                {
                    int heal = amountOverride > 0 ? amountOverride
                        : TryGetInt(_rewards, new[] { "heartHeal", "healAmount", "hpPerPickup" }, 1);
                    bool healed = _health != null && _health.Heal(heal);
                    if (!healed && !consumeEvenIfNoHeal) return;
                    break;
                }
            case Type.Magnet:
                {
                    float dur = durationOverride > 0f ? durationOverride
                        : TryGetFloat(_rewards, new[] { "magnetDuration", "magnetTime", "magnetSeconds" }, 5f);
                    _powerups?.ActivateMagnet(dur);
                    break;
                }
            case Type.Shield:
                {
                    float dur = durationOverride > 0f ? durationOverride
                        : TryGetFloat(_rewards, new[] { "shieldDuration", "shieldTime", "shieldSeconds" }, 5f);
                    _powerups?.ActivateShield(dur);
                    break;
                }
        }

        _consumed = true;
        Destroy(gameObject);
    }


    private static int TryGetInt(object so, string[] names, int fallback)
    {
        if (so == null) return fallback;
        var t = so.GetType();
        foreach (var n in names)
        {
            var f = t.GetField(n, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (f != null && f.FieldType == typeof(int)) return (int)f.GetValue(so);
            var p = t.GetProperty(n, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (p != null && p.PropertyType == typeof(int)) return (int)p.GetValue(so);
        }
        return fallback;
    }

    private static float TryGetFloat(object so, string[] names, float fallback)
    {
        if (so == null) return fallback;
        var t = so.GetType();
        foreach (var n in names)
        {
            var f = t.GetField(n, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (f != null && f.FieldType == typeof(float)) return (float)f.GetValue(so);
            var p = t.GetProperty(n, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (p != null && p.PropertyType == typeof(float)) return (float)p.GetValue(so);
        }
        return fallback;
    }
}
