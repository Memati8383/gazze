#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Settings.Editor
{
    /// <summary>
    /// Editor menusu uzerinden ayarlar panelinin temel violet temasini kurar.
    /// </summary>
    public class SettingsSceneBuilder
    {
        [MenuItem("Tools/Gazze/Integrate Control Settings")]
        public static void Build()
        {
            GameObject canvasGo = GameObject.Find("Canvas");
            if (canvasGo == null) { Debug.LogError("Canvas bulunamadı!"); return; }

            // Mevcut SettingsPanel'i bul
            Transform settingsPanel = canvasGo.transform.Find("SettingsPanel");
            if (settingsPanel == null)
            {
                Debug.LogError("Mevcut 'SettingsPanel' bulunamadı! Lütfen önce temel paneli kurun veya ismini kontrol edin.");
                return;
            }

            // İçerik alanını bul (ScrollArea içindeki Content)
            Transform content = settingsPanel.Find("SettingsCard/ScrollArea/Viewport/Content");
            if (content == null)
            {
                // Alternatif yol denemesi (Eğer hiyerarşi farklıysa)
                content = settingsPanel.GetComponentInChildren<VerticalLayoutGroup>()?.transform;
            }

            if (content == null)
            {
                Debug.LogError("SettingsPanel içinde 'Content' (VerticalLayoutGroup) bulunamadı!");
                return;
            }

            // --- SCROLL DÜZELTME: ContentSizeFitter Ekle ---
            var fitter = content.GetComponent<ContentSizeFitter>() ?? content.gameObject.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;

            var vlg = content.GetComponent<VerticalLayoutGroup>();
            if (vlg)
            {
                vlg.childControlHeight = true;
                vlg.childForceExpandHeight = false;
                vlg.spacing = 20;
            }

            // Mevcut (eski) elemanların küçülmesini engellemek için LayoutElement ekle
            foreach (Transform child in content)
            {
                if (child.GetComponent<LayoutElement>() == null)
                {
                    var le = child.gameObject.AddComponent<LayoutElement>();
                    var rt = child.GetComponent<RectTransform>();
                    le.preferredHeight = rt != null ? rt.rect.height : 60;
                    if (le.preferredHeight < 10) le.preferredHeight = 60; // Güvenlik önlemi
                }
            }

            // Temizleme: Eğer daha önce eklediysek eski kontrol grubunu sil
            string[] toDelete = { "Header_KONTROL AYARLARI", "ControlMethod_Row", "AccelMode_Row", "Sensitivity_Group", "CalibrationPanel", "Control_Separator" };
            foreach (var name in toDelete)
            {
                Transform t = content.Find(name);
                if (t) Object.DestroyImmediate(t.gameObject);
            }

            // "DİL SEÇİMİ"nin (LangHeader) indeksini bul ki hemen üstüne ekleyelim
            int insertIndex = content.childCount;
            Transform langHeader = content.Find("LangHeader"); // Assuming "LangHeader" is the name of the language selection header
            if (langHeader != null) insertIndex = langHeader.GetSiblingIndex();

            // Renk Paleti (Orange/Amber Theme)
            Color32 OrangeAccent = new Color32(255, 165, 0, 255);
            Color32 HeaderColor = new Color32(255, 140, 0, 255);

            // --- AYRAÇ ---
            var sep = CreateSeparator(content, "Control_Separator");
            sep.transform.SetSiblingIndex(insertIndex++);

            // --- KONTROL AYARLARI BAŞLIK ---
            var headerGo = CreateSectionHeader(content, "KONTROL AYARLARI", HeaderColor);
            headerGo.transform.SetSiblingIndex(insertIndex++);
            
            // 1. Kontrol Yöntemi
            var controlRes = CreateDropdownRow(content, "Kontrol Yöntemi", out var controlDropdown);
            controlRes.name = "ControlMethod_Row";
            controlRes.transform.SetSiblingIndex(insertIndex++);
            controlDropdown.options.Clear();
            controlDropdown.options.Add(new TMP_Dropdown.OptionData("BUTON"));
            controlDropdown.options.Add(new TMP_Dropdown.OptionData("İVMEÖLÇER (TİLT)"));

            // 2. Hızlanma Modu
            var accelRes = CreateDropdownRow(content, "Hızlanma Modu", out var accelDropdown);
            accelRes.name = "AccelMode_Row";
            accelRes.transform.SetSiblingIndex(insertIndex++);
            accelDropdown.options.Clear();
            accelDropdown.options.Add(new TMP_Dropdown.OptionData("MANUEL"));
            accelDropdown.options.Add(new TMP_Dropdown.OptionData("OTOMATİK"));

            // 3. Hassasiyet Slider
            var sensRes = CreateSliderRowNoToggle(content, "Direksiyon Hassasiyeti", out var sensSlider, OrangeAccent);
            sensRes.name = "Sensitivity_Group";
            sensRes.transform.SetSiblingIndex(insertIndex++);

            // 4. Kalibrasyon Paneli
            var calibPanel = new GameObject("CalibrationPanel", typeof(RectTransform));
            calibPanel.transform.SetParent(content, false);
            calibPanel.transform.SetSiblingIndex(insertIndex++);
            var calibLe = calibPanel.AddComponent<LayoutElement>();
            calibLe.preferredHeight = 80;
            calibLe.minHeight = 80;
            
            var calibBtn = CreateButton(calibPanel.transform, "CİHAZI KALİBRE ET", OrangeAccent, Color.white);
            calibPanel.SetActive(false);

            // Logic Bağlantıları (SettingsView'ı mevcut objede güncelle)
            var view = settingsPanel.GetComponent<SettingsView>();
            if (view != null)
            {
                view.controlMethodDropdown = controlDropdown;
                view.accelerationModeDropdown = accelDropdown;
                view.controlSensitivitySlider = sensSlider;
                view.calibrateButton = calibBtn;
                view.calibrationPanel = calibPanel;
                EditorUtility.SetDirty(view);
            }

            Debug.Log("<color=orange>Gazze:</color> Kontrol Ayarları Mevcut Panele Doğru Sırada ve Scroll Desteğiyle Entegre Edildi!");
            Selection.activeGameObject = settingsPanel.gameObject;
        }

        static GameObject CreateSeparator(Transform parent, string name)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(parent, false);
            var img = go.GetComponent<Image>();
            img.color = new Color(1, 1, 1, 0.1f);
            var le = go.AddComponent<LayoutElement>();
            le.preferredHeight = 2;
            le.minHeight = 2;
            return go;
        }

        static GameObject CreateSectionHeader(Transform parent, string text, Color color)
        {
            var go = new GameObject("Header_" + text, typeof(RectTransform), typeof(TextMeshProUGUI));
            go.transform.SetParent(parent, false);
            var t = go.GetComponent<TextMeshProUGUI>();
            t.text = text;
            t.fontSize = 14;
            t.fontStyle = FontStyles.Bold;
            t.color = color;
            t.alignment = TextAlignmentOptions.Center;
            
            var le = go.AddComponent<LayoutElement>();
            le.preferredHeight = 60; // Başlık için geniş alan
            le.minHeight = 60;
            return go;
        }

        static GameObject CreateSliderWithLabel(Transform parent, string labelText, out Slider slider, out Toggle toggle)
        {
            var root = new GameObject(labelText + "_Group", typeof(RectTransform));
            root.transform.SetParent(parent, false);
            var vlg = root.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 5;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;

            var lbl = CreateText(root.transform, labelText, 18, FontStyles.Normal);
            lbl.alignment = TextAlignmentOptions.Left;
            lbl.color = new Color(0.8f, 0.8f, 0.8f);

            var row = CreateRow(root.transform, "Row");
            slider = CreateSlider(row.transform, new Color32(255, 165, 0, 255));
            toggle = CreateToggle(row.transform, new Color32(255, 165, 0, 255));

            return root;
        }

        static GameObject CreateSliderRowNoToggle(Transform parent, string labelText, out Slider slider, Color color)
        {
            var root = new GameObject(labelText + "_Group", typeof(RectTransform));
            root.transform.SetParent(parent, false);
            var vlg = root.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 10;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.padding = new RectOffset(0, 0, 10, 10);

            var le = root.AddComponent<LayoutElement>();
            le.preferredHeight = 90; // Slider satırı için dikey alan
            le.minHeight = 90;

            var lbl = CreateText(root.transform, labelText, 18, FontStyles.Normal);
            lbl.alignment = TextAlignmentOptions.Left;
            lbl.color = new Color(0.85f, 0.85f, 0.85f);

            slider = CreateSlider(root.transform, color);
            return root;
        }

        static GameObject CreateToggleRow(Transform parent, string labelText, out Toggle toggle, Color color)
        {
            var row = CreateRow(parent, labelText + "_Row");
            var le = row.AddComponent<LayoutElement>();
            le.preferredHeight = 80; // Toggle satırı için dikey alan
            le.minHeight = 80;

            var lbl = CreateText(row.transform, labelText, 18, FontStyles.Normal);
            lbl.alignment = TextAlignmentOptions.Left;
            lbl.color = new Color(0.85f, 0.85f, 0.85f);
            
            toggle = CreateToggle(row.transform, color);
            return row;
        }

        static GameObject CreateDropdownRow(Transform parent, string labelText, out TMP_Dropdown dropdown)
        {
            var row = new GameObject(labelText + "_Row", typeof(RectTransform));
            row.transform.SetParent(parent, false);
            var vlg = row.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 10;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.padding = new RectOffset(0, 0, 10, 10);

            var le = row.AddComponent<LayoutElement>();
            le.preferredHeight = 100; // Dropdown + Etiket için alan
            le.minHeight = 100;

            var lbl = CreateText(row.transform, labelText, 18, FontStyles.Normal);
            lbl.alignment = TextAlignmentOptions.Left;
            lbl.color = new Color(0.85f, 0.85f, 0.85f);

            dropdown = CreateDropdown(row.transform);
            return row;
        }

        static TMP_Dropdown CreateDropdownNoLabel(Transform parent)
        {
            return CreateDropdown(parent);
        }

        static TextMeshProUGUI CreateText(Transform parent, string content, float size, FontStyles style)
        {
            var go = new GameObject("Text", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            go.transform.SetParent(parent, false);
            var t = go.GetComponent<TextMeshProUGUI>();
            t.text = content;
            t.fontSize = size;
            t.fontStyle = style;
            t.alignment = TextAlignmentOptions.Center;
            return t;
        }

        static GameObject CreateRow(Transform parent, string name)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var hlg = go.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 20;
            hlg.childAlignment = TextAnchor.MiddleLeft;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = false;
            return go;
        }

        static void CreateLabel(Transform parent, string text)
        {
            var go = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
            go.transform.SetParent(parent, false);
            var t = go.GetComponent<TextMeshProUGUI>();
            t.text = text;
            t.fontSize = 22;
            t.fontStyle = FontStyles.Bold;
            t.color = new Color32(220, 220, 255, 255);
            go.AddComponent<LayoutElement>();
        }

        static Slider CreateSlider(Transform parent, Color32 accent)
        {
            var go = new GameObject("Slider", typeof(RectTransform), typeof(Slider));
            var le = go.AddComponent<LayoutElement>();
            le.preferredHeight = 12; // Daha ince modern slider
            le.flexibleWidth = 1;
            go.transform.SetParent(parent, false);

            var s = go.GetComponent<Slider>();
            
            var bg = new GameObject("Background", typeof(RectTransform), typeof(Image));
            bg.transform.SetParent(go.transform, false);
            bg.GetComponent<Image>().color = new Color(1, 1, 1, 0.15f);
            var bgRect = bg.GetComponent<RectTransform>();
            bgRect.anchorMin = new Vector2(0, 0.45f);
            bgRect.anchorMax = new Vector2(1, 0.55f);
            bgRect.sizeDelta = Vector2.zero;
            
            var fillArea = new GameObject("Fill Area", typeof(RectTransform));
            fillArea.transform.SetParent(go.transform, false);
            var faRect = fillArea.GetComponent<RectTransform>();
            faRect.anchorMin = new Vector2(0, 0);
            faRect.anchorMax = new Vector2(1, 1);
            faRect.sizeDelta = new Vector2(-10, 0);

            var fill = new GameObject("Fill", typeof(RectTransform), typeof(Image));
            fill.transform.SetParent(fillArea.transform, false);
            var fillImg = fill.GetComponent<Image>();
            fillImg.color = accent;
            s.fillRect = fill.GetComponent<RectTransform>();
            s.fillRect.sizeDelta = Vector2.zero;

            var handleArea = new GameObject("Handle Area", typeof(RectTransform));
            handleArea.transform.SetParent(go.transform, false);
            handleArea.GetComponent<RectTransform>().anchorMin = Vector2.zero;
            handleArea.GetComponent<RectTransform>().anchorMax = Vector2.one;
            handleArea.GetComponent<RectTransform>().sizeDelta = new Vector2(-20, 0);

            var handle = new GameObject("Handle", typeof(RectTransform), typeof(Image));
            handle.transform.SetParent(handleArea.transform, false);
            handle.GetComponent<Image>().color = Color.white;
            var hRect = handle.GetComponent<RectTransform>();
            hRect.sizeDelta = new Vector2(25, 25);
            s.handleRect = hRect;
            
            s.value = 0.8f;
            return s;
        }

        static Toggle CreateToggle(Transform parent, Color32 accent)
        {
            var go = new GameObject("Toggle", typeof(RectTransform), typeof(Toggle));
            go.transform.SetParent(parent, false);
            var t = go.GetComponent<Toggle>();
            var le = go.AddComponent<LayoutElement>();
            le.preferredWidth = 60;
            le.preferredHeight = 35;
            le.flexibleWidth = 0;
            
            var bg = new GameObject("Background", typeof(RectTransform), typeof(Image));
            bg.transform.SetParent(go.transform, false);
            var bgImg = bg.GetComponent<Image>();
            bgImg.color = accent; // Aktifken turuncu
            var bgRect = bg.GetComponent<RectTransform>();
            bgRect.sizeDelta = new Vector2(60, 30);
            
            var check = new GameObject("Checkmark", typeof(RectTransform), typeof(Image));
            check.transform.SetParent(bg.transform, false);
            check.GetComponent<Image>().color = Color.white;
            var ckRect = check.GetComponent<RectTransform>();
            ckRect.sizeDelta = new Vector2(25, 25);

            t.graphic = check.GetComponent<Image>();
            t.isOn = true;
            return t;
        }

        static TMP_Dropdown CreateDropdown(Transform parent)
        {
            var go = new GameObject("Dropdown", typeof(RectTransform), typeof(Image), typeof(TMP_Dropdown));
            go.transform.SetParent(parent, false);
            var d = go.GetComponent<TMP_Dropdown>();
            var le = go.AddComponent<LayoutElement>();
            le.flexibleWidth = 1;
            le.preferredHeight = 50;
            go.GetComponent<Image>().color = new Color(0.1f, 0.1f, 0.15f, 0.8f);
            
            var label = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
            label.transform.SetParent(go.transform, false);
            var t = label.GetComponent<TextMeshProUGUI>();
            t.fontSize = 20;
            t.alignment = TextAlignmentOptions.Center;
            t.fontStyle = FontStyles.Bold;
            t.color = Color.white;
            d.captionText = t;
            label.GetComponent<RectTransform>().sizeDelta = Vector2.zero;
            label.GetComponent<RectTransform>().anchorMin = Vector2.zero;
            label.GetComponent<RectTransform>().anchorMax = Vector2.one;
            
            // Temel template yapısı (basitleştirilmiş)
            var template = new GameObject("Template", typeof(RectTransform), typeof(Image), typeof(ScrollRect));
            template.transform.SetParent(go.transform, false);
            template.SetActive(false);
            var tempRect = template.GetComponent<RectTransform>();
            tempRect.anchorMin = new Vector2(0, 0);
            tempRect.anchorMax = new Vector2(1, 0);
            tempRect.pivot = new Vector2(0.5f, 1);
            tempRect.sizeDelta = new Vector2(0, 200);
            template.GetComponent<Image>().color = new Color32(20, 20, 30, 255);
            d.template = tempRect;

            return d;
        }

        static Button CreateButton(Transform parent, string label, Color32 color, Color32 textColor)
        {
            var go = new GameObject(label, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            var img = go.GetComponent<Image>();
            img.color = color;
            
            var le = go.AddComponent<LayoutElement>();
            le.preferredHeight = 75;
            le.flexibleWidth = 1;
            
            var txt = CreateText(go.transform, label, 20, FontStyles.Bold);
            txt.color = textColor;
            txt.alignment = TextAlignmentOptions.Center;
            txt.GetComponent<RectTransform>().anchorMin = Vector2.zero;
            txt.GetComponent<RectTransform>().anchorMax = Vector2.one;
            txt.GetComponent<RectTransform>().sizeDelta = Vector2.zero;
            
            return go.GetComponent<Button>();
        }

    }
}
#endif
