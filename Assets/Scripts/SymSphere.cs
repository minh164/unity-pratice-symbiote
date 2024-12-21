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
        // Debug.Log(Vector3.Distance(centerBone.transform.localPosition, symBone.GetBoneByIndex(10).transform.localPosition));
    }

    // Update is called once per frame
    void Update()
    {
        symDebug.DrawBoneConnections(symBone.GetBones());
    }

    protected override void Create()
    {
        // Add Center bone.
        centerBone = GenerateCell(Vector3.zero, "0");

        GenerateSide("front", true);
        GenerateSide("right", true);
        GenerateSide("top", true);
        GenerateCross();
        GenerateCross(true);

        // ConnectWithCenterCell();
    
        // Add collider for all bones.
        AddColliderForBones(colliderSize);
    }

    protected override GameObject GenerateSideCell(Vector3 position, string name)
    {
        // Spherize position of vertex and bone in initialization.
        position = SpherizePosition(position);

        return GenerateCell(position, name);
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

    private void ConnectWithCenterCell()
    {
        foreach (var cell in cells) {
            // Skip cell index is zero, it's Center Cell.
            if (cell.Key == 0) {
                continue;
            }

            GameObject bone = symBone.GetBoneByIndex(cell.Value);

            symJoint.CreateJoint(
                bone, centerBone,
                spring, damper, autoAnchor, enableCollision
            );
        }
    }
}
