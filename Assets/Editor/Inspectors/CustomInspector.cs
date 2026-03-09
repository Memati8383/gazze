using UnityEngine;
using UnityEditor;

namespace Gazze.Editor.Inspectors
{
    // [CustomEditor(typeof(HedefClass))]
    public class CustomInspector : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            EditorGUILayout.HelpBox("Bu özel bir inspector görünümüdür.", MessageType.Info);
        }
    }
}