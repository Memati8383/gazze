using UnityEngine;
using UnityEditor;
using System.IO;

public class AchievementAssetSetup : EditorWindow
{
    [MenuItem("Gazze/UI/Setup Achievement Assets")]
    public static void SetupAssets()
    {
        // Klasörleri oluştur
        CreateFolder("Assets", "Resources");
        CreateFolder("Assets/Resources", "UI");
        CreateFolder("Assets/Resources/UI", "Icons");
        CreateFolder("Assets/Resources", "Audio");

        // Taşınacak dosyalar
        MoveAndConfigureSprite("Assets/Yardımsever.png", "Assets/Resources/UI/Icons/Achievement_Yardimsever.png");
        MoveAndConfigureSprite("Assets/Uzun_Yol.png", "Assets/Resources/UI/Icons/Achievement_UzunYol.png");
        MoveAndConfigureSprite("Assets/Kıl_Payı.png", "Assets/Resources/UI/Icons/Achievement_KilPayi.png");
        MoveAndConfigureSprite("Assets/Hız_tutkunu.png", "Assets/Resources/UI/Icons/Achievement_HizTutkunu.png");

        // Ses dosyasını taşı
        string oldAudio = "Assets/başarım_sound.mp3";
        string newAudio = "Assets/Resources/Audio/Achievement_Sound.mp3";
        if (File.Exists(oldAudio) || AssetDatabase.LoadAssetAtPath<AudioClip>(oldAudio) != null)
        {
            string error = AssetDatabase.MoveAsset(oldAudio, newAudio);
            if (string.IsNullOrEmpty(error))
            {
                Debug.Log("Audio moved to: " + newAudio);
            }
            else
            {
                Debug.LogWarning("Could not move audio: " + error);
            }
        }
        else
        {
            Debug.LogWarning("Audio file not found: " + oldAudio);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Achievement assets setup complete.");
    }

    private static void CreateFolder(string parent, string newFolder)
    {
        if (!AssetDatabase.IsValidFolder(parent + "/" + newFolder))
        {
            AssetDatabase.CreateFolder(parent, newFolder);
        }
    }

    private static void MoveAndConfigureSprite(string oldPath, string newPath)
    {
        if (File.Exists(oldPath) || AssetDatabase.LoadAssetAtPath<Texture2D>(oldPath) != null)
        {
            string error = AssetDatabase.MoveAsset(oldPath, newPath);
            if (string.IsNullOrEmpty(error))
            {
                TextureImporter importer = AssetImporter.GetAtPath(newPath) as TextureImporter;
                if (importer != null)
                {
                    importer.textureType = TextureImporterType.Sprite;
                    importer.spriteImportMode = SpriteImportMode.Single;
                    importer.mipmapEnabled = false;
                    importer.alphaIsTransparency = true;
                    importer.SaveAndReimport();
                    Debug.Log("Sprite configured: " + newPath);
                }
            }
            else
            {
                Debug.LogWarning("Could not move sprite: " + oldPath + " Error: " + error);
            }
        }
        else
        {
            Debug.LogWarning("Sprite not found: " + oldPath);
        }
    }

    [MenuItem("Gazze/UI/Setup Ilk Adim Asset")]
    public static void SetupIlkAdim()
    {
        MoveAndConfigureSprite("Assets/ilk adım.png", "Assets/Resources/UI/Icons/Achievement_IlkAdim.png");
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Ilk Adim asset setup complete.");
    }
}
