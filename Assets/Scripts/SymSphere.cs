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
        
    }

    protected override void Create()
    {
        // Add Center bone.
        centerBone = GenerateCell(Vector3.zero, "0");

        GenerateSide("front", true);
        GenerateSide("right", true);
        GenerateSide("top", true);
    
        symBone.SpherizeBones();
        symVertex.SpherizeVectors();

        // Add collider for all bones.
        AddColliderForBones(colliderSize);
    }

    protected override void AddColliderForBones(float colliderSize)
    {
        foreach (GameObject bone in symBone.GetBones()) {
            AddCollider(bone, colliderSize, "sphere");
        }
    }
}
