using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class DynamicObject : MonoBehaviour
{
    public bool canBeSucked;

    private Vector3 startPos;
    private Vector3 startOrient;
    private Vector3 startScale;
    private float startMass;

    private Rigidbody2D rb;

    void Awake() //Start
    {
        rb = GetComponent<Rigidbody2D>();

        startPos = transform.position;
        startOrient = transform.eulerAngles;
        startScale = transform.localScale;
        startMass = rb.mass;
    }

    void Update()
    {
        
    }

    public float getMagnitude()
    {
        return rb.velocity.magnitude;
    }

    public void reset()
    {
        transform.DOKill();
        rb.velocity = Vector2.zero;
        rb.mass = startMass;
        rb.angularVelocity = 0;
        gameObject.layer = LayerMask.NameToLayer("Default");
        rb.gravityScale = 1;
        transform.localScale = startScale;
        transform.position = startPos;
        transform.eulerAngles = startOrient;
    }


    public void hitGoal(float shrinkDur, Ease ease, int objectRotSpeed)
    {
        rb.gravityScale = 0;
        rb.mass = 5; //for save suck

        //GetComponent<Collider2D>().enabled = false; //doesn't work bcz goal can't suck you in anymore
        gameObject.layer = LayerMask.NameToLayer("Collide With Nothing");


        transform.DOScale(Vector3.zero, shrinkDur).SetEase(ease);
        var dir = Random.Range(0, 2) > 0 ? 1 : -1;
        transform.DORotate(new Vector3(0,0,transform.rotation.z + objectRotSpeed * dir), 1f).SetLoops(-1);

        //Destroy(gameObject, shrinkDur); // need to respawn
    }
}
