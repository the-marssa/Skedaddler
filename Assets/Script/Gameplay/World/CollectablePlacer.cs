using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class CollectablePlacer: MonoBehaviour
{
    [System.Serializable]
    public class WeightedPrefab
    {
        public GameObject prefab;
        [Range(0f, 1f)] public float weight = 1f;
    }

    [Header("Prefabs (weighted)")]
    [SerializeField] private List<WeightedPrefab> prefabs = new();

    [Header("Spawn Points")]
    [SerializeField] private Transform pointsParent;
    [SerializeField, Range(0f, 1f)] private float spawnProbability = 1f;
    [SerializeField] private float yOffset = 0.1f;

    [Header("Optional Despawn Behind Player")]
    [SerializeField] private Transform player;
    [SerializeField, Min(0f)] private float despawnBehind = 0f;

    private readonly List<GameObject> _spawned = new();

    void Start() => SpawnAll();

    public void SpawnAll()
    {
        if (!pointsParent) return;

        var points = new List<Transform>();
        for (int i = 0; i < pointsParent.childCount; i++) points.Add(pointsParent.GetChild(i));
        if (points.Count == 0) return;

        float totalW = 0f;
        foreach (var w in prefabs) totalW += Mathf.Max(0f, w.weight);
        if (totalW <= 0f) return;

        foreach (var p in points)
        {
            if (Random.value > spawnProbability) continue;
            var pick = Pick();
            if (!pick) continue;
            var pos = p.position + Vector3.up * yOffset;
            var go = Instantiate(pick, pos, p.rotation, transform);
            _spawned.Add(go);
        }
    }

    GameObject Pick()
    {
        float total = 0f; foreach (var w in prefabs) total += Mathf.Max(0f, w.weight);
        if (total <= 0f) return null;
        float r = Random.value * total;
        foreach (var w in prefabs)
        {
            float wv = Mathf.Max(0f, w.weight);
            if (r <= wv) return w.prefab;
            r -= wv;
        }
        return prefabs[prefabs.Count - 1].prefab;
    }

    void Update()
    {
        if (player == null || despawnBehind <= 0f) return;

        for (int i = _spawned.Count - 1; i >= 0; i--)
        {
            var go = _spawned[i];
            if (!go) { _spawned.RemoveAt(i); continue; }

            Vector3 toItem = go.transform.position - player.position;
            float along = Vector3.Dot(player.forward, toItem);
            if (along < -despawnBehind)
            {
                Destroy(go);
                _spawned.RemoveAt(i);
            }
        }
    }
}
