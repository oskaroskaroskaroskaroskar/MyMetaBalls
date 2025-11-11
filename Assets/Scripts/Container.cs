using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class Container : MonoBehaviour {
    public GameObject containerGameObj;
    public float safeZone;
    public float resolution;
    public float threshold;
    public ComputeShader computeShader;
    public bool calculateNormals;

    private CubeGrid grid;
    float timer = 0f;
    float renderInterval = .2f;
    public int gridSize;

    public void Start() {
        this.grid = new CubeGrid(this, this.computeShader);
    }
    

    public void Update() {

        if (timer >= renderInterval)
        {
            Render();
            timer = 0f;
        }
        timer += Time.deltaTime;
    }
    
    public void Render()
    {
        this.grid.evaluateAll(this.GetComponentsInChildren<MetaBall>());
        Mesh mesh = this.GetComponent<MeshFilter>().mesh;
        mesh.Clear();
        mesh.vertices = this.grid.vertices.ToArray();
        mesh.triangles = this.grid.getTriangles();

        if (this.calculateNormals)
        {
            mesh.RecalculateNormals();
        }
    }
}