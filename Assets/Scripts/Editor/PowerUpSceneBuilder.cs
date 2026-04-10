#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;

namespace Gazze.PowerUps.Editor
{
    public static class PowerUpSceneBuilder
    {
        [MenuItem("Gazze/Power-Ups/Setup New Power-Ups (TimeWarp, ShockWave, Juggernaut)")]
        public static void SetupNewPowerUps()
        {
            // 1. Load icon sprites
            Sprite timeWarpIcon = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/zaman bükücü.png");
            Sprite shockWaveIcon = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Şok Dalgası.png");
            Sprite juggernautIcon = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/dev modu.png");

            if (timeWarpIcon == null) Debug.LogWarning("TimeWarp icon not found at Assets/zaman bükücü.png");
            if (shockWaveIcon == null) Debug.LogWarning("ShockWave icon not found at Assets/Şok Dalgası.png");
            if (juggernautIcon == null) Debug.LogWarning("Juggernaut icon not found at Assets/dev modu.png");

            // 2. Create PowerUp prefabs folder
            if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
                AssetDatabase.CreateFolder("Assets", "Prefabs");
            if (!AssetDatabase.IsValidFolder("Assets/Prefabs/PowerUps"))
                AssetDatabase.CreateFolder("Assets/Prefabs", "PowerUps");

            // 3. Create prefabs for each new power-up type
            CreatePowerUpPrefab("TimeWarp_PowerUp", PowerUpType.TimeWarp, timeWarpIcon,
                new Color(0.4f, 0.2f, 0.9f), "Assets/Prefabs/PowerUps/TimeWarp_PowerUp.prefab");
            CreatePowerUpPrefab("ShockWave_PowerUp", PowerUpType.ShockWave, shockWaveIcon,
                new Color(0.3f, 0.7f, 1f), "Assets/Prefabs/PowerUps/ShockWave_PowerUp.prefab");
            CreatePowerUpPrefab("Juggernaut_PowerUp", PowerUpType.Juggernaut, juggernautIcon,
                new Color(1f, 0.5f, 0f), "Assets/Prefabs/PowerUps/Juggernaut_PowerUp.prefab");

            // 4. Open/Find the SampleScene and set up managers
            SetupSampleScene(timeWarpIcon, shockWaveIcon, juggernautIcon);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("<color=green>[PowerUpSceneBuilder] New power-ups setup complete!</color>");
        }

        private static void CreatePowerUpPrefab(string name, PowerUpType type, Sprite icon, Color color, string path)
        {
            // Check if prefab already exists
            if (AssetDatabase.LoadAssetAtPath<GameObject>(path) != null)
            {
                Debug.Log($"Prefab already exists: {path}");
                return;
            }

            GameObject go = new GameObject(name);
            
            // Add PowerUpItem component
            PowerUpItem item = go.AddComponent<PowerUpItem>();
            item.powerUpType = type;
            item.rotationSpeed = 100f;
            item.floatHeight = 0.2f;
            
            // Add PowerUpVisual component
            PowerUpVisual visual = go.AddComponent<PowerUpVisual>();
            visual.icon = icon;
            visual.glowColor = color;

            // Add SphereCollider as trigger
            SphereCollider col = go.AddComponent<SphereCollider>();
            col.isTrigger = true;
            col.radius = 1.5f;

            // Add Rigidbody (kinematic)
            Rigidbody rb = go.AddComponent<Rigidbody>();
            rb.useGravity = false;
            rb.isKinematic = true;

            // Save as prefab
            PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);

            Debug.Log($"Created prefab: {path}");
        }

        private static void SetupSampleScene(Sprite timeWarpIcon, Sprite shockWaveIcon, Sprite juggernautIcon)
        {
            // Store which scene we're currently in
            string currentScene = SceneManager.GetActiveScene().path;
            
            // Load SampleScene
            Scene gameScene;
            bool needsLoad = false;
            if (SceneManager.GetActiveScene().name == "SampleScene")
            {
                gameScene = SceneManager.GetActiveScene();
            }
            else
            {
                gameScene = EditorSceneManager.OpenScene("Assets/Scenes/SampleScene.unity", OpenSceneMode.Additive);
                needsLoad = true;
            }

            // Find or create PowerUpManager
            var managers = Object.FindObjectsByType<PowerUpManager>(FindObjectsSortMode.None);
            PowerUpManager mgr = managers.Length > 0 ? managers[0] : null;

            if (mgr == null)
            {
                // Create PowerUpManager GameObject
                GameObject mgrGO = new GameObject("PowerUpManager");
                SceneManager.MoveGameObjectToScene(mgrGO, gameScene);
                mgr = mgrGO.AddComponent<PowerUpManager>();
                Debug.Log("Created PowerUpManager in SampleScene");
            }

            // Find or create PowerUpEffects
            var effects = Object.FindObjectsByType<PowerUpEffects>(FindObjectsSortMode.None);
            PowerUpEffects fx = effects.Length > 0 ? effects[0] : null;
            
            if (fx == null)
            {
                GameObject fxGO = new GameObject("PowerUpEffects");
                SceneManager.MoveGameObjectToScene(fxGO, gameScene);
                fx = fxGO.AddComponent<PowerUpEffects>();
                Debug.Log("Created PowerUpEffects in SampleScene");
            }

            // Find or create PowerUpBootstrapper
            var bootstrappers = Object.FindObjectsByType<PowerUpBootstrapper>(FindObjectsSortMode.None);
            if (bootstrappers.Length == 0)
            {
                // Add to the same GO as PowerUpManager
                mgr.gameObject.AddComponent<PowerUpBootstrapper>();
                Debug.Log("Added PowerUpBootstrapper to PowerUpManager");
            }

            // Configure PowerUpManager data
            var dataList = mgr.availablePowerUps != null
                ? new System.Collections.Generic.List<PowerUpData>(mgr.availablePowerUps)
                : new System.Collections.Generic.List<PowerUpData>();

            EnsurePowerUpData(dataList, PowerUpType.TimeWarp, 5f, timeWarpIcon,
                new Color(0.4f, 0.2f, 0.9f), "ZAMAN BÜKÜCÜ");
            EnsurePowerUpData(dataList, PowerUpType.ShockWave, 0.1f, shockWaveIcon,
                new Color(0.3f, 0.7f, 1f), "ŞOK DALGASI");
            EnsurePowerUpData(dataList, PowerUpType.Juggernaut, 6f, juggernautIcon,
                new Color(1f, 0.5f, 0f), "DEV MODU");

            mgr.availablePowerUps = dataList.ToArray();
            EditorUtility.SetDirty(mgr);

            // Load power-up prefabs into CoinSpawner's powerUpPrefabs array
            var coinSpawners = Object.FindObjectsByType<Gazze.Collectibles.CoinSpawner>(FindObjectsSortMode.None);
            if (coinSpawners.Length > 0)
            {
                var spawner = coinSpawners[0];
                var prefabList = spawner.powerUpPrefabs != null
                    ? new System.Collections.Generic.List<GameObject>(spawner.powerUpPrefabs)
                    : new System.Collections.Generic.List<GameObject>();

                AddPrefabIfNotExists(prefabList, "Assets/Prefabs/PowerUps/TimeWarp_PowerUp.prefab");
                AddPrefabIfNotExists(prefabList, "Assets/Prefabs/PowerUps/ShockWave_PowerUp.prefab");
                AddPrefabIfNotExists(prefabList, "Assets/Prefabs/PowerUps/Juggernaut_PowerUp.prefab");

                spawner.powerUpPrefabs = prefabList.ToArray();

                // Update weights array to match prefab count
                if (spawner.powerUpWeights == null || spawner.powerUpWeights.Length != prefabList.Count)
                {
                    float[] newWeights = new float[prefabList.Count];
                    for (int i = 0; i < newWeights.Length; i++)
                    {
                        if (spawner.powerUpWeights != null && i < spawner.powerUpWeights.Length)
                            newWeights[i] = spawner.powerUpWeights[i];
                        else
                            newWeights[i] = 30f; // Default weight for new power-ups
                    }
                    spawner.powerUpWeights = newWeights;
                }

                EditorUtility.SetDirty(spawner);
                Debug.Log($"Updated CoinSpawner with {prefabList.Count} power-up prefabs");
            }

            // Save
            EditorSceneManager.MarkSceneDirty(gameScene);
            EditorSceneManager.SaveScene(gameScene);

            if (needsLoad)
            {
                EditorSceneManager.CloseScene(gameScene, true);
            }
        }

        private static void EnsurePowerUpData(System.Collections.Generic.List<PowerUpData> list,
            PowerUpType type, float duration, Sprite icon, Color color, string name)
        {
            foreach (var d in list)
            {
                if (d.type == type)
                {
                    // Update existing
                    d.duration = duration;
                    d.icon = icon;
                    d.themeColor = color;
                    d.displayName = name;
                    return;
                }
            }

            list.Add(new PowerUpData
            {
                type = type,
                duration = duration,
                icon = icon,
                themeColor = color,
                displayName = name
            });
        }

        private static void AddPrefabIfNotExists(System.Collections.Generic.List<GameObject> list, string path)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null)
            {
                Debug.LogWarning($"Prefab not found: {path}");
                return;
            }

            // Check if already in list
            foreach (var go in list)
            {
                if (go != null && AssetDatabase.GetAssetPath(go) == path) return;
            }

            list.Add(prefab);
        }
    }
}
#endif
