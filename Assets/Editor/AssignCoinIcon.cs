using UnityEngine;
using UnityEditor;
using Gazze.Collectibles;

public static class AssignCoinIcon
{
    [MenuItem("Tools/Assign Coin Icon")]
    public static void Assign()
    {
        string prefabPath = "Assets/Prefabs/CoinPrefab.prefab";
        string spritePath = "Assets/Violet Theme Ui/Colored Icons/Coin.png";

        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        if (prefab == null) return;

        CoinController controller = prefab.GetComponent<CoinController>();
        if (controller == null) return;

        Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
        if (sprite == null)
        {
            Debug.LogError("Sprite not found at " + spritePath);
            return;
        }

        controller.icon = sprite;
        EditorUtility.SetDirty(prefab);
        PrefabUtility.SavePrefabAsset(prefab);
        Debug.Log("Coin icon assigned successfully!");
    }
}
