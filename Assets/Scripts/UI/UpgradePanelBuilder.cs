// V7 - 5 Upgrade System
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

namespace Gazze.UI
{
    /// <summary>
    /// Upgrade Panel – kart içi tamamen anchor tabanlı, LayoutElement.ignoreLayout ile VLG çakışması engelli.
    /// </summary>
    public class UpgradePanelBuilder : MonoBehaviour
    {
        static TMP_FontAsset fallbackFont;
        [Header("Colors")]
        [Tooltip("Panel genelinde kullanılan varsayılan vurgu rengi.")]
        public Color accentColor  = new Color32(255, 160, 80, 255); // Rich Amber
        [Tooltip("Maksimum seviye ve premium vurgu rengi.")]
        public Color goldColor    = new Color32(255, 215, 100, 255); // Polished Gold
        [Tooltip("Birincil metin rengi.")]
        public Color textColor    = new Color32(255, 253, 245, 255); // Silk White
        [Tooltip("İkincil metin rengi.")]
        public Color subTextColor = new Color32(210, 190, 160, 210); // Patina Gold
        [Tooltip("Kart arka plan rengi.")]
        public Color cardColor    = new Color32(25, 20, 15, 235); // Smoked Obsidian
        [Tooltip("Boş segmentlerin rengi.")]
        public Color segmentEmpty = new Color32(60, 55, 50, 200); // Desert Ash
        [Tooltip("Yükseltme satın alma butonu rengi.")]
        public Color btnBuyColor  = new Color32(255, 140, 40, 255); // Burnt Orange

        private const float CARD_H = 150f;

        static readonly Color[] CardAccents =
        {
            new Color32(255, 215, 130, 255), // Speed - Gold
            new Color32(100, 220, 140, 255), // Accel - Mint Green (Muted)
            new Color32(255,  85,  70, 255), // Durability - Crimson
            new Color32(255, 160,  60, 255), // Boost - Amber
            new Color32(255, 240, 200, 255), // Refill - Ivory
        };
        static readonly string[] Icons  = { "SPD", "ACC", "DUR", "BST", "FIL" };
        static readonly string[] Labels = { "HIZ", "İVME", "CAN", "BOOST", "DOLUM" };
        static readonly string[] LocKeys = { "Garage_Label_Speed", "Garage_Label_Accel", "Garage_Label_Durability", "Garage_Label_Boost", "Garage_Label_Refill" };

        // ─── ENTRY POINT ────────────────────────────────────────────────
        [ContextMenu("YÜKSELTME PANELİNİ SIFIRDAN KUR")]
        public void BuildUpgradePanel()
        {
            // Temizlik
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                var ch = transform.GetChild(i).gameObject;
                if (Application.isPlaying) Destroy(ch);
                else DestroyImmediate(ch);
            }

            // ── Ana panel ──────────────────────────────────────────────
            var rt = GetComponent<RectTransform>() ?? gameObject.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.04f, 0.01f);
            rt.anchorMax = new Vector2(0.96f, 0.25f);
            rt.pivot     = new Vector2(0.5f, 0.5f);
            rt.offsetMin = rt.offsetMax = Vector2.zero;

            // Transparan arka plan (hardcoded – Inspector serialize override'ı yoksay)
            var bg = gameObject.GetComponent<Image>() ?? gameObject.AddComponent<Image>();
            bg.color = new Color(0, 0, 0, 0);

            // Eski Outline varsa sil
            var oldOL = gameObject.GetComponent<Outline>();
            if (oldOL != null)
            {
                if (Application.isPlaying) Destroy(oldOL);
                else DestroyImmediate(oldOL);
            }

            // VLG – Header, Sep, CardRow yönetiyor
            var vlg = gameObject.GetComponent<VerticalLayoutGroup>()
                   ?? gameObject.AddComponent<VerticalLayoutGroup>();
            vlg.padding                = new RectOffset(14, 14, 10, 10);
            vlg.spacing                = 6;
            vlg.childAlignment         = TextAnchor.UpperCenter;
            vlg.childControlHeight     = true;
            vlg.childControlWidth      = true;
            vlg.childForceExpandHeight = false;
            vlg.childForceExpandWidth  = true;

            // ── Header ────────────────────────────────────────────
            BuildHeader();

            // ── Separator ─────────────────────────────────────────
            {
                var s = new GameObject("Sep", typeof(RectTransform));
                s.transform.SetParent(transform, false);
                AddLE(s, -1, 1, 1, 0);
                s.AddComponent<Image>().color =
                    new Color(accentColor.r, accentColor.g, accentColor.b, 0.15f);
            }

            // ── CardRow – SABİT YÜKSEKLİK ────────────────────────
            var rowGo = new GameObject("CardRow", typeof(RectTransform));
            rowGo.transform.SetParent(transform, false);
            AddLE(rowGo, -1, CARD_H, 1, 0);

            var hlg = rowGo.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing                = 10;
            hlg.childAlignment         = TextAnchor.MiddleCenter;
            hlg.childControlWidth      = true;
            hlg.childControlHeight     = true;
            hlg.childForceExpandWidth  = true;
            hlg.childForceExpandHeight = true;

            // ── 4 Kart ────────────────────────────────────────────
            var lvlTexts  = new List<TextMeshProUGUI>();
            var costTexts = new List<TextMeshProUGUI>();
            var buttons   = new List<Button>();

            for (int i = 0; i < 5; i++)
            {
                BuildCard(i, rowGo.transform, out var l, out var c, out var b);
                lvlTexts.Add(l); costTexts.Add(c); buttons.Add(b);
                int idx = i;
                b.onClick.AddListener(() =>
                {
                    var m = FindFirstObjectByType<MainMenuManager>();
                    if (m != null) m.OnUpgradeClicked(idx);
                });
            }

            SetupRefs(lvlTexts, costTexts, buttons);
            Debug.Log("<color=cyan>Gazze:</color> Upgrade Panel kuruldu!");
        }

        // ─── HEADER ─────────────────────────────────────────────────
        void BuildHeader()
        {
            var go = new GameObject("Header", typeof(RectTransform));
            go.transform.SetParent(transform, false);
            AddLE(go, -1, 22, 1, 0);

            var h = go.AddComponent<HorizontalLayoutGroup>();
            h.childAlignment         = TextAnchor.MiddleCenter;
            h.spacing                = 10;
            h.childControlWidth      = false;
            h.childControlHeight     = false;
            h.childForceExpandWidth  = false;
            h.childForceExpandHeight = false;

            MkLine(go.transform, 40);

            var tg = new GameObject("Title", typeof(RectTransform));
            tg.transform.SetParent(go.transform, false);
            tg.GetComponent<RectTransform>().sizeDelta = new Vector2(190, 20);
            var t = tg.AddComponent<TextMeshProUGUI>();
            
            string titleKey = "Garage_Upgrades_Title";
            t.text = LocalizationManager.Instance != null ? LocalizationManager.Instance.GetTranslation(titleKey) : "GELİŞTİRMELER";
            
            var loc = tg.AddComponent<LocalizedText>();
            loc.SetKey(titleKey);

            t.fontSize = 14;
            t.fontStyle = FontStyles.Bold; t.alignment = TextAlignmentOptions.Center;
            t.color = textColor; t.characterSpacing = 3; t.raycastTarget = false;

            EnsureFontFallback(t);
            MkLine(go.transform, 40);
        }

        void EnsureFontFallback(TextMeshProUGUI t)
        {
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
        }

        void MkLine(Transform p, float w)
        {
            var g = new GameObject("Ln", typeof(RectTransform));
            g.transform.SetParent(p, false);
            g.GetComponent<RectTransform>().sizeDelta = new Vector2(w, 2);
            g.AddComponent<Image>().color =
                new Color(accentColor.r, accentColor.g, accentColor.b, 0.4f);
        }

        // ─── KART ────────────────────────────────────────────────────
        //
        //  Anchor layout (Y: 0=alt, 1=üst):
        //    0.76–0.97   İkon + İsim
        //    0.55–0.73   Seviye metni
        //    0.37–0.53   Progress bar
        //    0.06–0.32   Buton ($+fiyat)
        //
        //  Tüm elemanlar ignoreLayout=true ile VLG'den bağımsız.

        void BuildCard(int idx, Transform parent,
            out TextMeshProUGUI levelText,
            out TextMeshProUGUI costText,
            out Button btn)
        {
            Color accent = CardAccents[idx % CardAccents.Length];

            // ── Kart kökü ──
            var card = new GameObject($"Card{idx}", typeof(RectTransform));
            card.transform.SetParent(parent, false);
            card.AddComponent<Image>().color = cardColor;
            // Kart yüksekliği HLG.childForceExpandHeight ile CardRow'dan geliyor

            // ── Sol aksan çizgisi ──
            var bar = Anchor(card.transform, "Bar",
                new Vector2(0f, 0.08f), new Vector2(0f, 0.92f),
                new Vector2(3.5f, 0));
            bar.AddComponent<Image>().color = accent;

            // ── İkon + İsim (üst) ──
            var topGo = Anchor(card.transform, "Top",
                new Vector2(0.07f, 0.76f), new Vector2(0.98f, 0.97f));

            var topH = topGo.AddComponent<HorizontalLayoutGroup>();
            topH.childAlignment = TextAnchor.MiddleLeft; topH.spacing = 5;
            topH.childControlWidth = false; topH.childControlHeight = false;
            topH.childForceExpandWidth = false; topH.childForceExpandHeight = false;

            // İkon badge
            var iGo = new GameObject("Ico", typeof(RectTransform));
            iGo.transform.SetParent(topGo.transform, false);
            iGo.GetComponent<RectTransform>().sizeDelta = new Vector2(26, 16);
            iGo.AddComponent<Image>().color = new Color(accent.r, accent.g, accent.b, 0.25f);
            // İkon text üzerine (stretch)
            var iTgo = new GameObject("IcoT", typeof(RectTransform));
            iTgo.transform.SetParent(iGo.transform, false);
            var iRT = iTgo.GetComponent<RectTransform>();
            iRT.anchorMin = Vector2.zero; iRT.anchorMax = Vector2.one; iRT.sizeDelta = Vector2.zero;
            var iText = iTgo.AddComponent<TextMeshProUGUI>();
            iText.text = Icons[idx]; iText.fontSize = 7f; iText.color = accent;
            iText.fontStyle = FontStyles.Bold;
            iText.alignment = TextAlignmentOptions.Center; iText.raycastTarget = false;

            // İsim (Localized)
            var nGo = new GameObject("Nm", typeof(RectTransform));
            nGo.transform.SetParent(topGo.transform, false);
            nGo.GetComponent<RectTransform>().sizeDelta = new Vector2(120, 16);
            var nT = nGo.AddComponent<TextMeshProUGUI>();
            nT.text = Labels[idx]; nT.fontSize = 9f; nT.color = textColor;
            nT.fontStyle = FontStyles.Bold; nT.alignment = TextAlignmentOptions.Left;
            nT.raycastTarget = false; nT.textWrappingMode = TextWrappingModes.NoWrap;
            
            var loc = nGo.AddComponent<LocalizedText>();
            loc.SetKey(LocKeys[idx]);

            // ── Seviye metni ──
            var lvlGo = Anchor(card.transform, "Lvl",
                new Vector2(0.07f, 0.55f), new Vector2(0.95f, 0.73f));
            levelText = lvlGo.AddComponent<TextMeshProUGUI>();
            levelText.text = "LVL 0   —   0 km/h";
            levelText.fontSize = 8.5f; levelText.color = subTextColor;
            levelText.alignment = TextAlignmentOptions.Left;
            levelText.raycastTarget = false; levelText.textWrappingMode = TextWrappingModes.NoWrap;

            // ── Progress Bar (5 segment) ──
            var progGo = Anchor(card.transform, "Prog",
                new Vector2(0.06f, 0.37f), new Vector2(0.94f, 0.53f));

            var pH = progGo.AddComponent<HorizontalLayoutGroup>();
            pH.spacing = 4; pH.childAlignment = TextAnchor.MiddleCenter;
            pH.childControlWidth = true; pH.childControlHeight = true;
            pH.childForceExpandWidth = true; pH.childForceExpandHeight = true;
            pH.padding = new RectOffset(1, 1, 1, 1);

            var segs = new Image[5];
            for (int s = 0; s < 5; s++)
            {
                var sg = new GameObject($"S{s}", typeof(RectTransform));
                sg.transform.SetParent(progGo.transform, false);
                segs[s] = sg.AddComponent<Image>();
                segs[s].color = segmentEmpty;
            }
            var holder = card.AddComponent<SegmentHolder>();
            holder.segments = segs;
            holder.cardIndex = idx;

            // ── Buton (alt) ──
            var btnGo = Anchor(card.transform, "Btn",
                new Vector2(0.05f, 0.06f), new Vector2(0.95f, 0.32f));
            var btnImg = btnGo.AddComponent<Image>();
            btnImg.color = btnBuyColor;

            btn = btnGo.AddComponent<Button>();
            var cb = btn.colors;
            cb.normalColor      = btnBuyColor;
            cb.highlightedColor = accentColor;
            cb.pressedColor     = new Color(accentColor.r, accentColor.g, accentColor.b, 0.4f);
            cb.disabledColor    = new Color(0.2f, 0.2f, 0.2f, 0.8f);
            btn.colors = cb;

            var sa = btnGo.AddComponent<ButtonScaleAnimator>();
            sa.pressedScale = 0.93f; sa.animationSpeed = 22f;
            sa.useDefaultClickSound = true;

            // Buton içi HLG
            var bH = btnGo.AddComponent<HorizontalLayoutGroup>();
            bH.childAlignment = TextAnchor.MiddleCenter; bH.spacing = 5;
            bH.padding = new RectOffset(4, 4, 2, 2);
            bH.childControlWidth = false; bH.childControlHeight = false;
            bH.childForceExpandWidth = false; bH.childForceExpandHeight = false;

            // $ simgesi
            var coinGo = new GameObject("Coin", typeof(RectTransform));
            coinGo.transform.SetParent(btnGo.transform, false);
            coinGo.GetComponent<RectTransform>().sizeDelta = new Vector2(14, 18);
            var coinT = coinGo.AddComponent<TextMeshProUGUI>();
            coinT.text = "$"; coinT.fontSize = 11; coinT.color = goldColor;
            coinT.fontStyle = FontStyles.Bold;
            coinT.alignment = TextAlignmentOptions.Center; coinT.raycastTarget = false;

            // Fiyat metni
            var priceGo = new GameObject("Price", typeof(RectTransform));
            priceGo.transform.SetParent(btnGo.transform, false);
            priceGo.GetComponent<RectTransform>().sizeDelta = new Vector2(72, 18);
            costText = priceGo.AddComponent<TextMeshProUGUI>();
            costText.text = "500"; costText.fontSize = 10f;
            costText.fontStyle = FontStyles.Bold; costText.color = Color.white;
            costText.alignment = TextAlignmentOptions.Center;
            costText.raycastTarget = false; costText.textWrappingMode = TextWrappingModes.NoWrap;
        }

        // ─── YARDIMCI ───────────────────────────────────────────────────

        /// <summary>
        /// Anchor tabanlı child oluşturur. ignoreLayout=true ile
        /// panel VLG'sinden tamamen bağımsız.
        /// </summary>
        static GameObject Anchor(Transform parent, string name,
            Vector2 anchorMin, Vector2 anchorMax,
            Vector2? overrideSizeDelta = null)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);

            // VLG/HLG'den bağımsız olması için ignoreLayout
            go.AddComponent<LayoutElement>().ignoreLayout = true;

            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.pivot     = new Vector2(0.5f, 0.5f);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            if (overrideSizeDelta.HasValue) rt.sizeDelta = overrideSizeDelta.Value;
            return go;
        }

        static void AddLE(GameObject go, float pw, float ph, float fw, float fh)
        {
            var le = go.GetComponent<LayoutElement>() ?? go.AddComponent<LayoutElement>();
            if (pw >= 0) le.preferredWidth  = pw;
            if (ph >= 0) { le.preferredHeight = ph; le.minHeight = ph; }
            le.flexibleWidth  = fw;
            le.flexibleHeight = fh;
        }

        void SetupRefs(List<TextMeshProUGUI> lvls,
                       List<TextMeshProUGUI> costs,
                       List<Button> btns)
        {
            var menu = FindFirstObjectByType<MainMenuManager>();
            if (menu == null) return;
            menu.upgradePanel      = gameObject;
            menu.upgradeLevelTexts = lvls.ToArray();
            menu.upgradeCostTexts  = costs.ToArray();
            menu.upgradeButtons    = btns.ToArray();
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(menu);
#endif
        }
    }

    // ─── SEGMENT HOLDER ────────────────────────────────────────────
    /// <summary>Progress segmentlerini tutar ve UpdateLevel API'si sunar.</summary>
    public class SegmentHolder : MonoBehaviour
    {
        [HideInInspector] public Image[] segments;
        [HideInInspector] public int cardIndex;

        static readonly Color GoldMax = new Color32(255, 200,  50, 255);
        static readonly Color Empty   = new Color32( 50,  56,  85, 255);

        static readonly Color[] Accents =
        {
            new Color32(255, 215, 130, 255),
            new Color32(100, 220, 140, 255),
            new Color32(255,  85,  70, 255),
            new Color32(255, 160,  60, 255),
            new Color32(255, 240, 200, 255),
        };

        public void UpdateLevel(int level, int maxLevel)
        {
            if (segments == null) return;
            Color fill = Accents[Mathf.Clamp(cardIndex, 0, Accents.Length - 1)];
            for (int i = 0; i < segments.Length; i++)
            {
                if (segments[i] == null) continue;
                segments[i].color = (i < level)
                    ? (level >= maxLevel ? GoldMax : fill)
                    : Empty;
            }
        }
    }
}
