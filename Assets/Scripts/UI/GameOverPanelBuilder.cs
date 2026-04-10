using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

namespace Gazze.UI
{
    /// <summary>
    /// "Burnished Amber & Obsidian" temalı oyun sonu paneli.
    /// Bulanık arka plan + cam kart + sinematik animasyonlar.
    /// PauseMenuBuilder ile görsel uyum sağlar.
    /// </summary>
    public class GameOverPanelBuilder
    {
        static TMP_FontAsset fallbackFont;
        // ═══════════════════════════════════════════════════════════════
        // DESIGN TOKENS — Luxury Amber & Obsidian (PauseMenu ile uyumlu)
        // ═══════════════════════════════════════════════════════════════
        static readonly Color BgTint          = new Color(0.012f, 0.008f, 0.004f, 0.92f);
        static readonly Color CardGlass       = new Color(0.12f, 0.08f, 0.05f, 0.80f);
        static readonly Color CardBorderA     = new Color32(255, 215, 110, 140);
        static readonly Color CardBorderB     = new Color32(220, 130, 60, 120);

        static readonly Color Accent1         = new Color32(255, 225, 140, 255);  // Luxury Gold
        static readonly Color Accent2         = new Color32(255, 160, 80, 255);   // Rich Amber
        static readonly Color Accent3         = new Color32(255, 250, 230, 255);  // Ivory Gold
        static readonly Color AccentDanger    = new Color32(255, 100, 60, 255);   // Deep Crimson
        static readonly Color AccentGold      = new Color32(255, 215, 120, 255);  // Warm Gold (Cyan replacement)

        static readonly Color TextWhite       = new Color32(255, 253, 245, 255);
        static readonly Color TextFaded       = new Color32(220, 200, 170, 230);
        static readonly Color TextDim         = new Color32(200, 180, 150, 220);
        static readonly Color SepColor        = new Color32(255, 210, 130, 55);

        // Stat card palette
        static readonly Color StatBg          = new Color(0.08f, 0.06f, 0.04f, 0.85f);
        static readonly Color StatGold        = new Color32(255, 215, 100, 255);
        static readonly Color StatAmber       = new Color32(255, 165, 75, 255);
        static readonly Color StatIvory       = new Color32(255, 245, 210, 255);
        static readonly Color StatAccent      = new Color32(255, 210, 130, 255);

        // Button palette (consistent amber theme)
        static readonly Color BtnRestart      = new Color32(255, 225, 140, 255);  // Gold Primary
        static readonly Color BtnMenu         = new Color32(140, 120, 100, 255);  // Muted Stone
        static readonly Color BtnShare        = new Color32(255, 160, 80, 255);   // Amber
        static readonly Color BtnSettings     = new Color32(180, 160, 140, 255);  // Warm Gray

        // Achievement badge colors
        static readonly Color AchBadgeBg      = new Color(0.15f, 0.11f, 0.07f, 0.90f);
        static readonly Color AchBorder       = new Color32(255, 200, 100, 80);
        static readonly Color AchText         = new Color32(255, 230, 180, 255);
        static readonly Color AchIcon         = new Color32(255, 200, 100, 255);

        // ═══════════════════════════════════════════════════════════════
        // MAIN PANEL ANIMATOR — Sinematik açılış
        // ═══════════════════════════════════════════════════════════════
        class GameOverPanelAnimator : MonoBehaviour
        {
            public CanvasGroup cg;
            public List<RectTransform> staggerTargets = new List<RectTransform>();
            public TextMeshProUGUI scoreCountTMP;
            public int targetScore;
            public TextMeshProUGUI titleTMP;
            public RectTransform cardRT;
            public RawImage blurImg;
            public Image scoreGlow;

            public float fadeDuration = 0.5f;
            public float staggerDelay = 0.06f;

            float t;
            bool done;
            bool countDone;
            float countTimer;
            int displayedScore;

            void OnEnable()
            {
                t = 0f; done = false; countDone = false;
                countTimer = 0f; displayedScore = 0;
                if (cg != null) cg.alpha = 0f;

                foreach (var rt in staggerTargets)
                {
                    if (rt == null) continue;
                    var cg2 = rt.GetComponent<CanvasGroup>();
                    if (cg2 != null) cg2.alpha = 0f;
                    rt.localScale = new Vector3(0.88f, 0.88f, 1f);
                }
                if (cardRT != null) cardRT.localScale = new Vector3(0.92f, 0.92f, 1f);
            }

            void Update()
            {
                t += Time.unscaledDeltaTime;

                // ── Phase 1: BG fade + blur reveal ──
                float oT = Mathf.Clamp01(t / 0.35f);
                float oE = 1f - Mathf.Pow(1f - oT, 2.5f);
                if (cg) cg.alpha = oE;

                if (blurImg)
                    blurImg.color = new Color(1, 1, 1, Mathf.Clamp01(t / 0.3f));

                // ── Phase 2: Card scale with overshoot ──
                if (cardRT)
                {
                    float cT = Mathf.Clamp01((t - 0.05f) / 0.45f);
                    // OutBack-style overshoot
                    float cE = 1f - Mathf.Pow(1f - cT, 3f) * (1f - cT * 1.15f);
                    cardRT.localScale = Vector3.LerpUnclamped(new Vector3(0.92f, 0.92f, 1f), Vector3.one, cE);
                }

                // ── Phase 3: Stagger children (diagonal entry) ──
                for (int i = 0; i < staggerTargets.Count; i++)
                {
                    if (staggerTargets[i] == null) continue;
                    float childStart = 0.2f + i * staggerDelay;
                    float childT = Mathf.Clamp01((t - childStart) / 0.4f);
                    // OutBack overshoot
                    float e = 1f - Mathf.Pow(1f - childT, 3f) * (1f - childT * 1.15f);

                    var cg2 = staggerTargets[i].GetComponent<CanvasGroup>();
                    if (cg2 != null) cg2.alpha = Mathf.Clamp01(childT * 2.5f);

                    var origin = staggerTargets[i].GetComponent<StaggerOrigin>();
                    if (origin != null)
                    {
                        Vector2 start = origin.pos + new Vector2(origin.side * 120f, -30f);
                        staggerTargets[i].anchoredPosition = Vector2.LerpUnclamped(start, origin.pos, e);
                    }

                    staggerTargets[i].localScale = Vector3.LerpUnclamped(
                        new Vector3(0.85f, 0.85f, 1f), Vector3.one, e);
                }

                // ── Phase 4: Score count-up with glow pulse ──
                if (scoreCountTMP != null && !countDone)
                {
                    float countStart = 0.55f;
                    if (t > countStart)
                    {
                        countTimer += Time.unscaledDeltaTime;
                        float countDuration = Mathf.Clamp(targetScore * 0.002f, 0.5f, 2.0f);
                        float countProgress = Mathf.Clamp01(countTimer / countDuration);
                        float easeCount = 1f - Mathf.Pow(1f - countProgress, 3f);
                        displayedScore = Mathf.RoundToInt(Mathf.Lerp(0f, targetScore, easeCount));
                        scoreCountTMP.text = displayedScore.ToString("N0");

                        // Score glow intensifies as count progresses
                        if (scoreGlow != null)
                        {
                            float glowA = Mathf.Lerp(0f, 0.35f, easeCount);
                            scoreGlow.color = new Color(Accent1.r, Accent1.g, Accent1.b, glowA);
                        }

                        if (countProgress >= 1f)
                        {
                            scoreCountTMP.text = targetScore.ToString("N0");
                            countDone = true;
                        }
                    }
                }

                // ── Phase 5: Title gradient pulse (continuous) ──
                if (titleTMP != null && t > 0.3f)
                {
                    float pulse = Mathf.Sin(t * 2.5f) * 0.5f + 0.5f;
                    Color c1 = Color.Lerp(Accent1, Accent2, pulse);
                    Color c2 = Color.Lerp(Accent2, Accent1, pulse);
                    titleTMP.colorGradient = new VertexGradient(c1, c2, c1, c2);
                }

                if (!done && t > fadeDuration + staggerTargets.Count * staggerDelay + 2f)
                    done = true;
            }
        }

        // ═══════════════════════════════════════════════════════════════
        // FLUID BUTTON ANIMATOR — Premium hover/press etkileşimi
        // ═══════════════════════════════════════════════════════════════
        class FluidButtonAnimator : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler,
            IPointerDownHandler, IPointerUpHandler
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
                transform.localScale = Vector3.Lerp(transform.localScale, _targetScale, Time.unscaledDeltaTime * 14f);
                transform.localRotation = Quaternion.Slerp(transform.localRotation, _targetRot, Time.unscaledDeltaTime * 12f);
                if (targetGraphic)
                    targetGraphic.color = Color.Lerp(targetGraphic.color, _targetColor, Time.unscaledDeltaTime * 16f);
            }

            public void OnPointerEnter(PointerEventData e)
            {
                _targetScale = Vector3.one * 1.04f;
                _targetRot = Quaternion.Euler(0, 0, 0.8f);
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
                _targetScale = Vector3.one * 0.96f;
                _targetRot = Quaternion.Euler(0, 0, -0.5f);
                _targetColor = pressColor;
                Settings.HapticManager.Light();
            }
            public void OnPointerUp(PointerEventData e)
            {
                _targetScale = Vector3.one * 1.04f;
                _targetColor = hoverColor;
            }
        }

        // ═══════════════════════════════════════════════════════════════
        // PRIMARY BUTTON GLOW PULSE
        // ═══════════════════════════════════════════════════════════════
        class PrimaryGlowPulse : MonoBehaviour
        {
            public Outline glowOutline;
            public float speed = 2.5f;
            public float minAlpha = 0.15f;
            public float maxAlpha = 0.5f;

            void Update()
            {
                if (!glowOutline) return;
                float alpha = Mathf.Lerp(minAlpha, maxAlpha, (Mathf.Sin(Time.unscaledTime * speed) + 1f) / 2f);
                var c = glowOutline.effectColor;
                glowOutline.effectColor = new Color(c.r, c.g, c.b, alpha);
            }
        }

        // ═══════════════════════════════════════════════════════════════
        // GRADIENT BORDER PULSE
        // ═══════════════════════════════════════════════════════════════
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

        // ═══════════════════════════════════════════════════════════════
        // FLOATING PARTICLE MOTES
        // ═══════════════════════════════════════════════════════════════
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
                    float o = i * 2.3f;
                    dots[i].anchoredPosition += new Vector2(
                        Mathf.Cos(ut * 0.4f + o) * 5f * Time.unscaledDeltaTime,
                        Mathf.Sin(ut * 0.55f + o) * 7f * Time.unscaledDeltaTime);
                    if (imgs[i])
                    {
                        float a = 0.05f + 0.07f * Mathf.Sin(ut * 0.9f + o);
                        var c = imgs[i].color;
                        imgs[i].color = new Color(c.r, c.g, c.b, a);
                    }
                }
            }
        }

        // ═══════════════════════════════════════════════════════════════
        // STAGGER ORIGIN
        // ═══════════════════════════════════════════════════════════════
        class StaggerOrigin : MonoBehaviour
        {
            public Vector2 pos;
            public float side;
        }

        // ═══════════════════════════════════════════════════════════════
        // DATA
        // ═══════════════════════════════════════════════════════════════
        public class Data
        {
            public int score;
            public int highScore;
            public int level;
            public float playTimeSeconds;
            public int nearMisses;
            public List<string> achievements;
            public System.Action onRestart;
            public System.Action onMainMenu;
            public System.Action onShare;
            public System.Action onSettings;
        }

        // ═══════════════════════════════════════════════════════════════
        // BUILD — Ana yapım metodu
        // ═══════════════════════════════════════════════════════════════
        public static GameObject Build(Data data)
        {
            bool isLandscape = Screen.width > Screen.height;

            // ── Canvas Setup ──
            GameObject canvasGO = new GameObject("GameOverCanvas",
                typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            Canvas canvas = canvasGO.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;
            CanvasScaler scaler = canvasGO.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;

            // Portrait: reference 1080x1920, scale by width (narrowest axis dominates).
            // Landscape: reference 1920x1080, scale by height (shortest axis dominates).
            // This ensures the card never overflows the screen in either orientation.
            if (isLandscape)
            {
                scaler.referenceResolution = new Vector2(1920, 1080);
                scaler.matchWidthOrHeight = 1f; // height-dominant in landscape
            }
            else
            {
                scaler.referenceResolution = new Vector2(1080, 1920);
                scaler.matchWidthOrHeight = 0f; // width-dominant in portrait
            }

            EnsureEventSystem();
            EnsureScreenshotShareManager(canvasGO);

            // ── Responsive layout parameters ──
            // uiScale: how much to shrink content relative to the reference design.
            // Portrait reference is 1080x1920. Landscape reference is 1920x1080.
            // We scale by the SHORT side so nothing overflows.
            float shortSideRef    = isLandscape ? 1080f : 1920f;
            float shortSideScreen = isLandscape ? Screen.height : Screen.height;
            float uiScale = Mathf.Clamp(shortSideScreen / shortSideRef, 0.45f, 1.0f);

            // Card dimensions — landscape gets wide+short, portrait gets narrow+tall
            float cardW, cardH;
            if (isLandscape)
            {
                // In landscape canvas space (1920x1080 ref), card fills most of the height
                // and about half the width so it looks like a centred popup.
                cardH = Mathf.Clamp(1080f * 0.92f * uiScale, 500f, 980f);
                cardW = Mathf.Clamp(cardH * 0.76f, 420f, 760f); // ~4:3 ratio
            }
            else
            {
                cardW = Mathf.Clamp(1080f * 0.88f * uiScale, 480f, 720f);
                cardH = Mathf.Clamp(cardW * 1.36f, 680f, 980f);
            }

            // Font scale: keep text readable but don't exceed design maximums
            float fs = uiScale; // shorthand

            // Vertical position multiplier (layout positions were designed for portrait)
            // In landscape, compress Y positions to fit the shorter card height
            float yMult = isLandscape ? (cardH / 980f) : uiScale;

            // ── Root (AnimateTarget) ──
            var root = MakeGO("Root", canvasGO.transform, typeof(CanvasGroup));
            Stretch(root.GetComponent<RectTransform>());
            var animator = root.AddComponent<GameOverPanelAnimator>();
            animator.cg = root.GetComponent<CanvasGroup>();

            // ── Blur BG ──
            Texture2D blurTex = CaptureBlur();
            if (blurTex != null)
            {
                var blurGO = new GameObject("Blur", typeof(RectTransform), typeof(RawImage));
                blurGO.transform.SetParent(root.transform, false);
                Stretch(blurGO.GetComponent<RectTransform>());
                var ri = blurGO.GetComponent<RawImage>();
                ri.texture = blurTex;
                ri.color = new Color(1, 1, 1, 0);
                ri.raycastTarget = false;
                animator.blurImg = ri;
            }

            // ── Tint overlay ──
            var tintGO = MakeGO("Tint", root.transform, typeof(Image));
            Stretch(tintGO.GetComponent<RectTransform>());
            tintGO.GetComponent<Image>().color = BgTint;
            tintGO.GetComponent<Image>().raycastTarget = false;

            // ── Ambient motes ──
            SpawnMotes(root.transform);

            // ═══════════════════════════════════════════════════════════
            // MAIN CARD
            // ═══════════════════════════════════════════════════════════
            var card = new GameObject("Card", typeof(RectTransform), typeof(Image), typeof(Outline), typeof(Shadow));
            card.transform.SetParent(root.transform, false);
            var crt = card.GetComponent<RectTransform>();
            crt.anchorMin = crt.anchorMax = new Vector2(0.5f, 0.5f);
            crt.sizeDelta = new Vector2(cardW, cardH);
            crt.anchoredPosition = Vector2.zero;
            card.GetComponent<Image>().color = CardGlass;
            var ol = card.GetComponent<Outline>();
            ol.effectColor = CardBorderA;
            ol.effectDistance = new Vector2(1.5f, -1.5f);
            card.AddComponent<GradientBorderPulse>().outline = ol;
            var sh = card.GetComponent<Shadow>();
            sh.effectColor = new Color(0, 0, 0, 0.5f);
            sh.effectDistance = new Vector2(6, -8);

            animator.cardRT = crt;

            // Glass inner layer
            var inner = MakeGO("InnerGlass", card.transform, typeof(Image));
            var irt = inner.GetComponent<RectTransform>();
            Stretch(irt);
            irt.offsetMin = new Vector2(3, 3); irt.offsetMax = new Vector2(-3, -3);
            inner.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.025f);
            inner.GetComponent<Image>().raycastTarget = false;

            MakeAccentBar(card.transform, true);
            MakeAccentBar(card.transform, false);

            // ═══════════════════════════════════════════════════════════
            // CONTENT — Staggered elements
            // ═══════════════════════════════════════════════════════════
            var staggerList = new List<RectTransform>();
            float innerW = cardW - 60f;

            // ── (1) TITLE ──
            string titleStr = Loc("Game_GameOver", "OYUN BİTTİ");
            float titleY = cardH * 0.43f;
            float titleH  = Mathf.Max(50f * fs, 42f);
            var titleGO = Stagger(card.transform, "Title", new Vector2(0, titleY), new Vector2(innerW, titleH), -1f);
            staggerList.Add(titleGO.GetComponent<RectTransform>());

            var titleTMP = AddTMP(titleGO.transform, "T", titleStr, Mathf.RoundToInt(Mathf.Max(48 * fs, 28)),
                FontStyles.Bold, Accent1, Vector2.zero, new Vector2(innerW, titleH));
            titleTMP.enableAutoSizing = true;
            titleTMP.fontSizeMin = 20; titleTMP.fontSizeMax = Mathf.RoundToInt(52 * fs);
            titleTMP.characterSpacing = 10;
            titleTMP.enableVertexGradient = true;
            titleTMP.colorGradient = new VertexGradient(Accent1, Accent2, Accent1, Accent2);
            animator.titleTMP = titleTMP;

            // ── (2) Separator ──
            float sep1Y = cardH * 0.385f;
            var sep1 = Stagger(card.transform, "Sep1", new Vector2(0, sep1Y), new Vector2(innerW * 0.85f, 2), 1f);
            staggerList.Add(sep1.GetComponent<RectTransform>());
            sep1.AddComponent<Image>().color = SepColor;

            // ── (3) Score section ──
            float scoreY = cardH * 0.28f;
            float scoreH = Mathf.Max(cardH * 0.17f, 100f);
            var scoreSection = Stagger(card.transform, "ScoreSection", new Vector2(0, scoreY), new Vector2(innerW, scoreH), -1f);
            staggerList.Add(scoreSection.GetComponent<RectTransform>());

            bool isNewHigh = data.score >= data.highScore && data.score > 0;
            if (isNewHigh)
            {
                string newHS = Loc("Game_NewHighScore", "YENi REKOR!");
                var badgeGO = new GameObject("Badge", typeof(RectTransform), typeof(Image), typeof(Outline));
                badgeGO.transform.SetParent(scoreSection.transform, false);
                var badgeRt = badgeGO.GetComponent<RectTransform>();
                badgeRt.anchorMin = badgeRt.anchorMax = new Vector2(0.5f, 1f);
                badgeRt.pivot = new Vector2(0.5f, 1f);
                badgeRt.sizeDelta = new Vector2(Mathf.Min(260f * fs, innerW * 0.6f), Mathf.Max(32f * fs, 28f));
                badgeRt.anchoredPosition = new Vector2(0, 0);
                badgeGO.GetComponent<Image>().color = new Color(Accent1.r, Accent1.g, Accent1.b, 0.18f);
                var nrOl = badgeGO.GetComponent<Outline>();
                nrOl.effectColor = new Color(Accent1.r, Accent1.g, Accent1.b, 0.3f);
                nrOl.effectDistance = new Vector2(1, -1);
                var nrTmp = AddTMP(badgeGO.transform, "BadgeTxt", newHS, Mathf.RoundToInt(Mathf.Max(15 * fs, 11)),
                    FontStyles.Bold, Accent1, Vector2.zero, badgeRt.sizeDelta);
                nrTmp.characterSpacing = 5;
            }

            // Score label
            string scoreLbl = Loc("Game_Score", "SKOR");
            var scoreLabelTMP = AddTMP(scoreSection.transform, "ScoreLbl", scoreLbl,
                Mathf.RoundToInt(Mathf.Max(14 * fs, 10)),
                FontStyles.Bold, TextFaded,
                new Vector2(0, isNewHigh ? scoreH * 0.1f : scoreH * 0.25f),
                new Vector2(innerW, Mathf.Max(22f * fs, 18f)));
            scoreLabelTMP.characterSpacing = 6;

            // Score number
            int scoreFontMax = Mathf.RoundToInt(Mathf.Max(isLandscape ? 54 * fs : 72 * fs, 28));
            var scoreNumTMP = AddTMP(scoreSection.transform, "ScoreNum", "0", scoreFontMax,
                FontStyles.Bold, TextWhite,
                new Vector2(0, isNewHigh ? -scoreH * 0.18f : -scoreH * 0.05f),
                new Vector2(innerW, scoreH * 0.6f));
            scoreNumTMP.enableAutoSizing = true;
            scoreNumTMP.fontSizeMin = 20; scoreNumTMP.fontSizeMax = scoreFontMax;
            scoreNumTMP.enableVertexGradient = true;
            scoreNumTMP.colorGradient = new VertexGradient(Accent3, Accent3, Accent1, Accent2);
            animator.scoreCountTMP = scoreNumTMP;
            animator.targetScore = data.score;

            // Score glow
            var scoreGlowGO = new GameObject("ScoreGlow", typeof(RectTransform), typeof(Image));
            scoreGlowGO.transform.SetParent(scoreSection.transform, false);
            scoreGlowGO.transform.SetAsFirstSibling();
            var sgRt = scoreGlowGO.GetComponent<RectTransform>();
            sgRt.anchorMin = sgRt.anchorMax = new Vector2(0.5f, 0.5f);
            sgRt.sizeDelta = new Vector2(innerW * 0.8f, scoreH * 0.65f);
            sgRt.anchoredPosition = Vector2.zero;
            var sgImg = scoreGlowGO.GetComponent<Image>();
            sgImg.color = new Color(Accent1.r, Accent1.g, Accent1.b, 0f);
            sgImg.raycastTarget = false;
            animator.scoreGlow = sgImg;

            // Score underline
            var scoreBar = new GameObject("ScoreBar", typeof(RectTransform), typeof(Image));
            scoreBar.transform.SetParent(scoreSection.transform, false);
            var sbRt = scoreBar.GetComponent<RectTransform>();
            sbRt.anchorMin = new Vector2(0.1f, 0f); sbRt.anchorMax = new Vector2(0.9f, 0f);
            sbRt.pivot = new Vector2(0.5f, 0f);
            sbRt.sizeDelta = new Vector2(0, 3);
            sbRt.anchoredPosition = Vector2.zero;
            scoreBar.GetComponent<Image>().color = new Color(Accent1.r, Accent1.g, Accent1.b, 0.5f);

            // ── (4) Stat cards row ──
            float statsY = cardH * 0.115f;
            float statsH  = Mathf.Max(cardH * 0.115f, 72f);
            var statsRow = Stagger(card.transform, "Stats", new Vector2(0, statsY), new Vector2(innerW, statsH), 1f);
            staggerList.Add(statsRow.GetComponent<RectTransform>());

            var hlg = statsRow.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = Mathf.Max(8f * fs, 5f);
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childControlWidth = true; hlg.childControlHeight = true;
            hlg.childForceExpandWidth = true; hlg.childForceExpandHeight = true;
            hlg.padding = new RectOffset(4, 4, 3, 3);

            string hsLabel  = Loc("Game_HighScore", "REKOR");
            string nmLabel  = Loc("Game_NearMisses", "MAKAS");
            string lvlLabel = Loc("Game_Level", "SEVİYE");
            string timeLabel = Loc("Game_PlayTime", "SÜRE");

            float statFontScale = fs;
            BuildStatCard(hlg.transform, hsLabel,  data.highScore.ToString(),             StatGold,  statFontScale);
            BuildStatCard(hlg.transform, nmLabel,  data.nearMisses.ToString(),            StatAmber, statFontScale);
            BuildStatCard(hlg.transform, lvlLabel, data.level.ToString(),                 StatAccent, statFontScale);
            BuildStatCard(hlg.transform, timeLabel, FormatTime(data.playTimeSeconds),     StatIvory, statFontScale);

            // ── (5) Separator ──
            float sep2Y = cardH * 0.045f;
            var sep2 = Stagger(card.transform, "Sep2", new Vector2(0, sep2Y), new Vector2(innerW * 0.85f, 2), -1f);
            staggerList.Add(sep2.GetComponent<RectTransform>());
            sep2.AddComponent<Image>().color = SepColor;

            // ── (6) Achievements section ──
            string achLabel = Loc("Game_Achievements", "BASARIMLAR");
            float achY = -cardH * 0.055f;
            float achH = Mathf.Max(cardH * 0.185f, 110f);
            var achSection = Stagger(card.transform, "AchSection", new Vector2(0, achY), new Vector2(innerW, achH), 1f);
            staggerList.Add(achSection.GetComponent<RectTransform>());

            var achLabelTMP = AddTMP(achSection.transform, "AchLbl", achLabel,
                Mathf.RoundToInt(Mathf.Max(14 * fs, 10)),
                FontStyles.Bold, TextFaded,
                new Vector2(0, achH * 0.42f),
                new Vector2(innerW, Mathf.Max(22f * fs, 16f)));
            achLabelTMP.characterSpacing = 5;

            if (data.achievements != null && data.achievements.Count > 0)
            {
                var badgeContainer = new GameObject("BadgeContainer", typeof(RectTransform));
                badgeContainer.transform.SetParent(achSection.transform, false);
                var bcRt = badgeContainer.GetComponent<RectTransform>();
                bcRt.anchorMin = new Vector2(0f, 0f); bcRt.anchorMax = new Vector2(1f, 0.78f);
                bcRt.offsetMin = new Vector2(4, 2); bcRt.offsetMax = new Vector2(-4, 0);

                var vlg = badgeContainer.AddComponent<VerticalLayoutGroup>();
                vlg.spacing = Mathf.Max(5f * fs, 4f);
                vlg.childAlignment = TextAnchor.UpperCenter;
                vlg.childControlWidth = true; vlg.childControlHeight = false;
                vlg.childForceExpandWidth = true; vlg.childForceExpandHeight = false;
                vlg.padding = new RectOffset(2, 2, 2, 2);

                // Landscape: 3 per row to save vertical space; portrait: 2 per row
                int achPerRow = isLandscape ? 3 : 2;
                float rowH = Mathf.Max(38f * fs, 34f);
                for (int rowStart = 0; rowStart < data.achievements.Count; rowStart += achPerRow)
                {
                    var row = new GameObject("AchRow", typeof(RectTransform), typeof(HorizontalLayoutGroup));
                    row.transform.SetParent(badgeContainer.transform, false);
                    row.GetComponent<RectTransform>().sizeDelta = new Vector2(0, rowH);

                    var rowHlg = row.GetComponent<HorizontalLayoutGroup>();
                    rowHlg.spacing = Mathf.Max(8f * fs, 5f);
                    rowHlg.childAlignment = TextAnchor.MiddleCenter;
                    rowHlg.childControlWidth = true; rowHlg.childControlHeight = true;
                    rowHlg.childForceExpandWidth = true; rowHlg.childForceExpandHeight = true;
                    rowHlg.padding = new RectOffset(2, 2, 0, 0);

                    int rowEnd = Mathf.Min(rowStart + achPerRow, data.achievements.Count);
                    for (int i = rowStart; i < rowEnd; i++)
                        BuildAchievementBadge(row.transform, data.achievements[i], fs);
                }
            }
            else
            {
                string achNone = Loc("Game_None", "—");
                AddTMP(achSection.transform, "AchNone", achNone, Mathf.RoundToInt(Mathf.Max(16 * fs, 12)),
                    FontStyles.Italic, TextDim, new Vector2(0, -achH * 0.1f), new Vector2(innerW, 40));
            }

            // ── (7) Separator ──
            float sep3Y = -cardH * 0.155f;
            var sep3 = Stagger(card.transform, "Sep3", new Vector2(0, sep3Y), new Vector2(innerW * 0.85f, 2), -1f);
            staggerList.Add(sep3.GetComponent<RectTransform>());
            sep3.AddComponent<Image>().color = SepColor;

            // ═══════════════════════════════════════════════════════════
            // BUTTONS
            // ═══════════════════════════════════════════════════════════
            string restartStr  = Loc("Game_Restart", "TEKRAR DENE");
            string menuStr     = Loc("Game_MainMenu", "ANA MENÜ");
            string shareStr    = Loc("Game_Share", "PAYLAŞ");
            string settingsStr = Loc("Menu_Settings", "AYARLAR");

            // Minimum touch targets: 48px height regardless of scale
            float primaryH   = Mathf.Max(cardH * 0.073f, 52f);
            float secondaryH = Mathf.Max(cardH * 0.056f, 48f);
            float btnW       = innerW;

            float primaryY   = -cardH * 0.22f;
            float secondaryY = -cardH * 0.31f;
            float hintY      = -cardH * 0.39f;

            // ── Primary button ──
            var primaryBtn = Stagger(card.transform, "PrimaryBtn", new Vector2(0, primaryY), new Vector2(btnW, primaryH), 1f);
            staggerList.Add(primaryBtn.GetComponent<RectTransform>());
            BuildPrimaryButton(primaryBtn, restartStr, BtnRestart, data.onRestart, fs);

            // ── Secondary buttons row ──
            var secRow = Stagger(card.transform, "SecBtns", new Vector2(0, secondaryY), new Vector2(btnW, secondaryH), -1f);
            staggerList.Add(secRow.GetComponent<RectTransform>());

            var secHlg = secRow.AddComponent<HorizontalLayoutGroup>();
            secHlg.spacing = Mathf.Max(8f * fs, 5f);
            secHlg.childAlignment = TextAnchor.MiddleCenter;
            secHlg.childControlWidth = true; secHlg.childControlHeight = true;
            secHlg.childForceExpandWidth = true; secHlg.childForceExpandHeight = true;

            System.Action shareAction = () =>
            {
                if (ScreenshotShareManager.Instance != null)
                    ScreenshotShareManager.Instance.CaptureAndShare(canvasGO);
                else data.onShare?.Invoke();
            };

            BuildSecondaryButton(secHlg.transform, menuStr,     BtnMenu,     data.onMainMenu, fs);
            BuildSecondaryButton(secHlg.transform, shareStr,    BtnShare,    shareAction,     fs);
            BuildSecondaryButton(secHlg.transform, settingsStr, BtnSettings, data.onSettings, fs);

            // ── Hint text ──
            var hint = Stagger(card.transform, "Hint", new Vector2(0, hintY), new Vector2(btnW, 18), 1f);
            staggerList.Add(hint.GetComponent<RectTransform>());
            AddTMP(hint.transform, "H", "GOREV: GAZZE", Mathf.RoundToInt(Mathf.Max(9 * fs, 8)),
                FontStyles.Italic, new Color(1, 1, 1, 0.22f), Vector2.zero, new Vector2(btnW, 18));

            // ── Wire stagger list ──
            animator.staggerTargets = staggerList;

            return canvasGO;
        }

        // ═══════════════════════════════════════════════════════════════
        // STAT CARD — Glassmorphism stat tiles (no Unicode icons)
        // ═══════════════════════════════════════════════════════════════
        static void BuildStatCard(Transform parent, string label, string value, Color accent, float uiScale)
        {
            var go = new GameObject("Stat_" + label, typeof(RectTransform), typeof(Image), typeof(Outline));
            go.transform.SetParent(parent, false);
            go.GetComponent<Image>().color = StatBg;
            var statOl = go.GetComponent<Outline>();
            statOl.effectColor = new Color(accent.r, accent.g, accent.b, 0.25f);
            statOl.effectDistance = new Vector2(1, -1);

            // Top accent full-width bar
            var bar = new GameObject("AccBar", typeof(RectTransform), typeof(Image));
            bar.transform.SetParent(go.transform, false);
            var barRt = bar.GetComponent<RectTransform>();
            barRt.anchorMin = new Vector2(0.08f, 1f);
            barRt.anchorMax = new Vector2(0.92f, 1f);
            barRt.pivot = new Vector2(0.5f, 1f);
            barRt.sizeDelta = new Vector2(0, 3f);
            bar.GetComponent<Image>().color = accent;

            int lblSize = Mathf.RoundToInt(Mathf.Max(12 * uiScale, 9));
            int valSize = Mathf.RoundToInt(Mathf.Max(26 * uiScale, 14));

            // Label
            var lbl = AddTMP(go.transform, "Lbl", label, lblSize, FontStyles.Bold,
                new Color(accent.r, accent.g, accent.b, 0.85f),
                new Vector2(0, 0), new Vector2(0, 0));
            // Stretch label to top half of card
            var lblRt = lbl.GetComponent<RectTransform>();
            lblRt.anchorMin = new Vector2(0.05f, 0.55f); lblRt.anchorMax = new Vector2(0.95f, 1f);
            lblRt.offsetMin = new Vector2(2, 2); lblRt.offsetMax = new Vector2(-2, -3);
            lbl.characterSpacing = 2;
            lbl.enableAutoSizing = true;
            lbl.fontSizeMin = 8; lbl.fontSizeMax = lblSize;

            // Value — stretch to bottom half
            var valTMP = AddTMP(go.transform, "Val", value, valSize, FontStyles.Bold, TextWhite,
                new Vector2(0, 0), new Vector2(0, 0));
            var valRt = valTMP.GetComponent<RectTransform>();
            valRt.anchorMin = new Vector2(0.05f, 0.05f); valRt.anchorMax = new Vector2(0.95f, 0.58f);
            valRt.offsetMin = new Vector2(2, 2); valRt.offsetMax = new Vector2(-2, -2);
            valTMP.enableVertexGradient = true;
            valTMP.colorGradient = new VertexGradient(TextWhite, TextWhite, accent, accent);
            valTMP.enableAutoSizing = true;
            valTMP.fontSizeMin = 10; valTMP.fontSizeMax = valSize;
        }

        // ═══════════════════════════════════════════════════════════════
        // ACHIEVEMENT BADGE — Individual styled pill
        // ═══════════════════════════════════════════════════════════════
        static void BuildAchievementBadge(Transform parent, string text, float uiScale = 1f)
        {
            var badge = new GameObject("AchBadge", typeof(RectTransform), typeof(Image), typeof(Outline));
            badge.transform.SetParent(parent, false);
            badge.GetComponent<Image>().color = AchBadgeBg;
            var badgeOl = badge.GetComponent<Outline>();
            badgeOl.effectColor = AchBorder;
            badgeOl.effectDistance = new Vector2(1, -1);

            // Get Theme to access Icon Path
            var theme = AchievementTheme.GetTheme(text);
            Sprite iconSprite = null;
            if (!string.IsNullOrEmpty(theme.iconPath))
            {
                iconSprite = Resources.Load<Sprite>(theme.iconPath);
            }

            // Icon or Accent bar
            var iconGO = new GameObject("Icon", typeof(RectTransform), typeof(Image));
            iconGO.transform.SetParent(badge.transform, false);
            var iRt = iconGO.GetComponent<RectTransform>();
            var img = iconGO.GetComponent<Image>();

            if (iconSprite != null)
            {
                // Real Icon
                iRt.anchorMin = new Vector2(0, 0.5f);
                iRt.anchorMax = new Vector2(0, 0.5f);
                iRt.pivot = new Vector2(0, 0.5f);
                iRt.sizeDelta = new Vector2(28 * uiScale, 28 * uiScale);
                iRt.anchoredPosition = new Vector2(8, 0);
                
                img.sprite = iconSprite;
                img.color = Color.white;
                img.preserveAspect = true;
            }
            else
            {
                // Fallback Accent bar
                iRt.anchorMin = new Vector2(0, 0.15f);
                iRt.anchorMax = new Vector2(0, 0.85f);
                iRt.pivot = new Vector2(0, 0.5f);
                iRt.sizeDelta = new Vector2(3, 0);
                iRt.anchoredPosition = new Vector2(6, 0);
                img.color = AchIcon;
            }

            // Text (uses badge width, auto-sizes to fit)
            int achFontMax = Mathf.RoundToInt(Mathf.Max(13 * uiScale, 9));
            var tmp = AddTMP(badge.transform, "Txt", text, achFontMax, FontStyles.Bold, AchText,
                Vector2.zero, Vector2.zero);
            // Stretch text to fill badge
            var txtRt = tmp.GetComponent<RectTransform>();
            txtRt.anchorMin = new Vector2(0, 0);
            txtRt.anchorMax = new Vector2(1, 1);
            
            // Offset text if icon exists
            float leftOffset = iconSprite != null ? 40f * uiScale : 14f;
            txtRt.offsetMin = new Vector2(leftOffset, 2);
            txtRt.offsetMax = new Vector2(-4, -2);
            
            tmp.alignment = TextAlignmentOptions.Left;
            tmp.enableAutoSizing = true;
            tmp.fontSizeMin = 8;
            tmp.fontSizeMax = achFontMax;
        }

        // ═══════════════════════════════════════════════════════════════
        // PRIMARY BUTTON — Full-width premium gold
        // ═══════════════════════════════════════════════════════════════
        static void BuildPrimaryButton(GameObject go, string label, Color color, System.Action onClick, float uiScale = 1f)
        {
            var img = go.AddComponent<Image>();
            img.color = new Color(color.r * 0.3f, color.g * 0.3f, color.b * 0.3f, 0.85f);

            var btn = go.AddComponent<Button>();
            var cb = btn.colors;
            cb.normalColor = Color.white;
            cb.highlightedColor = Color.white;
            cb.pressedColor = Color.white;
            cb.disabledColor = Color.white;
            cb.fadeDuration = 0f;
            btn.colors = cb;
            if (onClick != null) btn.onClick.AddListener(() => onClick());

            // Fluid animator
            var fluid = go.AddComponent<FluidButtonAnimator>();
            fluid.targetGraphic = img;
            fluid.normalColor = new Color(color.r * 0.3f, color.g * 0.3f, color.b * 0.3f, 0.85f);
            fluid.hoverColor = new Color(color.r * 0.4f, color.g * 0.4f, color.b * 0.4f, 0.92f);
            fluid.pressColor = new Color(color.r * 0.2f, color.g * 0.2f, color.b * 0.2f, 0.95f);

            // ButtonScaleAnimator for haptics
            var sa = go.AddComponent<ButtonScaleAnimator>();
            sa.pressedScale = 0.96f;
            sa.animationSpeed = 20f;
            sa.useDefaultClickSound = true;

            // Glow outline
            var glowOl = go.AddComponent<Outline>();
            glowOl.effectColor = new Color(color.r, color.g, color.b, 0.35f);
            glowOl.effectDistance = new Vector2(2, -2);

            // Glow pulse
            var gp = go.AddComponent<PrimaryGlowPulse>();
            gp.glowOutline = glowOl;
            gp.speed = 2.5f;

            // Border accent
            var borderLine = new GameObject("BorderLine", typeof(RectTransform), typeof(Image));
            borderLine.transform.SetParent(go.transform, false);
            var blRt = borderLine.GetComponent<RectTransform>();
            blRt.anchorMin = new Vector2(0, 0);
            blRt.anchorMax = new Vector2(1, 0);
            blRt.pivot = new Vector2(0.5f, 0);
            blRt.sizeDelta = new Vector2(0, 2);
            borderLine.GetComponent<Image>().color = new Color(color.r, color.g, color.b, 0.6f);

            var topLine = new GameObject("TopLine", typeof(RectTransform), typeof(Image));
            topLine.transform.SetParent(go.transform, false);
            var tlRt = topLine.GetComponent<RectTransform>();
            tlRt.anchorMin = new Vector2(0, 1);
            tlRt.anchorMax = new Vector2(1, 1);
            tlRt.pivot = new Vector2(0.5f, 1);
            tlRt.sizeDelta = new Vector2(0, 1);
            topLine.GetComponent<Image>().color = new Color(color.r, color.g, color.b, 0.3f);

            // Label — stretch to fill button
            int btnFontMax = Mathf.RoundToInt(Mathf.Max(22 * uiScale, 14));
            var txt = AddTMP(go.transform, "Lbl", label, btnFontMax, FontStyles.Bold, color,
                Vector2.zero, Vector2.zero);
            var txtRt = txt.GetComponent<RectTransform>();
            txtRt.anchorMin = Vector2.zero; txtRt.anchorMax = Vector2.one;
            txtRt.offsetMin = new Vector2(8, 4); txtRt.offsetMax = new Vector2(-8, -4);
            txt.enableAutoSizing = true;
            txt.fontSizeMin = 10; txt.fontSizeMax = btnFontMax;
            txt.characterSpacing = 6;
        }

        // ═══════════════════════════════════════════════════════════════
        // SECONDARY BUTTON — Ghost-style outline
        // ═══════════════════════════════════════════════════════════════
        static void BuildSecondaryButton(Transform parent, string label, Color color, System.Action onClick, float uiScale = 1f)
        {
            var go = new GameObject(label + "Btn", typeof(RectTransform));
            go.transform.SetParent(parent, false);

            var img = go.AddComponent<Image>();
            img.color = new Color(color.r * 0.12f, color.g * 0.12f, color.b * 0.12f, 0.6f);

            var btn = go.AddComponent<Button>();
            var cb = btn.colors;
            cb.normalColor = Color.white;
            cb.highlightedColor = Color.white;
            cb.pressedColor = Color.white;
            cb.disabledColor = Color.white;
            cb.fadeDuration = 0f;
            btn.colors = cb;
            if (onClick != null) btn.onClick.AddListener(() => onClick());

            // Fluid animator
            var fluid = go.AddComponent<FluidButtonAnimator>();
            fluid.targetGraphic = img;
            fluid.normalColor = new Color(color.r * 0.12f, color.g * 0.12f, color.b * 0.12f, 0.6f);
            fluid.hoverColor = new Color(color.r * 0.2f, color.g * 0.2f, color.b * 0.2f, 0.75f);
            fluid.pressColor = new Color(color.r * 0.08f, color.g * 0.08f, color.b * 0.08f, 0.8f);

            var sa = go.AddComponent<ButtonScaleAnimator>();
            sa.pressedScale = 0.95f;
            sa.animationSpeed = 22f;
            sa.useDefaultClickSound = true;

            // Subtle outline
            var secOl = go.AddComponent<Outline>();
            secOl.effectColor = new Color(color.r, color.g, color.b, 0.25f);
            secOl.effectDistance = new Vector2(1, -1);

            // Bottom accent micro-line
            var bottomLine = new GameObject("BottomLine", typeof(RectTransform), typeof(Image));
            bottomLine.transform.SetParent(go.transform, false);
            var btlRt = bottomLine.GetComponent<RectTransform>();
            btlRt.anchorMin = new Vector2(0.1f, 0);
            btlRt.anchorMax = new Vector2(0.9f, 0);
            btlRt.pivot = new Vector2(0.5f, 0);
            btlRt.sizeDelta = new Vector2(0, 1.5f);
            bottomLine.GetComponent<Image>().color = new Color(color.r, color.g, color.b, 0.4f);

            int secFontMax = Mathf.RoundToInt(Mathf.Max(14 * uiScale, 10));
            var secTxt = AddTMP(go.transform, "Lbl", label, secFontMax, FontStyles.Bold,
                new Color(color.r, color.g, color.b, 0.85f),
                Vector2.zero, Vector2.zero);
            var secTxtRt = secTxt.GetComponent<RectTransform>();
            secTxtRt.anchorMin = Vector2.zero; secTxtRt.anchorMax = Vector2.one;
            secTxtRt.offsetMin = new Vector2(4, 3); secTxtRt.offsetMax = new Vector2(-4, -3);
            secTxt.enableAutoSizing = true;
            secTxt.fontSizeMin = 8; secTxt.fontSizeMax = secFontMax;
        }

        // ═══════════════════════════════════════════════════════════════
        // ACCENT BAR — Top/bottom card edge
        // ═══════════════════════════════════════════════════════════════
        static void MakeAccentBar(Transform parent, bool top)
        {
            var go = new GameObject(top ? "TopAccent" : "BottomAccent", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            if (top)
            {
                rt.anchorMin = new Vector2(0.05f, 1f);
                rt.anchorMax = new Vector2(0.95f, 1f);
                rt.pivot = new Vector2(0.5f, 1f);
            }
            else
            {
                rt.anchorMin = new Vector2(0.05f, 0f);
                rt.anchorMax = new Vector2(0.95f, 0f);
                rt.pivot = new Vector2(0.5f, 0f);
            }
            rt.sizeDelta = new Vector2(0, 2.5f);
            rt.anchoredPosition = Vector2.zero;

            float gradT = top ? 0f : 1f;
            Color barColor = Color.Lerp(CardBorderA, CardBorderB, gradT);
            go.GetComponent<Image>().color = new Color(barColor.r, barColor.g, barColor.b, 0.6f);

            // Glow behind
            var glow = new GameObject("BarGlow", typeof(RectTransform), typeof(Image));
            glow.transform.SetParent(go.transform, false);
            var glowRt = glow.GetComponent<RectTransform>();
            Stretch(glowRt);
            glowRt.offsetMin = new Vector2(-6, -4);
            glowRt.offsetMax = new Vector2(6, 4);
            glow.GetComponent<Image>().color = new Color(barColor.r, barColor.g, barColor.b, 0.12f);
            glow.GetComponent<Image>().raycastTarget = false;
        }

        // ═══════════════════════════════════════════════════════════════
        // AMBIENT MOTES — Floating particles
        // ═══════════════════════════════════════════════════════════════
        static void SpawnMotes(Transform parent)
        {
            var container = MakeGO("Motes", parent);
            Stretch(container.GetComponent<RectTransform>());
            var fm = container.AddComponent<FloatingMotes>();

            Color[] moteColors = { Accent1, Accent2, CardBorderA, new Color32(255, 200, 130, 255) };

            for (int i = 0; i < 18; i++)
            {
                var dot = new GameObject("Mote", typeof(RectTransform), typeof(Image));
                dot.transform.SetParent(container.transform, false);
                var drt = dot.GetComponent<RectTransform>();
                drt.anchorMin = drt.anchorMax = new Vector2(0.5f, 0.5f);
                float sz = Random.Range(2f, 8f);
                drt.sizeDelta = new Vector2(sz, sz);
                drt.anchoredPosition = new Vector2(
                    Random.Range(-Screen.width * 0.4f, Screen.width * 0.4f),
                    Random.Range(-Screen.height * 0.4f, Screen.height * 0.4f));

                var mImg = dot.GetComponent<Image>();
                Color mCol = moteColors[i % moteColors.Length];
                mImg.color = new Color(mCol.r, mCol.g, mCol.b, Random.Range(0.03f, 0.10f));
                mImg.raycastTarget = false;

                fm.dots.Add(drt);
                fm.imgs.Add(mImg);
            }
        }

        // ═══════════════════════════════════════════════════════════════
        // BLUR CAPTURE
        // ═══════════════════════════════════════════════════════════════
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
                Debug.LogWarning("[GameOver] Blur capture failed: " + e.Message);
                return null;
            }
        }

        // ═══════════════════════════════════════════════════════════════
        // HELPERS
        // ═══════════════════════════════════════════════════════════════

        static void EnsureScreenshotShareManager(GameObject parent)
        {
            if (ScreenshotShareManager.Instance == null)
            {
                var go = new GameObject("ScreenshotShareManager", typeof(ScreenshotShareManager));
                go.transform.SetParent(parent.transform, false);
            }
        }

        static GameObject Stagger(Transform parent, string name, Vector2 pos, Vector2 size, float side)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasGroup));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = size;
            rt.anchoredPosition = pos;

            var origin = go.AddComponent<StaggerOrigin>();
            origin.pos = pos;
            origin.side = side;

            go.GetComponent<CanvasGroup>().alpha = 0f;
            return go;
        }

        static GameObject MakeGO(string name, Transform parent, params System.Type[] comps)
        {
            var typeList = new System.Type[comps.Length + 1];
            typeList[0] = typeof(RectTransform);
            for (int i = 0; i < comps.Length; i++) typeList[i + 1] = comps[i];
            var go = new GameObject(name, typeList);
            go.transform.SetParent(parent, false);
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
            tmp.overflowMode = TextOverflowModes.Truncate;

            // Ensure Turkish Fallback
            if (fallbackFont == null)
                fallbackFont = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
            
            if (tmp.font != null && fallbackFont != null)
            {
                if (tmp.font.fallbackFontAssetTable == null) tmp.font.fallbackFontAssetTable = new List<TMP_FontAsset>();
                if (!tmp.font.fallbackFontAssetTable.Contains(fallbackFont))
                {
                    tmp.font.fallbackFontAssetTable.Add(fallbackFont);
#if UNITY_EDITOR
                    UnityEditor.EditorUtility.SetDirty(tmp.font);
#endif
                }
            }

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
            if (Application.isPlaying) Object.DontDestroyOnLoad(es);
        }
    }
}