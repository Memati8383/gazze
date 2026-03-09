using UnityEditor;
using UnityEngine;
using System.Linq;

namespace Gazze.Editor
{
    public class FullProjectMissingScriptCleaner
    {
        [MenuItem("Tools/Gazze/Deep Clean ALL Missing Scripts")]
        public static void CleanAll()
        {
            int totalCleaned = 0;

            // 1. Scene içindeki objeleri temizle
            var gameObjects = Resources.FindObjectsOfTypeAll<GameObject>();
            foreach (var go in gameObjects)
            {
                if (go.scene.isLoaded)
                {
                    int count = GameObjectUtility.RemoveMonoBehavioursWithMissingScript(go);
                    if (count > 0)
                    {
                        totalCleaned += count;
                        Debug.Log($"<color=yellow>Sahne Temizlendi:</color> '{go.name}' ({count} script)", go);
                        EditorUtility.SetDirty(go);
                    }
                }
            }

            // 2. Projedeki tüm prefabları (Assets klasöründeki) bul ve temizle
            string[] allPrefabPaths = AssetDatabase.GetAllAssetPaths().Where(p => p.EndsWith(".prefab")).ToArray();
            foreach (string path in allPrefabPaths)
            {
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab != null)
                {
                    int count = GameObjectUtility.RemoveMonoBehavioursWithMissingScript(prefab);
                    if (count > 0)
                    {
                        totalCleaned += count;
                        Debug.Log($"<color=orange>Prefab Temizlendi:</color> '{prefab.name}' at path {path} ({count} script)", prefab);
                        PrefabUtility.SavePrefabAsset(prefab);
                    }
                }
            }
            
            // 3. ScriptableObjects içindeki bozuklukları temizlemek için (isteğe bağlı, ama genelde prefab ve sahnede olur)

            if (totalCleaned > 0)
            {
                Debug.Log($"<color=green>Gazze DEEP CLEAN Bitti!</color> Tüm projeden {totalCleaned} bozuk script bağlantısı tamamen silindi.");
                UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty();
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
            else
            {
                Debug.Log("<color=green>Gazze DEEP CLEAN Bitti!</color> Hiçbir bozuk bağlantı (Missing Script) bulunamadı. Proje zaten temiz.");
            }
        }
    }
}
