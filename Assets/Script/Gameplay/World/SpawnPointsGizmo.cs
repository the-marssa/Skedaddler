using UnityEngine;

public class SpawnPointsGizmo : MonoBehaviour
{
    [SerializeField] Color color = new Color(1f, 0.85f, 0.2f, 0.8f);
    [SerializeField, Min(0.01f)] float radius = 0.15f;
    private void OnDrawGizmos()
    {
        Gizmos.color = color;
        for (int i = 0; i < transform.childCount; i++)
            Gizmos.DrawSphere(transform.GetChild(i).position, radius);
    }
}
