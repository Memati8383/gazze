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
        [Tooltip("Muzik ses seviyesini ayarlayan slider.")]
        public Slider musicSlider;
        /// <summary> Müziği açıp kapatan toggle. </summary>
        [Tooltip("Muzigi acip kapatan toggle.")]
        public Toggle musicToggle;

        [Header("SFX Ayarları")]
        /// <summary> SFX ses seviyesini ayarlayan slider. </summary>
        [Tooltip("SFX ses seviyesini ayarlayan slider.")]
        public Slider sfxSlider;
        /// <summary> SFX'i açıp kapatan toggle. </summary>
        [Tooltip("SFX'i acip kapatan toggle.")]
        public Toggle sfxToggle;

        [Header("Dil Ayarları")]
        /// <summary> Dil seçimi dropdown menüsü. </summary>
        [Tooltip("Oyun dili secimi icin dropdown.")]
        public TMP_Dropdown languageDropdown;

        [Header("Titreşim Ayarları")]
        /// <summary> Titreşimi (Haptic) açıp kapatan toggle. </summary>
        [Tooltip("Haptic geri bildirimi acip kapatan toggle.")]
        public Toggle hapticToggle;

        [Header("Kontrol Ayarları")]
        /// <summary> Kontrol yöntemi dropdownı. </summary>
        public TMP_Dropdown controlMethodDropdown;
        /// <summary> Hızlanma modu dropdownı. </summary>
        public TMP_Dropdown accelerationModeDropdown;
        /// <summary> Kontrol hassasiyeti sliderı. </summary>
        public Slider controlSensitivitySlider;
        /// <summary> İvmeölçer kalibrasyon butonu. </summary>
        public Button calibrateButton;
        /// <summary> Kalibrasyon butonunun içinde bulunduğu panel (Tilt seçildiğinde görünür). </summary>
        public GameObject calibrationPanel;

        [Header("Navigasyon")]
        /// <summary> Ayarlar panelinden çıkış butonu. </summary>
        [Tooltip("Ayarlar panelini kapatan geri butonu.")]
        public Button backButton;
        /// <summary> Tüm ilerlemeyi sıfırlayan buton. </summary>
        [Tooltip("Tum oyun ilerlemesini sifirlama butonu.")]
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
        /// <summary> Kontrol yöntemi değiştiğinde tetiklenen olay. </summary>
        public event Action<int> OnControlMethodChanged;
        /// <summary> Hızlanma modu değiştiğinde tetiklenen olay. </summary>
        public event Action<int> OnAccelerationModeChanged;
        /// <summary> Kontrol hassasiyeti değiştiğinde tetiklenen olay. </summary>
        public event Action<float> OnControlSensitivityChanged;
        /// <summary> Kalibrasyon butonuna tıklandığında tetiklenen olay. </summary>
        public event Action OnCalibrateClicked;
        

        private void Start()
        {
            InitializeListeners();
        }

        /// <summary>
        /// UI bileşenlerini dinlemeye başlar. Referanslar değiştiğinde tekrar çağrılmalıdır.
        /// </summary>
        /// <summary>
        /// UI bileşenlerini dinlemeye başlar. Referanslar değiştiğinde tekrar çağrılmalıdır.
        /// </summary>
        public void InitializeListeners()
        {
            // Not: RemoveAllListeners() kullanmıyoruz çünkü VisualOverhaul tarafından eklenen 
            // dahili animasyon ve metin güncelleme dinleyicilerini (listeners) siler.
            // Bunun yerine her zaman tek bir wrapper listener ekliyoruz.

            if (musicSlider != null) 
            {
                musicSlider.onValueChanged.RemoveListener(OnMusicSliderInput);
                musicSlider.onValueChanged.AddListener(OnMusicSliderInput);
            }
            if (musicToggle != null) 
            {
                musicToggle.onValueChanged.RemoveListener(OnMusicToggleInput);
                musicToggle.onValueChanged.AddListener(OnMusicToggleInput);
            }
            if (sfxSlider != null) 
            {
                sfxSlider.onValueChanged.RemoveListener(OnSFXSliderInput);
                sfxSlider.onValueChanged.AddListener(OnSFXSliderInput);
            }
            if (sfxToggle != null) 
            {
                sfxToggle.onValueChanged.RemoveListener(OnSFXToggleInput);
                sfxToggle.onValueChanged.AddListener(OnSFXToggleInput);
            }
            if (languageDropdown != null) 
            {
                languageDropdown.onValueChanged.RemoveListener(OnLanguageInput);
                languageDropdown.onValueChanged.AddListener(OnLanguageInput);
            }
            if (hapticToggle != null) 
            {
                hapticToggle.onValueChanged.RemoveListener(OnHapticInput);
                hapticToggle.onValueChanged.AddListener(OnHapticInput);
            }
            if (backButton != null) 
            {
                backButton.onClick.RemoveListener(OnBackInput);
                backButton.onClick.AddListener(OnBackInput);
            }
            if (resetProgressButton != null) 
            {
                resetProgressButton.onClick.RemoveListener(OnResetInput);
                resetProgressButton.onClick.AddListener(OnResetInput);
            }
            if (controlMethodDropdown != null)
            {
                controlMethodDropdown.onValueChanged.RemoveListener(OnControlMethodInput);
                controlMethodDropdown.onValueChanged.AddListener(OnControlMethodInput);
            }
            if (accelerationModeDropdown != null)
            {
                accelerationModeDropdown.onValueChanged.RemoveListener(OnAccelerationModeInput);
                accelerationModeDropdown.onValueChanged.AddListener(OnAccelerationModeInput);
            }
            if (controlSensitivitySlider != null)
            {
                controlSensitivitySlider.onValueChanged.RemoveListener(OnControlSensitivityInput);
                controlSensitivitySlider.onValueChanged.AddListener(OnControlSensitivityInput);
            }
            if (calibrateButton != null)
            {
                calibrateButton.onClick.RemoveListener(OnCalibrateInput);
                calibrateButton.onClick.AddListener(OnCalibrateInput);
            }
        }

        private void OnMusicSliderInput(float v) => OnMusicVolumeChanged?.Invoke(v);
        private void OnMusicToggleInput(bool v) { PlayClick(); OnMusicToggleChanged?.Invoke(v); }
        private void OnSFXSliderInput(float v) => OnSFXVolumeChanged?.Invoke(v);
        private void OnSFXToggleInput(bool v) { PlayClick(); OnSFXToggleChanged?.Invoke(v); }
        private void OnLanguageInput(int v) { PlayClick(); OnLanguageChanged?.Invoke(v); languageDropdown.RefreshShownValue(); }
        private void OnHapticInput(bool v) { PlayClick(); OnHapticToggleChanged?.Invoke(v); }
        private void OnBackInput() { PlayClick(); OnBackButtonClicked?.Invoke(); }
        private void OnResetInput() { PlayClick(); OnResetProgressClicked?.Invoke(); }
        private void OnControlMethodInput(int v) { PlayClick(); OnControlMethodChanged?.Invoke(v); ToggleCalibrationPanel(v == 1); }
        private void OnAccelerationModeInput(int v) { PlayClick(); OnAccelerationModeChanged?.Invoke(v); }
        private void OnControlSensitivityInput(float v) => OnControlSensitivityChanged?.Invoke(v);
        private void OnCalibrateInput() { PlayClick(); OnCalibrateClicked?.Invoke(); }

        private void ToggleCalibrationPanel(bool visible)
        {
            if (calibrationPanel != null) calibrationPanel.SetActive(visible);
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

            // UI'ı varsayılan değerlere döndür
            UpdateUI(1f, true, 1f, true, 0, true, 0, 0, 0.5f);
            
            // Tüm sistemlere ayarların resetlendiğini bildir (PlayerController vb.)
            SettingsModel.NotifyChanges();
        }
            
        /// <summary> Sahneyi yeniden yükle ki değişiklikler hemen görünsün </summary>
        public void ResetScene()
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
        }

        /// <summary> Dil seçimini manuel tetikler (Dropdown yerine butonlar için). </summary>
        public void TriggerLanguageChange(int index)
        {
            OnLanguageChanged?.Invoke(index);
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
        /// <param name="hapticOn">Haptic açık mı?</param>
        /// <param name="controlIndex">Kontrol yöntemi.</param>
        /// <param name="accelIndex">Hızlanma modu.</param>
        /// <param name="sensitivity">Hassasiyet.</param>
        public void UpdateUI(float musicVol, bool musicOn, float sfxVol, bool sfxOn, int langIndex, bool hapticOn, int controlIndex, int accelIndex, float sensitivity)
        {
            if (musicSlider != null) musicSlider.SetValueWithoutNotify(musicVol);
            if (musicToggle != null)
            {
                musicToggle.SetIsOnWithoutNotify(musicOn);
                var anim = musicToggle.GetComponent<SettingsSmoothToggleAnimator>();
                if (anim != null) anim.SetState(musicOn, true);
            }
            if (sfxSlider != null) sfxSlider.SetValueWithoutNotify(sfxVol);
            if (sfxToggle != null)
            {
                sfxToggle.SetIsOnWithoutNotify(sfxOn);
                var anim = sfxToggle.GetComponent<SettingsSmoothToggleAnimator>();
                if (anim != null) anim.SetState(sfxOn, true);
            }
            if (languageDropdown != null)
            {
                languageDropdown.SetValueWithoutNotify(langIndex);
                languageDropdown.RefreshShownValue();
            }
            if (hapticToggle != null)
            {
                hapticToggle.SetIsOnWithoutNotify(hapticOn);
                var anim = hapticToggle.GetComponent<SettingsSmoothToggleAnimator>();
                if (anim != null) anim.SetState(hapticOn, true);
            }

            // Yeni Dil Butonlarını Güncelle
            var adapter = GetComponentInChildren<LanguageSwitcherAdapter>();
            if (adapter != null) adapter.UpdateVisuals(langIndex);

            // Yeni Kontrol Ayarlarını Güncelle
            if (controlMethodDropdown != null) controlMethodDropdown.SetValueWithoutNotify(controlIndex);
            if (accelerationModeDropdown != null) accelerationModeDropdown.SetValueWithoutNotify(accelIndex);
            if (controlSensitivitySlider != null) controlSensitivitySlider.SetValueWithoutNotify(sensitivity);
            ToggleCalibrationPanel(controlIndex == 1);
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