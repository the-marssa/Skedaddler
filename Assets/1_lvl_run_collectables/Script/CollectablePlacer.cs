using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class WeightedCollectable
{
    public GameObject prefab;
    [Range(0f, 1f)] public float weight = 1f;
}

public class CollectablePlacer : MonoBehaviour
{
    [Header("Prefabs (weighted)")]
    [SerializeField] private List<WeightedCollectable> entries = new(); 

    [Header("Points")]
    [SerializeField] private Transform pointsParent;
    [SerializeField, Range(0f, 1f)] private float spawnProbability = 0.65f;
    [SerializeField] private float yOffset = 0f;

    [Header("Random")]
    [SerializeField] private bool useSeed = false;
    [SerializeField] private int randomSeed = 12345;

    [Header("Optional Despawn Behind Player")]
    [SerializeField] private Transform player;
    [SerializeField, Min(0f)] private float despawnBehind = 15f;

    private readonly List<GameObject> spawned = new();
    private float _totalWeight;

    private void RecalcTotal()
    {
        _totalWeight = 0f;
        foreach (var e in entries) if (e != null && e.prefab != null) _totalWeight += Mathf.Max(0f, e.weight);
    }

    private GameObject WeightedPick()
    {
        if (_totalWeight <= 0f) return null;
        float r = Random.value * _totalWeight;
        float acc = 0f;
        foreach (var e in entries)
        {
            if (e == null || e.prefab == null) continue;
            acc += Mathf.Max(0f, e.weight);
            if (r <= acc) return e.prefab;
        }
        return entries[entries.Count - 1].prefab;
    }

    private void OnEnable()
    {
        if (useSeed) Random.InitState(randomSeed);
        RecalcTotal();
        Clear();
        SpawnOnPoints();
    }

    private void OnDisable() => Clear();

    private void Update()
    {
        if (!player) return;
        float minZ = player.position.z - despawnBehind;
        for (int i = spawned.Count - 1; i >= 0; i--)
        {
            if (spawned[i] == null) { spawned.RemoveAt(i); continue; }
            if (spawned[i].transform.position.z < minZ)
            {
                Destroy(spawned[i]);
                spawned.RemoveAt(i);
            }
        }
    }

    private void SpawnOnPoints()
    {
        if (!pointsParent || entries.Count == 0) return;

        for (int i = 0; i < pointsParent.childCount; i++)
        {
            if (Random.value > spawnProbability) continue;

            Transform p = pointsParent.GetChild(i);
            Vector3 pos = p.position + Vector3.up * yOffset;

            var prefab = WeightedPick();
            if (!prefab) continue;

            var go = Instantiate(prefab, pos, Quaternion.identity, transform);
            spawned.Add(go);
        }
    }

    private void Clear()
    {
        for (int i = spawned.Count - 1; i >= 0; i--)
        {
            if (spawned[i] != null) Destroy(spawned[i]);
            spawned.RemoveAt(i);
        }
    }
}
