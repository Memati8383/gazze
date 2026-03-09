/**
 * @file SettingsController.cs
 * @author Unity MCP Assistant
 * @date 2026-02-28
 * @last_update 2026-02-28
 * @description Model ve View arasındaki iletişimi yöneten, ayarların oyun dünyasına uygulanmasını sağlayan Controller sınıfıdır.
 */

using UnityEngine;

namespace Settings
{
    /// <summary>
    /// Ayarlar mantığını kontrol eden, UI olaylarını dinleyen ve oyun ayarlarını güncelleyen sınıf.
    /// </summary>
    public class SettingsController : MonoBehaviour
    {
        [Header("Görünüm Referansı")]
        /// <summary> Ayarlar ekranının View bileşeni. </summary>
        public SettingsView view;
        
        private SettingsModel model;

        private void Awake()
        {
            try
            {
                // Model'i oluştur ve View referansını kontrol et
                model = new();
                if (view == null) view = GetComponent<SettingsView>();
                
                if (view != null)
                {
                    InitializeView();
                }
                else
                {
                    // Debug.LogError("SettingsController: SettingsView bileşeni eksik!");
                }
            }
            catch (System.Exception)
            {
               // Debug.LogError($"SettingsController: Başlatma hatası! {e.Message}");
            }
        }

        /// <summary>
        /// View bileşenini model verileriyle senkronize eder ve olayları bağlar.
        /// </summary>
        private void InitializeView()
        {
            // Model verilerini UI'ya aktar
            view.UpdateUI(model.MusicVolume, model.MusicEnabled, model.SFXVolume, model.SFXEnabled, model.LanguageIndex, model.HapticEnabled);

            // View üzerindeki olayları Controller metodlarına bağla
            view.OnMusicVolumeChanged += HandleMusicVolumeChanged;
            view.OnMusicToggleChanged += HandleMusicToggleChanged;
            view.OnSFXVolumeChanged += HandleSFXVolumeChanged;
            view.OnSFXToggleChanged += HandleSFXToggleChanged;
            view.OnLanguageChanged += HandleLanguageChanged;
            view.OnHapticToggleChanged += HandleHapticToggleChanged;
            view.OnBackButtonClicked += HandleBackButtonClicked;
            view.OnResetProgressClicked += HandleResetProgressClicked;
            
            // Başlangıç ayarlarını uygula
            ApplyAllSettings();
        }

        private void HandleResetProgressClicked()
        {
            // Kullanıcıya bir onay penceresi gösterilebilir ama burada doğrudan sıfırlıyoruz
            view.ResetAllProgress();
        }

        private void HandleMusicVolumeChanged(float val)
        {
            model.SetMusicVolume(Mathf.Clamp01(val));
            ApplyAudioSettings();
        }

        private void HandleMusicToggleChanged(bool isOn)
        {
            model.SetMusicEnabled(isOn);
            ApplyAudioSettings();
        }

        private void HandleSFXVolumeChanged(float val)
        {
            model.SetSFXVolume(Mathf.Clamp01(val));
            ApplyAudioSettings();
        }

        private void HandleSFXToggleChanged(bool isOn)
        {
            model.SetSFXEnabled(isOn);
            ApplyAudioSettings();
        }

        private void HandleLanguageChanged(int index)
        {
            model.SetLanguage(index);
            ApplyLanguageSettings();
        }

        private void HandleHapticToggleChanged(bool isOn)
        {
            model.SetHapticEnabled(isOn);
        }

        private void HandleBackButtonClicked()
        {
            view.Hide();
            var mainMenu = FindFirstObjectByType<MainMenuManager>();
            if (mainMenu != null) mainMenu.ShowMainOptions();
        }

        /// <summary>
        /// Tüm ses ve dil ayarlarını oyun sistemlerine uygular.
        /// </summary>
        private void ApplyAllSettings()
        {
            ApplyAudioSettings();
            ApplyLanguageSettings();
        }

        private void ApplyAudioSettings()
        {
            float targetMusicVol = model.MusicEnabled ? model.MusicVolume : 0;
            float targetSFXVol = model.SFXEnabled ? model.SFXVolume : 0;

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.SetMusicVolume(targetMusicVol);
                AudioManager.Instance.SetSFXVolume(targetSFXVol);
            }
        }

        private void ApplyLanguageSettings()
        {
            if (Gazze.UI.LocalizationManager.Instance != null)
            {
                Gazze.UI.Language lang = (Gazze.UI.Language)model.LanguageIndex;
                Gazze.UI.LocalizationManager.Instance.SetLanguage(lang);
            }
        }

        private void OnDestroy()
        {
            if (view != null)
            {
                view.OnMusicVolumeChanged -= HandleMusicVolumeChanged;
                view.OnMusicToggleChanged -= HandleMusicToggleChanged;
                view.OnSFXVolumeChanged -= HandleSFXVolumeChanged;
                view.OnSFXToggleChanged -= HandleSFXToggleChanged;
                view.OnLanguageChanged -= HandleLanguageChanged;
                view.OnHapticToggleChanged -= HandleHapticToggleChanged;
                view.OnBackButtonClicked -= HandleBackButtonClicked;
            }
        }
    }
}