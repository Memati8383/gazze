/**
 * @file SettingsView.cs
 * @author Unity MCP Assistant
 * @date 2026-02-28
 * @last_update 2026-02-28
 * @description Ayarlar panelinin kullanıcı arayüzü (UI) bileşenlerini yöneten ve kullanıcı etkileşimlerini Controller'a ileten View sınıfıdır.
 */

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections;

namespace Settings
{
    /// <summary>
    /// Ayarlar ekranındaki UI elemanlarını kontrol eden ve olayları (events) tetikleyen sınıf.
    /// </summary>
    public class SettingsView : MonoBehaviour
    {
        [Header("Müzik Ayarları")]
        /// <summary> Müzik ses seviyesini ayarlayan slider. </summary>
        public Slider musicSlider;
        /// <summary> Müziği açıp kapatan toggle. </summary>
        public Toggle musicToggle;

        [Header("SFX Ayarları")]
        /// <summary> SFX ses seviyesini ayarlayan slider. </summary>
        public Slider sfxSlider;
        /// <summary> SFX'i açıp kapatan toggle. </summary>
        public Toggle sfxToggle;

        [Header("Dil Ayarları")]
        /// <summary> Dil seçimi dropdown menüsü. </summary>
        public TMP_Dropdown languageDropdown;

        [Header("Titreşim Ayarları")]
        /// <summary> Titreşimi (Haptic) açıp kapatan toggle. </summary>
        public Toggle hapticToggle;

        [Header("Navigasyon")]
        /// <summary> Ayarlar panelinden çıkış butonu. </summary>
        public Button backButton;
        /// <summary> Tüm ilerlemeyi sıfırlayan buton. </summary>
        public Button resetProgressButton;

        /// <summary> Müzik sesi değiştiğinde tetiklenen olay. </summary>
        public event Action<float> OnMusicVolumeChanged;
        /// <summary> Müzik durumu (açık/kapalı) değiştiğinde tetiklenen olay. </summary>
        public event Action<bool> OnMusicToggleChanged;
        /// <summary> SFX sesi değiştiğinde tetiklenen olay. </summary>
        public event Action<float> OnSFXVolumeChanged;
        /// <summary> SFX durumu (açık/kapalı) değiştiğinde tetiklenen olay. </summary>
        public event Action<bool> OnSFXToggleChanged;
        /// <summary> Dil değiştiğinde tetiklenen olay. </summary>
        public event Action<int> OnLanguageChanged;
        /// <summary> Haptic durumu (açık/kapalı) değiştiğinde tetiklenen olay. </summary>
        public event Action<bool> OnHapticToggleChanged;
        /// <summary> Geri butonuna tıklandığında tetiklenen olay. </summary>
        public event Action OnBackButtonClicked;
        /// <summary> İlerlemeyi sıfırla butonuna tıklandığında tetiklenen olay. </summary>
        public event Action OnResetProgressClicked;

        private void Start()
        {            // UI bileşenlerinin olaylarını C# olaylarına (events) bağla
            if (musicSlider != null) musicSlider.onValueChanged.AddListener(val => { OnMusicVolumeChanged?.Invoke(val); });
            if (musicToggle != null) musicToggle.onValueChanged.AddListener(val => { PlayClick(); OnMusicToggleChanged?.Invoke(val); });
            if (sfxSlider != null) sfxSlider.onValueChanged.AddListener(val => { OnSFXVolumeChanged?.Invoke(val); });
            if (sfxToggle != null) sfxToggle.onValueChanged.AddListener(val => { PlayClick(); OnSFXToggleChanged?.Invoke(val); });
            if (languageDropdown != null) languageDropdown.onValueChanged.AddListener(val => { PlayClick(); OnLanguageChanged?.Invoke(val); });
            if (hapticToggle != null) hapticToggle.onValueChanged.AddListener(val => { PlayClick(); OnHapticToggleChanged?.Invoke(val); });
            if (backButton != null) backButton.onClick.AddListener(() => { PlayClick(); OnBackButtonClicked?.Invoke(); });
            if (resetProgressButton != null) resetProgressButton.onClick.AddListener(() => { PlayClick(); OnResetProgressClicked?.Invoke(); });
        }

        /// <summary>
        /// Tüm oyun ilerlemesini (para, skor, kilitli araçlar) sıfırlar.
        /// </summary>
        public void ResetAllProgress()
        {            
            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();
            Debug.Log("Tüm oyun verileri sıfırlandı!");
            
            // Singleton yöneticilerine varsayılan ayarları yüklemelerini söyle
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.ApplyInitialSettings();
            }
            if (Gazze.UI.LocalizationManager.Instance != null)
            {
                Gazze.UI.LocalizationManager.Instance.LoadSavedLanguage();
            }
            
            // Sahneyi yeniden yükle ki değişiklikler hemen görünsün
            UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
        }

        private void PlayClick()
        {
            if (AudioManager.Instance != null) AudioManager.Instance.PlayClickSound();
        }

        /// <summary>
        /// UI bileşenlerini verilen değerlerle günceller (Olay tetiklemeden).
        /// </summary>
        /// <param name="musicVol">Müzik sesi.</param>
        /// <param name="musicOn">Müzik açık mı?</param>
        /// <param name="sfxVol">SFX sesi.</param>
        /// <param name="sfxOn">SFX açık mı?</param>
        /// <param name="langIndex">Dil indeksi.</param>
        /// <param name="langIndex">Dil indeksi.</param>
        /// <param name="hapticOn">Haptic açık mı?</param>
        public void UpdateUI(float musicVol, bool musicOn, float sfxVol, bool sfxOn, int langIndex, bool hapticOn)
        {
            if (musicSlider != null) musicSlider.SetValueWithoutNotify(musicVol);
            if (musicToggle != null) musicToggle.SetIsOnWithoutNotify(musicOn);
            if (sfxSlider != null) sfxSlider.SetValueWithoutNotify(sfxVol);
            if (sfxToggle != null) sfxToggle.SetIsOnWithoutNotify(sfxOn);
            if (languageDropdown != null) languageDropdown.SetValueWithoutNotify(langIndex);
            if (hapticToggle != null) hapticToggle.SetIsOnWithoutNotify(hapticOn);
        }

        private CanvasGroup canvasGroup;
        // visibilityCoroutine unused field removed

        private void Awake()
        {
            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        private void OnEnable()
        {
            // Reset for animation if enabled manually
            transform.localScale = Vector3.one * 0.8f;
            if (canvasGroup != null) canvasGroup.alpha = 0;
            StartCoroutine(AnimateVisibility(true));
        }

        /// <summary> Ayarlar panelini yumuşak bir animasyonla görünür yapar. </summary>
        public void Show() 
        {
            gameObject.SetActive(true);
        }

        /// <summary> Ayarlar panelini yumuşak bir animasyonla gizler. </summary>
        public void Hide() 
        {
            if (gameObject.activeInHierarchy)
                StartCoroutine(AnimateVisibility(false));
        }

        private IEnumerator AnimateVisibility(bool visible)
        {
            float targetAlpha = visible ? 1f : 0f;
            Vector3 targetScale = visible ? Vector3.one : Vector3.one * 0.8f;
            float duration = 0.25f;
            float elapsed = 0f;

            float startAlpha = canvasGroup.alpha;
            Vector3 startScale = transform.localScale;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = elapsed / duration;
                t = Mathf.Sin(t * Mathf.PI * 0.5f); // Ease out circle-ish

                canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, t);
                transform.localScale = Vector3.Lerp(startScale, targetScale, t);
                yield return null;
            }

            canvasGroup.alpha = targetAlpha;
            transform.localScale = targetScale;

            if (!visible) gameObject.SetActive(false);
            // visibilityCoroutine = null;
        }
    }
}