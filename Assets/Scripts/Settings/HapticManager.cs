using UnityEngine;

namespace Settings
{
    /// <summary>
    /// Mobil cihazlar için titreşim (Haptic Feedback) yönetimini sağlayan sınıf.
    /// Farklı yoğunluk düzeylerini simüle eder.
    /// </summary>
    public static class HapticManager
    {
        private const string HapticEnabledKey = "HapticEnabled";

        /// <summary> Haptic feedback açık mı? </summary>
        public static bool IsEnabled
        {
            get => PlayerPrefs.GetInt(HapticEnabledKey, 1) == 1;
            set => PlayerPrefs.SetInt(HapticEnabledKey, value ? 1 : 0);
        }

        /// <summary>
        /// Hafif bir geri bildirim gönderir (UI Seçimleri, Buton Tıklamaları).
        /// Not: Unity standart Handheld.Vibrate() sadece ağır bir titreşim yapar.
        /// Modern mobil cihazlarda (iOS/Android Taptic) daha ince ayar için eklenti gerekir.
        /// Bu metod temel titreşimi kontrollü bir şekilde tetikler.
        /// </summary>
        public static void Light()
        {
            if (!IsEnabled) return;

            #if UNITY_ANDROID || UNITY_IOS
            Vibrate();
            #endif
        }

        /// <summary>
        /// Orta seviye bir geri bildirim gönderir (Upgrade Başarılı, Nesne Toplama).
        /// </summary>
        public static void Medium()
        {
            if (!IsEnabled) return;

            #if UNITY_ANDROID || UNITY_IOS
            Vibrate();
            #endif
        }

        /// <summary>
        /// Ağır bir geri bildirim gönderir (Çarpışma, Oyun Sonu).
        /// </summary>
        public static void Heavy()
        {
            if (!IsEnabled) return;

            #if UNITY_ANDROID || UNITY_IOS
            Vibrate();
            #endif
        }

        private static void Vibrate()
        {
            // Unity'nin varsayılan titreşim fonksiyonu
            #if !UNITY_EDITOR && (UNITY_ANDROID || UNITY_IOS)
            Handheld.Vibrate();
            #endif
        }
    }
}
