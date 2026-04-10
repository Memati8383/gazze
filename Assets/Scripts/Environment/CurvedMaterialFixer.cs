using UnityEngine;

namespace Gazze.Environment
{
    public class CurvedMaterialFixer : MonoBehaviour
    {
        public Material[] materialsToFix;
        public float curvature = 0.005f;

        [ContextMenu("Fix Curvature")]
        void Start()
        {
            foreach (var mat in materialsToFix)
            {
                if (mat != null && mat.HasProperty("_Curvature"))
                {
                    mat.SetFloat("_Curvature", curvature);
                }
            }
        }
    }
}
