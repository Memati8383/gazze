using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

namespace Gazze.UI
{
    /// <summary>
    /// Ana menüdeki "MainOptionsPanel" içeriğini premium bir görünüme kavuşturan sistem.
    /// V5: RawImage arka plan desteği (Case-sensitivity fix) ve daha yüksek kontrastlı bir tasarım.
    /// </summary>
    public class MainOptionsVisualOverhaul : MonoBehaviour
    {
        static TMP_FontAsset fallbackFont;
        [Header("Tasarım Ayarları")]
        // Vurgu Rengi: Burnished Amber (Daha premium ve sıcak bir ton)
        public Color accentColor = new Color32(255, 160, 80, 255); // Rich Amber

        // Metin Rengi: Silk White (Daha az göz yoran kemik beyazı)
        public Color textColor = new Color32(255, 253, 245, 255); 

        public Color subTextColor = new Color32(210, 190, 160, 210); // Patina Gold / Dust Gray

        [Header("Font Seçenekleri")]
        public TMP_FontAsset titleFont;      // Ana başlık: Anton (Askeri/Görev havası)
        public TMP_FontAsset technicalFont;  // Bilgi metinleri: Electronic Highway Sign (Teknik hava)
        public TMP_FontAsset buttonFont;     // Butonlar: Oswald Bold (Premium/Modern)
        public TMP_FontAsset bodyFont;       // İpuçları: Oswald (Okunaklı)

        [Header("Branding")]
        public string gameTitle = "GÖREV: GAZZE";
        public string gameSubtitle = "İNSANİ YARDIM KONVOYU";
        public string versionInfo = "v1.0.0 - STABLE";

        [Header("Assets (Manual Assignment Required)")]
        public Texture backgroundTexture; 
        public Material dynamicBgMaterial;

        [ContextMenu("YENİ PALETİ UYGULA (AMBER & OBSIDIAN)")]
        public void ApplyAmberPalette()
        {
            accentColor = new Color32(255, 160, 80, 255);
            textColor = new Color32(255, 253, 245, 255);
            subTextColor = new Color32(210, 190, 160, 210);
            
            // Build the menu immediately to show changes
            BuildMainOptions();
            
            Debug.Log("<color=orange>Gazze:</color> Amber & Obsidian paleti uygulandı!");
        }

        [ContextMenu("ANA MENÜYÜ SIFIRDAN KUR (V7 - PARALLAX)")]
        public void BuildMainOptions()
        {
            // 0. Panel'in kendi Image'ını temizle veya şeffaf yap
            var panelImg = GetComponent<Image>();
            if (panelImg != null) panelImg.color = new Color(0, 0, 0, 0);

            // 1. Temizlik
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                var ch = transform.GetChild(i).gameObject;
                if (Application.isPlaying) Destroy(ch);
                else DestroyImmediate(ch);
            }

            // 2. Panel Config
            var rt = GetComponent<RectTransform>();
            if (rt == null) rt = gameObject.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one; rt.sizeDelta = Vector2.zero;
            
            var cg = GetComponent<CanvasGroup>();
            if (cg == null) cg = gameObject.AddComponent<CanvasGroup>();
            cg.alpha = 1;

            // Arka Plan Resmi (Dynamic Shader Support)
            var bgGo = new GameObject("BackgroundContainer", typeof(RectTransform), typeof(RawImage));
            bgGo.transform.SetParent(transform, false);
            bgGo.transform.SetAsFirstSibling();
            var bgRt = bgGo.GetComponent<RectTransform>();
            bgRt.anchorMin = Vector2.zero; bgRt.anchorMax = Vector2.one;
            bgRt.offsetMin = bgRt.offsetMax = Vector2.zero;
            
            var bgImg = bgGo.GetComponent<RawImage>();
            bgImg.texture = backgroundTexture;
            bgImg.material = dynamicBgMaterial;
            bgImg.color = (backgroundTexture != null) ? Color.white : new Color(0.05f, 0.05f, 0.08f, 1f);

            // Gelişmiş Cinematic Overlay
            var vignette = new GameObject("VignetteOverlay", typeof(RectTransform), typeof(Image));
            vignette.transform.SetParent(transform, false);
            vignette.transform.SetSiblingIndex(1);
            var vigRt = vignette.GetComponent<RectTransform>();
            vigRt.anchorMin = Vector2.zero; vigRt.anchorMax = Vector2.one;
            vigRt.offsetMin = vigRt.offsetMax = Vector2.zero;
            var vigImg = vignette.GetComponent<Image>();
            vigImg.color = new Color(0, 0, 0, 0.75f); // Deeper cinematic vignette

            // HAVADA UÇUŞAN TOZ ZERRELERİ
            CreateDustParticles(transform);

            // 3. Ana Başlık Bloğu (Sol Orta)
            var titleGroup = CreateAnchorGo("TitleGroup", new Vector2(0.06f, 0.45f), new Vector2(0.9f, 0.9f));
            titleGroup.transform.SetSiblingIndex(3);
            
            var title = CreateText(titleGroup.transform, "Title", gameTitle, 140, FontStyles.Bold, TextAlignmentOptions.Left, titleFont);
            title.color = textColor;
            title.characterSpacing = 1; // Daha sıkı, taktiksel görünüm
            title.textWrappingMode = TextWrappingModes.NoWrap;
            title.rectTransform.anchorMin = title.rectTransform.anchorMax = new Vector2(0, 1);
            title.rectTransform.pivot = new Vector2(0, 1);
            title.rectTransform.sizeDelta = new Vector2(1800, 180);
            title.rectTransform.anchoredPosition = new Vector2(0, 0);
            AddLocalization(title, "Game_Title");

            var sub = CreateText(titleGroup.transform, "Subtitle", gameSubtitle, 28, FontStyles.Normal, TextAlignmentOptions.Left, technicalFont);
            sub.color = accentColor;
            sub.characterSpacing = 14; 
            sub.fontStyle = FontStyles.Bold; // Ekstra netlik
            sub.rectTransform.anchorMin = sub.rectTransform.anchorMax = new Vector2(0, 1);
            sub.rectTransform.pivot = new Vector2(0, 1);
            sub.rectTransform.sizeDelta = new Vector2(1200, 60);
            sub.rectTransform.anchoredPosition = new Vector2(15, -150);
            AddLocalization(sub, "Game_Subtitle");

            var accentBar = new GameObject("AccentBar", typeof(RectTransform), typeof(Image));
            accentBar.transform.SetParent(titleGroup.transform, false);
            var abRT = accentBar.GetComponent<RectTransform>();
            abRT.anchorMin = abRT.anchorMax = new Vector2(0, 1);
            abRT.pivot = new Vector2(1, 1);
            abRT.sizeDelta = new Vector2(12, 220);
            abRT.anchoredPosition = new Vector2(-50, 20);
            accentBar.GetComponent<Image>().color = accentColor;

            // 4. İpuçları (Konvoy Rehberi - Daktilo Efektli)
            var tipsArea = CreateAnchorGo("TipsArea", new Vector2(0.05f, 0.08f), new Vector2(0.48f, 0.26f));
            tipsArea.transform.SetSiblingIndex(4);
            
            // Glassmorphism-ish background
            var tipsBg = tipsArea.AddComponent<Image>();
            tipsBg.color = new Color(0.03f, 0.03f, 0.05f, 0.85f); // Deep Obsidian background 
            
            // Side Border (Accent)
            var tipsBorder = new GameObject("SideBorder", typeof(RectTransform), typeof(Image));
            tipsBorder.transform.SetParent(tipsArea.transform, false);
            var borderRT = tipsBorder.GetComponent<RectTransform>();
            borderRT.anchorMin = Vector2.zero; borderRT.anchorMax = new Vector2(0, 1);
            borderRT.sizeDelta = new Vector2(6, 0);
            borderRT.anchoredPosition = Vector2.zero;
            tipsBorder.GetComponent<Image>().color = accentColor;
            
            // Top Danger Border
            var topBorder = new GameObject("TopBorder", typeof(RectTransform), typeof(Image));
            topBorder.transform.SetParent(tipsArea.transform, false);
            var topRT = topBorder.GetComponent<RectTransform>();
            topRT.anchorMin = new Vector2(0, 1); topRT.anchorMax = new Vector2(1, 1);
            topRT.sizeDelta = new Vector2(0, 3);
            topRT.anchoredPosition = Vector2.zero;
            topBorder.GetComponent<Image>().color = new Color(accentColor.r, accentColor.g, accentColor.b, 0.4f);

            // (Progress Bar removed as requested)

            var tipsLabel = CreateText(tipsArea.transform, "Label", "KONVOY REHBERİ //", 16, FontStyles.Bold, TextAlignmentOptions.Left, technicalFont);
            tipsLabel.color = accentColor;
            tipsLabel.characterSpacing = 8;
            tipsLabel.rectTransform.anchorMin = tipsLabel.rectTransform.anchorMax = new Vector2(0, 1);
            tipsLabel.rectTransform.pivot = new Vector2(0, 1);
            tipsLabel.rectTransform.sizeDelta = new Vector2(400, 30);
            tipsLabel.rectTransform.anchoredPosition = new Vector2(25, -20);

            var tipsText = CreateText(tipsArea.transform, "TipText", "Analiz yükleniyor...", 24, FontStyles.Normal, TextAlignmentOptions.Left, bodyFont);
            tipsText.color = new Color(1, 1, 1, 0.95f);
            tipsText.lineSpacing = 10;
            tipsText.rectTransform.anchorMin = tipsText.rectTransform.anchorMax = new Vector2(0, 1);
            tipsText.rectTransform.pivot = new Vector2(0, 1);
            tipsText.rectTransform.sizeDelta = new Vector2(740, 130);
            tipsText.rectTransform.anchoredPosition = new Vector2(25, -65);
            
            var tipsMgr = tipsArea.GetComponent<MenuTipsDisplay>();
            if (tipsMgr != null)
            {
                if (Application.isPlaying) Destroy(tipsMgr);
                else DestroyImmediate(tipsMgr);
            }
            tipsMgr = tipsArea.AddComponent<MenuTipsDisplay>();
            tipsMgr.tipText = tipsText;
// Progress bar reference removed

            // 5. Butonlar (Sağ Alt)
            var menuGroup = CreateAnchorGo("MenuGroup", new Vector2(0.60f, 0.12f), new Vector2(0.94f, 0.45f));
            menuGroup.transform.SetSiblingIndex(5);
            var vlg = menuGroup.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 40;
            vlg.childAlignment = TextAnchor.LowerRight;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandHeight = false;

            var playBtn = CreateCleanButton(menuGroup.transform, "Btn_OyunaBasla", "BAŞLA");
            var settingsBtn = CreateCleanButton(menuGroup.transform, "Btn_Ayarlar", "AYARLAR");
            var exitBtn = CreateCleanButton(menuGroup.transform, "Btn_Cikis", "ÇIKIŞ");

            // 6. Bilgi Metinleri + Glitch Efekti (Daha küçük ve geniş tracking - Taktiksel Görünüm)
            var bit1Go = CreateAnchorGo("StatusBit", new Vector2(0.04f, 0.94f), new Vector2(0.4f, 0.98f));
            var bit1 = CreateText(bit1Go.transform, "Text", "OPERASYON DURUMU // HAZIR", 16, FontStyles.Bold, TextAlignmentOptions.Left, technicalFont);
            bit1.color = subTextColor;
            bit1.characterSpacing = 12; // Geniş tracking premium hissettirir
            bit1.gameObject.AddComponent<TechnicalTextGlitch>();
            
            var bit2 = CreateDataBit(new Vector2(0.96f, 0.96f), versionInfo, TextAlignmentOptions.Right);
            bit2.GetComponent<TextMeshProUGUI>().characterSpacing = 8;
            bit2.GetComponent<TextMeshProUGUI>().fontSize = 14;

            // 7. Animasyon Bileşeni Ekle
            var anim = gameObject.GetComponent<MenuAnimationController>();
            if (anim == null) anim = gameObject.AddComponent<MenuAnimationController>();
            anim.titleGroup = titleGroup.GetComponent<RectTransform>();
            anim.buttonGroup = menuGroup.GetComponent<RectTransform>();
            anim.hudElements = new RectTransform[] { bit1Go.GetComponent<RectTransform>(), bit2, tipsArea.GetComponent<RectTransform>() };
            
            // 8. PARALLAX SİSTEMİ KURULUMU
            SetupParallax(bgRt, titleGroup.GetComponent<RectTransform>(), tipsArea.GetComponent<RectTransform>(), menuGroup.GetComponent<RectTransform>());

            // 9. Referanslar
            SetupMainMenuManagerRefs(playBtn, settingsBtn, exitBtn, null);

            if (!Application.isPlaying) anim.ResetToFinalState();
            
            Debug.Log("<color=cyan>Gazze:</color> Menü Yeniden Oluşturuldu (V7 - PARALLAX)!");
        }

        private void SetupParallax(RectTransform bg, RectTransform title, RectTransform tips, RectTransform menu)
        {
            var parallax = gameObject.GetComponent<MenuParallaxEffect>();
            if (parallax == null) parallax = gameObject.AddComponent<MenuParallaxEffect>();

            // Ana Kamera (Showcase) referansını bul
            var camGo = GameObject.Find("ShowcaseCamera");
            if (camGo != null) parallax.targetCamera = camGo.transform;

            parallax.backgroundLayer = bg;
            parallax.backgroundStrength = 20f;

            parallax.uiLayers = new RectTransform[] { title, tips, menu };
            parallax.layerStrengths = new float[] { 12f, 8f, -10f }; // Farklı yönler derinlik verir
            
            parallax.smoothTime = 0.25f;
            parallax.CaptureInitialState();
        }

        private void CreateDustParticles(Transform parent)
        {
            var go = new GameObject("DustParticles", typeof(ParticleSystem));
            go.transform.SetParent(parent, false);
            go.transform.localPosition = new Vector3(0, 0, -4f); // Move forward along Z
            var ps = go.GetComponent<ParticleSystem>();
            var main = ps.main;
            main.duration = 5f;
            main.loop = true;
            main.startLifetime = 15f; 
            main.startSize = new ParticleSystem.MinMaxCurve(0.015f, 0.07f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(0.05f, 0.35f);
            main.startColor = new Color(1f, 0.9f, 0.7f, 0.3f);
            main.maxParticles = 600;
            main.simulationSpace = ParticleSystemSimulationSpace.Local;
            main.playOnAwake = true;

            var emission = ps.emission;
            emission.rateOverTime = 65;

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(30, 15, 6); // More volume coverage

            var renderer = go.GetComponent<ParticleSystemRenderer>();
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            renderer.sortingOrder = 10; 
            
            // Fix Pink Material: Enhanced Pipeline-Aware Shader Finding
            string[] possibleShaders = { 
                "Universal Render Pipeline/2D/Sprite-Unlit-Default",
                "Universal Render Pipeline/Particles/Unlit", 
                "Legacy Shaders/Particles/Additive", 
                "Sprites/Default",
                "UI/Default"
            };
            
            Shader shader = null;
            foreach (var name in possibleShaders)
            {
                shader = Shader.Find(name);
                if (shader != null) break;
            }
            
            Material sharedParticMat = (shader != null) ? new Material(shader) : new Material(Shader.Find("Hidden/Internal-Colored"));
            sharedParticMat.color = new Color(1, 1, 1, 0.25f);
            renderer.material = sharedParticMat;
            
            // Start playing immediately in scene view
            if (!Application.isPlaying) ps.Simulate(0.1f); ps.Play();
        }

        private Button CreateCleanButton(Transform parent, string name, string label)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(CanvasGroup));
            go.transform.SetParent(parent, false);
            go.AddComponent<LayoutElement>().preferredHeight = 80;
            
            var hitArea = go.GetComponent<Image>();
            hitArea.color = new Color(0, 0, 0, 0); 
            hitArea.raycastTarget = true;

            var t = CreateText(go.transform, "Text", label, 64, FontStyles.Bold, TextAlignmentOptions.Right, buttonFont);
            t.characterSpacing = 5; // Hafif genişletilmiş buton metni
            t.raycastTarget = true;
            t.textWrappingMode = TextWrappingModes.NoWrap;
            t.rectTransform.anchorMin = Vector2.zero; t.rectTransform.anchorMax = Vector2.one;
            t.rectTransform.sizeDelta = Vector2.zero;
            t.rectTransform.anchoredPosition = new Vector2(-40, 0); // Biraz daha içeri

            var line = new GameObject("Underline", typeof(RectTransform), typeof(Image));
            line.transform.SetParent(go.transform, false);
            var lRT = line.GetComponent<RectTransform>();
            lRT.anchorMin = new Vector2(0.4f, 0); lRT.anchorMax = new Vector2(1, 0);
            lRT.sizeDelta = new Vector2(0, 4f);
            lRT.anchoredPosition = new Vector2(0, -12);
            line.GetComponent<Image>().color = new Color(accentColor.r, accentColor.g, accentColor.b, 0.5f);

            var btn = go.AddComponent<Button>();
            btn.targetGraphic = hitArea;
            var cb = btn.colors;
            cb.normalColor = Color.white;
            cb.highlightedColor = accentColor;
            cb.pressedColor = new Color(accentColor.r, accentColor.g, accentColor.b, 0.2f);
            btn.colors = cb;

            go.AddComponent<ButtonScaleAnimator>().pressedScale = 0.95f;

            return btn;
        }

        private RectTransform CreateDataBit(Vector2 anchor, string content, TextAlignmentOptions align)
        {
            var go = new GameObject("DataBit", typeof(RectTransform), typeof(CanvasGroup));
            go.transform.SetParent(transform, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = anchor;
            rt.pivot = (align == TextAlignmentOptions.Left) ? new Vector2(0, 1) : new Vector2(1, 0);
            rt.sizeDelta = new Vector2(1000, 50);
            var t = go.AddComponent<TextMeshProUGUI>();
            t.text = content; t.fontSize = 24; t.color = subTextColor;
            t.fontStyle = FontStyles.Bold; t.alignment = align;
            t.characterSpacing = 4f;
            t.font = technicalFont;
            return rt;
        }

        private GameObject CreateAnchorGo(string name, Vector2 min, Vector2 max)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(transform, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = min;
            rt.anchorMax = max;
            rt.offsetMin = rt.offsetMax = Vector2.zero;
            return go;
        }

        private TextMeshProUGUI CreateText(Transform parent, string name, string content, float size, FontStyles style, TextAlignmentOptions align, TMP_FontAsset font = null)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var t = go.AddComponent<TextMeshProUGUI>();
            t.text = content;
            t.fontSize = size;
            t.fontStyle = style;
            t.alignment = align;
            t.color = textColor;
            if (font != null) t.font = font;
            t.raycastTarget = false;

            // Ensure Turkish Fallback support
            if (fallbackFont == null)
                fallbackFont = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");

            if (t.font != null && fallbackFont != null)
            {
                if (t.font.fallbackFontAssetTable == null) t.font.fallbackFontAssetTable = new List<TMP_FontAsset>();
                if (!t.font.fallbackFontAssetTable.Contains(fallbackFont))
                {
                    t.font.fallbackFontAssetTable.Add(fallbackFont);
#if UNITY_EDITOR
                    UnityEditor.EditorUtility.SetDirty(t.font);
#endif
                }
            }

            return t;
        }

        private void SetupMainMenuManagerRefs(Button play, Button settings, Button exit, TextMeshProUGUI balance)
        {
            var menu = FindFirstObjectByType<MainMenuManager>();
            if (menu == null) return;

            menu.playButton = play;
            menu.settingsButton = settings;
            menu.exitButton = exit;
            menu.totalKrediText = balance;

            AddLocalization(play.GetComponentInChildren<TextMeshProUGUI>(), "Menu_Start");
            AddLocalization(settings.GetComponentInChildren<TextMeshProUGUI>(), "Menu_Settings");
            AddLocalization(exit.GetComponentInChildren<TextMeshProUGUI>(), "Menu_Exit");

#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(menu);
#endif
        }

        private void AddLocalization(TextMeshProUGUI text, string key)
        {
            if (text == null) return;
            var loc = text.gameObject.GetComponent<LocalizedText>();
            if (loc == null) loc = text.gameObject.AddComponent<LocalizedText>();
            loc.SetKey(key);
        }
    }
}
