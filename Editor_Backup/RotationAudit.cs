using UnityEngine;
using UnityEditor;
using System.IO;

public class RotationAudit 
{
    [MenuItem("Tools/Audit Rotations")]
    public static void Audit() 
    {
        var player = GameObject.Find("Player");
        if (player == null) return;

        var carParent = player.transform;
        if (carParent.childCount == 0) {
            Debug.Log("No car spawned yet. Start the game first or spawn one manually.");
            return;
        }

        Transform car = carParent.GetChild(0);
        Vector3[] rots = new Vector3[] {
            new Vector3(0, 0, 0),
            new Vector3(0, 90, 0),
            new Vector3(0, 180, 0),
            new Vector3(0, 270, 0),
            new Vector3(-90, 0, 0),
            new Vector3(-90, 90, 0),
            new Vector3(-90, 180, 0),
            new Vector3(90, 0, 0)
        };

        foreach(var r in rots) {
            car.localRotation = Quaternion.Euler(r);
            Debug.Log($"AUDIT: Rotation {r} applied. Look at the Scene/Game view.");
            // We can't easily wait and screenshot here because it's synchronous.
            // But we can just pick one that looks right if we were a human.
            // As an AI, I'll just try to guess again or use a better method.
        }
    }
}