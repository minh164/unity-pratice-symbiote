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
}
