using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

namespace Settings
{
    public class SettingsVisualOverhaul : MonoBehaviour
    {
        [Header("Premium UI Colors")]
        public Color backgroundColor = new Color32(10, 10, 15, 240); // Deep dark blue-black
        public Color accentColor = new Color32(100, 149, 237, 255); // Cornflower Blue
        public Color rowColor = new Color32(25, 25, 35, 255);
        public Color textColor = new Color32(245, 245, 255, 255);

        [ContextMenu("AYARLAR PANELİNİ SIFIRDAN KUR")]
        public void BuildSettingsPanel()
        {
            // 1. Temizlik
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                if (Application.isPlaying) Destroy(transform.GetChild(i).gameObject);
                else DestroyImmediate(transform.GetChild(i).gameObject);
            }

            // 2. Ana Panel Ayarları
            var rect = GetComponent<RectTransform>() ?? gameObject.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(550, 700);
            
            var mainImg = gameObject.GetComponent<Image>() ?? gameObject.AddComponent<Image>();
            mainImg.color = backgroundColor;
            mainImg.type = Image.Type.Sliced; // Background sprite usually supports slicing

            var vlg = gameObject.GetComponent<VerticalLayoutGroup>() ?? gameObject.AddComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(45, 45, 50, 45);
            vlg.spacing = 30;
            vlg.childAlignment = TextAnchor.UpperCenter;
            vlg.childControlHeight = true;
            vlg.childControlWidth = true;
            vlg.childForceExpandHeight = false;

            // 3. Başlık: AYARLAR
            CreateTextElement("AYARLAR", 36, FontStyles.Bold | FontStyles.UpperCase, TextAlignmentOptions.Center);
            
            // Ayırıcı çizgi
            var line = new GameObject("Separator", typeof(RectTransform), typeof(Image));
            line.transform.SetParent(transform, false);
            line.GetComponent<Image>().color = new Color(1, 1, 1, 0.15f);
            line.GetComponent<LayoutElement>().preferredHeight = 2;

            // 4. Müzik Satırı
            var musicRow = CreateHorizontalRow("MusicRow", 60);
            CreateLabel(musicRow.transform, "MÜZİK", 100);
            var musicSlider = CreateSlider(musicRow.transform);
            var musicToggle = CreateToggle(musicRow.transform);

            // 5. SFX Satırı
            var sfxRow = CreateHorizontalRow("SFXRow", 60);
            CreateLabel(sfxRow.transform, "SFX", 100);
            var sfxSlider = CreateSlider(sfxRow.transform);
            var sfxToggle = CreateToggle(sfxRow.transform);

            // 6. Dil Seçimi
            var langRow = CreateHorizontalRow("LanguageRow", 60);
            CreateLabel(langRow.transform, "DİL", 100);
            var langDropdown = CreateDropdown(langRow.transform);

            // 7. Haptic Satırı
            var hapticRow = CreateHorizontalRow("HapticRow", 60);
            CreateLabel(hapticRow.transform, "TİTREŞİM", 100);
            var hapticToggle = CreateToggle(hapticRow.transform);

            // Boşluk
            var spacer = new GameObject("Spacer", typeof(RectTransform));
            spacer.transform.SetParent(transform, false);
            spacer.AddComponent<LayoutElement>().flexibleHeight = 1;

            // 7. Butonlar
            var buttonRow = CreateHorizontalRow("ButtonRow", 70);
            var resetBtn = CreateButton(buttonRow.transform, "SIFIRLA", new Color32(180, 50, 50, 255), 180);
            var backBtn = CreateButton(buttonRow.transform, "GERİ", accentColor, 180);

            // 8. Referansları Otomatik Bağla
            SetupReferences(musicSlider, sfxSlider, musicToggle, sfxToggle, langDropdown, hapticToggle, resetBtn, backBtn);

            Debug.Log("<color=cyan>Gazze:</color> Premium Ayarlar Paneli oluşturuldu!");
        }

        #region Yardımcı Metotlar (UI Oluşturma)

        private GameObject CreateHorizontalRow(string name, float height)
        {
            var row = new GameObject(name, typeof(RectTransform));
            row.transform.SetParent(transform, false);
            var le = row.AddComponent<LayoutElement>();
            le.preferredHeight = height;
            row.AddComponent<Image>().color = rowColor;

            var hlg = row.AddComponent<HorizontalLayoutGroup>();
            hlg.padding = new RectOffset(15, 15, 0, 0);
            hlg.spacing = 20;
            hlg.childAlignment = TextAnchor.MiddleLeft;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = false;
            return row;
        }

        private void CreateLabel(Transform parent, string text, float width)
        {
            var go = new GameObject("Label", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var t = go.AddComponent<TextMeshProUGUI>();
            t.text = text;
            t.fontSize = 20;
            t.fontStyle = FontStyles.Bold;
            t.color = textColor;
            go.AddComponent<LayoutElement>().preferredWidth = width;
        }


        private Slider CreateSlider(Transform parent)
        {
            var go = new GameObject("Slider", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var sl = go.AddComponent<Slider>();
            go.AddComponent<LayoutElement>().flexibleWidth = 1;
            go.AddComponent<LayoutElement>().preferredHeight = 30;

            // Background
            var bg = new GameObject("Background", typeof(RectTransform), typeof(Image));
            bg.transform.SetParent(go.transform, false);
            var bgRect = bg.GetComponent<RectTransform>();
            bgRect.anchorMin = new Vector2(0, 0.25f);
            bgRect.anchorMax = new Vector2(1, 0.75f);
            bgRect.sizeDelta = Vector2.zero;
            bg.GetComponent<Image>().color = new Color(1, 1, 1, 0.1f);
            
            // Fill Area
            var fillArea = new GameObject("Fill Area", typeof(RectTransform));
            fillArea.transform.SetParent(go.transform, false);
            var faRect = fillArea.GetComponent<RectTransform>();
            faRect.anchorMin = new Vector2(0, 0.25f);
            faRect.anchorMax = new Vector2(1, 0.75f);
            faRect.sizeDelta = Vector2.zero;

            var fill = new GameObject("Fill", typeof(RectTransform), typeof(Image));
            fill.transform.SetParent(fillArea.transform, false);
            fill.GetComponent<Image>().color = accentColor;
            sl.fillRect = fill.GetComponent<RectTransform>();
            sl.fillRect.sizeDelta = Vector2.zero;

            sl.value = 0.8f;
            return sl;
        }

        private Toggle CreateToggle(Transform parent)
        {
            var go = new GameObject("Toggle", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var tg = go.AddComponent<Toggle>();
            go.AddComponent<LayoutElement>().preferredWidth = 50;
            go.AddComponent<LayoutElement>().preferredHeight = 30;

            var bg = new GameObject("Background", typeof(RectTransform), typeof(Image));
            bg.transform.SetParent(go.transform, false);
            var bgRect = bg.GetComponent<RectTransform>();
            bgRect.sizeDelta = new Vector2(50, 25);
            bg.GetComponent<Image>().color = new Color(1, 1, 1, 0.1f);

            var check = new GameObject("Checkmark", typeof(RectTransform), typeof(Image));
            check.transform.SetParent(bg.transform, false);
            var ckRect = check.GetComponent<RectTransform>();
            ckRect.sizeDelta = new Vector2(20, 20);
            check.GetComponent<Image>().color = accentColor;
            tg.graphic = check.GetComponent<Image>();
            
            tg.isOn = true;
            return tg;
        }

        private TMP_Dropdown CreateDropdown(Transform parent)
        {
            var go = new GameObject("Dropdown", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var dd = go.AddComponent<TMP_Dropdown>();
            go.AddComponent<LayoutElement>().flexibleWidth = 1;

            var label = new GameObject("Label", typeof(RectTransform));
            label.transform.SetParent(go.transform, false);
            var t = label.AddComponent<TextMeshProUGUI>();
            t.fontSize = 20;
            t.alignment = TextAlignmentOptions.Center;
            dd.captionText = t;

            // Örnek diller
            dd.options.Add(new TMP_Dropdown.OptionData("Türkçe"));
            dd.options.Add(new TMP_Dropdown.OptionData("English"));
            
            return dd;
        }

        private Button CreateButton(Transform parent, string label, Color col, float width)
        {
            var go = new GameObject(label + "_Btn", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            go.AddComponent<Image>().color = col;
            go.AddComponent<LayoutElement>().preferredHeight = 50;
            go.AddComponent<LayoutElement>().preferredWidth = width;
            
            var btn = go.AddComponent<Button>();
            var txt = new GameObject("Text", typeof(RectTransform));
            txt.transform.SetParent(go.transform, false);
            var t = txt.AddComponent<TextMeshProUGUI>();
            t.text = label;
            t.fontSize = 18;
            t.fontStyle = FontStyles.Bold;
            t.alignment = TextAlignmentOptions.Center;
            t.color = Color.white;

            return btn;
        }

        private void CreateTextElement(string content, float size, FontStyles style, TextAlignmentOptions align)
        {
            var go = new GameObject(content, typeof(RectTransform));
            go.transform.SetParent(transform, false);
            var t = go.AddComponent<TextMeshProUGUI>();
            t.text = content;
            t.fontSize = size;
            t.fontStyle = style;
            t.alignment = align;
            t.color = textColor;
        }

        private void SetupReferences(Slider mS, Slider sS, Toggle mT, Toggle sT, TMP_Dropdown dd, Toggle hT, Button rB, Button bB)
        {
            var view = GetComponent<SettingsView>() ?? gameObject.AddComponent<SettingsView>();
            view.musicSlider = mS;
            view.sfxSlider = sS;
            view.musicToggle = mT;
            view.sfxToggle = sT;
            view.languageDropdown = dd;
            view.hapticToggle = hT;
            view.resetProgressButton = rB;
            view.backButton = bB;

            // Add logic controller if missing
            if (GetComponent<SettingsController>() == null)
            {
                var controller = gameObject.AddComponent<SettingsController>();
                controller.view = view;
            }
        }


        #endregion
    }
}