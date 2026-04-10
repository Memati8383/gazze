using UnityEngine;
using System.Collections.Generic;

namespace Gazze.Managers
{
    [ExecuteAlways]
    public class InfiniteRoadSystem : MonoBehaviour
    {
        [Header("Prefab Settings")]
        [SerializeField] private GameObject tilePrefab;
        [SerializeField] private int initialTiles = 20;
        [SerializeField] private float tileLength = 50f;

        [Header("Procedural Visuals")]
        public bool useProceduralBarriers = true;
        public Color decorationColor = new Color(0.5f, 0.5f, 0.55f);
        public Color glowAccentColor = new Color(1f, 0.5f, 0f, 1f); // Neon Amber
        public float sideOffset = 5.5f;

        [Header("Visuals (Materials)")]
        [SerializeField] private Material roadMaterial;
        [SerializeField] private Texture2D asphaltTexture;

        [Header("Assets")]
        public GameObject[] buildingPrefabs;

        private List<GameObject> activeTiles = new List<GameObject>();
        private Transform cameraTransform;

        void Start()
        {
            // NEW: Check for duplicate road systems
            if (FindFirstObjectByType<RoadManager>() != null)
            {
                Debug.LogWarning("[Gazze] RoadManager detected. Disabling InfiniteRoadSystem to prevent Z-fighting.");
                this.enabled = false;
                return;
            }

            if (Application.isPlaying)
            {
                cameraTransform = Camera.main.transform;
            }

#if UNITY_EDITOR
            if (buildingPrefabs == null || buildingPrefabs.Length == 0)
                FindPolygonPrefabs();
#endif

            RefreshRoad();
        }

        [ContextMenu("Force Refresh Road")]
        public void RefreshRoad()
        {
            // NEW: Clear ALL children to prevent duplicate persistent tiles (especially after recompiles or ExecuteAlways)
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                GameObject child = transform.GetChild(i).gameObject;
                if (Application.isPlaying) Destroy(child);
                else DestroyImmediate(child);
            }
            activeTiles.Clear();

            float spawnZ = -tileLength * 3;
            for (int i = 0; i < initialTiles + 3; i++)
            {
                SpawnTile(spawnZ);
                spawnZ += tileLength;
            }
        }

        void Update()
        {
            if (!Application.isPlaying) return;

            // Geri sayım bitmeden yolu hareket ettirme
            if (Gazze.UI.CountdownManager.Instance != null && !Gazze.UI.CountdownManager.Instance.IsGameStarted) return;

            float speed = 0f;
            if (PlayerController.Instance != null)
                speed = PlayerController.Instance.currentWorldSpeed;

            float moveStep = speed * Time.deltaTime;

            // Move each tile back
            for (int i = 0; i < activeTiles.Count; i++)
            {
                activeTiles[i].transform.Translate(Vector3.back * moveStep, Space.World);
            }

            // Check if first tile needs relocation
            if (activeTiles.Count > 0)
            {
                if (activeTiles[0].transform.position.z < -tileLength * 3)
                {
                    float lastZ = activeTiles[activeTiles.Count - 1].transform.position.z;
                    GameObject firstTile = activeTiles[0];
                    activeTiles.RemoveAt(0);
                    firstTile.transform.position = new Vector3(0, 0, lastZ + tileLength);
                    
                    activeTiles.Add(firstTile); // Önce listeye ekle

                    // Dekorasyonları pozisyon set edildikten SONRA yenile
                    if (useProceduralBarriers)
                    {
                        RoadsideDecorator decorator = firstTile.GetComponent<RoadsideDecorator>();
                        if (decorator == null) decorator = firstTile.AddComponent<RoadsideDecorator>();
                        decorator.decorationColor = decorationColor;
                        decorator.glowAccentColor = glowAccentColor;
                        decorator.sideOffset      = sideOffset;
                        decorator.tileLength      = tileLength;
                        decorator.buildingPrefabs = buildingPrefabs;
                        decorator.SpawnDecorations();
                    }
                }
            }
        }
#if UNITY_EDITOR
        [ContextMenu("Find Polygon Prefabs")]
        public void FindPolygonPrefabs()
        {
            string[] guids = UnityEditor.AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/POLYGON city pack/Prefabs/Buildings" });
            List<GameObject> prefabs = new List<GameObject>();

            foreach (var guid in guids)
            {
                string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
                GameObject go = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (go != null) prefabs.Add(go);
            }

            buildingPrefabs = prefabs.ToArray();
            UnityEditor.EditorUtility.SetDirty(this);
            Debug.Log($"[InfiniteRoadSystem] Found {buildingPrefabs.Length} building prefabs.");
        }
#endif

        private void SpawnTile(float zPos)
        {
            if (tilePrefab == null) return;
            GameObject tile = Instantiate(tilePrefab, new Vector3(0, 0, zPos), Quaternion.identity, transform);
            tile.name = "RoadTile_" + activeTiles.Count;
            ApplyPremiumVisuals(tile);
            
            if (useProceduralBarriers)
            {
                RoadsideDecorator decorator = tile.GetComponent<RoadsideDecorator>();
                if (decorator == null) decorator = tile.AddComponent<RoadsideDecorator>();
                
                decorator.decorationColor = decorationColor;
                decorator.glowAccentColor = glowAccentColor;
                decorator.sideOffset = sideOffset;
                decorator.tileLength = tileLength;
                decorator.buildingPrefabs = buildingPrefabs; // PASS THE PREFABS
                decorator.SpawnDecorations();
            }
            
            activeTiles.Add(tile);
        }

        private void ApplyPremiumVisuals(GameObject tile)
        {
            MeshRenderer mr = tile.GetComponentInChildren<MeshRenderer>();
            if (mr != null && roadMaterial != null)
            {
                mr.sharedMaterial = roadMaterial;
                if (asphaltTexture != null)
                {
                    mr.sharedMaterial.SetTexture("_MainTex", asphaltTexture);
                }
            }
        }
    }
}