#if UNITY_EDITOR
using UnityEditor;

namespace Dreamteck.Forever
{
    [CustomEditor(typeof(CustomPathGenerator))]
    public class CustomPathGeneratorInspector : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
        }
    }
}
#endif
