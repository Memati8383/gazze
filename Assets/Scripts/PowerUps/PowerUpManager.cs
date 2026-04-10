using UnityEngine;
using System;
using System.Collections.Generic;

namespace Gazze.PowerUps
{
    public enum PowerUpType
    {
        Magnet,
        Shield,
        Ghost,
        TimeWarp,
        ShockWave,
        Juggernaut
    }

    [Serializable]
    public class PowerUpData
    {
        public PowerUpType type;
        public float duration;
        public Sprite icon;
        public Color themeColor;
        public string displayName;
    }

    public class PowerUpManager : MonoBehaviour
    {
        public static PowerUpManager Instance { get; private set; }

        [Header("Settings")]
        public PowerUpData[] availablePowerUps;
        
        private Dictionary<PowerUpType, float> activePowerUps = new Dictionary<PowerUpType, float>();
        private List<PowerUpType> activeKeys = new List<PowerUpType>();
        private List<PowerUpType> expiredKeys = new List<PowerUpType>();
        private HashSet<PowerUpType> expiringNotified = new HashSet<PowerUpType>();
        
        public event Action<PowerUpType, float, float> OnPowerUpActivated;
        public event Action<PowerUpType> OnPowerUpDeactivated;
        public event Action<PowerUpType, float> OnPowerUpTimerUpdated;
        public event Action<PowerUpType> OnPowerUpExpiring;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        public void ActivatePowerUp(PowerUpType type)
        {
            var data = Array.Find(availablePowerUps, p => p.type == type);
            if (data == null) return;

            if (activePowerUps.ContainsKey(type))
            {
                // Mevcut süreyi sıfırla ve yeni süreyi ekle (Stacking logic)
                activePowerUps[type] = data.duration;
                expiringNotified.Remove(type); // Tekrar alındığı için uyarıyı temizle
            }
            else
            {
                activePowerUps.Add(type, data.duration);
                activeKeys.Add(type);
            }

            OnPowerUpActivated?.Invoke(type, data.duration, data.duration);
        }

        private void Update()
        {
            if (activeKeys.Count == 0) return;

            expiredKeys.Clear();
            float unscaledDt = Time.unscaledDeltaTime;
            float dt = Time.deltaTime;

            for (int i = 0; i < activeKeys.Count; i++)
            {
                PowerUpType type = activeKeys[i];
                
                // Güç süreleri her zaman gerçek zaman (Unscaled) üzerinden ilerlemeli.
                // Aksi halde TimeWarp aktifken diğer güçlerin süresi 'haksız' şekilde uzar.
                float finalDt = Time.unscaledDeltaTime;
                
                activePowerUps[type] -= finalDt;
                float remainingTime = activePowerUps[type];

                OnPowerUpTimerUpdated?.Invoke(type, remainingTime);

                // Son 2.5 saniye kala "bitiyor" uyarısı ver
                if (remainingTime < 2.5f && !expiringNotified.Contains(type))
                {
                    OnPowerUpExpiring?.Invoke(type);
                    expiringNotified.Add(type);
                }

                if (remainingTime <= 0)
                {
                    expiredKeys.Add(type);
                }
            }

            foreach (var type in expiredKeys)
            {
                activePowerUps.Remove(type);
                activeKeys.Remove(type);
                expiringNotified.Remove(type);
                OnPowerUpDeactivated?.Invoke(type);
            }
        }

        public bool IsPowerUpActive(PowerUpType type)
        {
            return activePowerUps.ContainsKey(type) && activePowerUps[type] > 0;
        }

        public float GetRemainingTime(PowerUpType type)
        {
            return activePowerUps.TryGetValue(type, out float t) ? t : 0f;
        }

        public PowerUpData GetData(PowerUpType type)
        {
            return Array.Find(availablePowerUps, p => p.type == type);
        }
    }
}

