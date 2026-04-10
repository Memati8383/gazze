using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using System.IO;

[InitializeOnLoad]
public class SetupEnvironmentAssets
{
    static SetupEnvironmentAssets()
    {
        EditorApplication.delayCall += () => {
            ExecuteAssetCreation();
        };
    }

    [MenuItem("Gazze/Setup Environment Assets")]
    public static void ExecuteAssetCreation()
    {
        string path = "Assets/Prefabs/Environment";
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }

        // Only create if missing
        if (!File.Exists($"{path}/Env_ConcreteBarrier.prefab"))
            CreatePrefab(path, "Env_ConcreteBarrier", PrimitiveType.Cube, new Vector3(0.5f, 1.2f, 2f), new Color(0.2f, 0.2f, 0.22f));
        
        if (!File.Exists($"{path}/Env_SciFiPillar.prefab"))
            CreatePrefab(path, "Env_SciFiPillar", PrimitiveType.Cylinder, new Vector3(0.4f, 8f, 0.4f), Color.white);
            
        if (!File.Exists($"{path}/Env_FloatingShard.prefab"))
            CreatePrefab(path, "Env_FloatingShard", PrimitiveType.Cube, new Vector3(5f, 5f, 5f), new Color(0.1f, 0.1f, 0.1f));

        AssetDatabase.Refresh();
    }

    private static void CreatePrefab(string folder, string name, PrimitiveType type, Vector3 scale, Color color)
    {
        GameObject go = GameObject.CreatePrimitive(type);
        go.name = name;
        go.transform.localScale = scale;
        
        // Try to find the Curved shader first, fallback to URP Lit
        Shader shader = Shader.Find("Custom/CurvedWorld_URP");
        if (shader == null) shader = Shader.Find("Universal Render Pipeline/Lit");
        
        Material mat = new Material(shader);
        mat.color = color;
        if (shader.name.Contains("CurvedWorld"))
        {
            mat.SetFloat("_Curvature", 0.002f);
            mat.SetFloat("_CurvatureH", -0.0015f);
            mat.SetFloat("_HorizonOffset", 10.0f);
        }
        
        go.GetComponent<MeshRenderer>().sharedMaterial = mat;
        
        string assetPath = $"{folder}/{name}.prefab";
        PrefabUtility.SaveAsPrefabAsset(go, assetPath);
        Object.DestroyImmediate(go);
        
        Debug.Log($"[Gazze] Created Environment Prefab: {assetPath}");
    }
}
#endif
