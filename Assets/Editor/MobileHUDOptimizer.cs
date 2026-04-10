using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MobileHUDOptimizer : Editor
{
    [MenuItem("Tools/Optimize HUD for Mobile")]
    public static void OptimizeHUD()
    {
        // Mobile Padding - Stay away from extreme corners
        float topPadding = -80f; // Distance from top edge
        float sidePadding = 70f; // Distance from side edges
        Vector2 panelSize = new Vector2(300, 85);

        // Distance Panel (Top Left)
        SetupMobilePanel("Canvas/ScorePanel", new Vector2(sidePadding + panelSize.x/2, topPadding), panelSize, new Color(0, 0, 0, 0.6f));
        
        // Help Panel (Below Distance)
        SetupMobilePanel("Canvas/CoinDisplayPanel", new Vector2(sidePadding + panelSize.x/2, topPadding - panelSize.y - 40), panelSize, new Color(0, 0, 0, 0.6f));
        
        // Speed Panel (Top Right)
        SetupMobilePanel("Canvas/SpeedPanel", new Vector2(-sidePadding - panelSize.x/2, topPadding), panelSize, new Color(0, 0, 0, 0.6f));

        // Hearts (Top Center)
        GameObject hearts = GameObject.Find("Canvas/HeartContainer");
        if (hearts != null) {
            RectTransform rt = hearts.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 1f);
            rt.anchorMax = new Vector2(0.5f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.anchoredPosition = new Vector2(0, -60);
        }

        Debug.Log("HUD Optimized for Mobile: Increased sizes, safety padding, and high-readability text applied.");
    }

    private static void SetupMobilePanel(string path, Vector2 pos, Vector2 size, Color bgColor)
    {
        GameObject panel = GameObject.Find(path);
        if (panel == null) return;

        RectTransform rt = panel.GetComponent<RectTransform>();
        // Ensure correct anchors based on corner
        if (pos.x > 0) { // Left
            rt.anchorMin = new Vector2(0, 1);
            rt.anchorMax = new Vector2(0, 1);
        } else { // Right
            rt.anchorMin = new Vector2(1, 1);
            rt.anchorMax = new Vector2(1, 1);
        }
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;

        Image img = panel.GetComponent<Image>();
        if (img != null) {
            img.sprite = null; // Removing backgrounds as per user request to handle them later
            img.color = bgColor;
        }

        // Fix Text for Mobile
        foreach (Transform child in panel.transform) {
            TextMeshProUGUI text = child.GetComponent<TextMeshProUGUI>();
            if (text != null) {
                text.color = Color.white;
                text.fontStyle = FontStyles.Bold;
                text.fontSize = 32; // Large enough for mobile
                text.alignment = TextAlignmentOptions.Center;
                text.textWrappingMode = TextWrappingModes.NoWrap;
                
                RectTransform textRt = text.GetComponent<RectTransform>();
                textRt.anchorMin = Vector2.zero;
                textRt.anchorMax = Vector2.one;
                textRt.offsetMin = Vector2.zero;
                textRt.offsetMax = Vector2.zero;
            }
        }
    }
}
