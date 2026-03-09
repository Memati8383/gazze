using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using Settings;

namespace Settings.Editor
{
    public class SettingsUIBuilder : EditorWindow
    {
        [MenuItem("Tools/Build Professional Settings Panel")]
        public static void BuildSettingsPanel()
        {
            GameObject canvasObj = GameObject.Find("Canvas");
            if (canvasObj == null)
            {
                Debug.LogError("Canvas not found! Please create a Canvas first.");
                return;
            }

            // 1. Create Main Panel
            GameObject settingsPanel = new GameObject("SettingsPanel", typeof(RectTransform), typeof(Image));
            settingsPanel.transform.SetParent(canvasObj.transform, false);
            
            // 1.1 Create AudioManager in scene if not exists
            if (FindObjectOfType<AudioManager>() == null)
            {
                GameObject audioManagerObj = new GameObject("AudioManager", typeof(AudioManager));
                Undo.RegisterCreatedObjectUndo(audioManagerObj, "Create AudioManager");
            }

            RectTransform panelRect = settingsPanel.GetComponent<RectTransform>();
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;

            Image bg = settingsPanel.GetComponent<Image>();
            bg.color = new Color(0.1f, 0.1f, 0.12f, 0.95f);

            // 2. Add MVC Components
            SettingsView view = settingsPanel.AddComponent<SettingsView>();
            SettingsController controller = settingsPanel.AddComponent<SettingsController>();

            // 3. Create Container (ScrollView for responsiveness)
            GameObject container = new GameObject("Container", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
            container.transform.SetParent(settingsPanel.transform, false);
            
            RectTransform containerRect = container.GetComponent<RectTransform>();
            containerRect.anchorMin = new Vector2(0.3f, 0.1f);
            containerRect.anchorMax = new Vector2(0.7f, 0.9f);
            containerRect.offsetMin = Vector2.zero;
            containerRect.offsetMax = Vector2.zero;

            VerticalLayoutGroup layout = container.GetComponent<VerticalLayoutGroup>();
            layout.spacing = 30;
            layout.padding = new RectOffset(20, 20, 20, 20);
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.childControlHeight = false;
            layout.childForceExpandHeight = false;

            ContentSizeFitter fitter = container.GetComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            // 4. Title
            CreateText(container.transform, "PROFESSIONAL SETTINGS", 45, FontStyles.Bold, Color.white);

            // 5. Sections
            view.musicSlider = CreateProfessionalSlider(container.transform, "MUSIC VOLUME");
            view.musicToggle = CreateProfessionalToggle(container.transform, "MUSIC MUTE");

            view.sfxSlider = CreateProfessionalSlider(container.transform, "SFX VOLUME");
            view.sfxToggle = CreateProfessionalToggle(container.transform, "SFX MUTE");

            view.graphicsDropdown = CreateProfessionalDropdown(container.transform, "GRAPHICS QUALITY", new string[] { "Low", "Medium", "High", "Ultra" });

            // 6. Back Button
            GameObject btnObj = new GameObject("BackButton", typeof(RectTransform), typeof(Image), typeof(Button));
            btnObj.transform.SetParent(container.transform, false);
            btnObj.GetComponent<RectTransform>().sizeDelta = new Vector2(250, 60);
            btnObj.GetComponent<Image>().color = new Color(0.8f, 0.2f, 0.2f, 1f);
            view.backButton = btnObj.GetComponent<Button>();
            CreateText(btnObj.transform, "BACK", 22, FontStyles.Bold, Color.white);

            // Link to MainMenuManager if exists
            MainMenuManager mainMenu = FindObjectOfType<MainMenuManager>();
            if (mainMenu != null)
            {
                mainMenu.settingsPanel = settingsPanel;
                EditorUtility.SetDirty(mainMenu);
            }

            settingsPanel.SetActive(false);
            Selection.activeGameObject = settingsPanel;
            Undo.RegisterCreatedObjectUndo(settingsPanel, "Build Professional Settings Panel");
            Debug.Log("Unity MCP: Professional Settings Panel built and linked to MainMenuManager.");
        }

        private static Slider CreateProfessionalSlider(Transform parent, string label)
        {
            GameObject group = new GameObject(label + "_Group", typeof(RectTransform), typeof(VerticalLayoutGroup));
            group.transform.SetParent(parent, false);
            group.GetComponent<VerticalLayoutGroup>().childAlignment = TextAnchor.MiddleLeft;
            group.GetComponent<VerticalLayoutGroup>().spacing = 5;
            CreateText(group.transform, label, 18, FontStyles.Normal, Color.gray);

            // Slider Root
            GameObject sliderRoot = new GameObject("Slider", typeof(RectTransform), typeof(Slider));
            sliderRoot.transform.SetParent(group.transform, false);
            sliderRoot.GetComponent<RectTransform>().sizeDelta = new Vector2(400, 30);
            Slider slider = sliderRoot.GetComponent<Slider>();

            // Background
            GameObject bg = new GameObject("Background", typeof(RectTransform), typeof(Image));
            bg.transform.SetParent(sliderRoot.transform, false);
            bg.GetComponent<RectTransform>().anchorMin = new Vector2(0, 0.25f);
            bg.GetComponent<RectTransform>().anchorMax = new Vector2(1, 0.75f);
            bg.GetComponent<RectTransform>().sizeDelta = Vector2.zero;
            bg.GetComponent<Image>().color = Color.black;

            // Fill Area
            GameObject fillArea = new GameObject("Fill Area", typeof(RectTransform));
            fillArea.transform.SetParent(sliderRoot.transform, false);
            fillArea.GetComponent<RectTransform>().anchorMin = new Vector2(0, 0.25f);
            fillArea.GetComponent<RectTransform>().anchorMax = new Vector2(1, 0.75f);
            fillArea.GetComponent<RectTransform>().sizeDelta = Vector2.zero;

            GameObject fill = new GameObject("Fill", typeof(RectTransform), typeof(Image));
            fill.transform.SetParent(fillArea.transform, false);
            fill.GetComponent<RectTransform>().sizeDelta = Vector2.zero;
            fill.GetComponent<Image>().color = new Color(0.2f, 0.6f, 1f, 1f);

            // Handle Area
            GameObject handleArea = new GameObject("Handle Area", typeof(RectTransform));
            handleArea.transform.SetParent(sliderRoot.transform, false);
            handleArea.GetComponent<RectTransform>().anchorMin = new Vector2(0, 0);
            handleArea.GetComponent<RectTransform>().anchorMax = new Vector2(1, 1);
            handleArea.GetComponent<RectTransform>().sizeDelta = Vector2.zero;

            GameObject handle = new GameObject("Handle", typeof(RectTransform), typeof(Image));
            handle.transform.SetParent(handleArea.transform, false);
            handle.GetComponent<RectTransform>().sizeDelta = new Vector2(30, 0);
            handle.GetComponent<Image>().color = Color.white;

            slider.fillRect = fill.GetComponent<RectTransform>();
            slider.handleRect = handle.GetComponent<RectTransform>();
            slider.targetGraphic = handle.GetComponent<Image>();
            slider.minValue = 0;
            slider.maxValue = 1;

            return slider;
        }

        private static Toggle CreateProfessionalToggle(Transform parent, string label)
        {
            GameObject toggleRoot = new GameObject(label, typeof(RectTransform), typeof(Toggle));
            toggleRoot.transform.SetParent(parent, false);
            toggleRoot.GetComponent<RectTransform>().sizeDelta = new Vector2(250, 40);
            Toggle toggle = toggleRoot.GetComponent<Toggle>();

            GameObject bg = new GameObject("Background", typeof(RectTransform), typeof(Image));
            bg.transform.SetParent(toggleRoot.transform, false);
            bg.GetComponent<RectTransform>().sizeDelta = new Vector2(40, 40);
            bg.GetComponent<RectTransform>().anchoredPosition = new Vector2(-100, 0);
            bg.GetComponent<Image>().color = Color.black;

            GameObject check = new GameObject("Checkmark", typeof(RectTransform), typeof(Image));
            check.transform.SetParent(bg.transform, false);
            check.GetComponent<RectTransform>().sizeDelta = new Vector2(30, 30);
            check.GetComponent<Image>().color = new Color(0.2f, 1f, 0.2f, 1f);

            GameObject textObj = CreateText(toggleRoot.transform, label, 18, FontStyles.Normal, Color.white).gameObject;
            textObj.GetComponent<RectTransform>().anchoredPosition = new Vector2(30, 0);

            toggle.graphic = check.GetComponent<Image>();
            toggle.targetGraphic = bg.GetComponent<Image>();

            return toggle;
        }

        private static TMP_Dropdown CreateProfessionalDropdown(Transform parent, string label, string[] options)
        {
            GameObject group = new GameObject(label + "_Group", typeof(RectTransform), typeof(VerticalLayoutGroup));
            group.transform.SetParent(parent, false);
            group.GetComponent<VerticalLayoutGroup>().childAlignment = TextAnchor.MiddleLeft;
            CreateText(group.transform, label, 18, FontStyles.Normal, Color.gray);

            // 1. Dropdown Root
            GameObject dropdownObj = new GameObject("Dropdown", typeof(RectTransform), typeof(Image), typeof(TMP_Dropdown));
            dropdownObj.transform.SetParent(group.transform, false);
            dropdownObj.GetComponent<RectTransform>().sizeDelta = new Vector2(400, 45);
            dropdownObj.GetComponent<Image>().color = new Color(0.2f, 0.2f, 0.25f, 1f);

            TMP_Dropdown dropdown = dropdownObj.GetComponent<TMP_Dropdown>();

            // 2. Caption Text
            GameObject captionTextObj = new GameObject("Caption Text", typeof(RectTransform), typeof(TextMeshProUGUI));
            captionTextObj.transform.SetParent(dropdownObj.transform, false);
            RectTransform captionRect = captionTextObj.GetComponent<RectTransform>();
            captionRect.anchorMin = Vector2.zero;
            captionRect.anchorMax = Vector2.one;
            captionRect.offsetMin = new Vector2(10, 0);
            captionRect.offsetMax = new Vector2(-25, 0);

            TextMeshProUGUI captionTmp = captionTextObj.GetComponent<TextMeshProUGUI>();
            captionTmp.alignment = TextAlignmentOptions.Left;
            captionTmp.fontSize = 20;
            captionTmp.color = Color.white;
            dropdown.captionText = captionTmp;

            // 3. Template (Essential for TMP_Dropdown to work)
            GameObject template = new GameObject("Template", typeof(RectTransform), typeof(Image), typeof(ScrollRect));
            template.transform.SetParent(dropdownObj.transform, false);
            RectTransform templateRect = template.GetComponent<RectTransform>();
            templateRect.anchorMin = new Vector2(0, 0);
            templateRect.anchorMax = new Vector2(1, 0);
            templateRect.pivot = new Vector2(0.5f, 1);
            templateRect.anchoredPosition = new Vector2(0, -2);
            templateRect.sizeDelta = new Vector2(0, 150);
            template.GetComponent<Image>().color = new Color(0.15f, 0.15f, 0.2f, 1f);
            template.SetActive(false);
            dropdown.template = templateRect;

            // 4. Viewport
            GameObject viewport = new GameObject("Viewport", typeof(RectTransform), typeof(Image), typeof(Mask));
            viewport.transform.SetParent(template.transform, false);
            RectTransform viewRect = viewport.GetComponent<RectTransform>();
            viewRect.anchorMin = Vector2.zero;
            viewRect.anchorMax = Vector2.one;
            viewRect.sizeDelta = Vector2.zero;
            viewport.GetComponent<Mask>().showMaskGraphic = false;

            // 5. Content
            GameObject content = new GameObject("Content", typeof(RectTransform));
            content.transform.SetParent(viewport.transform, false);
            RectTransform contentRect = content.GetComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0, 1);
            contentRect.anchorMax = new Vector2(1, 1);
            contentRect.pivot = new Vector2(0.5f, 1);
            contentRect.sizeDelta = new Vector2(0, 35); // Initial height for one item

            // 6. Item (Must have a Toggle)
            GameObject item = new GameObject("Item", typeof(RectTransform), typeof(Toggle));
            item.transform.SetParent(content.transform, false);
            RectTransform itemRect = item.GetComponent<RectTransform>();
            itemRect.anchorMin = new Vector2(0, 0.5f);
            itemRect.anchorMax = new Vector2(1, 0.5f);
            itemRect.pivot = new Vector2(0.5f, 0.5f);
            itemRect.anchoredPosition = Vector2.zero;
            itemRect.sizeDelta = new Vector2(0, 35);
            
            Toggle itemToggle = item.GetComponent<Toggle>();

            // 7. Item Background (Target Graphic)
            GameObject itemBg = new GameObject("Item Background", typeof(RectTransform), typeof(Image));
            itemBg.transform.SetParent(item.transform, false);
            RectTransform itemBgRect = itemBg.GetComponent<RectTransform>();
            itemBgRect.anchorMin = Vector2.zero;
            itemBgRect.anchorMax = Vector2.one;
            itemBgRect.sizeDelta = Vector2.zero;
            itemBg.GetComponent<Image>().color = new Color(0, 0, 0, 0); // Transparent by default
            itemToggle.targetGraphic = itemBg.GetComponent<Image>();

            // 8. Item Checkmark (Graphic)
            GameObject itemCheckmark = new GameObject("Item Checkmark", typeof(RectTransform), typeof(Image));
            itemCheckmark.transform.SetParent(item.transform, false);
            RectTransform checkmarkRect = itemCheckmark.GetComponent<RectTransform>();
            checkmarkRect.anchorMin = new Vector2(0, 0.5f);
            checkmarkRect.anchorMax = new Vector2(0, 0.5f);
            checkmarkRect.anchoredPosition = new Vector2(10, 0);
            checkmarkRect.sizeDelta = new Vector2(20, 20);
            itemCheckmark.GetComponent<Image>().color = new Color(0.2f, 0.8f, 0.2f, 1f);
            itemToggle.graphic = itemCheckmark.GetComponent<Image>();

            // 9. Item Label
            GameObject itemLabel = new GameObject("Item Label", typeof(RectTransform), typeof(TextMeshProUGUI));
            itemLabel.transform.SetParent(item.transform, false);
            RectTransform itemLabelRect = itemLabel.GetComponent<RectTransform>();
            itemLabelRect.anchorMin = Vector2.zero;
            itemLabelRect.anchorMax = Vector2.one;
            itemLabelRect.offsetMin = new Vector2(35, 0);
            itemLabelRect.offsetMax = new Vector2(-10, 0);
            
            TextMeshProUGUI itemTmp = itemLabel.GetComponent<TextMeshProUGUI>();
            itemTmp.alignment = TextAlignmentOptions.Left;
            itemTmp.fontSize = 18;
            itemTmp.color = Color.white;
            dropdown.itemText = itemTmp;

            // 10. ScrollRect Setup
            ScrollRect sr = template.GetComponent<ScrollRect>();
            sr.content = contentRect;
            sr.viewport = viewRect;
            sr.horizontal = false;
            sr.vertical = true;
            sr.movementType = ScrollRect.MovementType.Clamped;

            // Finalize options
            dropdown.ClearOptions();
            foreach (var opt in options) dropdown.options.Add(new TMP_Dropdown.OptionData(opt));

            return dropdown;
        }

        private static TextMeshProUGUI CreateText(Transform parent, string text, float size, FontStyles style, Color color)
        {
            GameObject obj = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
            obj.transform.SetParent(parent, false);
            TextMeshProUGUI tmp = obj.GetComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = size;
            tmp.fontStyle = style;
            tmp.color = color;
            tmp.alignment = TextAlignmentOptions.Center;
            return tmp;
        }
    }
}