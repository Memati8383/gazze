using UnityEngine;
using UnityEngine.EventSystems;

namespace Gazze.UI
{
    /// <summary>
    /// Sahnedeki EventSystem çakışmalarını önleyen ve sadece bir tanesinin aktif kalmasını sağlayan yardımcı sınıf.
    /// </summary>
    [DefaultExecutionOrder(-100)] // Diğer scriptlerden önce çalışması için
    public class EventSystemSingleton : MonoBehaviour
    {
        private void Awake()
        {
            // Sahnedeki tüm EventSystem objelerini bul (Pasif olanlar dahil)
            EventSystem[] systems = Object.FindObjectsByType<EventSystem>(FindObjectsSortMode.None);
            
            if (systems.Length > 1)
            {
                // En az bir tane kalsın, diğerlerini yok et
                bool firstFound = false;
                foreach (var system in systems)
                {
                    if (!firstFound)
                    {
                        firstFound = true;
                        continue;
                    }
                    
                    Debug.LogWarning($"Çakışan EventSystem bulundu ve yok edildi: {system.gameObject.name}", system.gameObject);
                    Destroy(system.gameObject);
                }
            }
        }
    }
}
