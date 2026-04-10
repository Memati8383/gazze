using UnityEngine;
using UnityEditor;
using UnityEditor.Build;

public class FixProjectBranding : EditorWindow
{
    [MenuItem("Gazze/Fix Branding (Icon & Splash)")]
    public static void FixBranding()
    {
        string iconPath = "Assets/Sprites/UI/gazze_icon.png";
        string splashPath = "Assets/Sprites/Backgrounds/gazze splash screen.png";

        FixTexture(iconPath, true);
        FixTexture(splashPath, false);

        Debug.Log("<b>Branding Fix:</b> Texture import settings updated (Uncompressed, High Quality).");
        
        // Update Player Settings
        Sprite iconSprite = AssetDatabase.LoadAssetAtPath<Sprite>(iconPath);
        Texture2D splashTex = AssetDatabase.LoadAssetAtPath<Texture2D>(splashPath);

        if (iconSprite != null)
        {
            // Set as Default Icon
            PlayerSettings.SetIcons(NamedBuildTarget.Unknown, new Texture2D[] { iconSprite.texture }, IconKind.Any);
            // Set for Android
            PlayerSettings.SetIcons(NamedBuildTarget.Android, new Texture2D[] { iconSprite.texture }, IconKind.Any);
            Debug.Log("<b>Branding Fix:</b> PlayerSettings Icons updated.");
        }

        AssetDatabase.SaveAssets();
    }

    private static void FixTexture(string path, bool isSprite)
    {
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer != null)
        {
            importer.textureType = isSprite ? TextureImporterType.Sprite : TextureImporterType.Default;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;
            importer.SaveAndReimport();
        }
    }
}
