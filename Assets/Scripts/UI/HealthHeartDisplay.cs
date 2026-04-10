using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

namespace Gazze.UI
{
    /// <summary>
    /// Dayaniklilik degerini kalp ikonlariyla gosteren ve gecis animasyonlarini yoneten UI bileşeni.
    /// </summary>
    public class HealthHeartDisplay : MonoBehaviour
    {
        [Header("Sprites")]
        [Tooltip("Dolu kalp icin kullanilan sprite.")]
        public Sprite fullHeartSprite;
        [Tooltip("Bos kalp icin kullanilan sprite.")]
        public Sprite emptyHeartSprite;

        [Header("Settings")]
        [Tooltip("Bir kalbin temsil ettigi can miktari.")]
        public float healthPerHeart = 100f;
        [Tooltip("Her bir kalp ikonunun boyutu.")]
        public Vector2 heartSize = new Vector2(60, 60);

        [Header("Animation Settings")]
        [Tooltip("Kalplerin giris animasyon hizi.")]
        public float entranceSpeed = 10f;
        [Tooltip("Can degisiminde uygulanan punch olcegi.")]
        public float punchIntensity = 1.3f;
        [Tooltip("Idle salinim frekansi.")]
        public float idleFloatFrequency = 1.5f;
        [Tooltip("Idle salinim miktari.")]
        public float idleFloatAmount = 2.5f;

        private List<Image> heartImages = new List<Image>();
        private List<Coroutine> punchCoroutines = new List<Coroutine>();
        private Vector3 initialScale = Vector3.one;
        private Vector2 initialAnchoredPos;
        private RectTransform rectTransform;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            initialScale = transform.localScale;
            initialAnchoredPos = rectTransform.anchoredPosition;
        }

        private void Update()
        {
            // Subtle premium idle float (Even more subtle as requested)
            float yOffset = Mathf.Sin(Time.time * idleFloatFrequency) * idleFloatAmount;
            rectTransform.anchoredPosition = initialAnchoredPos + new Vector2(0, yOffset);
        }

        public void SetupHearts(float maxHealth)
        {
            // Clear ALL existing children to prevent "unnecessary" heart buildup
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                if (Application.isPlaying) Destroy(transform.GetChild(i).gameObject);
                else DestroyImmediate(transform.GetChild(i).gameObject);
            }
            
            heartImages.Clear();
            punchCoroutines.Clear();

            // Calculate heart count: Each heart represents 100 durability
            int heartCount = Mathf.Max(1, Mathf.CeilToInt(maxHealth / healthPerHeart));
            
            for (int i = 0; i < heartCount; i++)
            {
                GameObject heartGo = new GameObject("Heart_" + i, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                heartGo.transform.SetParent(this.transform, false);
                
                var rect = heartGo.GetComponent<RectTransform>();
                rect.sizeDelta = heartSize;
                heartGo.transform.localScale = Vector3.zero;

                var img = heartGo.GetComponent<Image>();
                img.sprite = fullHeartSprite;
                img.preserveAspect = true;
                
                heartImages.Add(img);
                punchCoroutines.Add(null);
                
                var le = heartGo.AddComponent<LayoutElement>();
                le.preferredWidth = heartSize.x;
                le.preferredHeight = heartSize.y;

                if (Application.isPlaying)
                    StartCoroutine(EntranceAnimation(heartGo.transform, i * 0.1f));
                else
                    heartGo.transform.localScale = Vector3.one;
            }
        }

        private IEnumerator EntranceAnimation(Transform t, float delay)
        {
            yield return new WaitForSeconds(delay);
            float elapsed = 0;
            while (elapsed < 1f)
            {
                elapsed += Time.deltaTime * entranceSpeed;
                t.localScale = Vector3.Lerp(Vector3.zero, Vector3.one * 1.1f, elapsed);
                yield return null;
            }
            t.localScale = Vector3.one;
        }

        public void SetHealth(float currentHealth, float maxHealth)
        {
            // If lists are out of sync or empty, re-setup (Failsafe for Domain Reloads/Serialization)
            if (heartImages.Count == 0 || heartImages.Count != punchCoroutines.Count)
            {
                if (maxHealth > 0) SetupHearts(maxHealth);
                else return;
            }

            for (int i = 0; i < heartImages.Count; i++)
            {
                float threshold = (i + 1) * healthPerHeart;
                bool shouldBeFull = currentHealth >= threshold - 0.1f;
                Sprite targetSprite = shouldBeFull ? fullHeartSprite : emptyHeartSprite;

                if (i < punchCoroutines.Count && heartImages[i].sprite != targetSprite)
                {
                    heartImages[i].sprite = targetSprite;
                    if (punchCoroutines[i] != null) StopCoroutine(punchCoroutines[i]);
                    punchCoroutines[i] = StartCoroutine(PunchHeart(heartImages[i].transform));
                }

                if (shouldBeFull)
                {
                    heartImages[i].color = Color.white;
                }
                else if (currentHealth > threshold - healthPerHeart)
                {
                    float ratio = (currentHealth - (threshold - healthPerHeart)) / healthPerHeart;
                    heartImages[i].color = Color.Lerp(new Color(0.4f, 0.4f, 0.4f, 0.7f), Color.white, ratio); 
                }
                else
                {
                    heartImages[i].color = new Color(0.4f, 0.4f, 0.4f, 0.7f);
                }
            }

            // Subtle pulsing alert
            if (currentHealth <= healthPerHeart && currentHealth > 0)
            {
                float pulse = 1f + Mathf.PingPong(Time.time * 3f, 0.1f);
                transform.localScale = initialScale * pulse;
            }
            else
            {
                transform.localScale = Vector3.Lerp(transform.localScale, initialScale, Time.deltaTime * 5f);
            }
        }

        private IEnumerator PunchHeart(Transform t)
        {
            float elapsed = 0;
            Vector3 pScale = Vector3.one * punchIntensity;
            while (elapsed < 1f)
            {
                elapsed += Time.deltaTime * 10f;
                t.localScale = Vector3.Lerp(pScale, Vector3.one, elapsed);
                yield return null;
            }
            t.localScale = Vector3.one;
        }
    }
}
