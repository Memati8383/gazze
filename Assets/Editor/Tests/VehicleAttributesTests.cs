using NUnit.Framework;
using UnityEngine;
using Gazze.Models;

namespace Gazze.Editor.Tests
{
    /// <summary>
    /// Araç özelliklerinin ve depo işlemlerinin sınır değerlerini test eden sınıf.
    /// </summary>
    public class VehicleAttributesTests
    {
        [Test]
        public void VehicleAttributes_Speed_BoundaryTest()
        {
            var attr = ScriptableObject.CreateInstance<VehicleAttributes>();
            attr.maxSpeedKmh = 350f; // Üst sınırı aşan değer
            attr.Validate();
            
            Assert.AreEqual(300f, attr.maxSpeedKmh, "Hız değeri 300 km/s sınırına çekilmelidir.");
            
            attr.maxSpeedKmh = -50f; // Alt sınırı aşan değer
            attr.Validate();
            Assert.AreEqual(0f, attr.maxSpeedKmh, "Hız değeri 0 km/s sınırına çekilmelidir.");
        }

        [Test]
        public void VehicleAttributes_Durability_BoundaryTest()
        {
            var attr = ScriptableObject.CreateInstance<VehicleAttributes>();
            attr.durability = 150f; // Üst sınırı aşan değer
            attr.Validate();
            Assert.AreEqual(100f, attr.durability, "Dayanıklılık %100 sınırına çekilmelidir.");
            
            attr.durability = -20f; // Alt sınırı aşan değer
            attr.Validate();
            Assert.AreEqual(0f, attr.durability, "Dayanıklılık %0 sınırına çekilmelidir.");
        }

        [Test]
        public void VehicleRepository_CRUD_Operations()
        {
            var repo = ScriptableObject.CreateInstance<VehicleRepository>();
            var car = ScriptableObject.CreateInstance<VehicleAttributes>();
            car.name = "TestCar";
            
            // Create
            repo.Create(car);
            Assert.Contains(car, repo.vehicles, "Araç başarıyla oluşturulmalıdır.");
            
            // Read
            var readCar = repo.Read("TestCar");
            Assert.AreEqual(car, readCar, "Araç ismiyle okunabilmelidir.");
            
            // Update
            var updatedCar = ScriptableObject.CreateInstance<VehicleAttributes>();
            updatedCar.name = "UpdatedCar";
            repo.UpdateVehicle("TestCar", updatedCar);
            Assert.AreEqual(updatedCar, repo.vehicles[0], "Araç başarıyla güncellenmelidir.");
            
            // Delete
            repo.Delete("UpdatedCar");
            Assert.IsEmpty(repo.vehicles, "Araç başarıyla silinmelidir.");
        }

    }
}
