using UnityEngine;
using UnityEditor;
using Gazze.Collectibles;

public static class UpdateCoinPrefab
{
    [MenuItem("Tools/Update Coin Prefab Scale")]
    public static void UpdatePrefab()
    {
        string prefabPath = "Assets/Prefabs/CoinPrefab.prefab";
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        if (prefab == null) return;

        CoinController controller = prefab.GetComponent<CoinController>();
        if (controller == null) return;

        controller.iconScale = 0.25f;
        controller.glowScale = 0.45f;
        controller.pulseAmount = 0.15f;
        
        // Find and assign the coin sprite
        Object[] assets = AssetDatabase.LoadAllAssetsAtPath("Assets/Violet Theme Ui/Colored Icons/Coin.png");
        foreach(Object asset in assets) {
            if (asset is Sprite) {
                controller.icon = (Sprite)asset;
                break;
            }
        }
        
        EditorUtility.SetDirty(prefab);
        PrefabUtility.SavePrefabAsset(prefab);
        Debug.Log("Coin prefab scale and sprite updated successfully!");
    }
}
