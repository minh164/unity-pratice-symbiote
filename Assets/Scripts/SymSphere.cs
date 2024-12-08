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
    
        Spherize();
        
        // Add collider for all bones.
        AddColliderForBones(colliderSize);
    }

    protected override void AddColliderForBones(float colliderSize)
    {
        foreach (GameObject bone in symBone.GetBones()) {
            AddCollider(bone, colliderSize, "sphere");
        }
    }

    public float GetRadius()
    {
        double volume = (double) GetVolume();
        double exponent = 1 / 3d;

        double doubleValue = Math.Pow(
            3 * volume / (4 * Math.PI),
            exponent
        );

        return (float) doubleValue;
    }

    private void Spherize()
    {
        float radius = GetRadius();

        symBone.SpherizeBones(radius);
        symVertex.SpherizeVectors(radius);
    }
}
