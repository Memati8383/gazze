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
                Debug.LogWarning($"{gameObject.name} (Path: {GetPath(transform)}) üzerinde 'Vehicle' bileşeni var ama 'Attributes' atanmamış!");
            }
        }

        private string GetPath(Transform t)
        {
            string path = t.name;
            while (t.parent != null)
            {
                t = t.parent;
                path = t.name + "/" + path;
            }
            return path;
        }
    }
}
