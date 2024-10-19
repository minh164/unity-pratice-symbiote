using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameGod : MonoBehaviour
{
    [SerializeField]
    private GameObject Symbiote;
    [SerializeField]
    private Vector3 initPosition;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyUp(KeyCode.R)) {
            Destroy(GameObject.FindWithTag("Player"));
            Instantiate(Symbiote, initPosition, Symbiote.transform.rotation);
        }
    }
}
