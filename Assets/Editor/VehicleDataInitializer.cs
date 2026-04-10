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
                // Resources klasörünü kontrol et ve oluştur
                if (!System.IO.Directory.Exists("Assets/Resources"))
                {
                    UnityEditor.AssetDatabase.CreateFolder("Assets", "Resources");
                }

                repo = ScriptableObject.CreateInstance<VehicleRepository>();
                UnityEditor.AssetDatabase.CreateAsset(repo, "Assets/Resources/VehicleRepository.asset");
                UnityEditor.AssetDatabase.SaveAssets();
            }

            repo.vehicles.Clear();

            // Alt-varlıkları (sub-assets) temizle
            string path = UnityEditor.AssetDatabase.GetAssetPath(repo);
            var assets = UnityEditor.AssetDatabase.LoadAllAssetsAtPath(path);
            foreach (var asset in assets)
            {
                if (asset is VehicleAttributes)
                {
                    Object.DestroyImmediate(asset, true);
                }
            }

            // 0. Standart Araba 1
            var car0 = ScriptableObject.CreateInstance<VehicleAttributes>();
            car0.name = "Car0_Standard";
            car0.maxSpeedKmh = 140f;
            car0.zeroToHundredTime = 9.0f;
            car0.accelerationMs2 = 3.8f;
            car0.steeringSensitivity = 1.0f;
            car0.durability = 100f;
            car0.vehicleClass = VehicleClass.Standard;
            car0.isLocked = false;
            UnityEditor.AssetDatabase.AddObjectToAsset(car0, repo);
            repo.Create(car0);

            // 1. Standart Araba 2
            var car1 = ScriptableObject.CreateInstance<VehicleAttributes>();
            car1.name = "Car1_Standard";
            car1.maxSpeedKmh = 160f;
            car1.zeroToHundredTime = 8.0f;
            car1.accelerationMs2 = 4.2f;
            car1.steeringSensitivity = 1.1f;
            car1.durability = 100f;
            car1.vehicleClass = VehicleClass.Standard;
            car1.isLocked = true;
            UnityEditor.AssetDatabase.AddObjectToAsset(car1, repo);
            repo.Create(car1);

            // 2. Spor Araba 1
            var car2 = ScriptableObject.CreateInstance<VehicleAttributes>();
            car2.name = "Car2_Sports";
            car2.maxSpeedKmh = 230f;
            car2.zeroToHundredTime = 4.5f;
            car2.accelerationMs2 = 7.0f;
            car2.steeringSensitivity = 1.7f;
            car2.durability = 100f;
            car2.vehicleClass = VehicleClass.Sports;
            car2.isLocked = true;
            UnityEditor.AssetDatabase.AddObjectToAsset(car2, repo);
            repo.Create(car2);

            // 3. Spor Araba 2
            var car3 = ScriptableObject.CreateInstance<VehicleAttributes>();
            car3.name = "Car3_Sports";
            car3.maxSpeedKmh = 260f;
            car3.zeroToHundredTime = 4.0f;
            car3.accelerationMs2 = 8.0f;
            car3.steeringSensitivity = 1.9f;
            car3.durability = 100f;
            car3.vehicleClass = VehicleClass.Sports;
            car3.isLocked = true;
            UnityEditor.AssetDatabase.AddObjectToAsset(car3, repo);
            repo.Create(car3);

            // 4. Ağır Araç
            var car4 = ScriptableObject.CreateInstance<VehicleAttributes>();
            car4.name = "Car4_Heavy";
            car4.maxSpeedKmh = 110f;
            car4.zeroToHundredTime = 11.0f;
            car4.accelerationMs2 = 2.8f;
            car4.steeringSensitivity = 0.7f;
            car4.durability = 150f;
            car4.vehicleClass = VehicleClass.Heavy;
            car4.isLocked = true;
            UnityEditor.AssetDatabase.AddObjectToAsset(car4, repo);
            repo.Create(car4);

            UnityEditor.EditorUtility.SetDirty(repo);
            UnityEditor.AssetDatabase.SaveAssets();
        }
    }
}

