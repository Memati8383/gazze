using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

namespace Gazze.UI
{
    public class CountdownManager : MonoBehaviour
    {
        public static CountdownManager Instance { get; private set; }

        /// <summary> Geri sayım tamamlandığında true olur. </summary>
        public bool IsGameStarted { get; private set; } = false;

        [Header("─── UI References ───")]
        [Tooltip("Geri sayım metnini gösteren TextMeshPro")]
        public TextMeshProUGUI countdownText;

        [Tooltip("Geri sayım arka plan overlay (CanvasGroup)")]
        public CanvasGroup overlayGroup;

        [Tooltip("Alt bilgi metni (HAZIR OL! vs.)")]
        public TextMeshProUGUI subtitleText;

        [Tooltip("Radial progress ring (Image, Filled)")]
        public Image radialRing;

        [Tooltip("Shockwave halka efekti (Image)")]
        public Image shockwaveRing;

        [Tooltip("Geri sayım bitişindeki parçacık efekti")]
        public ParticleSystem goParticles;

        [Header("─── Zamanlama ───")]
        public float introDuration = 1.0f;
        public float numberDuration = 0.85f;
        public float goDuration = 0.7f;
        public float outroFadeDuration = 0.4f;
        public bool autoStart = true;

        [Header("─── Scale Animasyon ───")]
        public float numberStartScale = 3.0f;
        public float numberEndScale = 0.9f;
        public float goStartScale = 0.3f;
        public float goPeakScale = 1.4f;
        public float goEndScale = 1.0f;

        [Header("─── Ses Efektleri ───")]
        [Tooltip("Her sayı değişiminde oynatılacak tick sesi")]
        public AudioClip tickSound;
        [Tooltip("BAŞLA! anında oynatılacak ses")]
        public AudioClip goSound;

        [Header("─── Kamera Efektleri ───")]
        [Tooltip("Geri sayımda kamera FOV değişimi miktarı")]
        public float cameraZoomAmount = 5f;
        [Tooltip("BAŞLA! anındaki kamera punch zoom")]
        public float goPunchZoom = 8f;

        // ── Renk Paleti ──
        private readonly Color color3 = new Color(1f, 0.30f, 0.30f, 1f);   // Kırmızı
        private readonly Color color2 = new Color(1f, 0.72f, 0.15f, 1f);   // Turuncu
        private readonly Color color1 = new Color(0.15f, 0.92f, 0.40f, 1f); // Yeşil
        private readonly Color colorGo = new Color(0.0f, 1f, 0.55f, 1f);   // Neon yeşil
        private readonly Color ringBg = new Color(1f, 1f, 1f, 0.08f);
        private readonly Color subtitleColor = new Color(1f, 1f, 1f, 0.6f);

        // ── Cache ──
        private RectTransform textRect;
        private RectTransform shockwaveRect;
        private Vector3 baseScale;
        private float baseFontSize;
        private Camera mainCam;
        private float originalFOV;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        private void Start()
        {
            IsGameStarted = false;
            mainCam = Camera.main;
            if (mainCam != null) originalFOV = mainCam.fieldOfView;

            if (countdownText != null)
            {
                textRect = countdownText.GetComponent<RectTransform>();
                baseScale = textRect.localScale;
                baseFontSize = countdownText.fontSize;
            }

            if (shockwaveRing != null)
            {
                shockwaveRect = shockwaveRing.GetComponent<RectTransform>();
                shockwaveRing.gameObject.SetActive(false);
            }

            // Radial ring başlangıç
            if (radialRing != null)
            {
                radialRing.type = Image.Type.Filled;
                radialRing.fillMethod = Image.FillMethod.Radial360;
                radialRing.fillOrigin = (int)Image.Origin360.Top;
                radialRing.fillClockwise = false;
                radialRing.fillAmount = 0f;
                radialRing.color = ringBg;
            }

            // Subtitle gizle
            if (subtitleText != null)
            {
                subtitleText.text = "";
                subtitleText.color = subtitleColor;
            }

            if (autoStart)
            {
                StartCoroutine(PremiumCountdownSequence());
            }
            else
            {
                // Ensure initial state is ready for manual start
                if (overlayGroup != null) overlayGroup.alpha = 0f;
                if (countdownText != null) countdownText.text = "";
            }
        }

        public void StartCountdown()
        {
            StopAllCoroutines();
            StartCoroutine(PremiumCountdownSequence());
        }

        public static void TriggerResumeCountdown(System.Action onFinished)
        {
            if (Instance == null)
            {
                onFinished?.Invoke();
                return;
            }
            Instance.StopAllCoroutines();
            Instance.StartCoroutine(Instance.ResumeCountdownSequence(onFinished));
        }

        private IEnumerator ResumeCountdownSequence(System.Action onFinished)
        {
            // ── Pre-setup ──
            if (overlayGroup != null)
            {
                overlayGroup.gameObject.SetActive(true);
                yield return StartCoroutine(FadeOverlay(0f, 0.4f, 0.15f));
            }

            if (countdownText != null)
            {
                countdownText.gameObject.SetActive(true);
                countdownText.text = "";
            }

            // ── 3-2-1 ──
            yield return StartCoroutine(AnimateNumber("3", color3, 0));
            yield return StartCoroutine(AnimateNumber("2", color2, 1));
            yield return StartCoroutine(AnimateNumber("1", color1, 2));

            // ── GO! ──
            yield return StartCoroutine(AnimateGo());

            // ── Cleanup ──
            if (countdownText != null) countdownText.gameObject.SetActive(false);
            if (subtitleText != null) subtitleText.gameObject.SetActive(false);
            if (radialRing != null) radialRing.gameObject.SetActive(false);

            if (overlayGroup != null)
            {
                StartCoroutine(FadeOverlay(0.4f, 0f, 0.3f));
            }

            // ── Resume Game ──
            IsGameStarted = true; // Oyunun başladığını garanti et
            
            // Callback'i çağır (PauseMenuBuilder burada Time.timeScale = 1 yapacak)
            onFinished?.Invoke();

            // Arka planı tamamen temizlemek için bekle
            yield return new WaitForSecondsRealtime(0.35f);
            if (overlayGroup != null) overlayGroup.gameObject.SetActive(false);
        }

        private IEnumerator PremiumCountdownSequence()
        {
            // ── Overlay fade in ──
            if (overlayGroup != null)
            {
                overlayGroup.gameObject.SetActive(true);
                yield return StartCoroutine(FadeOverlay(0f, 0.65f, 0.3f));
            }

            if (countdownText != null) countdownText.gameObject.SetActive(true);

            // ── INTRO: "HAZIR OL!" ──
            yield return StartCoroutine(AnimateIntro());

            // ── Kamera zoom in ──
            if (mainCam != null)
                StartCoroutine(SmoothCameraFOV(originalFOV - cameraZoomAmount, 2.5f));

            // ── 3 ──
            yield return StartCoroutine(AnimateNumber("3", color3, 0));

            // ── 2 ──
            yield return StartCoroutine(AnimateNumber("2", color2, 1));

            // ── 1 ──
            yield return StartCoroutine(AnimateNumber("1", color1, 2));

            // ── BAŞLA! ──
            yield return StartCoroutine(AnimateGo());

            // ── Oyun başladı ──
            IsGameStarted = true;

            // ── Kamera punch zoom out ──
            if (mainCam != null)
                StartCoroutine(PunchCameraFOV(originalFOV + goPunchZoom, originalFOV, 0.5f));

            // ── Temizlik ──
            if (countdownText != null) countdownText.gameObject.SetActive(false);
            if (subtitleText != null) subtitleText.gameObject.SetActive(false);
            if (radialRing != null) radialRing.gameObject.SetActive(false);

            if (overlayGroup != null)
            {
                yield return StartCoroutine(FadeOverlay(0.65f, 0f, outroFadeDuration));
                overlayGroup.gameObject.SetActive(false);
            }
        }

        private IEnumerator AnimateIntro()
        {
            if (subtitleText == null) yield break;

            string readyText = LocalizationManager.Instance != null
                ? LocalizationManager.Instance.GetTranslation("Countdown_Ready")
                : "HAZIR OL!";

            subtitleText.gameObject.SetActive(true);
            subtitleText.text = readyText;
            subtitleText.fontSize = baseFontSize * 0.35f;

            // Metin gizle
            if (countdownText != null) countdownText.text = "";

            RectTransform subRect = subtitleText.GetComponent<RectTransform>();
            float elapsed = 0f;

            while (elapsed < introDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = elapsed / introDuration;

                // Slide up + fade in (ilk yarı)
                if (t < 0.4f)
                {
                    float easeT = t / 0.4f;
                    easeT = easeT * easeT * (3f - 2f * easeT); // SmoothStep
                    float yOff = Mathf.Lerp(-30f, 0f, easeT);
                    subRect.anchoredPosition = new Vector2(0, yOff - 80f);
                    subtitleText.color = new Color(subtitleColor.r, subtitleColor.g, subtitleColor.b, easeT * 0.8f);
                }
                // Sabit kal
                else if (t < 0.7f)
                {
                    subRect.anchoredPosition = new Vector2(0, -80f);
                    subtitleText.color = new Color(subtitleColor.r, subtitleColor.g, subtitleColor.b, 0.8f);
                }
                // Fade out
                else
                {
                    float fadeT = (t - 0.7f) / 0.3f;
                    subtitleText.color = new Color(subtitleColor.r, subtitleColor.g, subtitleColor.b, Mathf.Lerp(0.8f, 0f, fadeT));
                }

                yield return null;
            }

            subtitleText.color = new Color(1, 1, 1, 0);
        }

        private IEnumerator AnimateNumber(string text, Color numColor, int index)
        {
            if (countdownText == null) yield break;

            PlayTickSound();
            Settings.HapticManager.Medium();

            StartCoroutine(PlayShockwave(numColor));

            if (radialRing != null)
            {
                radialRing.gameObject.SetActive(true);
                radialRing.fillAmount = 0f;
                radialRing.color = new Color(numColor.r, numColor.g, numColor.b, 0.25f);
            }

            if (subtitleText != null)
            {
                subtitleText.gameObject.SetActive(true);
                subtitleText.fontSize = baseFontSize * 0.22f;
                string[] subTexts = GetCountdownSubtitles();
                if (index < subTexts.Length)
                    subtitleText.text = subTexts[index];
            }

            countdownText.text = text;
            countdownText.fontSize = baseFontSize;
            float elapsed = 0f;

            while (elapsed < numberDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = elapsed / numberDuration;

                float easedT = ElasticEaseOut(t);
                float scale = Mathf.Lerp(numberStartScale, numberEndScale, easedT);
                textRect.localScale = baseScale * scale;

                float wobble = Mathf.Sin(t * Mathf.PI * 4f) * (1f - t) * 3f;
                textRect.localEulerAngles = new Vector3(0, 0, wobble);

                float glowPulse = 0.5f + 0.5f * Mathf.Sin(t * Mathf.PI * 6f);
                Color glowColor = Color.Lerp(numColor, Color.white, glowPulse * 0.3f * (1f - t));

                float alpha;
                if (t < 0.65f) alpha = 1f;
                else alpha = Mathf.Lerp(1f, 0f, (t - 0.65f) / 0.35f);

                countdownText.color = new Color(glowColor.r, glowColor.g, glowColor.b, alpha);

                if (radialRing != null)
                {
                    radialRing.fillAmount = t;
                    radialRing.color = new Color(numColor.r, numColor.g, numColor.b, 0.25f * (1f - t));
                }

                if (subtitleText != null)
                {
                    float subAlpha = t < 0.15f ? t / 0.15f : (t < 0.55f ? 1f : Mathf.Lerp(1f, 0f, (t - 0.55f) / 0.45f));
                    subtitleText.color = new Color(subtitleColor.r, subtitleColor.g, subtitleColor.b, subAlpha * 0.5f);
                }

                yield return null;
            }
            textRect.localEulerAngles = Vector3.zero;
        }

        private IEnumerator AnimateGo()
        {
            if (countdownText == null) yield break;

            PlayGoSound();
            Settings.HapticManager.Heavy();

            StartCoroutine(PlayShockwave(colorGo, 1.8f));

            if (radialRing != null) radialRing.gameObject.SetActive(false);

            string goText = LocalizationManager.Instance != null
                ? LocalizationManager.Instance.GetTranslation("Countdown_Go")
                : "BAŞLA!";

            countdownText.text = goText;
            countdownText.fontSize = baseFontSize * 1.15f;

            if (subtitleText != null)
            {
                subtitleText.gameObject.SetActive(true);
                subtitleText.fontSize = baseFontSize * 0.2f;
                string goSub = LocalizationManager.Instance != null
                    ? LocalizationManager.Instance.GetTranslation("Countdown_GoSub")
                    : "İLERİ!";
                subtitleText.text = goSub;
            }

            StartCoroutine(OverlayFlash());

            float elapsed = 0f;
            while (elapsed < goDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = elapsed / goDuration;

                float scale;
                if (t < 0.25f) scale = Mathf.Lerp(goStartScale, goPeakScale, (t / 0.25f) * (t / 0.25f));
                else if (t < 0.45f) scale = Mathf.Lerp(goPeakScale, goEndScale * 0.85f, (t - 0.25f) / 0.2f);
                else if (t < 0.55f) scale = Mathf.Lerp(goEndScale * 0.85f, goEndScale * 1.05f, (t - 0.45f) / 0.1f);
                else scale = Mathf.Lerp(goEndScale * 1.05f, goEndScale, (t - 0.55f) / 0.45f);
                
                textRect.localScale = baseScale * scale;

                float glow = 0.5f + 0.5f * Mathf.Sin(t * Mathf.PI * 8f);
                Color c = Color.Lerp(colorGo, Color.white, glow * 0.4f * (1f - t));

                float alpha = t < 0.6f ? 1f : Mathf.Lerp(1f, 0f, (t - 0.6f) / 0.4f);
                countdownText.color = new Color(c.r, c.g, c.b, alpha);

                if (subtitleText != null)
                {
                    float subA = t < 0.2f ? t / 0.2f : (t < 0.5f ? 1f : Mathf.Lerp(1f, 0f, (t - 0.5f) / 0.5f));
                    subtitleText.color = new Color(colorGo.r, colorGo.g, colorGo.b, subA * 0.5f);
                }

                yield return null;
            }
            countdownText.fontSize = baseFontSize;
            textRect.localEulerAngles = Vector3.zero;
        }

        private IEnumerator PlayShockwave(Color color, float scaleMultiplier = 1f)
        {
            if (shockwaveRing == null) yield break;
            shockwaveRing.gameObject.SetActive(true);
            shockwaveRing.color = new Color(color.r, color.g, color.b, 0.6f);
            shockwaveRect.localScale = Vector3.one * 0.3f;
            float duration = 0.5f, elapsed = 0f;
            Vector3 targetScale = Vector3.one * 4f * scaleMultiplier;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = elapsed / duration;
                float easeT = 1f - Mathf.Pow(1f - t, 2f);
                shockwaveRect.localScale = Vector3.Lerp(Vector3.one * 0.3f, targetScale, easeT);
                shockwaveRing.color = new Color(color.r, color.g, color.b, Mathf.Lerp(0.6f, 0f, t));
                yield return null;
            }
            shockwaveRing.gameObject.SetActive(false);
        }

        private IEnumerator OverlayFlash()
        {
            if (overlayGroup == null) yield break;
            float original = overlayGroup.alpha, flashPeak = Mathf.Min(original + 0.3f, 1f), elapsed = 0f, dur = 0.08f;
            while (elapsed < dur) { elapsed += Time.unscaledDeltaTime; overlayGroup.alpha = Mathf.Lerp(original, flashPeak, elapsed / dur); yield return null; }
            elapsed = 0f; dur = 0.25f;
            while (elapsed < dur) { elapsed += Time.unscaledDeltaTime; overlayGroup.alpha = Mathf.Lerp(flashPeak, original, elapsed / dur); yield return null; }
            overlayGroup.alpha = original;
        }

        private IEnumerator SmoothCameraFOV(float targetFOV, float duration)
        {
            if (mainCam == null) yield break;
            float startFOV = mainCam.fieldOfView, elapsed = 0f;
            while (elapsed < duration) { elapsed += Time.unscaledDeltaTime; float t = elapsed / duration; t = t * t * (3f - 2f * t); mainCam.fieldOfView = Mathf.Lerp(startFOV, targetFOV, t); yield return null; }
            mainCam.fieldOfView = targetFOV;
        }

        private IEnumerator PunchCameraFOV(float punchFOV, float returnFOV, float duration)
        {
            if (mainCam == null) yield break;
            float elapsed = 0f, half = duration * 0.3f, startFOV = mainCam.fieldOfView;
            while (elapsed < half) { elapsed += Time.unscaledDeltaTime; mainCam.fieldOfView = Mathf.Lerp(startFOV, punchFOV, (elapsed / half) * (elapsed / half)); yield return null; }
            elapsed = 0f; float remaining = duration - half;
            while (elapsed < remaining) { elapsed += Time.unscaledDeltaTime; float t = elapsed / remaining; mainCam.fieldOfView = Mathf.Lerp(punchFOV, returnFOV, 1f - Mathf.Pow(1f - t, 3f)); yield return null; }
            mainCam.fieldOfView = returnFOV;
        }

        private void PlayTickSound() { if (tickSound != null && Settings.AudioManager.Instance != null) Settings.AudioManager.Instance.PlaySFX(tickSound); }
        private void PlayGoSound() { if (goSound != null && Settings.AudioManager.Instance != null) Settings.AudioManager.Instance.PlaySFX(goSound); }

        private IEnumerator FadeOverlay(float from, float to, float duration)
        {
            if (overlayGroup == null) yield break;
            float elapsed = 0f;
            while (elapsed < duration) { elapsed += Time.unscaledDeltaTime; overlayGroup.alpha = Mathf.Lerp(from, to, elapsed / duration); yield return null; }
            overlayGroup.alpha = to;
        }

        private float ElasticEaseOut(float t) { if (t <= 0f) return 0f; if (t >= 1f) return 1f; float p = 0.4f, s = p / 4f; return Mathf.Pow(2f, -10f * t) * Mathf.Sin((t - s) * (2f * Mathf.PI) / p) + 1f; }

        private string[] GetCountdownSubtitles()
        {
            if (LocalizationManager.Instance != null)
                return new string[] { LocalizationManager.Instance.GetTranslation("Countdown_Sub3"), LocalizationManager.Instance.GetTranslation("Countdown_Sub2"), LocalizationManager.Instance.GetTranslation("Countdown_Sub1") };
            return new string[] { "HAZIRLAN...", "DİKKAT...", "ŞİMDİ!" };
        }
    }
}
