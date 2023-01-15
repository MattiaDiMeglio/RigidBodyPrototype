using UnityEngine;

public class WallJump : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            other.GetComponent<TestRbMovement>().InWallJump(transform.position.x);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            other.GetComponent<TestRbMovement>().OutWallJump();
        }
    }
}
