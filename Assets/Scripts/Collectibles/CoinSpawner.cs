using UnityEngine;
using System.Collections.Generic;

namespace Gazze.Collectibles
{
    /// <summary>
    /// Coin (Yardım Kolisi) spawn ve havuz yönetimini sağlar.
    /// Bağımsız çalışır, yol (RoadManager) curvature ayarlarıyla uyumludur.
    /// </summary>
    public class CoinSpawner : MonoBehaviour
    {
        public static CoinSpawner Instance { get; private set; }

        [Header("Görsel Varlık (Prefab)")]
        [Tooltip("Altın (Coin) veya yardım kolisi olarak kullanılacak prefab.")]
        public GameObject coinPrefab;

        [Header("Havuz ve Oluşturma (Spawn) Ayarları")]
        [Tooltip("Nesne havuzunda (object pool) tutulacak maksimum nesne sayısı.")]
        public int poolSize = 40;
        [Tooltip("Yeni nesnelerin oyuncunun ne kadar önünde (Z ekseni) belireceği.")]
        public float spawnZ = 300f;
        [Tooltip("Geride kalan nesnelerin ne kadar arkada yok edileceği.")]
        public float despawnZ = -50f;
        [Tooltip("Nesnelerin yerden yüksekliği.")]
        public float spawnY = 1.3f;

        [Header("Şerit Ayarları")]
        [Tooltip("Nesnelerin oluşabileceği şeritlerin X koordinatları.")]
        public float[] lanes = { -2.25f, 0f, 2.25f };

        [Header("Grup Oluşturma Ayarları")]
        [Tooltip("Coinlerin tek tek yerine grup halinde çıkma ihtimali (0.0 - 1.0).")]
        [Range(0f, 1f)] public float groupSpawnChance = 0.50f;
        [Tooltip("Bir grupta kaç adet coin bulunacağı.")]
        public int groupCount = 2;
        [Tooltip("Gruptaki coinler arasındaki mesafe (Z ekseni).")]
        public float groupSpacing = 15f;

        [Header("Güçlendirici (Power-Up) Ayarları")]
        [Tooltip("Grup yerine bir güçlendirici nesnesinin çıkma ihtimali.")]
        [Range(0f, 1f)] public float powerUpChance = 0.10f;
        [Tooltip("Kullanılabilir güçlendirici prefabları listesi.")]
        public GameObject[] powerUpPrefabs;
        [Tooltip("Hangi güçlendiricinin ne kadar sıklıkla çıkacağını belirleyen ağırlıklar.")]
        public float[] powerUpWeights = { 80f, 40f, 40f };

        /// <summary> Sahne donmalarını engellemek için önceden oluşturulan coin listesi. </summary>
        private List<GameObject> coinPool = new List<GameObject>();
        /// <summary> Sahnedeki aktif (hareket eden) güçlendiriciler. </summary>
        private List<GameObject> activePowerUps = new List<GameObject>();
        /// <summary> Peş peşe güçlü yeteneklerin gelmesini sınırlamak için son index. </summary>
        private int lastPowerUpIndex = -1;

        /// <summary> Kat edilen mesafeyi sayan değişken. </summary>
        private float spawnDistanceCounter;
        /// <summary> Bir sonraki nesne spawn olana kadar geçmesi gereken mesafe. </summary>
        private float nextSpawnDistance;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            InitializePool();
            nextSpawnDistance = Random.Range(30f, 60f);
        }

        private void Update()
        {
            if (Time.timeScale == 0f) return;

            // Geri sayım bitmeden coin oluşturma
            if (Gazze.UI.CountdownManager.Instance != null && !Gazze.UI.CountdownManager.Instance.IsGameStarted) return;

            float speed = PlayerController.Instance != null ? PlayerController.Instance.currentWorldSpeed : 20f;

            MoveObjects(speed);
            CheckSpawn(speed);
        }

        private void InitializePool()
        {
            for (int i = 0; i < poolSize; i++)
            {
                GameObject coin = CreateNewCoin();
                coin.SetActive(false);
                coinPool.Add(coin);
            }
        }

        private GameObject CreateNewCoin()
        {
            GameObject coin;
            if (coinPrefab != null)
            {
                coin = Instantiate(coinPrefab);
            }
            else
            {
                coin = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                coin.transform.localScale = Vector3.one * 0.8f;
                // Mesh renderer kullanılmaması için (visuals scriptte oluşturulur)
                DestroyImmediate(coin.GetComponent<MeshFilter>());
                DestroyImmediate(coin.GetComponent<MeshRenderer>());
            }

            coin.tag = "Coin";
            
            Collider col = coin.GetComponent<Collider>();
            if (col == null) col = coin.AddComponent<SphereCollider>();
            col.isTrigger = true;

            if (coin.GetComponent<CoinController>() == null)
            {
                coin.AddComponent<CoinController>();
            }

            if (coin.GetComponent<Rigidbody>() == null)
            {
                Rigidbody rb = coin.AddComponent<Rigidbody>();
                rb.useGravity = false;
                rb.isKinematic = true;
            }

            return coin;
        }

        private GameObject GetCoinFromPool()
        {
            for (int i = coinPool.Count - 1; i >= 0; i--)
            {
                if (coinPool[i] == null) { coinPool.RemoveAt(i); continue; }
                if (!coinPool[i].activeInHierarchy) return coinPool[i];
            }

            // Pool doluysa ekle
            GameObject newCoin = CreateNewCoin();
            newCoin.SetActive(false);
            coinPool.Add(newCoin);
            return newCoin;
        }

        private void CheckSpawn(float speed)
        {
            spawnDistanceCounter += speed * Time.deltaTime;
            if (spawnDistanceCounter >= nextSpawnDistance)
            {
                spawnDistanceCounter = 0f;
                nextSpawnDistance = Random.Range(30f, 60f);
                SpawnBatch();
            }
        }

        private void SpawnBatch()
        {
            float lane = lanes[Random.Range(0, lanes.Length)];

            // % Power-up spawn
            if (powerUpPrefabs != null && powerUpPrefabs.Length > 0 && Random.value < powerUpChance)
            {
                SpawnPowerUp(lane);
                return;
            }

            // Aksi halde Coin grubu
            bool isGroup = Random.value < groupSpawnChance;
            int count = isGroup ? groupCount : 1;

            for (int i = 0; i < count; i++)
            {
                GameObject coin = GetCoinFromPool();
                if (coin == null) continue;

                Vector3 spawnPos = new Vector3(lane, spawnY, spawnZ + (i * groupSpacing));
                coin.transform.position = spawnPos;
                coin.SetActive(true); // CoinController OnEnable çağrılır
            }
        }

        private void SpawnPowerUp(float lane)
        {
            int randomIndex = 0;

            if (powerUpWeights != null && powerUpWeights.Length == powerUpPrefabs.Length)
            {
                float totalWeight = 0;
                foreach (float w in powerUpWeights) totalWeight += w;

                float randomWeight = Random.Range(0, totalWeight);
                float currentWeight = 0;

                for (int i = 0; i < powerUpWeights.Length; i++)
                {
                    currentWeight += powerUpWeights[i];
                    if (randomWeight <= currentWeight)
                    {
                        randomIndex = i;
                        break;
                    }
                }
            }
            else
            {
                randomIndex = Random.Range(0, powerUpPrefabs.Length);
            }

            // Peş peşe güçlü powerup (shield, ghost vb) gelmemesi için
            if (powerUpPrefabs.Length > 1 && lastPowerUpIndex != 0 && lastPowerUpIndex != -1)
            {
                randomIndex = 0; // Genelde magnet veya coin çarpanı 0 indekslidir
            }
            lastPowerUpIndex = randomIndex;

            GameObject prefab = powerUpPrefabs[randomIndex];
            if (prefab == null) return;
            
            GameObject pu = Instantiate(prefab);
            pu.transform.localScale = Vector3.one * 0.25f;
            pu.transform.position = new Vector3(lane, 1.5f, 200f);
            
            activePowerUps.Add(pu);
        }

        private void MoveObjects(float speed)
        {
            // Move Coins
            for (int i = coinPool.Count - 1; i >= 0; i--)
            {
                GameObject coin = coinPool[i];
                if (coin == null) { coinPool.RemoveAt(i); continue; }
                if (!coin.activeInHierarchy) continue;

                coin.transform.Translate(Vector3.back * speed * Time.deltaTime, Space.World);

                if (coin.transform.position.z < despawnZ)
                    coin.SetActive(false);
            }

            // Move Power-Ups
            for (int i = activePowerUps.Count - 1; i >= 0; i--)
            {
                GameObject pu = activePowerUps[i];
                if (pu == null || !pu.activeInHierarchy) 
                { 
                    activePowerUps.RemoveAt(i); 
                    if (pu != null) Destroy(pu); 
                    continue; 
                }

                if (pu.transform.position.z < despawnZ)
                {
                    activePowerUps.RemoveAt(i);
                    Destroy(pu);
                }
                else
                {
                    pu.transform.Translate(Vector3.back * speed * Time.deltaTime, Space.World);
                }
            }
        }

        private void OnDrawGizmos()
        {
            // 1. Coin Şeritlerini Çiz
            Gizmos.color = Color.cyan;
            foreach (float lx in lanes)
            {
                Gizmos.DrawLine(new Vector3(lx, spawnY, despawnZ), new Vector3(lx, spawnY, spawnZ));
                
                // Spawn Y yüksekliği görseli
                Gizmos.DrawWireCube(new Vector3(lx, spawnY, spawnZ), Vector3.one * 0.5f);
#if UNITY_EDITOR
                UnityEditor.Handles.Label(new Vector3(lx, spawnY + 1f, spawnZ), "COIN LANE: " + lx);
#endif
            }

            // 2. Spawn Sınırı
            Gizmos.color = new Color(0, 1, 1, 0.5f);
            Gizmos.DrawCube(new Vector3(0, spawnY, spawnZ), new Vector3(10, 0.1f, 1f));
#if UNITY_EDITOR
            UnityEditor.Handles.Label(new Vector3(0, spawnY + 2f, spawnZ), "COIN SPAWN Z");
#endif
        }
    }
}
