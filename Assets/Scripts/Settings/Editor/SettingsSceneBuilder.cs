#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Settings.Editor
{
    public class SettingsSceneBuilder
    {
        [MenuItem("Tools/Gazze/Final Settings Build")]
        public static void Build()
        {
            GameObject canvasGo = GameObject.Find("Canvas");
            if (canvasGo == null) return;

            // Delete existing
            GameObject old = GameObject.Find("SettingsPanel");
            if (old) Object.DestroyImmediate(old);

            // Sprite loading
            Sprite panelSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Violet Theme Ui/Buttons/Misc/Menu Background.png");
            Sprite blueBtnSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Violet Theme Ui/Buttons/Button Blue.png");
            Sprite redBtnSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Violet Theme Ui/Buttons/Button Red.png");

            // Create Root
            GameObject root = new GameObject("SettingsPanel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            root.transform.SetParent(canvasGo.transform, false);
            
            var rect = root.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(500, 650);
            
            var img = root.GetComponent<Image>();
            img.sprite = panelSprite;
            img.type = Image.Type.Sliced;
            img.color = Color.white;

            var vlg = root.AddComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(50, 50, 60, 50);
            vlg.spacing = 30;
            vlg.childAlignment = TextAnchor.UpperCenter;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandHeight = false;

            // 1. Title
            var title = CreateText(root.transform, "AYARLAR", 36, FontStyles.Bold | FontStyles.UpperCase);
            title.color = new Color32(255, 230, 255, 255);

            // 2. Music Row
            var musicRow = CreateRow(root.transform, "MusicRow");
            CreateLabel(musicRow.transform, "MÜZİK");
            var musicSlider = CreateSlider(musicRow.transform);
            var musicToggle = CreateToggle(musicRow.transform);

            // 3. SFX Row
            var sfxRow = CreateRow(root.transform, "SFXRow");
            CreateLabel(sfxRow.transform, "SFX");
            var sfxSlider = CreateSlider(sfxRow.transform);
            var sfxToggle = CreateToggle(sfxRow.transform);

            // 4. Language Dropdown
            var langRow = CreateRow(root.transform, "LangRow");
            CreateLabel(langRow.transform, "DİL");
            var dropdown = CreateDropdown(langRow.transform);

            // Spacer
            var spacer = new GameObject("Spacer", typeof(RectTransform));
            spacer.transform.SetParent(root.transform, false);
            spacer.AddComponent<LayoutElement>().flexibleHeight = 1;

            // 5. Buttons
            var btnRow = CreateRow(root.transform, "BtnRow");
            var resetBtn = CreateButton(btnRow.transform, "SIFIRLA", redBtnSprite);
            var backBtn = CreateButton(btnRow.transform, "GERİ", blueBtnSprite);

            // Attach Logic
            var view = root.GetComponent<SettingsView>() ?? root.AddComponent<SettingsView>();
            view.musicSlider = musicSlider;
            view.musicToggle = musicToggle;
            view.sfxSlider = sfxSlider;
            view.sfxToggle = sfxToggle;
            view.languageDropdown = dropdown;
            view.resetProgressButton = resetBtn;
            view.backButton = backBtn;

            if (root.GetComponent<SettingsController>() == null)
            {
                var ctrl = root.AddComponent<SettingsController>();
                ctrl.view = view;
            }

            Selection.activeGameObject = root;
            Debug.Log("<color=magenta>Gazze:</color> Premium Violet Settings Panel Built!");
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
            go.AddComponent<LayoutElement>().preferredWidth = 100;
        }

        static Slider CreateSlider(Transform parent)
        {
            var go = new GameObject("Slider", typeof(RectTransform), typeof(Slider));
            go.transform.SetParent(parent, false);
            var s = go.GetComponent<Slider>();
            go.AddComponent<LayoutElement>().flexibleWidth = 1;
            go.AddComponent<LayoutElement>().preferredHeight = 35;
            
            var bg = new GameObject("Background", typeof(RectTransform), typeof(Image));
            bg.transform.SetParent(go.transform, false);
            bg.GetComponent<Image>().color = new Color(1, 1, 1, 0.15f);
            var bgRect = bg.GetComponent<RectTransform>();
            bgRect.anchorMin = new Vector2(0, 0.25f);
            bgRect.anchorMax = new Vector2(1, 0.75f);
            bgRect.sizeDelta = Vector2.zero;
            
            var fillArea = new GameObject("Fill Area", typeof(RectTransform));
            fillArea.transform.SetParent(go.transform, false);
            var faRect = fillArea.GetComponent<RectTransform>();
            faRect.anchorMin = new Vector2(0, 0.25f);
            faRect.anchorMax = new Vector2(1, 0.75f);
            faRect.sizeDelta = Vector2.zero;

            var fill = new GameObject("Fill", typeof(RectTransform), typeof(Image));
            fill.transform.SetParent(fillArea.transform, false);
            fill.GetComponent<Image>().color = new Color32(147, 112, 219, 255); // Medium Purple
            s.fillRect = fill.GetComponent<RectTransform>();
            s.fillRect.sizeDelta = Vector2.zero;
            
            s.value = 0.8f;
            return s;
        }

        static Toggle CreateToggle(Transform parent)
        {
            var go = new GameObject("Toggle", typeof(RectTransform), typeof(Toggle));
            go.transform.SetParent(parent, false);
            var t = go.GetComponent<Toggle>();
            go.AddComponent<LayoutElement>().preferredWidth = 50;
            go.AddComponent<LayoutElement>().preferredHeight = 30;
            
            var bg = new GameObject("Background", typeof(RectTransform), typeof(Image));
            bg.transform.SetParent(go.transform, false);
            bg.GetComponent<Image>().color = new Color(1, 1, 1, 0.1f);
            var bgRect = bg.GetComponent<RectTransform>();
            bgRect.sizeDelta = new Vector2(50, 25);
            
            var check = new GameObject("Checkmark", typeof(RectTransform), typeof(Image));
            check.transform.SetParent(bg.transform, false);
            check.GetComponent<Image>().color = new Color32(147, 112, 219, 255);
            var ckRect = check.GetComponent<RectTransform>();
            ckRect.sizeDelta = new Vector2(20, 20);

            t.graphic = check.GetComponent<Image>();
            t.isOn = true;
            return t;
        }

        static TMP_Dropdown CreateDropdown(Transform parent)
        {
            var go = new GameObject("Dropdown", typeof(RectTransform), typeof(Image), typeof(TMP_Dropdown));
            go.transform.SetParent(parent, false);
            var d = go.GetComponent<TMP_Dropdown>();
            go.AddComponent<LayoutElement>().flexibleWidth = 1;
            go.AddComponent<LayoutElement>().preferredHeight = 40;
            go.GetComponent<Image>().color = new Color(0, 0, 0, 0.3f);
            
            // 1. Caption Color & Alignment
            var label = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
            label.transform.SetParent(go.transform, false);
            var t = label.GetComponent<TextMeshProUGUI>();
            t.fontSize = 18;
            t.alignment = TextAlignmentOptions.Center;
            t.color = Color.white;
            d.captionText = t;
            
            // 2. Build TEMPLATE
            var template = new GameObject("Template", typeof(RectTransform), typeof(Image), typeof(ScrollRect));
            template.transform.SetParent(go.transform, false);
            template.SetActive(false); // Must be inactive by default
            var tempRect = template.GetComponent<RectTransform>();
            tempRect.anchorMin = new Vector2(0, 0);
            tempRect.anchorMax = new Vector2(1, 0);
            tempRect.pivot = new Vector2(0.5f, 1);
            tempRect.anchoredPosition = new Vector2(0, -2);
            tempRect.sizeDelta = new Vector2(0, 150);
            template.GetComponent<Image>().color = new Color32(30, 30, 50, 255);
            d.template = tempRect;

            // Viewport
            var viewport = new GameObject("Viewport", typeof(RectTransform), typeof(Mask), typeof(Image));
            viewport.transform.SetParent(template.transform, false);
            viewport.GetComponent<RectTransform>().anchorMin = Vector2.zero;
            viewport.GetComponent<RectTransform>().anchorMax = Vector2.one;
            viewport.GetComponent<RectTransform>().sizeDelta = Vector2.zero;
            viewport.GetComponent<Image>().sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UIMask.psd");

            // Content
            var content = new GameObject("Content", typeof(RectTransform));
            content.transform.SetParent(viewport.transform, false);
            var contentRect = content.GetComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0, 1);
            contentRect.anchorMax = new Vector2(1, 1);
            contentRect.pivot = new Vector2(0.5f, 1);
            contentRect.sizeDelta = new Vector2(0, 40);
            
            var scroll = template.GetComponent<ScrollRect>();
            scroll.content = contentRect;
            scroll.viewport = viewport.GetComponent<RectTransform>();
            scroll.horizontal = false;

            // Item (The required Toggle)
            var item = new GameObject("Item", typeof(RectTransform), typeof(Toggle));
            item.transform.SetParent(content.transform, false);
            var itemRect = item.GetComponent<RectTransform>();
            itemRect.anchorMin = new Vector2(0, 0.5f);
            itemRect.anchorMax = new Vector2(1, 0.5f);
            itemRect.sizeDelta = new Vector2(0, 40);
            
            var itemToggle = item.GetComponent<Toggle>();
            
            // Item Background
            var itemBg = new GameObject("Item Background", typeof(RectTransform), typeof(Image));
            itemBg.transform.SetParent(item.transform, false);
            itemBg.GetComponent<RectTransform>().anchorMin = Vector2.zero;
            itemBg.GetComponent<RectTransform>().anchorMax = Vector2.one;
            itemBg.GetComponent<RectTransform>().sizeDelta = Vector2.zero;
            itemBg.GetComponent<Image>().color = new Color(1, 1, 1, 0.05f);

            // Item Checkmark
            var itemCheck = new GameObject("Item Checkmark", typeof(RectTransform), typeof(Image));
            itemCheck.transform.SetParent(item.transform, false);
            itemCheck.GetComponent<Image>().color = new Color32(147, 112, 219, 255);
            itemToggle.graphic = itemCheck.GetComponent<Image>();

            // Item Label
            var itemLabel = new GameObject("Item Label", typeof(RectTransform), typeof(TextMeshProUGUI));
            itemLabel.transform.SetParent(item.transform, false);
            var itemT = itemLabel.GetComponent<TextMeshProUGUI>();
            itemT.fontSize = 16;
            itemT.alignment = TextAlignmentOptions.Center;
            d.itemText = itemT;

            d.options.Add(new TMP_Dropdown.OptionData("Türkçe"));
            d.options.Add(new TMP_Dropdown.OptionData("English"));
            
            return d;
        }


        static Button CreateButton(Transform parent, string label, Sprite sprite)
        {
            var go = new GameObject(label, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            var img = go.GetComponent<Image>();
            img.sprite = sprite;
            img.type = Image.Type.Sliced;
            
            go.AddComponent<LayoutElement>().preferredHeight = 60;
            go.AddComponent<LayoutElement>().flexibleWidth = 1;
            
            var txt = CreateText(go.transform, label, 20, FontStyles.Bold);
            txt.color = Color.white;
            
            return go.GetComponent<Button>();
        }

    }
}
#endif
