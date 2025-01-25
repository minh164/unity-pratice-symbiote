using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SymCube : Symbiote
{
    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    protected override void Create()
    {
        // Add Center Cell.
        GenerateCell(Vector3.zero, "0");

        GenerateSide("front");
        GenerateSide("right");
        GenerateSide("top");
        GenerateCross();
        GenerateCross(true);

        // Add collider for all bones.
        AddColliderForBones(colliderSize);
    }

    protected void GenerateSide(string side, bool onlyOutsideQuads = false)
    {
        float forward = 0, width = 0, height = 0;
        int vertexNumber = 0;
        // Determine which first (-1) or last (1) quad will be counter cloclwise.
        int counterClockwiseQuad = -1;

        switch (side) {
            case "front":
                forward = z;
                width = x;
                height = y;
                vertexNumber = verticalVertexNumber;
                counterClockwiseQuad = 1;
                break;
            case "right":
                forward = x;
                width = z;
                height = y;
                vertexNumber = horizontalVertexNumber;
                break;
            case "top":
                forward = y;
                width = x;
                height = z;
                vertexNumber = verticalVertexNumber;
                break;
        }
        
        float spacePerForward = forward / (vertexNumber * 2);
        float forwardPos = forward / 2 * -1;
        int forwardStart = vertexNumber * -1;
        for (int i = forwardStart; i <= vertexNumber; i++) {
            // Only generate outside quads (exclude inside ones).
            if (
                onlyOutsideQuads
                && i != forwardStart 
                && i != vertexNumber 
            ) {
                forwardPos += spacePerForward;
                continue;
            }

            GenerateQuadCells(width, height, forwardPos, side);
            CreateQuadTriangles(
                horizontalVertexNumber * 2 + 1,
                verticalVertexNumber * 2 + 1,
                counterClockwiseQuad < 0 ? i == forwardStart : i == vertexNumber
            );
            forwardPos += spacePerForward;
        }
    }

    protected void GenerateCross(bool rightSide = false)
    {
        // Crosses in a Quad are separated by:
        // - One middle cross.
        // - And two half cross parts in two Right Triangles.
        int totalCrosses = (horizontalVertexNumber * 2 + 1) + (verticalVertexNumber * 2 + 1) - 3;
        int halfCrosses = (totalCrosses - 1) / 2;

        // Which the side is crosses will be at.
        int side = rightSide ? -1 : 1;

        float spacePerHorizontal = x / (horizontalVertexNumber * 2);
        float spacePerVertical = z / (verticalVertexNumber * 2);
        int crossVertexTotal = 1;

        // Process Crosses in Right Triagle on top.
        float horizontalPos = x / 2 * -1 * side;
        float verticalPos = z / 2;
        for (int i = 1; i <= halfCrosses; i++) {
            crossVertexTotal += 1;
            verticalPos -= spacePerVertical;

            GenerateCrossQuad();
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

            GenerateCrossQuad();
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
        GenerateCrossQuad();

        void GenerateCrossQuad()
        {
            // This vertex is start point to generate cross quad.
            Vector3 vertexStartPosition = new Vector3(horizontalPos, (y / 2) * -1, verticalPos);

            GenerateCrossQuadCells(
                vertexStartPosition,
                x / (horizontalVertexNumber * 2),
                y / (verticalVertexNumber * 2),
                z / (verticalVertexNumber * 2),
                crossVertexTotal,
                verticalVertexNumber * 2 + 1,
                rightSide
            );
            CreateQuadTriangles(
                crossVertexTotal,
                verticalVertexNumber * 2 + 1,
                rightSide
            );
        }
    }

    private void GenerateQuadCells(float width, float height, float forwardPos, string side)
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
                GenerateSideCell(position, name);

                verticalPos += spacePerRow;
                currentVertexIndex++;
            }
            horizontalPos += spacePerCol;
        }
    }

    private void GenerateCrossQuadCells(
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
                GenerateCrossCell(position, name);

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

    protected void CreateQuadTriangles(int horizontalVertexTotal, int verticalVertexTotal, bool isCounterClockwise = false)
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

    protected virtual void AddColliderForBones(float colliderSize = 0)
    {
        if (colliderSize <= 0) {
            // Calculate volume total of Parent.
            float volumeOfParent = GetVolume();

            // Calculate volume each bone (we forced scale number of bone is the same parent).
            float volumeEachBone  = volumeOfParent / symBone.CountBones();

            // Calculate each vector's magnitude of a collider.
            double colliderSizeDouble = Math.Cbrt((double) volumeEachBone);
            colliderSize = (float) colliderSizeDouble;
        }

        foreach (GameObject bone in symBone.GetBones()) {
            AddCollider(bone, colliderSize);
        }
    }

    public virtual float GetVolume()
    {
        return x * y * z;
    }
}
