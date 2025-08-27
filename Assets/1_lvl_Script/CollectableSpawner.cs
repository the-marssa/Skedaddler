using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class CollectableSpawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private List<GameObject> collectablePrefabs = new(); // Heart, Star

    [Header("Spawn Area")]
    [SerializeField, Min(1f)] private float spawnAhead = 60f;   // how far ahead we keep items spawned
    [SerializeField, Min(1f)] private float despawnBehind = 25f;// cleanup distance behind player
    [SerializeField, Min(1f)] private float spacingZ = 8f;      // distance between rows
    [SerializeField, Min(0f)] private float yOffset = 0.1f;     // small lift above ground

    [Header("Lanes & Probabilities")]
    [SerializeField, Min(0.5f)] private float laneWidth = 2.2f;  // lanes at -1, 0, +1
    [SerializeField, Range(0f, 1f)] private float spawnProbabilityPerLane = 0.65f;

    [Header("Loop (Coroutine)")]
    [SerializeField, Min(0.05f)] private float pollInterval = 0.25f; // how often we extend/cleanup

    [Header("Random")]
    [SerializeField] private bool useSeed = false;
    [SerializeField] private int randomSeed = 12345;

    private readonly List<Transform> spawned = new();
    private float nextSpawnZ;
    private Coroutine loop;

    private void Start()
    {
        if (useSeed) Random.InitState(randomSeed);
        nextSpawnZ = player ? player.position.z + 5f : 5f;

        loop = StartCoroutine(SpawnLoop());
    }

    private void OnDisable()
    {
        if (loop != null) StopCoroutine(loop);

        // Cleanup all spawned on disable (optional)
        for (int i = spawned.Count - 1; i >= 0; i--)
        {
            if (spawned[i] != null) Destroy(spawned[i].gameObject);
        }
        spawned.Clear();
    }

    private IEnumerator SpawnLoop()
    {
        while (true)
        {
            if (player != null && collectablePrefabs.Count > 0)
            {
                // OWN SPAWN ALGORITHM:
                // Extend rows forward up to player.z + spawnAhead, spaced by spacingZ.
                float targetZ = player.position.z + spawnAhead;
                while (nextSpawnZ <= targetZ)
                {
                    SpawnRow(nextSpawnZ);     // custom per-row logic
                    nextSpawnZ += spacingZ;
                }

                // Remove items that fell far behind the player.
                DespawnOld();
            }

            yield return new WaitForSeconds(pollInterval);
        }
    }

    private void SpawnRow(float z)
    {
        // OWN SPAWN ALGORITHM (row):
        // For this row at Z = z, roll each lane (-1, 0, +1). If passed probability,
        // choose a random prefab (Heart/Star) and spawn it.
        int[] lanes = { -1, 0, 1 };

        foreach (int lane in lanes)
        {
            if (Random.value > spawnProbabilityPerLane) continue;

            float x = lane * laneWidth;
            Vector3 pos = new Vector3(x, yOffset, z);

            GameObject prefab = collectablePrefabs[Random.Range(0, collectablePrefabs.Count)];

            // INSTANTIATE (spawn Heart/Star here)
            GameObject go = Instantiate(prefab, pos, Quaternion.identity, transform);

            spawned.Add(go.transform);
        }
    }

    private void DespawnOld()
    {
        float minZ = player.position.z - despawnBehind;

        for (int i = spawned.Count - 1; i >= 0; i--)
        {
            if (spawned[i] == null) { spawned.RemoveAt(i); continue; }
            if (spawned[i].position.z < minZ)
            {
                Destroy(spawned[i].gameObject);
                spawned.RemoveAt(i);
            }
        }
    }
}
