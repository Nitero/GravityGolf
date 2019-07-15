using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DynamicObject : MonoBehaviour
{
    private Vector3 startPos;
    private Vector3 startOrient;

    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        startPos = transform.position;
        startOrient = transform.eulerAngles;
    }

    void Update()
    {
        
    }

    public void reset()
    {
        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0;
        transform.position = startPos;
        transform.eulerAngles = startOrient;
    }
}
