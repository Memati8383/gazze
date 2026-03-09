/**
 * @file PlayerController.cs
 * @author Unity MCP Assistant
 * @date 2026-02-28
 * @last_update 2026-02-28
 * @description Oyuncu aracının hareketini, hız yönetimini, çarpışma kontrollerini ve puanlama sistemini yöneten ana sınıftır.
 */

using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Collections.Generic;

/// <summary>
/// Oyuncu aracının hareketini, hızını, puanlamasını ve UI etkileşimlerini yöneten ana kontrolcü.
/// </summary>
public class PlayerController : MonoBehaviour
{
    [Header("Yatay Hareket Ayarları (Şerit Değiştirme)")]
    /// <summary> Aracın sağa ve sola şerit değiştirme hızı. </summary>
    [Tooltip("Aracın sağa ve sola şerit değiştirme hızı.")]
    public float horizontalMoveSpeed = 15f;
    /// <summary> Aracın merkezden maksimum ne kadar uzağa gidebileceği. </summary>
    [Tooltip("Aracın merkezden maksimum ne kadar uzağa gidebileceği.")]
    public float limitX = 1.5f;
    /// <summary> Şerit değiştirirken aracın yana yatma miktarı. </summary>
    [Tooltip("Şerit değiştirirken aracın yana yatma miktarı.")]
    public float tiltAmount = 10f;
    /// <summary> Rotasyonun yumuşama hızı. </summary>
    [Tooltip("Rotasyonun yumuşama hızı.")]
    public float tiltSpeed = 8f;
    /// <summary> Yatay yöndeki hareketin yumuşatılması. </summary>
    [Tooltip("Sağa veya sola giderken ivmelenmenin yumuşama hızı (Düşük = Daha yumuşak).")]
    public float horizontalSmoothing = 10f;
    /// <summary> Hızlanma veya yavaşlama anında aracın öne/arkaya eğilme miktarı. </summary>
    [Tooltip("Hızlanma veya yavaşlama anında aracın öne/arkaya eğilme miktarı.")]
    public float pitchAmount = 5f;

    [Header("Hız Ayarları (Dünya Hızı)")]
    /// <summary> Aracın (ve dolayısıyla yolun) şu anki hızı. </summary>
    [Tooltip("Aracın (ve dolayısıyla yolun) şu anki hızı.")]
    public float currentWorldSpeed = 40f;
    /// <summary> Aracın inebileceği minimum hız. </summary>
    [Tooltip("Aracın inebileceği minimum hız.")]
    public float minSpeed = 20f;
    /// <summary> Aracın çıkabileceği maksimum hız. </summary>
    [Tooltip("Aracın çıkabileceği maksimum hız.")]
    public float maxSpeed = 100f;
    /// <summary> Gaza basıldığında hızın artış hızı. </summary>
    [Tooltip("Gaza basıldığında hızın artış hızı.")]
    public float acceleration = 20f;
    /// <summary> Frene basıldığında hızın azalma hızı. </summary>
    [Tooltip("Frene basıldığında hızın azalma hızı.")]
    public float deceleration = 25f;
    /// <summary> Gaz veya fren basılmadığında aracın sabitlendiği hız. </summary>
    [Tooltip("Gaz veya fren basılmadığında aracın sabitlendiği hız.")]
    public float cruiseSpeed = 40f;

    [Header("UI Elemanları")]
    /// <summary> Anlık puanın (mesafenin) gösterildiği metin. </summary>
    [Tooltip("Anlık puanın (mesafenin) gösterildiği metin.")]
    public TextMeshProUGUI scoreText;
    /// <summary> Anlık hızın gösterildiği metin. </summary>
    [Tooltip("Anlık hızın gösterildiği metin.")]
    public TextMeshProUGUI speedText;
    /// <summary> Toplanan yardım kolisi miktarının gösterildiği metin. </summary>
    [Tooltip("Toplanan yardım kolisi miktarının gösterildiği metin.")]
    public TextMeshProUGUI coinText;
    public TextMeshProUGUI lockedVehicleText;
    /// <summary> Dayanıklılığı gösteren slider. </summary>
    [Tooltip("Dayanıklılığı gösteren progress bar slider.")]
    public Slider durabilitySlider;
    /// <summary> Kalp bazlı can göstergesi. </summary>
    [Tooltip("Kalp bazlı can göstergesi.")]
    public Gazze.UI.HealthHeartDisplay heartDisplay;
    /// <summary> Boost durumunu gösteren metin veya slider (opsiyonel). </summary>
    [Tooltip("Boost durumunu gösteren metin veya slider.")]
    public Slider boostSlider;
    /// <summary> Boost süresini saniye cinsinden gösteren metin. </summary>
    [Tooltip("Boost süresini saniye cinsinden gösteren metin.")]
    public TextMeshProUGUI boostTimeText;
    /// <summary> Boost aktifken ekranda görünecek renkli overlay. </summary>
    [Tooltip("Boost aktifken ekranda görünecek renkli overlay Image.")]
    public Image boostOverlay;
    /// <summary> Çarpışma anında ekranda görünecek flaş efekti overlay. </summary>
    [Tooltip("Çarpışma anında ekranda görünecek flaş efekti overlay Image.")]
    public Image crashOverlay;

    [Header("Dayanıklılık Barı Animasyonları")]
    [Tooltip("Düşük can uyarısı için pulsing eşiği (%0-1).")]
    public float lowHealthThreshold = 0.25f;
    [Tooltip("Hasar alındığında barın sarsılma şiddeti.")]
    public float barShakeIntensity = 2.5f; // Reduced from 5
    [Tooltip("Hasar alındığında barın büyüme miktarı.")]
    public float barPunchAmount = 1.1f; // Reduced from 1.2

    private Vector3 durabilityBarInitialScale = Vector3.one;
    private Vector3 durabilityBarInitialPos;
    private Coroutine barAnimationCoroutine;

    [Header("Boost Ayarları")]
    /// <summary> Boost sırasında eklenen hız miktarı. </summary>
    [Tooltip("Boost sırasında eklenen hız miktarı.")]
    public float boostSpeedBonus = 20f;
    /// <summary> Boost sırasında eklenen ivme miktarı. </summary>
    [Tooltip("Boost sırasında eklenen ivme miktarı.")]
    public float boostAccelerationBonus = 15f;
    /// <summary> Boost etkisinin ne kadar süreceği. </summary>
    [Tooltip("Boost etkisinin ne kadar süreceği.")]
    public float boostDuration = 2f;
    /// <summary> Boost'un tekrar kullanılabilmesi için gereken bekleme süresi. </summary>
    [Tooltip("Boost'un tekrar kullanılabilmesi için gereken bekleme süresi.")]
    public float boostCooldown = 5f;
    
    private bool isBoosting = false;
    private float boostTimer = 0f;
    private float lastBoostTime = -999f;

    [Header("Görsel ve İşitsel Efektler")]
    /// <summary> Boost aktifken oynatılacak ses klibi. </summary>
    [Tooltip("Boost aktifken oynatılacak ses klibi.")]
    public AudioClip boostSound;
    /// <summary> Boost aktifken oluşacak efekt prefabı (ör: egzoz ateşi). </summary>
    [Tooltip("Boost aktifken oluşacak efekt prefabı.")]
    public GameObject boostEffectPrefab;
    /// <summary> Coin (Yardım Paketi) toplandığında oynatılacak ses klibi. </summary>
    [Tooltip("Coin toplandığında oynatılacak ses klibi.")]
    public AudioClip coinSound;

    /// <summary> Çarpışma anında oynatılacak ses klibi. </summary>
    [Tooltip("Çarpışma anında oynatılacak ses klibi.")]
    public AudioClip crashSound;
    /// <summary> Oyun bittiğinde (öldüğümüzde) oynatılacak ses klibi. </summary>
    [Tooltip("Oyun bittiğinde oynatılacak ses klibi.")]
    public AudioClip deathSound;
    /// <summary> Çarpışma anında oluşacak patlama/toz efekti prefabı. </summary>
    [Tooltip("Çarpışma anında oluşacak patlama/toz efekti prefabı.")]
    public GameObject crashEffectPrefab;
    /// <summary> Ekran sarsıntısı şiddeti. </summary>
    [Tooltip("Ekran sarsıntısı şiddeti.")]
    public float shakeIntensity = 0.35f; // Artık kullanılmıyor – SmoothCameraFollow yönetiyor
    /// <summary> Ekran sarsıntısı süresi. </summary>
    [Tooltip("Ekran sarsıntısı süresi.")]
    public float shakeDuration = 0.2f; // Artık kullanılmıyor – SmoothCameraFollow yönetiyor

    private float horizontalDirection = 0f;
    private float smoothHorizontal = 0f;
    private bool isGameOver = false;
    private float score = 0f;
    private int coins = 0;
    private float playTime = 0f;
    private System.DateTime sessionStart;
    
    private bool isGassing = false;
    private bool isBraking = false;
    private bool isMovingLeft = false;
    private bool isMovingRight = false;
    private Quaternion initialRotation;
    private float currentYawOffset = 0f;
    private float currentPitchOffset = 0f;
    private GameObject currentBoostEffect;
    private float visualDurability; // Animasyonlu geçiş için görsel dayanıklılık değeri


    /// <summary> Seçilen aracın oyunda doğru yöne bakması için rotasyon. </summary>
    [Tooltip("Seçilen aracın oyunda doğru yöne bakması için rotasyon (ters dönmeyi engeller)")]
    public Vector3 carSpawnRotation = new(-90, 90, 0);

    [Header("Seçili Araç Özellikleri (Yeni Sistem)")]
    /// <summary> Şu anki aracın özellikleri. </summary>
    public Gazze.Models.VehicleAttributes currentAttributes;
    /// <summary> Aracın şu anki dayanıklılığı (%0-100). </summary>
    public float currentDurability = 100f;
    /// <summary> Aracın maksimum dayanıklılığı. </summary>
    public float maxDurability = 100f;

    [Header("Koruma Süresi (Invulnerability)")]
    /// <summary> Çarpışma sonrası koruma süresi (saniye). </summary>
    public float invulnerabilityDuration = 5f;
    private bool isInvulnerable = false;
    private float invulnerabilityTimer = 0f;

    [Header("Araç Listesi")]
    /// <summary> Kullanılabilecek araba prefabları. </summary>
    [Tooltip("Hierarchy'den veya Project'ten kullanılabilecek prefab listesi.")]
    public GameObject[] availableCarPrefabs;

    /// <summary> PlayerController Singleton örneği. </summary>
    public static PlayerController Instance;

    private void Awake()
    {
        Instance = this;
        // AudioManager'ı hemen başlatarak müziğin ilk andan itibaren çalmasını sağlarız.
        if (Settings.AudioManager.Instance != null) { /* Singleton tetiklendi */ }
    }

    private void Start()
    {
        // Zaman akışını normale döndür ve değişkenleri sıfırla
        Time.timeScale = 1f;
        isGameOver = false;
        score = 0f;
        coins = 0;
        currentWorldSpeed = 0f;
        initialRotation = transform.localRotation;
        sessionStart = System.DateTime.UtcNow;
        visualDurability = currentDurability; // Başlangıçta anlık eşitle


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
            string carName = Gazze.UI.LocalizationManager.Instance != null ? Gazze.UI.LocalizationManager.Instance.GetTranslation("Game_LockedVehicle") : "KİLİTLİ ARAÇ";
            if (Gazze.Models.VehicleRepository.Instance != null && Gazze.Models.VehicleRepository.Instance.vehicles.Count > selectedIndex)
            {
                carName = Gazze.Models.VehicleRepository.Instance.vehicles[selectedIndex].name;
            }
            
            if (lockedVehicleText != null)
            {
                string format = Gazze.UI.LocalizationManager.Instance != null ? Gazze.UI.LocalizationManager.Instance.GetTranslation("Game_PurchaseRequired") : "ARAÇ SATIN ALINMAMIŞ: {0}\nLütfen ana menüden aracı satın alın veya başka bir araç seçin.";
                lockedVehicleText.text = string.Format(format, carName);
                lockedVehicleText.gameObject.SetActive(true);
            }
            
            // Oyunu durdur ve ana menüye dönmek için butonların görünmesini sağla veya otomatik dön
            isGameOver = true;
            Time.timeScale = 0f;
            
            // Final ekranını kilitli araç mesajıyla göster
            string achMsg = Gazze.UI.LocalizationManager.Instance != null ? Gazze.UI.LocalizationManager.Instance.GetTranslation("Game_NotPurchasedAch") : "Araç henüz alınmadı!";
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
        
        // Boost süresi yükseltmesi
        boostDuration = Gazze.Models.VehicleUpgradeManager.GetUpgradedValue(selectedIndex, Gazze.Models.VehicleUpgradeManager.UpgradeType.BoostDuration, 2.0f); // Varsayılan 2.0 sn üzerinden

        maxSpeed = upgradedMaxSpeed / 3.6f; // km/h to m/s
        acceleration = upgradedAccel;
        horizontalMoveSpeed = currentAttributes.steeringSensitivity * 15f;
        maxDurability = upgradedDurability;
        currentDurability = maxDurability;

        if (heartDisplay != null)
        {
            heartDisplay.SetupHearts(maxDurability);
        }

        // Debug.Log($"Özellikler (Yükseltilmiş) uygulandı: Hız={maxSpeed}, İvme={acceleration}, Dayanıklılık={maxDurability}, Boost={boostDuration}");
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
            bc.size = transform.InverseTransformVector(b.size) * multiplier;
            
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

        currentDurability -= amount;
        currentDurability = Mathf.Clamp(currentDurability, 0f, maxDurability);

        if (currentDurability <= 0)
        {
            GameOver();
        }
        
        // Çarpışma efekti
        if (crashOverlay != null) StartCoroutine(FlashCrashOverlay());
        if (crashSound != null && !isGameOver) AudioSource.PlayClipAtPoint(crashSound, transform.position);

        // Dayanıklılık göstergesi sarsılma/büyüme efekti
        if (heartDisplay != null)
        {
            // Kalp sistemi zaten kendi içindeki state değişimiyle punch animasyonunu tetikliyor.
            // visualDurability'yi anlık düşürerek görsel tepkiyi hızlandıralım.
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

        HandleInput();
        HandleSpeed();
        HandleMovement();
        HandleBoost();
        HandleInvulnerability();
        
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
        
        UpdateUI();
        playTime = (float)(System.DateTime.UtcNow - sessionStart).TotalSeconds;
        HandleShortcuts();
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

        // Tüm girişleri topla ve -1 ile 1 arasına sıkıştır
        horizontalDirection = Mathf.Clamp(kbInput + gpInput + buttonInput, -1f, 1f);
        
        // Gaz/Fren durumlarını güncelle
        if (Keyboard.current != null)
        {
            // Eğer klavye basılıysa ilgili flag'i true yap, değilse UI butonunun durumunu koru
            if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) isGassing = true;
            else if (!isGassingFromUI()) isGassing = false; 

            if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) isBraking = true;
            else if (!isBrakingFromUI()) isBraking = false;
        }

        // Mutlak mutual exclusion (Gaz ve fren aynı anda basılamaz)
        if (isGassing && isBraking)
        {
            isBraking = false;
        }

        // Boost girişi
        if (Keyboard.current != null && (Keyboard.current.leftShiftKey.wasPressedThisFrame || Keyboard.current.spaceKey.wasPressedThisFrame))
        {
            ActivateBoost();
        }
    }

    // UI butonlarının basılı olup olmadığını kontrol eden yardımcı metodlar (opsiyonel ama temizlik için)
    private bool isGassingFromUI() { return isGassing && (Keyboard.current == null || (!Keyboard.current.wKey.isPressed && !Keyboard.current.upArrowKey.isPressed)); }
    private bool isBrakingFromUI() { return isBraking && (Keyboard.current == null || (!Keyboard.current.sKey.isPressed && !Keyboard.current.downArrowKey.isPressed)); }

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

        // Boost durumunda hız ve ivme bonuslarını uygula
        if (isBoosting)
        {
            targetSpeed += boostSpeedBonus;
            currentAcceleration += boostAccelerationBonus;
        }

        if (gasInput)
        {
            float maxEffectiveSpeed = maxSpeed + (isBoosting ? boostSpeedBonus : 0f);
            currentWorldSpeed = Mathf.MoveTowards(currentWorldSpeed, maxEffectiveSpeed, currentAcceleration * Time.deltaTime);
            currentPitchOffset = Mathf.MoveTowards(currentPitchOffset, -pitchAmount, tiltSpeed * Time.deltaTime);
        }
        else if (brakeInput)
        {
            targetSpeed = minSpeed;
            currentWorldSpeed = Mathf.MoveTowards(currentWorldSpeed, targetSpeed, deceleration * Time.deltaTime);
            currentPitchOffset = Mathf.MoveTowards(currentPitchOffset, pitchAmount, tiltSpeed * Time.deltaTime);
        }
        else
        {
            currentWorldSpeed = Mathf.MoveTowards(currentWorldSpeed, targetSpeed, (deceleration / 2f) * Time.deltaTime);
            currentPitchOffset = Mathf.MoveTowards(currentPitchOffset, 0, tiltSpeed * Time.deltaTime);
        }
    }

    /// <summary>
    /// Şerit değiştirme hareketini ve görsel yatma (tilt) etkisini uygular.
    /// </summary>
    private void HandleMovement()
    {
        // Girdiyi hedefe doğru yumuşatarak taşı -> Daha smooth bir şerit değiştirme sağlar
        smoothHorizontal = Mathf.Lerp(smoothHorizontal, horizontalDirection, Time.deltaTime * horizontalSmoothing);

        float newX = transform.position.x + (smoothHorizontal * horizontalMoveSpeed * Time.deltaTime);
        newX = Mathf.Clamp(newX, -limitX, limitX);
        transform.position = new(newX, transform.position.y, transform.position.z);

        // Görsel rotasyon (Yana yatma)
        currentYawOffset = Mathf.MoveTowards(currentYawOffset, -smoothHorizontal * tiltAmount, tiltSpeed * Time.deltaTime);
        transform.localRotation = initialRotation * Quaternion.Euler(currentPitchOffset, 0, currentYawOffset);
    }

    /// <summary>
    /// Boost süresini takip eder ve bitince hızı normale döndürür.
    /// </summary>
    private void HandleBoost()
    {
        if (isBoosting)
        {
            boostTimer -= Time.deltaTime;
            if (boostTimer <= 0)
            {
                EndBoost();
            }
        }
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
        string distLabel = Gazze.UI.LocalizationManager.Instance != null ? Gazze.UI.LocalizationManager.Instance.GetTranslation("Game_Distance") : "MESAFE";
        string speedLabel = Gazze.UI.LocalizationManager.Instance != null ? Gazze.UI.LocalizationManager.Instance.GetTranslation("Game_Speed") : "HIZ";
        string helpLabel = Gazze.UI.LocalizationManager.Instance != null ? Gazze.UI.LocalizationManager.Instance.GetTranslation("Game_Help") : "YARDIM";
        string kmhLabel = Gazze.UI.LocalizationManager.Instance != null ? Gazze.UI.LocalizationManager.Instance.GetTranslation("Game_Kmh") : "KM/H";

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
            if (isBoosting)
            {
                boostSlider.value = boostTimer / boostDuration;
                if (boostTimeText != null) 
                {
                    boostTimeText.text = boostTimer.ToString("F1") + "s";
                    boostTimeText.color = Color.cyan;
                }
            }
            else
            {
                float timeSinceLastBoost = Time.time - lastBoostTime;
                float cooldownProgress = timeSinceLastBoost / boostCooldown;
                boostSlider.value = Mathf.Clamp01(cooldownProgress);

                if (boostTimeText != null)
                {
                    if (timeSinceLastBoost < boostCooldown)
                    {
                        float remainingCooldown = boostCooldown - timeSinceLastBoost;
                        boostTimeText.text = remainingCooldown.ToString("F0") + "s";
                        boostTimeText.color = Color.white;
                    }
                    else
                    {
                        boostTimeText.text = "READY";
                        boostTimeText.color = Color.green;
                    }
                }
            }
        }

        // Boost Overlay Efekti (Pulsing)
        if (boostOverlay != null)
        {
            if (isBoosting)
            {
                if (!boostOverlay.gameObject.activeSelf) boostOverlay.gameObject.SetActive(true);
                // Yeni görsel için daha yüksek başlangıç opaklığı
                float alpha = 0.4f + Mathf.PingPong(Time.time * 2f, 0.4f);
                boostOverlay.color = new Color(1, 1, 1, alpha); // Görselin orijinal renklerini korumak için Beyaz + Alpha
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
    /// Diğer objelerle olan çarpışmaları kontrol eder.
    /// </summary>
    private void OnTriggerEnter(Collider other)
    {
        if (isGameOver) return;

        if (other.CompareTag("Obstacle") || other.CompareTag("TrafficCar"))
        {
            HandleCollision(other);
        }
        else if (other.CompareTag("Coin"))
        {
            coins++;
            
            // Ses Efekti
            if (coinSound != null && Settings.AudioManager.Instance != null)
            {
                Settings.AudioManager.Instance.PlaySFX(coinSound);
            }

            Settings.HapticManager.Light();
            other.gameObject.SetActive(false); // Pooling sistemi için Destroy yerine SetActive(false) kullanıyoruz
            // Debug.Log("Yardım paketi toplandı!");
        }
        else if (other.CompareTag("Boost"))
        {
            ActivateBoost();
            other.gameObject.SetActive(false); // Eğer Boost objeleri de pool ediliyorsa SetActive(false)
        }
    }

    /// <summary>
    /// Çarpışma anında görsel ve işitsel efektleri tetikler ve oyun sonu mantığını başlatır.
    /// </summary>
    private void HandleCollision(Collider other)
    {
        if (isInvulnerable) return;

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
        currentWorldSpeed = 0;
        
        // 6. Hasar ver (Yeni Sistem)
        TakeDamage(25f); // Her çarpışmada %25 hasar
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
        if (isGameOver) return;
        
        // Debug.Log("ActivateBoost çağrıldı. Mevcut durum - Boosting: " + isBoosting + ", Cooldown: " + (lastBoostTime + boostCooldown - Time.time).ToString("F1"));

        if (isBoosting)
        {
            // Debug.Log("Boost zaten aktif!");
            return;
        }

        if (Time.time < lastBoostTime + boostCooldown)
        {
            // Debug.Log("Boost beklemede: " + (lastBoostTime + boostCooldown - Time.time).ToString("F1") + "s");
            return;
        }

        isBoosting = true;
        boostTimer = boostDuration;

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
        
        isGameOver = true;

        // Ölüş/Oyun Bitti sesini çal
        if (deathSound != null && Settings.AudioManager.Instance != null)
        {
            Settings.AudioManager.Instance.PlaySFX(deathSound);
        }
        
        Settings.HapticManager.Heavy();
        
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
            achievements = CalculateAchievements(currentScore, coins),
            onRestart = RestartGame,
            onMainMenu = MainMenu,
            onShare = ShareScore,
            onSettings = GoToSettings
        };
        Gazze.UI.GameOverPanelBuilder.Build(data);
        
        // Final UI Güncellemesi (Barın 0 olduğunu göstermek için animasyonu beklemeden eşitle)
        visualDurability = 0f; 
        currentDurability = 0f;

        // Devam eden animasyonları durdur ve barı sıfırla
        if (barAnimationCoroutine != null)
        {
            StopCoroutine(barAnimationCoroutine);
            barAnimationCoroutine = null;
        }
        if (durabilitySlider != null)
        {
            RectTransform rt = durabilitySlider.GetComponent<RectTransform>();
            rt.anchoredPosition = durabilityBarInitialPos;
            rt.localScale = durabilityBarInitialScale;
        }

        UpdateUI();
        
        // Post-process efektlerini durdur, ölüm shake'ini tetikle
        if (Gazze.CameraSystem.SmoothCameraFollow.Instance != null)
        {
            Gazze.CameraSystem.SmoothCameraFollow.Instance.TriggerDeathShake();
        }
        if (Gazze.VisualEffects.BoostPostProcessManager.Instance != null)
        {
            Gazze.VisualEffects.BoostPostProcessManager.Instance.StopBoostEffect();
        }
        
        Time.timeScale = 0f; 
    }

    /// <summary>
    /// Oyunu tamamen sıfırlar ve yeniden başlatır.
    /// </summary>
    public void RestartGame()
    {
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
    /// <summary> Gaza basıldığında çağrılır. </summary>
    public void GasDown() { isGassing = true; }
    /// <summary> Gaz bırakıldığında çağrılır. </summary>
    public void GasUp() { isGassing = false; }
    /// <summary> Frene basıldığında çağrılır. </summary>
    public void BrakeDown() { isBraking = true; }
    /// <summary> Fren bırakıldığında çağrılır. </summary>
    public void BrakeUp() { isBraking = false; }

    /// <summary> Sola gitme butonuna basıldığında çağrılır. </summary>
    public void MoveLeftDown() { isMovingLeft = true; isMovingRight = false; }
    /// <summary> Sağa gitme butonuna basıldığında çağrılır. </summary>
    public void MoveRightDown() { isMovingRight = true; isMovingLeft = false; }
    /// <summary> Yön butonları bırakıldığında çağrılır. </summary>
    public void StopHorizontal() { isMovingLeft = false; isMovingRight = false; }
    /// <summary> Boost butonuna basıldığında çağrılır. </summary>
    public void BoostDown() { ActivateBoost(); }

    /// <summary> Unity Button OnClick() olayından doğrudan çağırmak için. </summary>
    public void OnBoostButtonClick() 
    { 
        // Debug.Log("OnBoostButtonClick: Buton tıklaması algılandı.");
        ActivateBoost(); 
    }

    private int CalculateLevel(int sc, int c)
    {
        int lvl = 1 + sc / 500 + c / 10;
        if (lvl < 1) lvl = 1;
        return lvl;
    }

    private System.Collections.Generic.List<string> CalculateAchievements(int sc, int c)
    {
        var list = new System.Collections.Generic.List<string>();
        if (sc >= 1000) list.Add("Uzun Yol");
        if (c >= 10) list.Add("Yardımsever");
        if (maxSpeed >= 45f) list.Add("Hız Tutkunu");
        if (list.Count == 0) list.Add("İlk Adım");
        return list;
    }

    private void HandleShortcuts()
    {
        if (Keyboard.current == null) return;
        if (Keyboard.current.rKey.wasPressedThisFrame) RestartGame();
        if (Keyboard.current.escapeKey.wasPressedThisFrame) MainMenu();
        if (Keyboard.current.sKey.wasPressedThisFrame) GoToSettings();
        if (Keyboard.current.pKey.wasPressedThisFrame) ShareScore();
    }
}
