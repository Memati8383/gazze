using UnityEngine;

namespace Gazze.UI
{
    /// <summary>
    /// Başarım teması - her başarım için özel renk ve stil
    /// </summary>
    [System.Serializable]
    public class AchievementTheme
    {
        public string achievementName;
        // Premium Dark Background
        public Color backgroundColor = new Color(0.07f, 0.07f, 0.08f, 0.98f);
        public Color accentColor = new Color(1f, 0.84f, 0f, 1f); // Altın
        public Color glowColor = new Color(1f, 0.84f, 0f, 0.15f);
        public Color titleColor = new Color(0.95f, 0.95f, 0.95f, 1f);
        public Color descriptionColor = new Color(0.6f, 0.6f, 0.65f, 1f);
        public string iconText = "*"; // Unicode fallback replaced with ASCII to prevent font warnings
        public string iconPath = ""; // Resources altındaki ikon yolu
        public float iconRotation = 0f;
        public bool useParticles = true;
        public Color particleColor = new Color(1f, 0.84f, 0f, 1f);
        
        // Önceden tanımlı temalar
        public static AchievementTheme GetTheme(string achievementName)
        {
            if (string.IsNullOrEmpty(achievementName)) return GetDefaultTheme("Empty");
            
            // Normalize to handle Turkish characters (I/İ) and casing robustly
            string searchName = NormalizeTitle(achievementName);
            
            // Debug matching
            Debug.Log($"[Achievement] Matching: '{achievementName}' -> '{searchName}'");
            
            switch (searchName)
            {
                case "UZUN YOL":
                case "LONG WAY":
                case "MARATON":
                case "MARATHON":
                    return new AchievementTheme
                    {
                        achievementName = achievementName,
                        backgroundColor = new Color(0.06f, 0.08f, 0.12f, 0.98f),
                        accentColor = new Color(0.1f, 0.7f, 1f, 1f),
                        glowColor = new Color(0.1f, 0.7f, 1f, 0.15f),
                        titleColor = new Color(1f, 1f, 1f, 1f),
                        descriptionColor = new Color(0.6f, 0.7f, 0.8f, 1f),
                        iconText = "|>",
                        iconPath = "UI/Icons/Achievement_UzunYol",
                        particleColor = new Color(0.1f, 0.7f, 1f, 1f)
                    };
                    
                case "YARDIMSEVER":
                case "HELPFUL":
                case "COMERT":
                case "COMERT KALP":
                case "GENEROUS HEART":
                case "GENEROUS":
                    return new AchievementTheme
                    {
                        achievementName = achievementName,
                        backgroundColor = new Color(0.1f, 0.06f, 0.12f, 0.98f),
                        accentColor = new Color(0.8f, 0.2f, 1f, 1f),
                        glowColor = new Color(0.8f, 0.2f, 1f, 0.15f),
                        titleColor = new Color(1f, 1f, 1f, 1f),
                        descriptionColor = new Color(0.8f, 0.6f, 0.8f, 1f),
                        iconText = "<3",
                        iconPath = "UI/Icons/Achievement_Yardimsever",
                        particleColor = new Color(0.8f, 0.2f, 1f, 1f)
                    };
                    
                case "KIL PAYI":
                case "CLOSE CALL":
                case "YAKIN GECIS":
                case "TEHLIKE AVCISI":
                case "DANGER HUNTER":
                    return new AchievementTheme
                    {
                        achievementName = achievementName,
                        backgroundColor = new Color(0.12f, 0.06f, 0.06f, 0.98f),
                        accentColor = new Color(1f, 0.2f, 0.3f, 1f),
                        glowColor = new Color(1f, 0.2f, 0.3f, 0.15f),
                        titleColor = new Color(1f, 1f, 1f, 1f),
                        descriptionColor = new Color(0.8f, 0.6f, 0.6f, 1f),
                        iconText = "!!",
                        iconPath = "UI/Icons/Achievement_KilPayi",
                        particleColor = new Color(1f, 0.2f, 0.3f, 1f)
                    };
                    
                case "HIZ TUTKUNU":
                case "SPEED ENTHUSIAST":
                case "SPEED LOVER":
                    return new AchievementTheme
                    {
                        achievementName = achievementName,
                        backgroundColor = new Color(0.12f, 0.08f, 0.0f, 0.98f),
                        accentColor = new Color(1f, 0.65f, 0f, 1f),
                        glowColor = new Color(1f, 0.65f, 0f, 0.15f),
                        titleColor = new Color(1f, 1f, 1f, 1f),
                        descriptionColor = new Color(0.9f, 0.8f, 0.6f, 1f),
                        iconText = ">>",
                        iconPath = "UI/Icons/Achievement_HizTutkunu",
                        particleColor = new Color(1f, 0.65f, 0f, 1f)
                    };
                    
                case "SUPERSONIK":
                case "SUPERSONIC":
                    return new AchievementTheme
                    {
                        achievementName = achievementName,
                        backgroundColor = new Color(0.12f, 0.04f, 0.08f, 0.98f),
                        accentColor = new Color(1f, 0.1f, 0.8f, 1f),
                        glowColor = new Color(1f, 0.1f, 0.8f, 0.15f),
                        titleColor = new Color(1f, 1f, 1f, 1f),
                        descriptionColor = new Color(0.9f, 0.6f, 0.8f, 1f),
                        iconText = ">>",
                        iconPath = "UI/Icons/Achievement_Supersonic",
                        particleColor = new Color(1f, 0.1f, 0.8f, 1f)
                    };
                    
                case "ILK ADIM":
                case "FIRST STEP":
                    return new AchievementTheme
                    {
                        achievementName = achievementName,
                        backgroundColor = new Color(0.06f, 0.11f, 0.08f, 0.98f),
                        accentColor = new Color(0.1f, 1f, 0.4f, 1f),
                        glowColor = new Color(0.1f, 1f, 0.4f, 0.15f),
                        titleColor = new Color(1f, 1f, 1f, 1f),
                        descriptionColor = new Color(0.6f, 0.8f, 0.6f, 1f),
                        iconText = "+",
                        iconPath = "UI/Icons/Achievement_IlkAdim",
                        particleColor = new Color(0.1f, 1f, 0.4f, 1f)
                    };

                case "EFSANE":
                case "LEGEND":
                    return new AchievementTheme
                    {
                        achievementName = achievementName,
                        backgroundColor = new Color(0.15f, 0.15f, 0.2f, 0.98f), // Deep Platinum
                        accentColor = new Color(0.9f, 0.9f, 1f, 1f), // Shiny White
                        glowColor = new Color(0.9f, 0.9f, 1f, 0.3f),
                        titleColor = new Color(1f, 1f, 1f, 1f),
                        descriptionColor = new Color(0.7f, 0.7f, 0.9f, 1f),
                        iconText = "!!!",
                        iconPath = "UI/Icons/Achievement_Efsane",
                        particleColor = new Color(0.9f, 0.9f, 1f, 1f)
                    };

                case "GORUNMEZ":
                case "INVISIBLE":
                case "GÖRÜNMEZ":
                    return new AchievementTheme
                    {
                        achievementName = achievementName,
                        backgroundColor = new Color(0.05f, 0.15f, 0.15f, 0.98f), // Deep Teal
                        accentColor = new Color(0.2f, 1f, 0.9f, 1f), // Cyan Glow
                        glowColor = new Color(0.2f, 1f, 0.9f, 0.2f),
                        titleColor = new Color(1f, 1f, 1f, 1f),
                        descriptionColor = new Color(0.6f, 0.9f, 0.8f, 1f),
                        iconText = "?",
                        iconPath = "UI/Icons/Achievement_Gorunmez",
                        particleColor = new Color(0.2f, 1f, 0.9f, 1f)
                    };

                case "ISIK HIZI":
                case "LIGHT SPEED":
                case "IŞIK HIZI":
                    return new AchievementTheme
                    {
                        achievementName = achievementName,
                        backgroundColor = new Color(0.05f, 0.05f, 0.25f, 0.98f), // Electric Blue
                        accentColor = new Color(0.4f, 0.6f, 1f, 1f), // Lightning Blue
                        glowColor = new Color(0.4f, 0.6f, 1f, 0.4f),
                        titleColor = new Color(1f, 1f, 1f, 1f),
                        descriptionColor = new Color(0.7f, 0.8f, 1f, 1f),
                        iconText = ">>>",
                        iconPath = "UI/Icons/Achievement_IsikHizi",
                        particleColor = new Color(0.4f, 0.6f, 1f, 1f)
                    };
                    
                default:
                    return GetDefaultTheme(achievementName);
            }
        }

        private static string NormalizeTitle(string text)
        {
            if (string.IsNullOrEmpty(text)) return "";
            return text.Replace("İ", "I")
                       .Replace("ı", "I")
                       .Replace("i", "I")
                       .ToUpperInvariant()
                       .Replace("Ö", "O")
                       .Replace("Ü", "U")
                       .Replace("Ç", "C")
                       .Replace("Ş", "S")
                       .Replace("Ğ", "G")
                       .Trim();
        }

        private static AchievementTheme GetDefaultTheme(string achievementName)
        {
            return new AchievementTheme
            {
                achievementName = achievementName,
                backgroundColor = new Color(0.08f, 0.08f, 0.09f, 0.98f),
                accentColor = new Color(1f, 0.84f, 0f, 1f), // Gold
                glowColor = new Color(1f, 0.84f, 0f, 0.15f),
                titleColor = new Color(1f, 1f, 1f, 1f),
                descriptionColor = new Color(0.7f, 0.7f, 0.75f, 1f),
                iconText = "*",
                iconPath = "",
                particleColor = new Color(1f, 0.84f, 0f, 1f)
            };
        }
    }
}
