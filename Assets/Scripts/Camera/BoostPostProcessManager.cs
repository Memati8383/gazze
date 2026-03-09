using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Gazze.VisualEffects
{
    /// <summary>
    /// Boost (hızlanma) anında Post-Processing efektlerini yöneten sınıf.
    /// URP Volume bileşeni üzerinden Motion Blur ve Chromatic Aberration efektlerini kontrol eder.
    /// </summary>
    public class BoostPostProcessManager : MonoBehaviour
    {
        [Header("Bileşenler")]
        public Volume boostVolume;
        
        [Header("Boost Efekt Ayarları")]
        [Tooltip("Boost aktifken ulaşılacak maksimum yoğunluk (Volume Weight).")]
        [Range(0, 1)]
        public float maxWeight = 1.0f;
        [Tooltip("Efektin fade-in hızı.")]
        public float fadeInSpeed = 5f;
        [Tooltip("Efektin fade-out hızı.")]
        public float fadeOutSpeed = 3f;

        [Header("Motion Blur Ayarları")]
        [Tooltip("Hıza orantılı motion blur'un başlayacağı hız oranı (0-1).")]
        [Range(0, 1)]
        public float motionBlurSpeedThreshold = 0.25f;
        [Tooltip("Maksimum hızda motion blur şiddeti.")]
        [Range(0, 1)]
        public float maxMotionBlurIntensity = 0.45f;
        [Tooltip("Boost sırasında eklenen ekstra motion blur.")]
        [Range(0, 1)]
        public float boostMotionBlurBonus = 0.35f;
        [Tooltip("Motion blur clamp değeri (yüksek = daha belirgin çizgiler).")]
        [Range(0.05f, 0.5f)]
        public float motionBlurClamp = 0.15f;

        [Header("Chromatic Aberration Ayarları")]
        [Tooltip("Hıza göre Chromatic Aberration şiddetini artır.")]
        public bool scaleWithSpeed = true;
        [Tooltip("Chromatic aberration'ın başlayacağı hız oranı.")]
        [Range(0, 1)]
        public float chromaticSpeedThreshold = 0.5f;
        [Tooltip("Maksimum chromatic aberration şiddeti.")]
        [Range(0, 1)]
        public float maxChromaticIntensity = 0.4f;
        
        private ChromaticAberration chromaticAberration;
        private MotionBlur motionBlur;
        private bool isBoosting = false;

        public static BoostPostProcessManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);

            if (boostVolume == null)
            {
                boostVolume = GetComponent<Volume>();
            }

            // Volume Profile içindeki efektleri bul
            if (boostVolume != null && boostVolume.profile != null)
            {
                boostVolume.profile.TryGet(out chromaticAberration);
                boostVolume.profile.TryGet(out motionBlur);
            }
            
            // Başlangıçta aktif tut (weight hıza göre ayarlanacak)
            if (boostVolume != null) boostVolume.weight = 0f;
        }

        private void Update()
        {
            if (PlayerController.Instance == null) return;
            
            float currentSpeed = PlayerController.Instance.currentWorldSpeed;
            float maxSpeed = PlayerController.Instance.maxSpeed;
            float speedRatio = Mathf.Clamp01(currentSpeed / maxSpeed);

            HandleMotionBlur(speedRatio);
            HandleChromaticAberration(speedRatio);
            HandleVolumeWeight(speedRatio);
        }

        /// <summary>
        /// Hıza orantılı motion blur. Boost sırasında ekstra şiddet eklenir.
        /// </summary>
        private void HandleMotionBlur(float speedRatio)
        {
            if (motionBlur == null) return;

            // Eşik altında blur yok, eşik üstünde kademeli artış
            float normalizedSpeed = Mathf.InverseLerp(motionBlurSpeedThreshold, 1f, speedRatio);
            float intensity = normalizedSpeed * maxMotionBlurIntensity;

            // Boost aktifken ekstra blur
            if (isBoosting)
            {
                intensity += boostMotionBlurBonus;
            }

            motionBlur.intensity.value = Mathf.Clamp01(intensity);
            motionBlur.clamp.value = Mathf.Lerp(0.05f, motionBlurClamp, normalizedSpeed);
        }

        /// <summary>
        /// Yüksek hızlarda chromatic aberration efekti.
        /// </summary>
        private void HandleChromaticAberration(float speedRatio)
        {
            if (chromaticAberration == null || !scaleWithSpeed) return;

            float normalizedSpeed = Mathf.InverseLerp(chromaticSpeedThreshold, 1f, speedRatio);
            float intensity = normalizedSpeed * maxChromaticIntensity;

            if (isBoosting)
            {
                intensity = Mathf.Max(intensity, maxChromaticIntensity * 0.8f);
            }

            chromaticAberration.intensity.value = intensity;
        }

        /// <summary>
        /// Volume weight'i hıza ve boost durumuna göre ayarlar.
        /// Motion blur her zaman hıza orantılı aktif kalır.
        /// </summary>
        private void HandleVolumeWeight(float speedRatio)
        {
            if (boostVolume == null) return;

            // Hıza orantılı minimum weight + boost'ta tam weight
            float speedWeight = Mathf.Clamp01(speedRatio * 1.2f); // %80 hızda tam weight
            float targetWeight = isBoosting ? maxWeight : speedWeight;
            
            float fadeSpeed = (targetWeight > boostVolume.weight) ? fadeInSpeed : fadeOutSpeed;
            boostVolume.weight = Mathf.MoveTowards(boostVolume.weight, targetWeight, fadeSpeed * Time.deltaTime);
        }

        /// <summary>
        /// Boost efektini başlatır.
        /// </summary>
        public void StartBoostEffect()
        {
            isBoosting = true;
        }

        /// <summary>
        /// Boost efektini durdurur.
        /// </summary>
        public void StopBoostEffect()
        {
            isBoosting = false;
        }
    }
}
