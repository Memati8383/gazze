using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using Gazze.UI;

namespace Settings
{
    /// <summary>
    /// Ayarlar ekranını Avant-Garde ve modern bir tasarımla yeniden inşa eden sınıftır.
    /// Violet Theme assetlerini kullanarak yüksek kaliteli bir görsel deneyim sunar.
    /// </summary>
    public class SettingsBespokeUI : MonoBehaviour
    {
        private SettingsView view;
        private RectTransform container;

        [Header("Assets - Violet Theme")]
        public Sprite panelBg;
        public Sprite buttonSprite;
        public Sprite sliderBg;
        public Sprite sliderFill;
        public Sprite sliderHandle;

        public void Reconstruct()
        {
            view = GetComponent<SettingsView>();
            if (view == null) return;

            // 1. Ana Paneli Temizle ve Hazırla
            var panelImage = GetComponent<Image>();
            if (panelImage != null)
            {
                panelImage.sprite = panelBg;
                panelImage.type = Image.Type.Sliced;
                panelImage.color = Color.white;
            }

            // Mevcut Container'ı bul
            var containerTransform = transform.Find("Container");
            if (containerTransform != null)
            {
                // Silmek yerine modernize edelim
                ModernizeContainer(containerTransform.GetComponent<RectTransform>());
            }
        }

        private void ModernizeContainer(RectTransform cont)
        {
            // Margin ve Padding ayarları
            var group = cont.GetComponent<VerticalLayoutGroup>();
            if (group != null)
            {
                group.padding = new RectOffset(60, 60, 80, 60);
                group.spacing = 35;
            }

            // Başlık (TITLE)
            var titleText = cont.Find("Text")?.GetComponent<TextMeshProUGUI>();
            if (titleText != null)
            {
                titleText.text = "AYARLAR";
                titleText.fontSize = 54;
                titleText.fontStyle = FontStyles.Bold;
                titleText.alignment = TextAlignmentOptions.Center;
                titleText.color = new Color(1f, 1f, 1f, 0.95f);
                
                var loc = titleText.gameObject.GetComponent<LocalizedText>() ?? titleText.gameObject.AddComponent<LocalizedText>();
                loc.SetKey("Settings_Title");
            }

            // Bölümleri Modernize Et
            SetupSliderGroup(cont.Find("MUSIC VOLUME_Group"), "Settings_Music", "White Music Note");
            SetupSliderGroup(cont.Find("SFX VOLUME_Group"), "Settings_SFX", "White Sound Note"); 
            SetupToggle(cont.Find("MUSIC MUTE"), "Settings_EnableMusic");
            SetupToggle(cont.Find("SFX MUTE"), "Settings_EnableSFX");
            
            // Dil Dropdown'u Ekle (Eğer yoksa)
            if (cont.Find("LANGUAGE_Group") == null)
            {
                CreateLanguageDropdown(cont);
            }

            // Geri Butonu
            var backBtn = cont.Find("BackButton")?.GetComponent<RectTransform>();
            if (backBtn != null)
            {
                var img = backBtn.GetComponent<Image>();
                if (img != null)
                {
                    img.sprite = buttonSprite;
                    img.type = Image.Type.Sliced;
                }
                
                var btnText = backBtn.GetComponentInChildren<TextMeshProUGUI>();
                if (btnText != null)
                {
                    btnText.fontSize = 24;
                    btnText.color = Color.white;
                    var loc = btnText.gameObject.GetComponent<LocalizedText>() ?? btnText.gameObject.AddComponent<LocalizedText>();
                    loc.SetKey("Settings_Back");
                }
            }
        }

        private void SetupSliderGroup(Transform group, string locKey, string iconName)
        {
            if (group == null) return;
            
            var label = group.GetComponentInChildren<TextMeshProUGUI>();
            if (label != null)
            {
                label.fontSize = 18;
                label.color = new Color(1, 1, 1, 0.6f);
                var loc = label.gameObject.GetComponent<LocalizedText>() ?? label.gameObject.AddComponent<LocalizedText>();
                loc.SetKey(locKey);
            }

            var slider = group.GetComponentInChildren<Slider>();
            if (slider != null)
            {
                // Background
                var bg = slider.transform.Find("Background")?.GetComponent<Image>();
                if (bg != null) { bg.sprite = sliderBg; bg.type = Image.Type.Sliced; }
                
                // Fill
                var fill = slider.fillRect?.GetComponent<Image>();
                if (fill != null) { fill.sprite = sliderFill; fill.type = Image.Type.Sliced; }
                
                // Handle
                var handle = slider.handleRect?.GetComponent<Image>();
                if (handle != null) { handle.sprite = sliderHandle; handle.preserveAspect = true; }
            }
        }

        private void SetupToggle(Transform toggle, string locKey)
        {
            if (toggle == null) return;
            
            var label = toggle.GetComponentInChildren<TextMeshProUGUI>();
            if (label != null)
            {
                label.fontSize = 20;
                var loc = label.gameObject.GetComponent<LocalizedText>() ?? label.gameObject.AddComponent<LocalizedText>();
                loc.SetKey(locKey);
            }
            
            // Toggle Background'u Violet Theme yapabiliriz (Switch gibi)
            var bg = toggle.Find("Background")?.GetComponent<Image>();
            if (bg != null)
            {
                bg.color = new Color(1, 1, 1, 0.1f);
            }
        }

        private void SetupDropdown(Transform group, string locKey)
        {
            if (group == null) return;
            
            var label = group.GetComponentInChildren<TextMeshProUGUI>();
            if (label != null)
            {
                label.fontSize = 18;
                label.color = new Color(1, 1, 1, 0.6f);
                var loc = label.gameObject.GetComponent<LocalizedText>() ?? label.gameObject.AddComponent<LocalizedText>();
                loc.SetKey(locKey);
            }
            
            var drp = group.GetComponentInChildren<TMP_Dropdown>();
            if (drp != null)
            {
                var img = drp.GetComponent<Image>();
                if (img != null)
                {
                    img.sprite = buttonSprite;
                    img.type = Image.Type.Sliced;
                    img.color = new Color(1, 1, 1, 0.2f);
                }
            }
        }

        private void CreateLanguageDropdown(Transform container)
        {
            // TODO: Mevcut bir dropdown'u kopyala veya sıfırdan oluştur
            // Bu kısım genelde Editor'de yapılsa daha iyi olur ama kodla da mümkün
        }
    }
}
