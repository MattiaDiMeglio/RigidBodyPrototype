using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    [SerializeField] private Transform point;
    private Vector3 startingPos;
    private Vector2 newPos;

    private void Awake()
    {
        startingPos = transform.position;
        newPos.y = startingPos.y;
    }
    private void FixedUpdate()
    {
        newPos.x = Mathf.Lerp(startingPos.x, point.position.x, Mathf.Sin(Time.time * 0.5f));
        transform.position = newPos;
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            other.transform.SetParent(transform);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            other.transform.SetParent(null);
        }
    }
}
