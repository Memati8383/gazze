using UnityEngine;
using System.Collections.Generic;

public class RoadManager : MonoBehaviour
{
    [Header("Yol Ayarları")]
    public GameObject roadPrefab;
    public int tileCount = 40; // 20'den 40'a çıkarıldı (Yoğunluk artırıldı)
    public float tileLength = 50f;

    [Header("Çevre ve Görünüm")]
    public Texture2D roadTexture;
    public GameObject[] sideObjectPrefabs;
    public float sideOffset = 6f;
    [Range(0, 1)]
    public float sideObjectChance = 0.85f; // 0.5'ten 0.85'e çıkarıldı (Daha sık çevre objesi)

    [Header("Pooling Ayarları")]
    public int sideObjectPoolSize = 100;

    private List<GameObject> tiles = new List<GameObject>();
    private List<GameObject> sideObjectPool = new List<GameObject>();
    private List<GameObject> activeSideObjects = new List<GameObject>();
    
    // private float lastTileZ = 0f; // Unused field removed
    private MaterialPropertyBlock propBlock;
    private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
    private static readonly int ColorId = Shader.PropertyToID("_Color");
    private static readonly int MainTexId = Shader.PropertyToID("_MainTex");

    private void Awake()
    {
        propBlock = new MaterialPropertyBlock();
    }

    private void Start()
    {
        InitializeSideObjectPool();

        // Inspector'dan düşük bir değer girilmişse arabaların havada belirmesini önlemek için 
        // TrafficManager'in objeleri spawnladığı mesafeden çok daha uzağa yol gitmesini kesinleştiriyoruz.
        TrafficManager tm = FindFirstObjectByType<TrafficManager>();
        float neededDistance = tm != null ? tm.spawnZ + 200f : 800f; // Spawnlanan yerin min 200 birim ilerisinde de yol olsun
        
        int minimumTilesNeeded = Mathf.CeilToInt(neededDistance / tileLength);
        if (tileCount < minimumTilesNeeded)
        {
            tileCount = minimumTilesNeeded;
        }

        // Oyuncunun kamerasının hemen arkasını da kaplamak için i'yi -1'den başlatıyoruz
        for (int i = -1; i < tileCount; i++)
        {
            float zPos = i * tileLength;
            SpawnTile(zPos);
        }
    }

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
        GameObject tile = Instantiate(roadPrefab, new Vector3(0, 0, zPos), Quaternion.identity);
        tile.transform.SetParent(this.transform); // Hiyerarşi temizliği için
        tiles.Add(tile);
        ApplyWartornTheme(tile);
        SpawnSideObjects(zPos);
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
            if (tiles[i].transform.position.z < -tileLength)
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

            if (sideObj.transform.position.z < -tileLength)
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
        // Aracın hızı, frame gecikmeleri gibi sebeplerle -tileLength hizasını bir miktar kaçırmış olabiliriz.
        // Bu yüzden yolun pürüzsüz ilerlemesi ve hiçbir boşluk kalmaması için, bir sonraki ekleneceği
        // yeri eski yerine göre matematiksel olarak hesaplayıp koyuyoruz. Her zaman tiles.Count kadar yanyana dizililer.
        float offset = tiles.Count * tileLength;
        tile.transform.position += new Vector3(0, 0, offset);
        
        // Her yeni tile eklendiğinde yanına obje ekleme şansı ver
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
        // Daha fazla yoğunluk için her iki tarafa da birden fazla obje eklenebilir
        if (Random.value < sideObjectChance) CreateSideObject(-sideOffset, zPos);
        if (Random.value < sideObjectChance) CreateSideObject(sideOffset, zPos);
        
        // Ekstra yoğunluk: Orta-uzak mesafe için ek şans
        if (Random.value < sideObjectChance * 0.5f) CreateSideObject(-sideOffset * 1.5f, zPos + tileLength * 0.5f);
        if (Random.value < sideObjectChance * 0.5f) CreateSideObject(sideOffset * 1.5f, zPos + tileLength * 0.5f);
    }

    private void ApplyWartornTheme(GameObject tile)
    {
        MeshRenderer[] renderers = tile.GetComponentsInChildren<MeshRenderer>();
        Color roadColor = new Color(0.15f, 0.15f, 0.17f);
        
        foreach (var mr in renderers)
        {
            if (mr != null)
            {
                mr.GetPropertyBlock(propBlock);
                
                // URP ve Standart shader uyumluluğu için
                if (mr.sharedMaterial.HasProperty("_BaseColor"))
                    propBlock.SetColor(BaseColorId, roadColor);
                else
                    propBlock.SetColor(ColorId, roadColor);

                if (roadTexture != null)
                    propBlock.SetTexture(MainTexId, roadTexture);

                mr.SetPropertyBlock(propBlock);
            }
        }
    }
}