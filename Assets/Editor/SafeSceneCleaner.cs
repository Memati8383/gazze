using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Gazze.Editor
{
    public static class SafeSceneCleaner
    {
        [MenuItem("Tools/Gazze/4. ADIM - KESİN ÇÖZÜM TEMİZLEYİCİ", priority = 2)]
        public static void CleanExtremelySafe()
        {
            int totalRemoved = 0;
            Scene scene = EditorSceneManager.GetActiveScene();
            GameObject[] rootObjects = scene.GetRootGameObjects();

            foreach (var root in rootObjects)
            {
                Transform[] allTransforms = root.GetComponentsInChildren<Transform>(true);
                foreach (var t in allTransforms)
                {
                    GameObject go = t.gameObject;
                    
                    // Eğer prefab instance içindeyse, unity'nin modifikasyon engeline (IllegalModificationError) takılmamak için prefab bağını koparıyoruz.
                    if (PrefabUtility.IsPartOfAnyPrefab(go))
                    {
                        var rootObj = PrefabUtility.GetOutermostPrefabInstanceRoot(go);
                        if (rootObj != null)
                        {
                            PrefabUtility.UnpackPrefabInstance(rootObj, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
                        }
                    }

                    int count = GameObjectUtility.RemoveMonoBehavioursWithMissingScript(go);
                    if (count > 0)
                    {
                        totalRemoved += count;
                        Debug.Log($"<color=cyan>Kalıcı Olarak Silindi:</color> '{go.name}' üzerindeki {count} bozuk bağlantı uçuruldu.", go);
                        EditorUtility.SetDirty(go);
                    }
                }
            }

            if (totalRemoved > 0)
            {
                Debug.Log($"<color=green>TÜM İŞLEMLER BAŞARILI!</color> Toplam {totalRemoved} adet saklanan (Prefablara kilitlenmiş) eksik bağlantı kökünden silindi.");
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
            }
            else
            {
                Debug.Log("<color=yellow>BİLGİ:</color> Sahnedeki Prefablarda veya objelerde hiç eksik referans kalmadı. Tamamen temizsiniz.");
            }
        }
    }
}
