using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Ana menüdeki tüm UI panellerini ve araç sergileme (showcase) sistemini kontrol eden yönetici sınıf.
/// </summary>
public class MainMenuManager : MonoBehaviour
{
    [Header("Paneller")]
    /// <summary> Ana menü seçeneklerinin bulunduğu panel. </summary>
    public GameObject mainOptionsPanel;
    /// <summary> Araç seçme arayüzünün bulunduğu panel. </summary>
    public GameObject carSelectionPanel;
    /// <summary> Ayarlar menüsünün bulunduğu panel. </summary>
    public GameObject settingsPanel;
    
    [Header("Geri Bildirim")]
    /// <summary> Aracın kilitli olduğunu gösteren ikon (Image). </summary>
    public Image lockIcon;
    /// <summary> Aracın satın alma maliyetini gösteren metin. </summary>
    public TextMeshProUGUI vehiclePriceText;

    [Header("Kilit & Satın Alma Yeni Sistem")]
    /// <summary> Toplam Krediyi menüde göstermek için opsiyonel metin. </summary>
    public TextMeshProUGUI totalKrediText;
    [Tooltip("Yetersiz bakiye gibi hata durumlarında çalınacak ses.")]
    public AudioClip errorSound;
    [Tooltip("Araç satın alma başarılı olduğunda çalınacak ses.")]
    public AudioClip buySound;
    [Tooltip("Yükseltme başarılı olduğunda çalınacak ses.")]
    public AudioClip upgradeSound;

    [Header("Yükseltme (Upgrade) Sistemi")]
    [Tooltip("Araç yükseltme kartlarını içeren panel.")]
    public GameObject upgradePanel;
    [Tooltip("Yükseltme seviyelerini gösteren metinler (sıra önemlidir).")]
    public TextMeshProUGUI[] upgradeLevelTexts; // Speed, Accel, Durability, Boost sırasıyla
    [Tooltip("Yükseltme maliyetlerini gösteren metinler.")]
    public TextMeshProUGUI[] upgradeCostTexts;
    [Tooltip("Her yükseltme tipi için tetikleyici butonlar.")]
    public Button[] upgradeButtons;

    [Header("Navigasyon Butonları (Otomatik Bağlanır)")]
    [Tooltip("Araç seçim panelini açan ana buton.")]
    public Button playButton;
    [Tooltip("Ayarlar panelini açan buton.")]
    public Button settingsButton;
    [Tooltip("Uygulamadan çıkış butonu.")]
    public Button exitButton;
    [Tooltip("Araç seçim panelinden ana menüye dönen buton.")]
    public Button backFromCarSelectionButton;
    [Tooltip("Seçili araçla oyunu başlatan buton.")]
    public Button startRaceButton;
    [Tooltip("Sonraki araca geçen buton.")]
    public Button nextCarButton;
    [Tooltip("Önceki araca geçen buton.")]
    public Button prevCarButton;

    [Header("Araç Seçimi")]
    /// <summary> Sergilenen aracın yerleştirileceği nokta. </summary>
    public Transform showcasePoint;
    /// <summary> Seçili aracın isminin gösterildiği metin. </summary>
    public TextMeshProUGUI carNameText;
    /// <summary> Araçlar için özel isim listesi. </summary>
    public string[] customCarNames;

    [Header("Rotasyon Ayarları")]
    /// <summary> Sergilenen aracın saniyedeki dönme hızı (derece). </summary>
    [Tooltip("Araç seçim ekranında aktif aracın dönme hızı.")]
    public float rotationSpeed = 30f;
    
    /// <summary> Seçilebilir araç objelerinin listesi. </summary>
    [Tooltip("Hierarchy panelindeki araç objelerini alır. Boş bırakırsanız ShowcasePoint içindeki tüm objeleri otomatik bulur.")]
    public GameObject[] selectableCars;
    
    private int currentIndex = 0;
    private float lastClickTime = 0f;

    private void Awake()
    {
        try 
        {
            // AudioManager'ı hemen başlat (Müziğin menüde de çalması için)
            if (Settings.AudioManager.Instance != null) { /* Singleton tetiklendi */ }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[MainMenuManager] AudioManager initialization failed: {e.Message}");
        }
    }

    private void Start()
    {
        // Tooltip'te belirtildiği gibi, eğer selectableCars boşsa ShowcasePoint içindekileri bul
        if ((selectableCars == null || selectableCars.Length == 0) && showcasePoint != null)
        {
            System.Collections.Generic.List<GameObject> cars = new System.Collections.Generic.List<GameObject>();
            foreach (Transform child in showcasePoint)
            {
                cars.Add(child.gameObject);
            }
            selectableCars = cars.ToArray();
        }

        // Başlangıçta panelleri ayarla
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (carSelectionPanel != null) carSelectionPanel.SetActive(false);
        if (mainOptionsPanel != null) mainOptionsPanel.SetActive(true);
        if (lockIcon != null) lockIcon.gameObject.SetActive(false);
        if (vehiclePriceText != null) vehiclePriceText.gameObject.SetActive(false);

        // Kayıtlı seçili araç indexini PlayerPrefs'ten al
        currentIndex = PlayerPrefs.GetInt("SelectedCarIndex", 0);
        
        // Dizi sınırlarını kontrol et
        if (selectableCars != null && selectableCars.Length > 0)
        {
            if (currentIndex >= selectableCars.Length) currentIndex = 0;
            
            // Tüm araçlara build'de pembe görünmemeleri için Curved World shader'ını uygula
            foreach (var car in selectableCars)
            {
                if (car != null) ApplyCurvedShaderToCar(car);
            }
        }
        
        UpdateCarDisplay();
        
        // Menü butonlarını otomatik bul ve bağla
        SetupNavigationButtons();

        // Yükseltme butonlarına dinleyicileri çalışma anında bağla (Editor'deki kayıpları önlemek için)
        if (upgradeButtons != null)
        {
            for (int i = 0; i < upgradeButtons.Length; i++)
            {
                if (upgradeButtons[i] == null) continue;
                int index = i; // Closure için kopyala
                upgradeButtons[i].onClick.RemoveAllListeners();
                upgradeButtons[i].onClick.AddListener(() => OnUpgradeClicked(index));
            }
        }
        
        if (PlayerPrefs.GetInt("OpenSettingsOnStart", 0) == 1)
        {
            PlayerPrefs.SetInt("OpenSettingsOnStart", 0);
            PlayerPrefs.Save();
            ShowSettings();
        }

        if (Gazze.UI.LocalizationManager.Instance != null)
        {
            Gazze.UI.LocalizationManager.Instance.OnLanguageChanged -= UpdateBalanceDisplay;
            Gazze.UI.LocalizationManager.Instance.OnLanguageChanged += UpdateBalanceDisplay;
        }
        UpdateBalanceDisplay();
    }

    private void OnDestroy()
    {
        if (Gazze.UI.LocalizationManager.Instance != null)
        {
            Gazze.UI.LocalizationManager.Instance.OnLanguageChanged -= UpdateBalanceDisplay;
        }
    }

    public void UpdateBalanceDisplay()
    {
        if (totalKrediText != null)
        {
            var loc = totalKrediText.GetComponent<Gazze.UI.LocalizedText>();
            if (loc != null) Destroy(loc);

            int totalKredi = PlayerPrefs.GetInt("TotalKredi", 0);
            
            // "BAKİYE" başlığı butonda zaten var, biz sadece ikon yanındaki miktarı süsleyelim.
            // Avant-Garde stil: Yüksek kontrastlı altın rengi ve kalın font.
            totalKrediText.text = $"<color=#ffcc00><b>{totalKredi:N0}</b></color> <size=70%>KREDİ</size>";
        }
    }

    /// <summary>
    /// Navigasyon butonlarını hiyerarşide arayıp bulur ve olaylarını bağlar.
    /// </summary>
    private void SetupNavigationButtons()
    {
        // Ana Opsiyonlar Paneli Butonları
        if (mainOptionsPanel != null)
        {
            // Yeni hiyerarşide (MenuGroup altında) buttonları bulmak için name-based check
            Button[] allButtons = mainOptionsPanel.GetComponentsInChildren<Button>(true);
            foreach (var b in allButtons)
            {
                if (b.name == "Btn_OyunaBasla") playButton = b;
                else if (b.name == "Btn_Ayarlar") settingsButton = b;
                else if (b.name == "Btn_Cikis") exitButton = b;
            }
        }

        // Araç Seçim Paneli Butonları
        if (carSelectionPanel != null)
        {
            if (startRaceButton == null) startRaceButton = carSelectionPanel.transform.Find("Btn_SurusBasla")?.GetComponent<Button>();
            if (backFromCarSelectionButton == null) backFromCarSelectionButton = carSelectionPanel.transform.Find("Btn_Geri")?.GetComponent<Button>();
            if (prevCarButton == null) prevCarButton = carSelectionPanel.transform.Find("Btn_Prev")?.GetComponent<Button>();
            if (nextCarButton == null) nextCarButton = carSelectionPanel.transform.Find("Btn_Next")?.GetComponent<Button>();
            
            // Bakiyeyi (Para Yazısı) otomatik bul
            if (totalKrediText == null)
            {
                var priceParent = carSelectionPanel.transform.Find("para yazısı arka plan");
                if (priceParent != null)
                {
                    totalKrediText = priceParent.Find("para yazısı")?.GetComponent<TextMeshProUGUI>();
                }
            }
        }

        // Event Dinleyicilerini Ekle
        if (playButton != null) { playButton.onClick.RemoveAllListeners(); playButton.onClick.AddListener(ShowCarSelection); }
        if (settingsButton != null) { settingsButton.onClick.RemoveAllListeners(); settingsButton.onClick.AddListener(ShowSettings); }
        if (exitButton != null) { exitButton.onClick.RemoveAllListeners(); exitButton.onClick.AddListener(OnExitClicked); }
        
        if (startRaceButton != null) { startRaceButton.onClick.RemoveAllListeners(); startRaceButton.onClick.AddListener(StartGame); }
        if (backFromCarSelectionButton != null) { backFromCarSelectionButton.onClick.RemoveAllListeners(); backFromCarSelectionButton.onClick.AddListener(ShowMainOptions); }
        if (prevCarButton != null) { prevCarButton.onClick.RemoveAllListeners(); prevCarButton.onClick.AddListener(PreviousCar); }
        if (nextCarButton != null) { nextCarButton.onClick.RemoveAllListeners(); nextCarButton.onClick.AddListener(NextCar); }
    }

    private void Update()
    {
        // Sadece araç seçimi paneli aktifken ve bir araç seçiliyse aracı dünyanın Y (Up) ekseni etrafında döndür
        if (carSelectionPanel != null && carSelectionPanel.activeInHierarchy && 
            selectableCars != null && selectableCars.Length > 0 && 
            currentIndex >= 0 && currentIndex < selectableCars.Length && 
            selectableCars[currentIndex] != null)
        {
            selectableCars[currentIndex].transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);
        }
    }

    private Coroutine panelTransitionCoroutine;

    private void SwitchToPanel(GameObject targetPanel)
    {
        if (panelTransitionCoroutine != null) StopCoroutine(panelTransitionCoroutine);
        
        // Failsafe: Önceki transition yarıda kaldıysa alpha değerlerini sıfırla
        ResetPanelAlphas();
        
        // Araç görünürlüğünü panel açılışından önce anında güncelle
        UpdateCarDisplay(targetPanel == carSelectionPanel);
        
        panelTransitionCoroutine = StartCoroutine(PanelTransitionRoutine(targetPanel));
    }

    private void ResetPanelAlphas()
    {
        GameObject[] allPanels = { mainOptionsPanel, carSelectionPanel, settingsPanel };
        foreach (var p in allPanels)
        {
            if (p != null)
            {
                CanvasGroup cg = p.GetComponent<CanvasGroup>();
                if (cg != null) cg.alpha = 1f;
            }
        }
    }

    private IEnumerator PanelTransitionRoutine(GameObject targetPanel)
    {
        GameObject[] allPanels = { mainOptionsPanel, carSelectionPanel, settingsPanel };
        
        // Fade out active panels
        foreach (var p in allPanels)
        {
            if (p != null && p.activeSelf && p != targetPanel)
            {
                CanvasGroup cg = p.GetComponent<CanvasGroup>();
                if (cg == null) cg = p.AddComponent<CanvasGroup>();
                
                float elapsedOut = 0;
                while (elapsedOut < 0.15f)
                {
                    elapsedOut += Time.unscaledDeltaTime;
                    cg.alpha = Mathf.Lerp(1, 0, elapsedOut / 0.15f);
                    yield return null;
                }
                p.SetActive(false);
                cg.alpha = 1; // reset for next time
            }
        }

        // Fade in target panel
        if (targetPanel != null)
        {
            targetPanel.SetActive(true);
            CanvasGroup cg = targetPanel.GetComponent<CanvasGroup>();
            if (cg == null) cg = targetPanel.AddComponent<CanvasGroup>();
            
            cg.alpha = 0;
            float elapsedIn = 0;
            while(elapsedIn < 0.2f)
            {
                elapsedIn += Time.unscaledDeltaTime;
                cg.alpha = Mathf.Lerp(0, 1, Mathf.Sin(elapsedIn / 0.2f * Mathf.PI * 0.5f));
                yield return null;
            }
            cg.alpha = 1;

            // Panel tamamen açılınca bakiyeyi güncelle (eğer LocalizedText falan varsa güvene almak için)
            UpdateBalanceDisplay();
        }
    }

    /// <summary>
    /// Araç seçim ekranını gösterir.
    /// </summary>
    public void ShowCarSelection()
    {
        SwitchToPanel(carSelectionPanel);
    }

    /// <summary>
    /// Ana menü seçeneklerini gösterir ve araçları gizler.
    /// </summary>
    public void ShowMainOptions()
    {
        SwitchToPanel(mainOptionsPanel);
    }

    /// <summary>
    /// Ayarlar panelini gösterir.
    /// </summary>
    public void ShowSettings()
    {
        SwitchToPanel(settingsPanel);
    }

    /// <summary> Ayarlar butonuna tıklandığında çağrılır. </summary>
    public void OnSettingsClicked()
    {
        ShowSettings();
    }

    /// <summary>
    /// Oyundan çıkış yapar.
    /// </summary>
    public void OnExitClicked()
    {
        // Debug.Log("Çıkış yapılıyor.");
        Application.Quit();
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }

    /// <summary>
    /// Sonraki araca geçer.
    /// </summary>
    public void NextCar()
    {
        // Debounce: Çok hızlı üst üste tıklamaları veya çift tetiklenmeyi önle
        if (Time.unscaledTime - lastClickTime < 0.2f) return;
        lastClickTime = Time.unscaledTime;

        if (Settings.AudioManager.Instance != null) Settings.AudioManager.Instance.PlayClickSound();
        if (selectableCars == null || selectableCars.Length == 0) return;
        
        currentIndex++;
        if (currentIndex >= selectableCars.Length) currentIndex = 0;
        
        Settings.HapticManager.Light();
        UpdateCarDisplay();
    }

    /// <summary>
    /// Önceki araca geçer.
    /// </summary>
    public void PreviousCar()
    {
        // Debounce: Çok hızlı üst üste tıklamaları veya çift tetiklenmeyi önle
        if (Time.unscaledTime - lastClickTime < 0.2f) return;
        lastClickTime = Time.unscaledTime;

        if (Settings.AudioManager.Instance != null) Settings.AudioManager.Instance.PlayClickSound();
        if (selectableCars == null || selectableCars.Length == 0) return;
        
        currentIndex--;
        if (currentIndex < 0) currentIndex = selectableCars.Length - 1;
        
        Settings.HapticManager.Light();
        UpdateCarDisplay();
    }

    private Gazze.Models.VehicleAttributes GetCarAttributes(int index)
    {
        if (index < 0) return null;

        // 1. Sahnede bulunan araç prefabı üzerinden özellikleri almayı dene
        if (selectableCars != null && index < selectableCars.Length && selectableCars[index] != null)
        {
            var vehicleComp = selectableCars[index].GetComponent<Gazze.Vehicles.Vehicle>();
            if (vehicleComp != null && vehicleComp.attributes != null)
            {
                return vehicleComp.attributes;
            }
        }

        // 2. Bulunamadıysa Repository listesinden almayı dene
        if (Gazze.Models.VehicleRepository.Instance != null && Gazze.Models.VehicleRepository.Instance.vehicles.Count > index)
        {
            return Gazze.Models.VehicleRepository.Instance.vehicles[index];
        }

        return null;
    }

    /// <summary>
    /// Bağımsız yeni fiyat hesaplama sistemi (Modellere göre uyarlandı)
    /// </summary>
    private int GetCarPrice(int index)
    {
        if (index <= 0) return 0; // İlk araç ücretsiz
        
        Gazze.Models.VehicleAttributes attributes = GetCarAttributes(index);

        // Model sınıflarına göre fiyatlandırma
        if (attributes != null)
        {
            switch (attributes.vehicleClass)
            {
                case Gazze.Models.VehicleClass.Standard: return 1000;
                case Gazze.Models.VehicleClass.Heavy: return 2000;
                case Gazze.Models.VehicleClass.Sports: return 3000;
            }
        }

        // Failsafe (Güvenlik yedeği)
        return index * 1000 + (index * index * 500); 
    }

    private Coroutine carSwitchCoroutine;
    private Coroutine lockUiCoroutine;

    /// <summary>
    /// Seçili aracı görünür yapar ve ismini UI'da günceller.
    /// </summary>
    private void UpdateCarDisplay(bool? forceGarageVisible = null)
    {
        if (selectableCars == null || selectableCars.Length == 0) return;

        // Sadece seçim paneli açıkken seçili aracı göster
        bool shouldBeVisible = forceGarageVisible ?? (carSelectionPanel != null && carSelectionPanel.activeSelf);

        for (int i = 0; i < selectableCars.Length; i++)
        {
            if (selectableCars[i] != null)
            {
                bool wasActive = selectableCars[i].activeSelf;
                bool shouldActivate = shouldBeVisible && i == currentIndex;
                
                selectableCars[i].SetActive(shouldActivate);

                // Add pop-up scale animation when switching cars
                if (!wasActive && shouldActivate)
                {
                    if (carSwitchCoroutine != null) StopCoroutine(carSwitchCoroutine);
                    carSwitchCoroutine = StartCoroutine(CarEntranceAnimation(selectableCars[i].transform));
                }
            }
        }

        // Yeni Fiyat ve Kilit Sistemi: PlayerPrefs üzerinden yönetilir
        bool isLocked = false;
        var attributes = GetCarAttributes(currentIndex);
        if (attributes != null && attributes.isLocked && currentIndex > 0)
        {
            isLocked = PlayerPrefs.GetInt($"CarUnlocked_{currentIndex}", 0) == 0;
        }
        int purchaseCost = GetCarPrice(currentIndex);
        
        UpdateBalanceDisplay();

        bool showLockUI = shouldBeVisible && isLocked;

        if (lockIcon != null)
        {
            bool wasLockActive = lockIcon.gameObject.activeSelf;
            lockIcon.gameObject.SetActive(showLockUI);
            
            if (showLockUI && !wasLockActive)
            {
                if (lockUiCoroutine != null) StopCoroutine(lockUiCoroutine);
                lockUiCoroutine = StartCoroutine(UIEntranceAnimation(lockIcon.transform, vehiclePriceText != null ? vehiclePriceText.transform : null));
            }
        }

        if (vehiclePriceText != null)
        {
            vehiclePriceText.gameObject.SetActive(showLockUI);
            if (isLocked)
            {
                string priceLabel = Gazze.UI.LocalizationManager.Instance != null ? Gazze.UI.LocalizationManager.Instance.GetTranslation("Garage_Price") : "FİYAT";
                string currencyLabel = Gazze.UI.LocalizationManager.Instance != null ? Gazze.UI.LocalizationManager.Instance.GetTranslation("Game_Credit") : "KREDİ";
                vehiclePriceText.text = $"<size=80%><color=#cccccc>{priceLabel}:</color></size>\n<b><color=#ffcc00>{purchaseCost:N0}</color></b>\n<size=70%><color=#cccccc>{currencyLabel}</color></size>";
            }
        }

        if (carNameText != null)
        {
            // Panel görünürse ismi güncelle ve text objesini aktif et
            carNameText.gameObject.SetActive(shouldBeVisible);
            
            if (shouldBeVisible)
            {
                // Özel isim varsa kullan, yoksa obje adından türet
                if (customCarNames != null && currentIndex >= 0 && currentIndex < customCarNames.Length && !string.IsNullOrEmpty(customCarNames[currentIndex]))
                {
                    carNameText.text = customCarNames[currentIndex].ToUpper();
                }
                else if (currentIndex >= 0 && currentIndex < selectableCars.Length && selectableCars[currentIndex] != null)
                {
                    // İsmi temizle: (Clone) ekini ve alt çizgileri kaldır
                    string rawName = selectableCars[currentIndex].name.Replace("(Clone)", "").Replace("_", " ").Trim();
                    
                    // Eğer isim "GAZZE SULTAN" gibi bir prefix içeriyorsa ve boş kalmayacaksa ilk kelimeyi atla
                    int spaceIdx = rawName.IndexOf(" ");
                    string finalName = rawName;
                    
                    if (spaceIdx != -1 && spaceIdx < rawName.Length - 1)
                    {
                        string stripped = rawName.Substring(spaceIdx + 1).Trim();
                        if (!string.IsNullOrEmpty(stripped)) finalName = stripped;
                    }
                    
                    carNameText.text = finalName.ToUpper();
                }
            }
        }

        // Yükseltme Paneli Güncelleme
        UpdateUpgradeUI(isLocked, shouldBeVisible);
        
        // Araç özellikleri başlığını gizle (Çünkü Yukarıdaki panel zaten yükseltmeleri/özellikleri gösteriyor)
        // Bu daha ferah ve modern (Avant-Garde) bir görünüm sağlar.
        var titleGo = carSelectionPanel != null ? carSelectionPanel.transform.Find("CarTitleText") : null;
        if (titleGo != null) titleGo.gameObject.SetActive(false);
    }

    /// <summary>
    /// Yükseltme arayüzünü (panel, seviyeler, maliyetler) günceller.
    /// </summary>
    private void UpdateUpgradeUI(bool carIsLocked, bool isGarageVisible)
    {
        // Araç kilitli olsa bile istatistiklerini/yükseltmelerini görebilmeliyiz. Sadece garaj kapalıyken gizle.
        if (upgradePanel != null) upgradePanel.SetActive(isGarageVisible);

        if (!isGarageVisible) return;

        var attributes = GetCarAttributes(currentIndex);

        // SegmentHolder referanslarını topla
        // Serialization kayıplarını (Component sıfırlanması vb) önlemek için hiyerarşiyi dinamik kullanıyoruz
        Transform cardRow = upgradePanel != null ? upgradePanel.transform.Find("CardRow") : null;

        // Her tip için seviye ve maliyet bilgilerini güncelle
        int upgradeTypeCount = System.Enum.GetValues(typeof(Gazze.Models.VehicleUpgradeManager.UpgradeType)).Length;
        for (int i = 0; i < upgradeTypeCount; i++) 
        {
            if (i >= 5) break; // Şu anki UI tasarımı max 5 elementi destekleyebilir varsayalım

            Gazze.Models.VehicleUpgradeManager.UpgradeType type = (Gazze.Models.VehicleUpgradeManager.UpgradeType)i;
            int level = Gazze.Models.VehicleUpgradeManager.GetUpgradeLevel(currentIndex, type);
            int cost  = Gazze.Models.VehicleUpgradeManager.GetUpgradeCost(currentIndex, type);
            bool isMax = level >= Gazze.Models.VehicleUpgradeManager.MaxUpgradeLevel;

            // ──── Progress Bar Segmentleri (Dinamik Getirme) ────
            if (cardRow != null && i < cardRow.childCount)
            {
                Transform cardGo = cardRow.GetChild(i);
                Transform progGo = cardGo.Find("Prog");
                if (progGo != null)
                {
                    var segs = progGo.GetComponentsInChildren<UnityEngine.UI.Image>();
                    Color[] accents = {
                        new Color32(0,   210, 255, 255), // Speed - Blue
                        new Color32(60,  245, 110, 255), // Accel - Green
                        new Color32(255,  75,  80, 255), // Durability - Red
                        new Color32(255, 148,  20, 255), // Boost Duration - Orange
                        new Color32(0,   255, 230, 255)  // Boost Refill - Cyan
                    };
                    Color fill = accents[Mathf.Clamp(i, 0, accents.Length - 1)];
                    Color goldMax = new Color32(255, 200, 50, 255);
                    Color empty = new Color32(50, 56, 85, 255);

                    for (int s = 0; s < segs.Length; s++)
                    {
                        if (segs[s] != null)
                        {
                            segs[s].color = (s < level)
                                ? (level >= Gazze.Models.VehicleUpgradeManager.MaxUpgradeLevel ? goldMax : fill)
                                : empty;
                        }
                    }
                }
            }

            // ──── Seviye Metni ────
            if (upgradeLevelTexts != null && i < upgradeLevelTexts.Length && upgradeLevelTexts[i] != null)
            {
                string valueSuffix = "";
                float baseVal = 0;

                if (attributes != null)
                {
                    switch (type)
                    {
                        case Gazze.Models.VehicleUpgradeManager.UpgradeType.Speed:
                            baseVal = attributes.maxSpeedKmh;
                            valueSuffix = Gazze.UI.LocalizationManager.Instance != null ? Gazze.UI.LocalizationManager.Instance.GetTranslation("Game_Unit_Kmh") : " km/h";
                            break;
                        case Gazze.Models.VehicleUpgradeManager.UpgradeType.Acceleration:
                            baseVal = attributes.accelerationMs2;
                            valueSuffix = Gazze.UI.LocalizationManager.Instance != null ? Gazze.UI.LocalizationManager.Instance.GetTranslation("Game_Unit_Ms2") : " m/s²";
                            break;
                        case Gazze.Models.VehicleUpgradeManager.UpgradeType.Durability:
                            baseVal = attributes.durability;
                            valueSuffix = Gazze.UI.LocalizationManager.Instance != null ? Gazze.UI.LocalizationManager.Instance.GetTranslation("Game_Unit_Hp") : " HP";
                            break;
                        case Gazze.Models.VehicleUpgradeManager.UpgradeType.BoostDuration:
                            baseVal = 2.0f;
                            valueSuffix = Gazze.UI.LocalizationManager.Instance != null ? Gazze.UI.LocalizationManager.Instance.GetTranslation("Game_Unit_Sec") : "s";
                            break;
                        case Gazze.Models.VehicleUpgradeManager.UpgradeType.BoostRefillRate:
                            baseVal = 0.2f; // %20/sn
                            valueSuffix = Gazze.UI.LocalizationManager.Instance != null ? Gazze.UI.LocalizationManager.Instance.GetTranslation("Game_Unit_PerSec") : "/s";
                            break;
                    }
                }

                float upgradedVal = Gazze.Models.VehicleUpgradeManager.GetUpgradedValue(currentIndex, type, baseVal);
                string valStr = type == Gazze.Models.VehicleUpgradeManager.UpgradeType.BoostDuration
                    ? upgradedVal.ToString("F1")
                    : upgradedVal.ToString("F0");
                
                string lvlStr = Gazze.UI.LocalizationManager.Instance != null ? Gazze.UI.LocalizationManager.Instance.GetTranslation("Game_LevelShort") : "LVL";
                upgradeLevelTexts[i].text = $"{lvlStr} {level} <size=80%>({valStr}{valueSuffix})</size>";
            }

            // ──── Maliyet Metni ────
            if (upgradeCostTexts != null && i < upgradeCostTexts.Length && upgradeCostTexts[i] != null)
            {
                string maxLabel = Gazze.UI.LocalizationManager.Instance != null ? Gazze.UI.LocalizationManager.Instance.GetTranslation("Game_Max") : "MAX";
                upgradeCostTexts[i].text = isMax ? maxLabel : $"{cost:N0}";
            }

            // ──── Buton Durumu & Rengi ────
            if (upgradeButtons != null && i < upgradeButtons.Length && upgradeButtons[i] != null)
            {
                // Araç kilitliyse buton pasif ama panel görünür
                upgradeButtons[i].interactable = !isMax && !carIsLocked;

                // MAX olduğunda altın, kilitliyse gri, normalse mavi yap
                var btnImg = upgradeButtons[i].GetComponent<UnityEngine.UI.Image>();
                if (btnImg != null)
                {
                    if (isMax) btnImg.color = new Color32(255, 180, 0, 255); // Altın
                    else if (carIsLocked) btnImg.color = new Color32(100, 100, 120, 255); // Kilitli Rengi
                    else btnImg.color = new Color32(0, 180, 255, 255); // Aktif Mavi
                }
            }
        }
    }

    /// <summary>
    /// Mevcut aracı belirli bir kategoride yükseltir.
    /// </summary>
    public void OnUpgradeClicked(int typeIndex)
    {
        Gazze.Models.VehicleUpgradeManager.UpgradeType type = (Gazze.Models.VehicleUpgradeManager.UpgradeType)typeIndex;
        int cost = Gazze.Models.VehicleUpgradeManager.GetUpgradeCost(currentIndex, type);
        int totalKredi = PlayerPrefs.GetInt("TotalKredi", 0);

        if (Gazze.Models.VehicleUpgradeManager.Upgrade(currentIndex, type))
        {
            // Başarılı Yükseltme
            Debug.Log($"<color=green>Gazze:</color> {type} başarıyla yükseltildi! Kalan Kredi: {PlayerPrefs.GetInt("TotalKredi", 0)}");
            if (upgradeSound != null && Settings.AudioManager.Instance != null)
                Settings.AudioManager.Instance.PlaySFX(upgradeSound);
            else if (buySound != null && Settings.AudioManager.Instance != null)
                Settings.AudioManager.Instance.PlaySFX(buySound);
            
            Settings.HapticManager.Medium();

            // Seviye barı animasyonu
            int newLevel = Gazze.Models.VehicleUpgradeManager.GetUpgradeLevel(currentIndex, type);
            StartCoroutine(AnimateUpgradeBar(typeIndex, newLevel));
            
            // Araç üzerinde animasyon (kartın accent rengini kullan)
            StartCoroutine(AnimateVehicleOnUpgrade(typeIndex));
            
            UpdateCarDisplay();
        }
        else
        {
            // Yetersiz Kredi veya Max Seviye
            Debug.LogWarning($"<color=orange>Gazze:</color> Yükseltme başarısız! Tip: {type}, Gerekli: {cost}, Mevcut: {totalKredi}");
            Settings.HapticManager.Light(); // Hata için kısa bir uyarı
            StartCoroutine(FlashPriceTextError());
        }
    }

    /// <summary>
    /// Yükseltme sonrası seviye barının segmentlerini sırayla dolduran ve punch efekti uygulayan animasyon.
    /// </summary>
    private IEnumerator AnimateUpgradeBar(int cardIndex, int newLevel)
    {
        Transform cardRow = upgradePanel != null ? upgradePanel.transform.Find("CardRow") : null;
        if (cardRow == null || cardIndex >= cardRow.childCount) yield break;

        Transform cardGo = cardRow.GetChild(cardIndex);
        Transform progGo = cardGo.Find("Prog");
        if (progGo == null) yield break;

        var segs = progGo.GetComponentsInChildren<Image>();
        if (segs == null || segs.Length == 0) yield break;

        Color[] accents = {
            new Color32(0,   210, 255, 255),
            new Color32(60,  245, 110, 255),
            new Color32(255,  75,  80, 255),
            new Color32(255, 148,  20, 255),
            new Color32(0,   255, 230, 255)
        };
        Color fillColor = accents[Mathf.Clamp(cardIndex, 0, accents.Length - 1)];
        Color goldMax = new Color32(255, 200, 50, 255);
        bool isMaxLevel = newLevel >= Gazze.Models.VehicleUpgradeManager.MaxUpgradeLevel;
        Color targetColor = isMaxLevel ? goldMax : fillColor;

        // Yeni doldurulan segment indeksi (0-indexed)
        int newSegIndex = newLevel - 1;
        if (newSegIndex < 0 || newSegIndex >= segs.Length) yield break;

        // 1. Yeni segmenti animasyonla doldur
        Image seg = segs[newSegIndex];
        RectTransform segRT = seg.GetComponent<RectTransform>();
        Vector3 originalScale = segRT.localScale;
        Color emptyColor = new Color32(50, 56, 85, 255);

        // Flash beyaz → hedef renge geçiş
        float fillDuration = 0.3f;
        float elapsed = 0f;
        segRT.localScale = originalScale * 1.4f; // Büyüterek başla

        while (elapsed < fillDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / fillDuration;
            float easedT = 1f - Mathf.Pow(1f - t, 3f); // Ease out cubic

            // Beyazdan hedef renge geçiş
            seg.color = Color.Lerp(Color.white, targetColor, easedT);
            // Scale punch: büyükten normale
            segRT.localScale = Vector3.Lerp(originalScale * 1.4f, originalScale, easedT);

            yield return null;
        }
        seg.color = targetColor;
        segRT.localScale = originalScale;

        // 2. MAX ise tüm segmentleri altın dalgası
        if (isMaxLevel)
        {
            for (int i = 0; i < segs.Length; i++)
            {
                if (segs[i] == null) continue;
                StartCoroutine(GoldWavePulse(segs[i], i * 0.06f));
            }
        }

        // 3. Kartın tamamına scale punch
        RectTransform cardRT = cardGo.GetComponent<RectTransform>();
        if (cardRT != null)
        {
            Vector3 cardOrigScale = cardRT.localScale;
            elapsed = 0f;
            float punchDuration = 0.25f;

            while (elapsed < punchDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = elapsed / punchDuration;
                // Overshoot easeOutBack
                float s = 1f + Mathf.Sin(t * Mathf.PI) * 0.08f;
                cardRT.localScale = cardOrigScale * s;
                yield return null;
            }
            cardRT.localScale = cardOrigScale;
        }
    }

    /// <summary>
    /// MAX seviye altın dalga efekti.
    /// </summary>
    private IEnumerator GoldWavePulse(Image seg, float delay)
    {
        yield return new WaitForSecondsRealtime(delay);

        Color goldMax = new Color32(255, 200, 50, 255);
        Color brightGold = new Color32(255, 240, 150, 255);
        RectTransform rt = seg.GetComponent<RectTransform>();
        Vector3 orig = rt.localScale;

        float dur = 0.2f;
        float elapsed = 0f;

        while (elapsed < dur)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / dur;
            seg.color = Color.Lerp(brightGold, goldMax, t);
            rt.localScale = Vector3.Lerp(orig * 1.2f, orig, t);
            yield return null;
        }

        seg.color = goldMax;
        rt.localScale = orig;
    }

    /// <summary>
    /// Yükseltme sonrası araç üzerinde parlama ve sallanma animasyonu.
    /// Kartın accent rengine göre glow rengi değişir.
    /// </summary>
    private IEnumerator AnimateVehicleOnUpgrade(int cardIndex)
    {
        if (selectableCars == null || currentIndex < 0 || currentIndex >= selectableCars.Length) yield break;
        GameObject car = selectableCars[currentIndex];
        if (car == null) yield break;

        Transform carT = car.transform;
        Renderer[] renderers = car.GetComponentsInChildren<Renderer>();

        // Orijinal referansları kaydet
        Vector3 originalScale = carT.localScale;
        Quaternion originalRot = carT.localRotation;
        
        // Rendererların orijinal renkleri
        var originalColors = new Dictionary<Renderer, Color[]>();
        foreach (var r in renderers)
        {
            if (r == null) continue;
            Color[] colors = new Color[r.materials.Length];
            for (int m = 0; m < r.materials.Length; m++)
            {
                Material mat = r.materials[m];
                colors[m] = mat.HasProperty("_BaseColor") ? mat.GetColor("_BaseColor") : 
                           (mat.HasProperty("_Color") ? mat.color : Color.white);
            }
            originalColors[r] = colors;
        }

        // Faz 1: Beyaz flaş + scale punch (0.15s)
        float flashDuration = 0.15f;
        float elapsed = 0f;
        Color flashColor = new Color(1f, 1f, 1f, 1f);

        while (elapsed < flashDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / flashDuration;

            // Beyaza doğru flaş
            foreach (var r in renderers)
            {
                if (r == null || !originalColors.ContainsKey(r)) continue;
                for (int m = 0; m < r.materials.Length; m++)
                {
                    Material mat = r.materials[m];
                    Color lerped = Color.Lerp(originalColors[r][m], flashColor, 1f - t);
                    if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", lerped);
                    else if (mat.HasProperty("_Color")) mat.color = lerped;
                }
            }

            // Scale punch up
            float s = 1f + Mathf.Sin(t * Mathf.PI) * 0.12f;
            carT.localScale = originalScale * s;

            yield return null;
        }

        // Faz 2: Kartın accent renginde glow + hafif sallanma (0.35s)
        Color[] glowAccents = {
            new Color32(0,   230, 255, 255), // Cyan   – Hız
            new Color32(60,  255, 120, 255), // Green  – İvme
            new Color32(255,  90,  90, 255), // Red    – Dayanıklılık
            new Color32(255, 168,  40, 255), // Orange – Boost
        };
        Color upgradeGlow = glowAccents[Mathf.Clamp(cardIndex, 0, glowAccents.Length - 1)];
        float glowDuration = 0.35f;
        elapsed = 0f;

        while (elapsed < glowDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / glowDuration;
            float easedT = 1f - Mathf.Pow(1f - t, 2f);

            // Glow → orijinal renge geri dön
            foreach (var r in renderers)
            {
                if (r == null || !originalColors.ContainsKey(r)) continue;
                for (int m = 0; m < r.materials.Length; m++)
                {
                    Material mat = r.materials[m];
                    Color mid = Color.Lerp(upgradeGlow, originalColors[r][m], easedT);
                    if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", mid);
                    else if (mat.HasProperty("_Color")) mat.color = mid;
                }
            }

            // Hafif Y-ekseni sallanma
            float wobble = Mathf.Sin(elapsed * 25f) * (1f - easedT) * 5f;
            carT.localRotation = originalRot * Quaternion.Euler(0, wobble, 0);

            // Scale ease-back normalize
            carT.localScale = Vector3.Lerp(originalScale * 1.05f, originalScale, easedT);

            yield return null;
        }

        // Reset
        carT.localScale = originalScale;
        carT.localRotation = originalRot;
        foreach (var r in renderers)
        {
            if (r == null || !originalColors.ContainsKey(r)) continue;
            for (int m = 0; m < r.materials.Length; m++)
            {
                r.materials[m].color = originalColors[r][m];
            }
        }
    }

    private IEnumerator CarEntranceAnimation(Transform t)
    {
        t.localScale = Vector3.zero;
        float duration = 0.35f;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float percent = elapsed / duration;
            // Easing Out Back-ish calculation
            float s = 1f - Mathf.Pow(1f - percent, 3f); 
            float scaleAmount = s * (1f + Mathf.Sin(percent * Mathf.PI) * 0.15f);
            t.localScale = Vector3.one * scaleAmount;
            yield return null;
        }
        t.localScale = Vector3.one;
    }

    private IEnumerator UIEntranceAnimation(Transform t1, Transform t2)
    {
        float duration = 0.25f;
        float elapsed = 0f;

        if (t1 != null) t1.localScale = Vector3.zero;
        if (t2 != null) t2.localScale = Vector3.zero;

        // Slight pop effect for UI elements
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float percent = elapsed / duration;
            float s = 1f - Mathf.Pow(1f - percent, 3f); // Ease out cubic
            float scaleAmount = s * (1f + Mathf.Sin(percent * Mathf.PI) * 0.1f); // Subtle pop
            
            if (t1 != null) t1.localScale = Vector3.one * scaleAmount;
            if (t2 != null) t2.localScale = Vector3.one * scaleAmount;
            
            yield return null;
        }

        if (t1 != null) t1.localScale = Vector3.one;
        if (t2 != null) t2.localScale = Vector3.one;
    }

    private IEnumerator FlashPriceTextError()
    {
        if (vehiclePriceText == null) yield break;
        
        string originalText = vehiclePriceText.text;
        
        string errorTitle = Gazze.UI.LocalizationManager.Instance != null ? Gazze.UI.LocalizationManager.Instance.GetTranslation("Game_Insufficient") : "YETERSİZ";
        string errorDesc = Gazze.UI.LocalizationManager.Instance != null ? Gazze.UI.LocalizationManager.Instance.GetTranslation("Game_BalanceError") : "BAKİYE!";
        
        vehiclePriceText.text = $"<size=90%><color=#ff4d4d>{errorTitle}</color></size>\n<b><color=#ff0000>{errorDesc}</color></b>";
        
        if (errorSound != null && Settings.AudioManager.Instance != null)
        {
            Settings.AudioManager.Instance.PlaySFX(errorSound);
        }
        else
        {
            // Orijinal click sesini hata tonunda veremesek de uyarı olarak çalabiliriz
            if (Settings.AudioManager.Instance != null) Settings.AudioManager.Instance.PlayClickSound();
        }

        // Hızlıca titreme (shake) efekti
        Vector3 originalPos = vehiclePriceText.transform.localPosition;
        float elapsed = 0f;
        while (elapsed < 0.5f)
        {
            elapsed += Time.deltaTime;
            vehiclePriceText.transform.localPosition = originalPos + new Vector3(Random.Range(-5f, 5f), Random.Range(-5f, 5f), 0);
            yield return null;
        }
        
        vehiclePriceText.transform.localPosition = originalPos;
        
        yield return new WaitForSeconds(1f); // Hata metnini 1 sn ekranda tut
        UpdateCarDisplay(); // Metni ve fiyatı geri yükle
    }

    /// <summary>
    /// Yarışı/Oyunu başlatır veya kilitli aracı satın alır.
    /// </summary>
    public void StartGame()
    {
        // Debounce: Çok hızlı üst üste tıklamaları önle (0.5 saniye)
        if (Time.unscaledTime - lastClickTime < 0.5f) return;
        lastClickTime = Time.unscaledTime;

        if (Settings.AudioManager.Instance != null) Settings.AudioManager.Instance.PlayClickSound();

        int selectedIndex = PlayerPrefs.GetInt("SelectedCarIndex", 0);
        bool isLocked = false;
        var attributes = GetCarAttributes(selectedIndex);
        if (attributes != null && attributes.isLocked && selectedIndex > 0)
        {
            isLocked = PlayerPrefs.GetInt($"CarUnlocked_{selectedIndex}", 0) == 0;
        }

        if (isLocked)
        {
            int price = GetCarPrice(selectedIndex);
            int totalKredi = PlayerPrefs.GetInt("TotalKredi", 0);

            if (totalKredi >= price)
            {
                // Satın Alma Başarılı!
                PlayerPrefs.SetInt("TotalKredi", totalKredi - price);
                PlayerPrefs.SetInt($"CarUnlocked_{selectedIndex}", 1);
                PlayerPrefs.Save();
                
                if (buySound != null && Settings.AudioManager.Instance != null)
                {
                    Settings.AudioManager.Instance.PlaySFX(buySound);
                }

                // UI'ı güncelle ve metodu bitir (Hemen oyuna girmesin)
                UpdateCarDisplay();
                return; 
            }
            else
            {
                // Satın Alma Başarısız (Yetersiz Kredi)
                StartCoroutine(FlashPriceTextError());
                return;
            }
        }
        else
        {
            // Araç kilitli değilse (zaten alınmışsa veya ücretsizse) oyunu başlat
            if (LoadingManager.Instance != null)
            {
                LoadingManager.Instance.LoadScene("SampleScene");
            }
            else
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene("SampleScene");
            }
        }
    }

    /// <summary>
    /// Aracın tüm parçalarına Curved World shader'ını uygular.
    /// Bu, build'deki 'pink shader' hatasını çözer. Menüde eğrilik görünmese de (curvature=0 ayarlanabilir) 
    /// shader uyumluluğu için gereklidir.
    /// </summary>
    private void ApplyCurvedShaderToCar(GameObject car)
    {
        Shader vehicleShader = Shader.Find("Custom/VehicleShader_URP");
        if (vehicleShader == null) vehicleShader = Shader.Find("Custom/CurvedWorld_URP"); // Fallback
        if (vehicleShader == null) return;

        foreach (Renderer r in car.GetComponentsInChildren<Renderer>())
        {
            foreach (Material mat in r.materials)
            {
                if (mat.shader != vehicleShader)
                {
                    // Mevcut doku ve renkleri koruyarak shader'ı değiştir
                    Texture mainTex = mat.mainTexture;
                    Color mainColor = mat.HasProperty("_Color") ? mat.color : (mat.HasProperty("_BaseColor") ? mat.GetColor("_BaseColor") : Color.white);
                    mat.shader = vehicleShader;
                    if (mainTex != null) mat.SetTexture("_BaseMap", mainTex);
                    mat.SetColor("_BaseColor", mainColor);

                    // Showroom görünümü için kir ve aşınmayı sıfırla (veya çok az tut)
                    if (mat.HasProperty("_DirtAmount")) mat.SetFloat("_DirtAmount", 0.05f); // Çok hafif toz
                    if (mat.HasProperty("_WearStrength")) mat.SetFloat("_WearStrength", 0f); // Sıfır aşınma

                    // Yol ayarlarıyla (Road) senkronize et (Editor default değerleri)
                    mat.SetFloat("_Curvature", 0.002f);
                    mat.SetFloat("_CurvatureH", -0.0015f);
                    mat.SetFloat("_HorizonOffset", 10.0f);
                }
            }
        }
    }
}
