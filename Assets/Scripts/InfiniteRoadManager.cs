using UnityEngine;
using System.Collections.Generic;

public class InfiniteRoadManager : MonoBehaviour
{
    public GameObject roadPrefab; // Just a cube for now if not set
    public Material roadMat, bldMat;
    public Transform playerCamera;
    
    private List<GameObject> activeSegments = new List<GameObject>();
    private float segmentLength = 50f;
    private int segmentCount = 10;
    private float spawnZ = 0f;

    void Start()
    {
        if (playerCamera == null) playerCamera = Camera.main.transform;

        // Visual Environment Setup
        SetupEnvironment();

        // Initial Spawn
        for (int i = 0; i < segmentCount; i++)
        {
            SpawnSegment();
        }
    }

    void Update()
    {
        // Check if we need to move the oldest segment to the front
        if (playerCamera.position.z - 50f > (spawnZ - segmentCount * segmentLength))
        {
            MoveSegmentToFront();
        }

        // Camera simple movement for preview (Slowly moving)
        // playerCamera.Translate(Vector3.forward * Time.deltaTime * 10f, Space.World);
    }

    void SetupEnvironment()
    {
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.ExponentialSquared;
        RenderSettings.fogColor = new Color(0.05f, 0.05f, 0.08f); // Deep Cinematic Night
        RenderSettings.fogDensity = 0.035f;
        RenderSettings.ambientSkyColor = new Color(0.1f, 0.1f, 0.15f);
        
        // Darker skybox if exists
        Camera.main.backgroundColor = RenderSettings.fogColor;
        Camera.main.clearFlags = CameraClearFlags.SolidColor;
    }

    void SpawnSegment()
    {
        GameObject segment = new GameObject("Segment_" + spawnZ);
        segment.transform.position = new Vector3(0, 0, spawnZ);
        segment.transform.SetParent(transform);

        // Road
        GameObject road = GameObject.CreatePrimitive(PrimitiveType.Cube);
        road.name = "Road";
        road.transform.SetParent(segment.transform, false);
        road.transform.localScale = new Vector3(12f, 0.1f, segmentLength);
        if (roadMat) road.GetComponent<Renderer>().material = roadMat;

        // Left Sidewalk
        GameObject swL = GameObject.CreatePrimitive(PrimitiveType.Cube);
        swL.transform.SetParent(segment.transform, false);
        swL.transform.localPosition = new Vector3(-8.5f, 0.15f, 0);
        swL.transform.localScale = new Vector3(5f, 0.3f, segmentLength);
        
        // Right Sidewalk
        GameObject swR = GameObject.CreatePrimitive(PrimitiveType.Cube);
        swR.transform.SetParent(segment.transform, false);
        swR.transform.localPosition = new Vector3(8.5f, 0.15f, 0);
        swR.transform.localScale = new Vector3(5f, 0.3f, segmentLength);

        // Buildings
        CreateBuilding(segment.transform, -15f, Random.Range(20, 60));
        CreateBuilding(segment.transform, 15f, Random.Range(20, 60));

        activeSegments.Add(segment);
        spawnZ += segmentLength;
    }

    void CreateBuilding(Transform parent, float x, float height)
    {
        GameObject b = GameObject.CreatePrimitive(PrimitiveType.Cube);
        b.transform.SetParent(parent, false);
        b.transform.localPosition = new Vector3(x, height / 2f, 0);
        b.transform.localScale = new Vector3(10f, height, 10f);
        if (bldMat) b.GetComponent<Renderer>().material = bldMat;

        // Window glow point lights
        if (Random.value > 0.4f)
        {
            GameObject lightObj = new GameObject("Windowlight");
            lightObj.transform.SetParent(b.transform, false);
            lightObj.transform.localPosition = new Vector3(x > 0 ? -0.6f : 0.6f, 0, 0);
            Light l = lightObj.AddComponent<Light>();
            l.type = LightType.Point;
            l.color = new Color(1f, 0.9f, 0.5f);
            l.range = 25f;
            l.intensity = 3f;
        }
    }

    void MoveSegmentToFront()
    {
        GameObject firstSegment = activeSegments[0];
        activeSegments.RemoveAt(0);

        firstSegment.transform.position = new Vector3(0, 0, spawnZ);
        firstSegment.name = "Segment_" + spawnZ;
        
        // Dynamic building height update for infinite feel
        foreach(Transform child in firstSegment.transform)
        {
            if (child.name.Contains("Cube")) // Building/Road/SW
            {
               // Simplification: just move it. Real systems would randomize height here.
            }
        }

        activeSegments.Add(firstSegment);
        spawnZ += segmentLength;
    }
}
