using System.Collections;
using UnityEngine;

public enum CollectableType { Score, Health, Letter, Shield, Magnet }

[RequireComponent(typeof(Collider))]
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
    [SerializeField, Range(0f, 1f)] private float pickupVolume = 1f;
    [SerializeField, Min(0f)] private float destroyDelay = 0f;

    private bool _collected;

    private void Update()
    {
        if (!_collected && rotateSpeed != 0f)
            transform.Rotate(0f, rotateSpeed * Time.deltaTime, 0f);

        if (_collected) return;

        var mag = GameRefs.PlayerMagnet;
        if (mag != null && mag.IsActive)
        {
            Vector3 centerOnPlane = new Vector3(mag.Center.x, transform.position.y, mag.Center.z);
            Vector3 toCenter = centerOnPlane - transform.position;

            float r = mag.Radius;
            if (toCenter.sqrMagnitude <= r * r)
            {
                transform.position = Vector3.MoveTowards(
                    transform.position,
                    centerOnPlane,
                    mag.PullSpeed * Time.deltaTime
                );
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
                {
                    if (GameRefs.PlayerHealth != null && GameRefs.PlayerHealth.Heal(value))
                    {
                        if (StatsManager.Instance != null) StatsManager.Instance.AddHeartPickup(1);
                    }
                    break;
                }

            case CollectableType.Score:
                {
                    if (GameRefs.Score != null) GameRefs.Score.Add(value);                
                    if (StatsManager.Instance != null) StatsManager.Instance.AddStars(value); 
                    break;
                }

            case CollectableType.Letter:
                {
                    if (MailManager.Instance != null) MailManager.Instance.Add(value);      
                    if (StatsManager.Instance != null) StatsManager.Instance.AddLetters(value);
                    break;
                }

            case CollectableType.Shield:
                {
                    if (GameRefs.PlayerShield != null) GameRefs.PlayerShield.Enable(effectSeconds);
                    break;
                }

            case CollectableType.Magnet:
                {
                    if (GameRefs.PlayerMagnet != null) GameRefs.PlayerMagnet.Enable(effectSeconds);
                    break;
                }
        }

        if (pickupVfx != null) Instantiate(pickupVfx, transform.position, Quaternion.identity);
        if (pickupSfx != null) AudioSource.PlayClipAtPoint(pickupSfx, transform.position, pickupVolume);

        if (destroyDelay <= 0f) Destroy(gameObject);
        else StartCoroutine(DestroyAfterDelay());
    }

    private IEnumerator DestroyAfterDelay()
    {
        yield return new WaitForSeconds(destroyDelay);
        Destroy(gameObject);
    }
}
