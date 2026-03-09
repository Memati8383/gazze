using UnityEngine;
using UnityEditor;

namespace Gazze.Editor.Windows
{
    public class CustomEditorWindow : EditorWindow
    {
        [MenuItem("Gazze Tools/Custom Window")]
        public static void ShowWindow()
        {
            GetWindow<CustomEditorWindow>("Custom Window");
        }

        private void OnGUI()
        {
            GUILayout.Label("Custom Editor Window", EditorStyles.boldLabel);
            if (GUILayout.Button("İşlem Yap"))
            {
                // Debug.Log("Butona basıldı!");
            }
        }
    }
}