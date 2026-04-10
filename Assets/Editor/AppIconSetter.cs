using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Android;
using UnityEngine;
using System.IO;
using System.Collections.Generic;

namespace Gazze.Editor
{
    public class AppIconSetter : EditorWindow
    {
        [MenuItem("Gaza/Tools/Set App Icons")]
    public static void SetIcons()
    {
        string basePath = "Assets/Resources/app icon/res";
        string[] densities = { "mdpi", "hdpi", "xhdpi", "xxhdpi", "xxxhdpi" };

        // Legacy Icons
        SetLegacyIcons(basePath, densities);

        // Adaptive Icons
        SetAdaptiveIcons(basePath, densities);

        Debug.Log("App icons updated successfully.");
    }

    private static void SetLegacyIcons(string basePath, string[] densities)
    {
        PlatformIconKind kind = AndroidPlatformIconKind.Adaptive; // Unity suggests using Adaptive kind for modern deployments
        PlatformIcon[] icons = PlayerSettings.GetPlatformIcons(NamedBuildTarget.Android, kind);

        foreach (var icon in icons)
        {
            string densityName = GetDensityFromIcon(icon);
            if (string.IsNullOrEmpty(densityName)) continue;

            string texturePath = $"{basePath}/mipmap-{densityName}/ic_launcher.png";
            PrepareTextureForIcon(texturePath);

            Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);
            if (tex != null)
            {
                icon.SetTexture(tex);
            }
        }
        PlayerSettings.SetPlatformIcons(NamedBuildTarget.Android, kind, icons);
    }

    private static void SetAdaptiveIcons(string basePath, string[] densities)
    {
        PlatformIconKind kind = AndroidPlatformIconKind.Adaptive;
        PlatformIcon[] icons = PlayerSettings.GetPlatformIcons(NamedBuildTarget.Android, kind);

        foreach (var icon in icons)
        {
            string densityName = GetDensityFromIcon(icon);
            if (string.IsNullOrEmpty(densityName)) continue;

            string backPath = $"{basePath}/mipmap-{densityName}/ic_launcher_adaptive_back.png";
            string forePath = $"{basePath}/mipmap-{densityName}/ic_launcher_adaptive_fore.png";

            PrepareTextureForIcon(backPath);
            PrepareTextureForIcon(forePath);

            Texture2D backTex = AssetDatabase.LoadAssetAtPath<Texture2D>(backPath);
            Texture2D foreTex = AssetDatabase.LoadAssetAtPath<Texture2D>(forePath);

            if (backTex != null || foreTex != null)
            {
                // For adaptive icons, we set two layers
                Texture2D[] layers = new Texture2D[2];
                layers[0] = backTex;
                layers[1] = foreTex;
                icon.SetTextures(layers);
            }
        }
        PlayerSettings.SetPlatformIcons(NamedBuildTarget.Android, kind, icons);
    }

    private static void PrepareTextureForIcon(string path)
    {
        if (string.IsNullOrEmpty(path)) return;
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer == null) return;

        bool changed = false;
        if (importer.textureCompression != TextureImporterCompression.Uncompressed)
        {
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            changed = true;
        }

        if (importer.mipmapEnabled)
        {
            importer.mipmapEnabled = false;
            changed = true;
        }

        if (importer.alphaIsTransparency == false)
        {
            importer.alphaIsTransparency = true;
            changed = true;
        }

        if (changed)
        {
            EditorUtility.SetDirty(importer);
            importer.SaveAndReimport();
        }
    }

    private static string GetDensityFromIcon(PlatformIcon icon)
    {
        // Map Unity's width to density names
        // mdpi: 48, hdpi: 72, xhdpi: 96, xxhdpi: 144, xxxhdpi: 192
        switch (icon.width)
        {
            case 48: return "mdpi";
            case 72: return "hdpi";
            case 96: return "xhdpi";
            case 144: return "xxhdpi";
            case 192: return "xxxhdpi";
            default: return null;
        }
    }
    }
}
