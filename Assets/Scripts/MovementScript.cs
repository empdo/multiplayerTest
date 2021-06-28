using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementScript : MonoBehaviour
{
    public PlayerManager playerManager;
    public float speed = 50;
    public Rigidbody rb;
    // Start is called before the first frame update
    void Start()
    {
       rb = GetComponent<Rigidbody>(); 
    }

    // Update is called once per frame
    void Update()
    {
        float inputX = Input.GetAxis("Horizontal");

        Vector3 movement = new Vector3(speed * inputX, 0, 0);
        rb.AddForce(movement * Time.deltaTime, ForceMode.Impulse);
    }
}
