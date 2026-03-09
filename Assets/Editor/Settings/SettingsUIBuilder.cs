/**
 * @file SettingsUIBuilder.cs
 * @author Unity MCP Assistant
 * @date 2026-02-28
 * @last_update 2026-02-28
 * @description Unity Editor içerisinde profesyonel bir ayarlar paneli hiyerarşisini otomatik olarak oluşturan editör aracıdır.
 */

using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using Settings;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

namespace Settings.Editor
{
    /// <summary>
    /// Unity Editor menüsüne "Tools/Build Professional Settings Panel" seçeneğini ekleyen ve 
    /// tüm UI bileşenlerini (MVC dahil) otomatik inşa eden editör sınıfı.
    /// </summary>
    public class SettingsUIBuilder : EditorWindow
    {
        /// <summary>
        /// Ana menüden tetiklenen, ayarlar panelini ve gerekli tüm alt bileşenleri (AudioManager dahil) oluşturan ana metot.
        /// </summary>
        [MenuItem("Tools/Build Professional Settings Panel")]
        public static void BuildSettingsPanel()
        {
            // Sahnede bir Canvas olup olmadığını kontrol et
            GameObject canvasObj = GameObject.Find("Canvas");
            if (canvasObj == null)
            {
                // Debug.LogError("Canvas bulunamadı! Lütfen önce bir UI Canvas oluşturun.");
                return;
            }

            // EventSystem kontrolü ve Input System uyumluluğu
            var esObj = GameObject.Find("EventSystem");
            if (esObj == null)
            {
                esObj = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
                Undo.RegisterCreatedObjectUndo(esObj, "Create EventSystem");
            }
            else
            {
                var oldModule = esObj.GetComponent<StandaloneInputModule>();
                if (oldModule != null)
                {
                    Undo.DestroyObjectImmediate(oldModule);
                    Undo.AddComponent<InputSystemUIInputModule>(esObj);
                }
            }

            // 1. Ana Panelin Oluşturulması
            // RectTransform ve Image bileşenleri ile temel panel objesi
            GameObject settingsPanel = new("SettingsPanel", typeof(RectTransform), typeof(Image));
            settingsPanel.transform.SetParent(canvasObj.transform, false);
            
            // 1.1 AudioManager Kontrolü ve Oluşturulması
            // Eğer sahnede AudioManager yoksa, Singleton yapısına uygun olarak bir tane oluştur
            if (Object.FindFirstObjectByType<AudioManager>() == null)
            {
                GameObject audioManagerObj = new("AudioManager", typeof(AudioManager));
                Undo.RegisterCreatedObjectUndo(audioManagerObj, "AudioManager Oluştur");
            }

            // Panel Boyutlandırma: Tüm ekranı kaplayacak şekilde ayarla (Stretch)
            RectTransform panelRect = settingsPanel.GetComponent<RectTransform>();
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;

            // Arka Plan Görseli: Koyu, hafif şeffaf bir renk
            Image bg = settingsPanel.GetComponent<Image>();
            bg.color = new(0.1f, 0.1f, 0.12f, 0.95f);

            // 2. MVC Bileşenlerinin Eklenmesi
            // View ve Controller scriptleri doğrudan panel objesine eklenir
            SettingsView view = settingsPanel.AddComponent<SettingsView>();
            settingsPanel.AddComponent<SettingsController>();

            // 3. İçerik Taşıyıcı (Container) Oluşturma
            // Dikey hizalama (VerticalLayoutGroup) ve otomatik boyutlandırma (ContentSizeFitter) eklenir
            GameObject container = new("Container", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
            container.transform.SetParent(settingsPanel.transform, false);
            
            // Konteynerın ekranın ortasında %40'lık bir alanı kaplamasını sağlayan anchor ayarları
            RectTransform containerRect = container.GetComponent<RectTransform>();
            containerRect.anchorMin = new(0.3f, 0.1f);
            containerRect.anchorMax = new(0.7f, 0.9f);
            containerRect.offsetMin = Vector2.zero;
            containerRect.offsetMax = Vector2.zero;

            // Layout Ayarları: Elemanlar arası boşluk ve hizalama
            VerticalLayoutGroup layout = container.GetComponent<VerticalLayoutGroup>();
            layout.spacing = 30;
            layout.padding = new(20, 20, 20, 20);
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.childControlHeight = false;
            layout.childForceExpandHeight = false;

            // İçerik miktarına göre yüksekliği otomatik ayarla
            ContentSizeFitter fitter = container.GetComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            // 4. Başlık Metni
            CreateText(container.transform, "PROFESYONEL AYARLAR", 45, FontStyles.Bold, Color.white);

            // 5. Ayar Bölümlerinin İnşası (Müzik, SFX, Grafik)
            view.musicSlider = CreateProfessionalSlider(container.transform, "MÜZİK SES SEVİYESİ");
            view.musicToggle = CreateProfessionalToggle(container.transform, "MÜZİK SESİNİ KAPAT");

            view.sfxSlider = CreateProfessionalSlider(container.transform, "SFX SES SEVİYESİ");
            view.sfxToggle = CreateProfessionalToggle(container.transform, "SFX SESİNİ KAPAT");

            // 6. Geri Dönüş Butonu
            GameObject btnObj = new("BackButton", typeof(RectTransform), typeof(Image), typeof(Button));
            btnObj.transform.SetParent(container.transform, false);
            btnObj.GetComponent<RectTransform>().sizeDelta = new(250, 60);
            btnObj.GetComponent<Image>().color = new(0.8f, 0.2f, 0.2f, 1f); // Kırmızımsı bir renk
            view.backButton = btnObj.GetComponent<Button>();
            CreateText(btnObj.transform, "GERİ DÖN", 22, FontStyles.Bold, Color.white);

            // 7. MainMenuManager Bağlantısı (Otomatik Entegrasyon)
            // Sahnedeki MainMenuManager'ı bul ve oluşturulan paneli otomatik olarak referansla
            MainMenuManager mainMenu = Object.FindFirstObjectByType<MainMenuManager>();
            if (mainMenu != null)
            {
                mainMenu.settingsPanel = settingsPanel;
                EditorUtility.SetDirty(mainMenu); // Değişikliği kaydet
            }

            // Başlangıçta paneli gizle ve seçili yap
            settingsPanel.SetActive(false);
            Selection.activeGameObject = settingsPanel;
            Undo.RegisterCreatedObjectUndo(settingsPanel, "Profesyonel Ayarlar Paneli Oluştur");
            // Debug.Log("Unity MCP: Profesyonel Ayarlar Paneli oluşturuldu ve MainMenuManager'a bağlandı.");
        }

        /// <summary>
        /// Profesyonel görünümlü bir Slider (Kaydırıcı) yapısı oluşturur (Arka plan, Dolgu ve Tutamaç dahil).
        /// </summary>
        /// <param name="parent">Hangi objenin altına ekleneceği.</param>
        /// <param name="label">Slider başlığı.</param>
        /// <returns>Oluşturulan Slider bileşeni.</returns>
        private static Slider CreateProfessionalSlider(Transform parent, string label)
        {
            // Grup objesi ve dikey hizalama
            GameObject group = new(label + "_Group", typeof(RectTransform), typeof(VerticalLayoutGroup));
            group.transform.SetParent(parent, false);
            group.GetComponent<VerticalLayoutGroup>().childAlignment = TextAnchor.MiddleLeft;
            group.GetComponent<VerticalLayoutGroup>().spacing = 5;
            CreateText(group.transform, label, 18, FontStyles.Normal, Color.gray);

            // Slider Kök Objesi
            GameObject sliderRoot = new("Slider", typeof(RectTransform), typeof(Slider));
            sliderRoot.transform.SetParent(group.transform, false);
            sliderRoot.GetComponent<RectTransform>().sizeDelta = new(400, 30);
            Slider slider = sliderRoot.GetComponent<Slider>();

            // Arka Plan (Background)
            GameObject bg = new("Background", typeof(RectTransform), typeof(Image));
            bg.transform.SetParent(sliderRoot.transform, false);
            bg.GetComponent<RectTransform>().anchorMin = new(0, 0.25f);
            bg.GetComponent<RectTransform>().anchorMax = new(1, 0.75f);
            bg.GetComponent<RectTransform>().sizeDelta = Vector2.zero;
            bg.GetComponent<Image>().color = Color.black;

            // Fill Area
            GameObject fillArea = new("Fill Area", typeof(RectTransform));
            fillArea.transform.SetParent(sliderRoot.transform, false);
            fillArea.GetComponent<RectTransform>().anchorMin = new(0, 0.25f);
            fillArea.GetComponent<RectTransform>().anchorMax = new(1, 0.75f);
            fillArea.GetComponent<RectTransform>().sizeDelta = Vector2.zero;

            GameObject fill = new("Fill", typeof(RectTransform), typeof(Image));
            fill.transform.SetParent(fillArea.transform, false);
            fill.GetComponent<RectTransform>().sizeDelta = Vector2.zero;
            fill.GetComponent<Image>().color = new(0.2f, 0.6f, 1f, 1f); // Mavi tonu

            // Tutamaç Alanı (Handle Area)
            GameObject handleArea = new("Handle Area", typeof(RectTransform));
            handleArea.transform.SetParent(sliderRoot.transform, false);
            handleArea.GetComponent<RectTransform>().anchorMin = new(0, 0);
            handleArea.GetComponent<RectTransform>().anchorMax = new(1, 1);
            handleArea.GetComponent<RectTransform>().sizeDelta = Vector2.zero;

            GameObject handle = new("Handle", typeof(RectTransform), typeof(Image));
            handle.transform.SetParent(handleArea.transform, false);
            handle.GetComponent<RectTransform>().sizeDelta = new(30, 0);
            handle.GetComponent<Image>().color = Color.white;

            // Slider Referanslarının Atanması
            slider.fillRect = fill.GetComponent<RectTransform>();
            slider.handleRect = handle.GetComponent<RectTransform>();
            slider.targetGraphic = handle.GetComponent<Image>();
            slider.minValue = 0;
            slider.maxValue = 1;

            return slider;
        }

        /// <summary>
        /// Profesyonel görünümlü bir Toggle (Aç/Kapat) bileşeni oluşturur.
        /// </summary>
        /// <param name="parent">Üst obje.</param>
        /// <param name="label">Toggle metni.</param>
        /// <returns>Oluşturulan Toggle bileşeni.</returns>
        private static Toggle CreateProfessionalToggle(Transform parent, string label)
        {
            GameObject toggleRoot = new(label, typeof(RectTransform), typeof(Toggle));
            toggleRoot.transform.SetParent(parent, false);
            toggleRoot.GetComponent<RectTransform>().sizeDelta = new(250, 40);
            Toggle toggle = toggleRoot.GetComponent<Toggle>();

            // Toggle Kutusu (Background)
            GameObject bg = new("Background", typeof(RectTransform), typeof(Image));
            bg.transform.SetParent(toggleRoot.transform, false);
            bg.GetComponent<RectTransform>().sizeDelta = new(40, 40);
            bg.GetComponent<RectTransform>().anchoredPosition = new(-100, 0);
            bg.GetComponent<Image>().color = Color.black;

            // Onay İşareti (Checkmark)
            GameObject check = new("Checkmark", typeof(RectTransform), typeof(Image));
            check.transform.SetParent(bg.transform, false);
            check.GetComponent<RectTransform>().sizeDelta = new(30, 30);
            check.GetComponent<Image>().color = new(0.2f, 1f, 0.2f, 1f); // Yeşil tonu

            // Toggle Etiketi
            GameObject textObj = CreateText(toggleRoot.transform, label, 18, FontStyles.Normal, Color.white).gameObject;
            textObj.GetComponent<RectTransform>().anchoredPosition = new(30, 0);

            toggle.graphic = check.GetComponent<Image>();
            toggle.targetGraphic = bg.GetComponent<Image>();

            return toggle;
        }

        /// <summary>
        /// TextMeshProUGUI bileşeni içeren temel bir metin objesi oluşturur.
        /// </summary>
        /// <param name="parent">Üst obje.</param>
        /// <param name="text">Görüntülenecek metin.</param>
        /// <param name="size">Yazı boyutu.</param>
        /// <param name="style">Yazı stili (Bold, Italic vb.).</param>
        /// <param name="color">Yazı rengi.</param>
        /// <returns>Oluşturulan metin bileşeni.</returns>
        private static TextMeshProUGUI CreateText(Transform parent, string text, float size, FontStyles style, Color color)
        {
            GameObject obj = new("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
            obj.transform.SetParent(parent, false);
            TextMeshProUGUI tmp = obj.GetComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = size;
            tmp.fontStyle = style;
            tmp.color = color;
            tmp.alignment = TextAlignmentOptions.Center;
            return tmp;
        }
    }
}