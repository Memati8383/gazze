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
    public class LocalizationManager : MonoBehaviour
    {
        public static LocalizationManager Instance { get; private set; }

        [SerializeField] private Language currentLanguage = Language.TR;

        public event Action OnLanguageChanged;

        private Dictionary<string, DisplayData> translations = new Dictionary<string, DisplayData>();

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
                if (Application.isPlaying) DontDestroyOnLoad(gameObject);
                InitializeTranslations();
                LoadSavedLanguage();
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
            }
        }

        private void InitializeTranslations()
        {
            // Settings
            Add("Settings_Title", "AYARLAR", "SETTINGS");
            Add("Settings_Music", "MÜZİK", "MUSIC");
            Add("Settings_SFX", "SES EFEKTLERİ", "SFX");
            Add("Settings_EnableMusic", "MÜZİĞİ ETKİNLEŞTİR", "ENABLE MUSIC");
            Add("Settings_EnableSFX", "EFEKTLERİ ETKİNLEŞTİR", "ENABLE SFX");
            Add("Settings_Language", "OYUN DİLİ", "GAME LANGUAGE");
            Add("Settings_Back", "GERİ", "BACK");
            Add("Settings_ResetProgress", "İLERLEMEYİ SIFIRLA", "RESET PROGRESS");
            
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
            Add("Game_HighScore", "EN YÜKSEK SKOR", "HIGH SCORE");
            Add("Game_Level", "SEVİYE", "LEVEL");
            Add("Game_PlayTime", "OYNANIŞ SÜRESİ", "PLAY TIME");
            Add("Game_Achievements", "BAŞARIMLAR", "ACHIEVEMENTS");
            Add("Game_None", "YOK", "NONE");
            Add("Game_Balance", "BAKİYE", "BALANCE");
            Add("Game_Credit", "KREDİ", "CREDIT");
            Add("Game_Insufficient", "YETERSİZ", "INSUFFICIENT");
            Add("Game_BalanceError", "BAKİYE!", "BALANCE!");
            Add("Game_Share", "PAYLAŞ", "SHARE");
            Add("Game_Cancel", "İPTAL ET", "CANCEL");
            
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
            Add("Garage_CarTitle", "ARAÇ ÖZELLİKLERİ", "VEHICLE STATS");
            Add("Garage_Upgrades", "ARAÇ YÜKSELTMELERİ", "VEHICLE UPGRADES");
            Add("Garage_Price", "FİYAT", "PRICE");
            
            // New UI Elements
            Add("Game_LevelShort", "LVL", "LVL");
            Add("Game_Max", "MAKS", "MAX");
            Add("Game_Unit_Kmh", " km/s", " km/h");
            Add("Game_Unit_Ms2", " m/s²", " m/s²");
            Add("Game_Unit_Hp", " CAN", " HP");
            Add("Game_Unit_Sec", "s", "s");
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
