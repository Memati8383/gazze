using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Gazze.UI
{
    public class PowerUpOverlayManager : MonoBehaviour
    {
        [Header("Prefabs")]
        public GameObject overlayPrefab;
        public Transform overlayContainer;

        [Header("Timing")]
        public float entryTime = 0.5f;
        public float exitTime = 0.35f;

        [Header("Shimmer")]
        public float shimmerInterval = 3f;
        public float shimmerSpeed = 1.8f;

        private class PowerUpUIItem
        {
            public GameObject gameObject;
            public Image fillImage;
            public Image barFillImage;
            public Image glowImage;
            public Image borderImage;
            public Image shimmerImage;
            public Transform iconTransform;
            public TextMeshProUGUI timerText;
            public TextMeshProUGUI labelText;
            public float totalDuration;
            public float shimmerTimer;
            public Color themeColor;
            public bool isExpiring;
        }

        private Dictionary<Gazze.PowerUps.PowerUpType, PowerUpUIItem> activeUI = new Dictionary<Gazze.PowerUps.PowerUpType, PowerUpUIItem>();
        private bool isSubscribed = false;

        private void Awake()
        {
            // Oyun başladığında veya obje aktif olduğunda, konteynerin konumunu ve boyutunu
            // garanti altına al (editör scriptini çalıştırmaya gerek kalmadan Play modunda çalışması için).
            RectTransform rect = GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.anchorMin = new Vector2(0, 1);
                rect.anchorMax = new Vector2(0, 1);
                rect.pivot = new Vector2(0, 1);
                rect.anchoredPosition = new Vector2(25, -450); // Sol HUD altına hizalandı
                rect.sizeDelta = new Vector2(400, 600);
                rect.localScale = Vector3.one;

                // Layout Group ile animasyon çakışmasını önlemek için devre dışı bırakıyoruz
                var layout = GetComponent<UnityEngine.UI.VerticalLayoutGroup>();
                if (layout != null) layout.enabled = false;
            }
        }

        private void OnEnable() => TrySubscribe();

        private void Update()
        {
            if (!isSubscribed) TrySubscribe();

            foreach (var kvp in activeUI)
            {
                var item = kvp.Value;
                if (item.iconTransform != null) item.iconTransform.Rotate(Vector3.forward, 30f * Time.deltaTime);

                if (item.glowImage != null)
                {
                    float pulse = 0.5f + Mathf.Sin(Time.time * 2.5f) * 0.3f;
                    item.glowImage.color = new Color(item.themeColor.r, item.themeColor.g, item.themeColor.b, pulse);
                }

                if (item.borderImage != null)
                {
                    float borderPulse = 0.15f + Mathf.Sin(Time.time * 1.8f + 0.5f) * 0.12f;
                    item.borderImage.color = new Color(item.themeColor.r, item.themeColor.g, item.themeColor.b, borderPulse);
                }

                if (item.shimmerImage != null)
                {
                    item.shimmerTimer -= Time.deltaTime;
                    if (item.shimmerTimer <= 0)
                    {
                        item.shimmerTimer = shimmerInterval;
                        StartCoroutine(ShimmerSweep(item.shimmerImage));
                    }
                }

                if (item.isExpiring && item.labelText != null)
                {
                    float flash = 0.5f + Mathf.Sin(Time.time * 12f) * 0.5f;
                    item.labelText.color = Color.Lerp(Color.white, Color.red, flash);
                    if (item.borderImage != null) item.borderImage.color = Color.Lerp(item.themeColor, Color.red, flash);
                }
            }
        }

        private void TrySubscribe()
        {
            if (isSubscribed) return;
            if (Gazze.PowerUps.PowerUpManager.Instance != null)
            {
                Gazze.PowerUps.PowerUpManager.Instance.OnPowerUpActivated += SpawnOverlay;
                Gazze.PowerUps.PowerUpManager.Instance.OnPowerUpDeactivated += RemoveOverlay;
                Gazze.PowerUps.PowerUpManager.Instance.OnPowerUpTimerUpdated += UpdateTimer;
                Gazze.PowerUps.PowerUpManager.Instance.OnPowerUpExpiring += (type) => {
                    if (activeUI.TryGetValue(type, out var item)) item.isExpiring = true;
                };
                isSubscribed = true;
            }
        }

        private void SpawnOverlay(Gazze.PowerUps.PowerUpType type, float duration, float totalDuration)
        {
            if (activeUI.TryGetValue(type, out PowerUpUIItem existing))
            {
                existing.totalDuration = totalDuration;
                StartCoroutine(PulseRefresh(existing.gameObject));
                UpdateTimer(type, duration);
                return;
            }

            if (overlayPrefab == null) return;

            GameObject go = Instantiate(overlayPrefab, overlayContainer);
            
            // Fix layout and pivot for left alignment
            RectTransform childRt = go.GetComponent<RectTransform>();
            if (childRt != null)
            {
                childRt.anchorMin = new Vector2(0, 1);
                childRt.anchorMax = new Vector2(0, 1);
                childRt.pivot = new Vector2(0, 1);
            }

            var data = Gazze.PowerUps.PowerUpManager.Instance.GetData(type);
            if (data == null) return;

            Image icon = FindChild<Image>(go, "Icon");
            if (icon != null) { icon.sprite = data.icon; icon.color = Color.white; }

            Image glow = FindChild<Image>(go, "Glow");
            if (glow != null) glow.color = new Color(data.themeColor.r, data.themeColor.g, data.themeColor.b, 0.6f);

            Image fill = FindChild<Image>(go, "Fill");
            if (fill != null) { fill.color = data.themeColor; fill.fillAmount = 1.0f; }

            Image barFill = FindChild<Image>(go, "BarFill");
            if (barFill != null) { barFill.color = data.themeColor; barFill.fillAmount = 1.0f; }

            Image border = FindChild<Image>(go, "Border");
            if (border != null) border.color = new Color(data.themeColor.r, data.themeColor.g, data.themeColor.b, 0.25f);

            TextMeshProUGUI label = FindChild<TextMeshProUGUI>(go, "Label");
            if (label != null) 
            { 
                string locKey = "PowerUp_" + type.ToString();
                label.text = LocalizationManager.Get(locKey, data.displayName.ToUpper());
                label.color = Color.white; 
            }


            TextMeshProUGUI timer = FindChild<TextMeshProUGUI>(go, "Timer");
            if (timer != null) { timer.text = $"{duration:F1}s"; timer.color = new Color(data.themeColor.r, data.themeColor.g, data.themeColor.b, 0.7f); }

            activeUI.Add(type, new PowerUpUIItem
            {
                gameObject = go, fillImage = fill, barFillImage = barFill, glowImage = glow, borderImage = border,
                shimmerImage = FindChild<Image>(go, "Shimmer"), iconTransform = icon != null ? icon.transform : null,
                timerText = timer, labelText = label, totalDuration = totalDuration, shimmerTimer = 1f, themeColor = data.themeColor
            });

            ArrangeItems();
            StartCoroutine(SpringEntry(go));
        }

        private void ArrangeItems()
        {
            int index = 0;
            foreach (var kvp in activeUI)
            {
                RectTransform rt = kvp.Value.gameObject.GetComponent<RectTransform>();
                if (rt != null)
                {
                    Vector2 target = new Vector2(0, -index * 130f); // 130px spacing
                    StartCoroutine(MoveToPosition(rt, target, 0.4f));
                }
                index++;
            }
        }

        private IEnumerator MoveToPosition(RectTransform rt, Vector2 target, float duration)
        {
            if (rt == null) yield break;
            Vector2 start = rt.anchoredPosition;
            float elapsed = 0;
            while (elapsed < duration)
            {
                if (rt == null) yield break;
                elapsed += Time.deltaTime;
                rt.anchoredPosition = Vector2.Lerp(start, target, elapsed / duration);
                yield return null;
            }
            if (rt != null) rt.anchoredPosition = target;
        }

        private void UpdateTimer(Gazze.PowerUps.PowerUpType type, float remainingTime)
        {
            if (!activeUI.TryGetValue(type, out PowerUpUIItem item)) return;

            float ratio = Mathf.Clamp01(remainingTime / item.totalDuration);
            if (item.fillImage != null) item.fillImage.fillAmount = ratio;
            if (item.barFillImage != null) item.barFillImage.fillAmount = ratio;
            if (item.timerText != null) item.timerText.text = $"{Mathf.Max(0, remainingTime):F1}s";

            if (remainingTime < 2f)
            {
                float flash = (Mathf.Sin(Time.time * 18f) + 1f) * 0.5f;
                Color warningColor = Color.Lerp(Color.red, item.themeColor, flash);
                if (item.fillImage != null) item.fillImage.color = warningColor;
                if (item.barFillImage != null) item.barFillImage.color = warningColor;
                item.gameObject.transform.localScale = Vector3.one * (1.0f + Mathf.Sin(Time.time * 20f) * 0.025f);
                if (item.timerText != null) item.timerText.color = new Color(1f, 0.3f, 0.3f, 0.9f);
            }
            else
            {
                if (item.fillImage != null) item.fillImage.color = item.themeColor;
                if (item.barFillImage != null) item.barFillImage.color = item.themeColor;
                if (item.timerText != null) item.timerText.color = new Color(item.themeColor.r, item.themeColor.g, item.themeColor.b, 0.7f);
                item.gameObject.transform.localScale = Vector3.one;
            }
        }

        private IEnumerator SpringEntry(GameObject go)
        {
            CanvasGroup cg = go.GetComponent<CanvasGroup>() ?? go.AddComponent<CanvasGroup>();
            RectTransform rt = go.GetComponent<RectTransform>();
            Vector2 finalPos = rt.anchoredPosition;
            Vector2 startPos = finalPos + new Vector2(-550f, 0); 
            rt.anchoredPosition = startPos; // Start immediately from off-screen
            
            float elapsed = 0;
            while (elapsed < entryTime)
            {
                if (go == null) yield break;
                elapsed += Time.deltaTime;
                float t = elapsed / entryTime;
                float ease = 1f - Mathf.Pow(1f - t, 3); // Cubic Ease Out
                
                // Entry slide
                // Update finalPos in case order changed during entry
                float currentY = rt.anchoredPosition.y; 
                rt.anchoredPosition = new Vector2(Mathf.Lerp(startPos.x, finalPos.x, ease), currentY);
                
                // Entry scale pop
                float scale = 0.8f + (Mathf.Sin(t * Mathf.PI) * 0.15f) + (t * 0.2f);
                go.transform.localScale = Vector3.one * scale;
                cg.alpha = Mathf.Min(1, t * 2f);
                yield return null;
            }
            if (go != null) 
            { 
                go.transform.localScale = Vector3.one; 
                cg.alpha = 1; 
                if (rt != null) rt.anchoredPosition = finalPos;
            }
        }

        private IEnumerator PulseRefresh(GameObject go)
        {
            float elapsed = 0;
            while (elapsed < 0.3f)
            {
                if (go == null) yield break;
                elapsed += Time.deltaTime;
                go.transform.localScale = Vector3.one * (1.0f + Mathf.Sin((elapsed / 0.3f) * Mathf.PI) * 0.12f);
                yield return null;
            }
            if (go != null) go.transform.localScale = Vector3.one;
        }

        private IEnumerator ShimmerSweep(Image shimmer)
        {
            if (shimmer == null) yield break;
            RectTransform rt = shimmer.rectTransform;
            float elapsed = 0;
            while (elapsed < shimmerSpeed)
            {
                if (shimmer == null) yield break;
                elapsed += Time.deltaTime;
                float t = elapsed / shimmerSpeed;
                rt.anchorMin = new Vector2(Mathf.Clamp01(Mathf.Lerp(-0.05f, 1.05f, t)), 0);
                rt.anchorMax = new Vector2(Mathf.Clamp01(rt.anchorMin.x + 0.08f), 1);
                rt.offsetMin = rt.offsetMax = Vector2.zero;
                shimmer.color = new Color(1, 1, 1, Mathf.Sin(t * Mathf.PI) * 0.08f);
                yield return null;
            }
            if (shimmer != null) shimmer.color = new Color(1, 1, 1, 0);
        }

        private void RemoveOverlay(Gazze.PowerUps.PowerUpType type)
        {
            if (activeUI.TryGetValue(type, out PowerUpUIItem item))
            {
                activeUI.Remove(type);
                ArrangeItems();
                StartCoroutine(FadeOutAndDestroy(item.gameObject));
            }
        }

        private IEnumerator FadeOutAndDestroy(GameObject go)
        {
            CanvasGroup cg = go.GetComponent<CanvasGroup>() ?? go.AddComponent<CanvasGroup>();
            float elapsed = 0;
            while (elapsed < exitTime)
            {
                if (go == null) yield break;
                elapsed += Time.deltaTime;
                float t = elapsed / exitTime;
                cg.alpha = 1f - t;
                go.transform.localScale = Vector3.one * (1f - (t * 0.5f));
                yield return null;
            }
            if (go != null) Destroy(go);
        }

        private void OnDestroy()
        {
            if (Gazze.PowerUps.PowerUpManager.Instance != null)
            {
                Gazze.PowerUps.PowerUpManager.Instance.OnPowerUpActivated -= SpawnOverlay;
                Gazze.PowerUps.PowerUpManager.Instance.OnPowerUpDeactivated -= RemoveOverlay;
                Gazze.PowerUps.PowerUpManager.Instance.OnPowerUpTimerUpdated -= UpdateTimer;
            }
        }

        private static T FindChild<T>(GameObject root, string childName) where T : Component
        {
            return Array.Find(root.GetComponentsInChildren<T>(true), c => c.gameObject.name == childName);
        }
    }
}
