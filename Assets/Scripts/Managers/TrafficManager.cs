/**
 * @file TrafficManager.cs
 * @author Unity MCP Assistant
 * @date 2026-02-28
 * @last_update 2026-02-28
 * @description Trafikteki engellerin, araçların ve toplanabilir objelerin (yardım kolileri) havuzlama (pooling) sistemiyle yönetimini sağlar.
 */

using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Trafikteki engellerin (araçların/enkazların) ve yardım kolilerinin oluşmasını ve hareketini yönetir.
/// </summary>
public class TrafficManager : MonoBehaviour
{
    [Header("Prefab Ayarları")]
    /// <summary> Engel olarak kullanılacak araç veya enkaz prefabları. </summary>
    [Tooltip("Engel olarak kullanılacak araç veya enkaz prefabları.")]
    public GameObject[] trafficCarPrefabs;
    
    /// <summary> Toplanacak yardım kolisi (coin) prefabı. </summary>
    [Tooltip("Toplanacak yardım kolisi prefabı.")]
    public GameObject coinPrefab;
    
    [Header("Oluşturma (Spawning) Ayarları")]
    /// <summary> Engel havuzunda (pool) tutulacak maksimum nesne sayısı. </summary>
    [Tooltip("Pool'da tutulacak maksimum engel sayısı.")]
    public int poolSize = 30; // 15'ten 30'a çıkarıldı
    
    /// <summary> Yardım kolisi havuzunda tutulacak maksimum nesne sayısı. </summary>
    [Tooltip("Pool'da tutulacak maksimum koli sayısı.")]
    public int coinPoolSize = 40; // 20'den 40'a çıkarıldı
    
    /// <summary> Nesnelerin oyuncunun önünde oluşacağı uzaklık (Z ekseni). </summary>
    [Tooltip("Nesnelerin sahnede oluşacağı uzak mesafe (Z ekseni).")]
    public float spawnZ = 300f; // 250'den 300'e çıkarıldı
    
    /// <summary> Nesnelerin oyuncunun arkasında yok edileceği uzaklık (Z ekseni). </summary>
    [Tooltip("Nesnelerin sahneden yok edileceği yakın mesafe (Z ekseni).")]
    public float despawnZ = -50f;
    
    /// <summary> Şeritlerdeki araçlar arasında bırakılacak minimum güvenli mesafe. </summary>
    [Tooltip("Art arda oluşan araçlar arasındaki minimum güvenli mesafe.")]
    public float minDistanceBetweenCars = 15f; // 25'ten 15'e düşürüldü

    /// <summary> Trafik araçlarının doğru yöne bakması için rotasyon. </summary>
    [Tooltip("Trafik araçlarının doğru yöne bakması için rotasyon (FBX modellerine göre ayarlanmalıdır).")]
    public Vector3 trafficCarRotation = new(-90f, 90f, 0f);

    /// <summary> Aracın hareket edebileceği şeritlerin X koordinatları (Sol ve Sağ). </summary>
    private float[] lanes = { -1.5f, 1.5f };
    
    /// <summary> Engel (araç/enkaz) nesne havuzu. </summary>
    private List<GameObject> pool = new List<GameObject>();
    
    /// <summary> Yardım kolisi nesne havuzu. </summary>
    private List<GameObject> coinPool = new List<GameObject>();

    private MaterialPropertyBlock coinPropBlock;
    private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
    private static readonly int ColorId = Shader.PropertyToID("_Color");

    private void Awake()
    {
        coinPropBlock = new MaterialPropertyBlock();
    }

    private void Start()
    {
        // 1. Araç Prefablarını Yükle (Eğer atanmamışsa Resources'tan al)
        if (trafficCarPrefabs == null || trafficCarPrefabs.Length == 0)
        {
            trafficCarPrefabs = Resources.LoadAll<GameObject>("Cars");
            if (trafficCarPrefabs == null || trafficCarPrefabs.Length == 0)
            {
                Debug.LogWarning("TrafficManager: Hiç araç prefabı bulunamadı! 'Resources/Cars' klasörünü kontrol edin.");
            }
        }

        // 2. Engel Havuzunu (Car Pool) Oluştur: Performans için nesneleri önceden belleğe yükler.
        if (trafficCarPrefabs != null && trafficCarPrefabs.Length > 0)
        {
            for (int i = 0; i < poolSize; i++)
            {
                GameObject prefab = trafficCarPrefabs[Random.Range(0, trafficCarPrefabs.Length)];
                GameObject car = Instantiate(prefab);
                car.SetActive(false);
                car.tag = "TrafficCar"; // Tag'in doğru olduğundan emin ol
                
                // BoxCollider ve Rigidbody kontrolü
                 BoxCollider bc = car.GetComponent<BoxCollider>();
                 if (bc == null) bc = car.AddComponent<BoxCollider>();
                 bc.isTrigger = true; // Player OnTrigger kullanıyor

                 if (car.GetComponent<Rigidbody>() == null)
                 {
                     Rigidbody rb = car.AddComponent<Rigidbody>();
                     rb.useGravity = false;
                     rb.isKinematic = true; 
                 }

                  // Collider'ı modele tam oturacak şekilde (game-pose) ayarla
                  car.transform.rotation = Quaternion.Euler(trafficCarRotation);
                  AdjustColliderToFitModel(car, bc, 0.7f);

                // Rigidbody ve Collider ayarları
                pool.Add(car);
            }
        }

        // 2. Yardım Kolisi Havuzunu (Aid Box Pool) Oluştur
        if (coinPrefab != null)
        {
            for (int i = 0; i < coinPoolSize; i++)
            {
                GameObject coin = Instantiate(coinPrefab);
                SetupAsSimpleColoredCube(coin); // Koli görünümü için temel renk ve şekil ataması.
                coin.SetActive(false);
                coinPool.Add(coin);
            }
        }

        // Nesne üretim döngülerini başlat - Sıklık 2 kat artırıldı
        InvokeRepeating(nameof(SpawnCar), 1f, 0.6f);
        InvokeRepeating(nameof(SpawnCoin), 1.5f, 0.9f);
    }

    private void Update()
    {
        // Oyun durmuşsa (Pause/GameOver) nesne hareketlerini durdur.
        if (Time.timeScale == 0) return;
        
        MoveObjects();
    }

    /// <summary>
    /// Uygun bir şeritte rastgele bir engel (araç/enkaz) aktif eder.
    /// </summary>
    private void SpawnCar()
    {
        if (Time.timeScale == 0) return; // Oyun durmuşsa spawn yapma

        float randomLane = lanes[Random.Range(0, lanes.Length)];
        if (IsLaneBlocked(randomLane)) return;

        // Rastgele bir car prefabı seç (Pool genişlemesi gerekirse diye)
        GameObject selectedPrefab = trafficCarPrefabs[Random.Range(0, trafficCarPrefabs.Length)];
        GameObject car = GetInactive(pool, selectedPrefab);
        
        if (car != null)
        {
            Vector3 targetPos = new(randomLane, 1.0f, spawnZ);
            Quaternion targetRot = Quaternion.Euler(trafficCarRotation.x, trafficCarRotation.y - 90f, trafficCarRotation.z);
            
            car.transform.position = new(randomLane, 0.25f, spawnZ);
            car.transform.rotation = Quaternion.Euler(trafficCarRotation);
            car.SetActive(true);

            StartCoroutine(AnimateEntrance(car, targetPos, targetRot));
        }
    }

    /// <summary>
    /// Nesneyi belirlenen pozisyon ve rotasyona yumuşak bir şekilde taşır ve döndürür.
    /// </summary>
    private System.Collections.IEnumerator AnimateEntrance(GameObject obj, Vector3 targetPos, Quaternion targetRot)
    {
        float duration = 0.8f; // Animasyon süresi
        float elapsed = 0f;
        Vector3 startPos = obj.transform.position;
        Quaternion startRot = obj.transform.rotation;

        while (elapsed < duration)
        {
            if (obj == null || !obj.activeInHierarchy) yield break;

            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            
            // SmoothStep (Ease In/Out) eğrisi kullanarak daha doğal bir geçiş sağlarız
            t = t * t * (3f - 2f * t);

            // Pozisyon ve rotasyonu eş zamanlı olarak güncelle
            obj.transform.position = Vector3.Lerp(startPos, targetPos, t);
            obj.transform.rotation = Quaternion.Slerp(startRot, targetRot, t);

            yield return null;
        }

        // Son değerleri netleştir
        if (obj != null)
        {
            obj.transform.position = targetPos;
            obj.transform.rotation = targetRot;
        }
    }

    /// <summary>
    /// Uygun bir şeritte yardım kolisi aktif eder.
    /// </summary>
    private void SpawnCoin()
    {
        if (Time.timeScale == 0) return; // Oyun durmuşsa spawn yapma

        float randomLane = lanes[Random.Range(0, lanes.Length)];
        if (IsLaneBlocked(randomLane)) return;

        GameObject coin = GetInactive(coinPool, coinPrefab);
        if (coin != null)
        {
            coin.transform.position = new(randomLane, 0.8f, spawnZ);
            coin.SetActive(true);
        }
    }

    /// <summary>
    /// Nesne havuzundaki pasif (kullanılmayan) bir objeyi döner. Eğer havuz boşsa yeni bir tane oluşturur.
    /// </summary>
    /// <param name="objectList">Aranacak havuz listesi.</param>
    /// <param name="prefab">Havuz boşsa instantiate edilecek prefab.</param>
    /// <returns>Kullanıma hazır pasif obje.</returns>
    private GameObject GetInactive(List<GameObject> objectList, GameObject prefab)
    {
        for (int i = objectList.Count - 1; i >= 0; i--)
        {
            if (objectList[i] == null)
            {
                objectList.RemoveAt(i);
                continue;
            }
            if (!objectList[i].activeInHierarchy) return objectList[i];
        }

        // Eğer buraya ulaşıldıysa havuzda boş yer kalmamıştır, yeni bir tane oluştur (Dinamik Genişleme)
        if (prefab != null)
        {
            Debug.LogWarning($"TrafficManager: Havuz yetersiz! ({prefab.name}) için yeni nesne oluşturuluyor. Başlangıç havuz boyutunu (poolSize) artırmayı düşünün.");
            GameObject newObj = Instantiate(prefab);
            newObj.SetActive(false);
            
            // Prefab tipine göre uygun setup'ı yap
            if (prefab == coinPrefab)
            {
                SetupAsSimpleColoredCube(newObj);
            }
            else
            {
                newObj.tag = "TrafficCar";
                
                BoxCollider bc = newObj.GetComponent<BoxCollider>();
                if (bc == null) bc = newObj.AddComponent<BoxCollider>();
                bc.isTrigger = true;

                if (newObj.GetComponent<Rigidbody>() == null)
                {
                    Rigidbody rb = newObj.AddComponent<Rigidbody>();
                    rb.useGravity = false;
                    rb.isKinematic = true;
                }

                // Collider'ı modele tam oturacak şekilde (game-pose) ayarla
                newObj.transform.rotation = Quaternion.Euler(trafficCarRotation);
                AdjustColliderToFitModel(newObj, bc, 0.7f);
            }
            
            objectList.Add(newObj);
            return newObj;
        }

        return null;
    }

    /// <summary>
    /// Modelin mesh sınırlarını hesaplayıp collider'ı ona göre ayarlar.
    /// </summary>
    private void AdjustColliderToFitModel(GameObject root, BoxCollider bc, float multiplier)
    {
        // ÖNEMLİ: Rotation reset kaldırıldı çünkü modeller game-pose (flat) durumundayken ölçülmeli.
        Bounds b = new Bounds();
        bool first = true;
        
        // Tüm alt objelerin renderer'larını topla (World Space Bounds)
        foreach (Renderer r in root.GetComponentsInChildren<Renderer>())
        {
            if (first) { b = r.bounds; first = false; }
            else b.Encapsulate(r.bounds);
        }

        if (!first)
        {
            // World space bounds'u root'un local space'ine çevir
            bc.center = root.transform.InverseTransformPoint(b.center);
            bc.size = root.transform.InverseTransformVector(b.size) * multiplier;
            
            // Pozisyon düzenlemeleri: Yükseklik tabana, merkez biraz öne (Forward bias)
            Vector3 center = bc.center;
            center.y = bc.size.y * 0.5f; 
            center.z += bc.size.z * 0.15f; // Öne doğru %15 kaydır (Kullanıcı isteği: "biraz daha önde olsun")
            bc.center = center;
        }
    }

    /// <summary>
    /// Tüm aktif trafik nesnelerini oyuncunun hızına göre geriye taşır ve menzil dışındakileri deaktif eder.
    /// </summary>
    private void MoveObjects()
    {
        float speed = PlayerController.Instance != null ? PlayerController.Instance.currentWorldSpeed : 20f;
        
        // Trafik araçlarını hareket ettir ve yeni çarpışma sistemine kaydet
        for (int i = pool.Count - 1; i >= 0; i--)
        {
            GameObject car = pool[i];
            if (car == null)
            {
                pool.RemoveAt(i);
                continue;
            }

            if (car.activeInHierarchy)
            {
                car.transform.Translate(Vector3.back * speed * Time.deltaTime, Space.World);
                
                // Çarpışma sistemine kayıt (ID: 1+ = Traffic)
                if (Gazze.Collision.HighPerformanceCollisionManager.Instance != null)
                {
                    BoxCollider bc = car.GetComponent<BoxCollider>();
                    Gazze.Collision.HighPerformanceCollisionManager.Instance.RegisterEntity(new Gazze.Collision.HighPerformanceCollisionManager.EntityData
                    {
                        id = i + 1,
                        position = bc.bounds.center,
                        extents = bc.bounds.extents,
                        rotation = car.transform.rotation,
                        type = Gazze.Collision.HighPerformanceCollisionManager.CollisionType.AABB,
                        layer = 1 // Traffic Layer
                    });
                }

                if (car.transform.position.z < despawnZ) car.SetActive(false);
            }
        }

        // Kolileri hareket ettir
        for (int i = coinPool.Count - 1; i >= 0; i--)
        {
            GameObject coin = coinPool[i];
            if (coin == null)
            {
                coinPool.RemoveAt(i);
                continue;
            }

            if (coin.activeInHierarchy)
            {
                coin.transform.Translate(Vector3.back * speed * Time.deltaTime, Space.World);
                if (coin.transform.position.z < despawnZ) coin.SetActive(false);
            }
        }
    }

    /// <summary>
    /// Belirli bir şeritte, yeni bir obje oluşturmak için yeterli boşluk olup olmadığını kontrol eder.
    /// </summary>
    /// <param name="laneX">Kontrol edilecek şeridin X koordinatı.</param>
    /// <returns>Şerit doluysa true, boşsa false.</returns>
    private bool IsLaneBlocked(float laneX)
    {
        for (int i = pool.Count - 1; i >= 0; i--)
        {
            GameObject car = pool[i];
            if (car == null)
            {
                pool.RemoveAt(i);
                continue;
            }

            if (car.activeInHierarchy && Mathf.Abs(car.transform.position.x - laneX) < 0.5f)
            {
                // Eğer şeritteki en son araç hala spawnZ'ye çok yakınsa yeni araç oluşturma
                if (car.transform.position.z > (spawnZ - minDistanceBetweenCars)) return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Basit yardım kolisi görünümü (Haki renkli küp) oluşturur.
    /// </summary>
    private void SetupAsSimpleColoredCube(GameObject coin)
    {
        MeshRenderer mr = coin.GetComponent<MeshRenderer>();
        if (mr != null)
        {
            Color coinColor = new(0.4f, 0.4f, 0.2f); // Haki/Askeri yeşil tonu.
            mr.GetPropertyBlock(coinPropBlock);
            
            if (mr.sharedMaterial.HasProperty("_BaseColor"))
                coinPropBlock.SetColor(BaseColorId, coinColor);
            else
                coinPropBlock.SetColor(ColorId, coinColor);
                
            mr.SetPropertyBlock(coinPropBlock);
        }
    }
}