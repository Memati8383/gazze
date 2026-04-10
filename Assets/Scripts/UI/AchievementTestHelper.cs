using UnityEngine;
using UnityEngine.InputSystem;

namespace Gazze.UI
{
    /// <summary>
    /// Başarım bildirimlerini test etmek için yardımcı script
    /// </summary>
    public class AchievementTestHelper : MonoBehaviour
    {
        [Header("Test Settings")]
        [Tooltip("1 tuşu - Uzun Yol başarımı")]
        [SerializeField] private Key testKey1 = Key.Digit1;
        [Tooltip("2 tuşu - Yardımsever başarımı")]
        [SerializeField] private Key testKey2 = Key.Digit2;
        [Tooltip("3 tuşu - Kıl Payı başarımı")]
        [SerializeField] private Key testKey3 = Key.Digit3;
        [Tooltip("4 tuşu - Hız Tutkunu başarımı")]
        [SerializeField] private Key testKey4 = Key.Digit4;
        [Tooltip("5 tuşu - Süpersonik başarımı")]
        [SerializeField] private Key testKey5 = Key.Digit5;
        [Tooltip("6 tuşu - İlk Adım başarımı")]
        [SerializeField] private Key testKey6 = Key.Digit6;
        [Tooltip("7 tuşu - Efsane başarımı")]
        [SerializeField] private Key testKey7 = Key.Digit7;
        [Tooltip("8 tuşu - Görünmez başarımı")]
        [SerializeField] private Key testKey8 = Key.Digit8;
        [Tooltip("9 tuşu - Işık Hızı başarımı")]
        [SerializeField] private Key testKey9 = Key.Digit9;
        
        private void Update()
        {
            if (Keyboard.current != null)
            {
                if (Keyboard.current[testKey1].wasPressedThisFrame)
                {
                    TestAchievement("Uzun Yol", "1000+ puan kazandınız!");
                }
                
                if (Keyboard.current[testKey2].wasPressedThisFrame)
                {
                    TestAchievement("Yardımsever", "10+ yardım topladınız!");
                }
                
                if (Keyboard.current[testKey3].wasPressedThisFrame)
                {
                    TestAchievement("Kıl Payı", "5+ kıl payı kaçış yaptınız!");
                }
                
                if (Keyboard.current[testKey4].wasPressedThisFrame)
                {
                    TestAchievement("Hız Tutkunu", "45+ hıza ulaştınız!");
                }
                
                if (Keyboard.current[testKey5].wasPressedThisFrame)
                {
                    TestAchievement("Süpersonik", "60+ hıza ulaştınız!");
                }
                
                if (Keyboard.current[testKey6].wasPressedThisFrame)
                {
                    TestAchievement("İlk Adım", "Oyuna ilk adımınızı attınız!");
                }

                if (Keyboard.current[testKey7].wasPressedThisFrame)
                {
                    TestAchievement("EFSANE", "10,000 SKORA ULAŞILDI!");
                }

                if (Keyboard.current[testKey8].wasPressedThisFrame)
                {
                    TestAchievement("GÖRÜNMEZ", "50 YAKIN GEÇİŞ YAPILDI!");
                }

                if (Keyboard.current[testKey9].wasPressedThisFrame)
                {
                    TestAchievement("IŞIK HIZI", "90 KM/H HIZA ULAŞILDI!");
                }
            }
        }
        
        private void TestAchievement(string title, string description)
        {
            if (AchievementNotificationManager.Instance != null)
            {
                AchievementNotificationManager.Instance.ShowAchievement(title, description);
                Debug.Log($"Test başarımı gösterildi: {title}");
            }
            else
            {
                Debug.LogWarning("AchievementNotificationManager bulunamadı!");
            }
        }
        
        [ContextMenu("Test Multiple Achievements")]
        private void TestMultipleAchievements()
        {
            if (AchievementNotificationManager.Instance != null)
            {
                AchievementNotificationManager.Instance.ShowAchievement("İlk Başarım", "Oyuna hoş geldiniz!");
                Invoke(nameof(TestSecond), 0.5f);
                Invoke(nameof(TestThird), 1f);
            }
        }
        
        private void TestSecond()
        {
            AchievementNotificationManager.Instance.ShowAchievement("İkinci Başarım", "Harika gidiyorsunuz!");
        }
        
        private void TestThird()
        {
            AchievementNotificationManager.Instance.ShowAchievement("Üçüncü Başarım", "Mükemmel performans!");
        }
        
        [ContextMenu("Clear All Achievements")]
        private void ClearAllAchievements()
        {
            string[] achievements = new string[]
            {
                "Uzun Yol", "Maraton", "Yardımsever", "Cömert Kalp",
                "Kıl Payı", "Tehlike Avcısı", "Hız Tutkunu", "Süpersonik", "İlk Adım", 
                "EFSANE", "GÖRÜNMEZ", "IŞIK HIZI"
            };
            
            foreach (string achievement in achievements)
            {
                PlayerPrefs.DeleteKey($"Achievement_{achievement}");
            }
            
            PlayerPrefs.Save();
            Debug.Log("Tüm başarımlar temizlendi!");
        }
    }
}
