using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class HUDSpriteImporter : Editor
{
    [MenuItem("Tools/Re-Skin HUD with New PNGs")]
    public static void ApplyHUD()
    {
        // Restore original skybox
        Material skyboxMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/SkySeries Freebie/6SidedFluffball.mat");
        if (skyboxMat != null)
        {
            RenderSettings.skybox = skyboxMat;
            Debug.Log("Original Skybox Restored!");
        }

        string[] paths = {
            "Assets/Sprites/UI/HUD/hud_distance_v2.png",
            "Assets/Sprites/UI/HUD/hud_speed_v2.png",
            "Assets/Sprites/UI/HUD/hud_help_v2.png"
        };

        foreach (var path in paths)
        {
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.spriteBorder = new Vector4(20, 20, 20, 20); // Smaller border
                importer.alphaIsTransparency = true;
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            }
        }

        // Apply to GameObjects
        GameObject scorePanel = GameObject.Find("Canvas/ScorePanel");
        GameObject speedPanel = GameObject.Find("Canvas/SpeedPanel");
        GameObject coinPanel = GameObject.Find("Canvas/CoinDisplayPanel");

        if (scorePanel != null) SetSprite(scorePanel, paths[0]);
        if (speedPanel != null) SetSprite(speedPanel, paths[1]);
        if (coinPanel != null) SetSprite(coinPanel, paths[2]);

        Debug.Log("HUD Skins Applied!");
    }

    private static void SetSprite(GameObject panel, string path)
    {
        Image img = panel.GetComponent<Image>();
        if (img != null)
        {
            Sprite s = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (s != null)
            {
                img.sprite = s;
                img.type = Image.Type.Simple; // Try simple first to avoid slicing issues
                img.color = Color.white; 
            }
            else {
                Debug.LogError("Sprite not found at: " + path);
            }
        }
    }
}
