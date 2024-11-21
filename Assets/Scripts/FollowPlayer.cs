using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowPlayer : MonoBehaviour
{
    public GameObject player;
    public Vector3 distance;

    // Start is called before the first frame update
    void Start()
    {
        distance = new Vector3(0,0,-3);
    }

    void LateUpdate()
    {
        transform.position = distance + player.transform.position;
    }
}
