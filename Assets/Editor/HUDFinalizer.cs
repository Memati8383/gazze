using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HUDFinalizer : Editor
{
    [MenuItem("Tools/Finalize HUD Look")]
    public static void FinalizeHUD()
    {
        string spritePath = "Assets/Sprites/UI/HUD/hud_panel_final.png";
        
        // Import as Sprite first
        TextureImporter importer = AssetImporter.GetAtPath(spritePath) as TextureImporter;
        if (importer != null)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.alphaIsTransparency = true;
            importer.spriteBorder = new Vector4(100, 20, 100, 20); // 4:1 widescreen borders
            AssetDatabase.ImportAsset(spritePath, ImportAssetOptions.ForceUpdate);
        }

        Sprite sharedSprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
        if (sharedSprite == null) {
            Debug.LogError("Final Sprite not found!");
            return;
        }

        ConfigurePanel("Canvas/ScorePanel", sharedSprite, new Color(0.2f, 0.7f, 1f, 1f), new Vector2(160, -60));
        ConfigurePanel("Canvas/CoinDisplayPanel", sharedSprite, new Color(0.3f, 1f, 0.4f, 1f), new Vector2(160, -150));
        ConfigurePanel("Canvas/SpeedPanel", sharedSprite, new Color(1f, 0.6f, 0.1f, 1f), new Vector2(-160, -60));

        // Center Hearts better
        GameObject hearts = GameObject.Find("Canvas/HeartContainer");
        if (hearts != null) {
            RectTransform rt = hearts.GetComponent<RectTransform>();
            rt.anchoredPosition = new Vector2(0, -55);
        }

        Debug.Log("HUD Finalized with 4:1 panels and tint differentiation!");
    }

    private static void ConfigurePanel(string path, Sprite sprite, Color tint, Vector2 pos)
    {
        GameObject panel = GameObject.Find(path);
        if (panel == null) return;

        RectTransform rt = panel.GetComponent<RectTransform>();
        rt.anchoredPosition = pos;
        rt.sizeDelta = new Vector2(280, 75);

        Image img = panel.GetComponent<Image>();
        if (img != null) {
            img.sprite = sprite;
            img.type = Image.Type.Sliced;
            img.color = tint;
        }

        // Fix Text to be white, bold, and centered
        foreach (Transform child in panel.transform) {
            TextMeshProUGUI text = child.GetComponent<TextMeshProUGUI>();
            if (text != null) {
                text.color = Color.white;
                text.fontStyle = FontStyles.Bold;
                text.fontSize = 28;
                text.alignment = TextAlignmentOptions.Center;
                
                RectTransform textRt = text.GetComponent<RectTransform>();
                textRt.anchoredPosition = Vector2.zero;
                textRt.sizeDelta = new Vector2(240, 50);
            }
        }
    }
}
