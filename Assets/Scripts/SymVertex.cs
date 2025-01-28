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

    public Vector3 FindOrCreateVertex(Vector3 position, bool spherize = false)
    {
        int vertexIndex = FindVertexIndexByPosition(position);
        if (vertexIndex >= 0) {
            return position;
        }

        return CreateVertex(position, spherize);
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

    public int FindVertexIndexByPosition(Vector3 position)
    {
        int vertexIndex = -1;
        for (int i = 0; i < _mesh.vertices.Length; i++) {
            // One position can be belonged to many vertices, this function will get latest vertex.
            if (_mesh.vertices[i] != position) {
                continue;
            }

            vertexIndex = i;
        }

        return vertexIndex;
    }

    public void SpherizeVectors(float radius)
    {
        Vector3[] vectors = new Vector3[] {};
        for(int i = 0; i<_mesh.vertices.Length; i++)
        {
            // Vector3 vector = vectors[i]-origin;
            // Vector3 sphereVector = vector.normalized * (size/2)*1.67f;
            // Vector3 lerpdVector = Vector3.Lerp(vector,sphereVector,morphValue);
            // vectors[i] = origin+lerpdVector;
            Vector3 normalize = _mesh.vertices[i].normalized;
            
            Vector3[] addVector = new Vector3[1] {new Vector3(
                normalize.x * radius,
                normalize.y * radius,
                normalize.z * radius
            )};
            vectors = vectors.Concat(addVector).ToArray();
        }

        _mesh.vertices = vectors;
    }
}
