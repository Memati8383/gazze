using UnityEngine;
using UnityEditor;
using System.IO;

public class RoadGenerator : EditorWindow
{
    [MenuItem("Gazze / Yeni Mukemmel Yolu Tasarla")]
    public static void GenerateRoad()
    {
        // 1. Doku (Texture) Oluşturma
        int width = 1024;
        int height = 2048;
        Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
        Color asphaltColor = new Color(0.18f, 0.18f, 0.20f);
        Color lineWhite = new Color(0.85f, 0.85f, 0.85f, 0.9f);
        Color lineYellow = new Color(1.0f, 0.75f, 0.0f, 0.9f);

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float u = (float)x / width;
                float v = (float)y / height;
                Color col = asphaltColor;

                // Asfalt Dokusu (Noise)
                float noise = Random.Range(-0.02f, 0.02f);
                col += new Color(noise, noise, noise);

                // Sol ve Sağ Şerit Çizgileri
                // Sol sarı sürekli çizgi
                if (u > 0.05f && u < 0.07f) col = lineYellow;
                // Sağ beyaz sürekli çizgi
                if (u > 0.93f && u < 0.95f) col = lineWhite;

                // Orta Şerit Kesik Çizgisi (2 şerit)
                float dashLength = 0.15f; 
                float emptyLength = 0.15f;
                float totalDash = dashLength + emptyLength;
                
                bool isDash = (v % totalDash) < dashLength;
                
                if (isDash)
                {
                    if (u > 0.49f && u < 0.51f) col = lineWhite;
                }

                tex.SetPixel(x, y, col);
            }
        }
        tex.Apply();

        // 2. Texture Kaydet
        byte[] bytes = tex.EncodeToPNG();
        if(!Directory.Exists(Application.dataPath + "/Textures")) Directory.CreateDirectory(Application.dataPath + "/Textures");
        File.WriteAllBytes(Application.dataPath + "/Textures/NewRoadTex.png", bytes);
        AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);

        // 3. Materyal Oluştur
        Texture2D loadedTex = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Textures/NewRoadTex.png");
        
        string texPath = AssetDatabase.GetAssetPath(loadedTex);
        TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(texPath);
        if(importer != null){
            importer.wrapMode = TextureWrapMode.Repeat;
            importer.filterMode = FilterMode.Trilinear;
            importer.anisoLevel = 16;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.mipmapEnabled = true;
            importer.SaveAndReimport();
        }

        Material mat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/NewRoadMaterial.mat");
        if (mat == null)
        {
            mat = new Material(Shader.Find("Custom/CurvedWorld_URP"));
            if(!Directory.Exists(Application.dataPath + "/Materials")) Directory.CreateDirectory(Application.dataPath + "/Materials");
            AssetDatabase.CreateAsset(mat, "Assets/Materials/NewRoadMaterial.mat");
        }
        else
        {
            mat.shader = Shader.Find("Custom/CurvedWorld_URP");
        }

        mat.SetTexture("_BaseMap", loadedTex);
        mat.SetColor("_BaseColor", Color.white);
        mat.SetFloat("_Curvature", 0.002f);
        mat.SetFloat("_CurvatureH", -0.0015f);
        mat.SetFloat("_HorizonOffset", 10f);
        EditorUtility.SetDirty(mat);

        // 4. Mesh Oluştur (Eğriliğin pürüzsüz olması için alt bölmelere ayrılmış mesh)
        float hw = 4.5f; // Toplam Genişlik = 9 birim (2 şerit için ideal)
        float hz = 25f; // Toplam Uzunluk = 50 birim
        int zSegments = 25; // Eğimin kırık değil, yumuşak görünmesi için parça sayısı
        
        Vector3[] vertices = new Vector3[(zSegments + 1) * 2];
        Vector2[] uvs = new Vector2[(zSegments + 1) * 2];
        int[] tris = new int[zSegments * 6];

        for (int i = 0; i <= zSegments; i++)
        {
            float zPos = Mathf.Lerp(-hz, hz, (float)i / zSegments);
            float v = Mathf.Lerp(0, 5, (float)i / zSegments); // 5 kat tekrar
            
            // Sol ve Sağ noktalar
            vertices[i * 2] = new Vector3(-hw, 0, zPos);
            vertices[i * 2 + 1] = new Vector3(hw, 0, zPos);
            
            uvs[i * 2] = new Vector2(0, v);
            uvs[i * 2 + 1] = new Vector2(1, v);
            
            if (i < zSegments)
            {
                int t = i * 6;
                int v0 = i * 2;
                int v1 = i * 2 + 1;
                int v2 = (i + 1) * 2;
                int v3 = (i + 1) * 2 + 1;
                
                // İlk üçgen
                tris[t] = v0;
                tris[t + 1] = v2;
                tris[t + 2] = v1;
                // İkinci üçgen
                tris[t + 3] = v2;
                tris[t + 4] = v3;
                tris[t + 5] = v1;
            }
        }

        if(!Directory.Exists(Application.dataPath + "/Models")) Directory.CreateDirectory(Application.dataPath + "/Models");
        
        string meshPath = "Assets/Models/NewRoadMesh.asset";
        Mesh mesh = AssetDatabase.LoadAssetAtPath<Mesh>(meshPath);
        if (mesh != null)
        {
            mesh.Clear();
            mesh.vertices = vertices;
            mesh.triangles = tris;
            mesh.uv = uvs;
            mesh.RecalculateNormals();
            EditorUtility.SetDirty(mesh);
        }
        else
        {
            mesh = new Mesh();
            mesh.name = "CustomRoadMesh";
            mesh.vertices = vertices;
            mesh.triangles = tris;
            mesh.uv = uvs;
            mesh.RecalculateNormals();
            AssetDatabase.CreateAsset(mesh, meshPath);
        }
        AssetDatabase.SaveAssets();

        // 5. Prefab Oluştur
        GameObject roadObj = new GameObject("NewCustomRoadTile");
        roadObj.tag = "Untagged";
        
        MeshFilter mf = roadObj.AddComponent<MeshFilter>();
        mf.sharedMesh = mesh;
        
        MeshRenderer mr = roadObj.AddComponent<MeshRenderer>();
        mr.sharedMaterial = mat;
        
        BoxCollider bc = roadObj.AddComponent<BoxCollider>();
        // Adjust collider thickness since a flat quad has 0 Z/Y height and could cause physics issues.
        bc.size = new Vector3(hw * 2, 0.5f, hz * 2);

        if(!Directory.Exists(Application.dataPath + "/Prefabs")) Directory.CreateDirectory(Application.dataPath + "/Prefabs");
        string prefabPath = "Assets/Prefabs/NewCustomRoadTile.prefab";
        PrefabUtility.SaveAsPrefabAsset(roadObj, prefabPath);
        DestroyImmediate(roadObj);

        // 6. Projedeki RoadManager'ı Otomatik Güncelle!
        RoadManager rm = GameObject.FindFirstObjectByType<RoadManager>();
        if(rm != null)
        {
            rm.roadPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            rm.tileLength = hz * 2; // 50f
            EditorUtility.SetDirty(rm);
        }

        EditorUtility.DisplayDialog("Yol Tasarımı Tamamlandı", "Sıfırdan mükemmel bir asfalt, şerit çizgileri, materyal ve 3D zemin modeli oluşturuldu!\n\nAyrıca RoadManager sistemine otomatik olarak entegre edildi.", "Süper!");
    }
}
