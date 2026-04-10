#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using Gazze.UI;

namespace Gazze.Editor
{
    public class MainMenuOverhaulTools
    {
        [MenuItem("Gazze/UI/Rebuild Main Menu V6 (Dynamic)")]
        public static void RebuildMainMenu()
        {
            var overhaul = Object.FindFirstObjectByType<MainOptionsVisualOverhaul>();
            if (overhaul != null)
            {
                AssetDatabase.Refresh();
                
                // 1. Texture Finding
                string[] guids = AssetDatabase.FindAssets("MainMenuBackground");
                Texture2D bgTex = null;
                foreach (string guid in guids)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guid);
                    if (path.Contains("UI") && (path.EndsWith(".png") || path.EndsWith(".jpg")))
                    {
                        bgTex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                        break;
                    }
                }
                if (bgTex != null) overhaul.backgroundTexture = bgTex;

                // 2. Material Finding
                string matPath = "Assets/Materials/UI/DynamicBackgroundMat.mat";
                var mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
                if (mat != null) overhaul.dynamicBgMaterial = mat;

                // 3. Rebuild
                overhaul.BuildMainOptions();
                
                // 4. Set tipText reference
                var tipsDisplay = overhaul.GetComponentInChildren<MenuTipsDisplay>();
                var tipText = overhaul.transform.Find("TipsArea/TipText")?.GetComponent<TMPro.TextMeshProUGUI>();
                if (tipsDisplay != null && tipText != null)
                {
                    tipsDisplay.tipText = tipText;
                    EditorUtility.SetDirty(tipsDisplay);
                }

                EditorUtility.SetDirty(overhaul);
                Debug.Log("Main Menu Rebuilt with V6 Dynamic Aesthetics!");
            }
            else
            {
                Debug.LogError("MainOptionsVisualOverhaul component not found in scene!");
            }
        }

        [MenuItem("Gazze/UI/Rebuild Upgrade Panel")]
        public static void RebuildUpgradePanel()
        {
            var builder = Object.FindFirstObjectByType<UpgradePanelBuilder>();
            if (builder != null)
            {
                builder.BuildUpgradePanel();
                EditorUtility.SetDirty(builder);
                Debug.Log("Upgrade Panel Rebuilt!");
            }
            else
            {
                Debug.LogError("UpgradePanelBuilder not found in scene!");
            }
        }
    }
}
#endif
