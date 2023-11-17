using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cartridge : MonoBehaviour
{
    public Rigidbody rigidBody;
    public float jumpPower;

    void Start()
    {
        rigidBody.AddTorque(new Vector3(Random.Range(-100, 100), Random.Range(-100, 100), Random.Range(-5, 5)) * jumpPower, ForceMode.Impulse);
        rigidBody.AddForce(transform.TransformDirection(new Vector3(3, 3, 0)) * jumpPower, ForceMode.Impulse);
        
        Destroy(gameObject, 5f);
    }
}
