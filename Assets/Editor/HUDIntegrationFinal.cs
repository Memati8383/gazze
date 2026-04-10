using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HUDIntegrationFinal : Editor
{
    [MenuItem("Tools/Integrate New HUD Backgrounds")]
    public static void IntegrateHUD()
    {
        string[] paths = {
            "Assets/Sprites/UI/HUD/hud_distance_bg.png",
            "Assets/Sprites/UI/HUD/hud_speed_bg.png",
            "Assets/Sprites/UI/HUD/hud_help_bg.png"
        };

        foreach (var path in paths)
        {
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.alphaIsTransparency = true;
                importer.spriteBorder = new Vector4(80, 20, 80, 20); 
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            }
        }

        ConfigurePanel("ScorePanel", paths[0], new Vector2(170, -80), new Vector2(300, 85));
        ConfigurePanel("CoinDisplayPanel", paths[2], new Vector2(170, -180), new Vector2(300, 85));
        ConfigurePanel("SpeedPanel", paths[1], new Vector2(-200, -80), new Vector2(360, 85));

        Debug.Log("HUD Beautified: Enhanced text styling with character spacing and outlines.");
    }

    private static void ConfigurePanel(string name, string assetPath, Vector2 pos, Vector2 size)
    {
        GameObject panelObj = GameObject.Find(name);
        if (panelObj == null) return;

        RectTransform rt = panelObj.GetComponent<RectTransform>();
        if (pos.x < 0) {
            rt.anchorMin = new Vector2(1, 1);
            rt.anchorMax = new Vector2(1, 1);
        } else {
            rt.anchorMin = new Vector2(0, 1);
            rt.anchorMax = new Vector2(0, 1);
        }
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;

        Image img = panelObj.GetComponent<Image>();
        if (img != null) {
            img.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
            img.type = Image.Type.Sliced;
            img.color = Color.white;
        }

        foreach (Transform child in panelObj.transform) {
            TextMeshProUGUI text = child.GetComponent<TextMeshProUGUI>();
            if (text != null) {
                text.color = Color.white;
                text.fontStyle = FontStyles.Bold;
                text.fontSize = 30; // Slightly larger
                text.alignment = TextAlignmentOptions.Center;
                text.textWrappingMode = TextWrappingModes.NoWrap;
                text.characterSpacing = 4f; // More elite/tech feel
                text.margin = Vector4.zero;
                
                // Add soft outline/glow effect via TMP properties
                text.outlineColor = new Color(0, 0, 0, 0.7f); // Dark semi-transparent outline
                text.outlineWidth = 0.15f; // Substantial enough to read against anything
                
                // Shadow effect
                text.gameObject.name = child.name; // Keep name
                
                RectTransform textRt = text.GetComponent<RectTransform>();
                textRt.anchorMin = Vector2.zero;
                textRt.anchorMax = Vector2.one;
                textRt.pivot = new Vector2(0.5f, 0.5f);
                textRt.offsetMin = new Vector2(40, 0); 
                textRt.offsetMax = new Vector2(-40, 0);
            }
        }
    }
}
