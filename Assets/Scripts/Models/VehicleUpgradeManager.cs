using UnityEngine;

namespace Gazze.Models
{
    /// <summary>
    /// Araç yükseltme (upgrade) sistemini yöneten, seviyeleri ve maliyetleri hesaplayan yardımcı sınıf.
    /// </summary>
    public static class VehicleUpgradeManager
    {
        public enum UpgradeType
        {
            Speed,
            Acceleration,
            Durability,
            BoostDuration,
            BoostRefillRate
        }

        public const int MaxUpgradeLevel = 5;
        private const int BaseUpgradeCost = 500;
        private const float BonusPerLevel = 0.1f; // %10 bonus her seviyede

        /// <summary>
        /// Belirli bir araç ve tip için mevcut yükseltme seviyesini döndürür.
        /// </summary>
        public static int GetUpgradeLevel(int carIndex, UpgradeType type)
        {
            return PlayerPrefs.GetInt($"CarUpgrade_{carIndex}_{type}", 0);
        }

        /// <summary>
        /// Belirli bir araç ve tip için bir sonraki yükseltme maliyetini hesaplar.
        /// </summary>
        public static int GetUpgradeCost(int carIndex, UpgradeType type)
        {
            int currentLevel = GetUpgradeLevel(carIndex, type);
            if (currentLevel >= MaxUpgradeLevel) return -1; // Maksimum seviye

            // Maliyet formülü: Temel * (Seviye + 1) * 1.5 (veya benzeri bir artış)
            return Mathf.RoundToInt(BaseUpgradeCost * (currentLevel + 1) * 1.5f);
        }

        /// <summary>
        /// Belirli bir araç ve tip için yükseltme işlemini gerçekleştirir.
        /// </summary>
        public static bool Upgrade(int carIndex, UpgradeType type)
        {
            int currentLevel = GetUpgradeLevel(carIndex, type);
            if (currentLevel >= MaxUpgradeLevel) return false;

            int cost = GetUpgradeCost(carIndex, type);
            int currentKredi = PlayerPrefs.GetInt("TotalKredi", 0);

            if (currentKredi >= cost)
            {
                PlayerPrefs.SetInt("TotalKredi", currentKredi - cost);
                PlayerPrefs.SetInt($"CarUpgrade_{carIndex}_{type}", currentLevel + 1);
                PlayerPrefs.Save();
                return true;
            }

            return false;
        }

        /// <summary>
        /// Temel değeri mevcut yükseltme seviyesine göre artırılmış olarak döndürür.
        /// </summary>
        public static float GetUpgradedValue(int carIndex, UpgradeType type, float baseValue)
        {
            int currentLevel = GetUpgradeLevel(carIndex, type);
            
            // Dayanıklılık için her seviye başlangıç canının %25'i kadar ekler
            // Eğer araç 4 kalp ile başlıyorsa, her seviye tam +1 kalp eklemiş olur (4 -> 5 -> 6)
            if (type == UpgradeType.Durability)
            {
                return baseValue * (1f + (currentLevel * 0.25f));
            }
            
            return baseValue * (1f + (currentLevel * BonusPerLevel));
        }

        /// <summary>
        /// Tüm yükseltmeleri sıfırlar (Hata ayıklama veya tam sıfırlama için).
        /// </summary>
        public static void ResetAllUpgrades(int carCount)
        {
            for (int i = 0; i < carCount; i++)
            {
                foreach (UpgradeType type in System.Enum.GetValues(typeof(UpgradeType)))
                {
                    PlayerPrefs.DeleteKey($"CarUpgrade_{i}_{type}");
                }
            }
            PlayerPrefs.Save();
        }
    }
}
