#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

/// <summary>
/// Sahne atmosferini tek tıkla (objeye ihtiyaç duymadan) ayarlayan editör yardımcısı.
/// Üst menüde "Gaza -> Apply Cinema Atmosphere" kısmından çalıştırılabilir.
/// </summary>
public static class GazaAtmosphereEditor
{
    [MenuItem("Gaza/Apply Cinema Atmosphere")]
    public static void ApplyAtmosphere()
    {
        // 1. Sis Ayarları (Dusty/Bleak Look)
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.ExponentialSquared;
        RenderSettings.fogColor = new Color(0.18f, 0.15f, 0.12f); 
        RenderSettings.fogDensity = 0.015f;

        // 2. Skybox Ayarları (GazaSkybox Shader Kullanarak)
        Shader skyShader = Shader.Find("Custom/GazaSkybox_URP");
        if (skyShader != null)
        {
            Material skyMat = new Material(skyShader);
            skyMat.name = "GazaSkybox_Cinema";
            
            // Parametreleri ayarla
            skyMat.SetColor("_HorizonColor", new Color(0.45f, 0.35f, 0.25f)); 
            skyMat.SetColor("_SkyColor",     new Color(0.15f, 0.15f, 0.18f)); 
            skyMat.SetColor("_GlowColor",    new Color(1.0f, 0.35f, 0.1f));   
            skyMat.SetFloat("_SmokeIntensity", 0.5f);
            
            // Materyali projeye kaydetmek iyi bir fikir (Assets/Materials/GazaSkybox.mat)
            if (!AssetDatabase.IsValidFolder("Assets/Materials"))
                AssetDatabase.CreateFolder("Assets", "Materials");
                
            AssetDatabase.CreateAsset(skyMat, "Assets/Materials/GazaSkybox_Applied.mat");
            RenderSettings.skybox = skyMat;
        }

        // 3. Ambient Lighting
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
        RenderSettings.ambientSkyColor     = new Color(0.2f, 0.2f, 0.25f);
        RenderSettings.ambientEquatorColor = new Color(0.3f, 0.25f, 0.2f); 
        RenderSettings.ambientGroundColor  = new Color(0.1f, 0.08f, 0.06f);

        // Değişiklikleri kaydet ve yansıt
        DynamicGI.UpdateEnvironment();
        EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        
        Debug.Log("[Gaza Atmosphere] Cinema Atmosphere successfully applied to the scene!");
    }
}
#endif
