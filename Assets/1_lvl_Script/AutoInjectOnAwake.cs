using UnityEngine;
using VContainer;
using VContainer.Unity;

[DefaultExecutionOrder(-10000)]
public class AutoInjectOnAwake : MonoBehaviour
{
    void Awake()
    {
        LifetimeScope scope = null;

#if UNITY_2023_1_OR_NEWER
        scope = FindFirstObjectByType<LifetimeScope>(FindObjectsInactive.Exclude);
#else
#pragma warning disable CS0618
        scope = FindObjectOfType<LifetimeScope>(); // older Unity
#pragma warning restore CS0618
#endif

        if (scope?.Container != null)
        {
           
            scope.Container.InjectGameObject(gameObject);
        }
        else
        {
            Debug.LogError("[AutoInjectOnAwake] LifetimeScope not found in scene.");
        }
    }
}
