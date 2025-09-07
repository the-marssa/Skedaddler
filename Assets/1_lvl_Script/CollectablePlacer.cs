using System.Collections.Generic;
using UnityEngine;
using VContainer;
using VContainer.Unity;

[System.Serializable]
public class WeightedCollectable
{
    public GameObject prefab;
    [Range(0f, 1f)] public float weight = 1f;
}

[DisallowMultipleComponent]
public class CollectablePlacer : MonoBehaviour
{
    [Header("Prefabs (weighted)")]
    [SerializeField] private List<WeightedCollectable> entries = new();

    [Header("Points")]
    [SerializeField] private Transform pointsParent;
    [SerializeField, Range(0f, 1f)] private float spawnProbability = 1f;
    [SerializeField] private float yOffset = 0.1f;

    [Header("Random")]
    [SerializeField] private bool useSeed = false;
    [SerializeField] private int randomSeed = 12345;

    [Header("Optional Despawn Behind Player")]
    [SerializeField] private Transform player;
    [SerializeField, Min(0f)] private float despawnBehind = 0f;

    
    private readonly List<GameObject> _spawned = new();
    private float _totalWeight;

    private IObjectResolver _resolver;  
    private LifetimeScope _scope;       

    [Inject]
    public void Construct(IObjectResolver resolver) => _resolver = resolver;

    private void Start()
    {
        
        if (_resolver == null) TryResolveScope();

        if (useSeed) Random.InitState(randomSeed);
        RecalcTotal();
        Clear();
        SpawnOnPoints();

        if (despawnBehind > 0f && player == null)
            Debug.LogWarning("[CollectablePlacer] 'Despawn Behind' is set but no Player assigned.");
    }

    private void Update()
    {
        if (player == null || despawnBehind <= 0f) return;

        
        for (int i = _spawned.Count - 1; i >= 0; i--)
        {
            if (_spawned[i] == null) { _spawned.RemoveAt(i); continue; }

            Vector3 toItem = _spawned[i].transform.position - player.position;
            float along = Vector3.Dot(player.forward, toItem); 
            if (along < -despawnBehind)
            {
                Destroy(_spawned[i]);
                _spawned.RemoveAt(i);
            }
        }
    }

    
    private void SpawnOnPoints()
    {
        if (pointsParent == null)
        {
            Debug.LogWarning("[CollectablePlacer] Points Parent is not assigned.");
            return;
        }
        if (entries.Count == 0 || _totalWeight <= 0f)
        {
            Debug.LogWarning("[CollectablePlacer] No entries with positive weight.");
            return;
        }

        for (int i = 0; i < pointsParent.childCount; i++)
        {
            if (Random.value > spawnProbability) continue;

            Transform p = pointsParent.GetChild(i);
            Vector3 pos = p.position + Vector3.up * yOffset;

            GameObject prefab = WeightedPick();
            if (prefab == null) continue;

            GameObject go = SpawnViaContainer(prefab, pos, Quaternion.identity, transform);
            _spawned.Add(go);
        }
    }

    private GameObject SpawnViaContainer(GameObject prefab, Vector3 pos, Quaternion rot, Transform parent)
    {
        
        if (_resolver != null)
            return _resolver.Instantiate(prefab, pos, rot, parent);

       
        var go = Instantiate(prefab, pos, rot, parent);
        if (_scope != null && _scope.Container != null)
            _scope.Container.InjectGameObject(go);

        return go;
    }

   
    private void RecalcTotal()
    {
        _totalWeight = 0f;
        foreach (var e in entries)
        {
            if (e == null || e.prefab == null) continue;
            _totalWeight += Mathf.Max(0f, e.weight);
        }
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

       
        for (int i = entries.Count - 1; i >= 0; i--)
            if (entries[i] != null && entries[i].prefab != null)
                return entries[i].prefab;

        return null;
    }

    private void Clear()
    {
        for (int i = _spawned.Count - 1; i >= 0; i--)
        {
            if (_spawned[i] != null) Destroy(_spawned[i]);
            _spawned.RemoveAt(i);
        }
    }

    private void TryResolveScope()
    {
#if UNITY_2023_1_OR_NEWER
        _scope = FindFirstObjectByType<LifetimeScope>(FindObjectsInactive.Exclude);
#else
#pragma warning disable CS0618
        _scope = FindObjectOfType<LifetimeScope>();
#pragma warning restore CS0618
#endif
        if (_scope == null)
            Debug.LogError("[CollectablePlacer] LifetimeScope not found in scene (fallback). " +
                           "Ensure there is an active LifetimeScope.");
    }
}
