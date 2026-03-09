using UnityEngine;
using UnityEditor;
using System.IO;

public class TestRotations 
{
    [MenuItem("Tools/Test Car Rotations")]
    public static void Test() 
    {
        var showcase = GameObject.Find("ShowcasePoint");
        if (showcase != null && showcase.transform.childCount > 0) 
        {
            var car = showcase.transform.GetChild(0);
            foreach(Transform child in showcase.transform) {
                child.gameObject.SetActive(false);
            }
            car.gameObject.SetActive(true);

            Vector3[] tests = new Vector3[] {
                new Vector3(0, 0, 0),
                new Vector3(0, -90, 0),
                new Vector3(0, 90, 0),
                new Vector3(0, 180, 0),
                new Vector3(90, 0, 0),
                new Vector3(90, 180, 0),
                new Vector3(-90, 0, 0),
                new Vector3(0, 0, 90),
                new Vector3(0, 0, -90),
                new Vector3(-90, -90, 0),
                new Vector3(-90, 90, 0),
                new Vector3(-90, 0, 90)
            };

            for (int i = 0; i < tests.Length; i++) {
                car.localRotation = Quaternion.Euler(tests[i]);
                Debug.Log($"Test {i}: Rotation {tests[i]}");
                // Ekran görüntüsünü alacak kodu eklemek için EditorApplication.delayCall kullanılabilir.
                // Ama Unity Editor'da hemen yenilenmez. Bu yüzden manuel bakarız veya ekran görüntüsünü bir scriptle alırız.
            }
        }
    }
}