using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class CurvedWorldSetup : EditorWindow
{
    // Görseldeki gibi ufka doğru aşağıya eğimli (pozitif) ve sola doğru virajlı (negatif):
    private static float defaultCurvature = 0.002f; 
    private static float defaultCurvatureH = -0.0015f; 
    private static float defaultHorizonOffset = 10.0f;

    [MenuItem("Gazze / Tüm Objeleri Curved Yap (Havada Uçma Çözümü)")]
    public static void ApplyCurvedShaderGlobally()
    {
        Shader curvedShader = Shader.Find("Custom/CurvedWorld_URP");
        if (curvedShader == null)
        {
            Debug.LogError("Custom/CurvedWorld_URP shader bulunamadı!");
            return;
        }

        int changedMatsCount = 0;
        HashSet<Material> matsToChange = new HashSet<Material>();

        // 1. Projedeki tüm ilgili materyalleri bul
        string[] searchFolders = new[] { "Assets/Resources", "Assets/Materials", "Assets/Prefabs", "Assets/Models" };
        List<string> validFolders = new List<string>();
        foreach (string folder in searchFolders)
        {
            if (AssetDatabase.IsValidFolder(folder)) validFolders.Add(folder);
        }

        if (validFolders.Count > 0)
        {
            string[] matGuids = AssetDatabase.FindAssets("t:Material", validFolders.ToArray());
            
            foreach (string guid in matGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
                
                if (mat != null)
                {
                    if (mat.shader.name == "Custom/CurvedWorld_URP") continue;

                    if (mat.shader.name.Contains("Lit") || mat.shader.name.Contains("Standard") || mat.shader.name.Contains("Diffuse"))
                    {
                        matsToChange.Add(mat);
                    }
                }
            }
        }

        // 2. Sahnedeki nesnelere de göz at
        Renderer[] allRenderers = GameObject.FindObjectsByType<Renderer>(FindObjectsSortMode.None);
        foreach (Renderer r in allRenderers)
        {
            if (r.sharedMaterials != null)
            {
                foreach (Material mat in r.sharedMaterials)
                {
                    if (mat != null && mat.shader.name != "Custom/CurvedWorld_URP")
                    {
                        if (mat.shader.name.Contains("Lit") || mat.shader.name.Contains("Standard") || mat.name.Contains("car") || mat.name.Contains("Road"))
                        {
                            matsToChange.Add(mat);
                        }
                    }
                }
            }
        }

        // Değiştir
        foreach (Material mat in matsToChange)
        {
            // Eski özellikleri kopyala
            Texture mainTex = null;
            Texture normalTex = null;
            Texture emissionTex = null;
            Color color = Color.white;
            Color emissionColor = Color.black;
            float smoothness = 0.3f;

            // Albedo
            if (mat.HasProperty("_BaseMap")) mainTex = mat.GetTexture("_BaseMap");
            else if (mat.HasProperty("_MainTex")) mainTex = mat.GetTexture("_MainTex");

            // Color
            if (mat.HasProperty("_BaseColor")) color = mat.GetColor("_BaseColor");
            else if (mat.HasProperty("_Color")) color = mat.GetColor("_Color");

            // Normal Map
            if (mat.HasProperty("_BumpMap")) normalTex = mat.GetTexture("_BumpMap");

            // Emission
            if (mat.HasProperty("_EmissionMap")) emissionTex = mat.GetTexture("_EmissionMap");
            if (mat.HasProperty("_EmissionColor")) emissionColor = mat.GetColor("_EmissionColor");

            // Smoothness
            if (mat.HasProperty("_Smoothness")) smoothness = mat.GetFloat("_Smoothness");
            else if (mat.HasProperty("_Glossiness")) smoothness = mat.GetFloat("_Glossiness");

            // Shader ataması
            Undo.RecordObject(mat, "Curved Shader Apply");
            mat.shader = curvedShader;

            // Özellikleri geri yaz
            if (mainTex != null) mat.SetTexture("_BaseMap", mainTex);
            mat.SetColor("_BaseColor", color);

            // Normal Map varsa aktar
            if (normalTex != null)
            {
                mat.SetTexture("_BumpMap", normalTex);
                mat.SetFloat("_UseNormalMap", 1.0f);
                mat.EnableKeyword("_NORMALMAP");
            }

            // Emission varsa aktar
            if (emissionTex != null || emissionColor != Color.black)
            {
                if (emissionTex != null) mat.SetTexture("_EmissionMap", emissionTex);
                mat.SetColor("_EmissionColor", emissionColor);
                mat.SetFloat("_UseEmission", 1.0f);
                mat.EnableKeyword("_EMISSION");
            }

            // Smoothness
            mat.SetFloat("_Smoothness", smoothness);

            // Curve ayarları
            mat.SetFloat("_Curvature", defaultCurvature);
            mat.SetFloat("_CurvatureH", defaultCurvatureH);
            mat.SetFloat("_HorizonOffset", defaultHorizonOffset);

            EditorUtility.SetDirty(mat);
            changedMatsCount++;
        }

        AssetDatabase.SaveAssets();

        EditorUtility.DisplayDialog("İşlem Tamam!", 
            $"{changedMatsCount} adet materyale Kıvrımlı Dünya Shader'ı v2.0 başarıyla uygulandı!\n\n" +
            "✓ Normal Map, Emission ve Smoothness özellikleri korundu\n" +
            "✓ Tüm objeler aynı eğrilik açısında render edilecek\n" +
            "✓ Havada uçma problemi çözüldü", "Harika!");
    }
}
