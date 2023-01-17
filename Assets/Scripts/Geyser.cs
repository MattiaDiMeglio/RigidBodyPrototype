using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Geyser : MonoBehaviour
{
    [SerializeField] private float _upForce = 20f;

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player") && other.GetComponent<TestRbMovement>().IsGliding)
        {
            other.GetComponent<Rigidbody>().AddForce(Vector2.up * (_upForce - other.GetComponent<Rigidbody>().velocity.y) * 2f, ForceMode.Acceleration);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player") && other.GetComponent<TestRbMovement>().IsGliding)
        {
            other.GetComponent<Rigidbody>().AddForce(Vector2.up * (_upForce - other.GetComponent<Rigidbody>().velocity.y), ForceMode.Acceleration);
        }
    }
}
