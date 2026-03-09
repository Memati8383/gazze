using UnityEditor;
using UnityEngine;

namespace Gazze.Editor
{
    public class FixSerializationCorruption
    {
        [MenuItem("Tools/Gazze/Fix Corrupted Assets (Reserialize)")]
        public static void ReserializeEverything()
        {
            Debug.Log("<color=yellow>Gazze: Projedeki tüm sahneler ve prefablar yeniden serileştiriliyor. Bu işlem birkaç dakika sürebilir, lütfen bekleyin...</color>");
            
            // Bu komut, projede gizlice bozulan, dizileri/referansları kopan (OutOfBounds hatası verdiren) 
            // tüm assetlerin Unity tarafından zorla yeniden yazılmasını sağlar.
            AssetDatabase.ForceReserializeAssets();
            
            Debug.Log("<color=green>Gazze: Yeniden serileştirme başarıyla tamamlandı!</color>");
        }
    }
}
