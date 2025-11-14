using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class Container : MonoBehaviour {
    public GameObject metaBallPrefab;
    public float edgeSize;
    List<GameObject> metaBalls = new List<GameObject>();

    public float safeZone;
    public float resolution;
    public float threshold;
    public ComputeShader computeShader;
    public bool calculateNormals;

    private CubeGrid grid;
    public int gridSize;

    public GameObject drawZone;
    public Material material;
    public void Start() {
        this.grid = new CubeGrid(this, this.computeShader);
        Render();
    }
    

    public void Update() {

        
    }
    public void InstantiateMetaBalls(List<Vector3> globalPositions, Vector3 lowestPosition) 
    {

        transform.position = new Vector3( 
            lowestPosition.x + transform.localScale.x / 2 - edgeSize, 
            lowestPosition.y + transform.localScale.y / 2 - edgeSize, 
            lowestPosition.z + transform.localScale.z / 2 - edgeSize);
        foreach (Vector3 globalPos in globalPositions)
        {
            GameObject newMetaBall = Instantiate(metaBallPrefab, this.transform);
            //newMetaBall.transform.localPosition = globalPos - transform.position;
            newMetaBall.transform.position = globalPos;
            newMetaBall.transform.localScale = new Vector3(.06f,.06f,.06f);
            metaBalls.Add(newMetaBall);

        }
        StartCoroutine(Render());

    }
    void ClearMetaBalls()
    {
        foreach (GameObject metaBall in metaBalls)
        {
            Destroy(metaBall);
        }
        metaBalls.Clear();
    }
    
    public IEnumerator Render()
    {
        yield return null;
        this.grid.evaluateAll(this.GetComponentsInChildren<MetaBall>());
        Mesh mesh = this.GetComponent<MeshFilter>().mesh;
        mesh.Clear();
        mesh.vertices = this.grid.vertices.ToArray();
        mesh.triangles = this.grid.getTriangles();
        //mesh.RecalculateNormals();
        RecalculateSmoothNormals(mesh);
        if (this.calculateNormals)
        {
          
        }
        GameObject drawZoneNewObj = new GameObject();
        drawZoneNewObj.transform.localScale = transform.localScale;
        //First set scale then parent!
        drawZoneNewObj.transform.SetParent(drawZone.transform);
        Mesh independentMesh = Instantiate(mesh); //Mesh needs to be independent
        drawZoneNewObj.AddComponent<MeshFilter>().mesh = independentMesh;
        drawZoneNewObj.AddComponent<MeshRenderer>().material = material;
        drawZoneNewObj.transform.position = transform.position;
        mesh.Clear();

        ClearMetaBalls();

    }
    static void RecalculateNormalsSeamless(Mesh mesh) //NOT in use
    {
        var trianglesOriginal = mesh.triangles;
        var triangles = trianglesOriginal.ToArray();

        var vertices = mesh.vertices;

        var mergeIndices = new Dictionary<int, int>();

        for (int i = 0; i < vertices.Length; i++)
        {
            var vertexHash = vertices[i].GetHashCode();

            if (mergeIndices.TryGetValue(vertexHash, out var index))
            {
                for (int j = 0; j < triangles.Length; j++)
                    if (triangles[j] == i)
                        triangles[j] = index;
            }
            else
                mergeIndices.Add(vertexHash, i);
        }

        mesh.triangles = triangles;

        var normals = new Vector3[vertices.Length];

        mesh.RecalculateNormals();
        var newNormals = mesh.normals;

        for (int i = 0; i < vertices.Length; i++)
            if (mergeIndices.TryGetValue(vertices[i].GetHashCode(), out var index))
                normals[i] = newNormals[index];

        mesh.triangles = trianglesOriginal;
        mesh.normals = normals;
    }

    public static void RecalculateSmoothNormals(Mesh mesh, float mergeEpsilon = 0.000001f, float smoothingAngle = 180f, int laplacianIterations = 0)
    {
        Vector3[] vertices = mesh.vertices;
        int[] triangles = mesh.triangles;

        // Step 1: Merge nearby vertices
        Dictionary<int, int> mergeIndices = new Dictionary<int, int>();
        List<Vector3> mergedVertices = new List<Vector3>();
        int[] remap = new int[vertices.Length];

        for (int i = 0; i < vertices.Length; i++)
        {
            bool found = false;
            for (int j = 0; j < mergedVertices.Count; j++)
            {
                if (Vector3.Distance(vertices[i], mergedVertices[j]) <= mergeEpsilon)
                {
                    remap[i] = j;
                    found = true;
                    break;
                }
            }
            if (!found)
            {
                mergedVertices.Add(vertices[i]);
                remap[i] = mergedVertices.Count - 1;
            }
        }

        // Step 2: Remap triangles
        int[] newTriangles = new int[triangles.Length];
        for (int i = 0; i < triangles.Length; i++)
            newTriangles[i] = remap[triangles[i]];

        Mesh tempMesh = new Mesh();
        tempMesh.vertices = mergedVertices.ToArray();
        tempMesh.triangles = newTriangles;

        // Step 3: Recalculate normals
        tempMesh.RecalculateNormals();

        Vector3[] smoothNormals = new Vector3[vertices.Length];
        for (int i = 0; i < vertices.Length; i++)
            smoothNormals[i] = tempMesh.normals[remap[i]];

        // Step 4: Optional Laplacian smoothing
        if (laplacianIterations > 0)
        {
            List<int>[] vertexNeighbors = new List<int>[vertices.Length];
            for (int i = 0; i < vertices.Length; i++)
                vertexNeighbors[i] = new List<int>();

            for (int i = 0; i < triangles.Length; i += 3)
            {
                int a = triangles[i];
                int b = triangles[i + 1];
                int c = triangles[i + 2];
                if (!vertexNeighbors[a].Contains(b)) vertexNeighbors[a].Add(b);
                if (!vertexNeighbors[a].Contains(c)) vertexNeighbors[a].Add(c);
                if (!vertexNeighbors[b].Contains(a)) vertexNeighbors[b].Add(a);
                if (!vertexNeighbors[b].Contains(c)) vertexNeighbors[b].Add(c);
                if (!vertexNeighbors[c].Contains(a)) vertexNeighbors[c].Add(a);
                if (!vertexNeighbors[c].Contains(b)) vertexNeighbors[c].Add(b);
            }

            for (int iter = 0; iter < laplacianIterations; iter++)
            {
                Vector3[] temp = new Vector3[smoothNormals.Length];
                for (int i = 0; i < smoothNormals.Length; i++)
                {
                    Vector3 sum = smoothNormals[i];
                    foreach (int n in vertexNeighbors[i])
                        sum += smoothNormals[n];
                    temp[i] = sum.normalized;
                }
                smoothNormals = temp;
            }
        }

        // Step 5: Apply normals to original mesh
        mesh.normals = smoothNormals;
    }
}