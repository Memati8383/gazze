/**
 * @file SettingsModel.cs
 * @author Unity MCP Assistant
 * @date 2026-02-28
 * @last_update 2026-02-28
 * @description Ayarlar verilerinin PlayerPrefs üzerinden yönetilmesini ve kalıcılığını sağlayan Model sınıfıdır.
 */

using UnityEngine;

namespace Settings
{
    /// <summary>
    /// Ayarlar verilerini tutan ve kalıcı hafızada (PlayerPrefs) saklayan sınıf.
    /// </summary>
    public class SettingsModel
    {
        /// <summary> Müzik ses seviyesi için kullanılan PlayerPrefs anahtarı. </summary>
        public const string MusicVolumeKey = "MusicVolume";
        /// <summary> SFX ses seviyesi için kullanılan PlayerPrefs anahtarı. </summary>
        public const string SFXVolumeKey = "SFXVolume";
        /// <summary> Müzik açık/kapalı durumu için kullanılan PlayerPrefs anahtarı. </summary>
        public const string MusicEnabledKey = "MusicEnabled";
        /// <summary> SFX açık/kapalı durumu için kullanılan PlayerPrefs anahtarı. </summary>
        public const string SFXEnabledKey = "SFXEnabled";
        /// <summary> Dil seçeneği için kullanılan PlayerPrefs anahtarı. </summary>
        public const string LanguageKey = "Language";
        /// <summary> Haptic (Titreşim) açık/kapalı durumu için kullanılan PlayerPrefs anahtarı. </summary>
        public const string HapticEnabledKey = "HapticEnabled";

        /// <summary> Mevcut müzik ses seviyesi (0.0f - 1.0f). </summary>
        public float MusicVolume { get; private set; }
        /// <summary> Mevcut SFX ses seviyesi (0.0f - 1.0f). </summary>
        public float SFXVolume { get; private set; }
        /// <summary> Müzik çalma durumu. </summary>
        public bool MusicEnabled { get; private set; }
        /// <summary> SFX çalma durumu. </summary>
        public bool SFXEnabled { get; private set; }
        /// <summary> Seçili dil indeksi (0: TR, 1: EN). </summary>
        public int LanguageIndex { get; private set; }
        /// <summary> Haptic (Titreşim) durumu. </summary>
        public bool HapticEnabled { get; private set; }

        /// <summary>
        /// Yeni bir SettingsModel örneği oluşturur ve mevcut ayarları yükler.
        /// </summary>
        public SettingsModel()
        {
            Load();
        }

        /// <summary>
        /// Ayarları PlayerPrefs'ten yükler ve doğrular.
        /// </summary>
        public void Load()
        {
            try
            {
                // Değerleri oku ve 0-1 aralığına sınırla
                MusicVolume = Mathf.Clamp01(PlayerPrefs.GetFloat(MusicVolumeKey, 1f));
                SFXVolume = Mathf.Clamp01(PlayerPrefs.GetFloat(SFXVolumeKey, 1f));
                MusicEnabled = PlayerPrefs.GetInt(MusicEnabledKey, 1) == 1;
                SFXEnabled = PlayerPrefs.GetInt(SFXEnabledKey, 1) == 1;
                LanguageIndex = PlayerPrefs.GetInt(LanguageKey, 0); // Varsayılan TR (0)
                HapticEnabled = PlayerPrefs.GetInt(HapticEnabledKey, 1) == 1; // Varsayılan Açık (1)
            }
            catch (System.Exception e)
            {
                Debug.LogError($"SettingsModel: Ayarlar yüklenirken hata oluştu! Varsayılan değerler kullanılıyor. Hata: {e.Message}");
                SetToDefaults();
            }
        }

        /// <summary>
        /// Ayarları varsayılan değerlerine sıfırlar.
        /// </summary>
        private void SetToDefaults()
        {
            MusicVolume = 1f;
            SFXVolume = 1f;
            MusicEnabled = true;
            SFXEnabled = true;
            LanguageIndex = 0;
            HapticEnabled = true;
        }

        /// <summary>
        /// Müzik ses seviyesini ayarlar ve kaydeder.
        /// </summary>
        public void SetMusicVolume(float volume)
        {
            MusicVolume = Mathf.Clamp01(volume);
            PlayerPrefs.SetFloat(MusicVolumeKey, MusicVolume);
            PlayerPrefs.Save();
        }

        /// <summary>
        /// SFX ses seviyesini ayarlar ve kaydeder.
        /// </summary>
        public void SetSFXVolume(float volume)
        {
            SFXVolume = Mathf.Clamp01(volume);
            PlayerPrefs.SetFloat(SFXVolumeKey, SFXVolume);
            PlayerPrefs.Save();
        }

        /// <summary>
        /// Müzik çalma durumuunu değiştirir ve kaydeder.
        /// </summary>
        public void SetMusicEnabled(bool enabled)
        {
            MusicEnabled = enabled;
            PlayerPrefs.SetInt(MusicEnabledKey, enabled ? 1 : 0);
            PlayerPrefs.Save();
        }

        /// <summary>
        /// SFX çalma durumunu değiştirir ve kaydeder.
        /// </summary>
        public void SetSFXEnabled(bool enabled)
        {
            SFXEnabled = enabled;
            PlayerPrefs.SetInt(SFXEnabledKey, enabled ? 1 : 0);
            PlayerPrefs.Save();
        }

        /// <summary>
        /// Dil tercihini ayarlar ve kaydeder.
        /// </summary>
        public void SetLanguage(int index)
        {
            LanguageIndex = index;
            PlayerPrefs.SetInt(LanguageKey, index);
            PlayerPrefs.Save();
        }

        /// <summary>
        /// Haptic durumunu ayarlar ve kaydeder.
        /// </summary>
        public void SetHapticEnabled(bool enabled)
        {
            HapticEnabled = enabled;
            PlayerPrefs.SetInt(HapticEnabledKey, enabled ? 1 : 0);
            HapticManager.IsEnabled = enabled;
            PlayerPrefs.Save();
        }
    }
}