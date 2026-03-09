using UnityEngine;
using UnityEditor;

public class TestRots 
{
    [MenuItem("Tools/Rot0_0_0")]
    public static void R1() { SetRot(0, 0, 0); }
    [MenuItem("Tools/Rot0_180_0")]
    public static void R2() { SetRot(0, 180, 0); }
    [MenuItem("Tools/Rot-90_0_0")]
    public static void R3() { SetRot(-90, 0, 0); }
    [MenuItem("Tools/Rot-90_180_0")]
    public static void R4() { SetRot(-90, 180, 0); }
    [MenuItem("Tools/Rot0_-90_0")]
    public static void R5() { SetRot(0, -90, 0); }
    [MenuItem("Tools/Rot0_90_0")]
    public static void R6() { SetRot(0, 90, 0); }
    [MenuItem("Tools/Rot-90_90_0")]
    public static void R7() { SetRot(-90, 90, 0); }
    [MenuItem("Tools/Rot-90_-90_0")]
    public static void R8() { SetRot(-90, -90, 0); }

    private static void SetRot(float x, float y, float z) {
        var showcase = GameObject.Find("ShowcasePoint");
        if (showcase != null) {
            foreach(Transform child in showcase.transform) {
                child.localRotation = Quaternion.Euler(x, y, z);
            }
            Debug.Log($"Set rot to {x}, {y}, {z}");
            UnityEditor.SceneManagement.EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        }
    }
}