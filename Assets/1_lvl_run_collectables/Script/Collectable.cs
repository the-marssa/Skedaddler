using System.Collections;
using UnityEngine;

public enum CollectableType { Score, Health, Letter, Shield, Magnet }

public class Collectable : MonoBehaviour
{
    private const string PlayerTag = "Player";

    [Header("Setup")]
    [SerializeField] private CollectableType type = CollectableType.Score;
    [SerializeField, Min(0)] private int value = 1;      
    [SerializeField, Min(0f)] private float effectSeconds = 5f; 

    [Header("Feedback")]
    [SerializeField] private float rotateSpeed = 90f;
    [SerializeField] private GameObject pickupVfx;
    [SerializeField] private AudioClip pickupSfx;
    [SerializeField, Min(0f)] private float destroyDelay = 0f;

    private bool _collected;

    private void Update()
    {
        if (!_collected && rotateSpeed != 0f)
            transform.Rotate(0f, rotateSpeed * Time.deltaTime, 0f);

      
        if (!_collected && (type == CollectableType.Score || type == CollectableType.Health || type == CollectableType.Shield))
        {
            var mag = GameRefs.PlayerMagnet;
            if (mag != null && mag.IsActive)
            {
                Vector3 toPlayer = mag.Center - transform.position;
                if (toPlayer.sqrMagnitude <= mag.Radius * mag.Radius)
                {
                    transform.position = Vector3.MoveTowards(transform.position, mag.Center, mag.PullSpeed * Time.deltaTime);
                }
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_collected || !other.CompareTag(PlayerTag)) return;
        _collected = true;

        switch (type)
        {
            case CollectableType.Health:
                GameRefs.PlayerHealth?.Heal(value);
                break;
            case CollectableType.Score:
                GameRefs.Score?.Add(value);
                break;
            case CollectableType.Letter:
                MailManager.Instance?.Add(value);
                break;
            case CollectableType.Shield:
                GameRefs.PlayerShield?.Enable(effectSeconds);
                break;
            case CollectableType.Magnet:
                GameRefs.PlayerMagnet?.Enable(effectSeconds);
                break;
        }

        if (pickupVfx) Instantiate(pickupVfx, transform.position, Quaternion.identity);
        if (pickupSfx) AudioSource.PlayClipAtPoint(pickupSfx, transform.position);

        if (destroyDelay <= 0f) Destroy(gameObject);
        else StartCoroutine(DestroyAfterDelay());
    }

    private IEnumerator DestroyAfterDelay()
    {
        yield return new WaitForSeconds(destroyDelay);
        Destroy(gameObject);
    }
}
