using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Gazze.Editor
{
    public static class AggressiveMissingScriptCleaner
    {
        [MenuItem("Tools/Gazze/3. ADIM - AGRESİF HATA TEMİZLEYİCİ (ZORUNLU)", priority = 1)]
        public static void ForceCleanAllMissingScripts()
        {
            int totalRemoved = 0;

            // 1. Sahnedeki nesneleri (gizli olanlar dahil) çok daha sert bir yöntemle tara
            GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();

            foreach (GameObject go in allObjects)
            {
                // Asset dosyası (Prefab vb.) değilse ve sahnede ise
                if (go.hideFlags == HideFlags.NotEditable || go.hideFlags == HideFlags.HideAndDontSave)
                    continue;

                // Unity'nin derleyicisi alt seviyede SerializedObject nesnesini okuyarak
                // m_Component arrayindeki kopuk referansları bulur ve fiziksel olarak yokedir.
                SerializedObject so = new SerializedObject(go);
                SerializedProperty prop = so.FindProperty("m_Component");

                if (prop != null)
                {
                    bool isModified = false;
                    for (int i = prop.arraySize - 1; i >= 0; i--)
                    {
                        SerializedProperty compProp = prop.GetArrayElementAtIndex(i);
                        SerializedProperty refProp = compProp.FindPropertyRelative("component");

                        // Eğer referansın ID'si sıfır değil ama kendisi null dönüyorsa (veya tamamen 0 ise) bu bir Missing Script"tir.
                        if (refProp != null && refProp.objectReferenceValue == null && refProp.objectReferenceInstanceIDValue != 0)
                        {
                            prop.DeleteArrayElementAtIndex(i);
                            isModified = true;
                            totalRemoved++;
                        }
                    }

                    if (isModified)
                    {
                        so.ApplyModifiedPropertiesWithoutUndo();
                        EditorUtility.SetDirty(go);
                        Debug.Log($"<color=red>Zorla Silindi:</color> '{go.name}' isimli objenin üzerindeki yok olmuş (Missing) bağlantı söküldü.", go);
                    }
                }
            }

            if (totalRemoved > 0)
            {
                Debug.Log($"<color=green>AGRESİF TEMİZLİK BİTTİ!</color> Toplam {totalRemoved} adet hayalet bağlantı zorla silindi.");
                EditorSceneManager.MarkAllScenesDirty();
                EditorSceneManager.SaveOpenScenes();
                AssetDatabase.SaveAssets();
            }
            else
            {
                Debug.Log("<color=yellow>BİLGİ:</color> Agresif tarayıcı bile bozuk script bulamadı, her şey temiz görünüyor.");
            }
        }
    }
}
