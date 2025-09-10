#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.IO;

public static class PrefabIconSaver
{
    [MenuItem("Tools/Icons/Save Icon From Selected Prefab")]
    private static void SaveIconFromSelectedPrefab()
    {
        var obj = Selection.activeObject;
        if (!obj || PrefabUtility.GetPrefabAssetType(obj) == PrefabAssetType.NotAPrefab)
        {
            EditorUtility.DisplayDialog("Save Icon", "Pick Prefab.", "OK");
            return;
        }

        Texture2D tex = AssetPreview.GetAssetPreview(obj);
        for (int i = 0; i < 50 && tex == null; i++)
        {
            System.Threading.Thread.Sleep(50);
            tex = AssetPreview.GetAssetPreview(obj);
        }
        if (!tex)
        {
            EditorUtility.DisplayDialog("Save Icon", "Fail.", "OK");
            return;
        }

        var path = AssetDatabase.GetAssetPath(obj);
        var dir = Path.GetDirectoryName(path);
        var file = Path.GetFileNameWithoutExtension(path) + "_icon.png";
        var savePath = EditorUtility.SaveFilePanel("Save icon PNG", dir, file, "png");
        if (string.IsNullOrEmpty(savePath)) return;

        File.WriteAllBytes(savePath, tex.EncodeToPNG());
        AssetDatabase.Refresh();
        EditorUtility.RevealInFinder(savePath);
    }
}
#endif
