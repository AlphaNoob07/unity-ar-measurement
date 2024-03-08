using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class Measurement : MonoBehaviour
{
    [SerializeField]
    private GameObject m_pointPrefab;
    public GameObject pointPrefab
    {
        get { return m_pointPrefab; }
        private set { m_pointPrefab = value; }
    }

    [SerializeField]
    private GameObject m_linePrefab;
    public GameObject linePrefab
    {
        get { return m_linePrefab; }
        private set { m_linePrefab = value; }
    }

    [SerializeField]
    private Button buttonPlacePoint;

    [SerializeField]
    private Button buttonClear;

    [SerializeField]
    private Button buttonUndo;

    public event Action onPlacedObject;

    private List<GameObject> spawnedPoints = new List<GameObject>();
    private List<GameObject> spawnedLines = new List<GameObject>();

    [SerializeField]
    public GameObject movePointHint;
    public GameObject PlacePointHint;

    // Wall Mechanism
    [SerializeField]
    private float wallHeight = 2.0f; // Height of the wall

    public List<GameObject> wallPoints = new List<GameObject>();
    [SerializeField] Material wallMat;


    public RenderTexture renderTexture;
    public Button captureButton;

    private void Awake()
    {
        buttonPlacePoint.onClick.AddListener(ClickPlacePoint);
        buttonClear.onClick.AddListener(ClearPointsAndLines);
        buttonUndo.onClick.AddListener(ClickUndo);
        captureButton.onClick.AddListener(Capture);
        onPlacedObject += GenerateWall;
    }

    public void ClickPlacePoint()
    {

        if (PlacePointHint.activeInHierarchy)
        {
            PlacePointHint.SetActive(true);
        }

        Pose hitPose = PlaceARFocus.s_Hits[0].pose;

        GameObject spawnedPoint = Instantiate(pointPrefab, hitPose.position, hitPose.rotation);
        spawnedPoints.Add(spawnedPoint);
        wallPoints.Add(spawnedPoint);
        if (spawnedPoints.Count > 1)
        {
            Vector3 from = spawnedPoints[spawnedPoints.Count - 2].transform.position;
            Vector3 to = spawnedPoints[spawnedPoints.Count - 1].transform.position;

            GameObject line = Instantiate(linePrefab);
            line.GetComponent<LineRenderer>().SetPosition(0, from);
            line.GetComponent<LineRenderer>().SetPosition(1, to);
            spawnedLines.Add(line);

            Vector3 centerPoint = (to + from) / 2f;
            line.transform.GetChild(0).localPosition = centerPoint;

            Vector3 direction = to - from;
            float distance = Vector3.Magnitude(direction);

            float distanceInMeters = distance / 100f;
            line.GetComponentInChildren<Text>().text = Mathf.Round(distanceInMeters * 100f) / 100f + " m";
            // line.GetComponentInChildren<Text>().text = Mathf.Round(distance * 100f).ToString() + " cm";

            Debug.Log(
                "From A: " + from + " to B: " + to + "\n" +
                "Center of Line: " + centerPoint + "\n" +
                "Direction: " + direction + "\n");



        }

        if (onPlacedObject != null)
        {
            onPlacedObject();
        }
    }

    private void ClickUndo()
    {
        if(spawnedPoints.Count > 0)
        {
            Destroy(spawnedPoints[spawnedPoints.Count - 1]);
            spawnedPoints.RemoveAt(spawnedPoints.Count - 1);
        }

        if (spawnedLines.Count > 0)
        {
            Destroy(spawnedLines[spawnedLines.Count - 1]);
            spawnedLines.RemoveAt(spawnedLines.Count - 1);
        }
    }



    private void GenerateWall()
    {
        // Ensure we have at least two points to form a wall
        if (wallPoints.Count < 1)
            return;

        // Create a list to store vertices of the wall mesh
        List<Vector3> vertices = new List<Vector3>();

        // Iterate through the wall points to generate the wall
        for (int i = 0; i < wallPoints.Count - 1; i++)
        {
            Vector3 startPoint = wallPoints[i].transform.position;
            Vector3 endPoint = wallPoints[i + 1].transform.position;

            // Calculate wall width and center position
            Vector3 direction = endPoint - startPoint;
            float distance = Vector3.Magnitude(direction);
            Vector3 centerPoint = (startPoint + endPoint) / 2f;

            // Add vertices for the wall segment
            vertices.Add(startPoint);
            vertices.Add(endPoint);
            vertices.Add(new Vector3(startPoint.x, wallHeight, startPoint.z)); // Top left
            vertices.Add(new Vector3(endPoint.x, wallHeight, endPoint.z)); // Top right

            // Create triangles to form the wall segment
            int lastIndex = vertices.Count - 1;
            int secondLastIndex = lastIndex - 1;
            int thirdLastIndex = lastIndex - 2;
            int fourthLastIndex = lastIndex - 3;

            // Front face triangles
            int[] frontFaceTriangles = { secondLastIndex, thirdLastIndex, fourthLastIndex, secondLastIndex, fourthLastIndex, lastIndex };
            // Top face triangles
            int[] topFaceTriangles = { thirdLastIndex, secondLastIndex, lastIndex, thirdLastIndex, lastIndex, fourthLastIndex };

            // Create the wall segment mesh
            GameObject wallSegment = new GameObject("WallSegment");
            MeshFilter meshFilter = wallSegment.AddComponent<MeshFilter>();
            MeshRenderer meshRenderer = wallSegment.AddComponent<MeshRenderer>();
            Mesh mesh = new Mesh();

            // Assign vertices and triangles to the mesh
            mesh.vertices = vertices.ToArray();
            mesh.triangles = frontFaceTriangles.Concat(topFaceTriangles).ToArray();

            // Recalculate normals for proper lighting
            mesh.RecalculateNormals();

            // Assign the mesh to the mesh filter
            meshFilter.mesh = mesh;

            // Clear vertices for the next wall segment
            vertices.Clear();

            spawnedLines.Add(wallSegment);
            meshRenderer.material = wallMat;
        }

        // Clear the wall points list for future wall generation
       // wallPoints.Clear();
    }


    public void Capture()
    {
        // Render the scene into the render texture
        RenderTexture.active = renderTexture;
        Camera.main.Render();

        // Render the wall points as lines
        foreach (var point in wallPoints)
        {
            Vector3 startPoint = point.transform.position;
            foreach (var otherPoint in wallPoints)
            {
                if (otherPoint != point)
                {
                    Vector3 endPoint = otherPoint.transform.position;
                    // Draw a line between the two points
                    Debug.DrawLine(startPoint, endPoint, Color.red);
                }
            }
        }

        // Capture the rendered image
        Texture2D tex = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.RGB24, false);
        tex.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
        tex.Apply();

        // Save the texture as an image file
        byte[] bytes = tex.EncodeToPNG();
        System.IO.File.WriteAllBytes("LineImage.png", bytes);
    }


    private void ClearPointsAndLines()
    {
        foreach(var point in spawnedPoints)
        {
            Destroy(point);
        }
        spawnedPoints.Clear();

        foreach (var line in spawnedLines)
        {
            Destroy(line);
        }
        spawnedLines.Clear();
    }

    private void OnDisable()
    {
        onPlacedObject -= GenerateWall;
    }






}
