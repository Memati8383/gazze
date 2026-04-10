using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

namespace Gazze.UI
{
    /// <summary>
    /// Gelişmiş Ana Menü İpucu Sistemi.
    /// Daktilo efekti, imleç, ilerleme çubuğu ve etkileşimli geçiş desteği.
    /// </summary>
    public class MenuTipsDisplay : MonoBehaviour, IPointerClickHandler
    {
        [Header("UI References")]
        public TextMeshProUGUI tipText;
        public Image progressBar;
        
        [Header("Timing Settings")]
        [SerializeField] private float displayDuration = 7.0f;
        [SerializeField] private float fadeDuration = 0.6f;
        [SerializeField] private float typeSpeed = 0.03f;
        
        [Header("Aesthetics")]
        [SerializeField] private bool useCursor = true;
        [SerializeField] private char cursorChar = '_';
        [SerializeField] private float slideOffset = 15f;
        
        private string[] tipKeys = { 
            "Tip_Safe", 
            "Tip_Damage", 
            "Tip_Hope", 
            "Tip_Alert", 
            "Tip_Fuel", 
            "Tip_Duty",
            "Tip_Night",
            "Tip_Upgrade",
            "Tip_Combo",
            "Tip_Powerup",
            "Tip_Magnet",
            "Tip_Shield",
            "Tip_Garage",
            "Tip_Score",
            "Tip_Unity",
            "Tip_Neon"
        };

        private List<string> shuffledKeys = new List<string>();
        private int currentTipIndex = 0;
        private Coroutine rotationCoroutine;
        private CanvasGroup canvasGroup;
        private RectTransform rectTransform;
        private Vector2 initialPosition;
        private bool isSkipping = false;

        private void Awake()
        {
            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();
            
            rectTransform = GetComponent<RectTransform>();
            if (rectTransform != null) initialPosition = rectTransform.anchoredPosition;

            // Arka plan tıklandığında geçiş yapabilmesi için RaycastTarget kontrolü
            var img = GetComponent<Image>();
            if (img != null) img.raycastTarget = true;
        }

        private void OnEnable()
        {
            if (tipText != null)
            {
                ShuffleTips();
                rotationCoroutine = StartCoroutine(RotateTipsRoutine());
            }
        }

        private void OnDisable()
        {
            if (rotationCoroutine != null) StopCoroutine(rotationCoroutine);
        }

        private void ShuffleTips()
        {
            shuffledKeys = new List<string>(tipKeys);
            for (int i = 0; i < shuffledKeys.Count; i++)
            {
                string temp = shuffledKeys[i];
                int randomIndex = Random.Range(i, shuffledKeys.Count);
                shuffledKeys[i] = shuffledKeys[randomIndex];
                shuffledKeys[randomIndex] = temp;
            }
            currentTipIndex = 0;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (!isSkipping)
            {
                isSkipping = true;
                // Mevcut beklemeyi kesip bir sonrakine geçmeyi sağlar
            }
        }

        private IEnumerator RotateTipsRoutine()
        {
            while (true)
            {
                isSkipping = false;

                // 1. Get Translation
                string fullText = shuffledKeys[currentTipIndex];
                if (LocalizationManager.Instance != null)
                {
                    fullText = LocalizationManager.Instance.GetTranslation(shuffledKeys[currentTipIndex]);
                }

                // 2. Reveal Animation (Fade In + Slide Up)
                canvasGroup.alpha = 0;
                if (rectTransform != null)
                    rectTransform.anchoredPosition = initialPosition - new Vector2(0, slideOffset);

                float revealElapsed = 0;
                while (revealElapsed < fadeDuration)
                {
                    revealElapsed += Time.deltaTime;
                    float t = revealElapsed / fadeDuration;
                    float easedT = 1f - Mathf.Pow(1f - t, 3f); // OutCubic
                    
                    canvasGroup.alpha = t;
                    if (rectTransform != null)
                        rectTransform.anchoredPosition = Vector2.Lerp(initialPosition - new Vector2(0, slideOffset), initialPosition, easedT);
                    
                    yield return null;
                }

                // 3. Typewriter Effect
                tipText.text = "";
                for (int i = 0; i <= fullText.Length; i++)
                {
                    string currentDisplay = fullText.Substring(0, i);
                    if (useCursor && i < fullText.Length)
                    {
                        tipText.text = currentDisplay + cursorChar;
                    }
                    else
                    {
                        tipText.text = currentDisplay;
                    }

                    if (isSkipping) break; // Skip typing
                    yield return new WaitForSeconds(typeSpeed);
                }
                
                tipText.text = fullText; // Ensure full text is shown

                // 4. Progress Bar & Wait
                float waitElapsed = 0;
                if (progressBar != null) progressBar.fillAmount = 0;

                while (waitElapsed < displayDuration)
                {
                    if (isSkipping) break;
                    
                    waitElapsed += Time.deltaTime;
                    if (progressBar != null)
                        progressBar.fillAmount = waitElapsed / displayDuration;
                    
                    yield return null;
                }

                if (progressBar != null) progressBar.fillAmount = 1f;

                // 5. Fade Out
                float fadeOutElapsed = 0;
                while (fadeOutElapsed < fadeDuration)
                {
                    fadeOutElapsed += Time.deltaTime;
                    canvasGroup.alpha = 1 - (fadeOutElapsed / fadeDuration);
                    yield return null;
                }
                canvasGroup.alpha = 0;

                // 6. Next Tip
                currentTipIndex = (currentTipIndex + 1) % shuffledKeys.Count;
                if (currentTipIndex == 0) ShuffleTips();
                
                yield return new WaitForSeconds(0.4f);
            }
        }
    }
}
