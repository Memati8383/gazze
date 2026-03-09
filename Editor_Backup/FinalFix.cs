using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public class FinalFix 
{
    [MenuItem("Tools/Final Fix Player")]
    public static void Fix() 
    {
        var player = GameObject.Find("Player");
        if (player == null) {
            Debug.LogError("Player not found in scene!");
            return;
        }

        // Rigidbody fix
        var rb = player.GetComponent<Rigidbody>();
        if (rb != null) {
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        // Remove mesh filters on parent
        var mf = player.GetComponent<MeshFilter>();
        if (mf != null) Object.DestroyImmediate(mf);
        var mr = player.GetComponent<MeshRenderer>();
        if (mr != null) Object.DestroyImmediate(mr);

        // Fix collider
        var colls = player.GetComponents<BoxCollider>();
        foreach(var c in colls) Object.DestroyImmediate(c);
        var bc = player.AddComponent<BoxCollider>();
        bc.size = new Vector3(2, 1, 4);
        bc.center = new Vector3(0, 0.5f, 0);

        // Fix PlayerController
        var pc = player.GetComponent<PlayerController>();
        if (pc == null) {
            pc = player.AddComponent<PlayerController>();
            Debug.Log("Re-added missing PlayerController");
        }
        pc.carSpawnRotation = new Vector3(-90, 270, 0);
        EditorUtility.SetDirty(pc);

        player.transform.rotation = Quaternion.identity;
        player.transform.position = new Vector3(0, 0.3f, 0);

        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
        Debug.Log("Player Fixed!");
    }
}