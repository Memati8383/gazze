using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Trafikteki engellerin (araçların/enkazların) ve yardım kolilerinin oluşmasını ve hareketini yönetir.
/// </summary>
public class TrafficManager : MonoBehaviour
{
    [Header("Prefab Ayarları")]
    [Tooltip("Trafikte engel olarak (karşı araçlar, bozuk araçlar vb.) kullanılacak prefablar.")]
    public GameObject[] trafficCarPrefabs;
    
    [Header("Oluşturma (Spawning) Ayarları")]
    [Tooltip("Nesne havuzunda (object pool) tutulacak maksimum araç sayısı.")]
    public int poolSize = 30;
    
    [Tooltip("Yeni araçların oyuncunun ne kadar önünde (Z ekseni) belireceği.")]
    public float spawnZ = 300f;
    
    [Tooltip("Yoldan çıkan veya geride kalan araçların ne kadar arkada yok edileceği.")]
    public float despawnZ = -50f;
    
    [Tooltip("Aynı şeritte art arda çıkan iki araç arasındaki minimum güvenli mesafe.")]
    public float minDistanceBetweenCars = 15f;

    [Tooltip("Trafik araçlarının Unity sahnemize uygun şekilde (yola paralel) durması için rotasyon düzeltmesi.")]
    public Vector3 trafficCarRotation = new(-90f, 90f, 0f);

    /// <summary> Yolun sahip olduğu şeritlerin merkez X koordinatları. </summary>
    private float[] lanes = { -2.25f, 2.25f };
    
    /// <summary> Sahne donmalarını engellemek için önceden oluşturulan araç listesi (Pooling). </summary>
    private List<GameObject> pool = new List<GameObject>();
    

    private void Awake()
    {
    }

    private float spawnDistanceCounter = 0f;
    private float nextSpawnDistance = 50f;

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

        // 2. Engel Havuzunu (Car Pool) Oluştur
        if (trafficCarPrefabs != null && trafficCarPrefabs.Length > 0)
        {
            for (int i = 0; i < poolSize; i++)
            {
                GameObject prefab = trafficCarPrefabs[Random.Range(0, trafficCarPrefabs.Length)];
                GameObject car = Instantiate(prefab);
                car.SetActive(false);
                car.tag = "TrafficCar";

                BoxCollider bc = car.GetComponent<BoxCollider>();
                if (bc == null) bc = car.AddComponent<BoxCollider>();
                bc.isTrigger = true;

                if (car.GetComponent<Rigidbody>() == null)
                {
                    Rigidbody rb = car.AddComponent<Rigidbody>();
                    rb.useGravity = false;
                    rb.isKinematic = true;
                }

                car.transform.rotation = Quaternion.Euler(trafficCarRotation);
                AdjustColliderToFitModel(car, bc, 0.7f);
                pool.Add(car);
            }
        }
        
        // Mesafe hesaplaması için başlangıç değeri
        nextSpawnDistance = 50f;
    }

    private void Update()
    {
        if (Time.timeScale == 0) return;

        // Geri sayım bitmeden trafik oluşturma
        if (Gazze.UI.CountdownManager.Instance != null && !Gazze.UI.CountdownManager.Instance.IsGameStarted) return;
        
        float speed = PlayerController.Instance != null ? PlayerController.Instance.currentWorldSpeed : 20f;
        
        // Mesafe bazlı araç spawn logic
        spawnDistanceCounter += speed * Time.deltaTime;
        if (spawnDistanceCounter >= nextSpawnDistance)
        {
            SpawnCar();
            spawnDistanceCounter = 0f;
            
            // Zorluk Dengesi: Hız arttıkça araçlar arasındaki mesafeyi azalt (Yoğunluğu artır)
            // Hız 20 iken: 80 metrede bir araç
            // Hız 100 iken: 35 metrede bir araç
            float t = Mathf.InverseLerp(20f, 100f, speed);
            nextSpawnDistance = Mathf.Lerp(80f, 35f, t);
        }

        MoveTrafficCars();
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
            if (PlayerController.Instance != null)
                PlayerController.Instance.ResetNearMiss(car.GetInstanceID());

            Vector3 targetPosition = new Vector3(randomLane, 0.85f, spawnZ);
            Quaternion targetRotation = Quaternion.Euler(trafficCarRotation.x, trafficCarRotation.y - 90f, trafficCarRotation.z);
            
            // Başlangıç: Araç direkt yerde ama dikey ölçeği 0 (Zeminden yükselme efekti)
            car.transform.position = targetPosition;
            car.transform.rotation = targetRotation;
            car.transform.localScale = new Vector3(1f, 0f, 1f); // X ve Z tam, Y sıfır
            
            ApplyCurvedShaderToCar(car);
            car.SetActive(true);
            
            // 'Digital Extrusion' / 'Zeminden Yükselme' efektini başlat
            StartCoroutine(PerformExtrusionSpawn(car));
        }
    }

    /// <summary>
    /// Trafik araçlarının zeminden 'dikey olarak yükselerek' belirmesini sağlar (Premium UX).
    /// </summary>
    private System.Collections.IEnumerator PerformExtrusionSpawn(GameObject car)
    {
        float duration = 0.35f; // Daha hızlı ve vurucu (snappy)
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            if (car == null || !car.activeInHierarchy) yield break;
            
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            // 'Elastic Out' hissi için (Hızlı yükselip sonda yavaşlama)
            float growth = 1f - Mathf.Pow(1f - t, 4f);
            
            // Sadece Y ölçeğini güncelliyoruz (X ve Z sabit kalsın ki 'genişleme' değil 'yükselme' olsun)
            car.transform.localScale = new Vector3(1f, growth, 1f);

            yield return null;
        }

        if (car != null) car.transform.localScale = Vector3.one;
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

        MaterialPropertyBlock carPropBlock = new MaterialPropertyBlock();

        // Her araç için farklı kir miktarı (Bazıları daha temiz, bazıları çok kirli)
        float randomDirt = Random.Range(0.15f, 0.65f);
        float randomWear = Random.Range(0.2f, 0.8f);

        foreach (Renderer r in car.GetComponentsInChildren<Renderer>())
        {
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

                    mats[i] = new Material(mats[i]);
                    mats[i].shader = vehicleShader;
                    if (mainTex != null) mats[i].SetTexture("_BaseMap", mainTex);
                    mats[i].SetColor("_BaseColor", mainColor);
                    changed = true;
                }
            }
            if (changed) r.sharedMaterials = mats;

            // PropertyBlock ile parametreleri sızdırmadan set et
            r.GetPropertyBlock(carPropBlock);
            carPropBlock.SetFloat("_DirtAmount", randomDirt);
            carPropBlock.SetFloat("_WearStrength", randomWear);
            carPropBlock.SetColor("_DirtColor", new Color(0.25f, 0.15f, 0.1f, 1f));
            carPropBlock.SetFloat("_Curvature", 0.002f);
            carPropBlock.SetFloat("_CurvatureH", -0.0015f);
            carPropBlock.SetFloat("_HorizonOffset", 10.0f);
            r.SetPropertyBlock(carPropBlock);
        }
    }


    /// <summary>
    /// Uygun bir şeritte yardım kolisi aktif eder.
    /// </summary>
// SpawnCoin kaldırıldı – artık Gazze.Collectibles.CoinSpawner yönetiyor.

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
            if (objectList[i] == null) { objectList.RemoveAt(i); continue; }
            if (!objectList[i].activeInHierarchy) return objectList[i];
        }

        // Havuz yetersizse dinamik olarak yeni trafik araci olustur
        if (prefab != null)
        {
            Debug.LogWarning($"TrafficManager: Havuz yetersiz! ({prefab.name}) icin yeni nesne olusturuluyor.");
            GameObject newObj = Instantiate(prefab);
            newObj.SetActive(false);
            newObj.tag = "TrafficCar";

            BoxCollider bc = newObj.GetComponent<BoxCollider>();
            if (bc == null) bc = newObj.AddComponent<BoxCollider>();
            bc.isTrigger = true;

            if (newObj.GetComponent<Rigidbody>() == null)
            {
                Rigidbody rb = newObj.AddComponent<Rigidbody>();
                rb.useGravity  = false;
                rb.isKinematic = true;
            }

            newObj.transform.rotation = Quaternion.Euler(trafficCarRotation);
            AdjustColliderToFitModel(newObj, bc, 0.7f);

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
            Vector3 localSize = root.transform.InverseTransformVector(b.size);
            bc.size = new Vector3(Mathf.Abs(localSize.x), Mathf.Abs(localSize.y), Mathf.Abs(localSize.z)) * multiplier;
            
            // Pozisyon düzenlemeleri: Yükseklik tabana, merkez biraz öne (Forward bias)
            Vector3 center = bc.center;
            center.y = bc.size.y * 0.5f; 
            center.z += bc.size.z * 0.15f; // Öne doğru %15 kaydır (Kullanıcı isteği: "biraz daha önde olsun")
            bc.center = center;
        }
    }

    /// <summary>
    /// Sadece trafik araçlarını hareket ettirir. Coin hareketi artık CoinSpawner'da.
    /// </summary>
    private void MoveTrafficCars()
    {
        float speed = PlayerController.Instance != null ? PlayerController.Instance.currentWorldSpeed : 20f;

        for (int i = pool.Count - 1; i >= 0; i--)
        {
            GameObject car = pool[i];
            if (car == null) { pool.RemoveAt(i); continue; }

            if (car.activeInHierarchy)
            {
                // ── Fling Check ──
                // Eğer araç bir güç (ShockWave/Juggernaut) ile fırlatılmışsa (kinematic değilse),
                // normal trafik akışından çıkar ve fizik motoruna bırak.
                Rigidbody rb = car.GetComponent<Rigidbody>();
                if (rb != null && !rb.isKinematic) 
                {
                    // Deaktivasyon kontrolünü yine de yap (Z ekseninde çok arkaya düştüyse)
                    if (car.transform.position.z < despawnZ - 20f) car.SetActive(false);
                    continue; 
                }

                car.transform.Translate(Vector3.back * speed * Time.deltaTime, Space.World);

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
                        layer = 1
                    });
                }

                if (car.transform.position.z < despawnZ) car.SetActive(false);
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
    private void OnDrawGizmos()
    {
        // 1. Spawn ve Despawn hatlarını çiz (Sadece Seçiliyken)
        Gizmos.color = Color.red;
        Gizmos.DrawLine(new Vector3(-10, 0, spawnZ), new Vector3(10, 0, spawnZ));
        Gizmos.DrawLine(new Vector3(-10, 0, despawnZ), new Vector3(10, 0, despawnZ));
#if UNITY_EDITOR
        UnityEditor.Handles.Label(new Vector3(0, 5, spawnZ), "TRAFFIC SPAWN Z");
        UnityEditor.Handles.Label(new Vector3(0, 5, despawnZ), "TRAFFIC DESPAWN Z");
#endif
        
        // 2. Şeritleri ve Güvenli Mesafe Sınırını Çiz
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.5f); // Turuncu, yarı saydam
        foreach (float lx in lanes)
        {
            // Şerit hattı
            Gizmos.DrawLine(new Vector3(lx, 0, despawnZ), new Vector3(lx, 0, spawnZ));
            
            // Spawn anında engel kontrol alanı (MinDistance)
            Gizmos.DrawWireCube(new Vector3(lx, 0.5f, spawnZ - minDistanceBetweenCars * 0.5f), 
                               new Vector3(1.5f, 1f, minDistanceBetweenCars));
#if UNITY_EDITOR
            UnityEditor.Handles.Label(new Vector3(lx, 2, spawnZ - minDistanceBetweenCars), "LANE: " + lx);
#endif
        }

        // 3. Aktif araçların collider sınırlarını göster (Sadece Oyun Çalışırken)
        if (Application.isPlaying)
        {
            Gizmos.color = Color.yellow;
            foreach (var car in pool)
            {
                if (car != null && car.activeInHierarchy)
                {
                    BoxCollider bc = car.GetComponent<BoxCollider>();
                    if (bc != null)
                    {
                         Gizmos.matrix = car.transform.localToWorldMatrix;
                         Gizmos.DrawWireCube(bc.center, bc.size);
                    }
                }
            }
            Gizmos.matrix = Matrix4x4.identity;
        }
    }
}
