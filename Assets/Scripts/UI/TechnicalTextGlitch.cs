using UnityEngine;
using TMPro;
using System.Collections;

namespace Gazze.UI
{
    public class TechnicalTextGlitch : MonoBehaviour
    {
        private TextMeshProUGUI textMesh;
        private string originalText;
        
        [Header("Glitch Settings")]
        public float glitchProbability = 0.05f;
        public float glitchDuration = 0.1f;
        
        private string glitchChars = "!@#$%^&*()_+-=[]{}|;:,.<>?/0123456789";

        private void Awake()
        {
            textMesh = GetComponent<TextMeshProUGUI>();
            originalText = textMesh.text;
        }

        private void Start()
        {
            StartCoroutine(GlitchRoutine());
        }

        private IEnumerator GlitchRoutine()
        {
            while (true)
            {
                yield return new WaitForSeconds(Random.Range(2f, 7f));
                
                if (Random.value < glitchProbability)
                {
                    // Trigger Glitch
                    int randomIdx = Random.Range(0, originalText.Length);
                    char originalChar = originalText[randomIdx];
                    
                    // Char swap
                    char[] modified = originalText.ToCharArray();
                    modified[randomIdx] = glitchChars[Random.Range(0, glitchChars.Length)];
                    textMesh.text = new string(modified);
                    
                    yield return new WaitForSeconds(glitchDuration);
                    
                    textMesh.text = originalText;
                }
            }
        }
    }
}
