using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Gazze.Editor
{
    [InitializeOnLoad]
    public static class AutoSceneCleaner
    {
        static AutoSceneCleaner()
        {
            // Unity kodları derledikten hemen sonra bu metodu otomatik çalıştıracak.
            EditorApplication.delayCall += AutoCleanMainMenu;
        }

        [MenuItem("Tools/Gazze/Force Clean MainMenu Now", priority = 0)]
        public static void AutoCleanMainMenu()
        {
            // Sadece bir kere çalışmasını garantilemek için SessionState kullanıyoruz.
            if (SessionState.GetBool("AutoCleanMainMenuDone", false)) return;
            SessionState.SetBool("AutoCleanMainMenuDone", true);

            string scenePath = "Assets/Scenes/MainMenu.unity";
            var sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath);
            
            if (sceneAsset != null)
            {
                // Mevcut sahneyi kaydettir
                if (EditorSceneManager.GetActiveScene().isDirty)
                {
                    EditorSceneManager.SaveOpenScenes();
                }

                // MainMenu sahnesini aç
                Scene s = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
                int totalMissingScriptsRemoved = 0;
                
                var roots = s.GetRootGameObjects();
                foreach (var root in roots)
                {
                    var transforms = root.GetComponentsInChildren<Transform>(true);
                    foreach (var t in transforms)
                    {
                        // Exclude the missing script itself
                        int count = GameObjectUtility.RemoveMonoBehavioursWithMissingScript(t.gameObject);
                        if (count > 0)
                        {
                            totalMissingScriptsRemoved += count;
                            Debug.Log($"<color=yellow>Silindi:</color> {t.gameObject.name} objesindeki {count} bozuk referans silindi!");
                        }
                    }
                }

                if (totalMissingScriptsRemoved > 0)
                {
                    EditorSceneManager.SaveScene(s);
                    Debug.Log($"<color=green>OTOMATİK TEMİZLİK TAMAMLANDI!</color> MainMenu sahnesindeki toplam {totalMissingScriptsRemoved} adet 'Missing Script' kalıcı olarak silindi ve sahne kaydedildi.");
                }
                else
                {
                    Debug.Log("<color=green>OTOMATİK KONTROL:</color> MainMenu sahnesinde bozuk betik bulunmadı.");
                }
            }
        }
    }
}
