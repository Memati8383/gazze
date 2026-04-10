using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

namespace Gazze.Editor
{
    public class PowerUpUISetup : EditorWindow
    {
        [MenuItem("Gazze/UI/Setup PowerUp UI")]
        public static void CreatePowerUpUI()
        {
            Canvas canvas = Object.FindAnyObjectByType<Canvas>();
            if (canvas == null)
            {
                GameObject canvasGo = new GameObject("Canvas");
                canvas = canvasGo.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvasGo.AddComponent<CanvasScaler>();
                canvasGo.AddComponent<GraphicRaycaster>();
            }

            GameObject containerGo = GameObject.Find("PowerUpContainer");
            if (containerGo == null)
            {
                containerGo = new GameObject("PowerUpContainer");
                containerGo.transform.SetParent(canvas.transform, false);
            }

            RectTransform containerRect = containerGo.GetOrAddComponent<RectTransform>();
            containerRect.anchorMin = new Vector2(1, 1);
            containerRect.anchorMax = new Vector2(1, 1);
            containerRect.pivot = new Vector2(1, 1);
            containerRect.anchoredPosition = new Vector2(-2, -170); // Maksimum sağa yaslandı
            containerRect.sizeDelta = new Vector2(400, 500);
            containerRect.localScale = new Vector3(1.05f, 1.05f, 1.05f); // 1.2'den 1.05'e indirildi

            VerticalLayoutGroup layout = containerGo.GetOrAddComponent<VerticalLayoutGroup>();
            layout.spacing = 10;
            layout.childAlignment = TextAnchor.UpperRight;
            layout.childControlHeight = false;
            layout.childControlWidth = false;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;

            var manager = containerGo.GetOrAddComponent<Gazze.UI.PowerUpOverlayManager>();
            manager.overlayContainer = containerGo.transform;

            Selection.activeGameObject = containerGo;

            // Değişikliklerin kaydedilmesi (Play mode dışında) için sahneyi kirli(dirty) işaretle
            if (!Application.isPlaying)
            {
                UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(containerGo.scene);
            }
        }

        [MenuItem("Gazze/UI/Create Premium PowerUp Prefab")]
        public static void CreatePremiumPrefab()
        {
            // ─── ROOT (280x64, layout element for VerticalLayoutGroup) ───
            GameObject root = new GameObject("PowerUpOverlay");
            root.AddComponent<RectTransform>().sizeDelta = new Vector2(280, 64);
            root.AddComponent<CanvasGroup>();
            var le = root.AddComponent<LayoutElement>();
            le.preferredHeight = 64;
            le.preferredWidth = 280;

            // ─── BORDER (2px bigger on each side, glow effect) ──────────
            GameObject border = CreateChild(root, "Border");
            RectTransform borderRT = border.GetComponent<RectTransform>();
            borderRT.anchorMin = Vector2.zero;
            borderRT.anchorMax = Vector2.one;
            borderRT.offsetMin = new Vector2(-2, -2);
            borderRT.offsetMax = new Vector2(2, 2);
            SetupImage(border, null, new Color(0f, 0.85f, 1f, 0.4f), true);

            // ─── BODY (fills root exactly, dark glassmorphic) ───────────
            GameObject body = CreateChild(root, "HUD_Body");
            RectTransform bodyRT = body.GetComponent<RectTransform>();
            bodyRT.anchorMin = Vector2.zero;
            bodyRT.anchorMax = Vector2.one;
            bodyRT.offsetMin = Vector2.zero;
            bodyRT.offsetMax = Vector2.zero;
            SetupImage(body, null, new Color(0.06f, 0.07f, 0.14f, 0.92f), true);

            // ─── LEFT ACCENT STRIP (4px wide, vertically centered) ──────
            GameObject glow = CreateChild(body, "Glow");
            RectTransform glowRT = glow.GetComponent<RectTransform>();
            glowRT.anchorMin = new Vector2(0, 0.1f);
            glowRT.anchorMax = new Vector2(0, 0.9f);
            glowRT.pivot = new Vector2(0, 0.5f);
            glowRT.anchoredPosition = new Vector2(5, 0);
            glowRT.sizeDelta = new Vector2(3, 0);
            SetupImage(glow, null, Color.cyan, false);

            // ─── ICON ORBIT (circle background, left side) ──────────────
            GameObject iconOrbit = CreateChild(body, "IconOrbit");
            RectTransform orbitRT = iconOrbit.GetComponent<RectTransform>();
            orbitRT.anchorMin = new Vector2(0, 0.5f);
            orbitRT.anchorMax = new Vector2(0, 0.5f);
            orbitRT.pivot = new Vector2(0.5f, 0.5f);
            orbitRT.anchoredPosition = new Vector2(35, 0);
            orbitRT.sizeDelta = new Vector2(40, 40);
            SetupImage(iconOrbit, null, new Color(1, 1, 1, 0.08f), false);

            // ─── RADIAL FILL (around icon orbit) ────────────────────────
            GameObject fill = CreateChild(iconOrbit, "Fill");
            RectTransform fillRT = fill.GetComponent<RectTransform>();
            fillRT.anchorMin = Vector2.zero;
            fillRT.anchorMax = Vector2.one;
            fillRT.offsetMin = new Vector2(-3, -3);
            fillRT.offsetMax = new Vector2(3, 3);
            Image fillImg = SetupImage(fill, null, Color.cyan, false);
            fillImg.type = Image.Type.Filled;
            fillImg.fillMethod = Image.FillMethod.Radial360;
            fillImg.fillOrigin = (int)Image.Origin360.Top;
            fillImg.fillClockwise = false;
            fillImg.fillAmount = 1f;

            // ─── ICON (centered in orbit) ───────────────────────────────
            GameObject icon = CreateChild(iconOrbit, "Icon");
            icon.GetComponent<RectTransform>().sizeDelta = new Vector2(26, 26);
            Image iconImg = SetupImage(icon, null, Color.white, false);
            iconImg.preserveAspect = true;

            // ─── LABEL (power-up name, upper-right area) ────────────────
            GameObject label = CreateChild(body, "Label", true);
            RectTransform labelRT = label.GetComponent<RectTransform>();
            labelRT.anchorMin = new Vector2(0, 0.5f);
            labelRT.anchorMax = new Vector2(1, 1f);
            labelRT.offsetMin = new Vector2(60, 0);
            labelRT.offsetMax = new Vector2(-12, -4);
            TextMeshProUGUI labelTxt = label.GetComponent<TextMeshProUGUI>();
            labelTxt.text = "POWER UP";
            labelTxt.fontSize = 14;
            labelTxt.fontStyle = FontStyles.Bold;
            labelTxt.characterSpacing = 3;
            labelTxt.alignment = TextAlignmentOptions.BottomLeft;
            labelTxt.color = Color.white;
            labelTxt.alpha = 0.95f;
            labelTxt.textWrappingMode = TextWrappingModes.NoWrap;
            labelTxt.overflowMode = TextOverflowModes.Ellipsis;
            labelTxt.raycastTarget = false;

            // ─── TIMER (top-right corner) ───────────────────────────────
            GameObject timer = CreateChild(body, "Timer", true);
            RectTransform timerRT = timer.GetComponent<RectTransform>();
            timerRT.anchorMin = new Vector2(0.7f, 0.5f);
            timerRT.anchorMax = new Vector2(1, 1f);
            timerRT.offsetMin = new Vector2(0, 0);
            timerRT.offsetMax = new Vector2(-12, -4);
            TextMeshProUGUI timerTxt = timer.GetComponent<TextMeshProUGUI>();
            timerTxt.text = "5.0s";
            timerTxt.fontSize = 13;
            timerTxt.alignment = TextAlignmentOptions.BottomRight;
            timerTxt.color = new Color(0.5f, 0.75f, 1f, 0.55f);
            timerTxt.textWrappingMode = TextWrappingModes.NoWrap;
            timerTxt.raycastTarget = false;

            // ─── BAR TRACK (bottom half, thin bar) ──────────────────────
            GameObject barTrack = CreateChild(body, "BarTrack");
            RectTransform barTrackRT = barTrack.GetComponent<RectTransform>();
            barTrackRT.anchorMin = new Vector2(0, 0);
            barTrackRT.anchorMax = new Vector2(1, 0);
            barTrackRT.pivot = new Vector2(0.5f, 0);
            barTrackRT.offsetMin = new Vector2(60, 14);
            barTrackRT.offsetMax = new Vector2(-12, 20); // 6px tall
            SetupImage(barTrack, null, new Color(1f, 1f, 1f, 0.1f), true);

            // ─── BAR FILL (inside track) ────────────────────────────────
            GameObject barFill = CreateChild(barTrack, "BarFill");
            RectTransform barFillRT = barFill.GetComponent<RectTransform>();
            barFillRT.anchorMin = Vector2.zero;
            barFillRT.anchorMax = Vector2.one;
            barFillRT.offsetMin = Vector2.zero;
            barFillRT.offsetMax = Vector2.zero;
            Image barFillImg = SetupImage(barFill, null, Color.cyan, true);
            barFillImg.type = Image.Type.Filled;
            barFillImg.fillMethod = Image.FillMethod.Horizontal;
            barFillImg.fillAmount = 1f;

            // ─── SHIMMER (sweep effect, starts invisible) ───────────────
            GameObject shimmer = CreateChild(body, "Shimmer");
            RectTransform shimmerRT = shimmer.GetComponent<RectTransform>();
            shimmerRT.anchorMin = new Vector2(0, 0);
            shimmerRT.anchorMax = new Vector2(0.05f, 1);
            shimmerRT.offsetMin = Vector2.zero;
            shimmerRT.offsetMax = Vector2.zero;
            Image shimmerImg = SetupImage(shimmer, null, new Color(1, 1, 1, 0), false);
            shimmerImg.raycastTarget = false;

            // Done
            Selection.activeGameObject = root;
        }

        // ─── HELPERS ────────────────────────────────────────────────────

        private static GameObject CreateChild(GameObject parent, string name, bool isTMP = false)
        {
            GameObject go;
            if (isTMP)
                go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            else
                go = new GameObject(name, typeof(RectTransform), typeof(Image));

            go.transform.SetParent(parent.transform, false);
            return go;
        }

        private static Image SetupImage(GameObject go, string spritePath, Color color, bool sliced)
        {
            Image img = go.GetComponent<Image>();
            if (img == null) img = go.AddComponent<Image>();

            if (!string.IsNullOrEmpty(spritePath))
            {
                img.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>(spritePath);
                if (sliced) img.type = Image.Type.Sliced;
            }
            img.color = color;
            img.raycastTarget = false;
            return img;
        }
    }

    public static class GameObjectExtensions
    {
        public static T GetOrAddComponent<T>(this GameObject go) where T : Component
        {
            T component = go.GetComponent<T>();
            if (component == null) component = go.AddComponent<T>();
            return component;
        }
    }
}
