using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class Symbiote : MonoBehaviour
{
    [SerializeField]
    protected Material mat;
    [SerializeField]
    protected int horizontalVertexNumber = 1; // From negative number to postive number of vertex range on horizontal.
    [SerializeField]
    protected int verticalVertexNumber = 1; // From negative number to postive number of vertex range on vertical.
    [SerializeField]
    protected bool freezePos = false;
    [SerializeField]
    protected bool onGizmo = false;
    [SerializeField]
    protected float colliderSize = 0.2f;
    [SerializeField]
    protected float spring = 10;
    [SerializeField]
    protected bool autoAnchor = false;
    [SerializeField]
    protected bool enableCollision = true;
    [SerializeField]
    protected float damper = 2;
    [SerializeField]
    protected bool isUpdateWhenOffscreen = false;
    
    public float x {get { return 1;}}
    public float y {get { return 1;}}
    public float z {get { return 1;}}

    // Each cell includes: key is vertex index and value is bone index.
    protected Dictionary<int, int> cells = new Dictionary<int, int>{};
    // Each layer includes: key is cell layer index and value is an array of vertex indexes.
    protected Dictionary<int, int[]> cellLayers = new Dictionary<int, int[]>();
    protected SkinnedMeshRenderer rend;
    protected Mesh mesh;
    protected SymBone symBone;
    protected SymVertex symVertex;
    protected SymDebug symDebug;
    protected SymJoint symJoint;

    // Start is called before the first frame update
    protected virtual void Start()
    {
        gameObject.AddComponent<SkinnedMeshRenderer>();
        rend = gameObject.GetComponent<SkinnedMeshRenderer>();

        mesh = new Mesh();
        GameObject[] bones = new GameObject[]{};

        symBone = new SymBone(this, bones, rend, mesh);
        symVertex = new SymVertex(mesh);
        symDebug = new SymDebug();
        symJoint = new SymJoint();

        Create();

        // Recalculate bounds of Renderer every frame.
        rend.updateWhenOffscreen = isUpdateWhenOffscreen;

        mesh.RecalculateNormals();
        
        // Add Bones and Mesh into Renderer.
        rend.sharedMesh = mesh;
        rend.material = mat;

        if (freezePos) {
            Rigidbody rigidbody = gameObject.GetComponent<Rigidbody>();
            rigidbody.constraints = RigidbodyConstraints.FreezePosition;

        }
    }

    // Update is called once per frame
    void Update()
    {
        // UpdateCellPositions(); // TODO: need to investigate.
    }

    void OnDrawGizmos()
    {
        if (! mesh || ! onGizmo) {
            return;
        }

        // symDebug.GizmoMeshVertices(mesh, transform.position);
        // symDebug.GizmoRendBones(rend);
        symDebug.GizmoVerticesAndBones(mesh, transform.position, rend);
    }

    void FixedUpdate()
    {
    }

    // Main method to create object.
    protected virtual void Create()
    {
    }

    protected int GenerateCell(Vector3 position, string name)
    {
        Vector3 vertex = symVertex.CreateVertex(position);

        // Add first bone weight, others are auto-added when create new vertex.
        if (mesh.boneWeights.Length <= 0) {
            mesh.boneWeights = new BoneWeight[] {
                new BoneWeight{weight0 = 1, boneIndex0 = 0}
            };
        }

        int boneIndex = symBone.FindOrCreateBoneIndex(name, vertex, colliderSize, freezePos);
        symBone.SetBoneToWeight(boneIndex, mesh.boneWeights.Length - 1);

        // Add cell.
        int vertexIndex = symVertex.FindVertexIndexByPosition(position);
        cells.Add(vertexIndex, boneIndex);

        // Return cell key.
        return vertexIndex;
    }

    protected virtual int GenerateSideCell(Vector3 position, string name)
    {
        return GenerateCell(position, name);
    }

    protected virtual int GenerateCrossCell(Vector3 position, string name)
    {
        return GenerateCell(position, name);
    }

    protected void UpdateCellPositions()
    {
        foreach (var cell in cells) {
            Vector3[] cloneVertices = mesh.vertices;
            int vertexIndex = cell.Key;
            int boneIndex = cell.Value;
            cloneVertices[vertexIndex] = rend.bones[boneIndex].localPosition;
            mesh.vertices = cloneVertices;
        }
        mesh.RecalculateNormals();
        rend.sharedMesh = mesh;
    }

    protected void AddCollider(GameObject bone, float colliderSize, string type = "box")
    {        
        if (type == "sphere") {
            bone.AddComponent<SphereCollider>();
            SphereCollider collider = bone.GetComponent<SphereCollider>();
            collider.radius = colliderSize;
            collider.transform.localScale = bone.transform.localScale;
        } else {
            bone.AddComponent<BoxCollider>();
            BoxCollider collider = bone.GetComponent<BoxCollider>();
            collider.size = new Vector3(colliderSize, colliderSize, colliderSize);
            collider.transform.localScale = bone.transform.localScale;
        }
    }

    protected void SetCellLayers(int[][] vertexData)
    {
        int layerTotal = vertexData.Length;
        if (layerTotal % 2 == 0) {
            Debug.Log("Vertex data must be an odd array");
        }

        int midNumber = (layerTotal - 1) / 2;
        int cellLayerIndex = 0;

        for (int i = midNumber; i >= midNumber; i--) {
            int[] first = vertexData[midNumber - i];
            int[] second = vertexData[midNumber + i];
            int[] combined = first.Concat(second).ToArray();

            if (cellLayers.ContainsKey(cellLayerIndex)) {
                cellLayers[cellLayerIndex] = cellLayers[cellLayerIndex].Concat(combined).ToArray();
            } else {
                cellLayers.Add(cellLayerIndex,combined);
            }
            cellLayerIndex++;
        }
    }
}
