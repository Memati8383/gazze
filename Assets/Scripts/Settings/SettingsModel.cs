using UnityEngine;

namespace Settings
{
    /// <summary>
    /// Ayarlar verilerini tutan ve kalıcı hafızada (PlayerPrefs) saklayan sınıf.
    /// </summary>
    public class SettingsModel
    {
        /// <summary> Ayarlar değiştiğinde tetiklenen statik olay. </summary>
        public static System.Action OnSettingsChanged;

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
        /// <summary> Kontrol yöntemi için kullanılan PlayerPrefs anahtarı (0: Button, 1: Tilt). </summary>
        public const string ControlMethodKey = "ControlMethod";
        /// <summary> Hızlanma modu için kullanılan PlayerPrefs anahtarı (0: Manual, 1: Auto). </summary>
        public const string AccelerationModeKey = "AccelerationMode";
        /// <summary> Kontrol hassasiyeti için kullanılan PlayerPrefs anahtarı. </summary>
        public const string ControlSensitivityKey = "ControlSensitivity";
        /// <summary> İvmeölçer sıfır noktası için kullanılan PlayerPrefs anahtarı. </summary>
        public const string AccelerometerOffsetKey = "AccelerometerOffset";

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
        /// <summary> Seçili kontrol yöntemi (0: Button, 1: Tilt). </summary>
        public int ControlMethod { get; private set; }
        /// <summary> Seçili hızlanma modu (0: Auto, 1: Manual). </summary>
        public int AccelerationMode { get; private set; }
        /// <summary> Kontrol hassasiyeti (0.0f - 1.0f). </summary>
        public float ControlSensitivity { get; private set; }
        /// <summary> İvmeölçer sıfır noktası offset değeri. </summary>
        public float AccelerometerOffset { get; private set; }

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
                ControlMethod = PlayerPrefs.GetInt(ControlMethodKey, 0); // Varsayılan Button (0)
                AccelerationMode = PlayerPrefs.GetInt(AccelerationModeKey, 0); // Varsayılan Manuel (0)
                ControlSensitivity = PlayerPrefs.GetFloat(ControlSensitivityKey, 0.5f); // Varsayılan %50
                AccelerometerOffset = PlayerPrefs.GetFloat(AccelerometerOffsetKey, 0f);
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
            ControlMethod = 0;
            AccelerationMode = 0; // Varsayılan Manuel (0)
            ControlSensitivity = 0.5f;
            AccelerometerOffset = 0f;
            OnSettingsChanged?.Invoke();
        }

        /// <summary>
        /// Müzik ses seviyesini ayarlar ve kaydeder.
        /// </summary>
        public void SetMusicVolume(float volume)
        {
            MusicVolume = Mathf.Clamp01(volume);
            PlayerPrefs.SetFloat(MusicVolumeKey, MusicVolume);
            PlayerPrefs.Save();
            OnSettingsChanged?.Invoke();
        }

        /// <summary>
        /// SFX ses seviyesini ayarlar ve kaydeder.
        /// </summary>
        public void SetSFXVolume(float volume)
        {
            SFXVolume = Mathf.Clamp01(volume);
            PlayerPrefs.SetFloat(SFXVolumeKey, SFXVolume);
            PlayerPrefs.Save();
            OnSettingsChanged?.Invoke();
        }

        /// <summary>
        /// Müzik çalma durumuunu değiştirir ve kaydeder.
        /// </summary>
        public void SetMusicEnabled(bool enabled)
        {
            MusicEnabled = enabled;
            PlayerPrefs.SetInt(MusicEnabledKey, enabled ? 1 : 0);
            PlayerPrefs.Save();
            OnSettingsChanged?.Invoke();
        }

        /// <summary>
        /// SFX çalma durumunu değiştirir ve kaydeder.
        /// </summary>
        public void SetSFXEnabled(bool enabled)
        {
            SFXEnabled = enabled;
            PlayerPrefs.SetInt(SFXEnabledKey, enabled ? 1 : 0);
            PlayerPrefs.Save();
            OnSettingsChanged?.Invoke();
        }

        /// <summary>
        /// Dil tercihini ayarlar ve kaydeder.
        /// </summary>
        public void SetLanguage(int index)
        {
            LanguageIndex = index;
            PlayerPrefs.SetInt(LanguageKey, index);
            PlayerPrefs.Save();
            OnSettingsChanged?.Invoke();
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
            OnSettingsChanged?.Invoke();
        }

        /// <summary>
        /// Kontrol yöntemini ayarlar ve kaydeder.
        /// </summary>
        public void SetControlMethod(int index)
        {
            ControlMethod = index;
            PlayerPrefs.SetInt(ControlMethodKey, index);
            PlayerPrefs.Save();
            OnSettingsChanged?.Invoke();
        }

        /// <summary>
        /// Hızlanma modunu ayarlar ve kaydeder.
        /// </summary>
        public void SetAccelerationMode(int index)
        {
            AccelerationMode = index;
            PlayerPrefs.SetInt(AccelerationModeKey, index);
            PlayerPrefs.Save();
            OnSettingsChanged?.Invoke();
        }

        /// <summary>
        /// Kontrol hassasiyetini ayarlar ve kaydeder.
        /// </summary>
        public void SetControlSensitivity(float val)
        {
            ControlSensitivity = Mathf.Clamp(val, 0f, 1f);
            PlayerPrefs.SetFloat(ControlSensitivityKey, ControlSensitivity);
            PlayerPrefs.Save();
            OnSettingsChanged?.Invoke();
        }

        /// <summary>
        /// İvmeölçer sıfır noktası offsetini ayarlar ve kaydeder.
        /// </summary>
        public void SetAccelerometerOffset(float offset)
        {
            AccelerometerOffset = offset;
            PlayerPrefs.SetFloat(AccelerometerOffsetKey, offset);
            PlayerPrefs.Save();
            OnSettingsChanged?.Invoke();
        }


        /// <summary> Global değişim olayını tetikler (Dışarıdan sıfırlama durumları için). </summary>
        public static void NotifyChanges()
        {
            OnSettingsChanged?.Invoke();
        }

    }
}