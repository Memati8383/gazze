using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using Gazze.UI;

namespace Settings
{
    /// <summary>
    /// Modern Ayarlar Paneli — Warm Gold & Amber Glassmorphism
    /// Staggered entrance, fluid micro-interactions, breathing borders
    /// </summary>
    public class SettingsVisualOverhaul : MonoBehaviour
    {
        // ─── DESIGN TOKENS ────────────────────────────
        // Background & Surface (Warm Charcoal-Brown)
        static readonly Color DarkBackground    = new Color(0.04f, 0.03f, 0.02f, 0.98f);
        static readonly Color CardBackground    = new Color(0.08f, 0.06f, 0.04f, 0.88f);
        static readonly Color GlassOverlay      = new Color(0.25f, 0.18f, 0.08f, 0.15f);
        static readonly Color SurfaceElevated   = new Color(0.15f, 0.11f, 0.06f, 0.50f);

        // Accent — Warm Gold & Amber
        static readonly Color AccentCyan        = new Color32(255, 191, 36, 255);  // Rich Gold
        static readonly Color AccentViolet      = new Color32(205, 127, 50, 255);  // Copper
        static readonly Color AccentIndigo      = new Color32(180, 120, 50, 255);  // Deep Amber
        static readonly Color SuccessGreen      = new Color32(180, 210, 80, 255);  // Olive Gold
        static readonly Color DangerRed         = new Color32(220, 80, 60, 255);   // Warm Crimson

        // Text
        static readonly Color TextPrimary       = new Color32(255, 248, 235, 255); // Cream White
        static readonly Color TextSecondary     = new Color32(220, 190, 140, 230); // Soft Gold
        static readonly Color TextMuted         = new Color32(160, 140, 110, 200); // Warm Gray

        // Control Surfaces
        static readonly Color SliderTrack       = new Color(0.06f, 0.04f, 0.02f, 0.90f);
        static readonly Color SliderFill        = AccentCyan;
        static readonly Color ToggleOn          = AccentCyan;
        static readonly Color ToggleOff         = new Color(0.12f, 0.10f, 0.06f, 0.60f);
        static readonly Color DividerColor      = new Color(0.50f, 0.38f, 0.15f, 0.15f);

        // Row backgrounds (Warm elevated layers)
        static readonly Color RowBgA            = new Color(0.10f, 0.08f, 0.04f, 0.40f);
        static readonly Color RowBgB            = new Color(0.12f, 0.10f, 0.06f, 0.25f);

        [Header("Assets")]
        public Sprite panelSprite;
        public Sprite roundedSprite;
        public Sprite iconSprite;
        public Sprite backgroundSprite;
        
        [Header("Font Seçenekleri (Premium)")]
        public TMP_FontAsset headerFont;    // Başlıklar (Örn: Anton)
        public TMP_FontAsset labelFont;     // Ayar isimleri (Örn: Oswald Bold)
        public TMP_FontAsset valueFont;     // Değerler ve Sayılar (Örn: Electronic Highway Sign)
        public TMP_FontAsset buttonFont;    // Butonlar (Örn: Oswald)
        public TMP_FontAsset customFont;    // Legacy fallback
        public TMP_FontAsset fallbackFont;  // Türkçe karakter desteği için (Örn: LiberationSans)

#if UNITY_EDITOR
        [MenuItem("Tools/Gazze/Build Modern Settings Panel")]
        public static void BuildModernPanelStatic()
        {
            var panel = FindPanelInScene();
            if (panel != null) panel.BuildSettingsPanel();
            else Debug.LogError("Sahnede SettingsVisualOverhaul bulunamadı!");
        }

        [ContextMenu("Modern Ayarlar Panelini Oluştur")]
        public void BuildModernPanelContext()
        {
            BuildSettingsPanel();
        }

        static SettingsVisualOverhaul FindPanelInScene()
        {
            return UnityEngine.Object.FindFirstObjectByType<SettingsVisualOverhaul>(FindObjectsInactive.Include);
        }
#endif

        // Track row index for alternating backgrounds
        private int _rowIndex;

        public void BuildSettingsPanel()
        {
            _rowIndex = 0;

            // 0. Ensure Global Font Fallbacks (Turkish Support)
            if (fallbackFont == null)
                fallbackFont = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");

            var fontsToFix = new List<TMP_FontAsset> { headerFont, labelFont, valueFont, buttonFont, customFont };
            foreach (var f in fontsToFix)
            {
                if (f != null && fallbackFont != null)
                {
                    if (f.fallbackFontAssetTable == null) f.fallbackFontAssetTable = new List<TMP_FontAsset>();
                    if (!f.fallbackFontAssetTable.Contains(fallbackFont))
                    {
                        f.fallbackFontAssetTable.Add(fallbackFont);
#if UNITY_EDITOR
                        UnityEditor.EditorUtility.SetDirty(f);
#endif
                    }
                }
            }

            // 1. Temizlik
            ClearChildren();

            // 2. Canvas Setup
            var rect = GetComponent<RectTransform>() ?? gameObject.AddComponent<RectTransform>();
            Stretch(rect);

            // Ensure root is transparent overlay
            var rootImg = GetComponent<Image>() ?? gameObject.AddComponent<Image>();
            rootImg.color = new Color(0, 0, 0, 0);
            rootImg.raycastTarget = true;
            Stretch(GetComponent<RectTransform>());

            // ─── FULLSCREEN BACKGROUND LAYER ───
            var canvasParent = transform.parent;
            Transform bgTransform = canvasParent != null ? canvasParent.Find("GlobalMenuBackground") : null;
            GameObject bgObj;

            if (bgTransform == null && canvasParent != null)
            {
                bgObj = CreateUIObject("GlobalMenuBackground", canvasParent);
                bgObj.transform.SetAsFirstSibling();
            }
            else if (bgTransform != null)
            {
                bgObj = bgTransform.gameObject;
            }
            else
            {
                bgObj = CreateUIObject("GlobalMenuBackground", transform);
            }

            var bgRT = bgObj.GetComponent<RectTransform>();
            Stretch(bgRT);

            var bgImg = bgObj.GetComponent<Image>() ?? bgObj.AddComponent<Image>();
            if (backgroundSprite != null)
            {
                bgImg.sprite = backgroundSprite;
                bgImg.type = Image.Type.Simple;
                bgImg.preserveAspect = false;
                bgImg.color = new Color(0.95f, 0.88f, 0.75f, 1f); // Warm amber tint
            }
            else
            {
                bgImg.color = DarkBackground;
            }
            bgImg.raycastTarget = false;

            // Parallax
            var parallax = bgObj.GetComponent<ParallaxUI>() ?? bgObj.AddComponent<ParallaxUI>();
            parallax.intensity = 40f;
            parallax.smoothTime = 0.12f;

            // ─── DARK OVERLAY FOR DEPTH ───
            var overlay = CreateUIObject("DarkOverlay", transform);
            Stretch(overlay.GetComponent<RectTransform>());
            var overlayImg = overlay.AddComponent<Image>();
            overlayImg.color = new Color(0.03f, 0.02f, 0.01f, 0.55f);
            overlayImg.raycastTarget = false;

            // ─── MAIN CARD ───
            var mainCard = CreateUIObject("MainCard", transform);
            var cardRT = mainCard.GetComponent<RectTransform>();
            cardRT.anchorMin = new Vector2(0.04f, 0.04f);
            cardRT.anchorMax = new Vector2(0.96f, 0.96f);
            cardRT.pivot = new Vector2(0.5f, 0.5f);
            cardRT.sizeDelta = Vector2.zero;
            cardRT.anchoredPosition = Vector2.zero;

            var cardImg = mainCard.AddComponent<Image>();
            cardImg.color = CardBackground;
            if (roundedSprite != null) { cardImg.sprite = roundedSprite; cardImg.type = Image.Type.Sliced; }

            // Breathing border pulse
            var cardOutline = mainCard.AddComponent<Outline>();
            cardOutline.effectColor = new Color(AccentCyan.r, AccentCyan.g, AccentCyan.b, 0.25f);
            cardOutline.effectDistance = new Vector2(1.2f, -1.2f);
            var borderPulse = mainCard.AddComponent<SettingsGradientBorderPulse>();
            borderPulse.outline = cardOutline;

            // Shadow (Deep glow)
            var shadow = mainCard.AddComponent<Shadow>();
            shadow.effectColor = new Color(0, 0, 0, 0.75f);
            shadow.effectDistance = new Vector2(0, -12);

            // Glass inner overlay
            var glassOverlay = CreateUIObject("GlassOverlay", mainCard.transform);
            var glassRT = glassOverlay.GetComponent<RectTransform>();
            Stretch(glassRT);
            glassRT.offsetMin = new Vector2(2, 2);
            glassRT.offsetMax = new Vector2(-2, -2);
            var glassImg = glassOverlay.AddComponent<Image>();
            glassImg.color = GlassOverlay;
            glassImg.raycastTarget = false;

            // ─── SCROLL VIEW ───
            var mainScrollView = CreateScrollView(mainCard.transform);

            // ─── HEADER ───
            var topHeader = CreateHeader(mainCard.transform);
            topHeader.transform.SetAsLastSibling();

            // ─── SETTINGS SECTIONS ───
            var model = new SettingsModel();

            Slider musicSlider = null;
            Slider sfxSlider = null;
            Toggle musicToggle = null;
            Toggle sfxToggle = null;
            Toggle hapticToggle = null;
            TMP_Dropdown languageDD = null;
            TMP_Dropdown controlMethodDD = null;
            TMP_Dropdown accelModeDD = null;
            Slider sensitivitySlider = null;

            // 1. SİSTEM
            CreateSection(mainScrollView.transform, "SİSTEM", "Settings_System_Header", 0,
                (sectionParent) => {
                    musicToggle = CreateModernToggle(sectionParent, "Müzik", "Settings_EnableMusic", model.MusicEnabled);
                    sfxToggle = CreateModernToggle(sectionParent, "Efekt", "Settings_EnableSFX", model.SFXEnabled);
                    hapticToggle = CreateModernToggle(sectionParent, "Titreşim", "Settings_Haptic", model.HapticEnabled);
                    return new Component[] { musicToggle, sfxToggle, hapticToggle };
                }
            );

            AddDivider(mainScrollView.transform);

            // 2. SES AYARLARI
            CreateSection(mainScrollView.transform, "SES AYARLARI", "Settings_Audio_Header", 1,
                (sectionParent) => {
                    musicSlider = CreateModernSlider(sectionParent, "Müzik Ses Seviyesi", "Settings_Music", model.MusicVolume);
                    sfxSlider = CreateModernSlider(sectionParent, "Efekt Ses Seviyesi", "Settings_SFX", model.SFXVolume);
                    return new Component[] { musicSlider, sfxSlider };
                }
            );

            AddDivider(mainScrollView.transform);

            // 3. DİL SEÇİMİ
            CreateSection(mainScrollView.transform, "DİL SEÇİMİ", "Settings_Lang_Header", 2,
                (sectionParent) => {
                    languageDD = CreateModernDropdown(sectionParent, "Dil", "Settings_Language");
                    languageDD.options.Clear();
                    languageDD.options.Add(new TMP_Dropdown.OptionData("Türkçe"));
                    languageDD.options.Add(new TMP_Dropdown.OptionData("English"));
                    languageDD.value = model.LanguageIndex;
                    languageDD.RefreshShownValue();
                    return new Component[] { languageDD };
                }
            );

            AddDivider(mainScrollView.transform);

            // 4. KONTROLLER
            Button calibrateBtn = null;
            GameObject calibrationPanel = null;

            CreateSection(mainScrollView.transform, "KONTROLLER", "Settings_Control_Header", 3,
                (sectionParent) => {
                    controlMethodDD = CreateModernDropdown(sectionParent, Loc("Settings_ControlMethod", "Kontrol Tipi"), "Settings_ControlMethod");
                    controlMethodDD.options.Clear();
                    controlMethodDD.options.Add(new TMP_Dropdown.OptionData(Loc("Settings_ControlMethod_Buttons", "Butonlar")));
                    controlMethodDD.options.Add(new TMP_Dropdown.OptionData(Loc("Settings_ControlMethod_Tilt", "Tilt")));
                    controlMethodDD.value = model.ControlMethod;
                    controlMethodDD.RefreshShownValue();

                    accelModeDD = CreateModernDropdown(sectionParent, Loc("Settings_AccelMode", "İvme Modu"), "Settings_AccelMode");
                    accelModeDD.options.Clear();
                    accelModeDD.options.Add(new TMP_Dropdown.OptionData(Loc("Settings_Accel_Manual", "Manuel")));
                    accelModeDD.options.Add(new TMP_Dropdown.OptionData(Loc("Settings_Accel_Auto", "Otomatik")));
                    accelModeDD.value = model.AccelerationMode;
                    accelModeDD.RefreshShownValue();

                    sensitivitySlider = CreateModernSlider(sectionParent, "Hassasiyet", "Settings_Sensitivity", model.ControlSensitivity);
                    calibrationPanel = CreateCalibrationPanel(sectionParent, out calibrateBtn);

                    return new Component[] { controlMethodDD, accelModeDD, sensitivitySlider };
                }
            );

            // ─── FOOTER BUTTONS ───
            var (resetBtn, backBtn) = CreateFooter(mainCard.transform);

            // ─── SETUP REFERENCES ───
            SetupRefs(musicSlider, sfxSlider, musicToggle, sfxToggle, languageDD,
                     hapticToggle, resetBtn, backBtn, controlMethodDD, accelModeDD,
                     sensitivitySlider, calibrateBtn, calibrationPanel);

            // ─── FORCE LAYOUT REBUILD ───
            LayoutRebuilder.ForceRebuildLayoutImmediate(mainScrollView.GetComponent<RectTransform>());

            Debug.Log("✅ Modern Ayarlar Paneli başarıyla oluşturuldu!");
        }

        // ═══════════════════════════════════════════════
        // HEADER
        // ═══════════════════════════════════════════════
        private GameObject CreateHeader(Transform parent)
        {
            var header = CreateUIObject("Header", parent);
            var headerRT = header.GetComponent<RectTransform>();
            headerRT.anchorMin = new Vector2(0, 1);
            headerRT.anchorMax = new Vector2(1, 1);
            headerRT.pivot = new Vector2(0.5f, 1);
            headerRT.anchoredPosition = Vector2.zero;
            headerRT.sizeDelta = new Vector2(0, 110);

            var img = header.AddComponent<Image>();
            img.color = new Color(0.05f, 0.04f, 0.02f, 0.88f); // Warm header glass
            if (roundedSprite != null) { img.sprite = roundedSprite; img.type = Image.Type.Sliced; }

            // Title
            var title = AddText(header.transform, "Title",
                Loc("Menu_Settings", "AYARLAR"),
                58, FontStyles.Bold, TextPrimary,
                new Vector2(0, 12), new Vector2(600, 80), "Menu_Settings", headerFont);
            title.characterSpacing = 12; // Wider spacing for premium look
            title.alignment = TextAlignmentOptions.Center;

            // Subtitle
            var subtitle = AddText(header.transform, "Subtitle",
                Loc("Settings_Subtitle", "Oyun tercihlerinizi özelleştirin"),
                15, FontStyles.Italic, TextSecondary,
                new Vector2(0, -32), new Vector2(500, 28), null, labelFont);
            subtitle.characterSpacing = 2;
            subtitle.alignment = TextAlignmentOptions.Center;

            // Bottom accent line (cyan→violet gradient feel via dual lines)
            var accentLineLeft = CreateUIObject("AccentLineLeft", header.transform);
            var alRT = accentLineLeft.GetComponent<RectTransform>();
            alRT.anchorMin = new Vector2(0.1f, 0);
            alRT.anchorMax = new Vector2(0.5f, 0);
            alRT.pivot = new Vector2(0.5f, 0);
            alRT.anchoredPosition = new Vector2(0, 2);
            alRT.sizeDelta = new Vector2(0, 3f);
            var alImg = accentLineLeft.AddComponent<Image>();
            alImg.color = AccentCyan;

            var accentLineRight = CreateUIObject("AccentLineRight", header.transform);
            var arRT = accentLineRight.GetComponent<RectTransform>();
            arRT.anchorMin = new Vector2(0.5f, 0);
            arRT.anchorMax = new Vector2(0.95f, 0);
            arRT.pivot = new Vector2(0.5f, 0);
            arRT.anchoredPosition = new Vector2(0, 2);
            arRT.sizeDelta = new Vector2(0, 3f);
            var arImg = accentLineRight.AddComponent<Image>();
            arImg.color = AccentViolet;

            return header;
        }

        // ═══════════════════════════════════════════════
        // SCROLL VIEW
        // ═══════════════════════════════════════════════
        private GameObject CreateScrollView(Transform parent)
        {
            var scrollContainer = CreateUIObject("ScrollContainer", parent);
            var scrollRT = scrollContainer.GetComponent<RectTransform>();
            scrollRT.anchorMin = Vector2.zero;
            scrollRT.anchorMax = Vector2.one;
            scrollRT.sizeDelta = Vector2.zero;
            scrollRT.anchoredPosition = Vector2.zero;

            var scrollRect = scrollContainer.AddComponent<ScrollRect>();
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            scrollRect.scrollSensitivity = 60f;
            scrollRect.inertia = true;
            scrollRect.decelerationRate = 0.135f;
            scrollRect.movementType = ScrollRect.MovementType.Clamped;

            var viewport = CreateUIObject("Viewport", scrollContainer.transform);
            var viewportRT = viewport.GetComponent<RectTransform>();
            viewportRT.anchorMin = Vector2.zero;
            viewportRT.anchorMax = Vector2.one;
            viewportRT.pivot = new Vector2(0.5f, 1);
            viewportRT.offsetMin = new Vector2(30, 105);
            viewportRT.offsetMax = new Vector2(-30, -125);

            viewport.AddComponent<Image>().color = new Color(0, 0, 0, 0.01f);
            viewport.AddComponent<Mask>().showMaskGraphic = false;

            var content = CreateUIObject("Content", viewport.transform);
            var contentRT = content.GetComponent<RectTransform>();
            contentRT.anchorMin = new Vector2(0, 1);
            contentRT.anchorMax = new Vector2(1, 1);
            contentRT.pivot = new Vector2(0.5f, 1);
            contentRT.anchoredPosition = Vector2.zero;
            contentRT.sizeDelta = new Vector2(0, 0);

            var vlg = content.AddComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(30, 30, 30, 20);
            vlg.spacing = 20; // Bölümler arası boşluğu biraz artıralım
            vlg.childAlignment = TextAnchor.UpperCenter;
            vlg.childControlHeight = true; // Kritik Fix: Bölümlerin yüksekliklerini kontrol etmeli
            vlg.childForceExpandHeight = false;
            vlg.childControlWidth = true;
            vlg.childForceExpandWidth = true;

            var csf = content.AddComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            scrollRect.viewport = viewport.GetComponent<RectTransform>();
            scrollRect.content = contentRT;

            return content;
        }

        // ═══════════════════════════════════════════════
        // SECTION
        // ═══════════════════════════════════════════════
        private void CreateSection(Transform parent, string titleTR, string titleEN, int sectionIndex, System.Func<Transform, Component[]> createControls)
        {
            var section = CreateUIObject("Section_" + titleEN, parent);

            var vlg = section.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 8;
            vlg.padding = new RectOffset(0, 0, 0, 8);
            vlg.childAlignment = TextAnchor.UpperLeft;
            vlg.childControlHeight = true;
            vlg.childForceExpandHeight = false;
            vlg.childControlWidth = true;
            vlg.childForceExpandWidth = true;

            var csf = section.AddComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            // Section Header
            var sectionHeader = CreateUIObject("SectionHeader", section.transform);
            var headerRT = sectionHeader.GetComponent<RectTransform>();
            headerRT.sizeDelta = new Vector2(0, 42);
            var headerLE = sectionHeader.AddComponent<LayoutElement>();
            headerLE.preferredHeight = 42;

            var headerBg = sectionHeader.AddComponent<Image>();
            headerBg.color = new Color(0.08f, 0.06f, 0.03f, 0.65f);
            if (roundedSprite != null) { headerBg.sprite = roundedSprite; headerBg.type = Image.Type.Sliced; }

            // Accent line (left) — alternate cyan/violet per section
            var accentLine = CreateUIObject("AccentLine", sectionHeader.transform);
            var lineRT = accentLine.GetComponent<RectTransform>();
            lineRT.anchorMin = new Vector2(0, 0.1f);
            lineRT.anchorMax = new Vector2(0, 0.9f);
            lineRT.pivot = new Vector2(0, 0.5f);
            lineRT.sizeDelta = new Vector2(5, 0);
            lineRT.anchoredPosition = new Vector2(2, 0);
            var lineImg = accentLine.AddComponent<Image>();
            lineImg.color = (sectionIndex % 2 == 0) ? AccentCyan : AccentViolet;

            // Title
            var sectionTitle = AddText(sectionHeader.transform, "Title", titleTR,
                22, FontStyles.Bold, TextPrimary,
                Vector2.zero, new Vector2(0, 42), titleEN, headerFont);
            sectionTitle.alignment = TextAlignmentOptions.MidlineLeft;
            var titleRT = sectionTitle.GetComponent<RectTransform>();
            titleRT.anchorMin = Vector2.zero;
            titleRT.anchorMax = Vector2.one;
            titleRT.offsetMin = new Vector2(28, 0);
            titleRT.offsetMax = new Vector2(-20, 0);
            sectionTitle.characterSpacing = 3;

            // Staggered entrance animation (alpha handled at runtime by SettingsSectionEntrance)
            section.AddComponent<CanvasGroup>().alpha = 1f;
            var entrance = section.AddComponent<SettingsSectionEntrance>();
            entrance.delay = sectionIndex * 0.08f;

            // Create Controls
            createControls(section.transform);
            
            // Force recalculate height so parent VerticalLayoutGroup sees it
            LayoutRebuilder.ForceRebuildLayoutImmediate(section.GetComponent<RectTransform>());
        }

        // ═══════════════════════════════════════════════
        // MODERN SLIDER
        // ═══════════════════════════════════════════════
        private Slider CreateModernSlider(Transform parent, string label, string locKey, float value)
        {
            Color rowColor = (_rowIndex++ % 2 == 0) ? RowBgA : RowBgB;

            var container = CreateUIObject("Slider_" + locKey, parent);
            var containerRT = container.GetComponent<RectTransform>();
            containerRT.sizeDelta = new Vector2(0, 84);
            var layoutElement = container.AddComponent<LayoutElement>();
            layoutElement.minHeight = 84;
            layoutElement.preferredHeight = 84;

            var hlg = container.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 24;
            hlg.padding = new RectOffset(20, 20, 10, 10);
            hlg.childAlignment = TextAnchor.MiddleLeft;
            hlg.childControlWidth = false;
            hlg.childControlHeight = false;
            hlg.childForceExpandWidth = false;

            var bg = container.AddComponent<Image>();
            bg.color = rowColor;
            if (roundedSprite != null) { bg.sprite = roundedSprite; bg.type = Image.Type.Sliced; }

            // Accent Bar
            var accentBar = CreateUIObject("AccentBar", container.transform);
            var barRT = accentBar.GetComponent<RectTransform>();
            barRT.sizeDelta = new Vector2(3, 22);
            var barImg = accentBar.AddComponent<Image>();
            barImg.color = AccentCyan;
            if (roundedSprite != null) { barImg.sprite = roundedSprite; barImg.type = Image.Type.Sliced; }

            // Label Section
            var labelObj = CreateUIObject("LabelSection", container.transform);
            var labelRT = labelObj.GetComponent<RectTransform>();
            labelRT.sizeDelta = new Vector2(340, 56);

            var labelVlg = labelObj.AddComponent<VerticalLayoutGroup>();
            labelVlg.spacing = 3;
            labelVlg.childAlignment = TextAnchor.MiddleLeft;
            labelVlg.childControlWidth = true;
            labelVlg.childForceExpandWidth = true;

            var labelText = AddText(labelObj.transform, "Label", label,
                19, FontStyles.Bold, TextPrimary,
                Vector2.zero, new Vector2(340, 28), locKey, labelFont);
            labelText.alignment = TextAlignmentOptions.MidlineLeft;

            var valueText = AddText(labelObj.transform, "Value", Mathf.RoundToInt(value * 100f) + "%",
                14, FontStyles.Normal, TextMuted,
                Vector2.zero, new Vector2(340, 18), null, valueFont);
            valueText.alignment = TextAlignmentOptions.MidlineLeft;
            valueText.raycastTarget = false;

            // Flexible Spacer
            var spacer = CreateUIObject("FlexibleSpacer", container.transform);
            spacer.AddComponent<LayoutElement>().flexibleWidth = 1f;

            // ─── PREMIUM SCIFI SLIDER ───
            var sliderObj = CreateUIObject("Slider", container.transform);
            var sliderRT = sliderObj.GetComponent<RectTransform>();
            sliderRT.sizeDelta = new Vector2(400, 46);

            var slider = sliderObj.AddComponent<Slider>();
            slider.transition = Selectable.Transition.None;

            // Background Track (Hollow Tech Frame)
            var sliderBg = CreateUIObject("Background", sliderObj.transform);
            var bgSliderRT = sliderBg.GetComponent<RectTransform>();
            bgSliderRT.anchorMin = new Vector2(0, 0.5f);
            bgSliderRT.anchorMax = new Vector2(1, 0.5f);
            bgSliderRT.pivot = new Vector2(0.5f, 0.5f);
            bgSliderRT.offsetMin = bgSliderRT.offsetMax = Vector2.zero;
            bgSliderRT.sizeDelta = new Vector2(0, 4); // Thinner track
            
            var bgSliderImg = sliderBg.AddComponent<Image>();
            bgSliderImg.color = new Color(0.04f, 0.03f, 0.01f, 0.98f);
            if (roundedSprite != null) { bgSliderImg.sprite = roundedSprite; bgSliderImg.type = Image.Type.Sliced; }
            
            var bgOutline = sliderBg.AddComponent<Outline>();
            bgOutline.effectColor = new Color(AccentCyan.r, AccentCyan.g, AccentCyan.b, 0.15f);
            bgOutline.effectDistance = new Vector2(1f, -1f);

            // Fill Area
            var fillArea = CreateUIObject("Fill Area", sliderObj.transform);
            var fillAreaRT = fillArea.GetComponent<RectTransform>();
            fillAreaRT.anchorMin = new Vector2(0, 0.5f);
            fillAreaRT.anchorMax = new Vector2(1, 0.5f);
            fillAreaRT.pivot = new Vector2(0.5f, 0.5f);
            fillAreaRT.offsetMin = fillAreaRT.offsetMax = Vector2.zero;
            fillAreaRT.sizeDelta = new Vector2(0, 4); // Match track

            var fill = CreateUIObject("Fill", fillArea.transform);
            var fillRT = fill.GetComponent<RectTransform>();
            fillRT.anchorMin = Vector2.zero;
            fillRT.anchorMax = Vector2.one;
            fillRT.offsetMin = fillRT.offsetMax = Vector2.zero;
            var fillImg = fill.AddComponent<Image>();
            fillImg.color = AccentCyan;
            if (roundedSprite != null) { fillImg.sprite = roundedSprite; fillImg.type = Image.Type.Sliced; }

            // Handle Area
            var handleArea = CreateUIObject("Handle Area", sliderObj.transform);
            var haRT = handleArea.GetComponent<RectTransform>();
            haRT.anchorMin = new Vector2(0, 0.5f);
            haRT.anchorMax = new Vector2(1, 0.5f);
            haRT.pivot = new Vector2(0.5f, 0.5f);
            haRT.offsetMin = new Vector2(15, 0);
            haRT.offsetMax = new Vector2(-15, 0);
            haRT.sizeDelta = new Vector2(0, 24);

            var handle = CreateUIObject("Handle", handleArea.transform);
            var handleRT = handle.GetComponent<RectTransform>();
            handleRT.anchorMin = new Vector2(0.5f, 0);
            handleRT.anchorMax = new Vector2(0.5f, 1);
            handleRT.sizeDelta = new Vector2(16, 0); // Thinner, tech-style vertical grip
            
            var handleImg = handle.AddComponent<Image>();
            handleImg.color = new Color(0.08f, 0.06f, 0.03f, 1f); // Warm dark handle
            if (roundedSprite != null) { handleImg.sprite = roundedSprite; handleImg.type = Image.Type.Simple; }

            var handleOutline = handle.AddComponent<Outline>();
            handleOutline.effectColor = AccentCyan;
            handleOutline.effectDistance = new Vector2(1.5f, -1.5f);

            // ─── Fluid Micro-Animation on Handle ───
            var fluidAnim = handle.AddComponent<SettingsFluidButtonAnimator>();
            fluidAnim.targetGraphic = handleImg;
            fluidAnim.targetOutline = handleOutline;
            fluidAnim.normalColor = new Color(0.08f, 0.06f, 0.03f, 1f);
            fluidAnim.normalOutline = AccentCyan;
            
            fluidAnim.hoverColor = new Color(AccentCyan.r * 0.3f, AccentCyan.g * 0.3f, AccentCyan.b * 0.3f, 1f);
            fluidAnim.hoverOutline = Color.white;
            
            fluidAnim.pressColor = AccentCyan;
            fluidAnim.pressOutline = new Color(1f, 1f, 1f, 0.5f);

            // Assign references
            slider.fillRect = fillRT;
            slider.handleRect = handleRT;
            slider.targetGraphic = handleImg;
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.value = value;
            slider.wholeNumbers = false;

            // Value text binder
            var binder = valueText.gameObject.AddComponent<UISliderValueBinder>();
            binder.format = "{0}%";
            binder.multiplier = 100f;

            return slider;
        }

        // ═══════════════════════════════════════════════
        // MODERN TOGGLE
        // ═══════════════════════════════════════════════
        private Toggle CreateModernToggle(Transform parent, string label, string locKey, bool value)
        {
            Color rowColor = (_rowIndex++ % 2 == 0) ? RowBgA : RowBgB;

            var container = CreateUIObject("Toggle_" + locKey, parent);

            var layoutElement = container.AddComponent<LayoutElement>();
            layoutElement.minHeight = 72;
            layoutElement.preferredHeight = 72;
            layoutElement.flexibleWidth = 1f;

            var hlg = container.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 20;
            hlg.padding = new RectOffset(20, 20, 12, 12);
            hlg.childAlignment = TextAnchor.MiddleLeft;
            hlg.childControlWidth = true; // MUST be true for LayoutElements (flexibleWidth) to work!
            hlg.childForceExpandWidth = false;

            var bg = container.AddComponent<Image>();
            bg.color = rowColor;
            if (roundedSprite != null) { bg.sprite = roundedSprite; bg.type = Image.Type.Sliced; }

            // Accent Bar (Fix squishing bug by defining exact layout!)
            var accentBar = CreateUIObject("AccentBar", container.transform);
            var barRT = accentBar.GetComponent<RectTransform>();
            barRT.sizeDelta = new Vector2(4, 44);
            var barLE = accentBar.AddComponent<LayoutElement>();
            barLE.minWidth = 4;
            barLE.preferredWidth = 4;
            barLE.preferredHeight = 44;
            
            var barImg = accentBar.AddComponent<Image>();
            barImg.color = value ? AccentCyan : new Color(0.2f, 0.2f, 0.25f);
            if (roundedSprite != null) { barImg.sprite = roundedSprite; barImg.type = Image.Type.Sliced; }

            // Label
            var labelObj = CreateUIObject("Label", container.transform);
            var labelLE = labelObj.AddComponent<LayoutElement>();
            labelLE.minWidth = 260; // Better label width

            var labelText = AddText(labelObj.transform, "Text", label,
                19, FontStyles.Bold, TextPrimary,
                Vector2.zero, new Vector2(260, 36), locKey, labelFont);
            labelText.alignment = TextAlignmentOptions.MidlineLeft;
            labelText.textWrappingMode = TextWrappingModes.NoWrap;

            // Flexible Spacer
            var spacerObj = CreateUIObject("Spacer", container.transform);
            spacerObj.AddComponent<LayoutElement>().flexibleWidth = 1f;

            // ─── PREMIUM HOLLOW TOGGLE ───
            var toggleCont = CreateUIObject("ToggleContainer", container.transform);
            var toggleContRT = toggleCont.GetComponent<RectTransform>();
            toggleContRT.sizeDelta = new Vector2(64, 28);
            var toggleContLE = toggleCont.AddComponent<LayoutElement>();
            toggleContLE.minWidth = 64;
            toggleContLE.preferredWidth = 64;

            var toggleBgObj = CreateUIObject("Background", toggleCont.transform);
            Stretch(toggleBgObj.GetComponent<RectTransform>());

            var toggleBgImg = toggleBgObj.AddComponent<Image>();
            toggleBgImg.color = new Color(0.05f, 0.04f, 0.02f, 0.8f); // Warm dark track
            if (roundedSprite != null) { toggleBgImg.sprite = roundedSprite; toggleBgImg.type = Image.Type.Sliced; }
            
            // Neon Outline for the Track
            var trackOutline = toggleBgObj.AddComponent<Outline>();
            trackOutline.effectColor = value ? new Color(AccentCyan.r, AccentCyan.g, AccentCyan.b, 0.4f) : new Color(1, 1, 1, 0.1f);
            trackOutline.effectDistance = new Vector2(1.5f, -1.5f);

            var toggle = toggleBgObj.AddComponent<Toggle>();
            toggle.isOn = value;
            toggle.transition = Selectable.Transition.None;

            // Handle block
            var checkmark = CreateUIObject("Checkmark", toggleBgObj.transform);
            var checkRT = checkmark.GetComponent<RectTransform>();
            checkRT.anchorMin = new Vector2(0, 0.5f);
            checkRT.anchorMax = new Vector2(0, 0.5f);
            checkRT.pivot = new Vector2(0.5f, 0.5f);
            checkRT.anchoredPosition = value ? new Vector2(48, 0) : new Vector2(16, 0);
            checkRT.sizeDelta = new Vector2(24, 20); // Sharp tech block thumb

            var checkImg = checkmark.AddComponent<Image>();
            checkImg.color = value ? AccentCyan : new Color(0.6f, 0.6f, 0.6f);
            if (roundedSprite != null) { checkImg.sprite = roundedSprite; checkImg.type = Image.Type.Simple; }

            // Add glow to handle when active
            var checkOutline = checkmark.AddComponent<Outline>();
            checkOutline.effectColor = value ? new Color(1f, 1f, 1f, 0.4f) : new Color(0f, 0f, 0f, 0f);
            checkOutline.effectDistance = new Vector2(1f, -1f);

            toggle.targetGraphic = toggleBgImg;
            toggle.graphic = null;

            // Binder for smooth toggle animation
            var binder = toggle.gameObject.AddComponent<UIToggleValueBinder>();
            binder.handleRect = checkRT;
            binder.backgroundImage = toggleBgImg;
            binder.accentBarImage = barImg;
            binder.activeColor = new Color(0.15f, 0.12f, 0.04f, 0.9f); // Warm gold tinted bg
            binder.inactiveColor = new Color(0.05f, 0.04f, 0.02f, 0.8f);
            binder.activeBarColor = AccentCyan;
            binder.inactiveBarColor = new Color(0.2f, 0.2f, 0.25f);
            binder.activePos = new Vector2(48, 0);
            binder.inactivePos = new Vector2(16, 0);

            // Smooth toggle animator (iOS-style slide, customized for Cyber theme)
            var smoothAnim = toggle.gameObject.AddComponent<SettingsSmoothToggleAnimator>();
            smoothAnim.handle = checkRT;
            smoothAnim.bgImage = toggleBgImg;
            smoothAnim.onColor = new Color(0.15f, 0.12f, 0.04f, 0.9f);
            smoothAnim.offColor = new Color(0.05f, 0.04f, 0.02f, 0.8f);
            smoothAnim.onX = 48f;
            smoothAnim.offX = 16f;

            // To animate outline and handle color, we hook the Unity event
            toggle.onValueChanged.AddListener((isOn) => {
                trackOutline.effectColor = isOn ? new Color(AccentCyan.r, AccentCyan.g, AccentCyan.b, 0.4f) : new Color(1, 1, 1, 0.1f);
                checkImg.color = isOn ? AccentCyan : new Color(0.5f, 0.5f, 0.55f);
                checkOutline.effectColor = isOn ? new Color(1f, 1f, 1f, 0.4f) : new Color(0f, 0f, 0f, 0f);
            });

            return toggle;
        }

        // ═══════════════════════════════════════════════
        // MODERN DROPDOWN
        // ═══════════════════════════════════════════════
        private TMP_Dropdown CreateModernDropdown(Transform parent, string label, string locKey)
        {
            Color rowColor = (_rowIndex++ % 2 == 0) ? RowBgA : RowBgB;

            var container = CreateUIObject("Dropdown_" + locKey, parent);
            var containerRT = container.GetComponent<RectTransform>();
            containerRT.sizeDelta = new Vector2(0, 80);

            var layoutElement = container.AddComponent<LayoutElement>();
            layoutElement.minHeight = 80;
            layoutElement.preferredHeight = 80;

            var hlg = container.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 20;
            hlg.padding = new RectOffset(22, 22, 12, 12);
            hlg.childAlignment = TextAnchor.MiddleLeft;
            hlg.childControlWidth = false;
            hlg.childControlHeight = false;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = false;

            var bg = container.AddComponent<Image>();
            bg.color = rowColor;
            if (roundedSprite != null) { bg.sprite = roundedSprite; bg.type = Image.Type.Sliced; }

            // Accent Bar
            var accentBar = CreateUIObject("AccentBar", container.transform);
            var barRT = accentBar.GetComponent<RectTransform>();
            barRT.sizeDelta = new Vector2(4, 44);
            var barImg = accentBar.AddComponent<Image>();
            barImg.color = AccentViolet;
            if (roundedSprite != null) { barImg.sprite = roundedSprite; barImg.type = Image.Type.Sliced; }

            // Label
            var labelObj = CreateUIObject("Label", container.transform);
            var labelObjRT = labelObj.GetComponent<RectTransform>();
            labelObjRT.sizeDelta = new Vector2(340, 38);

            var labelText = AddText(labelObj.transform, "Text", label,
                19, FontStyles.Bold, TextPrimary,
                Vector2.zero, new Vector2(340, 38), locKey, labelFont);
            labelText.alignment = TextAlignmentOptions.MidlineLeft;

            // Flexible Spacer
            var spacer = CreateUIObject("FlexibleSpacer", container.transform);
            spacer.AddComponent<LayoutElement>().flexibleWidth = 1f;

            // ─── PREMIUM DROPDOWN BASE ───
            var ddObj = CreateUIObject("DropdownControl", container.transform);
            var ddRT = ddObj.GetComponent<RectTransform>();
            ddRT.sizeDelta = new Vector2(380, 46);

            var dd = ddObj.AddComponent<TMP_Dropdown>();

            var ddBg = ddObj.AddComponent<Image>();
            Color normalBg = new Color(0.06f, 0.05f, 0.03f, 0.95f);
            ddBg.color = normalBg;
            if (roundedSprite != null) { ddBg.sprite = roundedSprite; ddBg.type = Image.Type.Sliced; }

            // Clean Glowing Outline
            var ddOutline = ddObj.AddComponent<Outline>();
            Color normalOut = new Color(AccentCyan.r, AccentCyan.g, AccentCyan.b, 0.45f);
            ddOutline.effectColor = normalOut;
            ddOutline.effectDistance = new Vector2(1.5f, -1.5f);

            // Add fluid micro-animation
            var fluidAnim = ddObj.AddComponent<SettingsFluidButtonAnimator>();
            fluidAnim.targetGraphic = ddBg;
            fluidAnim.targetOutline = ddOutline;
            fluidAnim.normalColor = normalBg;
            fluidAnim.normalOutline = normalOut;
            fluidAnim.hoverColor = new Color(AccentCyan.r * 0.4f, AccentCyan.g * 0.4f, AccentCyan.b * 0.4f, 0.8f);
            fluidAnim.pressColor = normalBg;
            fluidAnim.hoverOutline = new Color(AccentCyan.r, AccentCyan.g, AccentCyan.b, 1f);
            fluidAnim.pressOutline = new Color(AccentCyan.r, AccentCyan.g, AccentCyan.b, 0.2f);

            // Selected text
            var ddLabel = CreateUIObject("Label", ddObj.transform);
            var ddLabelRT = ddLabel.GetComponent<RectTransform>();
            ddLabelRT.anchorMin = Vector2.zero;
            ddLabelRT.anchorMax = Vector2.one;
            ddLabelRT.offsetMin = new Vector2(20, 0); // Spaced from left
            ddLabelRT.offsetMax = new Vector2(-40, 0);

            var ddLabelText = ddLabel.AddComponent<TextMeshProUGUI>();
            ddLabelText.font = valueFont ?? customFont;
            ddLabelText.fontSize = 17;
            ddLabelText.color = Color.white;
            ddLabelText.alignment = TextAlignmentOptions.MidlineLeft;
            
            // Arrow
            var arrow = CreateUIObject("Arrow", ddObj.transform);
            var arrowRT = arrow.GetComponent<RectTransform>();
            arrowRT.anchorMin = new Vector2(1, 0.5f);
            arrowRT.anchorMax = new Vector2(1, 0.5f);
            arrowRT.pivot = new Vector2(1, 0.5f);
            arrowRT.anchoredPosition = new Vector2(-15, 0);
            arrowRT.sizeDelta = new Vector2(24, 24);

            var arrowText = arrow.AddComponent<TextMeshProUGUI>();
            arrowText.font = valueFont ?? customFont;
            arrowText.text = "▼";
            arrowText.fontSize = 16;
            arrowText.color = AccentCyan;
            arrowText.alignment = TextAlignmentOptions.Center;
            arrowText.raycastTarget = false;

            dd.targetGraphic = ddBg;
            dd.captionText = ddLabelText;

            // ─── DROPDOWN TEMPLATE (LIST) ───
            var template = CreateUIObject("Template", ddObj.transform);
            template.SetActive(false);
            var tempRT = template.GetComponent<RectTransform>();
            tempRT.anchorMin = new Vector2(0, 0);
            tempRT.anchorMax = new Vector2(1, 0);
            tempRT.pivot = new Vector2(0.5f, 1);
            tempRT.anchoredPosition = new Vector2(0, -6); // Gap
            tempRT.sizeDelta = new Vector2(0, 240);

            var tempBg = template.AddComponent<Image>();
            tempBg.color = new Color(0.05f, 0.04f, 0.02f, 0.98f);
            if (roundedSprite != null) { tempBg.sprite = roundedSprite; tempBg.type = Image.Type.Sliced; }
            
            var tempOutline = template.AddComponent<Outline>();
            tempOutline.effectColor = new Color(AccentCyan.r, AccentCyan.g, AccentCyan.b, 0.5f);
            tempOutline.effectDistance = new Vector2(1.5f, -1.5f);

            var scrollRect = template.AddComponent<ScrollRect>();
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            scrollRect.movementType = ScrollRect.MovementType.Clamped;

            var viewport = CreateUIObject("Viewport", template.transform);
            Stretch(viewport.GetComponent<RectTransform>());
            viewport.AddComponent<RectMask2D>();

            var content = CreateUIObject("Content", viewport.transform);
            var contentRT = content.GetComponent<RectTransform>();
            contentRT.anchorMin = new Vector2(0, 1);
            contentRT.anchorMax = new Vector2(1, 1);
            contentRT.pivot = new Vector2(0.5f, 1);
            contentRT.sizeDelta = new Vector2(0, 0);

            var contentVlg = content.AddComponent<VerticalLayoutGroup>();
            contentVlg.padding = new RectOffset(6, 6, 6, 6);
            contentVlg.spacing = 4;
            contentVlg.childAlignment = TextAnchor.UpperCenter;
            contentVlg.childControlWidth = true;
            contentVlg.childForceExpandWidth = true;
            contentVlg.childControlHeight = false;
            contentVlg.childForceExpandHeight = false;

            var contentCsf = content.AddComponent<ContentSizeFitter>();
            contentCsf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            scrollRect.viewport = viewport.GetComponent<RectTransform>();
            scrollRect.content = contentRT;

            // ─── DROPDOWN ITEM PREFAB ───
            var item = CreateUIObject("Item", content.transform);
            var itemRT = item.GetComponent<RectTransform>();
            itemRT.sizeDelta = new Vector2(0, 48);

            var itemLE = item.AddComponent<LayoutElement>();
            itemLE.minHeight = 48;
            itemLE.preferredHeight = 48;

            var itemToggle = item.AddComponent<Toggle>();
            var itemBg = item.AddComponent<Image>();
            itemBg.color = new Color(0, 0, 0, 0);
            if (roundedSprite != null) { itemBg.sprite = roundedSprite; itemBg.type = Image.Type.Sliced; }

            // Proper premium colors for list items
            var colors = itemToggle.colors;
            colors.normalColor = new Color(0, 0, 0, 0);
            colors.highlightedColor = new Color(AccentCyan.r, AccentCyan.g, AccentCyan.b, 0.35f);
            colors.pressedColor = new Color(AccentCyan.r, AccentCyan.g, AccentCyan.b, 0.5f);
            colors.selectedColor = new Color(AccentCyan.r, AccentCyan.g, AccentCyan.b, 0.15f);
            colors.colorMultiplier = 1f;
            colors.fadeDuration = 0.15f;
            itemToggle.colors = colors;

            var itemLabel = CreateUIObject("ItemLabel", item.transform);
            Stretch(itemLabel.GetComponent<RectTransform>());
            itemLabel.GetComponent<RectTransform>().offsetMin = new Vector2(15, 0);

            var itemText = itemLabel.AddComponent<TextMeshProUGUI>();
            itemText.font = valueFont ?? customFont;
            itemText.fontSize = 16;
            itemText.color = new Color(0.9f, 0.9f, 0.95f, 1f);
            itemText.alignment = TextAlignmentOptions.MidlineLeft;

            dd.template = tempRT;
            dd.itemText = itemText;
            dd.targetGraphic = itemBg; // This allows items to be tinted


            return dd;
        }

        // ═══════════════════════════════════════════════
        // FOOTER
        // ═══════════════════════════════════════════════
        private (Button resetBtn, Button backBtn) CreateFooter(Transform parent)
        {
            var footer = CreateUIObject("Footer", parent);
            var footerRT = footer.GetComponent<RectTransform>();
            footerRT.anchorMin = new Vector2(0, 0);
            footerRT.anchorMax = new Vector2(1, 0);
            footerRT.pivot = new Vector2(0.5f, 0);
            footerRT.anchoredPosition = Vector2.zero;
            footerRT.sizeDelta = new Vector2(0, 95);

            var hlg = footer.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 30;
            hlg.padding = new RectOffset(45, 45, 0, 25);
            hlg.childAlignment = TextAnchor.LowerRight;
            hlg.childControlWidth = false; // Fixed width
            hlg.childForceExpandWidth = false;

            // Animate Footer Entrance
            footer.AddComponent<CanvasGroup>().alpha = 1f;
            var footerEntrance = footer.AddComponent<SettingsSectionEntrance>();
            footerEntrance.delay = 0.45f;

            // Ghost Button
            var resetBtn = CreatePremiumButton(footer.transform, "Tüm İlerlemeyi Sıfırla",
                DangerRed, true, "Settings_ResetProgress");

            // Empty space pushes reset to the far left
            var spacer = CreateUIObject("Spacer", footer.transform);
            var spacerLE = spacer.AddComponent<LayoutElement>();
            spacerLE.flexibleWidth = 1;

            // Solid Button
            var backBtn = CreatePremiumButton(footer.transform, "Geri Dön",
                AccentCyan, false, "Settings_Back");

            return (resetBtn, backBtn);
        }

        // ═══════════════════════════════════════════════
        // ULTRA PREMIUM BUTTON (Ghost or Solid)
        // ═══════════════════════════════════════════════
        private Button CreatePremiumButton(Transform parent, string label, Color themeColor, bool isGhost, string locKey = null)
        {
            var btnObj = CreateUIObject("Button_" + label, parent);
            var btnRT = btnObj.GetComponent<RectTransform>();
            btnRT.sizeDelta = new Vector2(isGhost ? 280 : 250, 50);

            var btnImg = btnObj.AddComponent<Image>();
            
            // Solid button gets full theme color, Ghost gets ultra-dark transparent 
            Color normalBg = isGhost ? new Color(0.08f, 0.06f, 0.04f, 0.95f) : themeColor;
            btnImg.color = normalBg;
            if (roundedSprite != null) { btnImg.sprite = roundedSprite; btnImg.type = Image.Type.Sliced; }

            // Clean Glowing Outline (No messy shadows)
            var btnOutline = btnObj.AddComponent<Outline>();
            Color normalOut = isGhost ? new Color(themeColor.r, themeColor.g, themeColor.b, 0.35f) : new Color(0f, 0f, 0f, 0.2f);
            btnOutline.effectColor = normalOut;
            btnOutline.effectDistance = new Vector2(1.5f, -1.5f);

            var labelObj = CreateUIObject("Label", btnObj.transform);
            Stretch(labelObj.GetComponent<RectTransform>());

            var labelText = AddText(labelObj.transform, "Text", label,
                18, FontStyles.Bold, isGhost ? themeColor : new Color(0.01f, 0.01f, 0.02f, 1f),
                Vector2.zero, new Vector2(240, 36), locKey, buttonFont);
           
            labelText.alignment = TextAlignmentOptions.Center;
            labelText.characterSpacing = 4f; // More impact

            var btn = btnObj.AddComponent<Button>();
            btn.targetGraphic = btnImg;
            btn.transition = Selectable.Transition.None;

            // Micro-animation with clean color setup
            var fluidAnim = btnObj.AddComponent<SettingsFluidButtonAnimator>();
            fluidAnim.targetGraphic = btnImg;
            fluidAnim.targetOutline = btnOutline;

            // Normal states
            fluidAnim.normalColor = normalBg;
            fluidAnim.normalOutline = normalOut;
            
            if (isGhost)
            {
                // Hovering ghost button illuminates it
                fluidAnim.hoverColor = new Color(themeColor.r * 0.4f, themeColor.g * 0.4f, themeColor.b * 0.4f, 0.8f);
                fluidAnim.pressColor = normalBg;
                fluidAnim.hoverOutline = new Color(themeColor.r, themeColor.g, themeColor.b, 1f);
                fluidAnim.pressOutline = new Color(themeColor.r, themeColor.g, themeColor.b, 0.1f);
            }
            else
            {
                // Hovering solid button makes it pop bright
                fluidAnim.hoverColor = Color.Lerp(themeColor, Color.white, 0.25f);
                fluidAnim.pressColor = Color.Lerp(themeColor, Color.black, 0.3f);
                fluidAnim.hoverOutline = new Color(1, 1, 1, 0.4f);
                fluidAnim.pressOutline = new Color(0, 0, 0, 0.4f);
            }

            return btn;
        }

        // ═══════════════════════════════════════════════
        // CALIBRATION PANEL
        // ═══════════════════════════════════════════════
        private GameObject CreateCalibrationPanel(Transform parent, out Button calibrateButton)
        {
            var panel = CreateUIObject("CalibrationPanel", parent);
            var panelRT = panel.GetComponent<RectTransform>();
            panelRT.sizeDelta = new Vector2(0, 115);

            var layoutElement = panel.AddComponent<LayoutElement>();
            layoutElement.minHeight = 115;
            layoutElement.preferredHeight = 115;

            var panelBg = panel.AddComponent<Image>();
            panelBg.color = new Color(0.20f, 0.16f, 0.08f, 0.75f);
            if (roundedSprite != null) { panelBg.sprite = roundedSprite; panelBg.type = Image.Type.Sliced; }

            var vlg = panel.AddComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(22, 22, 16, 16);
            vlg.spacing = 10;
            vlg.childAlignment = TextAnchor.MiddleCenter;
            vlg.childControlWidth = false;
            vlg.childForceExpandWidth = false;
            vlg.childControlHeight = false;

            var infoText = AddText(panel.transform, "InfoText",
                Loc("Settings_Calibrate_Info", "Cihazı düz tutun ve kalibrasyon butonuna basın"),
                14, FontStyles.Normal, TextSecondary,
                Vector2.zero, new Vector2(700, 30), null, labelFont);
            infoText.alignment = TextAlignmentOptions.Center;

            var btnObj = CreateUIObject("CalibrateButton", panel.transform);
            var btnRT = btnObj.GetComponent<RectTransform>();
            btnRT.sizeDelta = new Vector2(240, 46);

            var btnImg = btnObj.AddComponent<Image>();
            btnImg.color = AccentViolet;
            if (roundedSprite != null) { btnImg.sprite = roundedSprite; btnImg.type = Image.Type.Sliced; }

            var btnLabel = AddText(btnObj.transform, "Label", Loc("Settings_Calibrate_Btn", "Kalibre Et"),
                15, FontStyles.Bold, Color.white,
                Vector2.zero, new Vector2(240, 46), null, buttonFont);
            btnLabel.alignment = TextAlignmentOptions.Center;

            calibrateButton = btnObj.AddComponent<Button>();
            calibrateButton.targetGraphic = btnImg;
            calibrateButton.transition = Selectable.Transition.None;

            // Fluid animation
            var fluidAnim = btnObj.AddComponent<SettingsFluidButtonAnimator>();
            fluidAnim.targetGraphic = btnImg;
            fluidAnim.normalColor = AccentViolet;
            fluidAnim.hoverColor = Color.Lerp(AccentViolet, Color.white, 0.18f);
            fluidAnim.pressColor = Color.Lerp(AccentViolet, Color.black, 0.22f);

            panel.SetActive(false);
            return panel;
        }

        // ═══════════════════════════════════════════════
        // DIVIDER
        // ═══════════════════════════════════════════════
        private void AddDivider(Transform parent)
        {
            var container = CreateUIObject("DividerContainer", parent);
            var le = container.AddComponent<LayoutElement>();
            le.minHeight = 8;
            le.preferredHeight = 8;

            // Simple centered line — no erroneous LayoutGroup
            var line = CreateUIObject("Line", container.transform);
            var lineRT = line.GetComponent<RectTransform>();
            lineRT.anchorMin = new Vector2(0.05f, 0.5f);
            lineRT.anchorMax = new Vector2(0.95f, 0.5f);
            lineRT.pivot = new Vector2(0.5f, 0.5f);
            lineRT.anchoredPosition = Vector2.zero;
            lineRT.sizeDelta = new Vector2(0, 1f);

            var img = line.AddComponent<Image>();
            img.color = DividerColor;
        }

        // ═══════════════════════════════════════════════
        // SETUP REFERENCES
        // ═══════════════════════════════════════════════
        void SetupRefs(Slider mS, Slider sS, Toggle mT, Toggle sT, TMP_Dropdown dd, Toggle hT,
                      Button rB, Button bB, TMP_Dropdown cMD, TMP_Dropdown aMD, Slider cSS,
                      Button cB, GameObject cP)
        {
            var view = GetComponent<SettingsView>() ?? gameObject.AddComponent<SettingsView>();
            view.musicSlider = mS;
            view.sfxSlider = sS;
            view.musicToggle = mT;
            view.sfxToggle = sT;
            view.languageDropdown = dd;
            view.hapticToggle = hT;
            view.backButton = bB;
            view.resetProgressButton = rB;
            view.controlMethodDropdown = cMD;
            view.accelerationModeDropdown = aMD;
            view.controlSensitivitySlider = cSS;
            view.calibrateButton = cB;
            view.calibrationPanel = cP;

            view.InitializeListeners();

            var ctrl = GetComponent<SettingsController>() ?? gameObject.AddComponent<SettingsController>();
            ctrl.view = view;
            ctrl.InitializeView();
        }

        // ═══════════════════════════════════════════════
        // UTILITIES
        // ═══════════════════════════════════════════════

        void ClearChildren()
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                var child = transform.GetChild(i).gameObject;
                if (Application.isPlaying) Destroy(child);
                else DestroyImmediate(child);
            }
        }

        void Stretch(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = rt.offsetMax = Vector2.zero;
        }

        GameObject CreateUIObject(string name, Transform parent)
        {
            var go = new GameObject(name);
            go.layer = LayerMask.NameToLayer("UI");
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
            rt.offsetMin = rt.offsetMax = Vector2.zero;
            return go;
        }

        TextMeshProUGUI AddText(Transform p, string n, string t, float s, FontStyles st, Color c, Vector2 pos, Vector2 size, string lk = null, TMP_FontAsset font = null)
        {
            var go = CreateUIObject(n, p);
            var rt = go.GetComponent<RectTransform>();
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;

            var tmp = go.AddComponent<TextMeshProUGUI>();
            
            // Eğer spesifik font verilmişse onu kullan, yoksa customFont (legacy), o da yoksa default
            if (font != null) tmp.font = font;
            else if (customFont != null) tmp.font = customFont;
            
            // Font'un kendisine fallback ekle (Global çözüm)
            var targetFont = tmp.font;
            if (targetFont != null && fallbackFont != null)
            {
                if (targetFont.fallbackFontAssetTable == null) targetFont.fallbackFontAssetTable = new List<TMP_FontAsset>();
                if (!targetFont.fallbackFontAssetTable.Contains(fallbackFont))
                {
                    targetFont.fallbackFontAssetTable.Add(fallbackFont);
#if UNITY_EDITOR
                    UnityEditor.EditorUtility.SetDirty(targetFont);
#endif
                }
            }
            
            tmp.text = t;
            tmp.fontSize = s;
            tmp.fontStyle = st;
            tmp.color = c;
            tmp.alignment = TextAlignmentOptions.MidlineLeft;
            tmp.raycastTarget = false;

            if (!string.IsNullOrEmpty(lk))
            {
                var loc = go.AddComponent<LocalizedText>();
                loc.SetKey(lk);
            }
            return tmp;
        }

        static string Loc(string key, string fallback)
        {
            if (LocalizationManager.Instance != null)
            {
                string v = LocalizationManager.Instance.GetTranslation(key);
                if (!string.IsNullOrEmpty(v) && v != key) return v;
            }
            return fallback;
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(SettingsVisualOverhaul))]
    public class SettingsVisualOverhaulEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            var panel = (SettingsVisualOverhaul)target;

            EditorGUILayout.Space(20);
            GUI.backgroundColor = new Color32(255, 191, 36, 255);

            if (GUILayout.Button("🎨 MODERN AYARLAR PANELİNİ OLUŞTUR", GUILayout.Height(50)))
            {
                panel.BuildSettingsPanel();
            }

            GUI.backgroundColor = Color.white;

            EditorGUILayout.Space(10);
            EditorGUILayout.HelpBox(
                "Warm Gold & Amber Settings Panel\n" +
                "• Staggered entrance animations\n" +
                "• Breathing border pulse\n" +
                "• Fluid button micro-interactions\n" +
                "• Smooth toggle slide\n" +
                "• Warm alternating row backgrounds",
                MessageType.Info
            );
        }
    }
#endif
}