using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using Gazze.UI;

/// <summary>
/// Oyuncu aracının hareketini, hızını, puanlamasını ve UI etkileşimlerini yöneten ana kontrolcü.
/// </summary>
public class PlayerController : MonoBehaviour
{
    [Header("Yatay Hareket Ayarları (Şerit Değiştirme)")]
    [Tooltip("Aracın sağa ve sola şerit değiştirme hızı.")]
    public float horizontalMoveSpeed = 18f;
    [Tooltip("Aracın yol ekseninden (merkezden) maksimum ne kadar uzağa gidebileceği.")]
    public float limitX = 2.25f;
    [Tooltip("Şerit değiştirirken aracın gövdesinin yana yatma (z-axis tilt) miktarı.")]
    public float tiltAmount = 10f;
    [Tooltip("Dönüş yumuşatması ve rotasyonun toparlanma hızı.")]
    public float tiltSpeed = 8f;
    [Tooltip("Sağa veya sola giderken ivmelenmenin yumuşama hızı (Düşük = Daha yumuşak).")]
    public float horizontalSmoothing = 12f;
    [Tooltip("Ani hızlanma veya yavaşlama anında burnun öne/arkaya eğilme miktarı.")]
    public float pitchAmount = 5f;

    [Header("Hız ve İlerleme Ayarları")]
    [Tooltip("Aracın (ve yol akışının) şu anki hızı (m/s).")]
    public float currentWorldSpeed = 40f;
    [Tooltip("Sürüş sırasında düşülebilecek taban hız sınırı.")]
    public float minSpeed = 20f;
    [Tooltip("Aracın ulaşabileceği tavan hız sınırı (Boost hariç).")]
    public float maxSpeed = 100f;
    [Tooltip("Gaza basıldığında (veya otomatik modda) hızın artış ivmesi.")]
    public float acceleration = 20f;
    [Tooltip("Frene basıldığında veya gaz kesildiğinde hızın düşüş ivmesi.")]
    public float deceleration = 25f;
    [Tooltip("Herhangi bir girdi olmadığında aracın sabitlenmeye çalıştığı seyir hızı.")]
    public float cruiseSpeed = 40f;

    [Header("Arayüz (UI) Referansları")]
    [Tooltip("Anlık kat edilen mesafenin gösterildiği metin bileşeni.")]
    public TextMeshProUGUI scoreText;
    [Tooltip("Aracın anlık hızının (km/s) gösterildiği metin bileşeni.")]
    public TextMeshProUGUI speedText;
    [Tooltip("Toplanan yardım kolisi miktarının gösterildiği metin bileşeni.")]
    public TextMeshProUGUI coinText;
    [Tooltip("Kilitli araç ile oyuna girildiğinde gösterilen uyarı metni.")]
    public TextMeshProUGUI lockedVehicleText;
    [Tooltip("Arabanın kalan dayanıklılığını gösteren slider/bar.")]
    public Slider durabilitySlider;
    [Tooltip("Kalp sembollerinden oluşan can göstergesi sistemi.")]
    public Gazze.UI.HealthHeartDisplay heartDisplay;
    [Tooltip("Kalan boost miktarını gösteren slider.")]
    public Slider boostSlider;
    [Tooltip("Boost süresini saniye cinsinden (0.0s) gösteren metin.")]
    public TextMeshProUGUI boostTimeText;
    [Tooltip("Boost aktifken tüm ekranı kaplayan hız efekti kaplaması.")]
    public Image boostOverlay;
    [Tooltip("Kaza anında ekranda anlık patlayan kırmızı/beyaz flash karesi.")]
    public Image crashOverlay;

    [Header("Dayanıklılık Barı Animasyonları")]
    [Tooltip("Can barının 'kritik' uyarısı vermeye başlayacağı yüzde eşiği (0.0 - 1.0).")]
    public float lowHealthThreshold = 0.25f;
    [Tooltip("Hasar alındığında sağlık barının ne kadar sert sarsılacağı.")]
    public float barShakeIntensity = 2.5f;
    [Tooltip("Hasar alındığında sağlık barının bir anlık büyüme oranı.")]
    public float barPunchAmount = 1.1f;

    /// <summary> Sağlık barının ilk ölçek dğeri. </summary>
    private Vector3 durabilityBarInitialScale = Vector3.one;
    /// <summary> Sağlık barının ilk pozisyonu. </summary>
    private Vector3 durabilityBarInitialPos;
    /// <summary> Çakışan bar animasyonlarını durdurmak için tutulan referans. </summary>
    private Coroutine barAnimationCoroutine;

    [Header("Turbo (Boost) Mekanik Ayarları")]
    [Tooltip("Boost aktifken mevcut hıza eklenen bonus hız.")]
    public float boostSpeedBonus = 20f;
    [Tooltip("Boost sırasında kazanılan ekstra ivme miktarı.")]
    public float boostAccelerationBonus = 15f;
    [Tooltip("Boost barının saniye başı ne kadar hızlı tükeneceği.")]
    public float boostConsumptionRate = 0.5f;
    [Tooltip("Kullanılmadığında boost barının saniye başı ne kadar hızlı dolacağı.")]
    public float boostRefillRate = 0.2f;

    [Header("Turbo (Boost) Görselleri")]
    [Tooltip("Boost barı doluyken görünecek buton ikonu.")]
    public Sprite boostReadySprite;
    [Tooltip("Boost yaparken görünecek buton ikonu.")]
    public Sprite boostUsingSprite;
    [Tooltip("Boost boşalmışken veya dolarken görünecek buton ikonu.")]
    public Sprite boostRefillingSprite;
    [Tooltip("Sahnedeki turbo butonunun Image bileşeni.")]
    public Image boostButtonImage;
    [Tooltip("Turbo sırasında ekranda akan hız çizgileri (wind effects).")]
    public Image windEffectOverlay;

    /// <summary> Şu an turbo kullanılıyor mu? </summary>
    public bool IsBoosting => isBoosting;
    [Header("Hata Ayıklama (Debug)")]
    [Tooltip("Boost durumunu inspector üzerinden izleme.")]
    [SerializeField] private bool isBoosting = false;
    /// <summary> Turbo butonu basılı mı tutuluyor? </summary>
    private bool isBoostButtonHeld = false;
    /// <summary> Mevcut turbo rezervi (0.0 - 1.0). </summary>
    private float currentBoostAmount = 1.0f;
    /// <summary> En son ne zaman turbo kullanıldı? </summary>
    private float lastBoostTime = -999f;
    /// <summary> Turbo tamamen bitince zorunlu beklemeye devrede mi? </summary>
    private bool isBoostEmpty = false;

    [Header("Kıl Payı (Near Miss) Algılama")]
    [Tooltip("Diğer araçları geçiş mesafesinin algılanacağı yarıçap.")]
    public float nearMissRadius = 3.2f;

    [Header("Boost Renk Paleti")]
    private Vector3 boostButtonInitialScale = Vector3.one;
    private Color boostSliderReadyColor = new Color(0f, 180f / 255f, 1f, 1f);
    private Color boostSliderUsingColor = new Color(0.5f, 0.8f, 1f, 1f);
    private Color boostSliderRefillingColor = new Color(1f, 0.6f, 0f, 1f);
    private float boostPulseTimer = 0f;
    private float visualBoostAmount = 0f;

    [Header("Ses ve Efekt Klipleri")]
    [Tooltip("Turbo ses klibi.")]
    public AudioClip boostSound;
    [Tooltip("Egzoz ateşi vb. turbo parçacık efekti.")]
    public GameObject boostEffectPrefab;
    [Tooltip("Yardım kolisi toplama ses klibi.")]
    public AudioClip coinSound;
    [Tooltip("Kaza anı ses klibi.")]
    public AudioClip crashSound;
    [Tooltip("Oyun biterken çalacak hüzünlü ses klibi.")]
    public AudioClip deathSound;
    [Tooltip("Kaza anında araçtan çıkan duman/parça efekti prefabı.")]
    public GameObject crashEffectPrefab;
    [Tooltip("Ekran sarsıntısı şiddeti (Legacy).")]
    public float shakeIntensity = 0.35f;
    [Tooltip("Ekran sarsıntısı süresi (Legacy).")]
    public float shakeDuration = 0.2f;

    // --- OYUN DURUM DEĞİŞKENLERİ ---
    private float horizontalDirection = 0f;
    private float smoothHorizontal = 0f;
    private bool isGameOver = false;
    private float score = 0f;
    private MaterialPropertyBlock propBlock;
    private int coins = 0;
    private int nearMissCount = 0;
    private float playTime = 0f;
    private System.DateTime sessionStart;
    private int currentCombo = 0;
    private float comboTimer = 0f;
    private HashSet<int> passedCars = new HashSet<int>();
    private bool isGassing = false;
    private bool isBraking = false;
    private bool isMovingLeft = false;
    private bool isMovingRight = false;
    private Quaternion initialRotation;
    private float currentYawOffset = 0f;
    private float currentPitchOffset = 0f;
    private GameObject currentBoostEffect;
    private float visualDurability;
    private bool isBoostButtonHeldFromUI = false;
    private List<Renderer> carRenderers = new List<Renderer>();
    private float coinFlashTimer = 0f;
    private Color coinFlashColor = new Color(1f, 0.9f, 0.1f, 1f);
    private List<string> sessionAchievements = new List<string>();

    public float CurrentScore => score;
    public int CurrentCoins => coins;

    public void AddScore(float amount) { score += amount; }
    public void AddCoins(int amount) { coins += amount; coinFlashTimer = 0.25f; }

    [Header("Kontrol Paneli UI Referansları")]
    [Tooltip("Ekrandaki Sol/Sağ butonlarının içinde bulunduğu panel.")]
    public GameObject touchButtonsPanel;
    [Tooltip("Joystick kontrolü için kullanılan panel.")]
    public GameObject joystickPanel;
    [Tooltip("Hızlanma (Gaz) pedalının bulunduğu panel.")]
    public GameObject gasPedalPanel;
    [Tooltip("Joystick'in hareketli orta noktası.")]
    public RectTransform joystickHandle;
    [Tooltip("Joystick'in çekilebileceği maksimum mesafe.")]
    public float joystickRadius = 50f;

    private int currentControlMethod = 0;
    private int currentAccelerationMode = 0;
    private float currentControlSensitivity = 0.5f;
    private float currentAccelOffset = 0f;
    private Vector2 joystickInput = Vector2.zero;

    [Tooltip("Seçilen aracın oyunda doğru yöne bakması için rotasyon (ters dönmeyi engeller)")]
    public Vector3 carSpawnRotation = new(-90, 90, 0);

    [Header("Aktif Araç Verileri")]
    [Tooltip("Yüklenen arabanın istatistik bilgilerini barındıran ScriptableObject.")]
    public Gazze.Models.VehicleAttributes currentAttributes;
    [Tooltip("Aracın anlık dayanıklılık puanı.")]
    public float currentDurability = 100f;
    [Tooltip("Aracın sahip olabileceği maksimum dayanıklılık sınırı.")]
    public float maxDurability = 100f;

    [Header("Korunma (Invulnerability) Ayarları")]
    [Tooltip("Hasar aldıktan sonra geçici dokunulmazlık süresi (sn).")]
    public float invulnerabilityDuration = 5f;
    private bool isInvulnerable = false;
    private float invulnerabilityTimer = 0f;

    [Header("Araç Havuzu")]
    [Tooltip("Oyunda kullanılabilecek araba prefabları listesi.")]
    public GameObject[] availableCarPrefabs;

    public static PlayerController Instance;

    private void Awake()
    {
        Instance = this;
        // AudioManager'ı hemen başlatarak müziğin ilk andan itibaren çalmasını sağlarız.
        if (Settings.AudioManager.Instance != null) { /* Singleton tetiklendi */ }

        // Eğer Inspector'dan atanmamışsa UI panellerini isimle bulmaya çalış (Hiyerarşi bağımsızlığı için)
        if (touchButtonsPanel == null)
        {
            // Sol/Sağ butonlarını içeren bir panel varsa bul, yoksa butonları tek tek bulup bir gruba alabiliriz 
            // ama en güvenlisi Canvas altındaki isimlere bakmak.
            var left = GameObject.Find("LeftButton");
            var right = GameObject.Find("RightButton");
            if (left != null && right != null)
            {
                // Eğer butonlar doğrudan Canvas altındaysa, geçici bir referans için birini kullanmak yerine 
                // kodda bunları tek tek yönetmek daha sağlıklı. Şimdilik hiyerarşideki yerlerine göre buluyoruz.
                touchButtonsPanel = left.transform.parent.gameObject; 
                // Not: Eğer butonlar doğrudan Canvas altındaysa parent=Canvas olur. 
                // Canvas'ı kapatmak istemeyiz, o yüzden aşağıda UpdateControlUIVisibility'yi buna göre güncelleyeceğiz.
            }
        }
        
        if (gasPedalPanel == null)
        {
            var gas = GameObject.Find("GasButton");
            var brake = GameObject.Find("BrakeButton");
            if (gas != null) gasPedalPanel = gas; // Gaz butonu referansı
        }

        // Çakışan EventSystem kontrolü
        var eventSystems = FindObjectsByType<UnityEngine.EventSystems.EventSystem>(FindObjectsSortMode.None);
        if (eventSystems.Length > 1)
            Debug.LogWarning("DİKKAT: Sahnede birden fazla EventSystem var! Bu durum butonların çalışmasını engelleyebilir.");
    }

    private void OnEnable()
    {
        Settings.SettingsModel.OnSettingsChanged += OnSettingsChanged;
    }

    private void OnDisable()
    {
        Settings.SettingsModel.OnSettingsChanged -= OnSettingsChanged;
    }

    private void OnSettingsChanged()
    {
        LoadControlSettings();
        UpdateControlUIVisibility();
    }

    private void Start()
    {
        // Zaman akışını normale döndür ve değişkenleri sıfırla
        Time.timeScale = 1f;
        isGameOver = false;
        Gazze.UI.PauseMenuBuilder.ForceClose(); // Önceki oturumdan kalma pause menüsünü temizle
        score = 0f;
        coins = 0;
        nearMissCount = 0;
        currentWorldSpeed = 0f;
        initialRotation = transform.localRotation;
        sessionStart = System.DateTime.UtcNow;
        visualDurability = currentDurability; // Başlangıçta anlık eşitle
        sessionAchievements.Clear();

        // Kontrol ayarlarını yükle
        LoadControlSettings();
        UpdateControlUIVisibility();


        if (lockedVehicleText != null) lockedVehicleText.gameObject.SetActive(false);
        
        // Araç listesini kontrol et ve gerekirse yükle
        if (availableCarPrefabs == null || availableCarPrefabs.Length == 0)
        {
            availableCarPrefabs = Resources.LoadAll<GameObject>("Cars");
            if (availableCarPrefabs == null || availableCarPrefabs.Length == 0)
            {
                // Debug.LogError("PlayerController: 'Resources/Cars' klasöründe araç bulunamadı!");
            }
        }

        SpawnSelectedCar();
        UpdateUI();

        if (boostOverlay != null) boostOverlay.gameObject.SetActive(false);
        if (crashOverlay != null) crashOverlay.gameObject.SetActive(false);

        // Dayanıklılık barı başlangıç değerlerini kaydet
        if (durabilitySlider != null)
        {
            RectTransform rt = durabilitySlider.GetComponent<RectTransform>();
            durabilityBarInitialScale = rt.localScale;
            durabilityBarInitialPos = rt.anchoredPosition;
        }

        // Boost butonu başlangıç ölçeğini kaydet
        if (boostButtonImage != null)
        {
            boostButtonInitialScale = boostButtonImage.rectTransform.localScale;
            // Boost butonu altındaki objelerin (slider vb.) tıklamayı engellemesini önle (Raycast Blocking)
            foreach (var graphic in boostButtonImage.GetComponentsInChildren<UnityEngine.UI.Graphic>())
            {
                if (graphic != boostButtonImage)
                    graphic.raycastTarget = false;
            }
        }
    }

    /// <summary>
    /// PlayerPrefs'ten seçilen araç indeksini okur ve sahneye yükler.
    /// </summary>
    private void SpawnSelectedCar()
    {
        int selectedIndex = PlayerPrefs.GetInt("SelectedCarIndex", 0);
        
        Gazze.Models.VehicleAttributes attributes = null;
        if (availableCarPrefabs != null && selectedIndex >= 0 && selectedIndex < availableCarPrefabs.Length)
        {
            var vehicleComp = availableCarPrefabs[selectedIndex].GetComponent<Gazze.Vehicles.Vehicle>();
            if (vehicleComp != null) attributes = vehicleComp.attributes;
        }

        if (attributes == null && Gazze.Models.VehicleRepository.Instance != null && Gazze.Models.VehicleRepository.Instance.vehicles.Count > selectedIndex)
        {
            attributes = Gazze.Models.VehicleRepository.Instance.vehicles[selectedIndex];
        }

        // Yeni Kilit kontrolü (PlayerPrefs + VehicleAttributes dictasyonu)
        bool isLocked = false;
        if (attributes != null && attributes.isLocked && selectedIndex > 0)
        {
            isLocked = PlayerPrefs.GetInt($"CarUnlocked_{selectedIndex}", 0) == 0;
        }
        
            if (isLocked)
            {
                string carName = GetTranslationSafe("Game_LockedVehicle", "KİLİTLİ ARAÇ");
                if (Gazze.Models.VehicleRepository.Instance != null && Gazze.Models.VehicleRepository.Instance.vehicles.Count > selectedIndex)
                {
                    carName = Gazze.Models.VehicleRepository.Instance.vehicles[selectedIndex].name;
                }
                
                if (lockedVehicleText != null)
                {
                    string format = GetTranslationSafe("Game_PurchaseRequired", "ARAÇ SATIN ALINMAMIŞ: {0}\nLütfen ana menüden aracı satın alın veya başka bir araç seçin.");
                    lockedVehicleText.text = string.Format(format, carName);
                    lockedVehicleText.gameObject.SetActive(true);
                }
                
                // Oyunu durdur ve ana menüye dönmek için butonların görünmesini sağla veya otomatik dön
                isGameOver = true;
                Time.timeScale = 0f;
                
                // Final ekranını kilitli araç mesajıyla göster
                string achMsg = GetTranslationSafe("Game_NotPurchasedAch", "Araç henüz alınmadı!");
            var data = new Gazze.UI.GameOverPanelBuilder.Data
            {
                score = 0,
                highScore = PlayerPrefs.GetInt("HighScore", 0),
                level = 0,
                playTimeSeconds = 0,
                achievements = new List<string> { achMsg },
                onRestart = RestartGame,
                onMainMenu = MainMenu,
                onShare = null,
                onSettings = null
            };
            Gazze.UI.GameOverPanelBuilder.Build(data);
            return;
        }

        if (availableCarPrefabs != null && selectedIndex >= 0 && selectedIndex < availableCarPrefabs.Length)
        {
            GameObject car = Instantiate(availableCarPrefabs[selectedIndex], transform);
            car.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.Euler(carSpawnRotation));
            car.transform.localScale = Vector3.one;
            // Debug.Log($"Araç başarıyla yüklendi: {availableCarPrefabs[selectedIndex].name}");

            // Cache renderers for visual effects
            carRenderers.Clear();
            carRenderers.AddRange(car.GetComponentsInChildren<Renderer>());

            // Yeni Sistem Özelliklerini Yükle
            // 1. Önce prefab üzerinde 'Vehicle' bileşeni var mı diye kontrol et (Daha güvenli yöntem)
            Gazze.Vehicles.Vehicle vehicleComp = car.GetComponent<Gazze.Vehicles.Vehicle>();
            if (vehicleComp != null && vehicleComp.attributes != null)
            {
                currentAttributes = vehicleComp.attributes;
                // Debug.Log($"Özellikler prefab üzerindeki 'Vehicle' bileşeninden yüklendi: {currentAttributes.name}");
            }
            // 2. Eğer bileşen yoksa, Repository içindeki sıraya göre bul (Geriye dönük uyumluluk)
            else if (Gazze.Models.VehicleRepository.Instance != null && Gazze.Models.VehicleRepository.Instance.vehicles.Count > selectedIndex)
            {
                currentAttributes = Gazze.Models.VehicleRepository.Instance.vehicles[selectedIndex];
                // Debug.Log($"Özellikler Repository sırasına göre yüklendi: {currentAttributes.name}");
            }

            if (currentAttributes != null)
            {
                ApplyAttributes();
                
                // Araçların build'de pembe görünmesini engellemek ve yola göre eğilmesini sağlamak için shader ata
                ApplyCurvedShaderToCar(car);

                // Yüksekliği yola oturacak şekilde ayarla
                car.transform.localPosition = new Vector3(0, 0.12f, 0);
                
                // Oyuncu collider'ını yüklenen modele göre otomatik (rotation-safe) ayarla
                BoxCollider bc = GetComponent<BoxCollider>();
                if (bc != null)
                {
                    AdjustColliderToFitModel(car, bc, 0.7f);
                }
            }
        }
    }

    /// <summary>
    /// Aracın tüm parçalarına Curved World shader'ını uygular.
    /// Bu, build'deki 'pink shader' hatasını çözer ve araçların yolla beraber eğilmesini sağlar.
    /// </summary>
    private void ApplyCurvedShaderToCar(GameObject car)
    {
        Shader vehicleShader = Shader.Find("Custom/VehicleShader_URP");
        if (vehicleShader == null) vehicleShader = Shader.Find("Custom/CurvedWorld_URP"); // Fallback
        if (vehicleShader == null) return;

        foreach (Renderer r in car.GetComponentsInChildren<Renderer>())
        {
            // Orijinal materyalleri sızdırmadan güncellemek için sharedMaterials kullanıyoruz.
            Material[] mats = r.sharedMaterials;
            bool changed = false;
            for (int i = 0; i < mats.Length; i++)
            {
                if (mats[i] == null) continue;
                if (mats[i].shader != vehicleShader)
                {
                    // Mevcut doku ve renkleri koruyarak shader'ı değiştir
                    Texture mainTex = null;
                    if (mats[i].HasProperty("_BaseMap")) mainTex = mats[i].GetTexture("_BaseMap");
                    else if (mats[i].HasProperty("_MainTex")) mainTex = mats[i].mainTexture;

                    Color mainColor = Color.white;
                    if (mats[i].HasProperty("_BaseColor")) mainColor = mats[i].GetColor("_BaseColor");
                    else if (mats[i].HasProperty("_Color")) mainColor = mats[i].color;

                    // NOT: Burada mecburen yeni materyal oluşturuyoruz çünkü shader değişiyor.
                    // Ancak bunu sadece bir kez (Start'ta) yapıyoruz. 
                    // İleride sızıntıyı tamamen sıfırlamak için bu materyallerin OnDestroy'da temizlenmesi gerekir.
                    mats[i] = new Material(mats[i]);
                    mats[i].shader = vehicleShader;
                    if (mainTex != null) mats[i].SetTexture("_BaseMap", mainTex);
                    mats[i].SetColor("_BaseColor", mainColor);

                    // Oyuncu aracı trafik araçlarından bir tık daha temiz ama yine de karakterli
                    if (mats[i].HasProperty("_DirtAmount")) mats[i].SetFloat("_DirtAmount", 0.25f);
                    if (mats[i].HasProperty("_WearStrength")) mats[i].SetFloat("_WearStrength", 0.35f);
                    if (mats[i].HasProperty("_DirtColor")) mats[i].SetColor("_DirtColor", new Color(0.25f, 0.15f, 0.1f, 1f));

                    // Yol ayarlarıyla (Road) senkronize et
                    mats[i].SetFloat("_Curvature", 0.002f);
                    mats[i].SetFloat("_CurvatureH", -0.0015f);
                    mats[i].SetFloat("_HorizonOffset", 10.0f);
                    changed = true;
                }
            }
            if (changed) r.sharedMaterials = mats;
        }
    }

    /// <summary>
    /// Araç özelliklerini oyun mekaniklerine uygular.
    /// </summary>
    private void ApplyAttributes()
    {
        if (currentAttributes == null) return;

        int selectedIndex = PlayerPrefs.GetInt("SelectedCarIndex", 0);

        // Yükseltilmiş değerleri hesapla
        float upgradedMaxSpeed = Gazze.Models.VehicleUpgradeManager.GetUpgradedValue(selectedIndex, Gazze.Models.VehicleUpgradeManager.UpgradeType.Speed, currentAttributes.maxSpeedKmh);
        float upgradedAccel = Gazze.Models.VehicleUpgradeManager.GetUpgradedValue(selectedIndex, Gazze.Models.VehicleUpgradeManager.UpgradeType.Acceleration, currentAttributes.accelerationMs2);
        float upgradedDurability = Gazze.Models.VehicleUpgradeManager.GetUpgradedValue(selectedIndex, Gazze.Models.VehicleUpgradeManager.UpgradeType.Durability, currentAttributes.durability);
        
        // Boost süresi ve dolma hızı yükseltmeleri
        float upgradedBoostDuration = Gazze.Models.VehicleUpgradeManager.GetUpgradedValue(selectedIndex, Gazze.Models.VehicleUpgradeManager.UpgradeType.BoostDuration, 2.0f);
        boostConsumptionRate = 1.0f / upgradedBoostDuration; // 2 sn sürüyorsa 0.5/sn tüketir

        boostRefillRate = Gazze.Models.VehicleUpgradeManager.GetUpgradedValue(selectedIndex, Gazze.Models.VehicleUpgradeManager.UpgradeType.BoostRefillRate, 0.2f);
        currentBoostAmount = 1.0f; // Yarış başı hazır
        isBoostEmpty = false;

        maxSpeed = upgradedMaxSpeed / 3.6f; // km/h to m/s
        acceleration = upgradedAccel;
        horizontalMoveSpeed = currentAttributes.steeringSensitivity * 15f;
        maxDurability = upgradedDurability;
        currentDurability = maxDurability;

        if (heartDisplay != null)
        {
            heartDisplay.SetupHearts(maxDurability);
        }

        // Debug.Log($"Özellikler (Yükseltilmiş) uygulandı: Hız={maxSpeed}, BoostTüketim={boostConsumptionRate}, BoostDolum={boostRefillRate}");
    }

    /// <summary>
    /// Modelin mesh sınırlarını hesaplayıp collider'ı ona göre ayarlar.
    /// </summary>
    private void AdjustColliderToFitModel(GameObject modelRoot, BoxCollider bc, float multiplier)
    {
        // ÖNEMLİ: Rotation reset kaldırıldı çünkü modeller game-pose (flat) durumundayken ölçülmeli.
        Bounds b = new Bounds();
        bool first = true;
        foreach (Renderer r in modelRoot.GetComponentsInChildren<Renderer>())
        {
            if (first) { b = r.bounds; first = false; }
            else b.Encapsulate(r.bounds);
        }

        if (!first)
        {
            // World space bounds'u Player'ın (this) local space'ine çevir
            bc.center = transform.InverseTransformPoint(b.center);
            Vector3 localSize = transform.InverseTransformVector(b.size);
            bc.size = new Vector3(Mathf.Abs(localSize.x), Mathf.Abs(localSize.y), Mathf.Abs(localSize.z)) * multiplier;
            
            // Pozisyon düzenlemeleri: Yükseklik tabana, merkez biraz öne (Forward bias)
            Vector3 center = bc.center;
            center.y = bc.size.y * 0.5f; 
            center.z += bc.size.z * 0.15f; // Öne doğru %15 kaydır (Kullanıcı isteği: "biraz daha önde olsun")
            bc.center = center;
        }
    }

    /// <summary>
    /// Araca hasar verir ve dayanıklılığı günceller.
    /// </summary>
    public void TakeDamage(float amount)
    {
        if (isInvulnerable) return; // Koruma süresindeyken hasar alma

        // ── PowerUp Check (Yeni) ──────────────────────
        if (Gazze.PowerUps.PowerUpManager.Instance != null)
        {
            if (Gazze.PowerUps.PowerUpManager.Instance.IsPowerUpActive(Gazze.PowerUps.PowerUpType.Ghost))
                return;

            if (Gazze.PowerUps.PowerUpManager.Instance.IsPowerUpActive(Gazze.PowerUps.PowerUpType.Shield))
                return;

            if (Gazze.PowerUps.PowerUpManager.Instance.IsPowerUpActive(Gazze.PowerUps.PowerUpType.Juggernaut))
                return;
        }

        currentDurability -= amount;
        currentDurability = Mathf.Clamp(currentDurability, 0f, maxDurability);

        if (currentDurability <= 0)
        {
            GameOver();
        }
        else
        {
            // Fix: Hasar sonrası koruma süresini başlat (HP Manager'dan gelen ardışık hasarları engeller)
            StartInvulnerability(0.5f); // 0.5 saniyelik kısa bir koruma
        }
        
        // Çarpışma efekti
        if (crashOverlay != null) StartCoroutine(FlashCrashOverlay());
        if (crashSound != null && !isGameOver && Settings.AudioManager.Instance != null) 
            Settings.AudioManager.Instance.PlaySFX(crashSound);

        // Dayanıklılık göstergesi sarsılma/büyüme efekti
        if (heartDisplay != null)
        {
            visualDurability = currentDurability; 
        }
        else if (durabilitySlider != null)
        {
            if (barAnimationCoroutine != null) StopCoroutine(barAnimationCoroutine);
            barAnimationCoroutine = StartCoroutine(AnimateDurabilityBarOnDamage());
        }
    }

    private void Update()
    {
        if (isGameOver) return;

        // ── Geri Sayım Kontrolü: Başlamadan önce hiçbir şey yapma ──
        bool countdownActive = Gazze.UI.CountdownManager.Instance != null && !Gazze.UI.CountdownManager.Instance.IsGameStarted;
        if (countdownActive)
        {
            currentWorldSpeed = 0f;
            UpdateUI();
            return;
        }

        // ── Visual Feedback (Power-Ups & Coin Flash) ──────────────────────
        if (coinFlashTimer > 0) coinFlashTimer -= Time.deltaTime;

        Color targetColor = Color.white;
        bool hasSpecificColor = false;

        if (Gazze.PowerUps.PowerUpManager.Instance != null)
        {
            if (Gazze.PowerUps.PowerUpManager.Instance.IsPowerUpActive(Gazze.PowerUps.PowerUpType.Ghost))
            {
                targetColor = new Color(0.5f, 0.5f, 1f, 0.5f);
                hasSpecificColor = true;
            }
            else if (Gazze.PowerUps.PowerUpManager.Instance.IsPowerUpActive(Gazze.PowerUps.PowerUpType.Shield))
            {
                targetColor = new Color(1.0f, 0.8f, 0.3f, 1f);
                hasSpecificColor = true;
            }
            else if (Gazze.PowerUps.PowerUpManager.Instance.IsPowerUpActive(Gazze.PowerUps.PowerUpType.Juggernaut))
            {
                // Turuncu enerji parlama efekti (Juggernaut / Dev Modu)
                float glow = 0.7f + Mathf.Sin(Time.time * 4f) * 0.3f;
                targetColor = new Color(1f, 0.5f * glow, 0.1f, 1f);
                hasSpecificColor = true;
            }
            else if (Gazze.PowerUps.PowerUpManager.Instance.IsPowerUpActive(Gazze.PowerUps.PowerUpType.TimeWarp))
            {
                // Mavi-mor ton (Zaman Bükücü)
                float pulse = 0.8f + Mathf.Sin(Time.unscaledTime * 3f) * 0.2f;
                targetColor = new Color(0.5f * pulse, 0.3f * pulse, 1f, 1f);
                hasSpecificColor = true;
            }
        }

        if (coinFlashTimer > 0)
        {
            float flashT = coinFlashTimer / 0.25f; // Duration 0.25s
            targetColor = Color.Lerp(targetColor, coinFlashColor, flashT);
            hasSpecificColor = true;
        }

        if (hasSpecificColor || Time.frameCount % 30 == 0) // Only update if needed or periodically to ensure sync
        {
            if (propBlock == null) propBlock = new MaterialPropertyBlock();

            foreach (var r in carRenderers)
            {
                if (r == null) continue;
                r.GetPropertyBlock(propBlock);
                propBlock.SetColor("_BaseColor", targetColor);
                propBlock.SetColor("_Color", targetColor); // Fallback for standard shaders
                r.SetPropertyBlock(propBlock);
            }
        }

        HandleInput();
        HandleSpeed();
        HandleMovement();
        HandleBoost();
        HandleInvulnerability();
        
        // ── Wind Sensation Haptics (Hız bazlı titreşim) ──
        float speedFactor = Mathf.InverseLerp(cruiseSpeed, maxSpeed + boostSpeedBonus, currentWorldSpeed);
        Settings.HapticManager.WindSensation(speedFactor);

        CheckHighSpeedCollectibles();

        HandleComboTimer();
        CheckNearMissContinuous();

        // Yeni Yüksek Performanslı Çarpışma Sistemine Kayıt (ID: 0 = Player)
        if (Gazze.Collision.HighPerformanceCollisionManager.Instance != null)
        {
            BoxCollider bc = GetComponent<BoxCollider>();
            Gazze.Collision.HighPerformanceCollisionManager.Instance.RegisterEntity(new Gazze.Collision.HighPerformanceCollisionManager.EntityData
            {
                id = 0,
                position = bc.bounds.center,
                extents = bc.bounds.extents,
                rotation = transform.rotation,
                type = Gazze.Collision.HighPerformanceCollisionManager.CollisionType.AABB,
                layer = 0 // Player Layer
            });
        }

        UpdateScore();
        
        // Dayanıklılık animasyonu (smooth transition)
        visualDurability = Mathf.Lerp(visualDurability, currentDurability, Time.deltaTime * 5f);
        
        // Gerçek zamanlı başarım kontrolü
        CheckRealtimeAchievements();
        
        UpdateUI();
        playTime = (float)(System.DateTime.UtcNow - sessionStart).TotalSeconds;
        HandleShortcuts();
    }

    /// <summary>
    /// PlayerPrefs'ten kontrol ayarlarını okur.
    /// </summary>
    public void LoadControlSettings()
    {
        currentControlMethod = PlayerPrefs.GetInt(Settings.SettingsModel.ControlMethodKey, 0);
        currentAccelerationMode = PlayerPrefs.GetInt(Settings.SettingsModel.AccelerationModeKey, 0);
        currentControlSensitivity = PlayerPrefs.GetFloat(Settings.SettingsModel.ControlSensitivityKey, 0.5f);
        currentAccelOffset = PlayerPrefs.GetFloat(Settings.SettingsModel.AccelerometerOffsetKey, 0f);
        
        // Analog (1) kaldırıldığı için eğer eskiden kalma bir 2 (Tilt) varsa onu 1'e çek
        if (currentControlMethod > 1)
        {
            currentControlMethod = 1;
            PlayerPrefs.SetInt(Settings.SettingsModel.ControlMethodKey, 1);
        }
    }

    /// <summary>
    /// Seçili kontrol tipine göre UI elementlerini gösterir/gizler.
    /// </summary>
    public void UpdateControlUIVisibility()
    {
        bool isButtons = currentControlMethod == 0;
        bool isManual = currentAccelerationMode == 0;

        // Pasif objeleri de bulabilmek için Canvas üzerinden arama yapıyoruz
        Canvas mainCanvas = null;
        var canvases = FindObjectsByType<Canvas>(FindObjectsSortMode.None);
        foreach(var c in canvases) { if(c.name == "Canvas") { mainCanvas = c; break; } }
        
        if (mainCanvas != null)
        {
            Transform canvasTrans = mainCanvas.transform;
            
            // Sol/Sağ butonlarını yönet
            Transform left = canvasTrans.Find("LeftButton");
            Transform right = canvasTrans.Find("RightButton");
            if (left != null) left.gameObject.SetActive(isButtons);
            if (right != null) right.gameObject.SetActive(isButtons);

            // Gaz/Fren butonlarını yönet
            Transform gas = canvasTrans.Find("GasButton");
            Transform brake = canvasTrans.Find("BrakeButton");
            if (gas != null) gas.gameObject.SetActive(isManual);
            if (brake != null) brake.gameObject.SetActive(isManual);

            // Joystick panelini yönet
            Transform jp = canvasTrans.Find("JoystickPanel");
            if (jp != null) jp.gameObject.SetActive(false);
        }

        // Eğer Inspector'dan atanmış özel paneller varsa onları da yönet (Geriye dönük uyumluluk)
        if (touchButtonsPanel != null && touchButtonsPanel.name != "Canvas")
            touchButtonsPanel.SetActive(isButtons);
        
        if (joystickPanel != null) 
            joystickPanel.SetActive(false);

        if (gasPedalPanel != null && gasPedalPanel.name != "Canvas")
            gasPedalPanel.SetActive(isManual);
    }

    /// <summary>
    /// Klavye ve dokunmatik girişleri kontrol eder.
    /// </summary>
    private void HandleInput()
    {
        float kbInput = 0f;
        
        // 1. New Input System Kontrolü (Klavye)
        if (Keyboard.current != null)
        {
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) kbInput = -1f;
            else if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) kbInput = 1f;
        }
        
        float gpInput = 0f;
        // 2. New Input System Kontrolü (Gamepad)
        if (Gamepad.current != null)
        {
            gpInput = Gamepad.current.leftStick.x.ReadValue();
            if (Mathf.Abs(gpInput) < 0.1f) gpInput = 0f;
            
            // D-Pad desteği
            if (gpInput == 0f)
            {
                if (Gamepad.current.dpad.left.isPressed) gpInput = -1f;
                else if (Gamepad.current.dpad.right.isPressed) gpInput = 1f;
            }
        }

        float buttonInput = 0f;
        if (isMovingLeft) buttonInput = -1f;
        else if (isMovingRight) buttonInput = 1f;

        // 3. Kontrol Yöntemine Göre Input Hesapla
        float finalHorizontal = 0f;
        switch (currentControlMethod)
        {
            case 0: // Touch Buttons (Klavye ile birleştirilmiş)
                finalHorizontal = Mathf.Clamp(kbInput + gpInput + buttonInput, -1f, 1f);
                break;
            case 1: // Tilt (Accelerometer - Mobile Optimized)
                float accelX = 0f;
                
                // 1. New Input System Accelerometer (Öncelikli)
                if (Accelerometer.current != null)
                {
                    if (!Accelerometer.current.enabled) InputSystem.EnableDevice(Accelerometer.current);
                    accelX = Accelerometer.current.acceleration.ReadValue().x;
                }
                // 2. Legacy Input Fallback
                else
                {
                    accelX = Input.acceleration.x;
                }

                // Tilt hassasiyeti (Mobil için optimize edildi)
                float sensMul = 5f + (currentControlSensitivity * 10f); // Hassasiyet aralığı genişletildi
                float tiltValue = (accelX - currentAccelOffset) * sensMul;
                
                // Deadzone kontrolü
                if (Mathf.Abs(accelX - currentAccelOffset) < 0.01f) tiltValue = 0f;
                
                // Klavye/Gamepad desteği ile birleştir (Test için iyi)
                finalHorizontal = Mathf.Clamp(kbInput + gpInput + tiltValue, -1f, 1f);
                break;
        }

        horizontalDirection = finalHorizontal;
        
        // Gaz/Fren durumlarını güncelle (OTOMATİK MOD)
        if (currentAccelerationMode == 1) // Automatic Acceleration (Index 1)
        {
            // "Ekrandaki herhangi bir yere tıkladığımızda fren yapsın"
            bool screenTouched = false;
            
            // 1. Dokunmatik ekran kontrolü (Tam ekran fren)
            if (Touchscreen.current != null)
            {
                foreach (var touch in Touchscreen.current.touches)
                {
                    if (touch.press.isPressed)
                    {
                        // UI üzerindeyse (buton vs) fren yapma!
                        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(touch.touchId.ReadValue()))
                            continue;

                        screenTouched = true;
                        break;
                    }
                }
            }
            
            // 2. Mouse tıklaması (Test kolaylığı için)
            if (!screenTouched && Mouse.current != null && Mouse.current.leftButton.isPressed) 
            {
                // UI üzerindeyse fren yapma!
                if (EventSystem.current == null || !EventSystem.current.IsPointerOverGameObject())
                    screenTouched = true;
            }

            // Eğer dokunuluyorsa fren yap, dokunulmuyorsa gaz ver
            isBraking = screenTouched;
            isGassing = !screenTouched; 
        }
        else // Manual Acceleration
        {
            if (Keyboard.current != null)
            {
                bool kbGas = Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed;
                bool kbBrake = Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed;
                
                isGassing = kbGas || isGassingFromUI;
                isBraking = kbBrake || isBrakingFromUI;
            }
            else
            {
                isGassing = isGassingFromUI;
                isBraking = isBrakingFromUI;
            }
        }

        // Mutlak mutual exclusion (Gaz ve fren aynı anda basılamaz)
        if (isGassing && isBraking) isBraking = false;

        // Boost girişi (Klavye & Mobile)
        isBoostButtonHeld = isBoostButtonHeldFromUI;
        if (Keyboard.current != null)
            isBoostButtonHeld |= Keyboard.current.leftShiftKey.isPressed || Keyboard.current.spaceKey.isPressed;

        if (isBoostButtonHeld && !isGameOver)
        {
            if (currentBoostAmount > 0 && !isBoostEmpty)
            {
                if (!isBoosting) ActivateBoost();
            }
            else if (isBoosting)
            {
                EndBoost();
            }
        }
        else if (isBoosting)
        {
            EndBoost();
        }
    }

    // Fix: Sticky input önleyici yeni fieldlar
    private bool isGassingFromUI = false;
    private bool isBrakingFromUI = false;

    /// <summary>
    /// Aracın hızlanma, yavaşlama ve cruise hızı mantığını yönetir.
    /// </summary>
    private void HandleSpeed()
    {
        float targetSpeed = cruiseSpeed;
        float currentAcceleration = acceleration;

        bool gasInput = isGassing;
        bool brakeInput = isBraking;

        if (Keyboard.current != null)
        {
            gasInput |= Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed;
            brakeInput |= Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed;
        }

        if (Gamepad.current != null)
        {
            gasInput |= Gamepad.current.buttonSouth.isPressed || Gamepad.current.rightTrigger.isPressed;
            brakeInput |= Gamepad.current.buttonEast.isPressed || Gamepad.current.leftTrigger.isPressed;
        }

        // Otomatik gaz modu HandleInput içinde hesaplandığı için buraya gerek yok.

        // Boost durumunda hız ve ivme bonuslarını uygula
        if (isBoosting)
        {
            targetSpeed += boostSpeedBonus;
            currentAcceleration += boostAccelerationBonus;
        }

        // MoveDt'yi HandleSpeed içinde de kullan (Kompansasyon için)
        float moveDt = Time.deltaTime;
        if (Gazze.PowerUps.PowerUpEffects.Instance != null && Gazze.PowerUps.PowerUpEffects.Instance.IsTimeWarpActive)
            moveDt = Time.unscaledDeltaTime;

        if (gasInput)
        {
            float maxEffectiveSpeed = maxSpeed + (isBoosting ? boostSpeedBonus : 0f);
            currentWorldSpeed = Mathf.MoveTowards(currentWorldSpeed, maxEffectiveSpeed, currentAcceleration * moveDt);
            currentPitchOffset = Mathf.MoveTowards(currentPitchOffset, -pitchAmount, tiltSpeed * moveDt);
        }
        else if (brakeInput)
        {
            targetSpeed = minSpeed;
            currentWorldSpeed = Mathf.MoveTowards(currentWorldSpeed, targetSpeed, deceleration * moveDt);
            currentPitchOffset = Mathf.MoveTowards(currentPitchOffset, pitchAmount, tiltSpeed * moveDt);
        }
        else
        {
            currentWorldSpeed = Mathf.MoveTowards(currentWorldSpeed, targetSpeed, (deceleration / 2f) * moveDt);
            currentPitchOffset = Mathf.MoveTowards(currentPitchOffset, 0, tiltSpeed * moveDt);
        }
    }

    /// <summary>
    /// Şerit değiştirme hareketini ve görsel yatma (tilt) etkisini uygular.
    /// </summary>
    private void HandleMovement()
    {
        // TimeWarp kompansasyonu: Dünya yavaşladığında oyuncunun manevra hızı aynı kalması için
        float moveDt = Time.deltaTime;
        if (Gazze.PowerUps.PowerUpEffects.Instance != null && Gazze.PowerUps.PowerUpEffects.Instance.IsTimeWarpActive)
        {
            moveDt = Time.unscaledDeltaTime;
        }

        // Girdiyi hedefe doğru yumuşatarak taşı
        smoothHorizontal = Mathf.Lerp(smoothHorizontal, horizontalDirection, moveDt * horizontalSmoothing);

        float newX = transform.position.x + (smoothHorizontal * horizontalMoveSpeed * moveDt);
        newX = Mathf.Clamp(newX, -limitX, limitX);
        transform.position = new(newX, transform.position.y, transform.position.z);

        // Görsel rotasyon (Yana yatma)
        currentYawOffset = Mathf.MoveTowards(currentYawOffset, -smoothHorizontal * tiltAmount, tiltSpeed * moveDt);
        transform.localRotation = initialRotation * Quaternion.Euler(currentPitchOffset, 0, currentYawOffset);
    }

    private void HandleBoost()
    {
        if (isBoosting)
        {
            currentBoostAmount -= boostConsumptionRate * Time.deltaTime;
            currentBoostAmount = Mathf.Clamp01(currentBoostAmount);
            
            // "Near Miss" (Sıfır Geçme) Kontrolü: Boost yaparken araçlara çok yakın geçersen enerji kazanırsın
            // CheckNearMissContinuous() artık Update'ten çalışıyor.

            if (currentBoostAmount <= 0)
            {
                isBoostEmpty = true;
                EndBoost();
            }
        }
        else
        {
            // Boost dolumu (Sadece boost yapılmıyorken)
            if (currentBoostAmount < 1.0f)
            {
                float prevAmount = currentBoostAmount;
                currentBoostAmount += boostRefillRate * Time.deltaTime;
                currentBoostAmount = Mathf.Clamp01(currentBoostAmount);
                
                // %100 olunca kısa bir flash/pulse efekti için sinyal (isteğe bağlı)
                if (prevAmount < 1.0f && currentBoostAmount >= 1.0f)
                {
                    Settings.HapticManager.Light();
                }

                // %25 dolunca tekrar kullanılabilir yap (biraz pay bırakmak iyi hissettirir)
                if (isBoostEmpty && currentBoostAmount > 0.25f)
                {
                    isBoostEmpty = false;
                }
            }
        }
    }

    /// <summary>
    /// Combo süresini yönetir (Combo düşmesi).
    /// </summary>
    private void HandleComboTimer()
    {
        if (currentCombo > 0)
        {
            comboTimer -= Time.deltaTime;
            if (comboTimer <= 0)
            {
                currentCombo = 0;
                // Opsiyonel: Zincir bitince listeyi temizleyebiliriz ama aynı araca 2 kez combo vermemek için şimdilik tutmaya devam ediyoruz (veya temizleyebiliriz)
                // passedCars.Clear(); 
            }
        }
    }

    /// <summary>
    /// Yakın geçen araçları tespit eder ve Combo / Skor ekler. Yeni Gelişmiş Sistem (Mesafe ve Yön Algılamalı).
    /// </summary>
    private void CheckNearMissContinuous()
    {
        if (currentWorldSpeed < 30f) return; // Belirli bir hızın üstündeyken sayılır

        // Yakın geçiş algılama alanı (Editor'den ayarlanabilir radius)
        Collider[] hits = Physics.OverlapSphere(transform.position, nearMissRadius);
        foreach (var hit in hits)
        {
            if (hit.CompareTag("TrafficCar"))
            {
                int carId = hit.gameObject.GetInstanceID();
                if (!passedCars.Contains(carId))
                {
                    Vector3 closestPoint = hit.ClosestPoint(transform.position);
                    Vector3 toCarSurface = closestPoint - transform.position;
                    Vector3 toCarCenter = hit.transform.position - transform.position;

                    // Önümüzde değilse veya hafif yanımızdaysa kaza olmadan sıyırmışızdır
                    // 0.2f dar bir açıydı, 0.65f daha toleranslı ve güvenli (oyuncunun hakkını yemez)
                    if (Vector3.Dot(transform.forward, toCarCenter.normalized) < 0.65f)
                    {
                        passedCars.Add(carId);

                        // ─── 1. Mesafe ve Kalite Hesaplama ───
                        float dist = toCarSurface.magnitude; // Yüzeye olan net mesafe (Kutu merkeziden ziyade yüzeye yakınlık en doğrusudur)
                        float qualityMultiplier = 1f;
                        string gradeText = LocalizationManager.Instance != null ? LocalizationManager.Instance.GetTranslation("Grade_NearMiss") : "KIL PAYI!";
                        Color gradeColor = new Color(0f, 0.8f, 1f); // Neon Mavi
                        float textSize = 6.5f;

                        if (dist <= 0.85f)
                        {
                            // MÜKEMMEL (Riskli Geçiş)
                            qualityMultiplier = 3f;
                            gradeText = GetTranslationSafe("Grade_Perfect", "MÜKEMMEL!");
                            gradeColor = new Color(1f, 0.15f, 0.45f); // Neon Pink/Red
                            textSize = 10f;
                            
                            // Ekstra Risk Haptik
                            Settings.HapticManager.Heavy();
                            
                            // HitStop (Juicy effect)
                            StartCoroutine(HitStopRoutine(0.08f, 0.05f));
                        }
                        else if (dist <= 1.8f)
                        {
                            // HARİKA
                            qualityMultiplier = 2f;
                            gradeText = GetTranslationSafe("Grade_Amazing", "HARİKA!");
                            gradeColor = new Color(0.1f, 1f, 0.3f); // Neon Green
                            textSize = 8.5f;
                            Settings.HapticManager.Medium();
                        }
                        else
                        {
                            // KIL PAYI
                            qualityMultiplier = 1f;
                            gradeText = GetTranslationSafe("Grade_NearMiss", "KIL PAYI!");
                            gradeColor = new Color(0f, 0.8f, 1f);
                            textSize = 7f;
                            Settings.HapticManager.Light();
                        }

                        // ─── 2. Yön Bulma (Sağdan mı Soldan mı geçtik?) ───
                        float dotRight = Vector3.Dot(transform.right, toCarCenter);
                        float offsetX = (dotRight > 0f) ? 1.5f : -1.5f;

                        // ─── 3. Combo ve Skor ───
                        currentCombo++;
                        comboTimer = 2.5f; // Combo süresi sıfırlanır
                        nearMissCount++;

                        int baseScore = 200;
                        int bonusScore = Mathf.RoundToInt(baseScore * qualityMultiplier * currentCombo);
                        score += bonusScore;

                        // Combo 3'ün katıysa ekstra 1 Yardım Kolisi kazan!
                        bool wonCoin = false;
                        if (currentCombo > 0 && currentCombo % 3 == 0)
                        {
                            coins++;
                            coinFlashTimer = 0.25f; // Ekranda para parlaması
                            wonCoin = true;
                        }

                        // Boost enerjisini doldur
                        currentBoostAmount = Mathf.Clamp01(currentBoostAmount + (0.05f * qualityMultiplier));
                        
                        // ─── 4. Floating Text Gösterimi ───
                        string finalMsg = gradeText + $"\n+{bonusScore}";
                        if (currentCombo > 1) 
                        {
                            string comboFormat = GetTranslationSafe("Game_Combo", "COMBO x{0}!");
                            finalMsg = string.Format(comboFormat, currentCombo) + "\n" + finalMsg;
                        }
                        if (wonCoin) 
                        {
                            string coinMsg = GetTranslationSafe("Game_AwardedCoin", "+1 YARDIM!");
                            finalMsg += $"\n<color=yellow>{coinMsg}</color>";
                        }

                        ShowFloatingText(finalMsg, gradeColor, textSize, offsetX);

                        // Görsel/İşitsel bildirim
                        if (speedText != null) StartCoroutine(FlashNearMissUI());
                        
                        // "Whoosh" veya yükselen ses efekti varsa çalınabilir.
                        if (Settings.AudioManager.Instance != null && coinSound != null)
                        {
                            // Combo'ya göre hafif yükselen pitch efekti
                            Settings.AudioManager.Instance.PlaySFX(coinSound);
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// Ağır çekim / Hit Stop efekti yaratır.
    /// </summary>
    private System.Collections.IEnumerator HitStopRoutine(float duration, float scale)
    {
        float originalScale = Time.timeScale;
        if (originalScale <= 0f) yield break;
        
        Time.timeScale = scale;
        yield return new WaitForSecondsRealtime(duration);
        if (!isGameOver && !Gazze.UI.PauseMenuBuilder.IsPaused)
        {
            Time.timeScale = 1f;
        }
    }

    /// <summary>
    /// Ekranda yüzen bilgilendirme metni gösterir (Skorlar, Combo vb.)
    /// </summary>
    public void ShowFloatingText(string message, Color color, float size, float offsetX)
    {
        GameObject textGO = new GameObject("FloatingText");
        // Yazıyı geçtiğimiz yöne (sağ/sol) doğru ofsetli çıkart
        textGO.transform.position = transform.position + Vector3.up * 2.2f + Vector3.forward * 1.5f + Vector3.right * offsetX;
        
        var tmp = textGO.AddComponent<TextMeshPro>();
        tmp.text = message;
        tmp.fontSize = size;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontStyle = FontStyles.Bold | FontStyles.Italic;
        
        // ─── Avant-Garde Görünüm: Renk ve Gradient ───
        tmp.color = Color.white;
        tmp.enableVertexGradient = true;
        Color bottomColor = new Color(color.r * 0.25f, color.g * 0.25f, color.b * 0.25f, 1f);
        tmp.colorGradient = new VertexGradient(color, color, bottomColor, bottomColor);

        // ─── Avant-Garde Görünüm: Koyu Transparan Outline ───
        tmp.outlineWidth = 0.28f;
        tmp.outlineColor = new Color32(5, 8, 15, 255); // Okyanus Derinliği Siyahı
        
        if (Camera.main != null)
        {
            // Metin Kameraya baksın
            textGO.transform.rotation = Quaternion.LookRotation(Camera.main.transform.forward);
        }
        
        var ft = textGO.AddComponent<Gazze.UI.FloatingText>();
        ft.duration = 1.35f;
        ft.upwardSpeed = 4.5f; // Süzülerek çıkma hissi

        // ─── Pop Animator (AnimationCurve) ───
        AnimationCurve popCurve = new AnimationCurve();
        popCurve.AddKey(new Keyframe(0f, 0f, 0f, 7f)); // Hızla başla
        popCurve.AddKey(new Keyframe(0.2f, 1.25f, 0f, 0f)); // Büyüme tepe noktası (Pop)
        popCurve.AddKey(new Keyframe(1f, 0.9f, -0.6f, 0f)); // Yavaşça küçülerek öl
        ft.scaleCurve = popCurve;
    }

    private System.Collections.IEnumerator FlashNearMissUI()
    {
        if (speedText == null) yield break;
        Color orig = speedText.color;
        speedText.color = Color.yellow;
        yield return new WaitForSeconds(0.2f);
        speedText.color = orig;
    }

    /// <summary>
    /// Boost etkisini sonlandırır.
    /// </summary>
    private void EndBoost()
    {
        isBoosting = false;
        lastBoostTime = Time.time; // Cooldown starts exactly when boost ends
        if (currentBoostEffect != null)
        {
            Destroy(currentBoostEffect);
            currentBoostEffect = null;
        }
        
        // Post-Processing Efektini Durdur
        if (Gazze.VisualEffects.BoostPostProcessManager.Instance != null)
        {
            Gazze.VisualEffects.BoostPostProcessManager.Instance.StopBoostEffect();
        }

        // Debug.Log("Boost bitti.");
    }

    private void UpdateScore()
    {
        score += currentWorldSpeed * Time.deltaTime;
    }

    /// <summary>
    /// UI metinlerini güncel verilerle yeniler.
    /// </summary>
    private void UpdateUI()
    {
        string distLabel = GetTranslationSafe("Game_Distance", "MESAFE");
        string speedLabel = GetTranslationSafe("Game_Speed", "HIZ");
        string helpLabel = GetTranslationSafe("Game_Help", "YARDIM");
        string kmhLabel = GetTranslationSafe("Game_Kmh", "KM/H");

        if (scoreText != null) scoreText.text = $"{distLabel}: " + ((int)score).ToString() + "m";
        if (speedText != null) speedText.text = $"{speedLabel}: " + ((int)currentWorldSpeed).ToString() + " " + kmhLabel;
        if (coinText != null) coinText.text = $"{helpLabel}: " + coins.ToString();

        // Dayanıklılık (Durability) Göstergesi
        if (heartDisplay != null)
        {
            heartDisplay.SetHealth(visualDurability, maxDurability);
            
            // Slider varsa onu gizle veya opsiyonel olarak güncelle (Kullanıcı 'yerine' dediği için gizlenebilir)
            if (durabilitySlider != null && durabilitySlider.gameObject.activeSelf) 
                durabilitySlider.gameObject.SetActive(false);
        }
        else if (durabilitySlider != null && currentAttributes != null)
        {
            durabilitySlider.gameObject.SetActive(true);
            durabilitySlider.maxValue = maxDurability;
            durabilitySlider.value = visualDurability;
            
            float normalizedHealth = visualDurability / maxDurability;

            // 1. Renk Geçişi (Smooth Lerp)
            Image fillImage = durabilitySlider.fillRect.GetComponent<Image>();
            if (fillImage != null)
            {
                Color targetColor;
                if (normalizedHealth > 0.5f)
                    targetColor = Color.Lerp(new Color(1f, 0.8f, 0f), Color.green, (normalizedHealth - 0.5f) * 2f);
                else
                    targetColor = Color.Lerp(Color.red, new Color(1f, 0.8f, 0f), normalizedHealth * 2f);
                
                fillImage.color = targetColor;
            }

            // 2. Kritik Can Pulsing Efekti
            if (normalizedHealth < lowHealthThreshold && !isGameOver)
            {
                float pulse = 1f + Mathf.PingPong(Time.time * 4f, 0.12f);
                durabilitySlider.transform.localScale = durabilityBarInitialScale * pulse;
            }
            else if (barAnimationCoroutine == null)
            {
                durabilitySlider.transform.localScale = durabilityBarInitialScale;
            }
        }
        
        // SpeedText güncellemesi zaten yukarıda yapıldı, gereksiz ve unlocalized olan bu kısım silindi.
        if (boostSlider != null)
        {
            // Değer değişimini yumuşat
            visualBoostAmount = Mathf.Lerp(visualBoostAmount, currentBoostAmount, Time.deltaTime * 8f);
            boostSlider.value = visualBoostAmount;
            Image boostFill = boostSlider.fillRect.GetComponent<Image>();

            // Sprite ve Metin Güncelleme
            if (boostButtonImage != null)
            {
                // Sprite boyutlarının (whitespace) farklı olmasından kaynaklı görsel dengesizliği gidermek için ölçekleme ekliyoruz.
                if (isBoosting)
                {
                    boostButtonImage.sprite = boostUsingSprite;
                    
                    // BOOSTING ANİMASYONU: Titreme (Shake) ve Renk Pulsing
                    float shake = Mathf.PingPong(Time.time * 20f, 0.05f);
                    boostButtonImage.rectTransform.localScale = (boostButtonInitialScale * 0.88f) * (1f + shake);
                    boostButtonImage.preserveAspect = true;
                    
                    if (boostFill != null) boostFill.color = Color.Lerp(boostFill.color, boostSliderUsingColor, Time.deltaTime * 10f);

                    if (boostTimeText != null)
                    {
                        boostTimeText.text = GetTranslationSafe("Boost_Status_Active", "HIZLANDIRILIYOR...");
                        boostTimeText.color = Color.cyan;
                    }
                }
                else if (currentBoostAmount >= 1.0f)
                {
                    boostButtonImage.sprite = boostReadySprite;
                    
                    // READY ANİMASYONU: Nabız (Pulse)
                    boostPulseTimer += Time.deltaTime * 3f;
                    float pulse = 1f + Mathf.Sin(boostPulseTimer) * 0.1f;
                    boostButtonImage.rectTransform.localScale = boostButtonInitialScale * pulse;
                    boostButtonImage.preserveAspect = true;

                    if (boostFill != null) 
                    {
                        float glow = 0.8f + Mathf.PingPong(Time.time * 2f, 0.2f);
                        boostFill.color = boostSliderReadyColor * glow;
                    }

                    if (boostTimeText != null)
                    {
                        boostTimeText.text = GetTranslationSafe("Boost_Status_Ready", "HAZIR");
                        boostTimeText.color = Color.green;
                    }
                }
                else
                {
                    boostButtonImage.sprite = boostRefillingSprite;
                    boostButtonImage.rectTransform.localScale = Vector3.Lerp(boostButtonImage.rectTransform.localScale, boostButtonInitialScale, Time.deltaTime * 5f);
                    boostButtonImage.preserveAspect = true;
                    
                    if (boostFill != null) 
                    {
                        // Doluluk miktarına göre renk geçişi: Kırmızı -> Turuncu -> Sarı -> Turkuaz (0, 180, 255)
                        Color fillingColor;
                        Color targetCyan = boostSliderReadyColor;
                        Color orange = new Color(1f, 0.5f, 0f);

                        if (currentBoostAmount < 0.33f)
                            fillingColor = Color.Lerp(Color.red, orange, currentBoostAmount * 3f);
                        else if (currentBoostAmount < 0.66f)
                            fillingColor = Color.Lerp(orange, Color.yellow, (currentBoostAmount - 0.33f) * 3f);
                        else
                            fillingColor = Color.Lerp(Color.yellow, targetCyan, (currentBoostAmount - 0.66f) * 3f);
                            
                        boostFill.color = Color.Lerp(boostFill.color, fillingColor, Time.deltaTime * 5f);
                    }

                    if (boostTimeText != null)
                    {
                        boostTimeText.text = (currentBoostAmount * 100f).ToString("F0") + "%";
                        boostTimeText.color = Color.white;
                    }
                    
                    // Cooldown (Empty) uyarısı: Eğer boşsa slider hafifçe titreyebilir
                    if (isBoostEmpty && boostSlider.fillRect != null)
                    {
                        boostSlider.fillRect.anchoredPosition = new Vector2(Mathf.Sin(Time.time * 30f) * 2f, 0);
                    }
                    else if (boostSlider.fillRect != null)
                    {
                        boostSlider.fillRect.anchoredPosition = Vector2.zero;
                    }
                }
            }
        }

        // Wind Effect Overlay (Rüzgar çizgileri)
        if (windEffectOverlay != null)
        {
            if (isBoosting)
            {
                if (!windEffectOverlay.gameObject.activeSelf) windEffectOverlay.gameObject.SetActive(true);
                // Hızlı yanıp sönen rüzgar çizgileri
                float alpha = 0.2f + Mathf.PingPong(Time.time * 15f, 0.4f);
                windEffectOverlay.color = new Color(1, 1, 1, alpha);
                
                // Hafif ölçeklenme efekti (zoom hissi)
                float scale = 1f + Mathf.PingPong(Time.time * 2f, 0.05f);
                windEffectOverlay.transform.localScale = Vector3.one * scale;
            }
            else
            {
                if (windEffectOverlay.gameObject.activeSelf)
                {
                    windEffectOverlay.color = Color.Lerp(windEffectOverlay.color, new Color(1, 1, 1, 0), Time.deltaTime * 10f);
                    if (windEffectOverlay.color.a < 0.01f) windEffectOverlay.gameObject.SetActive(false);
                }
            }
        }

        // Boost Overlay (Renkli Overlay)
        if (boostOverlay != null)
        {
            if (isBoosting)
            {
                if (!boostOverlay.gameObject.activeSelf) boostOverlay.gameObject.SetActive(true);
                float alpha = 0.3f + Mathf.PingPong(Time.time * 2f, 0.3f);
                boostOverlay.color = new Color(1, 1, 1, alpha);
            }
            else
            {
                if (boostOverlay.gameObject.activeSelf)
                {
                    boostOverlay.color = Color.Lerp(boostOverlay.color, new Color(1, 1, 1, 0), Time.deltaTime * 5f);
                    if (boostOverlay.color.a < 0.01f) boostOverlay.gameObject.SetActive(false);
                }
            }
        }
    }

    /// <summary>
    /// Yüksek hızlarda nesnelerin içinden geçme (tunneling) hatasını önlemek için Sweep (OverlapBox) kontrolü yapar.
    /// </summary>
    private void CheckHighSpeedCollectibles()
    {
        if (currentWorldSpeed < 30f) return;

        float moveDist = currentWorldSpeed * Time.deltaTime;
        BoxCollider bc = GetComponent<BoxCollider>();
        if (bc == null) return;

        Vector3 center = transform.TransformPoint(bc.center);
        Vector3 halfExtents = Vector3.Scale(bc.size, transform.lossyScale) * 0.5f;
        
        halfExtents.z += moveDist * 0.5f;
        center += transform.forward * (moveDist * 0.5f);

        Collider[] hits = Physics.OverlapBox(center, halfExtents, transform.rotation);
        foreach (Collider hit in hits)
        {
            if (hit.CompareTag("Coin"))
            {
                GameObject root = hit.attachedRigidbody != null ? hit.attachedRigidbody.gameObject : hit.gameObject;
                ProcessCoinTrigger(root);
            }
        }
    }

    public void ProcessCoinTrigger(GameObject coinRoot)
    {
        if (isGameOver) return;
        
        if (coinRoot.TryGetComponent<Gazze.PowerUps.PowerUpItem>(out var pui)) return;

        Gazze.Collectibles.CoinController cc = coinRoot.GetComponent<Gazze.Collectibles.CoinController>();
        if (cc == null) cc = coinRoot.GetComponentInParent<Gazze.Collectibles.CoinController>();
        
        if (cc != null)
        {
            if (cc.IsCollected) return;
            cc.Collect();
            coins++;
            
            // Start coin collection flash
            coinFlashTimer = 0.25f;

            if (coinSound != null && Settings.AudioManager.Instance != null)
                Settings.AudioManager.Instance.PlaySFX(coinSound);
            Settings.HapticManager.Medium(); // Intensity increased for coins
        }
        else
        {
            if (!coinRoot.activeSelf) return;
            coinRoot.SetActive(false);
            coins++;
            if (coinSound != null && Settings.AudioManager.Instance != null)
                Settings.AudioManager.Instance.PlaySFX(coinSound);
            Settings.HapticManager.Medium();
        }
    }

    /// <summary>
    /// Diğer objelerle olan çarpışmaları kontrol eder.
    /// </summary>
    private void OnTriggerEnter(Collider other)
    {
        if (isGameOver) return;
        
        // Debug.Log($"Trigger Enter with: {other.name}, Tag: {other.tag}");

        if (other.CompareTag("Obstacle") || other.CompareTag("TrafficCar"))
        {
            HandleCollision(other);
        }
        else if (other.CompareTag("Coin"))
        {
            // Debug.Log("Coin/PowerUp Trigger Detected!");
            GameObject coinRoot = other.attachedRigidbody != null ? other.attachedRigidbody.gameObject : other.gameObject;
            ProcessCoinTrigger(coinRoot);
        }
        else if (other.CompareTag("Boost"))
        {
            currentBoostAmount = 1.0f;
            isBoostEmpty = false;
            other.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Çarpışma anında görsel ve işitsel efektleri tetikler ve oyun sonu mantığını başlatır.
    /// </summary>
    private void HandleCollision(Collider other)
    {
        // ── Juggernaut (Dev Modu): Çarpışma yerine aracı fırlat ──────────────────────
        if (Gazze.PowerUps.PowerUpEffects.Instance != null && Gazze.PowerUps.PowerUpEffects.Instance.IsJuggernautActive)
        {
            // Dev modunda aracı fırlatma PowerUpEffects tarafından yapılıyor (CheckJuggernautCollisions)
            // Burada sadece küçük bir kamera sarsıntısı ve ses efekti ver
            if (Gazze.CameraSystem.SmoothCameraFollow.Instance != null)
                Gazze.CameraSystem.SmoothCameraFollow.Instance.TriggerBoostShake();
            Settings.HapticManager.Medium();
            return; // Hasar alma, durma yok!
        }

        if (isInvulnerable) return;

        // ── Power-Up Immunity (Yeni) ──────────────────────
        if (Gazze.PowerUps.PowerUpManager.Instance != null)
        {
            if (Gazze.PowerUps.PowerUpManager.Instance.IsPowerUpActive(Gazze.PowerUps.PowerUpType.Ghost))
            {
                // Ghost modunda her şeyin içinden geçer, etkileşim yok.
                return;
            }

            if (Gazze.PowerUps.PowerUpManager.Instance.IsPowerUpActive(Gazze.PowerUps.PowerUpType.Shield))
            {
                // Shield aktifse çarpışmayı yok say (veya efekti oynat ama durma)
                // Şimdilik tamamen yok sayalım ki "yavaşlama" olmasın.
                return;
            }
        }

        // 1. Görsel Efekt (Patlama/Çarpışma)
        if (crashEffectPrefab != null)
        {
            Instantiate(crashEffectPrefab, transform.position + Vector3.forward, Quaternion.identity);
        }

        // 3. Ekran Sarsıntısı (Kaza)
        if (Gazze.CameraSystem.SmoothCameraFollow.Instance != null)
        {
            Gazze.CameraSystem.SmoothCameraFollow.Instance.TriggerCrashShake();
        }

        // 4. Crash Overlay Flaş Efekti
        if (crashOverlay != null)
        {
            StartCoroutine(FlashCrashOverlay());
        }

        // 5. Fiziksel tepki (Aracı durdur veya savur)
        float crashSpeed = currentWorldSpeed;
        currentWorldSpeed = 0;
        
        // 6. Boost'u iptal et
        if (isBoosting) EndBoost();
        
        // 7. Hasar ver (Hıza Göre Ölçeklendirilmiş Hasar)
        // 20 km/h altı %10, 100 km/h ve üstü %40 hasar. Arası lineer.
        // float dmg = Mathf.Lerp(10f, 40f, Mathf.InverseLerp(20f, 100f, crashSpeed));
        TakeDamage(25f); 
        Settings.HapticManager.Heavy();
        
        // 7. Koruma süresini başlat (Hasar alsa bile kaza sonrası 3 sn koruma)
        if (currentDurability > 0)
        {
            StartInvulnerability(invulnerabilityDuration);
        }
    }

    /// <summary>
    /// Koruma süresini (Invulnerability) başlatır.
    /// </summary>
    public void StartInvulnerability(float duration)
    {
        isInvulnerable = true;
        invulnerabilityTimer = duration;
        // Debug.Log("Koruma süresi başladı!");
    }

    private Renderer[] _invulnerabilityRenderers;

    /// <summary>
    /// Koruma süresini takip eder ve görsel efekti (yanıp sönme) uygular.
    /// </summary>
    private void HandleInvulnerability()
    {
        if (!isInvulnerable) return;

        invulnerabilityTimer -= Time.deltaTime;
        
        // Görsel Efekt: Yanıp Sönme (Blink)
        float blinkSpeed = (invulnerabilityTimer < 1f) ? 15f : 8f;
        bool isVisible = Mathf.PingPong(Time.time * blinkSpeed, 1f) > 0.5f;
        
        // Optimizasyon: Her frame'de GetComponentsInChildren çağırmak yerine cache'le
        if (_invulnerabilityRenderers == null || _invulnerabilityRenderers.Length == 0)
        {
            _invulnerabilityRenderers = GetComponentsInChildren<Renderer>();
        }

        foreach (var r in _invulnerabilityRenderers)
        {
            if (r != null) r.enabled = isVisible;
        }

        if (invulnerabilityTimer <= 0)
        {
            isInvulnerable = false;
            foreach (var r in _invulnerabilityRenderers)
            {
                if (r != null) r.enabled = true;
            }
            _invulnerabilityRenderers = null; // Belleği serbest bırak
        }
    }

    /// <summary>
    /// Çarpışma anında ekranda kırmızı bir flaş efekti oluşturur.
    /// </summary>
    private System.Collections.IEnumerator FlashCrashOverlay()
    {
        float elapsed = 0f;
        float duration = 0.4f;
        
        crashOverlay.gameObject.SetActive(true);
        // Görselin orijinal renklerini kullan (Beyaz + Alpha)
        crashOverlay.color = new Color(1, 1, 1, 0.9f);
        
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float alpha = Mathf.Lerp(0.9f, 0f, elapsed / duration);
            crashOverlay.color = new Color(1, 1, 1, alpha);
            yield return null;
        }
        
        crashOverlay.color = new Color(1, 1, 1, 0f);
        crashOverlay.gameObject.SetActive(false);
    }

    /// <summary>
    /// Boost etkisini başlatır (Bekleme süresi kontrolü ile).
    /// </summary>
    public void ActivateBoost()
    {
        if (isGameOver || isBoosting || isBoostEmpty || currentBoostAmount <= 0 || Gazze.UI.PauseMenuBuilder.IsPaused) return;
        
        isBoosting = true;

        // Post-Processing Efektini Başlat
        if (Gazze.VisualEffects.BoostPostProcessManager.Instance != null)
        {
            Gazze.VisualEffects.BoostPostProcessManager.Instance.StartBoostEffect();
        }

        // Boost Kamera Sarsıntısı
        if (Gazze.CameraSystem.SmoothCameraFollow.Instance != null)
        {
            Gazze.CameraSystem.SmoothCameraFollow.Instance.TriggerBoostShake();
        }

        // Ses Efekti
        if (boostSound != null && Settings.AudioManager.Instance != null)
        {
            Settings.AudioManager.Instance.PlaySFX(boostSound);
        }

        // Görsel Efekt
        if (boostEffectPrefab != null)
        {
            currentBoostEffect = Instantiate(boostEffectPrefab, transform);
            currentBoostEffect.transform.localPosition = Vector3.zero;
        }

        Settings.HapticManager.Medium();
    }

    /// <summary>
    /// Dayanıklılık barını hasar alındığında sarsar ve büyütür.
    /// </summary>
    private System.Collections.IEnumerator AnimateDurabilityBarOnDamage()
    {
        if (durabilitySlider == null) yield break;

        RectTransform rt = durabilitySlider.GetComponent<RectTransform>();
        float elapsed = 0f;
        float duration = 0.35f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float percent = elapsed / duration;

            // Sarsıntı (Shake)
            float curve = 1f - percent;
            float offsetX = Random.Range(-barShakeIntensity, barShakeIntensity) * curve;
            float offsetY = Random.Range(-barShakeIntensity, barShakeIntensity) * curve;
            rt.anchoredPosition = durabilityBarInitialPos + new Vector3(offsetX, offsetY, 0);

            // Büyüme (Punch)
            float scaleMultiplier = Mathf.Lerp(barPunchAmount, 1f, percent);
            rt.localScale = durabilityBarInitialScale * scaleMultiplier;

            yield return null;
        }

        rt.anchoredPosition = durabilityBarInitialPos;
        rt.localScale = durabilityBarInitialScale;
        barAnimationCoroutine = null;
    }

    /// <summary>
    /// Oyunun bitiş durumunu tetikler ve final ekranını hazırlar.
    /// </summary>
    public void GameOver()
    {
        if (isGameOver) return;
        StartCoroutine(GameOverRoutine());
    }

    private System.Collections.IEnumerator GameOverRoutine()
    {
        // Eğer pause menüsü açıksa kapat
        Gazze.UI.PauseMenuBuilder.ForceClose();
        
        isGameOver = true;

        // ─── 1. Hit Stop & Impact ───
        Time.timeScale = 0.05f; // Neredeyse durdur (Darbe hissi)
        Settings.HapticManager.DeathSequence(this);
        
        if (deathSound != null && Settings.AudioManager.Instance != null)
            Settings.AudioManager.Instance.PlaySFX(deathSound);

        if (Gazze.CameraSystem.SmoothCameraFollow.Instance != null)
            Gazze.CameraSystem.SmoothCameraFollow.Instance.TriggerDeathShake();

        yield return new WaitForSecondsRealtime(0.15f);

        // ─── 2. Slow Motion Transition ───
        float elapsed = 0f;
        float duration = 0.8f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            // 0.2f'den 0'a doğru yavaşla
            Time.timeScale = Mathf.Lerp(0.2f, 0f, elapsed / duration);
            yield return null;
        }
        Time.timeScale = 0f;

        // ─── 3. Death Cutscene (6-Segment Cinematic Sequence) ───
        if (Gazze.Cutscene.CutsceneManager.Instance != null && Camera.main != null)
        {
            Time.timeScale = 1f;

            Gazze.Cutscene.CutsceneManager.Instance.ClearSegments();

            Vector3 playerPos = transform.position;
            Quaternion playerRot = transform.rotation;
            float camFOV = Camera.main.fieldOfView;

            // ── Kamera Noktaları (6 aşamalı sinematik yörünge) ──

            // Pt0: Orijinal kamera konumu (çarpışma anı)
            GameObject pt0 = new GameObject("Death_Pt0_Origin");
            pt0.transform.position = Camera.main.transform.position;
            pt0.transform.rotation = Camera.main.transform.rotation;

            // Pt1: Yakın çekim — arabanın ön tarafına zoom (close-up freeze)
            GameObject pt1 = new GameObject("Death_Pt1_CloseUp");
            pt1.transform.position = playerPos + transform.forward * 3f + transform.up * 1.5f + transform.right * 0.5f;
            pt1.transform.rotation = Quaternion.LookRotation(playerPos - pt1.transform.position);

            // Pt2: Alçak açı — aracın altından dramatik bakış (low-angle sweep)
            GameObject pt2 = new GameObject("Death_Pt2_LowAngle");
            pt2.transform.position = playerPos - transform.forward * 2f + transform.up * 0.4f + transform.right * 3f;
            pt2.transform.rotation = Quaternion.LookRotation((playerPos + Vector3.up * 1f) - pt2.transform.position);

            // Pt3: Yan dolly — arabanın sol yanından kayarak geçen çekim
            GameObject pt3 = new GameObject("Death_Pt3_SideDolly");
            pt3.transform.position = playerPos - transform.right * 7f + transform.up * 2.5f + transform.forward * 2f;
            pt3.transform.rotation = Quaternion.LookRotation(playerPos - pt3.transform.position);

            // Pt4: 360° yörünge — arabanın arkasından sağ tarafa doğru ark çizer
            GameObject pt4 = new GameObject("Death_Pt4_OrbitArc");
            pt4.transform.position = playerPos + transform.right * 8f + transform.up * 4f - transform.forward * 3f;
            pt4.transform.rotation = Quaternion.LookRotation(playerPos - pt4.transform.position);

            // Pt5: Dramatik geri çekilme — yukarı + geriye doğru crane shot
            GameObject pt5 = new GameObject("Death_Pt5_CranePull");
            pt5.transform.position = playerPos + transform.up * 10f - transform.forward * 8f;
            pt5.transform.rotation = Quaternion.LookRotation(playerPos - pt5.transform.position);

            // Pt6: Final kuş bakışı — en yukarıdan aşağı iniş (aerial descent)
            GameObject pt6 = new GameObject("Death_Pt6_AerialFinal");
            pt6.transform.position = playerPos + transform.up * 18f - transform.forward * 14f + transform.right * 3f;
            pt6.transform.rotation = Quaternion.LookRotation(playerPos - pt6.transform.position);

            // ═══════════════════════════════════════════════════════
            // Segment 1: DARBE ANI (Impact Freeze) — Bullet-time close-up
            // ═══════════════════════════════════════════════════════
            Gazze.Cutscene.CutsceneManager.Instance.AddSegment(new Gazze.Cutscene.CutsceneManager.CutsceneSegment
            {
                segmentName = "Impact Freeze",
                startPoint = pt0.transform,
                endPoint = pt1.transform,
                duration = 1.0f,
                smoothTransition = true,
                useDynamicFOV = true,
                startFOV = camFOV,
                endFOV = 30f,
                easeType = Gazze.Cutscene.CutsceneManager.CameraEaseType.EaseOut,
                lookAtPlayer = true,
                lookAtSpeed = 12f,
                useSlowMotion = true,
                timeScale = 0.05f,
                shakeCameraOnStart = true,
                shakeIntensity = 1.5f,
                shakeDuration = 0.6f,
                showSubtitle = false
            });

            // ═══════════════════════════════════════════════════════
            // Segment 2: ALÇAK AÇI SÜPÜRME (Low Angle Sweep)
            // ═══════════════════════════════════════════════════════
            Gazze.Cutscene.CutsceneManager.Instance.AddSegment(new Gazze.Cutscene.CutsceneManager.CutsceneSegment
            {
                segmentName = "Low Angle Sweep",
                startPoint = pt1.transform,
                endPoint = pt2.transform,
                duration = 1.4f,
                smoothTransition = true,
                useDynamicFOV = true,
                startFOV = 30f,
                endFOV = 50f,
                easeType = Gazze.Cutscene.CutsceneManager.CameraEaseType.SmoothStep,
                lookAtPlayer = true,
                lookAtSpeed = 8f,
                lookAtOffset = Vector3.up * 0.8f,
                useSlowMotion = true,
                timeScale = 0.12f,
                showSubtitle = false
            });

            // ═══════════════════════════════════════════════════════
            // Segment 3: YAN DOLLY (Side Dolly Track)
            // ═══════════════════════════════════════════════════════
            Gazze.Cutscene.CutsceneManager.Instance.AddSegment(new Gazze.Cutscene.CutsceneManager.CutsceneSegment
            {
                segmentName = "Side Dolly",
                startPoint = pt2.transform,
                endPoint = pt3.transform,
                duration = 1.8f,
                smoothTransition = true,
                useDynamicFOV = true,
                startFOV = 50f,
                endFOV = 42f,
                easeType = Gazze.Cutscene.CutsceneManager.CameraEaseType.EaseInOut,
                lookAtPlayer = true,
                smoothLookAt = true,
                lookAtSpeed = 6f,
                useSlowMotion = true,
                timeScale = 0.15f,
                showSubtitle = false
            });

            // ═══════════════════════════════════════════════════════
            // Segment 4: YÖRÜNGE ARKI (Orbit Arc — 360° hareket)
            // ═══════════════════════════════════════════════════════
            Gazze.Cutscene.CutsceneManager.Instance.AddSegment(new Gazze.Cutscene.CutsceneManager.CutsceneSegment
            {
                segmentName = "Orbit Arc",
                startPoint = pt3.transform,
                endPoint = pt4.transform,
                duration = 2.2f,
                smoothTransition = true,
                useDynamicFOV = true,
                startFOV = 42f,
                endFOV = 35f,
                easeType = Gazze.Cutscene.CutsceneManager.CameraEaseType.SmootherStep,
                lookAtPlayer = true,
                smoothLookAt = true,
                lookAtSpeed = 5f,
                useSlowMotion = true,
                timeScale = 0.2f,
                showSubtitle = false
            });

            // ═══════════════════════════════════════════════════════
            // Segment 5: DRAMATİK GERİ ÇEKİLME (Crane Pull-Back)
            // ═══════════════════════════════════════════════════════
            Gazze.Cutscene.CutsceneManager.Instance.AddSegment(new Gazze.Cutscene.CutsceneManager.CutsceneSegment
            {
                segmentName = "Crane Pull-Back",
                startPoint = pt4.transform,
                endPoint = pt5.transform,
                duration = 2.0f,
                smoothTransition = true,
                useDynamicFOV = true,
                startFOV = 35f,
                endFOV = 28f,
                easeType = Gazze.Cutscene.CutsceneManager.CameraEaseType.EaseIn,
                lookAtPlayer = true,
                smoothLookAt = true,
                lookAtSpeed = 4f,
                useSlowMotion = true,
                timeScale = 0.25f,
                shakeCameraOnStart = true,
                shakeIntensity = 0.3f,
                shakeDuration = 0.4f,
                showSubtitle = false
            });

            // ═══════════════════════════════════════════════════════
            // Segment 6: FİNAL HAVADAN İNİŞ (Aerial Descent — Kuş Bakışı)
            // ═══════════════════════════════════════════════════════
            Gazze.Cutscene.CutsceneManager.Instance.AddSegment(new Gazze.Cutscene.CutsceneManager.CutsceneSegment
            {
                segmentName = "Aerial Descent",
                startPoint = pt5.transform,
                endPoint = pt6.transform,
                duration = 2.5f,
                smoothTransition = true,
                useDynamicFOV = true,
                startFOV = 28f,
                endFOV = 22f,
                easeType = Gazze.Cutscene.CutsceneManager.CameraEaseType.EaseOut,
                lookAtPlayer = true,
                smoothLookAt = true,
                lookAtSpeed = 3f,
                useSlowMotion = true,
                timeScale = 0.3f,
                showSubtitle = false
            });

            // Ölüm ekranından sonra yeniden başlatma sayacını engelle
            Gazze.Cutscene.CutsceneManager.Instance.startCountdownAfter = false;
            Gazze.Cutscene.CutsceneManager.Instance.PlayCutscene();

            while (Gazze.Cutscene.CutsceneManager.Instance.IsPlaying)
            {
                yield return null;
            }

            // Temizlik — tüm geçici noktaları yok et
            if (pt0 != null) Destroy(pt0);
            if (pt1 != null) Destroy(pt1);
            if (pt2 != null) Destroy(pt2);
            if (pt3 != null) Destroy(pt3);
            if (pt4 != null) Destroy(pt4);
            if (pt5 != null) Destroy(pt5);
            if (pt6 != null) Destroy(pt6);
        }

        Time.timeScale = 0f;

        // Skor kaydetme ve High Score kontrolü
        int currentScore = (int)score;
        int lastHighScore = PlayerPrefs.GetInt("HighScore", 0);
        if (currentScore > lastHighScore)
        {
            PlayerPrefs.SetInt("HighScore", currentScore);
        }

        // Toplanan yardımları (coins) Kredi olarak hesaba ekle (Her yardım = 500 Kredi)
        if (coins > 0)
        {
            int currentKredi = PlayerPrefs.GetInt("TotalKredi", 0);
            PlayerPrefs.SetInt("TotalKredi", currentKredi + (coins * 500));
        }
        PlayerPrefs.Save();

        var data = new Gazze.UI.GameOverPanelBuilder.Data
        {
            score = currentScore,
            highScore = Mathf.Max(currentScore, lastHighScore),
            level = CalculateLevel(currentScore, coins),
            playTimeSeconds = playTime,
            nearMisses = nearMissCount,
            achievements = CalculateAchievements(currentScore, coins),
            onRestart = RestartGame,
            onMainMenu = MainMenu,
            onShare = ShareScore,
            onSettings = GoToSettings
        };
        Gazze.UI.GameOverPanelBuilder.Build(data);
        
        // Final UI Güncellemesi
        visualDurability = 0f; 
        currentDurability = 0f;

        if (barAnimationCoroutine != null)
        {
            StopCoroutine(barAnimationCoroutine);
            barAnimationCoroutine = null;
        }
        UpdateUI();
        
        if (Gazze.VisualEffects.BoostPostProcessManager.Instance != null)
        {
            Gazze.VisualEffects.BoostPostProcessManager.Instance.StopBoostEffect();
        }
    }

    #region Kontrol Sistemi API (UI Tarafından Çağrılır)

    public void OnLeftButton(bool isDown) { isMovingLeft = isDown; }
    public void OnRightButton(bool isDown) { isMovingRight = isDown; }
    public void OnGasButton(bool isDown) { isGassingFromUI = isDown; }
    public void OnBrakeButton(bool isDown) { isBrakingFromUI = isDown; }

    public void OnJoystickUpdate(Vector2 input)
    {
        joystickInput = input;
        if (joystickHandle != null)
        {
            joystickHandle.anchoredPosition = input * joystickRadius;
        }
    }

    public void CalibrateAccelerometer()
    {
        currentAccelOffset = Input.acceleration.x;
        PlayerPrefs.SetFloat(Settings.SettingsModel.AccelerometerOffsetKey, currentAccelOffset);
        PlayerPrefs.Save();
    }

    #endregion

    /// <summary>
    /// Oyunu tamamen sıfırlar ve yeniden başlatır.
    /// </summary>
    public void RestartGame()
    {
        Gazze.UI.PauseMenuBuilder.ForceClose();
        if (Settings.AudioManager.Instance != null) Settings.AudioManager.Instance.PlayClickSound();
        Time.timeScale = 1f;
        if (LoadingManager.Instance != null)
        {
            LoadingManager.Instance.LoadScene(SceneManager.GetActiveScene().name);
        }
        else
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }

    /// <summary>
    /// Ana menüye döner.
    /// </summary>
    public void MainMenu()
    {
        Gazze.UI.PauseMenuBuilder.ForceClose();
        if (Settings.AudioManager.Instance != null) Settings.AudioManager.Instance.PlayClickSound();
        Time.timeScale = 1f;
        if (LoadingManager.Instance != null)
        {
            LoadingManager.Instance.LoadScene("MainMenu");
        }
        else
        {
            SceneManager.LoadScene("MainMenu");
        }
    }

    public void GoToSettings()
    {
        Gazze.UI.PauseMenuBuilder.ForceClose();
        if (Settings.AudioManager.Instance != null) Settings.AudioManager.Instance.PlayClickSound();
        Time.timeScale = 1f;
        PlayerPrefs.SetInt("OpenSettingsOnStart", 1);
        PlayerPrefs.Save();
        if (LoadingManager.Instance != null)
        {
            LoadingManager.Instance.LoadScene("MainMenu");
        }
        else
        {
            SceneManager.LoadScene("MainMenu");
        }
    }

    public void ShareScore()
    {
        if (Settings.AudioManager.Instance != null) Settings.AudioManager.Instance.PlayClickSound();
        string text = "Skorum: " + ((int)score).ToString() + "m | Yardım: " + coins.ToString();
        GUIUtility.systemCopyBuffer = text;
        string url = "https://twitter.com/intent/tweet?text=" + UnityEngine.Networking.UnityWebRequest.EscapeURL(text);
        Application.OpenURL(url);
    }

    /// <summary>
    /// Oyundan tamamen çıkar.
    /// </summary>
    public void ExitGame()
    {
        if (Settings.AudioManager.Instance != null) Settings.AudioManager.Instance.PlayClickSound();
        // Debug.Log("OYUNDAN ÇIKILIYOR...");
        Application.Quit();
    }

    // Dokunmatik butonlar için metodlar
    public void GasDown() { if (Gazze.UI.PauseMenuBuilder.IsPaused) return; isGassingFromUI = true; }
    public void GasUp() { isGassingFromUI = false; }
    public void BrakeDown() { if (Gazze.UI.PauseMenuBuilder.IsPaused) return; isBrakingFromUI = true; }
    public void BrakeUp() { isBrakingFromUI = false; }

    /// <summary> Sola gitme butonuna basıldığında çağrılır. </summary>
    public void MoveLeftDown() { if (Gazze.UI.PauseMenuBuilder.IsPaused) return; isMovingLeft = true; isMovingRight = false; }
    /// <summary> Sağa gitme butonuna basıldığında çağrılır. </summary>
    public void MoveRightDown() { if (Gazze.UI.PauseMenuBuilder.IsPaused) return; isMovingRight = true; isMovingLeft = false; }
    /// <summary> Yön butonları bırakıldığında çağrılır. </summary>
    public void StopHorizontal() { isMovingLeft = false; isMovingRight = false; }
    /// <summary> Boost butonuna basıldığında çağrılır. </summary>
    public void BoostDown() { if (Gazze.UI.PauseMenuBuilder.IsPaused) return; isBoostButtonHeldFromUI = true; }
    /// <summary> Boost butonu bırakıldığında çağrılır. </summary>
    public void BoostUp() { isBoostButtonHeldFromUI = false; }

    /// <summary> Unity Button OnClick() olayından çağrıldığında kısa bir süreliğine boost'u tetiklemeyi dener. </summary>
    public void OnBoostButtonClick() 
    { 
        if (!isBoosting && currentBoostAmount > 0.1f && !isBoostEmpty && !Gazze.UI.PauseMenuBuilder.IsPaused)
        {
            ActivateBoost();
            // Tıklama durumunda 0.2 saniye sonra durması için isBoostButtonHeldFromUI'ı geçici açıp kapatabiliriz
            // Ama en sağlıklısı Butona "Event Trigger" eklemektir.
        }
    }

    private int CalculateLevel(int sc, int c)
    {
        int lvl = 1 + sc / 500 + c / 10;
        if (lvl < 1) lvl = 1;
        return lvl;
    }

    private System.Collections.Generic.List<string> CalculateAchievements(int sc, int c)
    {
        // "İlk Adım", ilk oyunu bitiren herkese verilmeli (eğer daha önceden almadıysa)
        string firstStepTitle = Gazze.UI.LocalizationManager.Get("Ach_FirstStep_Title", "İLK ADIM");
        if (!HasAchievement(firstStepTitle))
        {
            string firstStepDesc = Gazze.UI.LocalizationManager.Get("Ach_FirstStep_Desc", "OYUNU BİTİRDİN!");
            ShowAchievementNotification(firstStepTitle, firstStepDesc);
        }
        
        return new System.Collections.Generic.List<string>(sessionAchievements);
    }
    
    private bool HasAchievement(string achievementName)
    {
        return PlayerPrefs.GetInt($"Achievement_{achievementName}", 0) == 1;
    }
    
    private void ShowAchievementNotification(string title, string description)
    {
        // Başarımı kaydet
        PlayerPrefs.SetInt($"Achievement_{title}", 1);
        PlayerPrefs.Save();
        
        // Bildirimi göster
        if (Gazze.UI.AchievementNotificationManager.Instance != null)
        {
            Gazze.UI.AchievementNotificationManager.Instance.ShowAchievement(title, description);
        }

        if (!sessionAchievements.Contains(title))
        {
            sessionAchievements.Add(title);
        }
    }
    
    /// <summary>
    /// Oyun sırasında gerçek zamanlı başarım kontrolü
    /// </summary>
    private void CheckRealtimeAchievements()
    {
        // Skor bazlı başarımlar
        string longWayTitle = Gazze.UI.LocalizationManager.Get("Ach_LongWay_Title", "UZUN YOL");
        if (score >= 1000 && !HasAchievement(longWayTitle))
        {
            string longWayDesc = Gazze.UI.LocalizationManager.Get("Ach_LongWay_Desc", "1000 SKORA ULAŞILDI!");
            ShowAchievementNotification(longWayTitle, longWayDesc);
        }
        
        string marathonTitle = Gazze.UI.LocalizationManager.Get("Ach_Marathon_Title", "MARATON");
        if (score >= 5000 && !HasAchievement(marathonTitle))
        {
            string marathonDesc = Gazze.UI.LocalizationManager.Get("Ach_Marathon_Desc", "5000 SKORA ULAŞILDI!");
            ShowAchievementNotification(marathonTitle, marathonDesc);
        }
        
        // Coin bazlı başarımlar
        string helpfulTitle = Gazze.UI.LocalizationManager.Get("Ach_Helpful_Title", "YARDIMSEVER");
        if (coins >= 10 && !HasAchievement(helpfulTitle))
        {
            string helpfulDesc = Gazze.UI.LocalizationManager.Get("Ach_Helpful_Desc", "10 COIN TOPLANDI!");
            ShowAchievementNotification(helpfulTitle, helpfulDesc);
        }
        
        string generousTitle = Gazze.UI.LocalizationManager.Get("Ach_Generous_Title", "CÖMERT");
        if (coins >= 50 && !HasAchievement(generousTitle))
        {
            string generousDesc = Gazze.UI.LocalizationManager.Get("Ach_Generous_Desc", "50 COIN TOPLANDI!");
            ShowAchievementNotification(generousTitle, generousDesc);
        }
        
        // Near miss bazlı başarımlar
        string nearMissTitle = Gazze.UI.LocalizationManager.Get("Ach_NearMiss_Title", "YAKIN GEÇİŞ");
        if (nearMissCount >= 5 && !HasAchievement(nearMissTitle))
        {
            string nearMissDesc = Gazze.UI.LocalizationManager.Get("Ach_NearMiss_Desc", "5 YAKIN GEÇİŞ YAPILDI!");
            ShowAchievementNotification(nearMissTitle, nearMissDesc);
        }
        
        string dangerTitle = Gazze.UI.LocalizationManager.Get("Ach_DangerHunter_Title", "TEHLİKE AVCISI");
        if (nearMissCount >= 20 && !HasAchievement(dangerTitle))
        {
            string dangerDesc = Gazze.UI.LocalizationManager.Get("Ach_DangerHunter_Desc", "20 YAKIN GEÇİŞ YAPILDI!");
            ShowAchievementNotification(dangerTitle, dangerDesc);
        }
        
        // Hız bazlı başarımlar
        string speedLoverTitle = Gazze.UI.LocalizationManager.Get("Ach_SpeedLover_Title", "HIZ TUTKUNU");
        if (currentWorldSpeed >= 45f && !HasAchievement(speedLoverTitle))
        {
            string speedLoverDesc = Gazze.UI.LocalizationManager.Get("Ach_SpeedLover_Desc", "45 KM/H HIZA ULAŞILDI!");
            ShowAchievementNotification(speedLoverTitle, speedLoverDesc);
        }
        
        string supersonicTitle = Gazze.UI.LocalizationManager.Get("Ach_Supersonic_Title", "SÜPERSONİK");
        if (currentWorldSpeed >= 60f && !HasAchievement(supersonicTitle))
        {
            string supersonicDesc = Gazze.UI.LocalizationManager.Get("Ach_Supersonic_Desc", "60 KM/H HIZA ULAŞILDI!");
            ShowAchievementNotification(supersonicTitle, supersonicDesc);
        }

        // Yeni Başarımlar (Realtime Check)
        string legendTitle = Gazze.UI.LocalizationManager.Get("Ach_Legend_Title", "EFSANE");
        if (score >= 10000 && !HasAchievement(legendTitle))
        {
            string legendDesc = Gazze.UI.LocalizationManager.Get("Ach_Legend_Desc", "10,000 SKORA ULAŞILDI! BİR EFSANESİN!");
            ShowAchievementNotification(legendTitle, legendDesc);
        }

        string invisibleTitle = Gazze.UI.LocalizationManager.Get("Ach_Invisible_Title", "GÖRÜNMEZ");
        if (nearMissCount >= 50 && !HasAchievement(invisibleTitle))
        {
            string invisibleDesc = Gazze.UI.LocalizationManager.Get("Ach_Invisible_Desc", "50 YAKIN GEÇİŞ! SENİ GÖREMİYORLAR BİLE!");
            ShowAchievementNotification(invisibleTitle, invisibleDesc);
        }

        string lightSpeedTitle = Gazze.UI.LocalizationManager.Get("Ach_LightSpeed_Title", "IŞIK HIZI");
        if (currentWorldSpeed >= 90f && !HasAchievement(lightSpeedTitle))
        {
            string lightSpeedDesc = Gazze.UI.LocalizationManager.Get("Ach_LightSpeed_Desc", "90 KM/H! FİZİK KURALLARINI ZORLUYORSUN!");
            ShowAchievementNotification(lightSpeedTitle, lightSpeedDesc);
        }
    }

    private void HandleShortcuts()
    {
        if (Keyboard.current == null) return;
        
        // ESC artık duraklatma menüsünü açar/kapatır
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (!isGameOver)
            {
                Gazze.UI.PauseMenuBuilder.Toggle();
                return;
            }
        }
        
        // Pause durumundayken diğer kısayolları devre dışı bırak
        if (Gazze.UI.PauseMenuBuilder.IsPaused) return;
        
        if (Keyboard.current.rKey.wasPressedThisFrame) RestartGame();
        if (Keyboard.current.oKey.wasPressedThisFrame) GoToSettings();
        if (Keyboard.current.pKey.wasPressedThisFrame) ShareScore();
    }

    public void ResetNearMiss(int carInstanceId)
    {
        if (passedCars.Contains(carInstanceId))
        {
            passedCars.Remove(carInstanceId);
        }
    }

    private void OnDrawGizmos()
    {
        // 1. Yakın geçiş (Near Miss) alanını görselleştir
        Gizmos.color = new Color(0f, 1f, 0f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, nearMissRadius);
#if UNITY_EDITOR
        UnityEditor.Handles.Label(transform.position + Vector3.up * nearMissRadius, "Near Miss Area");
#endif
        
        // 2. High-speed sweep (tunneling prevention) alanını görselleştir (Sadece hız yüksekse)
        if (Application.isPlaying && currentWorldSpeed > 30f)
        {
            float moveDist = currentWorldSpeed * Time.deltaTime;
            BoxCollider bc = GetComponent<BoxCollider>();
            if (bc != null)
            {
                Gizmos.color = new Color(1f, 0.92f, 0.016f, 0.5f); // Altın sarısı
                Vector3 center = transform.TransformPoint(bc.center);
                Vector3 size = Vector3.Scale(bc.size, transform.lossyScale);
                
                size.z += moveDist;
                center += transform.forward * (moveDist * 0.5f);
                
                Matrix4x4 rotationMatrix = Matrix4x4.TRS(center, transform.rotation, size);
                Gizmos.matrix = rotationMatrix;
                Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
                Gizmos.matrix = Matrix4x4.identity;
#if UNITY_EDITOR
                UnityEditor.Handles.Label(center + Vector3.up * 2f, "High Speed Sweep Area");
#endif
            }
        }
        
        // 3. Araba hitbox'ı (Local space bounds)
        BoxCollider pbc = GetComponent<BoxCollider>();
        if (pbc != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(pbc.center, pbc.size);
            Gizmos.matrix = Matrix4x4.identity;
#if UNITY_EDITOR
            UnityEditor.Handles.Label(transform.position + Vector3.up * 1f, "Car Hitbox");
#endif
        }
    }
    private string GetTranslationSafe(string key, string fallback = "")
    {
        if (LocalizationManager.Instance != null)
        {
            return LocalizationManager.Instance.GetTranslation(key);
        }
        return string.IsNullOrEmpty(fallback) ? key : fallback;
    }
}
