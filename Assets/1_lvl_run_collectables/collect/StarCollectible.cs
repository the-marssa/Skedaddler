using UnityEngine;

public class StarCollectible : MonoBehaviour
{
    [SerializeField] int points = 1;
    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        ScoreManager.Instance.Add(points);
        Destroy(gameObject);
    }
}
