using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Creation : MonoBehaviour
{
    public Material mat;
    public float x;
    public float y;
    public float z;

    private SkinnedMeshRenderer rend;
    private Mesh mesh;
    private int horizontalVertexNumber = 2; // From negative number to postive number of vertex range on horizontal.
    private int vertivcalVertexNumber = 2; // From negative number to postive number of vertex range on vertical.
    private Dictionary<string, int> frontWeights;
    private Dictionary<string, int> backWeights;
    private Dictionary<string, int> rightWeights;
    private Dictionary<string, int> leftWeights;
    private Dictionary<string, int> topWeights;
    private Dictionary<string, int> botWeights;
    private Dictionary<string, int> bonesNameToIndex;

    // Start is called before the first frame update
    void Start()
    {
        bonesNameToIndex = new Dictionary<string, int>();
        CreateCube();
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

        GenerateFront();
        GenerateBack();
        GenerateRight();
        GenerateLeft();
        GenerateTop();
        GenerateBot();

        mesh.RecalculateNormals();

        // Add Center bone.
        GameObject centerBone = CreateBone("Center", Vector3.zero);

        // Add Bone Weights.
        CreateBoneWeights();

        // Add Top bones.
        GameObject topBlBone = CreateBone("Top-BotLeft", mesh.vertices[topWeights["bot_left"]]);
        GameObject topTlBone = CreateBone("Top-TopLeft", mesh.vertices[topWeights["top_left"]]);
        GameObject topTrBone = CreateBone("Top-TopRight", mesh.vertices[topWeights["top_right"]]);
        GameObject topBrBone = CreateBone("Top-BotRight", mesh.vertices[topWeights["bot_right"]]);
        
        // Add Bot bones.
        GameObject botBlBone = CreateBone("Bot-BotLeft", mesh.vertices[botWeights["bot_left"]]);
        GameObject botTlBone = CreateBone("Bot-TopLeft", mesh.vertices[botWeights["top_left"]]);
        GameObject botTrBone = CreateBone("Bot-TopRight", mesh.vertices[botWeights["top_right"]]);
        GameObject botBrBone = CreateBone("Bot-BotRight", mesh.vertices[botWeights["bot_right"]]);

        // Set bones to weights.
        SetBoneToSquadCorner("Top-BotLeft", topWeights, "BotLeft");
        SetBoneToSquadCorner("Top-BotLeft", leftWeights, "TopLeft");
        SetBoneToSquadCorner("Top-BotLeft", frontWeights, "TopLeft");

        SetBoneToSquadCorner("Top-TopLeft", topWeights, "TopLeft");
        SetBoneToSquadCorner("Top-TopLeft", backWeights, "TopLeft");
        SetBoneToSquadCorner("Top-TopLeft", leftWeights, "TopRight");

        SetBoneToSquadCorner("Top-BotRight", topWeights, "BotRight");
        SetBoneToSquadCorner("Top-BotRight", rightWeights, "TopLeft");
        SetBoneToSquadCorner("Top-BotRight", frontWeights, "TopRight");
        
        SetBoneToSquadCorner("Top-TopRight", topWeights, "TopRight");
        SetBoneToSquadCorner("Top-TopRight", rightWeights, "TopRight");
        SetBoneToSquadCorner("Top-TopRight", backWeights, "TopRight");

        SetBoneToSquadCorner("Bot-BotLeft", frontWeights, "BotLeft");
        SetBoneToSquadCorner("Bot-BotLeft", leftWeights, "BotLeft");
        SetBoneToSquadCorner("Bot-BotLeft", botWeights, "BotLeft");

        SetBoneToSquadCorner("Bot-TopLeft", botWeights, "TopLeft");
        SetBoneToSquadCorner("Bot-TopLeft", backWeights, "BotLeft");
        SetBoneToSquadCorner("Bot-TopLeft", leftWeights, "BotRight");

        SetBoneToSquadCorner("Bot-BotRight", frontWeights, "BotRight");
        SetBoneToSquadCorner("Bot-BotRight", botWeights, "BotRight");
        SetBoneToSquadCorner("Bot-BotRight", rightWeights, "BotLeft");

        SetBoneToSquadCorner("Bot-TopRight", rightWeights, "BotRight");
        SetBoneToSquadCorner("Bot-TopRight", backWeights, "BotRight");
        SetBoneToSquadCorner("Bot-TopRight", botWeights, "TopRight");

        // Add joints.
        CreateJoint(topBlBone, centerBone);
        CreateJoint(topTlBone, centerBone);
        CreateJoint(topBrBone, centerBone);
        CreateJoint(topTrBone, centerBone);
        
        CreateJoint(botBlBone, centerBone);
        CreateJoint(botTlBone, centerBone);
        CreateJoint(botBrBone, centerBone);
        CreateJoint(botTrBone, centerBone);

        mesh.vertices = SpherizeVectors(mesh.vertices);

        // Add Bones and Mesh into Renderer.
        rend.sharedMesh = mesh;
        rend.material = mat;
    }

    /// <summary>
    /// Set a bone to corner weights of a squad.
    /// </summary>
    /// <param name="boneName"></param>
    /// <param name="squadWeights"></param>
    /// <param name="corner"></param>
    private void SetBoneToSquadCorner(string boneName, Dictionary<string, int> squadWeights, string corner)
    {
        int spacePerCorner = (vertivcalVertexNumber * 2 + 1) * horizontalVertexNumber;
        switch (corner) {
            case "BotLeft":
                SetBoneToCorner(boneName, squadWeights["bot_left"]);
                break;
            case "TopLeft":
                SetBoneToCorner(boneName, squadWeights["top_left"] - vertivcalVertexNumber);
                break;
            case "BotRight":
                SetBoneToCorner(boneName, squadWeights["bot_right"] - spacePerCorner);
                break;
            case "TopRight":
                SetBoneToCorner(boneName, squadWeights["top_right"] - spacePerCorner - vertivcalVertexNumber);
                break;
        }
    }

    /// <summary>
    /// Set a bone to corner weights.
    /// </summary>
    /// <param name="boneName"></param>
    /// <param name="fromWeight"></param>
    private void SetBoneToCorner(string boneName, int fromWeight)
    {
        for (int i=fromWeight; i<=vertivcalVertexNumber+fromWeight; i++) {
            int colUnit = vertivcalVertexNumber * 2 + 1;
            for (int j=i; j<=(colUnit*horizontalVertexNumber+i); j+=colUnit) {
                SetBoneToWeight(boneName, j);
            }
        }
    }

    /// <summary>
    /// Set a bone to a weight.
    /// </summary>
    /// <param name="boneName"></param>
    /// <param name="weightIndex"></param>
    /// <param name="boneNumber"></param>
    private void SetBoneToWeight(string boneName, int weightIndex, int boneNumber = 0)
    {
        BoneWeight[] weights = mesh.boneWeights;

        // Auto set bone to empty bone number.
        if (boneNumber == 0) {
            if (weights[weightIndex].boneIndex0 < 0) {
                // Center bone has value is 0.
                boneNumber = 1;
            }
            else if (weights[weightIndex].boneIndex1 <= 0) {
                boneNumber = 2;
            }
            else if (weights[weightIndex].boneIndex2 <= 0) {
                boneNumber = 3;
            }
            else if (weights[weightIndex].boneIndex3 <= 0) {
                boneNumber = 4;
            }
        }

        switch (boneNumber) {
            case 1:
                weights[weightIndex].boneIndex0 = bonesNameToIndex[boneName];
                break;
            case 2:
                weights[weightIndex].boneIndex1 = bonesNameToIndex[boneName];
                break;
            case 3:
                weights[weightIndex].boneIndex2 = bonesNameToIndex[boneName];
                break;
            case 4:
                weights[weightIndex].boneIndex3 = bonesNameToIndex[boneName];
                break;
        }

        float boneSum = 0;
        if (weights[weightIndex].boneIndex0 >= 0) {
            // Center bone has value is 0.
            boneSum++;
        }
        if (weights[weightIndex].boneIndex1 > 0) {
            boneSum++;
        }
        if (weights[weightIndex].boneIndex2 > 0) {
            boneSum++;
        }
        if (weights[weightIndex].boneIndex3 > 0) {
            boneSum++;
        }

        // Recalculate each weight value.
        float weightValue = 1 / boneSum;

        switch (boneSum) {
            case 1:
                weights[weightIndex].weight0 = weightValue;
                break;
            case 2:
                weights[weightIndex].weight0 = weightValue;
                weights[weightIndex].weight1 = weightValue;
                break;
            case 3:
                weights[weightIndex].weight0 = weightValue;
                weights[weightIndex].weight1 = weightValue;
                weights[weightIndex].weight2 = weightValue;
                break;
            case 4:
                weights[weightIndex].weight0 = weightValue;
                weights[weightIndex].weight1 = weightValue;
                weights[weightIndex].weight2 = weightValue;
                weights[weightIndex].weight3 = weightValue;
                break;
        }

        mesh.boneWeights = weights;
    }

    private void CreateBoneWeights()
    {
        BoneWeight[] weights = new BoneWeight[mesh.vertexCount];

        for (int i=0; i<weights.Length; i++) {
            weights[i].weight0 = 1;
            weights[i].boneIndex0 = bonesNameToIndex["Center"];
        }
        mesh.boneWeights = weights;
    }

    private GameObject CreateBone(string name, Vector3 position)
    {
        Transform[] bones = new Transform[1];
        Matrix4x4[] bindposes = new Matrix4x4[1];

        GameObject bone = new GameObject(name);
        bone.AddComponent<Rigidbody>();
        bone.AddComponent<BoxCollider>();
        bones[0] = bone.transform;
        bones[0].parent = transform;
        // Set the position relative to the parent
        bones[0].localRotation = Quaternion.identity;
        bones[0].localPosition = position;
        
        bindposes[0] = bones[0].worldToLocalMatrix * transform.localToWorldMatrix;

        mesh.bindposes = mesh.bindposes.Concat(bindposes).ToArray();
        rend.bones = rend.bones.Concat(bones).ToArray();
        bonesNameToIndex[name] = rend.bones.Length - 1;

        return bone;
    }

    private void GenerateFront()
    {
        int fromIndex = mesh.vertexCount;
        float forwardPos = (z / 2) * -1;
        CreateSquadVertices(x, y, forwardPos, "front");
        CreateSquadTriangles();
        frontWeights = GetSquadCornerIndexes(fromIndex, mesh.vertexCount-1);
    }

    private void GenerateBack()
    {
        int fromIndex = mesh.vertexCount;
        CreateSquadVertices(x, y, z/2, "back");
        CreateSquadTriangles(true);
        backWeights = GetSquadCornerIndexes(fromIndex, mesh.vertexCount-1);
    }

    private void GenerateRight()
    {
        int fromIndex = mesh.vertexCount;
        CreateSquadVertices(z, y, x/2, "right");
        CreateSquadTriangles();
        rightWeights = GetSquadCornerIndexes(fromIndex, mesh.vertexCount-1);
    }

    private void GenerateLeft()
    {
        int fromIndex = mesh.vertexCount;
        float forwardPos = (x / 2) * -1;
        CreateSquadVertices(z, y, forwardPos, "left");
        CreateSquadTriangles(true);
        leftWeights = GetSquadCornerIndexes(fromIndex, mesh.vertexCount-1);
    }

    private void GenerateTop()
    {
        int fromIndex = mesh.vertexCount;
        CreateSquadVertices(x, z, y/2, "top");
        CreateSquadTriangles();
        topWeights = GetSquadCornerIndexes(fromIndex, mesh.vertexCount-1);
    }

    private void GenerateBot()
    {
        int fromIndex = mesh.vertexCount;
        float forwardPos = (y / 2) * -1;
        CreateSquadVertices(x, z, forwardPos, "bot");
        CreateSquadTriangles(true);
        botWeights = GetSquadCornerIndexes(fromIndex, mesh.vertexCount-1);
    }

    private void CreateSquadVertices(float width, float height, float forwardPos, string side)
    {
        float spacePerCol = width / (horizontalVertexNumber * 2);
        float spacePerRow = height / (vertivcalVertexNumber * 2);
        int cols = horizontalVertexNumber * 2 + 1;
        int rows = vertivcalVertexNumber * 2 + 1;

        int colStart = horizontalVertexNumber * -1;
        int rowStart = vertivcalVertexNumber * -1;

        // Generate vertices.
        Vector3[] vertices = new Vector3[cols * rows];
        int currentVertexIndex = 0;

        float horizontalPos = (width / 2) * -1;
        for (int i=colStart; i<=horizontalVertexNumber; i++) {
            float verticalPos = (height / 2) * -1; 
            for (int j=rowStart; j<=vertivcalVertexNumber; j++) {
                if (currentVertexIndex == vertices.Length) {
                    break;
                }

                switch (side) {
                    case "front":
                    case "back":
                        vertices[currentVertexIndex] = new Vector3(horizontalPos, verticalPos, forwardPos);
                        break;
                    case "right":
                    case "left":
                        vertices[currentVertexIndex] = new Vector3(forwardPos, verticalPos, horizontalPos);
                        break;
                    case "top":
                    case "bot":
                        vertices[currentVertexIndex] = new Vector3(horizontalPos, forwardPos, verticalPos);
                        break;
                };

                verticalPos += spacePerRow;
                currentVertexIndex++;
            }
            horizontalPos += spacePerCol;
        }

        mesh.vertices = mesh.vertices.Concat(vertices).ToArray();
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
                int startJ = startI + j;

                // Bottom Right triangle.
                triangles[currentTriangleIndex++] = startJ;
                triangles[currentTriangleIndex++] = startJ + cols + 1;
                triangles[currentTriangleIndex++] = startJ + cols;

                // Top Left triangle.
                triangles[currentTriangleIndex++] = startJ;
                triangles[currentTriangleIndex++] = startJ + 1;
                triangles[currentTriangleIndex++] = startJ + cols + 1;
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
        SpringJoint joint = bone.GetComponent<SpringJoint>();
        joint.connectedBody = connectedBone.GetComponent<Rigidbody>();
        joint.autoConfigureConnectedAnchor = true;
        joint.spring = 20;
    }

    private Vector3[] SpherizeVectors(Vector3[] vectors)
    {
        Vector3 origin = Vector3.zero;
        float size = 1;
        float morphValue = 0.5f;

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
