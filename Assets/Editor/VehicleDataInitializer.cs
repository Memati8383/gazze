using UnityEngine;
using Gazze.Models;

namespace Gazze.Editor
{
    /// <summary>
    /// Proje ilk açıldığında veya araç listesi boş olduğunda varsayılan araç verilerini oluşturan yardımcı sınıf.
    /// </summary>
    public static class VehicleDataInitializer
    {
        [UnityEditor.MenuItem("Gazze/Initialize Default Vehicle Data")]
        public static void Initialize()
        {
            VehicleRepository repo = VehicleRepository.Instance;
            if (repo == null)
            {
                // Debug.Log("VehicleRepository bulunamadı, yeni bir tane oluşturuluyor...");
                
                // Resources klasörünü kontrol et ve oluştur
                if (!System.IO.Directory.Exists("Assets/Resources"))
                {
                    UnityEditor.AssetDatabase.CreateFolder("Assets", "Resources");
                }

                repo = ScriptableObject.CreateInstance<VehicleRepository>();
                UnityEditor.AssetDatabase.CreateAsset(repo, "Assets/Resources/VehicleRepository.asset");
                UnityEditor.AssetDatabase.SaveAssets();
                
                // Debug.Log("Yeni VehicleRepository 'Assets/Resources/VehicleRepository.asset' konumunda oluşturuldu.");
            }

            repo.vehicles.Clear();

            // Alt-varlıkları (sub-assets) temizle (opsiyonel ama temizlik için)
            string path = UnityEditor.AssetDatabase.GetAssetPath(repo);
            var assets = UnityEditor.AssetDatabase.LoadAllAssetsAtPath(path);
            foreach (var asset in assets)
            {
                if (asset is VehicleAttributes)
                {
                    Object.DestroyImmediate(asset, true);
                }
            }

            // 1. Standart Araba
            var standard = ScriptableObject.CreateInstance<VehicleAttributes>();
            standard.name = "Standard_Car";
            standard.maxSpeedKmh = 150f;
            standard.zeroToHundredTime = 8.5f;
            standard.accelerationMs2 = 4.0f;
            standard.steeringSensitivity = 1.0f;
            standard.durability = 100f; // Standart dayanıklılık
            standard.vehicleClass = VehicleClass.Standard;
            standard.isLocked = false; // Standart araç açık başlar
            UnityEditor.AssetDatabase.AddObjectToAsset(standard, repo);
            repo.Create(standard);

            // 2. Spor Araba
            var sports = ScriptableObject.CreateInstance<VehicleAttributes>();
            sports.name = "Sports_Car";
            sports.maxSpeedKmh = 250f;
            sports.zeroToHundredTime = 4.2f;
            sports.accelerationMs2 = 7.5f;
            sports.steeringSensitivity = 1.8f;
            sports.durability = 70f;
            sports.vehicleClass = VehicleClass.Sports;
            sports.isLocked = true;
            UnityEditor.AssetDatabase.AddObjectToAsset(sports, repo);
            repo.Create(sports);

            // 3. Tank / Ağır Araç
            var heavy = ScriptableObject.CreateInstance<VehicleAttributes>();
            heavy.name = "Heavy_Truck";
            heavy.maxSpeedKmh = 100f;
            heavy.zeroToHundredTime = 12.0f;
            heavy.accelerationMs2 = 2.5f;
            heavy.steeringSensitivity = 0.6f;
            heavy.durability = 150f; // %50 artırılmış dayanıklılık (100 * 1.5)
            heavy.vehicleClass = VehicleClass.Heavy;
            heavy.isLocked = true;
            UnityEditor.AssetDatabase.AddObjectToAsset(heavy, repo);
            repo.Create(heavy);

            UnityEditor.EditorUtility.SetDirty(repo);
            UnityEditor.AssetDatabase.SaveAssets();

            // Debug.Log("Varsayılan araç verileri başarıyla oluşturuldu.");
        }
    }
}
