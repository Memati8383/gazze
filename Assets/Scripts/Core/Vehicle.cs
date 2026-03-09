using UnityEngine;
using Gazze.Models;

namespace Gazze.Vehicles
{
    /// <summary>
    /// Araç prefabları üzerine eklenen ve o aracın özelliklerini (VehicleAttributes) tutan bileşen.
    /// </summary>
    public class Vehicle : MonoBehaviour
    {
        [Header("Araç Kimliği ve Özellikleri")]
        [Tooltip("Bu araca ait tüm teknik özellikler bu Asset üzerinden okunur.")]
        public VehicleAttributes attributes;

        /// <summary>
        /// Aracın ismini döndürür.
        /// </summary>
        public string GetName()
        {
            return attributes != null ? attributes.name : gameObject.name;
        }

        private void OnValidate()
        {
            if (attributes == null)
            {
                Debug.LogWarning($"{gameObject.name} üzerinde 'Vehicle' bileşeni var ama 'Attributes' atanmamış!");
            }
        }
    }
}
