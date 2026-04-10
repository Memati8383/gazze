using UnityEngine;
using System.Collections.Generic;

namespace Gazze.Models
{
    public enum VehicleClass
    {
        Standard,
        Heavy,
        Sports
    }

    [System.Serializable]
    public class SurfaceMultiplier
    {
        [Tooltip("Yuzey tipi etiketi (asfalt, toprak vb.).")]
        public string surfaceType;
        [Tooltip("Bu yuzeyde uygulanacak hiz carpani.")]
        public float multiplier = 1.0f;
    }

    [CreateAssetMenu(fileName = "NewVehicleAttributes", menuName = "Gazze/Vehicle Attributes")]
    public class VehicleAttributes : ScriptableObject
    {
        [Header("0. Araç Modeli (Vehicle Class)")]
        [Tooltip("Aracin sinifini belirler ve fiyatlandirmada kullanilir.")]
        public VehicleClass vehicleClass = VehicleClass.Standard;

        [Header("1. Hız (Speed)")]
        [Tooltip("Maksimum hız değeri (0-300 km/s)")]
        [Range(0, 300)]
        public float maxSpeedKmh = 120f;
        [Tooltip("Farkli zeminler icin hiz carpanlari listesi.")]
        public List<SurfaceMultiplier> surfaceMultipliers = new List<SurfaceMultiplier>();

        [Header("2. Hızlanma (Acceleration)")]
        [Tooltip("0-100 km/s süresi (saniye)")]
        public float zeroToHundredTime = 8.0f;
        [Tooltip("Saniyede kat edilen mesafe artışı (m/s²)")]
        public float accelerationMs2 = 3.5f;

        [Header("3. Manevra (Handling)")]
        [Tooltip("Dönüş yarıçapı")]
        public float turnRadius = 5.0f;
        [Tooltip("Direksiyon hassasiyeti")]
        public float steeringSensitivity = 1.0f;
        [Tooltip("Yan kayma katsayısı")]
        public float driftCoefficient = 0.5f;
        [Tooltip("Aerodinamik katsayılar")]
        public float aerodynamicCoefficient = 0.3f;

        [Header("4. Dayanıklılık (Durability)")]
        [Tooltip("Dayanıklılık seviyesi %0-100")]
        [Range(0, 100)]
        public float durability = 100f;
        [Tooltip("Onarım maliyeti katsayısı")]
        public float repairCostFactor = 1.0f;
        [Tooltip("Onarım süresi katsayısı")]
        public float repairTimeFactor = 1.0f;

        [Header("5. Satın Alma Ayarları")]
        [Tooltip("Araç başlangıçta kilitli mi?")]
        public bool isLocked = true;

        /// <summary>
        /// Sınır değerlerini kontrol eder ve düzeltir.
        /// </summary>
        public void Validate()
        {
            maxSpeedKmh = Mathf.Clamp(maxSpeedKmh, 0f, 300f);
            durability = Mathf.Clamp(durability, 0f, 100f);
        }
    }
}
