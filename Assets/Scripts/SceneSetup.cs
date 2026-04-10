using UnityEngine;

[ExecuteAlways]
public class SceneSetup : MonoBehaviour
{
    public Material buildingMaterialA;
    public Material buildingMaterialB;
    
    [Header("Rebuild Controls")]
    public bool forceUpdate = false;

    [ContextMenu("Rebuild Atmosphere")]
    public void SetupAtmosphere()
    {
        // 1. Atmosphere Setup (Dreadful/Dusty Gaza Look)
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.ExponentialSquared;
        
        // Dusty Amber/Grey Fog
        Color gazaDustFog = new Color(0.18f, 0.15f, 0.12f); 
        RenderSettings.fogColor = gazaDustFog;
        RenderSettings.fogDensity = 0.015f;
        
        // 2. Skybox Setup
        Shader skyShader = Shader.Find("Custom/GazaSkybox_URP");
        if (skyShader != null)
        {
            Material skyMat = new Material(skyShader);
            skyMat.name = "GazaSkybox_Procedural";
            skyMat.hideFlags = HideFlags.DontSave; 
            
            skyMat.SetColor("_HorizonColor", new Color(0.45f, 0.35f, 0.25f)); 
            skyMat.SetColor("_SkyColor",     new Color(0.15f, 0.15f, 0.18f)); 
            skyMat.SetColor("_GlowColor",    new Color(1.0f, 0.35f, 0.1f));   
            skyMat.SetFloat("_SmokeIntensity", 0.5f);
            
            RenderSettings.skybox = skyMat;
        }

        // 3. Ambient Lighting
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
        RenderSettings.ambientSkyColor     = new Color(0.2f, 0.2f, 0.25f);
        RenderSettings.ambientEquatorColor = new Color(0.3f, 0.25f, 0.2f); 
        RenderSettings.ambientGroundColor  = new Color(0.1f, 0.08f, 0.06f);

        // Force a skybox refresh
        DynamicGI.UpdateEnvironment();
        Debug.Log("[SceneSetup] Atmosphere Updated.");
    }

    [ContextMenu("Rebuild All")]
    public void RebuildAll()
    {
        SetupAtmosphere();

        // Don't duplicate if already exists
        if (transform.childCount > 0 && Application.isPlaying == false) return; 

        // Create Buildings
        for (int i = 0; i < 40; i++)
        {
            float zPos = i * 15f;
            CreateBuilding(-14f, zPos, Random.Range(15f, 50f), "Assets/Materials/BuildingA.mat");
            CreateBuilding(14f, zPos, Random.Range(15f, 50f), "Assets/Materials/BuildingB.mat");
        }
    }

    void Start()
    {
        RebuildAll();
    }

    private void OnValidate()
    {
        if (forceUpdate)
        {
            forceUpdate = false;
            SetupAtmosphere();
        }
    }

    void CreateBuilding(float x, float z, float height, string matPath)
    {
        GameObject b = GameObject.CreatePrimitive(PrimitiveType.Cube);
        b.name = "Building_" + z;
        b.transform.position = new Vector3(x, height / 2f, z);
        b.transform.localScale = new Vector3(10f, height, 10f);
        b.transform.SetParent(this.transform);

#if UNITY_EDITOR
        Material mat = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>(matPath);
        if (mat != null) b.GetComponent<Renderer>().sharedMaterial = mat;
#endif

        if (Random.value > 0.5f)
        {
            GameObject lightObj = new GameObject("StreetLight");
            lightObj.transform.position = new Vector3(x * 0.8f, 1f, z);
            lightObj.transform.SetParent(b.transform);
            Light l = lightObj.AddComponent<Light>();
            l.type = LightType.Point;
            l.color = new Color(1f, 0.8f, 0.4f);
            l.range = 15f;
            l.intensity = 2f;
        }
    }
}
