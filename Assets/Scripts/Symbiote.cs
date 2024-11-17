using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class Symbiote : MonoBehaviour
{
    public Material mat;
    public int horizontalVertexNumber = 1; // From negative number to postive number of vertex range on horizontal.
    public int verticalVertexNumber = 1; // From negative number to postive number of vertex range on vertical.
    public bool freezePos = false;
    public bool spherize = false;
    public bool isSphereCollider = false;

    [SerializeField]
    private bool onGizmo = false;
    [SerializeField]
    private float colliderSize = 0.2f;
    [SerializeField]
    private float spring = 10;
    [SerializeField]
    private bool autoAnchor = false;
    [SerializeField]
    private bool enableCollision = true;
    [SerializeField]
    private float damper = 2;
    [SerializeField]
    private bool isUpdateWhenOffscreen = false;
    
    public float x {get { return 1;}}
    public float y {get { return 1;}}
    public float z {get { return 1;}}
    private Dictionary<int, int> cells = new Dictionary<int, int>{}; // Each cell includes index is vertex index and value is bone index.
    private SkinnedMeshRenderer rend;
    private Mesh mesh;
    private GameObject centerBone;
    private SymBone symBone;
    private SymVertex symVertex;
    private SymDebug symDebug;
    private SymJoint symJoint;

    // Start is called before the first frame update
    void Start()
    {
        Born();
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
        // if (Vector3.Distance(transform.position, centerBone.transform.position) > 0.2f) {
        //     transform.position = centerBone.transform.position;
        // }
        // Debug.Log("p--" + transform.position);
        // Debug.Log("c--" + centerBone.transform.position);
        // Debug.Log("---------------------------------------");
    }

    private void Born()
    {
        gameObject.AddComponent<SkinnedMeshRenderer>();
        rend = gameObject.GetComponent<SkinnedMeshRenderer>();
        mesh = new Mesh();
        GameObject[] bones = new GameObject[]{};

        symBone = new SymBone(this, bones, rend, mesh);
        symVertex = new SymVertex(mesh);
        symDebug = new SymDebug();
        symJoint = new SymJoint();

        CreateCube();
    }

    private void CreateCube()
    {   
        if (freezePos) {
            Rigidbody rigidbody = gameObject.GetComponent<Rigidbody>();
            rigidbody.constraints = RigidbodyConstraints.FreezePosition;

        }

        // Add Center bone.
        centerBone = GenerateCell(Vector3.zero, "0");

        GenerateFront();
        GenerateRight();
        GenerateTop();
        GenerateCross();
        GenerateCross(true);

        if (spherize) {
            symBone.SpherizeBones();
            symVertex.SpherizeVectors();
        }

        // Add collider for all bones.
        symBone.AddColliderForBones(colliderSize);

        mesh.RecalculateNormals();

        // Recalculate bounds of Renderer every frame.
        rend.updateWhenOffscreen = isUpdateWhenOffscreen;

        // Add Bones and Mesh into Renderer.
        rend.sharedMesh = mesh;
        rend.material = mat;
    }

    private void GenerateFront()
    {
        float spacePerForward = z / (verticalVertexNumber * 2);
        float forwardPos = (z / 2) * -1;
        int forwardStart = verticalVertexNumber * -1;
        for (int i = forwardStart; i <= verticalVertexNumber; i++) {
            GenerateSquadCells(x, y, forwardPos, "front");
            CreateSquadTriangles(
                horizontalVertexNumber * 2 + 1,
                verticalVertexNumber * 2 + 1,
                i == verticalVertexNumber
            );
            forwardPos += spacePerForward;
        }
    }

    private void GenerateRight()
    {
        float spacePerForward = x / (horizontalVertexNumber * 2);
        float forwardPos = (x / 2) * -1;
        int forwardStart = horizontalVertexNumber * -1;
        for (int i = forwardStart; i <= horizontalVertexNumber; i++) {
            GenerateSquadCells(z, y, forwardPos, "right");
            CreateSquadTriangles(
                horizontalVertexNumber * 2 + 1,
                verticalVertexNumber * 2 + 1,
                i == forwardStart
            );
            forwardPos += spacePerForward;
        }
    }

    private void GenerateTop()
    {
        float spacePerForward = y / (verticalVertexNumber * 2);
        float forwardPos = (y / 2) * -1;
        int forwardStart = verticalVertexNumber * -1;
        for (int i = forwardStart; i <= verticalVertexNumber; i++) {
            GenerateSquadCells(x, z, forwardPos, "top");
            CreateSquadTriangles(
                horizontalVertexNumber * 2 + 1,
                verticalVertexNumber * 2 + 1,
                i == forwardStart
            );
            forwardPos += spacePerForward;
        }
    }

    private void GenerateCross(bool rightSide = false)
    {
        // Crosses in a Squad are separated by:
        // - One middle cross.
        // - And two half cross parts in two Right Triangles.
        int totalCrosses = (horizontalVertexNumber * 2 + 1) + (verticalVertexNumber * 2 + 1) - 3;
        int halfCrosses = (totalCrosses - 1) / 2;

        // Which the side is crosses will be at.
        int side = rightSide ? -1 : 1;

        float spacePerHorizontal = x / (horizontalVertexNumber * 2);
        float spacePerVertical = z / (verticalVertexNumber * 2);
        float verticalPos = 0;
        float horizontalPos = 0;
        int crossVertexTotal = 1;

        // Process Crosses in Right Triagle on top.
        horizontalPos = (x / 2) * -1 * side;
        verticalPos = z / 2;
        for (int i = 1; i <= halfCrosses; i++) {
            crossVertexTotal += 1;
            verticalPos -= spacePerVertical;

            GenerateCrossSquad();
        }

        // Process Crosses in Right Triagle on bottom.
        horizontalPos = (x / 2) * side;
        verticalPos = (z / 2) * -1;
        crossVertexTotal = 1;
        for (int i = 1; i <= halfCrosses; i++) {
            crossVertexTotal += 1;

            if (rightSide) {
                horizontalPos += spacePerHorizontal;
            } else {
                horizontalPos -= spacePerHorizontal;
            }

            GenerateCrossSquad();
        }

        // Process Middle cross.
        int crossVertexHalf = Math.Min(horizontalVertexNumber, verticalVertexNumber);
        crossVertexTotal = (crossVertexHalf * 2) + 1;
        horizontalPos = 0; // Start from center.
        verticalPos = 0; // Start from center.
        for (int i = 1; i <= crossVertexHalf; i++) {
            verticalPos -= spacePerVertical;
            if (rightSide) {
                horizontalPos += spacePerHorizontal;
            } else {
                horizontalPos -= spacePerHorizontal;
            }
        }
        GenerateCrossSquad();

        void GenerateCrossSquad()
        {
            // This vertex is start point to generate cross squad.
            Vector3 vertexStartPosition = new Vector3(horizontalPos, (y / 2) * -1, verticalPos);

            GenerateCrossSquadCells(
                vertexStartPosition,
                x / (horizontalVertexNumber * 2),
                y / (verticalVertexNumber * 2),
                z / (verticalVertexNumber * 2),
                crossVertexTotal,
                verticalVertexNumber * 2 + 1,
                rightSide
            );
            CreateSquadTriangles(
                crossVertexTotal,
                verticalVertexNumber * 2 + 1,
                rightSide
            );
        }
    }

    private GameObject GenerateCell(Vector3 position, string name)
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

        return symBone.GetLastBone();
    }

    private void GenerateSquadCells(float width, float height, float forwardPos, string side)
    {
        float spacePerCol = width / (horizontalVertexNumber * 2);
        float spacePerRow = height / (verticalVertexNumber * 2);
        int cols = horizontalVertexNumber * 2 + 1;
        int rows = verticalVertexNumber * 2 + 1;

        int colStart = horizontalVertexNumber * -1;
        int rowStart = verticalVertexNumber * -1;

        // Generate vertices.
        int vertexTotal = cols * rows;
        Vector3 position = Vector3.zero;
        int currentVertexIndex = 0;
        float horizontalPos = (width / 2) * -1;
        for (int i=colStart; i<=horizontalVertexNumber; i++) {
            float verticalPos = (height / 2) * -1; 
            for (int j=rowStart; j<=verticalVertexNumber; j++) {
                if (currentVertexIndex == vertexTotal) {
                    break;
                }

                switch (side) {
                    case "front":
                    case "back":
                        position = new Vector3(horizontalPos, verticalPos, forwardPos);
                        break;
                    case "right":
                    case "left":
                        position = new Vector3(forwardPos, verticalPos, horizontalPos);
                        break;
                    case "top":
                    case "bot":
                        position = new Vector3(horizontalPos, forwardPos, verticalPos);
                        break;
                };

                // Create bone for vertex.
                string name = symBone.CountBones().ToString();
                GenerateCell(position, name);

                verticalPos += spacePerRow;
                currentVertexIndex++;
            }
            horizontalPos += spacePerCol;
        }
    }

    private void GenerateCrossSquadCells(
        Vector3 vertexStartPosition,
        float spacePerHorizontal,
        float spacePerVertical,
        float spacePerForward,
        int horizontalVertexTotal,
        int verticalVertexTotal,
        bool rightSide = false
    )
    {
        float horizontalPos = vertexStartPosition.x;
        float forwardPos = vertexStartPosition.z;
        
        // Generate vertices.
        int vertexTotal = horizontalVertexTotal * verticalVertexTotal;
        int currentVertexIndex = 0;

        for (int i=1; i<=horizontalVertexTotal; i++) {
            float verticalPos = vertexStartPosition.y;
             
            for (int j=1; j<=verticalVertexTotal; j++) {
                if (currentVertexIndex == vertexTotal) {
                    break;
                }

                Vector3 position = new Vector3(horizontalPos, verticalPos, forwardPos);

                // Create bone for vertex.
                string name = symBone.CountBones().ToString();
                GenerateCell(position, name);

                verticalPos += spacePerVertical;
                currentVertexIndex++;
            }
            forwardPos += spacePerForward;

            if (rightSide) {
                horizontalPos -= spacePerHorizontal;
            } else {
                horizontalPos += spacePerHorizontal;
            }
        }
    }

    private void UpdateCellPositions()
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

    private void CreateSquadTriangles(int horizontalVertexTotal, int verticalVertexTotal, bool isCounterClockwise = false)
    {
        int cols = horizontalVertexTotal;
        int rows = verticalVertexTotal;
        int[] triangles = new int[(cols-1)*(rows-1)*3*2];
        int currentTriangleIndex = 0;
        for (int i=0; i<cols-1; i++) {
            int startI = i * rows + (mesh.vertexCount - cols * rows);
            for (int j=0; j<rows-1; j++) {
                int aVertex = startI + j;
                int bVertex = aVertex + rows + 1;
                int cVertex = aVertex + rows;
                int dVertex = aVertex + 1;

                // Bottom Right triangle.
                triangles[currentTriangleIndex++] = aVertex;
                triangles[currentTriangleIndex++] = bVertex;
                triangles[currentTriangleIndex++] = cVertex;

                // Top Left triangle.
                triangles[currentTriangleIndex++] = aVertex;
                triangles[currentTriangleIndex++] = dVertex;
                triangles[currentTriangleIndex++] = bVertex;

                // Connect bones together.
                symJoint.CreateJoint(
                    symBone.GetBoneByPosition(symVertex.GetVertexByIndex(aVertex)),
                    symBone.GetBoneByPosition(symVertex.GetVertexByIndex(bVertex)),
                    spring, damper, autoAnchor, enableCollision
                );
                symJoint.CreateJoint(
                    symBone.GetBoneByPosition(symVertex.GetVertexByIndex(bVertex)),
                    symBone.GetBoneByPosition(symVertex.GetVertexByIndex(cVertex)),
                    spring, damper, autoAnchor, enableCollision
                );
                symJoint.CreateJoint(
                    symBone.GetBoneByPosition(symVertex.GetVertexByIndex(cVertex)),
                    symBone.GetBoneByPosition(symVertex.GetVertexByIndex(aVertex)),
                    spring, damper, autoAnchor, enableCollision
                );
                symJoint.CreateJoint(
                    symBone.GetBoneByPosition(symVertex.GetVertexByIndex(aVertex)),
                    symBone.GetBoneByPosition(symVertex.GetVertexByIndex(dVertex)),
                    spring, damper, autoAnchor, enableCollision
                );
                symJoint.CreateJoint(
                    symBone.GetBoneByPosition(symVertex.GetVertexByIndex(dVertex)),
                    symBone.GetBoneByPosition(symVertex.GetVertexByIndex(bVertex)),
                    spring, damper, autoAnchor, enableCollision
                );
                symJoint.CreateJoint(
                    symBone.GetBoneByPosition(symVertex.GetVertexByIndex(cVertex)),
                    symBone.GetBoneByPosition(symVertex.GetVertexByIndex(dVertex)),
                    spring, damper, autoAnchor, enableCollision
                );
            }
        }

        if (isCounterClockwise) {
            Array.Reverse(triangles);
        }

        mesh.triangles = mesh.triangles.Concat(triangles).ToArray();
    }
}
