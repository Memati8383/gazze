using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Gazze.Models
{
    /// <summary>
    /// Araç özelliklerini yöneten depo sınıfı. CRUD işlemlerini sağlar.
    /// </summary>
    [CreateAssetMenu(fileName = "VehicleRepository", menuName = "Gazze/Vehicle Repository")]
    public class VehicleRepository : ScriptableObject
    {
        private static VehicleRepository _instance;
        public static VehicleRepository Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = Resources.Load<VehicleRepository>("VehicleRepository");
                }
                return _instance;
            }
        }

        [Header("Araç Listesi")]
        [Tooltip("Oyunda kullanilan tum VehicleAttributes kayitlari.")]
        public List<VehicleAttributes> vehicles = new List<VehicleAttributes>();

        /// <summary>
        /// Yeni bir araç özelliği ekler (Create).
        /// </summary>
        public void Create(VehicleAttributes vehicle)
        {
            if (vehicle != null && !vehicles.Contains(vehicle))
            {
                vehicles.Add(vehicle);
            }
        }

        /// <summary>
        /// Belirtilen isimdeki araç özelliğini döndürür (Read).
        /// </summary>
        public VehicleAttributes Read(string vehicleName)
        {
            return vehicles.FirstOrDefault(v => v.name == vehicleName);
        }

        /// <summary>
        /// Mevcut bir araç özelliğini günceller (Update).
        /// </summary>
        public void UpdateVehicle(string vehicleName, VehicleAttributes updatedAttributes)
        {
            int index = vehicles.FindIndex(v => v.name == vehicleName);
            if (index != -1)
            {
                vehicles[index] = updatedAttributes;
            }
        }

        /// <summary>
        /// Bir araç özelliğini siler (Delete).
        /// </summary>
        public void Delete(string vehicleName)
        {
            vehicles.RemoveAll(v => v.name == vehicleName);
        }


    }
}
