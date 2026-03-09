using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

namespace Gazze.UI
{
    public class GameOverPanelBuilder
    {
        // ─── DESIGN TOKENS ──────────────────────────────────────────────
        static readonly Color BgDim          = new Color(0f, 0f, 0f, 0.75f);
        static readonly Color CardBg         = new Color32(14, 18, 32, 240);
        static readonly Color CardBorder     = new Color32(45, 55, 90, 200);
        static readonly Color AccentCyan     = new Color32(0, 210, 255, 255);
        static readonly Color AccentGold     = new Color32(255, 200, 50, 255);
        static readonly Color AccentRed      = new Color32(255, 75, 80, 255);
        static readonly Color TextPrimary    = new Color32(235, 240, 255, 255);
        static readonly Color TextSecondary  = new Color32(130, 145, 185, 255);
        static readonly Color StatCardBg     = new Color32(25, 30, 55, 220);
        static readonly Color SeparatorColor = new Color32(0, 210, 255, 40);

        // Button palette
        static readonly Color BtnRestart  = new Color32(50, 215, 120, 255);
        static readonly Color BtnMenu     = new Color32(60, 130, 255, 255);
        static readonly Color BtnShare    = new Color32(255, 155, 40, 255);
        static readonly Color BtnSettings = new Color32(180, 80, 220, 255);

        // ─── MAIN ANIMATOR ──────────────────────────────────────────────
        class GameOverPanelAnimator : MonoBehaviour
        {
            public CanvasGroup cg;
            public List<RectTransform> staggerTargets = new List<RectTransform>();
            public TextMeshProUGUI scoreCountTMP;   // Skor sayma animasyonu
            public int targetScore;                  // Hedef skor değeri
            public TextMeshProUGUI titleTMP;         // Başlık glow pulse
            public RectTransform cardRT;             // Kart scale animasyonu

            public float fadeDuration = 0.5f;
            public float staggerDelay = 0.07f;

            float t;
            bool done;
            bool countDone;
            float countTimer;
            int displayedScore;

            void OnEnable()
            {
                t = 0f;
                done = false;
                countDone = false;
                countTimer = 0f;
                displayedScore = 0;
                if (cg != null) cg.alpha = 0f;

                // Set initial states
                foreach (var rt in staggerTargets)
                {
                    if (rt == null) continue;
                    var cg2 = rt.GetComponent<CanvasGroup>();
                    if (cg2 != null) cg2.alpha = 0f;
                    rt.localScale = new Vector3(0.85f, 0.85f, 1f);
                }

                // Card starts smaller
                if (cardRT != null) cardRT.localScale = new Vector3(0.9f, 0.9f, 1f);
            }

            void Update()
            {
                t += Time.unscaledDeltaTime;

                // ── Phase 1: Overlay fade + card scale ──
                float overlayT = Mathf.Clamp01(t / (fadeDuration * 0.5f));
                float overlayEase = 1f - Mathf.Pow(1f - overlayT, 2f);
                if (cg != null) cg.alpha = overlayEase;

                if (cardRT != null)
                {
                    float cardT = Mathf.Clamp01((t - 0.05f) / 0.4f);
                    float cardEase = 1f - Mathf.Pow(1f - cardT, 3f);
                    cardRT.localScale = Vector3.Lerp(new Vector3(0.9f, 0.9f, 1f), Vector3.one, cardEase);
                }

                // ── Phase 2: Stagger children (slide up + fade + scale) ──
                for (int i = 0; i < staggerTargets.Count; i++)
                {
                    if (staggerTargets[i] == null) continue;
                    float childStart = 0.2f + i * staggerDelay;
                    float childT = Mathf.Clamp01((t - childStart) / 0.35f);
                    float ease = 1f - Mathf.Pow(1f - childT, 3f);

                    var cg2 = staggerTargets[i].GetComponent<CanvasGroup>();
                    if (cg2 != null) cg2.alpha = ease;

                    // Slide up from 40px below
                    var origin = staggerTargets[i].GetComponent<StaggerOrigin>();
                    float baseY = origin != null ? origin.originY : staggerTargets[i].anchoredPosition.y;
                    staggerTargets[i].anchoredPosition =
                        new Vector2(staggerTargets[i].anchoredPosition.x, Mathf.Lerp(baseY - 40f, baseY, ease));

                    // Scale from 0.85 to 1.0
                    staggerTargets[i].localScale = Vector3.Lerp(new Vector3(0.85f, 0.85f, 1f), Vector3.one, ease);
                }

                // ── Phase 3: Score count-up ──
                if (scoreCountTMP != null && !countDone)
                {
                    float countStart = 0.5f;
                    if (t > countStart)
                    {
                        countTimer += Time.unscaledDeltaTime;
                        float countDuration = Mathf.Clamp(targetScore * 0.003f, 0.4f, 1.5f);
                        float countProgress = Mathf.Clamp01(countTimer / countDuration);
                        // Ease-out for deceleration effect
                        float easeCount = 1f - Mathf.Pow(1f - countProgress, 2f);
                        displayedScore = Mathf.RoundToInt(Mathf.Lerp(0f, targetScore, easeCount));
                        scoreCountTMP.text = displayedScore.ToString();

                        if (countProgress >= 1f)
                        {
                            scoreCountTMP.text = targetScore.ToString();
                            countDone = true;
                        }
                    }
                }

                // ── Phase 4: Title glow pulse (continuous) ──
                if (titleTMP != null && t > 0.3f)
                {
                    float pulse = 0.85f + 0.15f * Mathf.Sin(t * 2.5f);
                    Color c = AccentGold;
                    titleTMP.color = new Color(c.r, c.g, c.b, pulse);
                }

                // Done check
                if (!done && t > fadeDuration + staggerTargets.Count * staggerDelay + 1.5f)
                    done = true;
            }
        }

        // ─── ACCENT LINE PULSE ANIMATOR ─────────────────────────────────
        class AccentLinePulse : MonoBehaviour
        {
            public Image lineImage;
            public Color baseColor;
            public float speed = 1.5f;

            void Update()
            {
                if (lineImage == null) return;
                float a = 0.7f + 0.3f * Mathf.Sin(Time.unscaledTime * speed);
                lineImage.color = new Color(baseColor.r, baseColor.g, baseColor.b, a);
            }
        }

        // ─── STAGGER ORIGIN ─────────────────────────────────────────────
        class StaggerOrigin : MonoBehaviour
        {
            public float originY;
        }

        // ─── DATA ───────────────────────────────────────────────────────
        public class Data
        {
            public int score;
            public int highScore;
            public int level;
            public float playTimeSeconds;
            public List<string> achievements;
            public System.Action onRestart;
            public System.Action onMainMenu;
            public System.Action onShare;
            public System.Action onSettings;
        }

        // ─── BUILD ──────────────────────────────────────────────────────
        public static GameObject Build(Data data)
        {
            // Canvas setup
            GameObject canvasGO = new GameObject("GameOverCanvas",
                typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            Canvas canvas = canvasGO.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;
            CanvasScaler scaler = canvasGO.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.matchWidthOrHeight = 0.5f;

            EnsureEventSystem();

            // ScreenshotShareManager'ı garantile
            EnsureScreenshotShareManager(canvasGO);

            // ── Full-screen dimmer ──
            GameObject dimGO = new GameObject("Dimmer", typeof(RectTransform), typeof(Image), typeof(CanvasGroup));
            dimGO.transform.SetParent(canvasGO.transform, false);
            Stretch(dimGO.GetComponent<RectTransform>());
            dimGO.GetComponent<Image>().color = BgDim;

            var animator = dimGO.AddComponent<GameOverPanelAnimator>();
            animator.cg = dimGO.GetComponent<CanvasGroup>();

            // ── Central card ──
            GameObject cardGO = new GameObject("Card", typeof(RectTransform), typeof(Image), typeof(Outline));
            cardGO.transform.SetParent(dimGO.transform, false);
            RectTransform cardRt = cardGO.GetComponent<RectTransform>();
            cardRt.anchorMin = new Vector2(0.5f, 0.5f);
            cardRt.anchorMax = new Vector2(0.5f, 0.5f);
            cardRt.sizeDelta = new Vector2(680, 820);
            cardRt.anchoredPosition = new Vector2(0, 30);
            cardGO.GetComponent<Image>().color = CardBg;
            var outline = cardGO.GetComponent<Outline>();
            outline.effectColor = CardBorder;
            outline.effectDistance = new Vector2(1.5f, -1.5f);

            animator.cardRT = cardRt;

            // ── Top accent line (pulsing) ──
            var topLine = CreateAccentLine(cardGO.transform, new Vector2(0f, 1f), new Vector2(1f, 1f), 3f, AccentCyan);
            var topPulse = topLine.AddComponent<AccentLinePulse>();
            topPulse.lineImage = topLine.GetComponent<Image>();
            topPulse.baseColor = AccentCyan;
            topPulse.speed = 2f;

            // ═════════════════════════════════════════════════════════════
            // CONTENT
            // ═════════════════════════════════════════════════════════════

            var staggerList = new List<RectTransform>();

            // (1) TITLE
            string titleStr = Loc("Game_GameOver", "OYUN SONU");
            var titleGO = MakeStaggerChild(cardGO.transform, "Title", new Vector2(0, 340), new Vector2(600, 70));
            staggerList.Add(titleGO.GetComponent<RectTransform>());

            var titleTMP = titleGO.AddComponent<TextMeshProUGUI>();
            titleTMP.text = titleStr;
            titleTMP.fontSize = 52;
            titleTMP.fontStyle = FontStyles.Bold;
            titleTMP.alignment = TextAlignmentOptions.Center;
            titleTMP.color = AccentGold;
            titleTMP.characterSpacing = 6;
            titleTMP.raycastTarget = false;
            animator.titleTMP = titleTMP;

            // (2) Separator under title
            var sep1 = MakeStaggerChild(cardGO.transform, "Sep1", new Vector2(0, 295), new Vector2(500, 2));
            staggerList.Add(sep1.GetComponent<RectTransform>());
            sep1.AddComponent<Image>().color = SeparatorColor;

            // (3) Score — big hero number with count-up animation
            var scoreGO = MakeStaggerChild(cardGO.transform, "Score", new Vector2(0, 225), new Vector2(600, 90));
            staggerList.Add(scoreGO.GetComponent<RectTransform>());

            string scoreLabel = Loc("Game_Score", "SKOR");
            var scoreLabelTMP = AddTMP(scoreGO.transform, "Label", scoreLabel, 18,
                FontStyles.Bold, TextSecondary, new Vector2(0, 25), new Vector2(300, 28));
            scoreLabelTMP.characterSpacing = 4;

            // Score number starts at 0, animator will count up
            var scoreNumTMP = AddTMP(scoreGO.transform, "Num", "0", 56,
                FontStyles.Bold, TextPrimary, new Vector2(0, -15), new Vector2(400, 65));
            animator.scoreCountTMP = scoreNumTMP;
            animator.targetScore = data.score;

            // New high score badge
            bool isNewHigh = data.score >= data.highScore && data.score > 0;
            if (isNewHigh)
            {
                string newHS = Loc("Game_NewHighScore", "YENI REKOR!");
                var badgeGO = new GameObject("Badge", typeof(RectTransform), typeof(Image));
                badgeGO.transform.SetParent(scoreGO.transform, false);
                var badgeRt = badgeGO.GetComponent<RectTransform>();
                badgeRt.anchorMin = badgeRt.anchorMax = new Vector2(0.5f, 0.5f);
                badgeRt.sizeDelta = new Vector2(200, 30);
                badgeRt.anchoredPosition = new Vector2(0, -52);
                badgeGO.GetComponent<Image>().color = new Color(AccentGold.r, AccentGold.g, AccentGold.b, 0.18f);

                // Badge pulse animator
                var badgePulse = badgeGO.AddComponent<AccentLinePulse>();
                badgePulse.lineImage = badgeGO.GetComponent<Image>();
                badgePulse.baseColor = new Color(AccentGold.r, AccentGold.g, AccentGold.b, 0.18f);
                badgePulse.speed = 3f;

                AddTMP(badgeGO.transform, "BadgeTxt", "< " + newHS + " >", 14,
                    FontStyles.Bold, AccentGold, Vector2.zero, new Vector2(190, 28));
            }

            // (4) Stat cards row
            var statsRow = MakeStaggerChild(cardGO.transform, "Stats", new Vector2(0, 80), new Vector2(620, 100));
            staggerList.Add(statsRow.GetComponent<RectTransform>());

            var hlg = statsRow.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 12;
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = true;
            hlg.childForceExpandHeight = true;
            hlg.padding = new RectOffset(6, 6, 6, 6);

            string hsLabel = Loc("Game_HighScore", "EN YUKSEK");
            string lvlLabel = Loc("Game_Level", "SEVIYE");
            string timeLabel = Loc("Game_PlayTime", "SURE");

            BuildStatCard(hlg.transform, "HI", hsLabel, data.highScore.ToString(), AccentGold);
            BuildStatCard(hlg.transform, "LV", lvlLabel, data.level.ToString(), AccentCyan);
            BuildStatCard(hlg.transform, "TM", timeLabel, FormatTime(data.playTimeSeconds), new Color32(140, 220, 255, 255));

            // (5) Separator
            var sep2 = MakeStaggerChild(cardGO.transform, "Sep2", new Vector2(0, 20), new Vector2(500, 2));
            staggerList.Add(sep2.GetComponent<RectTransform>());
            sep2.AddComponent<Image>().color = SeparatorColor;

            // (6) Achievements
            string achLabel = Loc("Game_Achievements", "BASARIMLAR");
            string achNone = Loc("Game_None", "-");
            string achText = data.achievements != null && data.achievements.Count > 0
                ? string.Join("  -  ", data.achievements)
                : achNone;

            var achGO = MakeStaggerChild(cardGO.transform, "Ach", new Vector2(0, -35), new Vector2(600, 80));
            staggerList.Add(achGO.GetComponent<RectTransform>());

            var achLabelTMP = AddTMP(achGO.transform, "AchLabel", achLabel, 16,
                FontStyles.Bold, TextSecondary, new Vector2(0, 22), new Vector2(580, 24));
            achLabelTMP.characterSpacing = 3;

            var achValTMP = AddTMP(achGO.transform, "AchVal", achText, 20,
                FontStyles.Italic, new Color32(200, 210, 240, 255), new Vector2(0, -8), new Vector2(580, 50));
            achValTMP.textWrappingMode = TextWrappingModes.Normal;
            achValTMP.overflowMode = TextOverflowModes.Ellipsis;

            // (7) Bottom separator
            var sep3 = MakeStaggerChild(cardGO.transform, "Sep3", new Vector2(0, -90), new Vector2(500, 2));
            staggerList.Add(sep3.GetComponent<RectTransform>());
            sep3.AddComponent<Image>().color = SeparatorColor;

            // (8) Buttons
            string restartStr  = Loc("Game_Restart", "YENIDEN");
            string menuStr     = Loc("Game_MainMenu", "MENU");
            string shareStr    = Loc("Game_Share", "PAYLAS");
            string settingsStr = Loc("Menu_Settings", "AYARLAR");

            // Primary button (restart) – full width
            var primaryBtn = MakeStaggerChild(cardGO.transform, "PrimaryBtn", new Vector2(0, -140), new Vector2(580, 64));
            staggerList.Add(primaryBtn.GetComponent<RectTransform>());
            BuildPremiumButton(primaryBtn, restartStr, BtnRestart, data.onRestart, 24, true);

            // Secondary buttons row
            var secRow = MakeStaggerChild(cardGO.transform, "SecBtns", new Vector2(0, -220), new Vector2(580, 52));
            staggerList.Add(secRow.GetComponent<RectTransform>());

            var secHlg = secRow.AddComponent<HorizontalLayoutGroup>();
            secHlg.spacing = 10;
            secHlg.childAlignment = TextAnchor.MiddleCenter;
            secHlg.childControlWidth = true;
            secHlg.childControlHeight = true;
            secHlg.childForceExpandWidth = true;
            secHlg.childForceExpandHeight = true;

            BuildSecondaryButton(secHlg.transform, menuStr, BtnMenu, data.onMainMenu);

            // Share button → ScreenshotShareManager
            System.Action shareAction = () =>
            {
                if (ScreenshotShareManager.Instance != null)
                {
                    ScreenshotShareManager.Instance.CaptureAndShare(canvasGO);
                }
                else if (data.onShare != null)
                {
                    data.onShare();
                }
            };
            BuildSecondaryButton(secHlg.transform, shareStr, BtnShare, shareAction);

            BuildSecondaryButton(secHlg.transform, settingsStr, BtnSettings, data.onSettings);

            // (9) Bottom accent line (pulsing)
            var bottomLine = CreateAccentLine(cardGO.transform, new Vector2(0f, 0f), new Vector2(1f, 0f), 3f, AccentCyan);
            var bottomPulse = bottomLine.AddComponent<AccentLinePulse>();
            bottomPulse.lineImage = bottomLine.GetComponent<Image>();
            bottomPulse.baseColor = AccentCyan;
            bottomPulse.speed = 2f;

            // ── Particles ──
            CreateParticles(dimGO.transform);

            // ── Wire stagger list ──
            animator.staggerTargets = staggerList;

            return canvasGO;
        }

        // ─── STAT CARD ──────────────────────────────────────────────────
        static void BuildStatCard(Transform parent, string icon, string label, string value, Color accent)
        {
            var go = new GameObject("Stat", typeof(RectTransform), typeof(Image), typeof(Outline));
            go.transform.SetParent(parent, false);
            go.GetComponent<Image>().color = StatCardBg;
            var ol = go.GetComponent<Outline>();
            ol.effectColor = new Color(accent.r, accent.g, accent.b, 0.25f);
            ol.effectDistance = new Vector2(1, -1);

            // Top accent bar
            var bar = new GameObject("Bar", typeof(RectTransform), typeof(Image));
            bar.transform.SetParent(go.transform, false);
            var barRt = bar.GetComponent<RectTransform>();
            barRt.anchorMin = new Vector2(0.15f, 1f);
            barRt.anchorMax = new Vector2(0.85f, 1f);
            barRt.pivot = new Vector2(0.5f, 1f);
            barRt.sizeDelta = new Vector2(0, 2.5f);
            bar.GetComponent<Image>().color = accent;

            // Icon badge (colored background + text)
            var icoBg = new GameObject("IcoBg", typeof(RectTransform), typeof(Image));
            icoBg.transform.SetParent(go.transform, false);
            var icoBgRt = icoBg.GetComponent<RectTransform>();
            icoBgRt.anchorMin = icoBgRt.anchorMax = new Vector2(0.5f, 0.5f);
            icoBgRt.sizeDelta = new Vector2(36, 20);
            icoBgRt.anchoredPosition = new Vector2(0, 20);
            icoBg.GetComponent<Image>().color = new Color(accent.r, accent.g, accent.b, 0.2f);

            AddTMP(icoBg.transform, "IcoTxt", icon, 10, FontStyles.Bold, accent,
                Vector2.zero, new Vector2(34, 18));

            // Label
            var lbl = AddTMP(go.transform, "Lbl", label, 11, FontStyles.Bold, TextSecondary,
                new Vector2(0, -2), new Vector2(140, 18));
            lbl.characterSpacing = 2;

            // Value
            AddTMP(go.transform, "Val", value, 24, FontStyles.Bold, TextPrimary,
                new Vector2(0, -26), new Vector2(140, 34));
        }

        // ─── PREMIUM BUTTON ─────────────────────────────────────────────
        static void BuildPremiumButton(GameObject go, string label, Color color,
            System.Action onClick, int fontSize = 20, bool isPrimary = false)
        {
            var img = go.AddComponent<Image>();
            img.color = color;

            var btn = go.AddComponent<Button>();
            var cb = btn.colors;
            cb.normalColor = color;
            cb.highlightedColor = Color.Lerp(color, Color.white, 0.15f);
            cb.pressedColor = Color.Lerp(color, Color.black, 0.2f);
            cb.disabledColor = new Color(0.35f, 0.35f, 0.4f, 0.5f);
            cb.fadeDuration = 0.08f;
            btn.colors = cb;
            if (onClick != null) btn.onClick.AddListener(() => onClick());

            // Scale animator
            var sa = go.AddComponent<ButtonScaleAnimator>();
            sa.pressedScale = 0.94f;
            sa.animationSpeed = 24f;
            sa.useDefaultClickSound = true;

            // Outline glow
            var ol = go.AddComponent<Outline>();
            ol.effectColor = new Color(color.r, color.g, color.b, 0.3f);
            ol.effectDistance = new Vector2(2, -2);

            // Label
            var txt = AddTMP(go.transform, "Lbl", label, fontSize,
                FontStyles.Bold, Color.white, Vector2.zero, new Vector2(400, 50));
            txt.characterSpacing = isPrimary ? 5 : 2;
        }

        static void BuildSecondaryButton(Transform parent, string label, Color color, System.Action onClick)
        {
            var go = new GameObject(label + "Btn", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            BuildPremiumButton(go, label, new Color(color.r, color.g, color.b, 0.7f), onClick, 16);
        }

        // ─── ACCENT LINE ────────────────────────────────────────────────
        static GameObject CreateAccentLine(Transform parent, Vector2 anchorMin, Vector2 anchorMax, float height, Color color)
        {
            var go = new GameObject("AccentLine", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.pivot = new Vector2(0.5f, anchorMin.y > 0.5f ? 1f : 0f);
            rt.sizeDelta = new Vector2(0, height);
            rt.anchoredPosition = Vector2.zero;
            go.GetComponent<Image>().color = color;

            // Glow behind it
            var glow = new GameObject("Glow", typeof(RectTransform), typeof(Image));
            glow.transform.SetParent(go.transform, false);
            var glowRt = glow.GetComponent<RectTransform>();
            Stretch(glowRt);
            glowRt.offsetMin = new Vector2(-4, -4);
            glowRt.offsetMax = new Vector2(4, 4);
            glow.GetComponent<Image>().color = new Color(color.r, color.g, color.b, 0.15f);

            return go;
        }

        // ─── PARTICLES ──────────────────────────────────────────────────
        static void CreateParticles(Transform parent)
        {
            GameObject psGO = new GameObject("Particles", typeof(RectTransform), typeof(ParticleSystem));
            psGO.transform.SetParent(parent, false);
            RectTransform rt = psGO.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = new Vector2(0, -200);

            ParticleSystem ps = psGO.GetComponent<ParticleSystem>();
            var main = ps.main;
            main.startColor = new ParticleSystem.MinMaxGradient(
                new Color(AccentCyan.r, AccentCyan.g, AccentCyan.b, 0.5f),
                new Color(AccentGold.r, AccentGold.g, AccentGold.b, 0.4f));
            main.startSize = new ParticleSystem.MinMaxCurve(0.02f, 0.06f);
            main.startLifetime = 3f;
            main.maxParticles = 200;
            main.startSpeed = new ParticleSystem.MinMaxCurve(0.3f, 0.8f);
            main.simulationSpace = ParticleSystemSimulationSpace.World;

            var emission = ps.emission;
            emission.rateOverTime = 25f;

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = 35f;
            shape.radius = 2f;

            var colorOverLifetime = ps.colorOverLifetime;
            colorOverLifetime.enabled = true;
            Gradient grad = new Gradient();
            grad.SetKeys(
                new GradientColorKey[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                new GradientAlphaKey[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(0.6f, 0.3f), new GradientAlphaKey(0f, 1f) }
            );
            colorOverLifetime.color = grad;
        }

        // ─── HELPERS ────────────────────────────────────────────────────

        static void EnsureScreenshotShareManager(GameObject parent)
        {
            if (ScreenshotShareManager.Instance == null)
            {
                var go = new GameObject("ScreenshotShareManager", typeof(ScreenshotShareManager));
                go.transform.SetParent(parent.transform, false);
            }
        }

        static GameObject MakeStaggerChild(Transform parent, string name, Vector2 anchoredPos, Vector2 size)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasGroup));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = size;
            rt.anchoredPosition = anchoredPos;

            var origin = go.AddComponent<StaggerOrigin>();
            origin.originY = anchoredPos.y;

            go.GetComponent<CanvasGroup>().alpha = 0f;
            return go;
        }

        static TextMeshProUGUI AddTMP(Transform parent, string name, string text,
            int fontSize, FontStyles style, Color color, Vector2 pos, Vector2 size)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = size;
            rt.anchoredPosition = pos;

            var tmp = go.GetComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.fontStyle = style;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = color;
            tmp.raycastTarget = false;
            return tmp;
        }

        static void Stretch(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        static string Loc(string key, string fallback)
        {
            return LocalizationManager.Instance != null
                ? LocalizationManager.Instance.GetTranslation(key)
                : fallback;
        }

        static string FormatTime(float seconds)
        {
            int s = Mathf.Max(0, Mathf.FloorToInt(seconds));
            int h = s / 3600;
            int m = (s % 3600) / 60;
            int sec = s % 60;
            if (h > 0) return h.ToString("00") + ":" + m.ToString("00") + ":" + sec.ToString("00");
            return m.ToString("00") + ":" + sec.ToString("00");
        }

        static void EnsureEventSystem()
        {
            EventSystem existing = Object.FindFirstObjectByType<EventSystem>(FindObjectsInactive.Include);
            if (existing != null)
            {
                if (!existing.gameObject.activeSelf) existing.gameObject.SetActive(true);
                return;
            }

            GameObject es = new GameObject("EventSystem", typeof(EventSystem));
            #if PACKAGE_INPUT_SYSTEM_EXISTS || UNITY_INPUT_SYSTEM
            es.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
            #else
            es.AddComponent<StandaloneInputModule>();
            #endif
            Object.DontDestroyOnLoad(es);
        }
    }
}
