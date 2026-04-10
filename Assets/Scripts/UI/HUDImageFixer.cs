using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
public class HUDImageFixer : MonoBehaviour
{
    public bool updateNow = false;

    void Update()
    {
        if (updateNow)
        {
            FixAll();
            updateNow = false;
        }
    }

    public void FixAll()
    {
        Fix("ScorePanel");
        Fix("SpeedPanel");
        Fix("CoinDisplayPanel");
    }

    private void Fix(string name)
    {
        GameObject go = GameObject.Find(name);
        if (go == null) go = GameObject.Find("Canvas/" + name);
        if (go == null) return;
        
        Image img = go.GetComponent<Image>();
        if (img != null)
        {
            img.color = Color.white;
            img.material = null;
            img.type = Image.Type.Simple;
        }

        Outline outline = go.GetComponent<Outline>();
        if (outline != null) DestroyImmediate(outline);

        Shadow shadow = go.GetComponent<Shadow>();
        if (shadow != null) DestroyImmediate(shadow);
    }
}
