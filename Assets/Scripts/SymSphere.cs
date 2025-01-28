using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SymSphere : SymCube
{
    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
    }

    // Update is called once per frame
    void Update()
    {
        symDebug.DrawBoneConnections(symBone.GetBones());
    }

    protected override void Create()
    {
        // Add Center Cell.
        CreateCell(Vector3.zero, "0");

        GenerateSide("front", false);
        GenerateSide("right", false);
        GenerateSide("top", false);
        // GenerateCross();
        // GenerateCross(true);

        // Add collider for all bones.
        AddColliderForBones(colliderSize);
    }

    protected override int CreateSideCell(Vector3 position, string name, string side = null, bool isOutside = false)
    {
        if (side == null) {
            return CreateCell(position, name);    
        }

        // Spherize position of vertex and bone in initialization.
        if (isOutside) {
            // Spherize outside cell by origin centroid (0,0,0).
            position = SpherizePosition(position);
        } else {
            // Spherize inside cell by quad (will be transformed to a circle) centroid.
            position = SpherizeInsidePosition(position, side);
        }

        return CreateCell(position, name);
    }

    protected override int CreateCrossCell(Vector3 position, string name)
    {
        return base.CreateCrossCell(position, name);
    }

    protected override void AddColliderForBones(float colliderSize = 0)
    {
        if (colliderSize <= 0) {
            // Calculate volume total of Parent.
            float volumeOfParent = GetVolume();

            // Calculate volume each bone (we forced scale number of bone is the same parent).
            float volumeEachBone = volumeOfParent / symBone.CountBones();

            // Calculate each vector's magnitude of a collider.
            colliderSize = FindRadiusByVolume(volumeEachBone);
        }

        foreach (GameObject bone in symBone.GetBones()) {
            AddCollider(bone, colliderSize, "sphere");
        }
    }

    public float GetRadius()
    {
        return FindRadiusByVolume(GetVolume());
    }

    private float FindRadiusByVolume(float volume)
    {
        double doubleVolume = (double) volume;
        double exponent = 1 / 3d;

        double doubleValue = Math.Pow(
            3 * doubleVolume / (4 * Math.PI),
            exponent
        );

        return (float) doubleValue;
    }

    private Vector3 SpherizePosition(Vector3 position)
    {
        float radius = GetRadius();
        Vector3 normalize = position.normalized;

        return new Vector3(
            normalize.x * radius,
            normalize.y * radius,
            normalize.z * radius
        );
    }

    private Vector3 SpherizeInsidePosition(Vector3 position, string side)
    {
        // Radius from origin centroid (0,0,0).
        float originRadius = GetRadius();

        // Dimensions of quad position.
        Dictionary<string, float> dimentions = GetDimensionsBySide(position, side);

        // Quad centroid.
        Vector3 quadCentroid = CreatePositionBySide(side, 0, 0, dimentions["forward"]);

        // If quad centroid is quad position, it will NOT need moving.
        if (quadCentroid == position) {
            return position;
        }
        
        // Distance from origin centroid to quad centroid.
        float centroidDistance = Vector3.Distance(Vector3.zero, quadCentroid);

        // Calculate radius of circle will be transformed from quad.
        double circleRadiusDouble = Math.Sqrt((originRadius * originRadius) - (centroidDistance * centroidDistance));
        float circleRadius = (float) circleRadiusDouble;

        // Vector from quad position to quad centroid.
        Vector3 positionToQuadCentroid = quadCentroid - position;

        Vector3 spherizePosition = MoveByDisDir(
            position,
            positionToQuadCentroid.normalized,
            positionToQuadCentroid.magnitude - circleRadius
        );
    

        return spherizePosition;
    }

    /// <summary>
    /// Calculate new position from original by direction and distance.
    /// (Moving from original to new position follow direction and distance)
    /// </summary>
    /// <param name="original">Original position</param>
    /// <param name="direction">Heading direction from original will be moved (can be Normalize or Not)</param>
    /// <param name="distance">Distance will be moved</param>
    /// <returns></returns>
    private Vector3 MoveByDisDir(Vector3 original, Vector3 direction, float distance)
    {
        Vector3 heading = direction;

        // Percent to calculate heading distance will move to.
        float percentDistance = distance * 100 / heading.magnitude;
        // Debug.Log("Percent: " + percentDistance);

        // Calulate new heading vector by suitable percent.
        heading *= percentDistance / 100;
        // Debug.Log("New Direction: " + heading);

        // Final Position from Original.
        Vector3 finalPosition = original + heading;
        // Debug.Log("Final Position: " + finalPosition);

        return finalPosition;
    }
}
