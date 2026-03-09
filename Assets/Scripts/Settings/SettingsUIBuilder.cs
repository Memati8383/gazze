using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

namespace Settings
{
    /// <summary>
    /// Ayarlar ekranını dinamik olarak oluşturan veya mevcut yapıyı modernize eden yardımcı sınıf.
    /// Avant-Garde minimalist tasarım prensiplerine uygun olarak kodlanmıştır.
    /// </summary>
    public class SettingsUIBuilder : MonoBehaviour
    {
        public static void SetupBespokeSettings(SettingsView view)
        {
            if (view == null) return;

            // Arkaplanı modern, koyu bir cam efekti (glassmorphism) ile güncelle
            var bgImage = view.GetComponent<Image>();
            if (bgImage != null)
            {
                bgImage.color = new Color(0.02f, 0.02f, 0.05f, 0.95f);
            }

            // Başlıkları yerelleştirilmiş Text bileşenleriyle güncelle
            AddLocalization(view.musicSlider, "Settings_Music");
            AddLocalization(view.sfxSlider, "Settings_SFX");
            AddLocalization(view.languageDropdown, "Settings_Language");
            AddLocalization(view.backButton, "Settings_Back");
        }

        private static void AddLocalization(Component comp, string key)
        {
            if (comp == null) return;
            
            // Bileşenin üstünde veya yanında bir Label arayalım
            var label = comp.transform.parent.GetComponentInChildren<TextMeshProUGUI>();
            if (label != null)
            {
                var loc = label.gameObject.GetComponent<Gazze.UI.LocalizedText>();
                if (loc == null) loc = label.gameObject.AddComponent<Gazze.UI.LocalizedText>();
                loc.SetKey(key);
            }
        }
    }
}
