using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SymJoint
{
    public void CreateJoint(
        GameObject bone,
        GameObject connectedBone,
        float spring,
        float damper,
        bool autoAnchor,
        bool enableCollision
    )
    {
        // If Current Bone has connected with Connected Bone OR reverse, it will not do again.
        if (IsExistedJoint(bone, connectedBone) || IsExistedJoint(connectedBone, bone)) {
            return;
        }
        
        bone.AddComponent<SpringJoint>();
        SpringJoint[] joints = bone.GetComponents<SpringJoint>();
        SpringJoint newJoint = joints[joints.Length - 1];
        newJoint.connectedBody = connectedBone.GetComponent<Rigidbody>();
        newJoint.autoConfigureConnectedAnchor = autoAnchor;
        newJoint.spring = spring;
        newJoint.enableCollision = enableCollision;
        newJoint.damper = damper;

        // Debug.DrawLine(bone.transform.position, connectedBone.transform.position);
        // Debug.Log(bone.transform.position);
        // Debug.Log(connectedBone.transform.position);
        // Debug.Log("------");
    }

    private bool IsExistedJoint(GameObject bone, GameObject connectedBone)
    {
        SpringJoint[] joints = bone.GetComponents<SpringJoint>();
        foreach (SpringJoint joint in joints) {
            if (joint.connectedBody.GetInstanceID() == connectedBone.GetComponent<Rigidbody>().GetInstanceID()) {
                return true;
            }
        }

        return false;
    }
}
