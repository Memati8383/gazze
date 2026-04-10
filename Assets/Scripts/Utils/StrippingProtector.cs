using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;
using Gazze.Models;

[assembly: AlwaysLinkAssembly]

namespace Gazze
{
    /// <summary>
    /// Forces IL2CPP to NOT strip these types, arrays, and lists, 
    /// completely preventing CachedReader::OutOfBoundsError crashes.
    /// </summary>
    public class StrippingProtector : MonoBehaviour
    {
        /// <summary>
        /// Linker tarafindan korunmasi gereken tipleri olusturarak strip edilmesini engeller.
        /// </summary>
        [Preserve]
        public void PreserveTypes()
        {
            // Preserve basic arrays used in UI and Logic
            var t1 = new UnityEngine.GameObject[0];
            var t2 = new UnityEngine.Transform[0];
            var t3 = new UnityEngine.UI.Button[0];
            var t4 = new TMPro.TextMeshProUGUI[0];
            var t5 = new string[0];
            
            // Preserve Models & Collections
            var t6 = new List<VehicleAttributes>();
            var t7 = new List<SurfaceMultiplier>();
            var t8 = new SurfaceMultiplier[0];
            var t9 = new VehicleAttributes[0];

            // Prevent optimization
            if (t1.Length > 0 || t2.Length > 0 || t3.Length > 0 || t4.Length > 0 || t5.Length > 0 
                || t6.Count > 0 || t7.Count > 0 || t8.Length > 0 || t9.Length > 0)
            {
                Debug.Log("Preserved generic collections to prevent OutOfBounds errors.");
            }
        }
    }
}
