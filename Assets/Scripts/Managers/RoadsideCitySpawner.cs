using UnityEngine;

/// <summary>
/// Yol kenarı bina spawner — pivot-bağımsız, kesin güvenli yerleştirme.
///
/// SORUNUN KÖKü: Polygon City prefab'larının pivot noktası modelin
/// geometrik merkezinde DEĞİL. Bu yüzden bounds.max.x / bounds.min.x
/// ile "yola bakan yüz" hesabı her prefab için farklı hata veriyor.
///
/// ÇÖZÜM: Dinamik bounds hesabını tamamen terk et.
/// Bunun yerine prefab'ı origin'de oluştur, bounds.center'ı al,
/// ve binanın MERKEZ NOKTASINI yoldan sabit bir mesafeye koy.
/// Güvenli mesafe = roadHalfWidth + safeGap + maxHalfSize
/// maxHalfSize = scaleMax * prefab'ın maksimum boyutunun yarısı
/// Bu değer her zaman yeterince büyük olduğu için pivot offset önemsizleşir.
/// </summary>
public class RoadsideCitySpawner : MonoBehaviour
{
    [Header("Yol Ayarları")]
    public float roadHalfWidth = 5.5f;   // Yol merkezi (X=0) → kenar mesafesi

    [Header("Bina Ayarları")]
    public GameObject[] buildingPrefabs;
    [Range(1, 20)] public int buildingsPerTile = 8;
    public float scaleMin = 3.5f;
    public float scaleMax = 6.0f;

    [Header("Tile Bilgisi")]
    public float tileLength = 50f;

    [Header("Görünüm")]
    public bool useRuinedLook = true;

    // ── Public API ──────────────────────────────────────────────────────────────
    public void Rebuild()
    {
        ClearOldBuildings();

        if (buildingPrefabs == null || buildingPrefabs.Length == 0)
        {
            Debug.LogWarning("[RoadsideCitySpawner] buildingPrefabs atanmamış!", this);
            return;
        }

        // 1. Zemin / Kaldırım tabanını oluştur (boşluklardaki siyahlığı önlemek için)
        CreateGroundFoundation();

        // 2. Binaları yerleştir
        var leftZones  = new System.Collections.Generic.List<Vector2>();
        var rightZones = new System.Collections.Generic.List<Vector2>();

        for (int i = 0; i < buildingsPerTile; i++)
            SpawnBuilding(i, leftZones, rightZones);
    }

    private void CreateGroundFoundation()
    {
        // Her iki taraf için devasa bir zemin plane'i oluştur
        for (int s = 0; s < 2; s++)
        {
            float side = s == 0 ? -1f : 1f;
            GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ground.name = "CSB_Ground_" + (side < 0 ? "L" : "R");
            
            // X: Yoldan 100m dışarı uzansın, Z: Tile boyunca, Y: İnce bir tabaka
            float groundWidth = 120f;
            float centerX     = side * (roadHalfWidth + groundWidth * 0.5f);
            
            ground.transform.SetParent(transform, false);
            ground.transform.localPosition = new Vector3(centerX, -3.6f, 0f);
            ground.transform.localScale    = new Vector3(groundWidth, 0.2f, tileLength);
            
            RemoveColliders(ground);
            
            // Zemin rengini koyulaştır (asfalt/beton görünümü)
            Renderer r = ground.GetComponent<Renderer>();
            
            // Editör sızıntısını önlemek için material instance kullanımını kısıtla.
            // Sadece bir kez (eğer farklıysa) kopya oluştur.
            if (r.sharedMaterial.color != new Color(0.08f, 0.08f, 0.1f))
            {
                Material m = new Material(r.sharedMaterial);
                m.color = new Color(0.08f, 0.08f, 0.1f);
                r.sharedMaterial = m;
            }

            ApplyCurvedShader(ground);
        }
    }

    // ── Private ─────────────────────────────────────────────────────────────────
    private void ClearOldBuildings()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Transform c = transform.GetChild(i);
            if (!c.name.StartsWith("CSB_")) continue;
            if (Application.isPlaying) Destroy(c.gameObject);
            else                       DestroyImmediate(c.gameObject);
        }
    }

    // Yolda görünmemesi gereken prefab isim parçaları (küçük harf karşılaştırması)
    private static readonly string[] BLOCKED_NAME_PARTS = new string[]
    {
        "hospital", "motel", "prop", "vehicle", "car", "truck", "bus",
        "character", "person", "human", "tree", "lamp", "light", "pole",
        "fence", "wall", "road", "sign", "billboard", "bench", "hydrant"
    };

    private bool IsBuildingPrefab(GameObject prefab)
    {
        string nameLower = prefab.name.ToLower();
        foreach (string blocked in BLOCKED_NAME_PARTS)
            if (nameLower.Contains(blocked)) return false;
        return true;
    }

    private void SpawnBuilding(int index, System.Collections.Generic.List<Vector2> leftZones, System.Collections.Generic.List<Vector2> rightZones)
    {
        float side = Random.value < 0.5f ? -1f : 1f;
        var targetZones = (side < 0) ? leftZones : rightZones;

        // "Greedy" (Açgözlü) Spawn: Eğer bir bina sığmazsa, başka bir prefab dene (max 5 farklı prefab denemesi)
        for (int pAttempt = 0; pAttempt < 5; pAttempt++)
        {
            GameObject prefab = null;
            for (int selectionAttempt = 0; selectionAttempt < 10; selectionAttempt++)
            {
                GameObject candidate = buildingPrefabs[Random.Range(0, buildingPrefabs.Length)];
                if (candidate != null && IsBuildingPrefab(candidate)) { prefab = candidate; break; }
            }
            if (prefab == null) continue;

            float scale = Random.Range(scaleMin, scaleMax);
            float yRot  = side < 0f ? 90f : -90f;

            GameObject house = Instantiate(prefab);
            house.name = "CSB_" + index + "_P" + pAttempt;
            house.transform.position = Vector3.zero;
            house.transform.rotation = Quaternion.Euler(0f, yRot, 0f);
            house.transform.localScale = Vector3.one * scale;

            // 'Deprem' efekti
            house.transform.rotation *= Quaternion.Euler(Random.Range(-5f, 5f), 0f, Random.Range(-3f, 3f));
            RemoveColliders(house);

            Bounds b = GetWorldBounds(house);
            
            // Pivot-bağımsız Z kapsamı (house Vector3.zero'da olduğu için b.min/max pivot-relative'dir)
            float zMinRelative = b.min.z;
            float zMaxRelative = b.max.z;

            // Çitayı dar Tut: Eğer bina sığmıyorsa başka prefaba geç
            float finalZ = 0f;
            bool foundSpot = false;
            float buffer = 0.15f; // Binalar arası 15cm güvenlik boşluğu (Daha sık yerleşim)

            for (int zAttempt = 0; zAttempt < 20; zAttempt++)
            {
                // Rastgele bir merkez (pivot) noktası seç (Tile sınırlarına daha yakın: 0.49)
                float localZ = Random.Range(-tileLength * 0.49f, tileLength * 0.49f); 
                
                // Bu pivot noktasında binanın kaplayacağı gerçek dünya Z aralığı
                float occupiedMin = localZ + zMinRelative - buffer;
                float occupiedMax = localZ + zMaxRelative + buffer;

                // 1. Kendi tile'ı içinde mi?
                if (occupiedMin < -tileLength * 0.51f || occupiedMax > tileLength * 0.51f) continue;

                // 2. Diğer binalarla çakışıyor mu?
                bool overlap = false;
                foreach (var zone in targetZones) 
                { 
                    if (occupiedMin < zone.y && occupiedMax > zone.x) { overlap = true; break; } 
                }

                if (!overlap) 
                { 
                    finalZ = localZ; 
                    targetZones.Add(new Vector2(occupiedMin, occupiedMax)); 
                    foundSpot = true; 
                    break; 
                }
            }

            if (foundSpot)
            {
                // Yerleşim tamam - Derinlik randomizasyonu ekleyerek boşluk hissini azaltıyoruz
                float minGap = 1.0f;
                float maxGap = 5.5f;
                float gap = Random.Range(minGap, maxGap); 
                
                float targetCenterX = side * (roadHalfWidth + gap + b.extents.x);
                float pivotOffsetX = targetCenterX - b.center.x;
                float worldZ = transform.position.z + finalZ;
                float worldY = transform.position.y - 3.5f + Random.Range(-1.5f, 1.5f); 

                house.transform.position = new Vector3(pivotOffsetX, worldY, worldZ);
                house.transform.SetParent(transform, worldPositionStays: true);
                ApplyCurvedShader(house);
                return; // Başarıyla yerleşti, pAttempt loop'undan çık
            }
            else
            {
                // Sığmadı, bu prefab denemesini yok et ve bir sonrakini dene
                if (Application.isPlaying) Destroy(house); else DestroyImmediate(house);
            }
        }
    }

    // ── Yardımcılar ─────────────────────────────────────────────────────────────
    private Bounds GetWorldBounds(GameObject go)
    {
        Renderer[] rens = go.GetComponentsInChildren<Renderer>();
        if (rens.Length == 0) return new Bounds(go.transform.position, Vector3.one * 2f);
        Bounds b = rens[0].bounds;
        for (int i = 1; i < rens.Length; i++) b.Encapsulate(rens[i].bounds);
        return b;
    }

    private void RemoveColliders(GameObject go)
    {
        foreach (Collider col in go.GetComponentsInChildren<Collider>())
        {
            if (Application.isPlaying) Destroy(col);
            else                       DestroyImmediate(col);
        }
    }

    private void ApplyCurvedShader(GameObject go)
    {
        Shader ruined   = Shader.Find("Custom/GazaRuinedShader_URP");
        Shader curved   = Shader.Find("Custom/CurvedWorld_URP");
        Shader fallback = Shader.Find("Universal Render Pipeline/Lit");
        
        Shader target   = (useRuinedLook && ruined != null) ? ruined : (curved ?? fallback);
        if (target == null) return;

        MaterialPropertyBlock spawnerPropBlock = new MaterialPropertyBlock();

        foreach (Renderer r in go.GetComponentsInChildren<Renderer>())
        {
            // Gölge Ayarlarını Zorla
            r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
            r.receiveShadows    = true;

            Material[] mats = r.sharedMaterials;
            bool changed = false;
            for (int i = 0; i < mats.Length; i++)
            {
                if (mats[i] == null) continue;
                if (!mats[i].shader.name.Contains("CurvedWorld") && !mats[i].shader.name.Contains("GazaRuined"))
                {
                    // Orijinal prefab materyalini bozmamak için yeni bir kopya oluşturuyoruz.
                    mats[i] = new Material(mats[i]);
                    mats[i].shader = target;
                    changed = true;
                }
            }
            if (changed) r.sharedMaterials = mats;

            // Parametreleri sızıntısız PropertyBlock ile set ediyoruz
            r.GetPropertyBlock(spawnerPropBlock);
            spawnerPropBlock.SetFloat("_Curvature",     0.002f);
            spawnerPropBlock.SetFloat("_CurvatureH",   -0.0015f);
            spawnerPropBlock.SetFloat("_HorizonOffset", 10.0f);

            if (target.name.Contains("GazaRuined"))
            {
                spawnerPropBlock.SetFloat("_DirtIntensity", 0.7f);
                spawnerPropBlock.SetFloat("_GrimeIntensity", 0.5f);
            }
            r.SetPropertyBlock(spawnerPropBlock);
        }
    }
}