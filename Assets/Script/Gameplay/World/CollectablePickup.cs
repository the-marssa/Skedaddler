using Dreamteck.Forever;
using UnityEngine;

public enum CollectableKind { Star, Heart, Letter, Magnet, Shield }

[RequireComponent(typeof(Collider))]
[DisallowMultipleComponent]
public class CollectablePickup : MonoBehaviour
{
    [Header("Type & Amount")]
    public CollectableKind kind = CollectableKind.Star;
    public int amount = 1;

    [Header("Behaviour")]
    public bool destroyOnPickup = true;

    void Reset()
    {
        var c = GetComponent<Collider>();
        c.isTrigger = true;
    }

    void OnTriggerEnter(Collider other)
    {
        if (GameCore.Instance == null || GameCore.Instance.CurrentRun == null) return;

        var go = other.attachedRigidbody ? other.attachedRigidbody.gameObject : other.gameObject;
        if (!go) return;
        var root = go.transform;

        bool isPlayer =
            root.GetComponentInParent<Runner>() != null ||
            root.GetComponentInParent<PlayerController>() != null ||
            go.CompareTag("Player") ||
            root.CompareTag("Player");

        if (!isPlayer) return;

        var pp = root.GetComponentInParent<PlayerPowers>();

        switch (kind)
        {
            case CollectableKind.Star:
                GameCore.Instance.AddStar(Mathf.Max(1, amount));
                break;

            case CollectableKind.Heart:
                GameCore.Instance.AddHeart(Mathf.Max(1, amount));
                break;

            case CollectableKind.Letter:
                GameCore.Instance.AddLetter(Mathf.Max(1, amount));
                break;

            case CollectableKind.Magnet:
                if (pp) pp.ActivateMagnet();
                break;

            case CollectableKind.Shield:
                if (pp) pp.ActivateShield();
                break;
        }

        if (destroyOnPickup) Destroy(gameObject);
    }
}
