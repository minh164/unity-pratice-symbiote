using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SymBone
{
    private Symbiote _symbiote;
    private GameObject[] _bones = new GameObject[]{};
    private SkinnedMeshRenderer _rend;
    private Mesh _mesh;

    public SymBone(
        Symbiote symbiote,
        GameObject[] bones,
        SkinnedMeshRenderer renderer,
        Mesh mesh
    )
    {
        _symbiote = symbiote;
        _bones = bones;
        _rend = renderer;
        _mesh = mesh;
    }

    public GameObject[] GetBones()
    {
        return _bones;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public GameObject GetBoneByIndex(int index)
    {
        return _bones[index];
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="position"></param>
    /// <returns></returns>
    public GameObject GetBoneByPosition(Vector3 position)
    {
        return _bones[FindBoneIndexByPosition(position)];
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public GameObject GetLastBone()
    {
        return _bones[_bones.Length - 1];
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public int CountBones()
    {
        return _bones.Count();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="position"></param>
    /// <returns></returns>
    public int FindBoneIndexByPosition(Vector3 position)
    {
        float[] xRange = new float[2];
        float[] yRange = new float[2];
        float[] zRange = new float[2];
        if (_symbiote.isRelativePosition) {
            float relativeValue = _symbiote.relativePositionValue;
            xRange = new float[2] {position.x - relativeValue, position.x + relativeValue};
            yRange = new float[2] {position.y - relativeValue, position.y + relativeValue};
            zRange = new float[2] {position.z - relativeValue, position.z + relativeValue};
        }

        int boneIndex = -1;
        for (int i = 0; i < _bones.Length; i++) {
            GameObject bone = GetBoneByIndex(i);
            Vector3 bonePosition = bone.transform.localPosition;

            if (_symbiote.isRelativePosition) {
                if (
                    bonePosition.x >= xRange[0] && bonePosition.x <= xRange[1]
                    && bonePosition.y >= yRange[0] && bonePosition.y <= yRange[1]
                    && bonePosition.z >= zRange[0] && bonePosition.z <= zRange[1]
                ) {
                    boneIndex = i;
                    break;
                }
            } else {
                // Find latest bone with same position (if there are).
                if (bonePosition != position) {
                    continue;
                }
                boneIndex = i;
            }
        }

        return boneIndex;
    }

    /// <summary>
    /// If any bone has same position which need be to created,
    /// return it instead of create new one.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="position"></param>
    /// <param name="colliderSize"></param>
    /// <returns></returns>
    public int FindOrCreateBoneIndex(string name, Vector3 position, float colliderSize = 0.2f, bool freezePos = false)
    {
        int boneIndex = FindBoneIndexByPosition(position);
        if (boneIndex >= 0) {
            return boneIndex;
        }

        return CreateBoneIndex(name, position, colliderSize, freezePos);
    }

    public int CreateBoneIndex(string name, Vector3 position, float colliderSize = 0.2f, bool freezePos = false)
    {
        Transform[] tranBones = new Transform[1];
        Matrix4x4[] bindposes = new Matrix4x4[1];

        GameObject bone = new GameObject(name);

        // Set bone's scale equals parent's scale.
        bone.transform.localScale = _symbiote.transform.localScale;
        
        tranBones[0] = bone.transform;
        tranBones[0].parent = _symbiote.transform;
        // Set the position relative to the parent
        tranBones[0].localRotation = Quaternion.identity;
        tranBones[0].localPosition = position;
        
        bindposes[0] = tranBones[0].worldToLocalMatrix * _symbiote.transform.localToWorldMatrix;

        _mesh.bindposes = _mesh.bindposes.Concat(bindposes).ToArray();
        _rend.bones = _rend.bones.Concat(tranBones).ToArray();

        // Add rigid for bone.
        bone.AddComponent<Rigidbody>();
        Rigidbody rigidbody = bone.GetComponent<Rigidbody>();
        if (freezePos) {
            rigidbody.constraints = RigidbodyConstraints.FreezePosition;
        }

        // Add collider.
        // AddCollider(bone, colliderSize);

        GameObject[] boneList = new GameObject[] {
            bone
        };
        _bones = _bones.Concat(boneList).ToArray();
        
        return _bones.Length - 1;
    }

    /// <summary>
    /// Set a bone to a weight.
    /// </summary>
    /// <param name="boneIndex"></param>
    /// <param name="weightIndex"></param>
    /// <param name="boneNumber"></param>
    public void SetBoneToWeight(int boneIndex, int weightIndex, int boneNumber = 0)
    {
        // Must clone bone weights from mesh to update a specified weight element by index.
        BoneWeight[] cloneWeights = _mesh.boneWeights;
        BoneWeight weight = cloneWeights[weightIndex];

        // Auto set bone to empty bone number.
        if (boneNumber == 0) {
            if (weight.boneIndex0 < 0) {
                // Center bone has value is 0.
                boneNumber = 1;
            }
            else if (weight.boneIndex1 <= 0) {
                boneNumber = 2;
            }
            else if (weight.boneIndex2 <= 0) {
                boneNumber = 3;
            }
            else if (weight.boneIndex3 <= 0) {
                boneNumber = 4;
            }
        }

        switch (boneNumber) {
            case 1:
                weight.boneIndex0 = boneIndex;
                break;
            case 2:
                weight.boneIndex1 = boneIndex;
                break;
            case 3:
                weight.boneIndex2 = boneIndex;
                break;
            case 4:
                weight.boneIndex3 = boneIndex;
                break;
        }

        float boneSum = 0;
        if (weight.boneIndex0 >= 0) {
            // Center bone has value is 0.
            boneSum++;
        }
        if (weight.boneIndex1 > 0) {
            boneSum++;
        }
        if (weight.boneIndex2 > 0) {
            boneSum++;
        }
        if (weight.boneIndex3 > 0) {
            boneSum++;
        }

        // Recalculate each weight value.
        float weightValue = 1 / boneSum;

        switch (boneSum) {
            case 1:
                weight.weight0 = weightValue;
                break;
            case 2:
                weight.weight0 = weightValue;
                weight.weight1 = weightValue;
                break;
            case 3:
                weight.weight0 = weightValue;
                weight.weight1 = weightValue;
                weight.weight2 = weightValue;
                break;
            case 4:
                weight.weight0 = weightValue;
                weight.weight1 = weightValue;
                weight.weight2 = weightValue;
                weight.weight3 = weightValue;
                break;
        }

        cloneWeights[weightIndex] = weight;
        _mesh.boneWeights = cloneWeights;
    }

    public void SpherizeBones(float radius)
    {
        for(int i = 0; i<_bones.Length; i++)
        {
            GameObject bone = _bones[i];
            Vector3 normalize = bone.transform.localPosition.normalized;
            bone.transform.localPosition = new Vector3(
                normalize.x * radius,
                normalize.y * radius,
                normalize.z * radius
            );

            _bones[i] = bone;
            _rend.bones[i] = bone.transform;
        }
    }
}
