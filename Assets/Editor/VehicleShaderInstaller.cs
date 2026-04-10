using UnityEngine;
using UnityEditor;
using System.IO;

public class VehicleShaderInstaller : EditorWindow
{
    [MenuItem("Tools/Gazze/Araç Shaderlarını Uygula")]
    public static void ApplyVehicleShaderToAll()
    {
        string folderPath = "Assets/cars vehicle movie";
        string shaderName = "Custom/VehicleShader_URP";
        Shader targetShader = Shader.Find(shaderName);

        if (targetShader == null)
        {
            Debug.LogError($"Shader bulunamadı: {shaderName}");
            return;
        }

        string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { folderPath });
        int count = 0;

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            
            if (prefab == null) continue;

            bool modified = false;
            Renderer[] renderers = prefab.GetComponentsInChildren<Renderer>(true);

            foreach (Renderer r in renderers)
            {
                Material[] sharedMats = r.sharedMaterials;
                for (int i = 0; i < sharedMats.Length; i++)
                {
                    if (sharedMats[i] != null && sharedMats[i].shader != targetShader)
                    {
                        Undo.RecordObject(sharedMats[i], "Change Shader");
                        
                        // Korunacak özellikler
                        Texture mainTex = sharedMats[i].HasProperty("_BaseMap") ? sharedMats[i].GetTexture("_BaseMap") : sharedMats[i].mainTexture;
                        Color mainColor = sharedMats[i].HasProperty("_BaseColor") ? sharedMats[i].GetColor("_BaseColor") : (sharedMats[i].HasProperty("_Color") ? sharedMats[i].color : Color.white);

                        sharedMats[i].shader = targetShader;
                        
                        if (mainTex != null) sharedMats[i].SetTexture("_BaseMap", mainTex);
                        sharedMats[i].SetColor("_BaseColor", mainColor);

                        // Varsayılan Kir/Aşınma Ayarları
                        sharedMats[i].SetFloat("_DirtAmount", 0.35f);
                        sharedMats[i].SetFloat("_WearStrength", 0.5f);
                        sharedMats[i].SetColor("_DirtColor", new Color(0.25f, 0.15f, 0.1f, 1f));
                        
                        // Curved World Senkronizasyonu
                        sharedMats[i].SetFloat("_Curvature", 0.002f);
                        sharedMats[i].SetFloat("_CurvatureH", -0.0015f);
                        sharedMats[i].SetFloat("_HorizonOffset", 10.0f);

                        EditorUtility.SetDirty(sharedMats[i]);
                        modified = true;
                    }
                }
            }

            if (modified)
            {
                count++;
            }
        }

        AssetDatabase.SaveAssets();
        Debug.Log($"Başarıyla {count} araç prefabı/materyali güncellendi.");
        EditorUtility.DisplayDialog("İşlem Tamam", $"{count} araç materyali yeni shader ile güncellendi.", "Tamam");
    }
}
