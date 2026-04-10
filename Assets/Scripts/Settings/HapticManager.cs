using UnityEngine;
using System.Collections;

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
        /// </summary>
        public static void Light()
        {
            if (!IsEnabled) return;

            #if !UNITY_EDITOR && (UNITY_ANDROID || UNITY_IOS)
            // Not: Unity standart Handheld.Vibrate() sadece ağır bir titreşim yapar.
            // Bu metod gelecekte bir eklenti (ör: Taptic) ile daha hafif bir titreşime çevrilebilir.
            Handheld.Vibrate();
            #endif
        }

        /// <summary>
        /// Orta seviye bir geri bildirim gönderir (Nesne Toplama, Başarı).
        /// </summary>
        public static void Medium()
        {
            if (!IsEnabled) return;

            #if !UNITY_EDITOR && (UNITY_ANDROID || UNITY_IOS)
            Handheld.Vibrate();
            #endif
        }

        /// <summary>
        /// Ağır bir geri bildirim gönderir (Çarpışma, Oyun Sonu).
        /// </summary>
        public static void Heavy()
        {
            if (!IsEnabled) return;

            #if !UNITY_EDITOR && (UNITY_ANDROID || UNITY_IOS)
            Handheld.Vibrate();
            #endif
        }

        /// <summary>
        /// Ölüm anında azalarak devam eden bir titreşim serisi başlatır.
        /// </summary>
        public static void DeathSequence(MonoBehaviour runner)
        {
            if (!IsEnabled) return;
            runner.StartCoroutine(DoDeathSequence());
        }

        private static IEnumerator DoDeathSequence()
        {
            Heavy();
            yield return new WaitForSecondsRealtime(0.15f);
            Medium();
            yield return new WaitForSecondsRealtime(0.2f);
            Light();
        }

        private static float _nextWindHapticTime = 0f;

        /// <summary>
        /// Yüksek hızlarda "Rüzgar Hissiyatı" simüle etmek için periyodik hafif titreşim.
        /// </summary>
        /// <param name="speedFactor">0-1 arası hız faktörü (0: Cruise, 1: Max Boost)</param>
        public static void WindSensation(float speedFactor)
        {
            if (!IsEnabled || speedFactor < 0.3f) return;

            // Hız arttıkça titreşim sıklığı artar (0.3sn - 1.2sn arası)
            float interval = Mathf.Lerp(1.2f, 0.3f, speedFactor);
            
            if (Time.time >= _nextWindHapticTime)
            {
                _nextWindHapticTime = Time.time + interval;
                Light();
            }
        }
    }
}
