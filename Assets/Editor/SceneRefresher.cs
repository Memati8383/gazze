using UnityEditor;
using UnityEngine;

public class SceneRefresher : Editor
{
    [MenuItem("Tools/Apply Modern Skybox")]
    public static void ApplySkybox()
    {
        Material skyboxMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/Skybox_Neon.mat");
        if (skyboxMat != null)
        {
            RenderSettings.skybox = skyboxMat;
            Debug.Log("Modern Skybox Applied!");
        }
        else
        {
            Debug.LogError("Skybox Material not found at Assets/Materials/Skybox_Neon.mat");
        }
    }
}
