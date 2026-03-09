using UnityEngine;
using UnityEditor;
public class GetFBXInfo {
    [MenuItem("Tools/Print FBX Cars")]
    public static void PrintCars() {
        var assets = AssetDatabase.LoadAllAssetsAtPath("Assets/textures/LowPoly_Cars.FBX");
        foreach(var a in assets) {
            if (a is GameObject go && go.transform.parent == null) {
                // Root
                foreach (Transform child in go.transform) {
                    Debug.Log("Car Child: " + child.name);
                }
            }
        }
    }
}