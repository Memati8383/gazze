using System;
using System.Collections.Generic;
using UnityEngine;

namespace Gazze.UI
{
    public enum Language
    {
        TR,
        EN
    }

    [ExecuteAlways]
    [DefaultExecutionOrder(-100)]
    /// <summary>
    /// Uygulama genelindeki yerelleştirme anahtarlarını yönetir ve dil değişimlerini yayınlar.
    /// </summary>
    public class LocalizationManager : MonoBehaviour
    {
        public static LocalizationManager Instance { get; private set; }

        [Header("Dil Ayarları")]
        [Tooltip("Varsayılan ve aktif dil seçimi (TR: Türkçe, EN: İngilizce).")]
        [SerializeField] private Language currentLanguage = Language.TR;

        /// <summary> Dil değiştiğinde tetiklenen olay. </summary>
        public event Action OnLanguageChanged;

        /// <summary> Anahtar-Değer çiftlerini tutan çeviri sözlüğü. </summary>
        private Dictionary<string, DisplayData> translations = new Dictionary<string, DisplayData>();

        /// <summary> Her dil için çeviri metnini tutan yapı. </summary>
        private struct DisplayData
        {
            public string tr;
            public string en;

            public string Get(Language lang) => lang == Language.TR ? tr : en;
        }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                // Sahne değişimlerinde objeyi koru
                if (Application.isPlaying) DontDestroyOnLoad(gameObject);
                
                // Çevirileri belleğe yükle
                InitializeTranslations();
                
                // Kayıtlı dil tercihini yükle
                LoadSavedLanguage();
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
            }
        }

        private void InitializeTranslations()
        {
            // Main Menu
            Add("Menu_Start", "BAŞLA", "START");
            Add("Menu_Garage", "GARAJ", "GARAGE");
            Add("Menu_Settings", "AYARLAR", "SETTINGS");
            Add("Menu_Exit", "ÇIKIŞ", "EXIT");
            
            // Game UI
            Add("Game_Speed", "HIZ", "SPEED");
            Add("Game_Distance", "MESAFE", "DISTANCE");
            Add("Game_Repair", "ONAR", "REPAIR");
            Add("Game_Help", "YARDIM", "HELP");
            Add("Game_Kmh", "KM/S", "KM/H");
            Add("Game_GameOver", "OYUN BİTTİ", "GAME OVER");
            Add("Game_Restart", "TEKRAR DENE", "RETRY");
            Add("Game_MainMenu", "ANA MENÜ", "MAIN MENU");
            Add("Game_LockedVehicle", "KİLİTLİ ARAÇ", "LOCKED VEHICLE");
            Add("Game_PurchaseRequired", "ARAÇ SATIN ALINMAMIŞ: {0}\nLütfen ana menüden aracı satın alın veya başka bir araç seçin.", "VEHICLE NOT PURCHASED: {0}\nPlease purchase it from the main menu or select another vehicle.");
            Add("Game_NotPurchasedAch", "Araç henüz alınmadı!", "Vehicle not purchased yet!");
            
            Add("Game_Score", "SKOR", "SCORE");
            Add("Game_HighScore", "REKOR", "HIGH SCORE");
            Add("Game_Level", "SEVİYE", "LEVEL");
            Add("Game_PlayTime", "SÜRE", "PLAY TIME");
            Add("Game_NearMisses", "MAKAS", "NEAR MISS");
            Add("Game_Achievements", "BAŞARIMLAR", "ACHIEVEMENTS");
            Add("Game_None", "YOK", "NONE");
            Add("Game_Balance", "BAKİYE", "BALANCE");
            Add("Game_Credit", "KREDİ", "CREDIT");
            Add("Game_Insufficient", "YETERSİZ", "INSUFFICIENT");
            Add("Game_BalanceError", "BAKİYE!", "BALANCE!");
            Add("Game_Share", "PAYLAŞ", "SHARE");
            Add("Game_ShareText", "{0} oyunundaki skorumu gör!", "Check out my score in {0}!");
            Add("Game_Cancel", "İPTAL ET", "CANCEL");
            
            // In-Game Notifications & Grades
            Add("Grade_Amazing", "HARİKA!", "AMAZING!");
            Add("Grade_Perfect", "MÜKEMMEL!", "PERFECT!");
            Add("Grade_NearMiss", "KIL PAYI!", "NEAR MISS!");
            Add("Game_Combo", "COMBO x{0}!", "COMBO x{0}!");
            Add("Game_AwardedCoin", "+1 YARDIM!", "+1 AID!");
            Add("PowerUp_Crushed", "EZİLDİ!", "CRUSHED!");
            
            // Boost Status
            Add("Boost_Status_Active", "HIZLANDIRILIYOR...", "BOOSTING...");
            Add("Boost_Status_Ready", "HAZIR", "READY");

            // Pause Menu
            Add("Game_Paused", "DURAKLATILDI", "PAUSED");
            Add("Game_PausedSub", "Oyun duraklatıldı", "Game is paused");
            Add("Game_Continue", "DEVAM ET", "CONTINUE");
            Add("Game_PauseHint", "ESC ile devam et", "Press ESC to continue");
            
            // Settings Panel
            Add("Menu_Settings", "AYARLAR", "SETTINGS"); // Duplicate, but kept for consistency with the provided edit
            Add("Settings_Subtitle", "Oyun tercihlerinizi özelleştirin", "Customize your game preferences");
            Add("Settings_Audio_Header", "SES AYARLARI", "AUDIO SETTINGS");
            Add("Settings_Music", "Müzik Ses Seviyesi", "Music Volume");
            Add("Settings_SFX", "Efekt Ses Seviyesi", "SFX Volume");
            Add("Settings_EnableMusic", "Müziği Etkinleştir", "Enable Music");
            Add("Settings_EnableSFX", "Efektleri Etkinleştir", "Enable SFX");
            Add("Settings_System_Header", "SİSTEM", "SYSTEM");
            Add("Settings_Lang_Header", "DİL SEÇİMİ", "LANGUAGE SELECTION");
            Add("Settings_Back", "GERİ DÖN", "BACK");
            Add("Settings_Language", "Dil", "Language"); 
            Add("Settings_ResetProgress", "Tüm İlerlemeyi Sıfırla", "Reset All Progress");
            Add("Settings_Haptic", "TİTREŞİM", "HAPTIC FEEDBACK");
            
            // Control Settings
            Add("Settings_Control_Header", "KONTROL AYARLARI", "CONTROL SETTINGS");
            Add("Settings_ControlMethod", "KONTROL YÖNTEMİ", "CONTROL METHOD");
            Add("Settings_ControlMethod_Buttons", "BUTONLAR", "BUTTONS");
            Add("Settings_ControlMethod_Analog", "ANALOG", "ANALOG");
            Add("Settings_ControlMethod_Tilt", "TİLT", "TILT");
            Add("Settings_AccelMode", "HIZLANMA MODU", "ACCELERATION MODE");
            Add("Settings_Accel_Manual", "MANUEL", "MANUAL");
            Add("Settings_Accel_Auto", "OTOMATİK", "AUTO");
            Add("Settings_Sensitivity", "HASSASİYET", "SENSITIVITY");
            Add("Settings_Calibrate", "CİHAZI KALİBRE ET", "CALIBRATE DEVICE");
            Add("Settings_Calibrate_Info", "Cihazı düz tutun ve kalibrasyon butonuna basın", "Hold your device flat and press the calibrate button");
            Add("Settings_Calibrate_Btn", "KALİBRE ET", "CALIBRATE");
            Add("Settings_Calibrate_Now", "ŞİMDİ KALİBRE ET", "CALIBRATE NOW");
            
            // Loading
            Add("Load_Readying", "SİSTEM HAZIRLANIYOR", "PREPARING SYSTEM");
            Add("Load_Complete", "YÜKLEME TAMAMLANDI", "LOADING COMPLETE");
            Add("Load_Data", "VERİLER AYIKLANIYOR", "EXTRACTING DATA");
            Add("Load_Assets", "VARLIKLAR YÜKLENİYOR", "LOADING ASSETS");
            Add("Load_World", "DÜNYA OLUŞTURULUYOR", "CREATING WORLD");
            Add("Load_Final", "SON AYARLAR YAPILIYOR", "FINALIZING SETTINGS");
            
            // Garage
            Add("Garage_Select", "SEÇ", "SELECT");
            Add("Garage_Buy", "SATIN AL", "BUY");
            Add("Garage_Locked", "KİLİTLİ", "LOCKED");
            Add("Garage_Speed", "MAX HIZ", "MAX SPEED");
            Add("Garage_Acc", "HIZLANMA", "ACCEL.");
            Add("Garage_Handling", "YOL TUTUŞ", "HANDLING");
            Add("Garage_Durability", "DAYANIKLILIK", "DURABILITY");
            Add("Garage_Boost", "BOOST GÜCÜ", "BOOST POWER");
            Add("Garage_Refill", "DOLUM HIZI", "RECHARGE");
            Add("Garage_CarTitle", "ARAÇ ÖZELLİKLERİ", "VEHICLE STATS");
            Add("Garage_Upgrades", "ARAÇ YÜKSELTMELERİ", "VEHICLE UPGRADES");
            Add("Garage_Upgrades_Title", "GELİŞTİRMELER", "UPGRADES");
            Add("Garage_Price", "FİYAT", "PRICE");
            Add("Garage_Kredi_Total", "TOPLAM KREDİ", "TOTAL CREDITS");

            // Garage Labels (Short)
            Add("Garage_Label_Speed", "HIZ", "SPEED");
            Add("Garage_Label_Accel", "İVME", "ACCEL");
            Add("Garage_Label_Durability", "CAN", "HP");
            Add("Garage_Label_Boost", "BOOST", "BOOST");
            Add("Garage_Label_Refill", "DOLUM", "REFILL");
            
            // New UI Elements
            Add("Game_LevelShort", "LVL", "LVL");
            Add("Game_Max", "MAKS", "MAX");
            Add("Game_Unit_Kmh", " km/s", " km/h");
            Add("Game_Unit_Ms2", " m/s²", " m/s²");
            Add("Game_Unit_Hp", " CAN", " HP");
            Add("Game_Unit_Sec", "s", "s");
            Add("Game_Unit_Credit", " Kredi", " Credits");
            Add("Game_Unit_PerSec", "/sn", "/s");
            Add("Game_Unit_Percent", "%", "%");
            Add("Game_Unit_Dollar", "$", "$");

            // Branding
            Add("Game_Title", "GÖREV: GAZZE", "MISSION: GAZA");
            Add("Game_Subtitle", "İNSANİ YARDIM KONVOYU", "HUMANITARIAN CONVOY");
            Add("Game_NewHighScore", "YENİ REKOR!", "NEW RECORD!");

            // Countdown
            Add("Countdown_Ready", "HAZIR OL!", "GET READY!");
            Add("Countdown_Sub3", "HAZIRLAN...", "PREPARE...");
            Add("Countdown_Sub2", "DİKKAT...", "WATCH OUT...");
            Add("Countdown_Sub1", "ŞİMDİ!", "NOW!");
            Add("Countdown_Go", "BAŞLA!", "GO!");
            Add("Countdown_GoSub", "İLERİ!", "PUSH FORWARD!");

            // Language Buttons
            Add("Lang_TR", "TÜRKÇE", "TURKISH");
            Add("Lang_EN", "ENGLISH", "ENGLISH");

            // Tips
            Add("Tip_Safe", "Hızlı değil, güvenli ilerle. Konvoyun can damarı sensin!", "Drive safe, not fast. You are the lifeline of the convoy!");
            Add("Tip_Damage", "Hasar alan araçlar yavaşlar. Ekip ruhunu koru.", "Damaged vehicles slow down. Protect the team spirit.");
            Add("Tip_Hope", "Gazze'deki siviller için her teslimat bir umut ışığıdır.", "Every delivery is a ray of hope for the civilians in Gaza.");
            Add("Tip_Alert", "Yol üzerindeki engellere karşı tetikte ol.", "Be alert against obstacles on the road.");
            Add("Tip_Fuel", "Yakıtını tasarruflu kullan, hedefe ulaştığından emin ol.", "Use your fuel efficiently, ensure you reach the target.");
            Add("Tip_Upgrade", "Aracını garajda yükselterek daha zorlu yollara hazır ol.", "Prepare for tougher roads by upgrading your vehicle in the garage.");
            Add("Tip_Night", "Gece sürüşlerinde farlarını kontrol etmeyi unutma.", "Don't forget to check your headlights during night drives.");
            Add("Tip_Duty", "İnsani yardım sadece yük değil, bir sorumluluktur.", "Humanitarian aid is not just cargo, it's a responsibility.");
            Add("Tip_Combo", "Kombo çarpanını yüksek tutmak için araçlara kıl payı yaklaş!", "Get close to vehicles to keep your combo multiplier high!");
            Add("Tip_Powerup", "Yol üzerindeki yardım sandıklarını toplayarak özel güçler kazan.", "Collect aid boxes on the road to gain special power-ups.");
            Add("Tip_Magnet", "Mıknatıs ile çevrendeki yardımları zahmetsizce toplayabilirsin.", "With the Magnet, you can effortlessly collect nearby aid.");
            Add("Tip_Shield", "Kalkan, aracını beklenmedik çarpışmalara karşı tek seferlik korur.", "The Shield protects your vehicle once against unexpected collisions.");
            Add("Tip_Garage", "Daha dayanıklı bir konvoy için garajdaki yeni araçlara göz at.", "Check out new vehicles in the garage for a more durable convoy.");
            Add("Tip_Score", "Yüksek skor için hem hızlı gitmeli hem de yardımları toplamalısın.", "For a high score, you must both drive fast and collect aid.");
            Add("Tip_Unity", "Gazze'ye giden yol birlik ve beraberlikten geçer.", "The road to Gaza is paved with unity and solidarity.");
            Add("Tip_Neon", "Neon bariyerler yolun sınırlarını belirler, onlara dikkat et!", "Neon barriers mark the limits of the road, watch out for them!");
            
            // Power-Ups
            Add("PowerUp_Magnet", "MIKNATIS", "MAGNET");
            Add("PowerUp_Magnet_Desc", "Çevredeki tüm yardımları çeker.", "Attracts all aid boxes around you.");
            Add("PowerUp_Shield", "KALKAN", "SHIELD");
            Add("PowerUp_Shield_Desc", "Tek seferlik bir darbeyi engeller.", "Blocks a single impact.");
            Add("PowerUp_Ghost", "HAYALET", "GHOST");
            Add("PowerUp_Ghost_Desc", "Trafik araçlarının içinden geçmeni sağlar.", "Allows you to pass through traffic cars.");
            Add("PowerUp_TimeWarp", "ZAMAN BÜKÜCÜ", "TIME WARP");
            Add("PowerUp_TimeWarp_Desc", "Zamanı yavaşlatarak manevra kabiliyetini artırır.", "Slows down time to increase maneuverability.");
            Add("PowerUp_ShockWave", "ŞOK DALGASI", "SHOCKWAVE");
            Add("PowerUp_ShockWave_Desc", "Yakındaki tüm araçları uzağa fırlatır.", "Flings all nearby cars away.");
            Add("PowerUp_Juggernaut", "DEV MODU", "JUGGERNAUT");
            Add("PowerUp_Juggernaut_Desc", "Aracı büyüterek önüne çıkan her şeyi ezer.", "Enlarges the vehicle and crushes everything in its path.");
            
            // Achievements
            Add("Ach_FirstStep_Title", "İlk Adım", "First Step");
            Add("Ach_FirstStep_Desc", "İlk oyununuzu tamamladınız!", "You completed your first game!");
            Add("Ach_LongWay_Title", "Uzun Yol", "Long Way");
            Add("Ach_LongWay_Desc", "1000+ puan kazandınız!", "You earned 1000+ points!");
            Add("Ach_Marathon_Title", "Maraton", "Marathon");
            Add("Ach_Marathon_Desc", "5000+ puan kazandınız!", "You earned 5000+ points!");
            Add("Ach_Helpful_Title", "Yardımsever", "Helpful");
            Add("Ach_Helpful_Desc", "10+ yardım topladınız!", "You collected 10+ aid boxes!");
            Add("Ach_Generous_Title", "Cömert Kalp", "Generous Heart");
            Add("Ach_Generous_Desc", "50+ yardım topladınız!", "You collected 50+ aid boxes!");
            Add("Ach_NearMiss_Title", "Kıl Payı", "Close Call");
            Add("Ach_NearMiss_Desc", "5+ kıl payı kaçış yaptınız!", "You made 5+ near misses!");
            Add("Ach_DangerHunter_Title", "Tehlike Avcısı", "Danger Hunter");
            Add("Ach_DangerHunter_Desc", "20+ kıl payı kaçış yaptınız!", "You made 20+ near misses!");
            Add("Ach_SpeedLover_Title", "Hız Tutkunu", "Speed Enthusiast");
            Add("Ach_SpeedLover_Desc", "45+ hıza ulaştınız!", "You reached 45+ speed!");
            Add("Ach_Supersonic_Title", "Süpersonik", "Supersonic");
            Add("Ach_Supersonic_Desc", "60+ hıza ulaştınız!", "You reached 60+ speed!");
            
            // New Achievements
            Add("Ach_Legend_Title", "Efsane", "Legend");
            Add("Ach_Legend_Desc", "10,000 skora ulaşıldı! Bir efsanesin!", "10,000 score reached! You are a legend!");
            Add("Ach_Invisible_Title", "Görünmez", "Invisible");
            Add("Ach_Invisible_Desc", "50 yakın geçiş! Seni göremiyorlar bile!", "50 near misses! They can't even see you!");
            Add("Ach_LightSpeed_Title", "Işık Hızı", "Light Speed");
            Add("Ach_LightSpeed_Desc", "90 km/h! Fizik kurallarını zorluyorsun!", "90 km/h! You are breaking the laws of physics!");
            
            Add("Achievement_Unlocked", "Başarım kazanıldı!", "Achievement unlocked!");
        }

        private void Add(string key, string tr, string en)
        {
            translations[key] = new DisplayData { tr = tr, en = en };
        }

        public string GetTranslation(string key)
        {
            if (translations == null || translations.Count == 0) InitializeTranslations();
            if (string.IsNullOrEmpty(key) || key == "ENTER_KEY_HERE") return string.Empty;

            if (translations.TryGetValue(key, out var data))
            {
                return data.Get(currentLanguage);
            }
            return key;
        }

        /// <summary>
        /// Statik erişim noktası: Instance kontrolü yaparak çeviri döner. 
        /// Instance null ise anahtarı veya isteğe bağlı fallback metnini döndürür.
        /// </summary>
        public static string Get(string key, string fallback = "")
        {
            if (Instance == null) return !string.IsNullOrEmpty(fallback) ? fallback : key;
            return Instance.GetTranslation(key);
        }

        public static string GetFormatted(string key, params object[] args)
        {
            string template = Get(key, key);
            if (args == null || args.Length == 0) return template;

            try
            {
                return string.Format(template, args);
            }
            catch (FormatException)
            {
                return template;
            }
        }

        public void SetLanguage(Language lang)
        {
            if (currentLanguage != lang)
            {
                currentLanguage = lang;
                // SettingsModel ile uyumlu olması için INT olarak kaydediyoruz
                PlayerPrefs.SetInt("Language", (int)lang);
                PlayerPrefs.Save();
                OnLanguageChanged?.Invoke();
            }
        }

        public Language GetCurrentLanguage() => currentLanguage;

        public void LoadSavedLanguage()
        {
            // SettingsModel ile uyumlu olması için INT veya String yedeğini kontrol et
            if (PlayerPrefs.HasKey("Language"))
            {
                int savedInt = PlayerPrefs.GetInt("Language", 0);
                currentLanguage = (Language)Mathf.Clamp(savedInt, 0, 1);
            }
            else
            {
                currentLanguage = Language.TR;
            }
            OnLanguageChanged?.Invoke();
        }
    }
}
