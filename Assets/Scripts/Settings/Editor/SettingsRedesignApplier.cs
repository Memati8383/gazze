#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using Settings;

namespace Settings.Editor
{
    /// <summary>
    /// Sahneye modern ayarlar panelini olusturur ve gerekli bilesenleri baglar.
    /// </summary>
    public class SettingsRedesignApplier
    {
        [MenuItem("Tools/Gazze/Create Modern Settings")]
        public static void Apply()
        {
            // Canvas & EventSystem kontrolü
            Canvas canvas = Object.FindFirstObjectByType<Canvas>();
            if (canvas == null)
            {
                GameObject canvasGo = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler));
                canvas = canvasGo.GetComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            }
            if (Object.FindFirstObjectByType<EventSystem>() == null)
            {
                new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
            }

            // Panel oluştur veya bul
            GameObject panel = GameObject.Find("SettingsPanel");
            if (panel == null)
            {
                panel = new GameObject("SettingsPanel");
                panel.transform.SetParent(canvas.transform, false);
            }
            
            // Bileşenleri ekle ve sıfırdan kur
            var rebuilder = panel.GetComponent<SettingsVisualOverhaul>() ?? panel.AddComponent<SettingsVisualOverhaul>();
            rebuilder.BuildSettingsPanel();
            
            Selection.activeGameObject = panel;
            Debug.Log("Ayarlar ekranı en basit haliyle başarıyla oluşturuldu.");
        }
    }
}
#endif
