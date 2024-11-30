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
        base.Create();

        // if (spherize) {
        //     symBone.SpherizeBones();
        //     symVertex.SpherizeVectors();
        // }
    }

    protected override void GenerateSide(string side)
    {
    }

    protected override void GenerateCross(bool rightSide = false)
    {
    }

    protected override void AddColliderForBones(float colliderSize)
    {
        foreach (GameObject bone in symBone.GetBones()) {
            AddCollider(bone, colliderSize, "sphere");
        }
    }
}
