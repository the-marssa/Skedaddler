using System.Collections;
using UnityEngine;

public enum CollectableType { Health, Score }

public class Collectable : MonoBehaviour
{
    [Header("Setup")]
    [SerializeField] private CollectableType type = CollectableType.Score;
    [SerializeField, Min(0)] private int value = 1;

    [Header("Feedback")]
    [SerializeField] private float rotateSpeed = 90f;
    [SerializeField] private GameObject pickupVfx;
    [SerializeField] private AudioClip pickupSfx;
    [SerializeField, Min(0f)] private float destroyDelay = 0f;

    private void Update()
    {
        if (rotateSpeed != 0f)
            transform.Rotate(0f, rotateSpeed * Time.deltaTime, 0f);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if (type == CollectableType.Health && other.TryGetComponent(out PlayerHealth hp))
            hp.AddHealth(value);
        else if (type == CollectableType.Score)
            ScoreManager.Instance?.Add(value);

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
