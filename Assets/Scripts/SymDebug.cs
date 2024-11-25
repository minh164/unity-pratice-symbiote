using System;
using System.Collections.Generic;
using UnityEngine;

public class SymDebug
{
    public void DebugVertices(Vector3[] vertices)
    {
        for (int i=0; i<vertices.Length; i++) {
            Debug.Log(vertices[i]);
            Debug.Log(i);
        }
    }

    public void DebugRendBonePostions(Transform[] boneTransforms)
    {
        for (int i=0; i<boneTransforms.Length; i++) {
            Debug.Log(boneTransforms[i].position);
            Debug.Log(i);
        }
    }

    public void DebugTriangles(int[] triangles)
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

    public void DebugCornerIndexes(Dictionary<string, int> cornerIndexes)
    {
        Debug.Log("bot left: " + cornerIndexes["bot_left"]);
        Debug.Log("top left: " + cornerIndexes["top_left"]);
        Debug.Log("top right: " + cornerIndexes["top_right"]);
        Debug.Log("bot right: " + cornerIndexes["bot_right"]);
    }

    public void DebugWeight(BoneWeight weight)
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

    public void DrawVertices(Vector3[] vertices)
    {
        for (int i=0; i<vertices.Length; i++) {
            Debug.DrawLine(vertices[i], vertices[i]);
        }
    }

    public void GizmoMeshVertices(Mesh mesh, Vector3 mainPosition)
    {
        for (int i = 0; i < mesh.vertices.Length; i++) {
            float size = 0.1f;
            Vector3 vertexPos = new Vector3(
                mesh.vertices[i].x,
                mainPosition.y - mesh.vertices[i].y,
                mesh.vertices[i].z
            );
            Gizmos.color = new Color(1, 0, 0, 1);
            Gizmos.DrawCube(vertexPos, new Vector3(size, size, size));
            Debug.Log("vector: " + vertexPos);
            Debug.Log(mainPosition);
            Debug.Log(mesh.vertices[i]);
        }
    }

    public void GizmoRendBones(SkinnedMeshRenderer rend)
    {
        for (int i = 0; i < rend.bones.Length; i++) {
            float size2 = 0.1f;
            Vector3 bonePos = rend.bones[i].position;
            Gizmos.color = new Color(0, 173, 255, 0.5f);
            Gizmos.DrawSphere(bonePos, size2);
            Debug.Log("bone: " + bonePos);
        }
    }

    public void GizmoVerticesAndBones(Mesh mesh, Vector3 mainPosition, SkinnedMeshRenderer rend)
    {
        for (int i = 0; i < mesh.vertices.Length; i++) {
            float size = 0.1f;
            Vector3 vertexPos = new Vector3(
                mesh.vertices[i].x,
                mainPosition.y - mesh.vertices[i].y,
                mesh.vertices[i].z
            );
            Gizmos.color = new Color(1, 0, 0, 1);
            Gizmos.DrawCube(mesh.vertices[i], new Vector3(size, size, size));
            // Debug.Log("vector: " + mesh.vertices[i]);

            if (i < rend.bones.Length) {
                float size2 = 0.1f;
                Vector3 bonePos = rend.bones[i].position;
                Gizmos.color = new Color(0, 173, 255, 0.5f);
                Gizmos.DrawSphere(bonePos, size2);
                // Debug.Log("bone " + i + ": " + rend.bones[i].localPosition);
            }
            // Debug.Log("-------------------------------------");
        }
    }

    public void CheckMatchedVertexAndBone(Mesh mesh, SkinnedMeshRenderer rend)
    {
        for (int i = 0; i < mesh.vertices.Length; i++) {
            Vector3 vertex = mesh.vertices[i];

            if (i < rend.bones.Length) {
                Vector3 bone = rend.bones[i].localPosition;

                if (! vertex.Equals(bone)) {
                    Debug.Log("vector: " + mesh.vertices[i]);
                    Debug.Log("bone " + i + ": " + rend.bones[i].localPosition);
                    Debug.Log("-------------------------------------");
                }
            }
        }
    }

    private double CalculateHypotenuse(double sideA, double sideB)
    {
        // Using Pythagorean theorem (a right-triangle has two sides and one hypotenuse). 
        double hypotenuse = Math.Sqrt((sideA * sideA) + (sideB * sideB));

        Debug.Log(sideA);
        Debug.Log(sideB);
        Debug.Log(hypotenuse);

        return hypotenuse;
    }

    public void DebugCells(Dictionary<int, int> cells, SymBone symBone, SymVertex symVertex)
    {
        foreach (var cell in cells) {
            GameObject bone = symBone.GetBoneByIndex(cell.Value);
            Vector3 vertex = symVertex.GetVertexByIndex(cell.Key);

            Debug.Log("----" + cell.Key);
            Debug.Log("b :" + bone.transform.localPosition);
            Debug.Log("v :" + vertex);
        }
    }
}
