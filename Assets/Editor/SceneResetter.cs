using UnityEditor;
using UnityEngine;

public class SceneResetter : Editor
{
    [MenuItem("Tools/Reset Skybox to Default")]
    public static void ResetSkybox()
    {
        // Try to load default skybox material.
        // In many projects it's just RenderSettings.skybox = null; 
        // to use the default procedural one defined in the environment.
        RenderSettings.skybox = null;
        Debug.Log("Skybox reset to default!");
    }
}
