using UnityEditor;
using UnityEngine;

namespace Gazze.Editor
{
    public class MissingScriptCleaner
    {
        [MenuItem("Tools/Gazze/Clean Missing Scripts In Scene")]
        public static void CleanMissingScripts()
        {
            var gameObjects = Resources.FindObjectsOfTypeAll<GameObject>();
            int missingCount = 0;

            foreach (var go in gameObjects)
            {
                // Sadece sahnedeki objeleri etkile (Prefab editörde açık olanlar vs hariç tutmak isterseniz)
                if (go.scene.isLoaded)
                {
                    int removedCount = GameObjectUtility.RemoveMonoBehavioursWithMissingScript(go);
                    if (removedCount > 0)
                    {
                        missingCount += removedCount;
                        Debug.Log($"<color=yellow>Temizlendi:</color> '{go.name}' objesindeki bozuk script kaldırıldı.", go);
                        EditorUtility.SetDirty(go);
                    }
                }
            }

            if (missingCount > 0)
            {
                Debug.Log($"<color=green>Gazze:</color> Sahnede toplam {missingCount} adet 'Missing Script' temizlendi! Ctr+S (Save) yapmayı unutmayın.");
                UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty();
            }
            else
            {
                Debug.Log("<color=green>Gazze:</color> Sahnede Missing Script bulunamadı. Her şey temiz!");
            }
        }
    }
}
