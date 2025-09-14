#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public static class FindBrokenImages
{
    [MenuItem("Tools/UI/Find Broken UI Images")]
    static void Run()
    {
        int bad = 0, ok = 0;
        foreach (var img in Object.FindObjectsByType<Image>(FindObjectsSortMode.None))
        {
            try { _ = new SerializedObject(img); ok++; }
            catch { bad++; Debug.LogError("Broken Image: " + GetPath(img.gameObject), img.gameObject); }
        }
        Debug.Log($"Checked Images: OK={ok}, BROKEN={bad}");
    }

    static string GetPath(GameObject go)
    {
        string p = go.name; var t = go.transform;
        while (t.parent) { t = t.parent; p = t.name + "/" + p; }
        return p;
    }
}
#endif
