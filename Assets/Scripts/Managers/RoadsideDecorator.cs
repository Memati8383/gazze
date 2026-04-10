using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using System.Collections.Generic;
#endif

/// <summary>
/// Yol kenarına prosedürel olarak engel, bina, ışık ve atmosferik dekor oluşturur.
/// [ContextMenu("Rebuild Decorations")] ile editörden tetiklenebilir.
/// </summary>
[ExecuteAlways]
public class RoadsideDecorator : MonoBehaviour
{
    // ─────────────────────────────────────────────
    //  Dekorasyon Ayarları
    // ─────────────────────────────────────────────
    [Header("Decoration Settings")]
    public bool useProceduralBarriers = true;
    public bool useRuinedLook         = true;        // Gaza/Savaş teması için eskitilmiş shader kullan
    public bool updateAtmosphere       = true;        // Gökyüzü ve sis ayarlarını otomatik yap
    public Color decorationColor    = new Color(0.75f, 0.75f, 0.82f);
    public Color glowAccentColor    = new Color(1f, 0.55f, 0f, 1f);   // Brighter Neon Amber
    public float sideOffset         = 5.5f;
    public float tileLength         = 50f;

    [Header("Atmosphere Settings")]
    public bool  addDistantCities = true;
    public bool  addCables        = true;         // (ileride kullanılmak üzere)
    public Color skyCityColor     = new Color(0.12f, 0.12f, 0.15f);

    [Header("Assets")]
    public GameObject[] buildingPrefabs;         // Polygon City prefableri buraya gelecek

    // ─────────────────────────────────────────────
    //  Paylaşılan materyaller (spawn sırasında set edilir)
    // ─────────────────────────────────────────────
    private Material _baseMat;
    private Material _emissionMat;

    // ─────────────────────────────────────────────
    //  Yaşam Döngüsü
    // ─────────────────────────────────────────────
    private void Start() { /* Caller invokes SpawnDecorations() after setting buildingPrefabs */ }

    private void OnValidate()
    {
        if (!Application.isPlaying)
        {
            if (updateAtmosphere && useRuinedLook) SetupGazaAtmosphere();
        }
    }

    // ─────────────────────────────────────────────
    //  Yardımcı: Proc_ çocukları var mı?
    // ─────────────────────────────────────────────
    private bool HasProcDecorations()
    {
        foreach (Transform child in transform)
            if (child.name.StartsWith("Proc_")) return true;
        return false;
    }

    // ─────────────────────────────────────────────
    //  Collider temizleme yardımcısı
    // ─────────────────────────────────────────────
    private void DestroyCollider(GameObject go)
    {
        var col = go.GetComponent<Collider>();
        if (col == null) return;

        if (Application.isPlaying) Destroy(col);
        else                       DestroyImmediate(col);
    }

    // ─────────────────────────────────────────────
    //  Ana spawn metodu
    // ─────────────────────────────────────────────
    [ContextMenu("Rebuild Decorations")]
    public void SpawnDecorations()
    {
#if UNITY_EDITOR
        if (buildingPrefabs == null || buildingPrefabs.Length == 0)
            FindPolygonPrefabs();
#endif

        CleanupProcChildren();
        BuildMaterials();

        if (updateAtmosphere && useRuinedLook) SetupGazaAtmosphere();

        for (int i = 0; i < 2; i++)
        {
            float side = (i == 0) ? -1f : 1f;
            SpawnBarrierAndRailing(side);
            SpawnTrafficLight(side);
        }

        if (addDistantCities) SpawnRuinedCityscape();

        SpawnFoundation();
        SpawnSupportBeams();
        SpawnAbyssalGround();
        SpawnArch();
        SpawnSideGroundFillers();
    }

    private void SpawnSideGroundFillers()
    {
        // Temel blokların yanlarını ve altını doldurmak için zeminler ekliyoruz
        for (int i = 0; i < 2; i++)
        {
            float side = (i == 0) ? -1f : 1f;
            GameObject sand = CreatePrim(PrimitiveType.Plane, "Proc_SideSand");
            sand.transform.localPosition = new Vector3(side * (sideOffset + 50f), -3.5f, 0f);
            sand.transform.localScale    = new Vector3(10f, 1f, 10f); // 100x100m plane
            
            Material sandMat = new Material(_baseMat);
            sandMat.color = new Color(0.12f, 0.10f, 0.08f); // Dusty Brown
            sand.GetComponent<MeshRenderer>().sharedMaterial = sandMat;
            
            ApplyCurvedShader(sand);
        }
    }

    // ─────────────────────────────────────────────
    //  Eski Proc_ çocukları temizle
    // ─────────────────────────────────────────────
    private void CleanupProcChildren()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Transform child = transform.GetChild(i);
            if (!child.name.StartsWith("Proc_")) continue;

            if (Application.isPlaying) Destroy(child.gameObject);
            else                       DestroyImmediate(child.gameObject);
        }
    }

#if UNITY_EDITOR
    [ContextMenu("Find Polygon Prefabs")]
    public void FindPolygonPrefabs()
    {
        string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/POLYGON city pack/Prefabs/Buildings" });
        List<GameObject> prefabs = new List<GameObject>();

        foreach (var guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject go = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (go != null) prefabs.Add(go);
        }

        buildingPrefabs = prefabs.ToArray();
        EditorUtility.SetDirty(this);
        Debug.Log($"[RoadsideDecorator] Found {buildingPrefabs.Length} building prefabs in POLYGON city pack.");
    }
#endif

    // ─────────────────────────────────────────────
    //  Materyal oluşturma
    // ─────────────────────────────────────────────
    private void BuildMaterials()
    {
        Shader ruinedShader = Shader.Find("Custom/GazaRuinedShader_URP");
        Shader curvedShader = Shader.Find("Custom/CurvedWorld_URP");
        Shader fallback     = Shader.Find("Universal Render Pipeline/Lit");
        
        Shader chosenShader = (useRuinedLook && ruinedShader != null) ? ruinedShader : (curvedShader != null ? curvedShader : fallback);

        // Bellek sızıntısını önlemek için eğer zaten oluşturulmuşlarsa shaderlarını güncelle, yeniden yaratma.
        if (_baseMat == null)
        {
            _baseMat = new Material(chosenShader);
            _baseMat.name = "Proc_BaseMat";
        }
        else
        {
            _baseMat.shader = chosenShader;
        }
        _baseMat.color = decorationColor;
        
        if (_emissionMat == null)
        {
            _emissionMat = new Material(chosenShader);
            _emissionMat.name = "Proc_GlowMat";
        }
        else
        {
            _emissionMat.shader = chosenShader;
        }

        // Curvature Sync (Bükülme ayarlarını her iki materyal için de eşle)
        float curV = 0.002f;
        float curH = -0.0015f;
        float horO = 10.0f;

        Material[] targetMats = { _baseMat, _emissionMat };
        foreach (var m in targetMats)
        {
            if (m.HasProperty("_Curvature")) m.SetFloat("_Curvature", curV);
            if (m.HasProperty("_CurvatureH")) m.SetFloat("_CurvatureH", curH);
            if (m.HasProperty("_HorizonOffset")) m.SetFloat("_HorizonOffset", horO);
        }

        // Setup Emission for the glow material
        _emissionMat.EnableKeyword("_EMISSION");
        _emissionMat.SetFloat("_UseEmission", 1.0f); // Shader'daki toggle
        _emissionMat.SetColor("_EmissionColor", glowAccentColor);
        _emissionMat.SetFloat("_EmissionIntensity", 12f);
        
        // Base color for neons (usually match or be secondary)
        _emissionMat.SetColor("_BaseColor", glowAccentColor * 0.5f);
    }

    private void SetupGazaAtmosphere()
    {
        // 1. Fog Setup
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.ExponentialSquared;
        RenderSettings.fogColor = new Color(0.35f, 0.30f, 0.25f); // Brighter Dusty Amber
        RenderSettings.fogDensity = 0.012f; // Slightly less dense for better visibility

        // 2. Skybox Setup
        Shader skyShader = Shader.Find("Custom/GazaSkybox_URP");
        if (skyShader != null)
        {
            Material skyMat = RenderSettings.skybox;
            if (skyMat == null || skyMat.shader != skyShader)
            {
                skyMat = new Material(skyShader);
                skyMat.name = "GazaSkybox_Integrated";
                skyMat.hideFlags = HideFlags.HideAndDontSave;
                RenderSettings.skybox = skyMat;
            }
            
            skyMat.SetColor("_HorizonColor", new Color(0.65f, 0.55f, 0.45f)); 
            skyMat.SetColor("_SkyColor",     new Color(0.25f, 0.25f, 0.30f)); 
            skyMat.SetColor("_GlowColor",    new Color(1.0f, 0.50f, 0.2f));   
            skyMat.SetFloat("_SmokeIntensity", 0.5f);
        }

        // 3. Ambient
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
        RenderSettings.ambientSkyColor     = new Color(0.40f, 0.40f, 0.50f);
        RenderSettings.ambientEquatorColor = new Color(0.50f, 0.45f, 0.40f);
        RenderSettings.ambientGroundColor  = new Color(0.20f, 0.18f, 0.15f);

        DynamicGI.UpdateEnvironment();
    }

    // ─────────────────────────────────────────────
    //  1 ─ Beton bariyer + metal korkuluk
    // ─────────────────────────────────────────────
    private void SpawnBarrierAndRailing(float side)
    {
        float x = side * (sideOffset + 0.8f);
        int segmentCount = 6; // Daha sık segment
        float segLen = tileLength / segmentCount;

        for (int i = 0; i < segmentCount; i++)
        {
            float zPos = -tileLength * 0.5f + (i + 0.5f) * segLen;
            bool isBroken = Random.value < 0.15f; // %15 şansla kırık segment
            
            // 1. Ana Segment Parent
            GameObject segment = new GameObject("Proc_BarrierSegment_" + i + "_" + (side < 0 ? "L" : "R"));
            segment.transform.SetParent(this.transform, false);
            segment.transform.localPosition = new Vector3(x, 0f, zPos);
            
            // 2. Ana Gövde (Beton Panel)
            GameObject panel = CreatePrimInParent(PrimitiveType.Cube, "Panel", segment.transform, false);
            panel.transform.localPosition = new Vector3(0f, 0.5f, 0f);
            panel.transform.localScale    = new Vector3(0.8f, 1.0f, segLen * 0.96f);
            panel.GetComponent<MeshRenderer>().sharedMaterial = _baseMat;

            // 3. Dikey Destek Kolonları (Ribs) - Endüstriyel görünüm için
            for (int r = -1; r <= 1; r += 2)
            {
                GameObject rib = CreatePrimInParent(PrimitiveType.Cube, "Rib", segment.transform, false);
                rib.transform.localPosition = new Vector3(side * -0.3f, 0.6f, r * segLen * 0.4f);
                rib.transform.localScale    = new Vector3(0.2f, 1.2f, 0.2f);
                rib.GetComponent<MeshRenderer>().sharedMaterial = _baseMat;
            }

            // 4. Neon Detayı (Sadece kırık değilse)
            if (!isBroken)
            {
                GameObject neon = CreatePrimInParent(PrimitiveType.Cube, "Neon", segment.transform, false);
                neon.transform.localPosition = new Vector3(side * -0.42f, 0.8f, 0f); 
                neon.transform.localScale    = new Vector3(0.05f, 0.1f, segLen * 0.8f);
                neon.GetComponent<MeshRenderer>().sharedMaterial = _emissionMat;
            }

            // 5. Korkuluk Yerleşimi
            if (!isBroken || Random.value < 0.5f)
            {
                // Korkuluk Direği
                GameObject post = CreatePrimInParent(PrimitiveType.Cube, "Post", segment.transform, false);
                post.transform.localPosition = new Vector3(0f, 1.3f, 0f);
                post.transform.localScale    = new Vector3(0.15f, 0.6f, 0.15f);
                post.GetComponent<MeshRenderer>().sharedMaterial = _baseMat;

                // Yatay Borular (Railings)
                for (int h = 0; h < 2; h++)
                {
                    GameObject rail = CreatePrimInParent(PrimitiveType.Cylinder, "Rail", segment.transform, false);
                    rail.transform.localPosition = new Vector3(0f, 1.2f + h * 0.35f, 0f);
                    rail.transform.localScale    = new Vector3(0.08f, segLen * 0.5f, 0.08f);
                    rail.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
                    rail.GetComponent<MeshRenderer>().sharedMaterial = _baseMat;

                    // Hasar rotasyonu
                    if (isBroken || Random.value < 0.2f)
                    {
                        rail.transform.localRotation *= Quaternion.Euler(
                            Random.Range(-10, 10), 
                            Random.Range(-20, 20), 
                            0f
                        );
                        rail.transform.localPosition += new Vector3(0f, -0.1f, 0f);
                    }
                }
            }
            
            // 6. Ekstra Detay: Kablo demeti (Bazen görünür)
            if (Random.value < 0.3f)
            {
                GameObject wire = CreatePrimInParent(PrimitiveType.Cylinder, "Wire", segment.transform, false);
                wire.transform.localPosition = new Vector3(side * -0.35f, 0.3f, 0f);
                wire.transform.localScale    = new Vector3(0.03f, segLen * 0.5f, 0.03f);
                wire.transform.localRotation = Quaternion.Euler(90f, 0f, Random.Range(-5f, 5f));
                wire.GetComponent<MeshRenderer>().sharedMaterial = _baseMat;
            }
        }
    }

    // ─────────────────────────────────────────────
    //  2 ─ Trafik ışığı (hasar / bükülme efektli)
    // ─────────────────────────────────────────────
    private void SpawnTrafficLight(float side)
    {
        if (Random.value >= 0.20f) return; // %20 şans

        float zPos = Random.Range(-tileLength / 3f, tileLength / 3f);
        float x    = side * sideOffset * 1.5f;

        // Direk
        GameObject pole = CreatePrim(PrimitiveType.Cylinder, "Proc_TrafficLight");
        pole.transform.localPosition = new Vector3(x, 3f, zPos);
        pole.transform.localScale    = new Vector3(0.3f, 4f, 0.3f);
        // Savaş hasarı – hafif eğim
        pole.transform.localRotation = Quaternion.Euler(
            Random.Range(-10f, 10f),
            0f,
            side * Random.Range(-5f, 15f)
        );
        pole.GetComponent<MeshRenderer>().sharedMaterial = _baseMat;

        // Işık kutusu (direğin çocuğu, localPos ile)
        GameObject lightBox = CreatePrimInParent(PrimitiveType.Cube, "Proc_LightBox", pole.transform, worldPos: false);
        lightBox.transform.localPosition = new Vector3(side * -1.5f, 0.45f, 0f);
        lightBox.transform.localScale    = new Vector3(2.5f, 0.15f, 0.8f);
        lightBox.GetComponent<MeshRenderer>().sharedMaterial = _baseMat;

        // Lamba (kutunun çocuğu)
        GameObject lamp = CreatePrimInParent(PrimitiveType.Sphere, "Proc_Lamp", lightBox.transform, worldPos: false);
        lamp.transform.localPosition = new Vector3(side * 0.3f, -0.5f, 0f);
        lamp.transform.localScale    = new Vector3(0.3f, 1.2f, 0.6f);

        // %50 ihtimalle lamba sönük
        Material lampMat = new Material(_emissionMat);
        if (Random.value < 0.5f)
            lampMat.SetColor("_EmissionColor", glowAccentColor * 0.2f);
        lamp.GetComponent<MeshRenderer>().sharedMaterial = lampMat;
    }

    // ─────────────────────────────────────────────
    //  4 ─ Prosedürel Ev Silüetleri
    //      Her ev: gövde + üçgen çatı + pencereler
    //              + kapı + baca + isteğe bağlı enkaz
    // ─────────────────────────────────────────────
    private void SpawnRuinedCityscape()
    {
        // RoadsideCitySpawner'a delege et — pivot-bağımsız güvenli yerleştirme
        RoadsideCitySpawner spawner = GetComponent<RoadsideCitySpawner>();
        if (spawner == null) spawner = gameObject.AddComponent<RoadsideCitySpawner>();

        spawner.roadHalfWidth    = sideOffset;
        spawner.buildingPrefabs  = buildingPrefabs;
        spawner.buildingsPerTile = 15;
        spawner.scaleMin         = 3.5f;
        spawner.scaleMax         = 6.0f;
        spawner.tileLength       = tileLength;
        spawner.useRuinedLook    = useRuinedLook; // Ruined look ayarını aktar
        spawner.Rebuild();
    }
    private void ApplyCurvedShader(GameObject go)
    {
        if (_baseMat == null) BuildMaterials();
        MaterialPropertyBlock curvePropBlock = new MaterialPropertyBlock();

        var renderers = go.GetComponentsInChildren<Renderer>();
        foreach (var r in renderers)
        {
            // Gölge Ayarları
            r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
            r.receiveShadows    = true;

            Material[] mats = r.sharedMaterials;
            bool changed = false;
            for (int i = 0; i < mats.Length; i++)
            {
                if (mats[i] == null) continue;
                if (!mats[i].shader.name.Contains("CurvedWorld") && !mats[i].shader.name.Contains("GazaRuined"))
                {
                    // Shader değişimi mecburen sızıntı riski taşır ama sharedMaterials kullanarak
                    // aynı prefab materyalini kullanan tüm renderlar için tek kopya oluşturuyoruz.
                    mats[i] = new Material(mats[i]);
                    mats[i].shader = _baseMat != null ? _baseMat.shader : Shader.Find("Universal Render Pipeline/Lit");
                    changed = true;
                }
            }
            if (changed) r.sharedMaterials = mats;

            // Parametreleri sızıntısız PropertyBlock ile set ediyoruz
            r.GetPropertyBlock(curvePropBlock);
            curvePropBlock.SetFloat("_Curvature",     0.002f);
            curvePropBlock.SetFloat("_CurvatureH",   -0.0015f);
            curvePropBlock.SetFloat("_HorizonOffset", 10.0f);

            if (_baseMat != null && _baseMat.shader.name.Contains("GazaRuined"))
            {
                curvePropBlock.SetFloat("_DirtIntensity", 0.4f);
                curvePropBlock.SetFloat("_GrimeIntensity", 0.3f);
            }
            r.SetPropertyBlock(curvePropBlock);
        }
    }

    private void OnDestroy()
    {
        // Belleği temizle
        if (_baseMat != null) { if (Application.isPlaying) Destroy(_baseMat); else DestroyImmediate(_baseMat); }
        if (_emissionMat != null) { if (Application.isPlaying) Destroy(_emissionMat); else DestroyImmediate(_emissionMat); }
    }

    private void DestroyAllColliders(GameObject go)
    {
        var colliders = go.GetComponentsInChildren<Collider>();
        foreach (var col in colliders)
        {
            if (Application.isPlaying) Destroy(col);
            else                       DestroyImmediate(col);
        }
    }

    // ─────────────────────────────────────────────
    //  5 ─ Temel sütun
    // ─────────────────────────────────────────────
    private void SpawnFoundation()
    {
        GameObject pillar = CreatePrim(PrimitiveType.Cube, "Proc_FoundationPillar");
        // TOP yüzeyinin yoldan (Y=0) biraz aşağıda olması için Y pozisyonunu hafifçe düşürüyoruz
        // Aksi halde Z-fighting (stippled look) oluşuyor.
        pillar.transform.localPosition = new Vector3(0f, -50.1f, 0f);
        pillar.transform.localScale    = new Vector3(sideOffset * 2.3f, 100f, tileLength * 1.02f);
        pillar.GetComponent<MeshRenderer>().sharedMaterial = _baseMat;
    }

    // ─────────────────────────────────────────────
    //  5b ─ 'V' destek kirişleri
    // ─────────────────────────────────────────────
    private void SpawnSupportBeams()
    {
        for (int i = 0; i < 2; i++)
        {
            float side = (i == 0) ? -1f : 1f;
            GameObject beam = CreatePrim(PrimitiveType.Cube, "Proc_SupportBeam");
            beam.transform.localPosition = new Vector3(side * sideOffset * 2.5f, -10f, 0f);
            beam.transform.localScale    = new Vector3(sideOffset * 3f, 1f, 10f);
            beam.transform.localRotation = Quaternion.Euler(0f, 0f, side * 35f);
            beam.GetComponent<MeshRenderer>().sharedMaterial = _baseMat;
        }
    }

    // ─────────────────────────────────────────────
    //  6 ─ Abyssal zemin (her 5 karoda bir)
    // ─────────────────────────────────────────────
    private void SpawnAbyssalGround()
    {
        // Her karoya ekleyelim ama daha büyük ve daha aşağıda
        GameObject ground = CreatePrim(PrimitiveType.Plane, "Proc_AbyssalFloor");
        ground.transform.localPosition = new Vector3(0f, -120f, 0f);
        ground.transform.localScale    = new Vector3(250f, 1f, 250f); // Dev boyut

        Material groundMat = new Material(_baseMat) { color = skyCityColor * 0.3f };
        ground.GetComponent<MeshRenderer>().sharedMaterial = groundMat;

        // Aşağıda seyrek ama parlak neon ışıklar (Optimizasyon için sadece %30 ihtimalle)
        if (Random.value < 0.3f)
        {
            for (int j = 0; j < 3; j++)
            {
                GameObject light = CreatePrimInParent(PrimitiveType.Cube, "Proc_AbyssLight", ground.transform, worldPos: false);
                light.transform.localPosition = new Vector3(
                    Random.Range(-5f, 5f),
                    0.2f,
                    Random.Range(-5f, 5f)
                );
                light.transform.localScale = Vector3.one * 0.15f;
                light.GetComponent<MeshRenderer>().sharedMaterial = _emissionMat;
            }
        }
    }

    // ─────────────────────────────────────────────
    //  7 ─ Büyük kemer (her 200 birimde bir)
    // ─────────────────────────────────────────────
    private void SpawnArch()
    {
        if (Mathf.Abs(transform.position.z % 200f) >= 1f) return;

        GameObject arch = CreatePrim(PrimitiveType.Cube, "Proc_LargeArch");
        arch.transform.localPosition = new Vector3(0f, 25f, 0f);
        arch.transform.localScale    = new Vector3(sideOffset * 8f, 3f, 10f);
        arch.GetComponent<MeshRenderer>().sharedMaterial = _baseMat;
    }

    // ─────────────────────────────────────────────
    //  İç yardımcılar: primitive oluştur + collider kaldır
    // ─────────────────────────────────────────────
    private GameObject CreatePrim(PrimitiveType type, string objName)
    {
        GameObject go = GameObject.CreatePrimitive(type);
        go.name = objName;
        go.transform.SetParent(this.transform, false);
        
        Renderer r = go.GetComponent<Renderer>();
        if (r != null) { r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On; r.receiveShadows = true; }
        
        DestroyCollider(go);
        return go;
    }

    private GameObject CreatePrimInParent(PrimitiveType type, string objName, Transform parent, bool worldPos)
    {
        GameObject go = GameObject.CreatePrimitive(type);
        go.name = objName;
        go.transform.SetParent(parent, worldPos);
        
        Renderer r = go.GetComponent<Renderer>();
        if (r != null) { r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On; r.receiveShadows = true; }

        DestroyCollider(go);
        return go;
    }
}