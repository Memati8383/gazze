using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

namespace Gazze.Editor
{
    public static class InputSystemFixer
    {
        [MenuItem("Tools/Fix EventSystem Input Module")]
        public static void FixAllEventSystems()
        {
            var eventSystems = Object.FindObjectsByType<EventSystem>(FindObjectsSortMode.None);
            if (eventSystems.Length == 0)
            {
                var esObj = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
                Undo.RegisterCreatedObjectUndo(esObj, "Create EventSystem");
                // Debug.Log("Sahnede EventSystem yoktu, yeni ve uyumlu bir tane oluşturuldu.");
                return;
            }

            int count = 0;
            foreach (var es in eventSystems)
            {
                var oldModule = es.GetComponent<StandaloneInputModule>();
                if (oldModule != null)
                {
                    Undo.DestroyObjectImmediate(oldModule);
                    Undo.AddComponent<InputSystemUIInputModule>(es.gameObject);
                    count++;
                }
            }

            // Debug.Log($"{count} adet EventSystem üzerindeki eski StandaloneInputModule, InputSystemUIInputModule ile değiştirildi.");
        }
    }
}
