using UnityEngine;
using UnityEditor;
using Gazze.UI;

namespace Gazze.Editor
{
    public class MainOptionsOverhaulTrigger : EditorWindow
    {
        [MenuItem("Gazze/UI/Run Main Options Overhaul")]
        public static void Run()
        {
            var target = GameObject.Find("MainOptionsPanel");
            if (target != null)
            {
                var overhaul = target.GetComponent<MainOptionsVisualOverhaul>();
                if (overhaul != null)
                {
                    overhaul.BuildMainOptions();
                    Debug.Log("MainOptionsPanel fixed! UI elements are now visible in the Editor.");
                }
            }
        }
    }
}
