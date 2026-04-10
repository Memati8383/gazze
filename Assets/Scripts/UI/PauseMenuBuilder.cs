using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.EventSystems;

namespace Gazze.UI
{
    /// <summary>
    /// "Frosted Midnight" temalı duraklatma menüsü.
    /// Bulanık arka plan + cam kart + yerinde ayarlar paneli.
    /// </summary>
    public class PauseMenuBuilder
    {
        // ─── STATE ──────────────────────────────────────────────────────
        static GameObject _pauseCanvasGO;
        static TMP_FontAsset fallbackFont;
        static bool _isPaused;
        static Texture2D _blurTexture;
        static bool _settingsOpen;

        public static bool IsPaused => _isPaused;

        // ─── TOKENS — Luxury Amber & Obsidian ──────────────────────
        static readonly Color BgTint         = new Color(0.012f, 0.008f, 0.004f, 0.90f); // Deeper Obsidian
        static readonly Color CardGlass      = new Color(0.15f, 0.10f, 0.07f, 0.75f); // Smoked Amber Glass
        static readonly Color CardBorderA    = new Color32(255, 215, 110, 140);  // Polished Gold
        static readonly Color CardBorderB    = new Color32(220, 130, 60, 120);   // Burnished Copper

        static readonly Color Accent1        = new Color32(255, 225, 140, 255);  // Luxury Gold
        static readonly Color Accent2        = new Color32(255, 160, 80, 255);   // Rich Amber
        static readonly Color Accent3        = new Color32(255, 250, 230, 255);  // Ivory Gold (Primary)
        static readonly Color AccentDanger   = new Color32(255, 100, 60, 255);   // Deep Crimson-Gold

        static readonly Color TextWhite      = new Color32(255, 253, 245, 255);  // Silk White
        static readonly Color TextFaded      = new Color32(210, 190, 160, 210);  // Patina Gold
        static readonly Color SepColor       = new Color32(255, 210, 130, 45);   // Gold Filament
        static readonly Color SliderTrack    = new Color32(45, 35, 25, 200);
        static readonly Color SliderFill     = new Color32(255, 205, 90, 220);
        static readonly Color ToggleOn       = new Color32(255, 220, 100, 255);
        static readonly Color ToggleOff      = new Color32(70, 60, 50, 210);

// ─── YENİ BUTON ETKİLEŞİM ANİMATÖRÜ ──────────────────────────────────
        class FluidButtonAnimator : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
        {
            public Image targetGraphic;
            public Color normalColor;
            public Color hoverColor;
            public Color pressColor;
            
            Vector3 _targetScale = Vector3.one;
            Quaternion _targetRot = Quaternion.identity;
            Color _targetColor;

            void Start()
            {
                _targetColor = normalColor;
                if (targetGraphic) targetGraphic.color = normalColor;
            }

            void Update()
            {
                // Smooth Lerp for Scale and Slerp for Rotation for a premium weight feel
                transform.localScale = Vector3.Lerp(transform.localScale, _targetScale, Time.unscaledDeltaTime * 12f);
                transform.localRotation = Quaternion.Slerp(transform.localRotation, _targetRot, Time.unscaledDeltaTime * 10f);
                if (targetGraphic)
                {
                    targetGraphic.color = Color.Lerp(targetGraphic.color, _targetColor, Time.unscaledDeltaTime * 14f);
                }
            }

            public void OnPointerEnter(PointerEventData e) 
            { 
                _targetScale = Vector3.one * 1.04f; 
                _targetRot = Quaternion.Euler(0, 0, 1.0f); // Subtle Tilt
                _targetColor = hoverColor; 
            }
            public void OnPointerExit(PointerEventData e)  
            { 
                _targetScale = Vector3.one; 
                _targetRot = Quaternion.identity;
                _targetColor = normalColor; 
            }
            public void OnPointerDown(PointerEventData e)  
            { 
                _targetScale = Vector3.one * 0.97f; 
                _targetRot = Quaternion.Euler(0, 0, -0.5f);
                _targetColor = pressColor; 
            }
            public void OnPointerUp(PointerEventData e)    
            { 
                _targetScale = Vector3.one * 1.04f; 
                _targetColor = hoverColor; 
            }
        }

// ─── ANA BUTON İÇİN "NEFES ALAN PARLAMA" EFEKTİ ──────────────────────
        class PrimaryGlowPulse : MonoBehaviour
        {
            public Outline glowOutline;
            public float speed = 2.5f;
            public float minAlpha = 0.15f;
            public float maxAlpha = 0.45f;

            void Update()
            {
                if (!glowOutline) return;
                // High-end breathing effect
                float alpha = Mathf.Lerp(minAlpha, maxAlpha, (Mathf.Sin(Time.unscaledTime * speed) + 1f) / 2f);
                var c = glowOutline.effectColor;
                glowOutline.effectColor = new Color(c.r, c.g, c.b, alpha);
            }
        }

// ─── YENİ: YUMUŞAK TOGGLE ANİMATÖRÜ ──────────────────────────────────
class SmoothToggleAnimator : MonoBehaviour
{
    public RectTransform handle;
    public Image bgImage;
    public Color onColor;
    public Color offColor;
    public float onX = 16f;
    public float offX = -16f;
    
    bool _isOn;

    public void SetState(bool isOn, bool instant = false) 
    { 
        _isOn = isOn; 
        if(instant) 
        {
            handle.anchoredPosition = new Vector2(_isOn ? onX : offX, 0);
            bgImage.color = _isOn ? onColor : offColor;
        }
    }

    void Update()
    {
        if (!handle || !bgImage) return;
        
        float targetX = _isOn ? onX : offX;
        Vector2 pos = handle.anchoredPosition;
        pos.x = Mathf.Lerp(pos.x, targetX, Time.unscaledDeltaTime * 18f);
        handle.anchoredPosition = pos;

        Color targetCol = _isOn ? onColor : offColor;
        bgImage.color = Color.Lerp(bgImage.color, targetCol, Time.unscaledDeltaTime * 15f);
    }
}

        // ─── ANIMATOR ───────────────────────────────────────────────────
        class PanelAnimator : MonoBehaviour
        {
            public CanvasGroup cg;
            public List<RectTransform> targets = new List<RectTransform>();
            public RectTransform cardRT;
            public RawImage blurImg;

            float t;
            bool closing;
            System.Action onDone;

            void OnEnable()
            {
                t = 0f; closing = false;
                if (cg) cg.alpha = 0f;
                foreach (var rt in targets)
                {
                    if (!rt) continue;
                    var c = rt.GetComponent<CanvasGroup>();
                    if (c) c.alpha = 0f;
                    rt.localScale = Vector3.one * 0.9f;
                }
                if (cardRT) cardRT.localScale = Vector3.one * 0.93f;
            }

            void Update()
            {
                if (closing) { CloseUpdate(); return; }

                t += Time.unscaledDeltaTime;

                // Overlay fade
                float oT = Mathf.Clamp01(t / 0.3f);
                float oE = 1f - Mathf.Pow(1f - oT, 2.5f);
                if (cg) cg.alpha = oE;

                // Blur fade
                if (blurImg)
                    blurImg.color = new Color(1, 1, 1, Mathf.Clamp01(t / 0.25f));

                // Card scale
                if (cardRT)
                {
                    float cT = Mathf.Clamp01((t - 0.03f) / 0.35f);
                    float cE = 1f - Mathf.Pow(1f - cT, 3.5f);
                    cardRT.localScale = Vector3.Lerp(Vector3.one * 0.93f, Vector3.one, cE);
                }

                // Stagger
                for (int i = 0; i < targets.Count; i++)
                {
                    if (!targets[i]) continue;
                    float s = 0.15f + i * 0.05f; // Gecikme (Stagger)
                    float p = Mathf.Clamp01((t - s) / 0.35f); // Animasyon süresini biraz uzattık (0.35f)
                    
                    // "Overshoot" (Hafifçe geçip geri gelme) efekti için OutBack eğrisi
                    float e = 1f - Mathf.Pow(1f - p, 3f) * (1f - p * 1.2f); 

                    var c2 = targets[i].GetComponent<CanvasGroup>();
                    if (c2) c2.alpha = Mathf.Clamp01(p * 2f); // Alpha daha hızlı dolsun

                    var orig = targets[i].GetComponent<SOrig>();
                    if (orig)
                    {
                        // Çapraz başlangıç noktası: 
                        // X: side * 150 birim dışarıdan, Y: 40 birim aşağıdan başla
                        Vector2 startPos = orig.pos + new Vector2(orig.side * 150f, -40f);
                        targets[i].anchoredPosition = Vector2.LerpUnclamped(startPos, orig.pos, e);
                    }
                    
                    targets[i].localScale = Vector3.LerpUnclamped(Vector3.one * 0.8f, Vector3.one, e);
                }
            }

            public void Close(System.Action cb) { closing = true; onDone = cb; t = 0f; }

            void CloseUpdate()
            {
                t += Time.unscaledDeltaTime;
                float p = Mathf.Clamp01(t / 0.2f);
                if (cg) cg.alpha = 1f - p * p;
                if (cardRT) cardRT.localScale = Vector3.Lerp(Vector3.one, Vector3.one * 0.9f, p * p);
                if (p >= 1f) onDone?.Invoke();
            }
        }

        // Orijinal pozisyonu saklayan sınıfı güncelleyelim
        class SOrig : MonoBehaviour 
        { 
            public Vector2 pos; // Sadece Y değil, tüm pozisyonu tutalım
            public float side;  // -1: Soldan, 1: Sağdan gelecek
        }

        class GradientBorderPulse : MonoBehaviour
        {
            public Outline outline;
            void Update()
            {
                if (!outline) return;
                float t = Time.unscaledTime;
                float r = Mathf.Lerp(CardBorderA.r, CardBorderB.r, Mathf.Sin(t * 1.2f) * 0.5f + 0.5f);
                float g = Mathf.Lerp(CardBorderA.g, CardBorderB.g, Mathf.Sin(t * 1.2f) * 0.5f + 0.5f);
                float b = Mathf.Lerp(CardBorderA.b, CardBorderB.b, Mathf.Sin(t * 1.2f) * 0.5f + 0.5f);
                float a = 0.3f + 0.15f * Mathf.Sin(t * 2f);
                outline.effectColor = new Color(r, g, b, a);
            }
        }

        class FloatingMotes : MonoBehaviour
        {
            public List<RectTransform> dots = new List<RectTransform>();
            public List<Image> imgs = new List<Image>();
            void Update()
            {
                float ut = Time.unscaledTime;
                for (int i = 0; i < dots.Count; i++)
                {
                    if (!dots[i]) continue;
                    float o = i * 2.1f;
                    dots[i].anchoredPosition += new Vector2(
                        Mathf.Cos(ut * 0.35f + o) * 4f * Time.unscaledDeltaTime,
                        Mathf.Sin(ut * 0.5f + o) * 6f * Time.unscaledDeltaTime);
                    if (imgs[i])
                    {
                        float a = 0.06f + 0.08f * Mathf.Sin(ut * 0.8f + o);
                        var c = imgs[i].color;
                        imgs[i].color = new Color(c.r, c.g, c.b, a);
                    }
                }
            }
        }

        // ═════════════════════════════════════════════════════════════════
        // PUBLIC API
        // ═════════════════════════════════════════════════════════════════
        public static void Pause()
        {
            if (_isPaused) return;
            _isPaused = true;
            _settingsOpen = false;
            Time.timeScale = 0f;
            Build();
        }

        public static void Resume()
        {
            if (!_isPaused) return;
            if (Settings.AudioManager.Instance) Settings.AudioManager.Instance.PlayClickSound();
            
            if (_pauseCanvasGO)
            {
                var anim = _pauseCanvasGO.GetComponentInChildren<PanelAnimator>();
                if (anim)
                {
                    anim.Close(() => CompleteResume());
                    return;
                }
            }
            CompleteResume();
        }

        static void CompleteResume()
        {
            _isPaused = false;
            Cleanup();

            // Geri sayım baslatalım (eğer varsa)
            if (CountdownManager.Instance != null)
            {
                CountdownManager.TriggerResumeCountdown(() =>
                {
                    Time.timeScale = 1f;
                });
            }
            else
            {
                Time.timeScale = 1f;
            }
        }

        public static void Toggle() { if (_isPaused) Resume(); else Pause(); }

        public static void ForceClose() { _isPaused = false; Cleanup(); }

        static void Cleanup()
        {
            _settingsOpen = false;
            if (_pauseCanvasGO) Object.Destroy(_pauseCanvasGO);
            _pauseCanvasGO = null;
            if (_blurTexture) { Object.Destroy(_blurTexture); _blurTexture = null; }
        }

        // ═════════════════════════════════════════════════════════════════
        // BLUR
        // ═════════════════════════════════════════════════════════════════
        static Texture2D CaptureBlur()
        {
            Camera cam = Camera.main;
            if (!cam) return null;
            try
            {
                int fw = Screen.width, fh = Screen.height;
                var full = RenderTexture.GetTemporary(fw, fh, 24);
                var orig = cam.targetTexture;
                cam.targetTexture = full;
                cam.Render();
                cam.targetTexture = orig;

                // 3-step downsample
                var r1 = RenderTexture.GetTemporary(fw / 4, fh / 4, 0);
                Graphics.Blit(full, r1); RenderTexture.ReleaseTemporary(full);
                var r2 = RenderTexture.GetTemporary(fw / 8, fh / 8, 0);
                Graphics.Blit(r1, r2); RenderTexture.ReleaseTemporary(r1);
                var r3 = RenderTexture.GetTemporary(fw / 4, fh / 4, 0);
                Graphics.Blit(r2, r3); RenderTexture.ReleaseTemporary(r2);

                RenderTexture.active = r3;
                var tex = new Texture2D(r3.width, r3.height, TextureFormat.RGB24, false);
                tex.filterMode = FilterMode.Bilinear;
                tex.wrapMode = TextureWrapMode.Clamp;
                tex.ReadPixels(new Rect(0, 0, r3.width, r3.height), 0, 0);
                tex.Apply();
                RenderTexture.active = null;
                RenderTexture.ReleaseTemporary(r3);
                return tex;
            }
            catch (System.Exception e)
            {
                Debug.LogWarning("[Pause] Blur failed: " + e.Message);
                return null;
            }
        }

        // ═════════════════════════════════════════════════════════════════
        // BUILD — Main Pause View
        // ═════════════════════════════════════════════════════════════════
        static void Build()
        {
            if (_pauseCanvasGO) Object.Destroy(_pauseCanvasGO);
            _blurTexture = CaptureBlur();

            // Canvas
            _pauseCanvasGO = new GameObject("PauseCanvas",
                typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var cv = _pauseCanvasGO.GetComponent<Canvas>();
            cv.renderMode = RenderMode.ScreenSpaceOverlay;
            cv.sortingOrder = 99;
            var sc = _pauseCanvasGO.GetComponent<CanvasScaler>();
            sc.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            sc.referenceResolution = new Vector2(1080, 1920);
            sc.matchWidthOrHeight = 0.5f;
            EnsureEventSystem();

            // Root
            var root = MakeGO("Root", _pauseCanvasGO.transform, typeof(CanvasGroup));
            Stretch(root.GetComponent<RectTransform>());
            var anim = root.AddComponent<PanelAnimator>();
            anim.cg = root.GetComponent<CanvasGroup>();

            // Blur BG
            if (_blurTexture)
            {
                var blurGO = new GameObject("Blur", typeof(RectTransform), typeof(RawImage));
                blurGO.transform.SetParent(root.transform, false);
                Stretch(blurGO.GetComponent<RectTransform>());
                var ri = blurGO.GetComponent<RawImage>();
                ri.texture = _blurTexture;
                ri.color = new Color(1, 1, 1, 0);
                ri.raycastTarget = false;
                anim.blurImg = ri;
            }

            // Tint overlay
            var tint = MakeGO("Tint", root.transform, typeof(Image));
            Stretch(tint.GetComponent<RectTransform>());
            tint.GetComponent<Image>().color = BgTint;

            // Motes
            SpawnMotes(root.transform);

            // ── Card ──
            var card = new GameObject("Card", typeof(RectTransform), typeof(Image), typeof(Outline), typeof(Shadow));
            card.transform.SetParent(root.transform, false);
            var crt = card.GetComponent<RectTransform>();
            crt.anchorMin = crt.anchorMax = new Vector2(0.5f, 0.5f);
            crt.sizeDelta = new Vector2(600, 750);
            crt.anchoredPosition = new Vector2(0, 20);
            card.GetComponent<Image>().color = CardGlass;
            var ol = card.GetComponent<Outline>();
            ol.effectColor = CardBorderA;
            ol.effectDistance = new Vector2(1.5f, -1.5f);
            card.GetComponent<Shadow>().effectColor = new Color(0, 0, 0, 0.4f);
            card.GetComponent<Shadow>().effectDistance = new Vector2(6, -6);

            anim.cardRT = crt;
            
            // ── Parallax Efekti ──
            var parallax = root.AddComponent<MenuParallaxEffect>();
            parallax.smoothTime = 0.15f;
            parallax.useUnscaledTime = true;
            
            // Arka plan (blur) katmanı
            if (anim.blurImg)
            {
                parallax.backgroundLayer = anim.blurImg.rectTransform;
                parallax.backgroundStrength = -8f; // Ters yöne hafif hareket
            }
            
            // Ana kart katmanı
            parallax.uiLayers = new RectTransform[] { crt };
            parallax.layerStrengths = new float[] { 20f }; // Daha belirgin ileri hareket
            
            parallax.CaptureInitialState();

            // Glass inner layer
            var inner = MakeGO("InnerGlass", card.transform, typeof(Image));
            var irt = inner.GetComponent<RectTransform>();
            irt.anchorMin = Vector2.zero; irt.anchorMax = Vector2.one;
            irt.offsetMin = new Vector2(3, 3); irt.offsetMax = new Vector2(-3, -3);
            inner.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.025f);
            inner.GetComponent<Image>().raycastTarget = false;

            // Top accent gradient line
            MakeAccentBar(card.transform, true);
            // Bottom accent gradient line
            MakeAccentBar(card.transform, false);

            // ══════════ CONTENT ══════════
            var slist = new List<RectTransform>();

            // (1) Pause icon — two vertical bars
            var iconRow = Stagger(card.transform, "Icon", new Vector2(0, 295), new Vector2(60, 52), -1f);
            slist.Add(iconRow.GetComponent<RectTransform>());
            MakeBar(iconRow.transform, -13, Accent1);
            MakeBar(iconRow.transform, 13, Accent2);

            // (2) Title
            var titleGO = Stagger(card.transform, "Title", new Vector2(0, 235), new Vector2(540, 60), 1f);
            slist.Add(titleGO.GetComponent<RectTransform>());
            var titleTMP = AddTMP(titleGO.transform, "T", Loc("Game_Paused", "DURAKLATILDI"),
                42, FontStyles.Bold, Accent1, Vector2.zero, new Vector2(540, 60));
            titleTMP.characterSpacing = 16; // Harfler arası boşluğu artırdık (ÇOK ÖNEMLİ)
            titleTMP.enableVertexGradient = true;
            titleTMP.colorGradient = new VertexGradient(Accent1, Accent2, Accent1, Accent2);
            
            // (3) Sub
            var subGO = Stagger(card.transform, "Sub", new Vector2(0, 188), new Vector2(500, 24), -1f);
            slist.Add(subGO.GetComponent<RectTransform>());
            AddTMP(subGO.transform, "S", Loc("Game_PausedSub", "Oyun duraklatildi"),
                14, FontStyles.Italic, TextFaded, Vector2.zero, new Vector2(500, 24));

            // (4) Sep
            var sep = Stagger(card.transform, "Sep", new Vector2(0, 160), new Vector2(380, 1), 1f);
            slist.Add(sep.GetComponent<RectTransform>());
            sep.AddComponent<Image>().color = SepColor;

            // ══════════ BUTTONS ══════════
            float y = 70f, sp = 85f, bw = 400f, bh = 60f;

            // DEVAM ET — Primary mint
            var goBtn = Stagger(card.transform, "BtnResume", new Vector2(0, y), new Vector2(bw, bh), 1f);
            slist.Add(goBtn.GetComponent<RectTransform>());
            MakeButton(goBtn, Loc("Game_Continue", "DEVAM ET"), Accent3, true, () => Resume());

            // TEKRAR DENE — Ghost blue
            y -= sp;
            var restBtn = Stagger(card.transform, "BtnRestart", new Vector2(0, y), new Vector2(bw, bh), -1f);
            slist.Add(restBtn.GetComponent<RectTransform>());
            MakeButton(restBtn, Loc("Game_Restart", "TEKRAR DENE"), Accent1, false, () =>
            {
                ForceClose(); Time.timeScale = 1f;
                if (Settings.AudioManager.Instance) Settings.AudioManager.Instance.PlayClickSound();
                if (LoadingManager.Instance) LoadingManager.Instance.LoadScene(SceneManager.GetActiveScene().name);
                else SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            });

            // AYARLAR — Ghost violet, opens in-place settings sub-panel
            y -= sp;
            var setBtn = Stagger(card.transform, "BtnSettings", new Vector2(0, y), new Vector2(bw, bh), 1f);
            slist.Add(setBtn.GetComponent<RectTransform>());
            MakeButton(setBtn, Loc("Menu_Settings", "AYARLAR"), Accent2, false, () => ToggleSettingsPanel());

            // ANA MENU — Ghost red
            y -= sp;
            var mnBtn = Stagger(card.transform, "BtnMenu", new Vector2(0, y), new Vector2(bw, bh), -1f);
            slist.Add(mnBtn.GetComponent<RectTransform>());
            MakeButton(mnBtn, Loc("Game_MainMenu", "ANA MENU"), AccentDanger, false, () =>
            {
                ForceClose(); Time.timeScale = 1f;
                if (Settings.AudioManager.Instance) Settings.AudioManager.Instance.PlayClickSound();
                if (LoadingManager.Instance) LoadingManager.Instance.LoadScene("MainMenu");
                else SceneManager.LoadScene("MainMenu");
            });

            // Hint
            var hint = Stagger(card.transform, "Hint", new Vector2(0, -340), new Vector2(500, 22), -1f);
            slist.Add(hint.GetComponent<RectTransform>());
            AddTMP(hint.transform, "H", Loc("Game_PauseHint", "ESC ile devam et"),
                11, FontStyles.Italic, new Color(1, 1, 1, 0.25f), Vector2.zero, new Vector2(500, 22));

            anim.targets = slist;
        }

        // ═════════════════════════════════════════════════════════════════
        // IN-PLACE SETTINGS SUB-PANEL
        // ═════════════════════════════════════════════════════════════════
        static GameObject _settingsPanel;

        static void ToggleSettingsPanel()
        {
            if (Settings.AudioManager.Instance) Settings.AudioManager.Instance.PlayClickSound();
            if (_settingsOpen && _settingsPanel)
            {
                var anim = _settingsPanel.GetComponent<SettingsPanelAnimator>();
                if (anim)
                {
                    // Çıkış animasyonunu başlat ve bittiğinde (callback) temizliği yap
                    anim.Close(() =>
                    {
                        var mainCard = _pauseCanvasGO.transform.Find("Root/Card");
                        if (mainCard) mainCard.gameObject.SetActive(true);
                        Object.Destroy(_settingsPanel);
                        _settingsPanel = null;
                        _settingsOpen = false;
                    });
                }
                else
                {
                    // Güvenlik (Fallback) durumu
                    var mainCard = _pauseCanvasGO.transform.Find("Root/Card");
                    if (mainCard) mainCard.gameObject.SetActive(true);
                    Object.Destroy(_settingsPanel);
                    _settingsPanel = null;
                    _settingsOpen = false;
                }
                return;
            }
            BuildSettingsPanel();
        }

        static void BuildSettingsPanel()
        {
            if (!_pauseCanvasGO) return;

            var mainCard = _pauseCanvasGO.transform.Find("Root/Card");
            if(mainCard) mainCard.gameObject.SetActive(false);
            
            var root = _pauseCanvasGO.transform.GetChild(0); 

            // ── Settings Overlay Base ──
            _settingsPanel = new GameObject("SettingsOverlay", typeof(RectTransform), typeof(Image), typeof(CanvasGroup));
            _settingsPanel.transform.SetParent(root, false);
            Stretch(_settingsPanel.GetComponent<RectTransform>());
            _settingsPanel.GetComponent<Image>().color = new Color(0.012f, 0.008f, 0.005f, 0.90f); // Deep Obsidian Amber

            // ── Motes for Depth ──
            SpawnMotes(_settingsPanel.transform);

            // ── Main Settings Card ──
            var card = new GameObject("SetCard", typeof(RectTransform), typeof(Image), typeof(Outline), typeof(Shadow));
            card.transform.SetParent(_settingsPanel.transform, false);
            var crt = card.GetComponent<RectTransform>();
            crt.anchorMin = crt.anchorMax = new Vector2(0.5f, 0.5f);
            crt.sizeDelta = new Vector2(680, 960); // Genişliği biraz artırdık
            crt.anchoredPosition = Vector2.zero;
            card.GetComponent<Image>().color = CardGlass;
            
            var ol = card.GetComponent<Outline>();
            ol.effectColor = new Color(Accent1.r, Accent1.g, Accent1.b, 0.35f);
            ol.effectDistance = new Vector2(1.5f, -1.5f);
            card.AddComponent<GradientBorderPulse>().outline = ol;
            
            var sh = card.GetComponent<Shadow>();
            sh.effectColor = new Color(0, 0, 0, 0.7f);
            sh.effectDistance = new Vector2(8, -8);

            // Glass Overlay Layer
            var glass = MakeGO("GlassOverlay", card.transform, typeof(Image));
            var grt = glass.GetComponent<RectTransform>();
            Stretch(grt);
            grt.offsetMin = new Vector2(4, 4); grt.offsetMax = new Vector2(-4, -4);
            glass.GetComponent<Image>().color = new Color(1, 1, 1, 0.03f);
            glass.GetComponent<Image>().raycastTarget = false;

            MakeAccentBar(card.transform, true);
            MakeAccentBar(card.transform, false);

            // ── Parallax ──
            var parallax = _settingsPanel.AddComponent<MenuParallaxEffect>();
            parallax.smoothTime = 0.12f;
            parallax.useUnscaledTime = true;
            parallax.uiLayers = new RectTransform[] { crt };
            parallax.layerStrengths = new float[] { 12f }; 
            parallax.CaptureInitialState();

            var animTargets = new List<RectTransform>();

            // ── HEADER ──
            var titleGO = Stagger(card.transform, "TitleRow", new Vector2(0, 390), new Vector2(500, 60), -1f);
            animTargets.Add(titleGO.GetComponent<RectTransform>());
            var titleTMP = AddTMP(titleGO.transform, "SetTitle", Loc("Menu_Settings", "AYARLAR"),
                44, FontStyles.Bold, Accent1, new Vector2(0, 5), new Vector2(500, 60));
            titleTMP.characterSpacing = 12;
            titleTMP.enableVertexGradient = true;
            titleTMP.colorGradient = new VertexGradient(Accent1, Accent2, Accent1, Accent2);

            var subTMP = AddTMP(titleGO.transform, "SetSub", Loc("Settings_Subtitle", "Oyun tercihlerinizi özelleştirin"),
                13, FontStyles.Italic, TextFaded, new Vector2(0, -32), new Vector2(500, 24));

            var sepGO = Stagger(card.transform, "SepRow", new Vector2(0, 335), new Vector2(480, 2), 1f);
            animTargets.Add(sepGO.GetComponent<RectTransform>());
            sepGO.AddComponent<Image>().color = SepColor;

            // ── SETTINGS ROWS ──
            var model = new Settings.SettingsModel();
            float startY = 265f; 
            float rowSp = 68f; 
            float currentY = startY;

            // Buildup Rows
            var r1 = BuildSliderRow(card.transform, Loc("Settings_Music", "MÜZİK SESİ"), model.MusicVolume, currentY, -1f, (val) =>
            {
                model.SetMusicVolume(val);
                if (Settings.AudioManager.Instance) Settings.AudioManager.Instance.SetMusicVolume(model.MusicEnabled ? val : 0);
            });
            animTargets.Add(r1.GetComponent<RectTransform>());

            currentY -= rowSp;
            var r2 = BuildSliderRow(card.transform, Loc("Settings_SFX", "EFEKT SESİ"), model.SFXVolume, currentY, 1f, (val) =>
            {
                model.SetSFXVolume(val);
                if (Settings.AudioManager.Instance) Settings.AudioManager.Instance.SetSFXVolume(model.SFXEnabled ? val : 0);
            });
            animTargets.Add(r2.GetComponent<RectTransform>());

            currentY -= rowSp;
            var r3 = BuildToggleRow(card.transform, Loc("Settings_EnableMusic", "MÜZİK"), model.MusicEnabled, currentY, -1f, (on) =>
            {
                model.SetMusicEnabled(on);
                if (Settings.AudioManager.Instance) Settings.AudioManager.Instance.SetMusicVolume(on ? model.MusicVolume : 0);
            });
            animTargets.Add(r3.GetComponent<RectTransform>());

            currentY -= rowSp;
            var r4 = BuildToggleRow(card.transform, Loc("Settings_EnableSFX", "EFEKTLER"), model.SFXEnabled, currentY, 1f, (on) =>
            {
                model.SetSFXEnabled(on);
                if (Settings.AudioManager.Instance) Settings.AudioManager.Instance.SetSFXVolume(on ? model.SFXVolume : 0);
            });
            animTargets.Add(r4.GetComponent<RectTransform>());

            currentY -= rowSp;
            var r5 = BuildToggleRow(card.transform, Loc("Settings_Haptic", "TİTREŞİM"), model.HapticEnabled, currentY, -1f, (on) =>
            {
                model.SetHapticEnabled(on);
            });
            animTargets.Add(r5.GetComponent<RectTransform>());

            // Section 2: Kontroller
            currentY -= (rowSp + 15);
            var sec2 = Stagger(card.transform, "Section_Controls", new Vector2(0, currentY + 35), new Vector2(400, 1), 1f);
            animTargets.Add(sec2.GetComponent<RectTransform>());
            sec2.AddComponent<Image>().color = new Color(Accent1.r, Accent1.g, Accent1.b, 0.15f);

            // Kalibrasyon satırı
            GameObject calibRow = null;

            var r6 = BuildCycleRow(card.transform, Loc("Settings_ControlMethod", "KONTROL TİPİ"), model.ControlMethod, 
                new string[] { Loc("Settings_ControlMethod_Buttons", "BUTONLAR"), Loc("Settings_ControlMethod_Tilt", "TİLT") }, currentY, 1f, (val) =>
            {
                model.SetControlMethod(val);
                if (calibRow != null) calibRow.SetActive(val == 1);
            });
            animTargets.Add(r6.GetComponent<RectTransform>());

            currentY -= rowSp;
            var r7 = BuildCycleRow(card.transform, Loc("Settings_AccelMode", "İVME MODU"), model.AccelerationMode, 
                new string[] { Loc("Settings_Accel_Manual", "MANUEL"), Loc("Settings_Accel_Auto", "OTO") }, currentY, -1f, (val) =>
            {
                model.SetAccelerationMode(val);
            });
            animTargets.Add(r7.GetComponent<RectTransform>());

            currentY -= rowSp;
            var r8 = BuildSliderRow(card.transform, Loc("Settings_Sensitivity", "HASSASİYET"), model.ControlSensitivity, currentY, 1f, (val) =>
            {
                model.SetControlSensitivity(val);
            });
            animTargets.Add(r8.GetComponent<RectTransform>());

            currentY -= rowSp;
            calibRow = BuildActionRow(card.transform, Loc("Settings_Calibrate", "CİHAZI KALİBRE ET"), Loc("Settings_Calibrate_Btn", "KALİBRE ET"), currentY, -1f, () =>
            {
                float currentX = Input.acceleration.x;
                model.SetAccelerometerOffset(currentX);
                Debug.Log($"İvmeölçer kalibre edildi: {currentX}");
            });
            calibRow.SetActive(model.ControlMethod == 1);
            animTargets.Add(calibRow.GetComponent<RectTransform>());

            // ── FOOTER ──
            var backGO = Stagger(card.transform, "BtnBack", new Vector2(0, -420), new Vector2(440, 64), 1f);
            animTargets.Add(backGO.GetComponent<RectTransform>());
            MakeButton(backGO, Loc("Settings_Back", "GERİ DÖN"), Accent2, true, () => ToggleSettingsPanel());

            _settingsOpen = true;

            var cg = _settingsPanel.GetComponent<CanvasGroup>();
            var panelAnim = _settingsPanel.AddComponent<SettingsPanelAnimator>();
            panelAnim.cg = cg;
            panelAnim.cardRT = crt;
            panelAnim.targets = animTargets;
        }

        static GameObject BuildCycleRow(Transform parent, string label, int currentIndex, string[] options, float y, float staggerDir, System.Action<int> onChange)
        {
            var rowGO = Stagger(parent, label + "Row", new Vector2(0, y), new Vector2(540, 40), staggerDir);

            var lblTmp = AddTMP(rowGO.transform, label + "Lbl", label, 15, FontStyles.Bold, TextFaded,
                new Vector2(-160, 0), new Vector2(280, 24));
            lblTmp.alignment = TextAlignmentOptions.Right;

            var selectorGO = new GameObject(label + "Selector", typeof(RectTransform));
            selectorGO.transform.SetParent(rowGO.transform, false);
            var srt = selectorGO.GetComponent<RectTransform>();
            srt.anchorMin = srt.anchorMax = new Vector2(0.5f, 0.5f);
            srt.sizeDelta = new Vector2(240, 32);
            srt.anchoredPosition = new Vector2(100, 0);

            var valTMP = AddTMP(selectorGO.transform, "Val", options[currentIndex], 15, FontStyles.Bold, TextWhite,
                Vector2.zero, new Vector2(160, 32));
            valTMP.alignment = TextAlignmentOptions.Center;

            int currentVal = currentIndex;

            // Sol Buton
            var leftBtn = new GameObject("Left", typeof(RectTransform), typeof(Image), typeof(Button));
            leftBtn.transform.SetParent(selectorGO.transform, false);
            var lrt = leftBtn.GetComponent<RectTransform>();
            lrt.anchoredPosition = new Vector2(-100, 0);
            lrt.sizeDelta = new Vector2(32, 32);
            leftBtn.GetComponent<Image>().color = new Color(1, 1, 1, 0.1f);
            AddTMP(leftBtn.transform, "T", "<", 18, FontStyles.Bold, Accent1, Vector2.zero, lrt.sizeDelta);
            
            // Sağ Buton
            var rightBtn = new GameObject("Right", typeof(RectTransform), typeof(Image), typeof(Button));
            rightBtn.transform.SetParent(selectorGO.transform, false);
            var rrt = rightBtn.GetComponent<RectTransform>();
            rrt.anchoredPosition = new Vector2(100, 0);
            rrt.sizeDelta = new Vector2(32, 32);
            rightBtn.GetComponent<Image>().color = new Color(1, 1, 1, 0.1f);
            AddTMP(rightBtn.transform, "T", ">", 18, FontStyles.Bold, Accent1, Vector2.zero, rrt.sizeDelta);

            System.Action updateVal = () => {
                valTMP.text = options[currentVal];
                onChange?.Invoke(currentVal);
                if (Settings.AudioManager.Instance) Settings.AudioManager.Instance.PlayClickSound();
            };

            leftBtn.GetComponent<Button>().onClick.AddListener(() => {
                currentVal = (currentVal - 1 + options.Length) % options.Length;
                updateVal();
            });

            rightBtn.GetComponent<Button>().onClick.AddListener(() => {
                currentVal = (currentVal + 1) % options.Length;
                updateVal();
            });

            return rowGO;
        }

        static GameObject BuildActionRow(Transform parent, string label, string btnText, float y, float staggerDir, System.Action onAction)
        {
            var rowGO = Stagger(parent, label + "Row", new Vector2(0, y), new Vector2(540, 40), staggerDir);

            var lblTmp = AddTMP(rowGO.transform, label + "Lbl", label, 15, FontStyles.Bold, TextFaded,
                new Vector2(-160, 0), new Vector2(280, 24));
            lblTmp.alignment = TextAlignmentOptions.Right;

            var btnGO = new GameObject(label + "Btn", typeof(RectTransform), typeof(Image), typeof(Button));
            btnGO.transform.SetParent(rowGO.transform, false);
            var brt = btnGO.GetComponent<RectTransform>();
            brt.anchorMin = brt.anchorMax = new Vector2(0.5f, 0.5f);
            brt.sizeDelta = new Vector2(200, 36);
            brt.anchoredPosition = new Vector2(100, 0);
            btnGO.GetComponent<Image>().color = new Color(Accent2.r, Accent2.g, Accent2.b, 0.2f);
            
            AddTMP(btnGO.transform, "T", btnText, 14, FontStyles.Bold, TextWhite, Vector2.zero, brt.sizeDelta);

            btnGO.GetComponent<Button>().onClick.AddListener(() => {
                onAction?.Invoke();
                if (Settings.AudioManager.Instance) Settings.AudioManager.Instance.PlayClickSound();
            });

            return rowGO;
        }

        static GameObject BuildSliderRow(Transform parent, string label, float value, float y, float staggerDir, System.Action<float> onChange)
        {
            var rowGO = Stagger(parent, label + "Row", new Vector2(0, y), new Vector2(580, 52), staggerDir);
            
            // Row Background (Subtle glass)
            var bgRow = rowGO.AddComponent<Image>();
            bgRow.color = new Color(1, 1, 1, 0.04f);

            var lblTmp = AddTMP(rowGO.transform, label + "Lbl", label, 16, FontStyles.Bold, TextFaded,
                new Vector2(-170, 0), new Vector2(240, 30));
            lblTmp.alignment = TextAlignmentOptions.Right;
            lblTmp.characterSpacing = 2;

            // Slider Container
            var sliderGO = new GameObject(label + "Slider", typeof(RectTransform), typeof(Slider));
            sliderGO.transform.SetParent(rowGO.transform, false);
            var srt = sliderGO.GetComponent<RectTransform>();
            srt.anchorMin = srt.anchorMax = new Vector2(0.5f, 0.5f);
            srt.sizeDelta = new Vector2(240, 10); 
            srt.anchoredPosition = new Vector2(90, 0);

            // Technical Track
            var track = MakeGO("Track", sliderGO.transform, typeof(Image), typeof(Outline));
            Stretch(track.GetComponent<RectTransform>());
            track.GetComponent<Image>().color = new Color(0.01f, 0.01f, 0.05f, 0.95f);
            var toat = track.GetComponent<Outline>();
            toat.effectColor = new Color(Accent1.r, Accent1.g, Accent1.b, 0.15f);
            toat.effectDistance = new Vector2(1, -1);

            var fillArea = MakeGO("FillArea", sliderGO.transform);
            Stretch(fillArea.GetComponent<RectTransform>());

            var fill = MakeGO("Fill", fillArea.transform, typeof(Image));
            Stretch(fill.GetComponent<RectTransform>());
            fill.GetComponent<Image>().color = Accent1;

            var handleArea = MakeGO("HandleArea", sliderGO.transform);
            Stretch(handleArea.GetComponent<RectTransform>());

            var handle = new GameObject("Handle", typeof(RectTransform), typeof(Image), typeof(Outline), typeof(Shadow));
            handle.transform.SetParent(handleArea.transform, false);
            var hrt = handle.GetComponent<RectTransform>();
            hrt.sizeDelta = new Vector2(12, 32); 
            handle.GetComponent<Image>().color = new Color(0.05f, 0.05f, 0.1f, 1f);
            handle.GetComponent<Outline>().effectColor = Accent1;
            handle.GetComponent<Outline>().effectDistance = new Vector2(1.5f, -1.5f);
            
            handle.GetComponent<Shadow>().effectColor = new Color(0,0,0,0.6f);
            handle.GetComponent<Shadow>().effectDistance = new Vector2(4, -4);

            var valTMP = AddTMP(rowGO.transform, label + "Val", Mathf.RoundToInt(value * 100) + "%",
                14, FontStyles.Bold, Accent1, new Vector2(250, 0), new Vector2(60, 24));
            valTMP.alignment = TextAlignmentOptions.Left;

            var slider = sliderGO.GetComponent<Slider>();
            slider.fillRect = fill.GetComponent<RectTransform>();
            slider.handleRect = hrt;
            slider.targetGraphic = handle.GetComponent<Image>();
            slider.minValue = 0f; slider.maxValue = 1f;
            slider.value = value;
            slider.onValueChanged.AddListener((v) =>
            {
                onChange?.Invoke(v);
                if (valTMP) valTMP.text = Mathf.RoundToInt(v * 100) + "%";
            });

            return rowGO;
        }

        static GameObject BuildToggleRow(Transform parent, string label, bool isOn, float y, float staggerDir, System.Action<bool> onChange)
        {
            var rowGO = Stagger(parent, label + "Row", new Vector2(0, y), new Vector2(580, 52), staggerDir);
            
            var bgRow = rowGO.AddComponent<Image>();
            bgRow.color = new Color(1, 1, 1, 0.04f);

            var lblTmp = AddTMP(rowGO.transform, label + "Lbl", label, 16, FontStyles.Bold, TextFaded,
                new Vector2(-170, 0), new Vector2(240, 30));
            lblTmp.alignment = TextAlignmentOptions.Right;
            lblTmp.characterSpacing = 2;

            // Toggle base (Track)
            var togGO = new GameObject(label + "Toggle", typeof(RectTransform), typeof(Image), typeof(Toggle), typeof(Outline));
            togGO.transform.SetParent(rowGO.transform, false);
            var trt = togGO.GetComponent<RectTransform>();
            trt.anchorMin = trt.anchorMax = new Vector2(0.5f, 0.5f);
            trt.sizeDelta = new Vector2(60, 22); 
            trt.anchoredPosition = new Vector2(30, 0); 
            
            var bgImg = togGO.GetComponent<Image>();
            bgImg.color = new Color(0.01f, 0.01f, 0.08f, 0.95f);
            togGO.GetComponent<Outline>().effectColor = new Color(1, 1, 1, 0.1f);

            // Handle (Cyber Block)
            var checkGO = new GameObject("Check", typeof(RectTransform), typeof(Image), typeof(Shadow), typeof(Outline));
            checkGO.transform.SetParent(togGO.transform, false);
            var chrt = checkGO.GetComponent<RectTransform>();
            chrt.sizeDelta = new Vector2(26, 26); 
            
            var handleImg = checkGO.GetComponent<Image>();
            handleImg.color = isOn ? ToggleOn : new Color(0.6f, 0.6f, 0.65f);
            checkGO.GetComponent<Outline>().effectColor = isOn ? new Color(1, 1, 1, 0.4f) : new Color(0, 0, 0, 0);
            checkGO.GetComponent<Outline>().effectDistance = new Vector2(1, -1);

            var sh = checkGO.GetComponent<Shadow>();
            sh.effectColor = new Color(0, 0, 0, 0.5f);
            sh.effectDistance = new Vector2(2, -2);

            var toggle = togGO.GetComponent<Toggle>();
            toggle.targetGraphic = bgImg;
            toggle.graphic = null; 

            var smoothAnim = togGO.AddComponent<SmoothToggleAnimator>();
            smoothAnim.handle = chrt;
            smoothAnim.bgImage = bgImg;
            smoothAnim.onColor = new Color(0.12f, 0.12f, 0.25f, 0.95f); // Soft glow on track
            smoothAnim.offColor = new Color(0.01f, 0.01f, 0.08f, 0.95f);
            smoothAnim.onX = 18f;
            smoothAnim.offX = -18f;
            smoothAnim.SetState(isOn, true);

            toggle.isOn = isOn;
            toggle.onValueChanged.AddListener((val) =>
            {
                if (Settings.AudioManager.Instance) Settings.AudioManager.Instance.PlayClickSound();
                smoothAnim.SetState(val); 
                handleImg.color = val ? ToggleOn : new Color(0.6f, 0.6f, 0.65f);
                checkGO.GetComponent<Outline>().effectColor = val ? new Color(1, 1, 1, 0.4f) : new Color(0, 0, 0, 0);
                onChange?.Invoke(val);
            });

            return rowGO;
        }

        class SettingsPanelAnimator : MonoBehaviour
        {
            public CanvasGroup cg;
            public RectTransform cardRT;
            public List<RectTransform> targets = new List<RectTransform>();
            
            float t;
            bool closing;
            System.Action onDone;

            void OnEnable()
            {
                t = 0f; closing = false;
                if (cg) cg.alpha = 0f;
                if (cardRT) cardRT.localScale = Vector3.one * 0.9f;
                
                foreach (var rt in targets)
                {
                    if (!rt) continue;
                    var c = rt.GetComponent<CanvasGroup>();
                    if (c) c.alpha = 0f;
                    rt.localScale = Vector3.one * 0.9f;
                }
            }

            void Update()
            {
                if (closing) { CloseUpdate(); return; }

                t += Time.unscaledDeltaTime;

                // Ana kartın ve arka planın belirmesi
                float p = Mathf.Clamp01(t / 0.25f);
                float e = 1f - Mathf.Pow(1f - p, 3f);
                if (cg) cg.alpha = e;
                if (cardRT) cardRT.localScale = Vector3.Lerp(Vector3.one * 0.9f, Vector3.one, e);

                // Satırların sırayla (Stagger) gelmesi
                for (int i = 0; i < targets.Count; i++)
                {
                    if (!targets[i]) continue;
                    float s = 0.1f + i * 0.05f; // Gecikme süresi
                    float tp = Mathf.Clamp01((t - s) / 0.35f);
                    float te = 1f - Mathf.Pow(1f - tp, 3f) * (1f - tp * 1.2f); // Hafif esneme (OutBack) efekti

                    var c2 = targets[i].GetComponent<CanvasGroup>();
                    if (c2) c2.alpha = Mathf.Clamp01(tp * 2f);

                    var orig = targets[i].GetComponent<SOrig>();
                    if (orig)
                    {
                        // SOrig'deki "side" değerine göre sağdan (+1) veya soldan (-1) kayarak gel
                        Vector2 startPos = orig.pos + new Vector2(orig.side * 100f, 0f); 
                        targets[i].anchoredPosition = Vector2.LerpUnclamped(startPos, orig.pos, te);
                    }
                    targets[i].localScale = Vector3.LerpUnclamped(Vector3.one * 0.8f, Vector3.one, te);
                }
            }

            public void Close(System.Action cb) { closing = true; onDone = cb; t = 0f; }

            void CloseUpdate()
            {
                t += Time.unscaledDeltaTime;
                float p = Mathf.Clamp01(t / 0.2f);
                if (cg) cg.alpha = 1f - p * p;
                if (cardRT) cardRT.localScale = Vector3.Lerp(Vector3.one, Vector3.one * 0.9f, p * p);
                if (p >= 1f) onDone?.Invoke();
            }
        }



        // ═════════════════════════════════════════════════════════════════
        // SHARED BUILDERS
        // ═════════════════════════════════════════════════════════════════
        static void MakeButton(GameObject go, string label, Color accent, bool primary, System.Action onClick)
        {
            var rrt = go.GetComponent<RectTransform>();
            var img = go.AddComponent<Image>();
            
            var btn = go.AddComponent<Button>();
            btn.transition = Selectable.Transition.None; 
            if (onClick != null) btn.onClick.AddListener(() => 
            {
                if (Settings.AudioManager.Instance) Settings.AudioManager.Instance.PlayClickSound();
                onClick();
            });

            // Entegre Akıcı Animatör
            var fluidAnim = go.AddComponent<FluidButtonAnimator>();
            fluidAnim.targetGraphic = img;

            if (primary)
            {
                // Luxury Main Button Concept — Solid warm gold foundation
                fluidAnim.normalColor = accent;
                fluidAnim.hoverColor = Color.white; // Flash white on hover
                fluidAnim.pressColor = Color.Lerp(accent, Color.black, 0.2f);

                // Breathing Glow for Primary
                var o = go.AddComponent<Outline>();
                o.effectColor = new Color(accent.r, accent.g, accent.b, 0.4f);
                o.effectDistance = new Vector2(2.5f, -2.5f);
                var pulse = go.AddComponent<PrimaryGlowPulse>();
                pulse.glowOutline = o;
            }
            else
            {
                // Premium Ghost Button Concept — Glassy borders, phantom depth
                fluidAnim.normalColor = new Color(accent.r, accent.g, accent.b, 0.04f); 
                fluidAnim.hoverColor = new Color(accent.r, accent.g, accent.b, 0.25f);
                fluidAnim.pressColor = new Color(accent.r, accent.g, accent.b, 0.4f);

                var o = go.AddComponent<Outline>();
                o.effectColor = new Color(accent.r, accent.g, accent.b, 0.75f); // High-contrast border
                o.effectDistance = new Vector2(1.2f, -1.2f);
            }

            // Typography: High contrast for primary, luxury spacing for others
            Color tc = primary ? new Color(0.05f, 0.04f, 0.02f, 1f) : TextWhite;
            var t = AddTMP(go.transform, "L", label, primary ? 21 : 18,
                FontStyles.Bold, tc, Vector2.zero, new Vector2(400, 44));
            
            // Modern character spacing for premium look
            t.characterSpacing = primary ? 8 : 4; 
        }

        static void MakeBar(Transform parent, float xOff, Color color)
        {
            var b = new GameObject("Bar", typeof(RectTransform), typeof(Image));
            b.transform.SetParent(parent, false);
            var rt = b.GetComponent<RectTransform>();
            rt.anchoredPosition = new Vector2(xOff, 0);
            rt.sizeDelta = new Vector2(13, 44);
            b.GetComponent<Image>().color = color;
        }

        static void MakeAccentBar(Transform card, bool top)
        {
            var go = new GameObject("AccB", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(card, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.08f, top ? 1f : 0f);
            rt.anchorMax = new Vector2(0.92f, top ? 1f : 0f);
            rt.pivot = new Vector2(0.5f, top ? 1f : 0f);
            rt.sizeDelta = new Vector2(0, 2);
            rt.anchoredPosition = Vector2.zero;

            // Gradient simulation: left=Accent1, right=Accent2
            float blend = top ? 0f : 1f;
            Color c = Color.Lerp(Accent1, Accent2, 0.5f);
            go.GetComponent<Image>().color = new Color(c.r, c.g, c.b, 0.5f);
            go.GetComponent<Image>().raycastTarget = false;
        }

        static void SpawnMotes(Transform parent)
        {
            var h = MakeGO("Motes", parent);
            Stretch(h.GetComponent<RectTransform>());
            var fm = h.AddComponent<FloatingMotes>();
            var rng = new System.Random(42);
            for (int i = 0; i < 18; i++)
            {
                var d = new GameObject($"M{i}", typeof(RectTransform), typeof(Image));
                d.transform.SetParent(h.transform, false);
                var rt = d.GetComponent<RectTransform>();
                rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
                float sz = 2f + (float)rng.NextDouble() * 3.5f;
                rt.sizeDelta = new Vector2(sz, sz);
                rt.anchoredPosition = new Vector2(
                    (float)(rng.NextDouble() * 1000 - 500),
                    (float)(rng.NextDouble() * 1600 - 800));
                var img = d.GetComponent<Image>();
                Color c = rng.NextDouble() > 0.5 ? Accent1 : Accent2;
                img.color = new Color(c.r, c.g, c.b, 0.06f);
                img.raycastTarget = false;
                fm.dots.Add(rt);
                fm.imgs.Add(img);
            }
        }

        // ─── GENERIC HELPERS ────────────────────────────────────────────
        static GameObject MakeGO(string name, Transform parent, params System.Type[] types)
        {
            var list = new List<System.Type> { typeof(RectTransform) };
            list.AddRange(types);
            var go = new GameObject(name, list.ToArray());
            go.transform.SetParent(parent, false);
            return go;
        }

        static GameObject Stagger(Transform parent, string name, Vector2 pos, Vector2 size, float side)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasGroup));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = size;
            rt.anchoredPosition = pos;

            var o = go.AddComponent<SOrig>();
            o.pos = pos;
            o.side = side; // Fırlama yönü

            go.GetComponent<CanvasGroup>().alpha = 0f;
            return go;
        }

        static TextMeshProUGUI AddTMP(Transform parent, string name, string text,
            int size, FontStyles style, Color color, Vector2 pos, Vector2 dim)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = dim;
            rt.anchoredPosition = pos;
            var t = go.GetComponent<TextMeshProUGUI>();
            t.text = text; t.fontSize = size; t.fontStyle = style;
            t.alignment = TextAlignmentOptions.Center;
            t.color = color; t.raycastTarget = false;

            // Ensure Turkish Fallback
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

        static void Stretch(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
        }

        static string Loc(string key, string fb)
        {
            return LocalizationManager.Instance != null
                ? LocalizationManager.Instance.GetTranslation(key)
                : fb;
        }

        static void EnsureEventSystem()
        {
            var es = Object.FindFirstObjectByType<EventSystem>(FindObjectsInactive.Include);
            if (es) { if (!es.gameObject.activeSelf) es.gameObject.SetActive(true); return; }
            var go = new GameObject("EventSystem", typeof(EventSystem));
#if PACKAGE_INPUT_SYSTEM_EXISTS || UNITY_INPUT_SYSTEM
            go.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
#else
            go.AddComponent<StandaloneInputModule>();
#endif
            if (Application.isPlaying) Object.DontDestroyOnLoad(go);
        }
    }
}
