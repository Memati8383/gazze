using UnityEngine;
using UnityEditor;

public static class MissingScriptFixer
{
    [MenuItem("Gazze/Debug/Find Missing Scripts")]
    public static void Find()
    {
        var allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
        int count = 0;
        foreach (var go in allObjects)
        {
            if (go.hideFlags != HideFlags.None) continue;
            var components = go.GetComponents<Component>();
            for (int i = 0; i < components.Length; i++)
            {
                if (components[i] == null)
                {
                    string path = go.name;
                    Transform t = go.transform;
                    while(t.parent != null) {
                        t = t.parent;
                        path = t.name + "/" + path;
                    }
                    Debug.LogError($"Missing script on GameObject: {path}", go);
                    count++;
                }
            }
        }
        Debug.Log($"Finished searching. Found {count} missing scripts.");
    }
}
