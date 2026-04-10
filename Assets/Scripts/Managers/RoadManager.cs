using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Yol karolarinin ve kenar objelerinin sonsuz dongu mantigiyla yonetimini saglar.
/// </summary>
public class RoadManager : MonoBehaviour
{
    [Header("Yol Geometrisi ve Döngü Ayarları")]
    [Tooltip("Sahnede birbirini takip edecek olan ana yol parçası prefabı.")]
    public GameObject roadPrefab;
    [Tooltip("Sahnede aynı anda aktif tutulacak toplam yol parçası sayısı (Görüş mesafesi için).")]
    public int tileCount = 40;
    [Tooltip("Bir yol parçasının Z ekseni üzerindeki net uzunluğu (Metre).")]
    public float tileLength = 50f;
    
    [ContextMenu("Force Refresh Decoration Settings")]
    public void ForceRefresh()
    {
        useProceduralBarriers = true;
        sideObjectChance = 1.0f;
        decorationColor = new Color(0.6f, 0.6f, 0.7f);
        sideOffset = 5.5f;
        Debug.Log("[Gazze] RoadManager settings forced to high visibility.");
    }

    [Header("Çevre ve Görsel Detaylar")]
    [Tooltip("Yola uygulanacak özel kaplama (Asfalt, toprak vb.).")]
    public Texture2D roadTexture;
    [Tooltip("Yolun sağ ve sol kenarlarına rastgele yerleştirilecek dekoratif objeler.")]
    public GameObject[] sideObjectPrefabs;
    [Tooltip("Kenar objelerinin yolun merkezinden ne kadar uzağa (X ekseni) konulacağı.")]
    public float sideOffset = 5f;
    [Range(0, 1)]
    [Tooltip("Her yeni yol parçasında kenar objesi oluşturulma olasılığı.")]
    public float sideObjectChance = 1.0f;

    [Header("Nesne Havuzu (Pooling)")]
    [Tooltip("Performans için bellekte hazır tutulacak çevre objesi sayısı.")]
    public int sideObjectPoolSize = 100;

    [Header("Procedural Side Decoration")]
    [Tooltip("Dinamik olarak bariyerler eklensin mi?")]
    public bool useProceduralBarriers = true;
    [Tooltip("Bariyerlerin ve direklerin ana rengi.")]
    public Color decorationColor = new Color(0.48f, 0.48f, 0.52f);
    [Tooltip("Bariyerlerin üzerine eklenecek ışık şeridi rengi.")]
    public Color glowAccentColor = new Color(1f, 0.4f, 0f, 1f); // Neon Amber

    [Header("Building Assets")]
    public GameObject[] buildingPrefabs;

    private List<GameObject> tiles = new List<GameObject>();
    private List<GameObject> sideObjectPool = new List<GameObject>();
    private List<GameObject> activeSideObjects = new List<GameObject>();
    
    // private float lastTileZ = 0f; // Unused field removed
    private MaterialPropertyBlock propBlock;
    private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
    private static readonly int ColorId = Shader.PropertyToID("_Color");
    private static readonly int MainTexId = Shader.PropertyToID("_MainTex");
    private static readonly int BaseMapId = Shader.PropertyToID("_BaseMap");

    private void Awake()
    {
        propBlock = new MaterialPropertyBlock();
    }

    private void Start()
    {
        // NEW: Check for duplicate road systems (Prefer InfiniteRoadSystem if both exist)
        if (FindFirstObjectByType<Gazze.Managers.InfiniteRoadSystem>() != null)
        {
            Debug.LogWarning("[Gazze] InfiniteRoadSystem detected. Disabling legacy RoadManager to prevent Z-fighting.");
            this.enabled = false;
            return;
        }

        // Önce fallback varlıkları yükle ki havuz (pool) dolu objelerle başlasın
        LoadFallbackEnvironmentAssets();
        
        InitializeSideObjectPool();

        // NEW: Clear ALL existing children to prevent duplicate persistent tiles (Failsafe)
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Destroy(transform.GetChild(i).gameObject);
        }
        tiles.Clear();

        // Inspector'dan düşük bir değer girilmişse arabaların havada belirmesini önlemek için 
        // TrafficManager'in objeleri spawnladığı mesafeden çok daha uzağa yol gitmesini kesinleştiriyoruz.
        TrafficManager tm = FindFirstObjectByType<TrafficManager>();
        float neededDistance = tm != null ? tm.spawnZ + 200f : 800f; // Spawnlanan yerin min 200 birim ilerisinde de yol olsun
        
        int minimumTilesNeeded = Mathf.CeilToInt(neededDistance / tileLength);
        if (tileCount < minimumTilesNeeded)
        {
            tileCount = minimumTilesNeeded;
        }

        // Arkamızda boşluk kalmaması için i'yi -3'ten başlatıyoruz
        for (int i = -3; i < tileCount; i++)
        {
            float zPos = i * tileLength;
            SpawnTile(zPos);
        }
    }

    private void LoadFallbackEnvironmentAssets()
    {
        // Eğer Inspector'dan prefab atanmamışsa, otomatik olarak bizim oluşturduğumuz dosyaları bulmaya çalış
        if (sideObjectPrefabs == null || sideObjectPrefabs.Length == 0)
        {
            List<GameObject> fallbacks = new List<GameObject>();
#if UNITY_EDITOR
            string[] guids = UnityEditor.AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/Prefabs/Environment" });
            foreach (string guid in guids)
            {
                string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
                GameObject prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab != null) fallbacks.Add(prefab);
            }
#endif
            if (fallbacks.Count > 0)
            {
                sideObjectPrefabs = fallbacks.ToArray();
                Debug.Log($"[Gazze] Loaded {sideObjectPrefabs.Length} fallback environment assets.");
            }
        }
        
        // Polygon binalarını otomatik bul
        if (buildingPrefabs == null || buildingPrefabs.Length == 0)
        {
#if UNITY_EDITOR
            FindPolygonPrefabs();
#endif
        }
    }

#if UNITY_EDITOR
    [ContextMenu("Find Polygon Prefabs")]
    public void FindPolygonPrefabs()
    {
        string[] guids = UnityEditor.AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/POLYGON city pack/Prefabs/Buildings" });
        List<GameObject> prefabs = new List<GameObject>();
        foreach (string guid in guids)
        {
            string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab != null) prefabs.Add(prefab);
        }
        buildingPrefabs = prefabs.ToArray();
        UnityEditor.EditorUtility.SetDirty(this);
    }
#endif

    private void InitializeSideObjectPool()
    {
        if (sideObjectPrefabs == null || sideObjectPrefabs.Length == 0) return;

        for (int i = 0; i < sideObjectPoolSize; i++)
        {
            GameObject prefab = sideObjectPrefabs[Random.Range(0, sideObjectPrefabs.Length)];
            GameObject obj = Instantiate(prefab);
            obj.SetActive(false);
            obj.transform.SetParent(this.transform);
            sideObjectPool.Add(obj);
        }
    }

    private GameObject GetSideObjectFromPool()
    {
        for (int i = 0; i < sideObjectPool.Count; i++)
        {
            if (!sideObjectPool[i].activeInHierarchy)
            {
                return sideObjectPool[i];
            }
        }
        
        // Havuz yetersizse genişlet (Opsiyonel)
        if (sideObjectPrefabs.Length > 0)
        {
            GameObject prefab = sideObjectPrefabs[Random.Range(0, sideObjectPrefabs.Length)];
            GameObject obj = Instantiate(prefab);
            obj.SetActive(false);
            obj.transform.SetParent(this.transform);
            sideObjectPool.Add(obj);
            return obj;
        }
        return null;
    }

    private void Update()
    {
        MoveTiles();
    }

    private void SpawnTile(float zPos)
    {
        if (roadPrefab == null) return;

        // Karoyu olustur
        GameObject tile = Instantiate(roadPrefab, new Vector3(0, 0, zPos), Quaternion.identity, transform); // Added parent for cleanup
        tile.name = "RoadTile_" + zPos;
        tiles.Add(tile);
        ApplyWartornTheme(tile);
        SpawnSideObjects(zPos);

        // Dinamik bariyer sistemi
        if (useProceduralBarriers)
        {
            RoadsideDecorator decorator = tile.AddComponent<RoadsideDecorator>();
            decorator.decorationColor = decorationColor;
            decorator.glowAccentColor = glowAccentColor;
            decorator.sideOffset = sideOffset;
            decorator.tileLength = tileLength;
            decorator.buildingPrefabs = buildingPrefabs;
            decorator.SpawnDecorations();
        }
    }

    private void MoveTiles()
    {
        float speed = PlayerController.Instance != null ? PlayerController.Instance.currentWorldSpeed : 20f;
        float moveDelta = speed * Time.deltaTime;

        // Yol parçalarını hareket ettir
        for (int i = 0; i < tiles.Count; i++)
        {
            tiles[i].transform.Translate(Vector3.back * moveDelta, Space.World);

            // Z konumu başlangıçtan arkaya düşünce (1 parça geriye düştüğünde)
            if (tiles[i].transform.position.z < -tileLength * 3)
            {
                RelocateTile(tiles[i]);
            }
        }

        // Yan objeleri hareket ettir
        for (int i = activeSideObjects.Count - 1; i >= 0; i--)
        {
            GameObject sideObj = activeSideObjects[i];
            if (sideObj == null) { activeSideObjects.RemoveAt(i); continue; }

            sideObj.transform.Translate(Vector3.back * moveDelta, Space.World);

            if (sideObj.transform.position.z < -tileLength * 3)
            {
                sideObj.SetActive(false);
                activeSideObjects.RemoveAt(i);
                
                // Yeni obje oluşturma mantığı: Eğer öndeki yol parçası boşsa oraya ekle
                if (Random.value < sideObjectChance)
                {
                    // Yan objeyi dinamik olarak en uzak yol parçasının yakınlarına ekle
                    float maxZ = 0f;
                    foreach(var t in tiles) if(t.transform.position.z > maxZ) maxZ = t.transform.position.z;

                    float xPos = (Random.value < 0.5f) ? -sideOffset : sideOffset;
                    CreateSideObject(xPos, maxZ);
                }
            }
        }
    }

    private void RelocateTile(GameObject tile)
    {
        float offset = tiles.Count * tileLength;
        tile.transform.position += new Vector3(0, 0, offset);

        // Eski dekorasyonları temizleyip yenilerini oluştur
        RoadsideDecorator decorator = tile.GetComponent<RoadsideDecorator>();
        if (decorator == null && useProceduralBarriers)
            decorator = tile.AddComponent<RoadsideDecorator>();

        if (decorator != null)
        {
            decorator.decorationColor  = decorationColor;
            decorator.glowAccentColor  = glowAccentColor;
            decorator.sideOffset       = sideOffset;
            decorator.tileLength       = tileLength;
            decorator.buildingPrefabs  = buildingPrefabs;
            decorator.SpawnDecorations();
        }

        SpawnSideObjects(tile.transform.position.z);
    }

    private void CreateSideObject(float xPos, float zPos)
    {
        GameObject sideObj = GetSideObjectFromPool();
        if (sideObj != null)
        {
            Vector3 pos = new Vector3(xPos + Random.Range(-1.5f, 1.5f), 0, zPos + Random.Range(-tileLength/2.5f, tileLength/2.5f));
            sideObj.transform.position = pos;
            sideObj.transform.rotation = Quaternion.Euler(0, Random.Range(0, 360f), 0);
            sideObj.SetActive(true);
            activeSideObjects.Add(sideObj);
        }
    }

    private void SpawnSideObjects(float zPos)
    {
        // sideObjectPrefabs kucuk cevre objeleri icin. Yolun icine girmemesi icin
        // minimum guveli mesafeyi zorla: bariyer(~5.5) + 7 birim buffer = min ±12.5
        float safeSideX = Mathf.Max(sideOffset, 5f) + 7f;

        if (Random.value < sideObjectChance) CreateSideObject(-safeSideX, zPos);
        if (Random.value < sideObjectChance) CreateSideObject( safeSideX, zPos);

        if (Random.value < sideObjectChance * 0.5f) CreateSideObject(-safeSideX * 1.5f, zPos + tileLength * 0.5f);
        if (Random.value < sideObjectChance * 0.5f) CreateSideObject( safeSideX * 1.5f, zPos + tileLength * 0.5f);
    }

    private void ApplyWartornTheme(GameObject tile)
    {
        MeshRenderer[] renderers = tile.GetComponentsInChildren<MeshRenderer>();
        // Eğer doku varsa dokunun kendi renklerini korumak için Beyaz, yoksa belirgin bir Gri kullanıyoruz
        Color roadColor = roadTexture != null ? Color.white : new Color(0.45f, 0.45f, 0.48f); 
        Shader curvedShader = Shader.Find("Custom/CurvedWorld_URP");
        
        foreach (var mr in renderers)
        {
            if (mr != null)
            {
                // URP uyumluluğu ve 'pembe shader' hatasını önlemek için shader'ı zorla güncelle
                // sharedMaterial kullanarak her tile için yeni material instance oluşmasını önlüyoruz (Performance)
                // Ama eğer shader farklıysa mecburen müdahale ediyoruz.
                if (curvedShader != null && mr.sharedMaterial.shader != curvedShader)
                {
                    Material[] mats = mr.sharedMaterials;
                    for (int i = 0; i < mats.Length; i++)
                    {
                        if (mats[i] != null && mats[i].shader != curvedShader)
                        {
                            mats[i] = new Material(mats[i]);
                            mats[i].shader = curvedShader;
                        }
                    }
                    mr.sharedMaterials = mats;
                }

                mr.GetPropertyBlock(propBlock);
                
                // Shader tipine göre ana rengi ata
                if (mr.sharedMaterial.HasProperty("_BaseColor"))
                    propBlock.SetColor(BaseColorId, roadColor);
                else if (mr.sharedMaterial.HasProperty("_Color"))
                    propBlock.SetColor(ColorId, roadColor);

                // Eğer özel bir doku varsa uygula
                if (roadTexture != null)
                {
                    propBlock.SetTexture(MainTexId, roadTexture);
                    propBlock.SetTexture(BaseMapId, roadTexture); // URP desteği
                }
                
                // Tiling bilgisini ayarlayalım (Genişlikte 1, Uzunlukta 40 kez tekrar etsin - Maksimum keskinlik)
                propBlock.SetVector("_BaseMap_ST", new Vector4(1f, 40f, 0, 0));
                propBlock.SetVector("_MainTex_ST", new Vector4(1f, 40f, 0, 0)); 

                // Curved World parametrelerini Player ve Traffic ile senkronize et
                // Bu değerler yolun ufukta doğru şekilde eğilmesini sağlar
                propBlock.SetFloat("_Curvature", 0.002f);
                propBlock.SetFloat("_CurvatureH", -0.0015f);
                propBlock.SetFloat("_HorizonOffset", 10.0f);

                mr.SetPropertyBlock(propBlock);
            }
        }
    }

    private void OnDrawGizmos()
    {
        // 1. Şerit Kenar Sınırlarını (Side Offset) Çiz
        Gizmos.color = Color.magenta;
        Gizmos.DrawLine(new Vector3(-sideOffset, 0, -100), new Vector3(-sideOffset, 0, 800));
        Gizmos.DrawLine(new Vector3(sideOffset, 0, -100), new Vector3(sideOffset, 0, 800));
#if UNITY_EDITOR
        UnityEditor.Handles.Label(new Vector3(-sideOffset, 2, 0), "LEFT SIDE LIMIT");
        UnityEditor.Handles.Label(new Vector3(sideOffset, 2, 0), "RIGHT SIDE LIMIT");
#endif

        // 2. Yol Parçası Sınırlarını Çiz (Sadece Seçiliyken)
        Gizmos.color = new Color(0.5f, 0.5f, 1f, 0.4f);
        if (roadPrefab != null)
        {
             // Örnek olarak 10 parça ileriye doğru sınırları çiz
             for (int i = 0; i < 10; i++)
             {
                 float z = i * tileLength;
                 Gizmos.DrawWireCube(new Vector3(0, 0, z), new Vector3(10f, 0.1f, tileLength));
#if UNITY_EDITOR
                 UnityEditor.Handles.Label(new Vector3(0, 0, z), "ROAD TILE " + i);
#endif
             }
        }
    }
}