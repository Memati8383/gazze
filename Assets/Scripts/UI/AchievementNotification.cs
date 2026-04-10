using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

namespace Gazze.UI
{
    /// <summary>
    /// Başarım bildirimi UI bileşeni - sağdan kayan animasyonlu bildirim
    /// </summary>
    public class AchievementNotification : MonoBehaviour
    {
        [Header("UI References")]
        public TextMeshProUGUI titleText;
        public TextMeshProUGUI descriptionText;
        public Image iconImage;
        public Image backgroundImage;
        
        [Header("Animation Settings")]
        [SerializeField] private float slideInDuration = 0.6f;
        [SerializeField] private float displayDuration = 3.5f;
        [SerializeField] private float slideOutDuration = 0.5f;
        [SerializeField] private AnimationCurve slideInCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        [SerializeField] private AnimationCurve slideOutCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        [SerializeField] private bool useScaleAnimation = true;
        [SerializeField] private bool useRotationAnimation = true;
        
        [Header("Audio Settings")]
        [SerializeField] private AudioClip notificationSound;
        [SerializeField] private float soundVolume = 0.7f;
        
        [Header("Position Settings")]
        [SerializeField] private float offScreenOffset = 400f;
        [SerializeField] private float onScreenPositionX = -50f;
        
        private RectTransform rectTransform;
        private CanvasGroup canvasGroup;
        private Coroutine animationCoroutine;
        
        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            canvasGroup = GetComponent<CanvasGroup>();
            
            if (canvasGroup == null)
            {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }

            if (iconImage != null)
            {
                iconImage.preserveAspect = true;
            }
        }
        
        /// <summary>
        /// Başarım bildirimini gösterir
        /// </summary>
        public void Show(string title, string description, Sprite icon = null)
        {
            if (animationCoroutine != null)
            {
                StopCoroutine(animationCoroutine);
            }
            
            // Null check
            if (titleText == null || descriptionText == null)
            {
                Debug.LogError("AchievementNotification: titleText veya descriptionText null! Prefab referanslarını kontrol edin.");
                return;
            }
            
            // Temayı uygula (Icon burada Resource'dan çekilecek veya Unicode metnine dönecek)
            ApplyTheme(title);
            
            titleText.text = title;
            descriptionText.text = description;
            
            // Eğer dışarıdan harici bir sprite ikonu BİLEREK gönderilmişse temayı ez:
            if (icon != null && iconImage != null)
            {
                iconImage.sprite = icon;
                iconImage.color = Color.white;
            }
            
            // Önceden burada yer alan "else if icon == null, iconImage'ı kapat" BUG'ı silindi.
            // Çünkü kapatırsak yukarıdaki "ApplyTheme"in otomatik yüklediği ikonlar görünmez.
            
            // Obje kapalıysa diye emin olalım:
            if (iconImage != null)
                iconImage.gameObject.SetActive(true);
            
            animationCoroutine = StartCoroutine(AnimateNotification());
        }
        
        /// <summary>
        /// Başarıma özel temayı uygular
        /// </summary>
        private void ApplyTheme(string achievementName)
        {
            AchievementTheme theme = AchievementTheme.GetTheme(achievementName);
            
            if (backgroundImage != null)
            {
                backgroundImage.color = theme.backgroundColor;
            }
            
            if (titleText != null)
            {
                titleText.color = theme.titleColor;
            }
            
            if (descriptionText != null)
            {
                descriptionText.color = theme.descriptionColor;
            }
            
            // Icon güncelleme (Sistem: Sprite vs Emoji Fallback)
            if (iconImage != null)
            {
                var iconTextComponent = iconImage.GetComponentInChildren<TMPro.TextMeshProUGUI>();
                Sprite loadedIcon = null;
                
                if (!string.IsNullOrEmpty(theme.iconPath))
                {
                    loadedIcon = Resources.Load<Sprite>(theme.iconPath);
                    if (loadedIcon == null)
                    {
                        Debug.LogWarning($"[Achievement] Icon not found at path: Resources/{theme.iconPath} for achievement: {theme.achievementName}");
                    }
                }

                if (loadedIcon != null)
                {
                    // Şık yüksek kaliteli PNG ikonu bulundu!
                    iconImage.sprite = loadedIcon;
                    iconImage.color = Color.white; // Alfa kilidini aç
                    iconImage.preserveAspect = true; // Sıkışmayı önle
                    
                    if (iconTextComponent != null)
                    {
                        iconTextComponent.gameObject.SetActive(false); // Emojiyi iptal et
                    }
                }
                else
                {
                    // Fallback: Resim yoksa Emoji metnine geri dön
                    if (iconTextComponent == null)
                    {
                        GameObject iconTextObj = new GameObject("IconText");
                        iconTextObj.transform.SetParent(iconImage.transform, false);
                        iconTextComponent = iconTextObj.AddComponent<TMPro.TextMeshProUGUI>();
                        
                        RectTransform rt = iconTextComponent.GetComponent<RectTransform>();
                        rt.anchorMin = Vector2.zero;
                        rt.anchorMax = Vector2.one;
                        rt.sizeDelta = Vector2.zero;
                        rt.anchoredPosition = Vector2.zero;
                        
                        iconTextComponent.alignment = TMPro.TextAlignmentOptions.Center;
                        iconTextComponent.fontSize = 20; // Küçüldü
                        iconTextComponent.raycastTarget = false;
                    }
                    
                    iconTextComponent.gameObject.SetActive(true);
                    iconTextComponent.text = theme.iconText;
                    iconTextComponent.color = theme.titleColor;
                    iconImage.color = new Color(1, 1, 1, 0); // Kutu sınırını şeffaf yap
                }
            }
            
            // Accent ve glow renklerini güncelle
            Transform accentTransform = transform.Find("Background/Accent");
            if (accentTransform != null)
            {
                Image accentImage = accentTransform.GetComponent<Image>();
                if (accentImage != null)
                {
                    accentImage.color = theme.accentColor;
                }
            }
            
            Transform glowTransform = transform.Find("Background/Glow");
            if (glowTransform != null)
            {
                Image glowImage = glowTransform.GetComponent<Image>();
                if (glowImage != null)
                {
                    glowImage.color = theme.glowColor;
                }
            }
            
            // Parçacık rengini güncelle
            var effectsComponent = GetComponent<AchievementNotificationEffects>();
            if (effectsComponent != null)
            {
                var particleField = typeof(AchievementNotificationEffects).GetField("sparkleEffect", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (particleField != null)
                {
                    ParticleSystem ps = particleField.GetValue(effectsComponent) as ParticleSystem;
                    if (ps != null)
                    {
                        var main = ps.main;
                        main.startColor = theme.particleColor;
                    }
                }
            }
        }
        
        private IEnumerator AnimateNotification()
        {
            // Eğer inspector'dan ses verilmemişse yeni eklenen varsayılan sesi Resources'dan çek
            if (notificationSound == null)
            {
                notificationSound = Resources.Load<AudioClip>("Audio/Achievement_Sound");
            }

            // Ses efekti çal
            if (notificationSound != null)
            {
                if (Settings.AudioManager.Instance != null)
                {
                    Settings.AudioManager.Instance.PlaySFX(notificationSound);
                }
                else
                {
                    AudioSource.PlayClipAtPoint(notificationSound, Camera.main.transform.position, soundVolume);
                }
            }
            
            // Başlangıç pozisyonu (ekran dışı sağda)
            Vector2 startPos = new Vector2(offScreenOffset, rectTransform.anchoredPosition.y);
            Vector2 endPos = new Vector2(onScreenPositionX, rectTransform.anchoredPosition.y);
            
            rectTransform.anchoredPosition = startPos;
            canvasGroup.alpha = 1f;
            
            // Başlangıç scale ve rotation
            Vector3 targetScale = transform.localScale; // Manager tarafından atanan ölçeği (örn: 0.82) baz al
            Vector3 startScale = useScaleAnimation ? targetScale * 0.8f : targetScale;
            Vector3 endScale = targetScale;
            float startRotation = useRotationAnimation ? 5f : 0f;
            
            if (useScaleAnimation) transform.localScale = startScale;
            if (useRotationAnimation) transform.localEulerAngles = new Vector3(0, 0, startRotation);
            
            // Slide In - daha dinamik
            float elapsed = 0f;
            while (elapsed < slideInDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = elapsed / slideInDuration;
                float curveValue = slideInCurve.Evaluate(t);
                
                rectTransform.anchoredPosition = Vector2.Lerp(startPos, endPos, curveValue);
                
                if (useScaleAnimation)
                {
                    transform.localScale = Vector3.Lerp(startScale, endScale, curveValue);
                }
                
                if (useRotationAnimation)
                {
                    float rotation = Mathf.Lerp(startRotation, 0f, curveValue);
                    transform.localEulerAngles = new Vector3(0, 0, rotation);
                }
                
                // Hafif bounce efekti
                if (t > 0.7f)
                {
                    float bounce = Mathf.Sin((t - 0.7f) * 10f) * 0.02f;
                    transform.localScale = endScale * (1f + bounce);
                }
                
                yield return null;
            }
            
            rectTransform.anchoredPosition = endPos;
            transform.localScale = endScale;
            transform.localEulerAngles = Vector3.zero;
            
            // Display
            yield return new WaitForSecondsRealtime(displayDuration);
            
            // Slide Out - hızlı ve smooth
            elapsed = 0f;
            while (elapsed < slideOutDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = elapsed / slideOutDuration;
                float curveValue = slideOutCurve.Evaluate(t);
                
                rectTransform.anchoredPosition = Vector2.Lerp(endPos, startPos, curveValue);
                canvasGroup.alpha = 1f - (t * 0.5f); // Hafif fade
                
                if (useScaleAnimation)
                {
                    transform.localScale = Vector3.Lerp(endScale, startScale * 0.9f, curveValue);
                }
                
                yield return null;
            }
            
            canvasGroup.alpha = 0f;
            gameObject.SetActive(false);
        }
        
        /// <summary>
        /// Bildirimi hemen gizler
        /// </summary>
        public void Hide()
        {
            if (animationCoroutine != null)
            {
                StopCoroutine(animationCoroutine);
            }
            
            canvasGroup.alpha = 0f;
            gameObject.SetActive(false);
        }
    }
}
