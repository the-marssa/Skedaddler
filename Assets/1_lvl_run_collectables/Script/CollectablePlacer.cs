using System.Collections.Generic;
using UnityEngine;

public class CollectablePlacer : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private List<GameObject> collectablePrefabs = new(); // Heart, Star

    [Header("Points")]
    [SerializeField] private Transform pointsParent; // CollectablePoints
    [SerializeField, Range(0f, 1f)] private float spawnProbability = 0.65f;
    [SerializeField] private float yOffset = 0f;

    [Header("Random")]
    [SerializeField] private bool useSeed = false;
    [SerializeField] private int randomSeed = 12345;

    [Header("Optional Despawn Behind Player")]
    [SerializeField] private Transform player;                 // assign Player transform if you want auto-despawn
    [SerializeField, Min(0f)] private float despawnBehind = 15f;

    private readonly List<GameObject> spawned = new();

    private void OnEnable()
    {
        if (useSeed) Random.InitState(randomSeed);
        Clear();
        SpawnOnPoints();
    }

    private void OnDisable() => Clear();

    private void Update()
    {
        // Optional cleanup: destroy collectibles that fell behind the player
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
        if (!pointsParent || collectablePrefabs.Count == 0) return;

        for (int i = 0; i < pointsParent.childCount; i++)
        {
            if (Random.value > spawnProbability) continue;

            Transform p = pointsParent.GetChild(i);
            Vector3 pos = p.position + Vector3.up * yOffset;

            var prefab = collectablePrefabs[Random.Range(0, collectablePrefabs.Count)];
            var go = Instantiate(prefab, pos, Quaternion.identity, transform); // INSTANTIATE
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