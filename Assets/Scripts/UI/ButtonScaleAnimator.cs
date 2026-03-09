using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;
using Settings;

namespace Gazze.UI
{
    /// <summary>
    /// Mobil uyumlu buton animasyon bileşeni. 
    /// Basıldığında büyüme (scale up), ses desteği ve renk değişimi sağlar.
    /// </summary>
    [DisallowMultipleComponent]
    public class ButtonScaleAnimator : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {
        [Header("Scale Animasyon Ayarları")]
        [Tooltip("Basıldığında (Pressed) ulaşılacak boyut.")]
        public float pressedScale = 1.15f;
        [Tooltip("Animasyonun hızı (Düşük = Daha yavaş, yumuşak).")]
        public float animationSpeed = 18f;

        [Header("Ses Efektleri")]
        [Tooltip("Tıklandığında çalacak ses.")]
        public AudioClip clickSound;
        [Tooltip("AudioManager'daki varsayılan tıklama sesini kullan.")]
        public bool useDefaultClickSound = true;

        [Header("Görsel Efektler")]
        [Tooltip("Basıldığında rengi değiştir (Image bileşeni gerekir).")]
        public bool useColorTint = false;
        public Color pressedColor = new Color(0.8f, 0.8f, 0.8f, 1f);

        private Vector3 initialScale;
        private Color initialColor = Color.white;
        private Coroutine activeCoroutine;
        private Image targetImage;
        private bool isPressed = false;

        private void Awake()
        {
            initialScale = transform.localScale;
            targetImage = GetComponent<Image>();
            if (targetImage != null) initialColor = targetImage.color;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            isPressed = true;
            StartScaleAnimation(initialScale * pressedScale);
            
            if (useColorTint && targetImage != null)
                targetImage.color = pressedColor;

            // Haptic Feedback
            HapticManager.Light();

            // Oyun içi kontrol butonlarında (Gaz, Fren, Sol, Sağ, Boost) click sesi istenmiyor.
            // Hem bileşen kontrolü hem de isim kontrolü yaparak sessize alıyoruz.
            string n = gameObject.name.ToLower();
            bool isGameplayButton = GetComponent<UIButtonHandler>() != null || 
                                   n.Contains("gas") || n.Contains("brake") || 
                                   n.Contains("boost") || n.Contains("left") || n.Contains("right");

            if (useDefaultClickSound && AudioManager.Instance != null && !isGameplayButton)
                AudioManager.Instance.PlayClickSound();
            else if (!isGameplayButton)
                PlaySound(clickSound);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (isPressed)
            {
                ResetButton();
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (isPressed)
            {
                ResetButton();
            }
        }

        private void ResetButton()
        {
            isPressed = false;
            
            if (useColorTint && targetImage != null)
                targetImage.color = initialColor;

            StartScaleAnimation(initialScale);
        }

        private void StartScaleAnimation(Vector3 targetScale)
        {
            if (activeCoroutine != null) StopCoroutine(activeCoroutine);
            activeCoroutine = StartCoroutine(AnimateScale(targetScale));
        }

        private IEnumerator AnimateScale(Vector3 target)
        {
            while (Vector3.Distance(transform.localScale, target) > 0.001f)
            {
                transform.localScale = Vector3.Lerp(transform.localScale, target, Time.unscaledDeltaTime * animationSpeed);
                yield return null;
            }
            transform.localScale = target;
            activeCoroutine = null;
        }

        private void PlaySound(AudioClip clip)
        {
            if (clip != null && AudioManager.Instance != null)
            {
                AudioManager.Instance.SendMessage("PlaySFX", clip, SendMessageOptions.DontRequireReceiver);
            }
        }

        private void OnDisable()
        {
            if (activeCoroutine != null)
            {
                StopCoroutine(activeCoroutine);
                activeCoroutine = null;
            }
            transform.localScale = initialScale;
            if (useColorTint && targetImage != null) targetImage.color = initialColor;
            isPressed = false;
        }
    }
}
