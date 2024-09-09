using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class Symbiote : MonoBehaviour
{
    public Material mat;
    public float x;
    public float y;
    public float z;
    public bool freezePos = false;

    [SerializeField]
    private bool spherize = false;
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
    

    private SkinnedMeshRenderer rend;
    private Mesh mesh;
    private int horizontalVertexNumber = 2; // From negative number to postive number of vertex range on horizontal.
    private int vertivcalVertexNumber = 2; // From negative number to postive number of vertex range on vertical.

    private GameObject centerBone;
    private SymBone symBone;
    private SymVertex symVertex;
    private SymDebug symDebug;

    // Start is called before the first frame update
    void Start()
    {
        gameObject.AddComponent<SkinnedMeshRenderer>();
        rend = gameObject.GetComponent<SkinnedMeshRenderer>();
        mesh = new Mesh();
        GameObject[] bones = new GameObject[]{};

        symBone = new SymBone(this, bones, rend, mesh);
        symVertex = new SymVertex(mesh);
        symDebug = new SymDebug();

        CreateCube();
    }

    // Update is called once per frame
    void Update()
    {
        
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

        if (spherize) {
            symVertex.SpherizeVectors();
            symBone.SpherizeBones();
        }

        mesh.RecalculateNormals();

        // Connect center to parent.
        // CreateJoint(centerBone, gameObject);

        // Connect bones to center bone.
        // for (int i = 1; i < bones.Length; i++) {
        //     CreateJoint(bones[i], centerBone);
        // }

        // Add Bones and Mesh into Renderer.
        rend.sharedMesh = mesh;
        rend.material = mat;
    }

    private void GenerateFront()
    {
        float spacePerForward = z / (vertivcalVertexNumber * 2);
        float forwardPos = (z / 2) * -1;
        int forwardStart = vertivcalVertexNumber * -1;
        for (int i = forwardStart; i <= vertivcalVertexNumber; i++) {
            if (spherize && i != forwardStart && i != vertivcalVertexNumber) {
                forwardPos += spacePerForward;
                continue;
            }
            GenerateSquadCells(x, y, forwardPos, "front");
            CreateSquadTriangles(i == vertivcalVertexNumber);
            forwardPos += spacePerForward;
        }
    }

    private void GenerateRight()
    {
        float spacePerForward = x / (horizontalVertexNumber * 2);
        float forwardPos = (x / 2) * -1;
        int forwardStart = horizontalVertexNumber * -1;
        for (int i = forwardStart; i <= horizontalVertexNumber; i++) {
            if (spherize && i != forwardStart && i != vertivcalVertexNumber) {
                forwardPos += spacePerForward;
                continue;
            }
            GenerateSquadCells(z, y, forwardPos, "right");
            CreateSquadTriangles(i == forwardStart);
            forwardPos += spacePerForward;
        }
    }

    private void GenerateTop()
    {
        float spacePerForward = y / (vertivcalVertexNumber * 2);
        float forwardPos = (y / 2) * -1;
        int forwardStart = vertivcalVertexNumber * -1;
        for (int i = forwardStart; i <= vertivcalVertexNumber; i++) {
            if (spherize && i != forwardStart && i != vertivcalVertexNumber) {
                forwardPos += spacePerForward;
                continue;
            }
            GenerateSquadCells(x, z, forwardPos, "top");
            CreateSquadTriangles(i == forwardStart);
            forwardPos += spacePerForward;
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

        return symBone.GetLastBone();
    }

    private void GenerateSquadCells(float width, float height, float forwardPos, string side)
    {
        float spacePerCol = width / (horizontalVertexNumber * 2);
        float spacePerRow = height / (vertivcalVertexNumber * 2);
        int cols = horizontalVertexNumber * 2 + 1;
        int rows = vertivcalVertexNumber * 2 + 1;

        int colStart = horizontalVertexNumber * -1;
        int rowStart = vertivcalVertexNumber * -1;

        // Generate vertices.
        int vertexTotal = cols * rows;
        Vector3 position = Vector3.zero;
        int currentVertexIndex = 0;
        float horizontalPos = (width / 2) * -1;
        for (int i=colStart; i<=horizontalVertexNumber; i++) {
            float verticalPos = (height / 2) * -1; 
            for (int j=rowStart; j<=vertivcalVertexNumber; j++) {
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

    private void CreateSquadTriangles(bool isCounterClockwise = false)
    {
        int cols = horizontalVertexNumber * 2 + 1;
        int rows = vertivcalVertexNumber * 2 + 1;
        int[] triangles = new int[(cols-1)*(rows-1)*3*2];
        int currentTriangleIndex = 0;
        for (int i=0; i<cols-1; i++) {
            int startI = i * rows + (mesh.vertexCount - cols * rows);
            for (int j=0; j<rows-1; j++) {
                int aVertex = startI + j;
                int bVertex = aVertex + cols + 1;
                int cVertex = aVertex + cols;
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
                CreateJoint(
                    symBone.GetBoneByPosition(symVertex.GetVertexByIndex(aVertex)),
                    symBone.GetBoneByPosition(symVertex.GetVertexByIndex(bVertex))
                );
                CreateJoint(
                    symBone.GetBoneByPosition(symVertex.GetVertexByIndex(bVertex)),
                    symBone.GetBoneByPosition(symVertex.GetVertexByIndex(cVertex))
                );
                CreateJoint(
                    symBone.GetBoneByPosition(symVertex.GetVertexByIndex(cVertex)),
                    symBone.GetBoneByPosition(symVertex.GetVertexByIndex(aVertex))
                );
                CreateJoint(
                    symBone.GetBoneByPosition(symVertex.GetVertexByIndex(aVertex)),
                    symBone.GetBoneByPosition(symVertex.GetVertexByIndex(dVertex))
                );
                CreateJoint(
                    symBone.GetBoneByPosition(symVertex.GetVertexByIndex(dVertex)),
                    symBone.GetBoneByPosition(symVertex.GetVertexByIndex(bVertex))
                );
                CreateJoint(
                    symBone.GetBoneByPosition(symVertex.GetVertexByIndex(cVertex)),
                    symBone.GetBoneByPosition(symVertex.GetVertexByIndex(dVertex))
                );
            }
        }

        if (isCounterClockwise) {
            Array.Reverse(triangles);
        }

        mesh.triangles = mesh.triangles.Concat(triangles).ToArray();
    }

    private void CreateJoint(GameObject bone, GameObject connectedBone)
    {
        // If Current Bone has connected with Connected Bone OR reverse, it will not do again.
        if (IsExistedJoint(bone, connectedBone) || IsExistedJoint(connectedBone, bone)) {
            return;
        }
        
        bone.AddComponent<SpringJoint>();
        SpringJoint[] joints = bone.GetComponents<SpringJoint>();
        SpringJoint newJoint = joints[joints.Length - 1];
        newJoint.connectedBody = connectedBone.GetComponent<Rigidbody>();
        newJoint.autoConfigureConnectedAnchor = autoAnchor;
        newJoint.spring = spring;
        newJoint.enableCollision = enableCollision;
        newJoint.damper = damper;

        // Debug.DrawLine(bone.transform.position, connectedBone.transform.position);
    }

    private bool IsExistedJoint(GameObject bone, GameObject connectedBone)
    {
        SpringJoint[] joints = bone.GetComponents<SpringJoint>();
        foreach (SpringJoint joint in joints) {
            if (joint.connectedBody.GetInstanceID() == connectedBone.GetComponent<Rigidbody>().GetInstanceID()) {
                return true;
            }
        }

        return false;
    }
}
