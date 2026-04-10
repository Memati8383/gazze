using UnityEngine;
using UnityEditor;

/// <summary>
/// Custom/CurvedWorld_URP shader'ı için özel Material Inspector.
/// Toggle'lar, keyword'ler ve özellikleri düzenli gruplar halinde gösterir.
/// </summary>
public class CurvedWorldShaderGUI : ShaderGUI
{
    private bool showCurveSettings = true;
    private bool showLightingSettings = true;
    private bool showAdvancedSettings = false;

    public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
    {
        Material material = materialEditor.target as Material;

        // ========== TEXTURE & COLOR ==========
        EditorGUILayout.LabelField("🎨 Texture ve Renk", EditorStyles.boldLabel);
        EditorGUI.indentLevel++;
        
        var baseMap = FindProperty("_BaseMap", properties);
        var baseColor = FindProperty("_BaseColor", properties);
        materialEditor.TexturePropertySingleLine(new GUIContent("Albedo"), baseMap, baseColor);
        materialEditor.TextureScaleOffsetProperty(baseMap);

        EditorGUI.indentLevel--;
        EditorGUILayout.Space(8);

        // ========== NORMAL MAP ==========
        var useNormal = FindProperty("_UseNormalMap", properties);
        EditorGUILayout.LabelField("🗺️ Normal Map", EditorStyles.boldLabel);
        EditorGUI.indentLevel++;
        
        materialEditor.ShaderProperty(useNormal, "Normal Map Kullan");
        if (useNormal.floatValue > 0.5f)
        {
            material.EnableKeyword("_NORMALMAP");
            var bumpMap = FindProperty("_BumpMap", properties);
            var bumpScale = FindProperty("_BumpScale", properties);
            materialEditor.TexturePropertySingleLine(new GUIContent("Normal Map", "Yüzey detayları"), bumpMap, bumpScale);
        }
        else
        {
            material.DisableKeyword("_NORMALMAP");
        }

        EditorGUI.indentLevel--;
        EditorGUILayout.Space(8);

        // ========== EMISSION ==========
        var useEmission = FindProperty("_UseEmission", properties);
        EditorGUILayout.LabelField("💡 Emission", EditorStyles.boldLabel);
        EditorGUI.indentLevel++;

        materialEditor.ShaderProperty(useEmission, "Emission Kullan");
        if (useEmission.floatValue > 0.5f)
        {
            material.EnableKeyword("_EMISSION");
            var emissionMap = FindProperty("_EmissionMap", properties);
            var emissionColor = FindProperty("_EmissionColor", properties);
            materialEditor.TexturePropertySingleLine(new GUIContent("Emission Map"), emissionMap, emissionColor);
        }
        else
        {
            material.DisableKeyword("_EMISSION");
        }

        EditorGUI.indentLevel--;
        EditorGUILayout.Space(8);

        // ========== SPECULAR ==========
        showLightingSettings = EditorGUILayout.Foldout(showLightingSettings, "✨ Specular Ayarları", true);
        if (showLightingSettings)
        {
            EditorGUI.indentLevel++;
            materialEditor.ShaderProperty(FindProperty("_Smoothness", properties), "Smoothness");
            materialEditor.ShaderProperty(FindProperty("_SpecColor", properties), "Specular Color");
            EditorGUI.indentLevel--;
        }
        EditorGUILayout.Space(8);

        // ========== CURVED WORLD ==========
        showCurveSettings = EditorGUILayout.Foldout(showCurveSettings, "🌍 Curved World Ayarları", true);
        if (showCurveSettings)
        {
            EditorGUI.indentLevel++;

            var curvature = FindProperty("_Curvature", properties);
            var curvatureH = FindProperty("_CurvatureH", properties);
            var horizonOffset = FindProperty("_HorizonOffset", properties);

            materialEditor.ShaderProperty(curvature, new GUIContent("Dikey Eğrilik", "Yolun uzağa doğru ne kadar eğileceği (Y ekseni)"));
            materialEditor.ShaderProperty(curvatureH, new GUIContent("Yatay Eğrilik", "Yanlara doğru eğrilme (viraj hissi)"));
            materialEditor.ShaderProperty(horizonOffset, new GUIContent("Ufuk Ofseti", "Eğrilmenin ne kadar uzakta başlayacağı"));

            // Bilgi kutusu
            EditorGUILayout.HelpBox(
                "Dikey Eğrilik: 0.001 = Yumuşak eğri, 0.01 = Güçlü eğri\n" +
                "Yatay Eğrilik: 0 = Düz, 0.001+ = Viraj efekti\n" +
                "Ufuk Ofseti: Eğrilmenin kameradan ne kadar ileride başlayacağı",
                MessageType.Info);

            EditorGUI.indentLevel--;
        }
        EditorGUILayout.Space(8);

        // ========== DISTANCE FADE ==========
        var useFade = FindProperty("_UseDistanceFade", properties);
        EditorGUILayout.LabelField("🌫️ Mesafe Fading", EditorStyles.boldLabel);
        EditorGUI.indentLevel++;

        materialEditor.ShaderProperty(useFade, "Mesafe Fade Kullan");
        if (useFade.floatValue > 0.5f)
        {
            material.EnableKeyword("_DISTANCE_FADE");
            materialEditor.ShaderProperty(FindProperty("_FadeStart", properties), "Başlangıç Mesafesi");
            materialEditor.ShaderProperty(FindProperty("_FadeEnd", properties), "Bitiş Mesafesi");
        }
        else
        {
            material.DisableKeyword("_DISTANCE_FADE");
        }

        EditorGUI.indentLevel--;
        EditorGUILayout.Space(8);

        // ========== ADVANCED ==========
        showAdvancedSettings = EditorGUILayout.Foldout(showAdvancedSettings, "⚙️ Gelişmiş Render Ayarları", true);
        if (showAdvancedSettings)
        {
            EditorGUI.indentLevel++;

            var alphaClip = FindProperty("_AlphaClip", properties);
            materialEditor.ShaderProperty(alphaClip, "Alpha Clipping");
            if (alphaClip.floatValue > 0.5f)
            {
                material.EnableKeyword("_ALPHATEST_ON");
                materialEditor.ShaderProperty(FindProperty("_Cutoff", properties), "Cutoff Eşiği");
            }
            else
            {
                material.DisableKeyword("_ALPHATEST_ON");
            }

            materialEditor.ShaderProperty(FindProperty("_Cull", properties), "Cull Mode");

            EditorGUI.indentLevel--;
        }

        EditorGUILayout.Space(12);

        // ========== INFO FOOTER ==========
        EditorGUILayout.HelpBox(
            "Curved World URP Shader v2.0\n" +
            "• Normal Map, Emission, Specular desteği\n" +
            "• Dikey + Yatay eğrilik (viraj efekti)\n" +
            "• Fog, Shadow, Distance Fade entegrasyonu\n" +
            "• GPU Instancing & SRP Batcher uyumlu",
            MessageType.None);

        // GPU Instancing ve SRP Batcher bilgisi
        materialEditor.EnableInstancingField();
        materialEditor.RenderQueueField();
    }
}
