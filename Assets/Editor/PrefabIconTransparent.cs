#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.IO;

public static class PrefabIconTransparent
{
    private const int WIDTH = 512;  
    private const int HEIGHT = 512;
    private const float FOV = 20f;  
    private const float PADDING = 1.15f;

    [MenuItem("Tools/Icons/Render Transparent Icon (Selected Prefab)")]
    private static void RenderTransparentIcon()
    {
        var obj = Selection.activeObject;
        if (!obj || PrefabUtility.GetPrefabAssetType(obj) == PrefabAssetType.NotAPrefab)
        {
            EditorUtility.DisplayDialog("Icon Render", "Pick Prefab.", "OK");
            return;
        }

        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GetAssetPath(obj));
        if (!prefab)
        {
            EditorUtility.DisplayDialog("Icon Render", "Не удалось загрузить префаб.", "OK");
            return;
        }

        var savePath = EditorUtility.SaveFilePanel("Save icon PNG",
            Path.GetDirectoryName(AssetDatabase.GetAssetPath(prefab)),
            Path.GetFileNameWithoutExtension(AssetDatabase.GetAssetPath(prefab)) + "_icon.png", "png");
        if (string.IsNullOrEmpty(savePath)) return;

        var root = new GameObject("[IconRenderRoot]") { hideFlags = HideFlags.HideAndDontSave };
        try
        {
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            var t = instance.transform;
            t.SetParent(root.transform, false);
            t.position = Vector3.zero;
            instance.hideFlags = HideFlags.HideAndDontSave;

            var bounds = GetCombinedBounds(instance);
            if (bounds.size == Vector3.zero)
            {
                Object.DestroyImmediate(root);
                EditorUtility.DisplayDialog("Icon Render", "У префаба нет видимых Renderer’ов.", "OK");
                return;
            }

            int tempLayer = 31;
            int[] originalLayers = SetLayerRecursive(instance, tempLayer);

            var camGO = new GameObject("IconCamera") { hideFlags = HideFlags.HideAndDontSave };
            camGO.transform.SetParent(root.transform, false);

            var cam = camGO.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0, 0, 0, 0); 
            cam.cullingMask = 1 << tempLayer;
            cam.fieldOfView = FOV;
            cam.nearClipPlane = 0.01f;
            cam.farClipPlane = 1000f;

            var lightGO = new GameObject("IconLight") { hideFlags = HideFlags.HideAndDontSave };
            lightGO.transform.SetParent(root.transform, false);
            var light = lightGO.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.2f;
            light.transform.rotation = Quaternion.Euler(50, -30, 0);

            PositionCameraToBounds(cam, bounds);

            var rt = new RenderTexture(WIDTH, HEIGHT, 24, RenderTextureFormat.ARGB32);
            rt.antiAliasing = 8;
            var prevRT = RenderTexture.active;
            var prevTarget = cam.targetTexture;

            cam.targetTexture = rt;
            cam.Render();

            RenderTexture.active = rt;
            var tex = new Texture2D(WIDTH, HEIGHT, TextureFormat.ARGB32, false, true);
            tex.ReadPixels(new Rect(0, 0, WIDTH, HEIGHT), 0, 0);
            tex.Apply();

            File.WriteAllBytes(savePath, tex.EncodeToPNG());

            cam.targetTexture = prevTarget;
            RenderTexture.active = prevRT;
            rt.Release();
            Object.DestroyImmediate(rt);
            Object.DestroyImmediate(tex);

            RestoreLayers(instance, originalLayers);

            AssetDatabase.Refresh();
            EditorUtility.RevealInFinder(savePath);
        }
        finally
        {
            if (root) Object.DestroyImmediate(root);
        }
    }

    private static Bounds GetCombinedBounds(GameObject go)
    {
        var renderers = go.GetComponentsInChildren<Renderer>(true);
        Bounds b = new Bounds(go.transform.position, Vector3.zero);
        bool has = false;
        foreach (var r in renderers)
        {
            if (!r || !r.enabled) continue;
            if (!has) { b = r.bounds; has = true; }
            else b.Encapsulate(r.bounds);
        }
        return has ? b : new Bounds(Vector3.zero, Vector3.zero);
    }

    private static void PositionCameraToBounds(Camera cam, Bounds b)
    {
        var dir = (Quaternion.Euler(10f, -25f, 0f) * Vector3.forward).normalized;

        float radius = b.extents.magnitude;
        
        float dist = (radius * PADDING) / Mathf.Sin(cam.fieldOfView * 0.5f * Mathf.Deg2Rad);

        var target = b.center;
        cam.transform.position = target - dir * dist;
        cam.transform.LookAt(target, Vector3.up);

        cam.nearClipPlane = Mathf.Max(0.01f, dist - radius * 3f);
        cam.farClipPlane = dist + radius * 3f;
    }

    private static int[] SetLayerRecursive(GameObject go, int layer)
    {
        var trs = go.GetComponentsInChildren<Transform>(true);
        var original = new int[trs.Length];
        for (int i = 0; i < trs.Length; i++)
        {
            original[i] = trs[i].gameObject.layer;
            trs[i].gameObject.layer = layer;
        }
        return original;
    }

    private static void RestoreLayers(GameObject go, int[] original)
    {
        var trs = go.GetComponentsInChildren<Transform>(true);
        for (int i = 0; i < trs.Length && i < original.Length; i++)
            trs[i].gameObject.layer = original[i];
    }
}
#endif
