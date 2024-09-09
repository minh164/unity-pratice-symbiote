using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SymVertex
{
    private Mesh _mesh;

    public SymVertex(Mesh mesh)
    {
        _mesh = mesh;
    }

    public Vector3 CreateVertex(Vector3 position, bool spherize = false)
    {
        if (spherize) {
            position = position.normalized;
        }

        Vector3[] vertices = new Vector3[] {
            position
        };
        _mesh.vertices = _mesh.vertices.Concat(vertices).ToArray();

        return position;
    }

    public Vector3 GetVertexByIndex(int index)
    {
        return _mesh.vertices[index];
    }

    public void SpherizeVectors()
    {
        Vector3[] vectors = new Vector3[] {};
        for(int i = 0; i<_mesh.vertices.Length; i++)
        {
            // Vector3 vector = vectors[i]-origin;
            // Vector3 sphereVector = vector.normalized * (size/2)*1.67f;
            // Vector3 lerpdVector = Vector3.Lerp(vector,sphereVector,morphValue);
            // vectors[i] = origin+lerpdVector;
            Vector3[] addVector = new Vector3[1] {_mesh.vertices[i].normalized};
            vectors = vectors.Concat(addVector).ToArray();
        }

        _mesh.vertices = vectors;
    }
}
