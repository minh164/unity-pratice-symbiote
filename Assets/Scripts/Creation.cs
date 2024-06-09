using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class Creation : MonoBehaviour
{
    public Material mat;
    public float x;
    public float y;
    public float z;
    [SerializeField]
    private bool spherize = false;
    [SerializeField]
    private float colliderSize = 0.2f;
    [SerializeField]
    private float spring = 10;
    [SerializeField]
    private bool autoAnchor = false;
    

    private SkinnedMeshRenderer rend;
    private Mesh mesh;
    private int horizontalVertexNumber = 2; // From negative number to postive number of vertex range on horizontal.
    private int vertivcalVertexNumber = 2; // From negative number to postive number of vertex range on vertical.

    private GameObject[] bones = new GameObject[]{};

    // Start is called before the first frame update
    void Start()
    {
        CreateCube();

        // for (int i = 0; i < mesh.vertices.Length; i++) {
        //     for (int j = 0; j < mesh.vertices.Length; j++) {
        //         if (i != j && mesh.vertices[i] == mesh.vertices[j]) {
        //             Debug.Log(i + "----" + mesh.vertices[i]);
        //             Debug.Log(i + " = " + j);
        //         }
        //     }
        // }
    }

    // Update is called once per frame
    void Update()
    {
    }

    private void CreateCube()
    {
        gameObject.AddComponent<SkinnedMeshRenderer>();
        rend = gameObject.GetComponent<SkinnedMeshRenderer>();

        mesh = new Mesh();

        // Add Center bone.
        GameObject centerBone = GenerateCell(Vector3.zero, "0");

        GenerateFront();
        GenerateBack();
        GenerateRight();
        GenerateLeft();
        GenerateTop();
        GenerateBot();

        mesh.RecalculateNormals();

        // Connect center to parent.
        // CreateJoint(centerBone, gameObject);

        // Connect bones to center bone.
        for (int i = 1; i < bones.Length; i++) {
            CreateJoint(bones[i], centerBone);
        }

        // Add Bones and Mesh into Renderer.
        rend.sharedMesh = mesh;
        rend.material = mat;
    }

    private void GenerateFront()
    {
        float forwardPos = (z / 2) * -1;
        GenerateSquadCells(x, y, forwardPos, "front");
        CreateSquadTriangles();
    }

    private void GenerateBack()
    {
        GenerateSquadCells(x, y, z/2, "back");
        CreateSquadTriangles(true);
    }

    private void GenerateRight()
    {
        GenerateSquadCells(z, y, x/2, "right");
        CreateSquadTriangles();
    }

    private void GenerateLeft()
    {
        float forwardPos = (x / 2) * -1;
        GenerateSquadCells(z, y, forwardPos, "left");
        CreateSquadTriangles(true);
    }

    private void GenerateTop()
    {
        GenerateSquadCells(x, z, y/2, "top");
        CreateSquadTriangles();
    }

    private void GenerateBot()
    {
        float forwardPos = (y / 2) * -1;
        GenerateSquadCells(x, z, forwardPos, "bot");
        CreateSquadTriangles(true);
    }

    private GameObject GenerateCell(Vector3 position, string name)
    {
        Vector3 vertex = CreateVertex(position);

        // Add first bone weight, others are auto-added when create new vertex.
        if (mesh.boneWeights.Length <= 0) {
            mesh.boneWeights = new BoneWeight[] {
                new BoneWeight{weight0 = 1, boneIndex0 = 0}    
            };
        }

        int boneIndex = FindOrCreateBoneIndex(name, vertex);
        SetBoneToWeight(boneIndex, mesh.boneWeights.Length - 1);

        return bones[bones.Length - 1];
    }

    /// <summary>
    /// Set a bone to a weight.
    /// </summary>
    /// <param name="boneIndex"></param>
    /// <param name="weightIndex"></param>
    /// <param name="boneNumber"></param>
    private void SetBoneToWeight(int boneIndex, int weightIndex, int boneNumber = 0)
    {
        // Must clone bone weights from mesh to update a specified weight element by index.
        BoneWeight[] cloneWeights = mesh.boneWeights;
        BoneWeight weight = cloneWeights[weightIndex];

        // Auto set bone to empty bone number.
        if (boneNumber == 0) {
            if (weight.boneIndex0 < 0) {
                // Center bone has value is 0.
                boneNumber = 1;
            }
            else if (weight.boneIndex1 <= 0) {
                boneNumber = 2;
            }
            else if (weight.boneIndex2 <= 0) {
                boneNumber = 3;
            }
            else if (weight.boneIndex3 <= 0) {
                boneNumber = 4;
            }
        }

        switch (boneNumber) {
            case 1:
                weight.boneIndex0 = boneIndex;
                break;
            case 2:
                weight.boneIndex1 = boneIndex;
                break;
            case 3:
                weight.boneIndex2 = boneIndex;
                break;
            case 4:
                weight.boneIndex3 = boneIndex;
                break;
        }

        float boneSum = 0;
        if (weight.boneIndex0 >= 0) {
            // Center bone has value is 0.
            boneSum++;
        }
        if (weight.boneIndex1 > 0) {
            boneSum++;
        }
        if (weight.boneIndex2 > 0) {
            boneSum++;
        }
        if (weight.boneIndex3 > 0) {
            boneSum++;
        }

        // Recalculate each weight value.
        float weightValue = 1 / boneSum;

        switch (boneSum) {
            case 1:
                weight.weight0 = weightValue;
                break;
            case 2:
                weight.weight0 = weightValue;
                weight.weight1 = weightValue;
                break;
            case 3:
                weight.weight0 = weightValue;
                weight.weight1 = weightValue;
                weight.weight2 = weightValue;
                break;
            case 4:
                weight.weight0 = weightValue;
                weight.weight1 = weightValue;
                weight.weight2 = weightValue;
                weight.weight3 = weightValue;
                break;
        }

        cloneWeights[weightIndex] = weight;
        mesh.boneWeights = cloneWeights;
    }

    private Vector3 CreateVertex(Vector3 position)
    {
        if (spherize) {
            position = position.normalized;
        }

        Vector3[] vertices = new Vector3[] {
            position
        };
        mesh.vertices = mesh.vertices.Concat(vertices).ToArray();

        return position;
    }

    private Vector3 GetVertexByIndex(int index)
    {
        return mesh.vertices[index];
    }

    private GameObject GetBoneByIndex(int index)
    {
        return bones[index];
    }

    private GameObject GetBoneByPosition(Vector3 position)
    {
        return bones[FindBoneIndexByPosition(position)];
    }

    private int FindBoneIndexByPosition(Vector3 position)
    {
        int boneIndex = -1;
        for (int i = 0; i < bones.Length; i++) {
            GameObject bone = GetBoneByIndex(i);
            if (bone.transform.localPosition != position) {
                continue;
            }

            boneIndex = i;
        }

        return boneIndex;
    }

    /// <summary>
    /// If any bone has same position which need be to created,
    /// return it instead of create new one.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="position"></param>
    /// <returns></returns>
    private int FindOrCreateBoneIndex(string name, Vector3 position)
    {
        int boneIndex = FindBoneIndexByPosition(position);
        if (boneIndex >= 0) {
            return boneIndex;
        }

        Transform[] tranBones = new Transform[1];
        Matrix4x4[] bindposes = new Matrix4x4[1];

        GameObject bone = new GameObject(name);
        tranBones[0] = bone.transform;
        tranBones[0].parent = transform;
        // Set the position relative to the parent
        tranBones[0].localRotation = Quaternion.identity;
        tranBones[0].localPosition = position;
        
        bindposes[0] = tranBones[0].worldToLocalMatrix * transform.localToWorldMatrix;

        mesh.bindposes = mesh.bindposes.Concat(bindposes).ToArray();
        rend.bones = rend.bones.Concat(tranBones).ToArray();

        // Add rigid for bone.
        bone.AddComponent<Rigidbody>();
        bone.AddComponent<SphereCollider>();
        SphereCollider collider = bone.GetComponent<SphereCollider>();
        collider.radius = colliderSize;

        // Add movements for each bone.
        bone.AddComponent<Movement>();

        GameObject[] boneList = new GameObject[] {
            bone
        };
        bones = bones.Concat(boneList).ToArray();
        
        return bones.Length - 1;
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
                string name = bones.Count().ToString();
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
                CreateJoint(GetBoneByPosition(GetVertexByIndex(aVertex)), GetBoneByPosition(GetVertexByIndex(bVertex)));
                CreateJoint(GetBoneByPosition(GetVertexByIndex(bVertex)), GetBoneByPosition(GetVertexByIndex(cVertex)));
                CreateJoint(GetBoneByPosition(GetVertexByIndex(cVertex)), GetBoneByPosition(GetVertexByIndex(aVertex)));
                // CreateJoint(bones[aVertex], bones[dVertex]); // TODO:
            }
        }

        if (isCounterClockwise) {
            Array.Reverse(triangles);
        }

        mesh.triangles = mesh.triangles.Concat(triangles).ToArray();
    }

    private Dictionary<string, int> GetSquadCornerIndexes(int fromIndex, int toIndex)
    {
        int colTotalIndex = horizontalVertexNumber * 2;
        int rowTotalIndex = vertivcalVertexNumber * 2;

        int topLeft = fromIndex + rowTotalIndex;
        int topRight = topLeft + (colTotalIndex * rowTotalIndex) + colTotalIndex;

        Dictionary<string, int> cornerIndexes = new Dictionary<string, int>() {
            {"bot_left", fromIndex},    
            {"top_left", topLeft},    
            {"top_right", topRight},
            {"bot_right", topRight - rowTotalIndex},    
        };

        return cornerIndexes;
    }

    private void CreateJoint(GameObject bone, GameObject connectedBone)
    {
        bone.AddComponent<SpringJoint>();
        SpringJoint[] joints = bone.GetComponents<SpringJoint>();
        SpringJoint joint = joints[joints.Length - 1];
        joint.connectedBody = connectedBone.GetComponent<Rigidbody>();
        joint.autoConfigureConnectedAnchor = autoAnchor;
        joint.spring = spring;
    }

    private Vector3[] SpherizeVectors(Vector3[] vectors)
    {
        for(int i = 0; i<vectors.Length; i++)
        {
            // Vector3 vector = vectors[i]-origin;
            // Vector3 sphereVector = vector.normalized * (size/2)*1.67f;
            // Vector3 lerpdVector = Vector3.Lerp(vector,sphereVector,morphValue);
            // vectors[i] = origin+lerpdVector;
            Vector3 vector = vectors[i].normalized;
            vectors[i] = vector;
        }
        return vectors;
    }

    private void DebugVertices(Vector3[] vertices)
    {
        for (int i=0; i<vertices.Length; i++) {
            Debug.Log(vertices[i]);
            Debug.Log(i);
        }
    }

    private void DebugTriangles(int[] triangles)
    {
        int[] triangle = new int[3];
        int currentIndex = 0;
        for (int i=0; i<triangles.Length; i++) {
            triangle[currentIndex++] = triangles[i];
            
            if (currentIndex >= 3) {
                Debug.Log(string.Join(",", triangle));
                triangle = new int[3];
                currentIndex = 0;
            }
        }
    }

    private void DebugCornerIndexes(Dictionary<string, int> cornerIndexes)
    {
        Debug.Log("bot left: " + cornerIndexes["bot_left"]);
        Debug.Log("top left: " + cornerIndexes["top_left"]);
        Debug.Log("top right: " + cornerIndexes["top_right"]);
        Debug.Log("bot right: " + cornerIndexes["bot_right"]);
    }

    private void DebugWeight(BoneWeight weight)
    {
        Debug.Log("index0: " + weight.boneIndex0);
        Debug.Log("index1: " + weight.boneIndex1);
        Debug.Log("index2: " + weight.boneIndex2);
        Debug.Log("index3: " + weight.boneIndex3);
        Debug.Log("w0: " + weight.weight0);
        Debug.Log("w1: " + weight.weight1);
        Debug.Log("w2: " + weight.weight2);
        Debug.Log("w3: " + weight.weight3);
    }
}
