using UnityEditor;
using UnityEngine;
using Gazze.UI;
using Settings;

namespace Gazze.Editor
{
    public static class GazaUITools
    {
        [MenuItem("Gazze Tools/Rebuild All UI Panels", false, 0)]
        public static void RebuildAll()
        {
            RebuildMainMenu();
            RebuildSettings();
            RebuildUpgradePanel();
            Debug.Log("<color=orange>Gazze:</color> Tüm UI panelleri yeniden oluşturuldu!");
        }

        [MenuItem("Gazze Tools/UI/Rebuild Main Menu", false, 11)]
        public static void RebuildMainMenu()
        {
            var menu = Object.FindFirstObjectByType<MainOptionsVisualOverhaul>();
            if (menu != null)
            {
                menu.BuildMainOptions();
                EditorUtility.SetDirty(menu);
                Debug.Log("Ana Menü yeniden oluşturuldu.");
            }
            else
            {
                Debug.LogWarning("Sahnede MainOptionsVisualOverhaul bulunamadı.");
            }
        }

        [MenuItem("Gazze Tools/UI/Rebuild Settings Panel", false, 12)]
        public static void RebuildSettings()
        {
            var settings = Object.FindFirstObjectByType<SettingsVisualOverhaul>();
            if (settings != null)
            {
                settings.BuildSettingsPanel();
                EditorUtility.SetDirty(settings);
                Debug.Log("Ayarlar Paneli yeniden oluşturuldu.");
            }
            else
            {
                Debug.LogWarning("Sahnede SettingsVisualOverhaul bulunamadı.");
            }
        }

        [MenuItem("Gazze Tools/UI/Rebuild Upgrade Panel", false, 13)]
        public static void RebuildUpgradePanel()
        {
            var upgrade = Object.FindFirstObjectByType<UpgradePanelBuilder>();
            if (upgrade != null)
            {
                upgrade.BuildUpgradePanel();
                EditorUtility.SetDirty(upgrade);
                Debug.Log("Yükseltme Paneli yeniden oluşturuldu.");
            }
            else
            {
                Debug.LogWarning("Sahnede UpgradePanelBuilder bulunamadı.");
            }
        }
    }
}
