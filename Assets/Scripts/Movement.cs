using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour
{
    [SerializeField]
    private float movingSpeed = 3;
    [SerializeField]
    private float jumpForce = 1;
    private float horizontal;
    private float vertical;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        horizontal = Input.GetAxis("Horizontal");
        vertical = Input.GetAxis("Vertical");

        transform.Translate(new Vector3(
            Time.deltaTime * horizontal * movingSpeed,
            0,
            Time.deltaTime * vertical * movingSpeed
        ), Space.World);
    }

    // Update is called once per fixed delta time (0.02).
    void FixedUpdate()
    {
        Rigidbody rigid = gameObject.GetComponent<Rigidbody>();
        if (Input.GetButton("Jump")) {
            rigid.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }             
    }
}
