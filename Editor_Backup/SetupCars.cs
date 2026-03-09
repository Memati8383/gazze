using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

public class SetupCars 
{
    [MenuItem("Tools/Setup Car Prefabs")]
    public static void CreateCarPrefabs() 
    {
        string resourcesPath = "Assets/Resources/Cars";
        if (!Directory.Exists(resourcesPath)) 
        {
            Directory.CreateDirectory(resourcesPath);
        }

        var assets = AssetDatabase.LoadAllAssetsAtPath("Assets/textures/LowPoly_Cars.FBX");
        List<GameObject> chosenCars = new List<GameObject>();

        // Find the root object to get children from
        foreach(var a in assets) 
        {
            if (a is GameObject go && go.transform.parent == null) 
            {
                foreach (Transform child in go.transform) 
                {
                    if (child.name.StartsWith("car_") || child.name.StartsWith("tractor_") || child.name.StartsWith("truck_")) 
                    {
                        // Sadece ilk bazi modelini alalım
                        if (chosenCars.Find(c => c.name == child.name) == null) 
                        {
                            chosenCars.Add(child.gameObject);
                        }
                    }
                }
            }
        }

        int count = 0;
        foreach (var car in chosenCars) 
        {
            if (count >= 5) break; // 5 tane yeterli olur
            
            GameObject instance = Object.Instantiate(car);
            // Sifirla
            instance.transform.position = Vector3.zero;
            instance.transform.rotation = Quaternion.identity;
            instance.transform.localScale = Vector3.one;

            string path = resourcesPath + "/" + count + "_" + car.name + ".prefab";
            PrefabUtility.SaveAsPrefabAsset(instance, path);
            Object.DestroyImmediate(instance);
            count++;
        }
        
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Created " + count + " car prefabs in " + resourcesPath);
    }
}