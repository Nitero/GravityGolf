using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class DynamicObject : MonoBehaviour
{
    public bool canBeSucked; //Into portal
    [Space]
    [Header("Move Object")]
    [SerializeField] private bool doesMovePattern;
    [SerializeField] private AnimationCurve moveEase;
    [SerializeField] private int initialDir;
    [SerializeField] private float moveSpeed;
    [SerializeField] private Vector3 startMovePos;
    [SerializeField] private Vector3 endMovePos;

    private float moveTimer;
    private int moveDir;

    private Vector3 startPos;
    private Vector3 startOrient;
    private Vector3 startScale;
    private float startMass;

    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        // Save state for restart

        startPos = transform.position;
        startOrient = transform.eulerAngles;
        startScale = transform.localScale;
        startMass = rb.mass;

        // Calc pattern
        if (doesMovePattern)
        {
            startMovePos = startPos + startMovePos;
            endMovePos = startPos + endMovePos;

            moveTimer = getPercentageBetweenVectors(startPos, startMovePos, endMovePos); //0.5f; //startTimerDistance; 

            moveDir = initialDir;
        }
    }

    void Update()
    {
        if (doesMovePattern)
        {
            moveTimer += Time.deltaTime * moveDir * moveSpeed;
            if (moveTimer >= 1) moveDir = -1;
            if (moveTimer <= 0) moveDir =  1;
        }
    }

    void FixedUpdate()
    {
        if(doesMovePattern)
        {
            transform.position = Vector2.Lerp(startMovePos, endMovePos, moveEase.Evaluate(moveTimer));
        }
    }

    public float getMagnitude()
    {
        return rb.velocity.magnitude;
    }

    public void reset() // On restart
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

        if (doesMovePattern)
        {
            moveTimer = getPercentageBetweenVectors(startPos, startMovePos, endMovePos); 
            moveDir = initialDir;
        }
    }


    public void hitGoal(float shrinkDur, Ease ease, int objectRotSpeed)
    {
        rb.gravityScale = 0;
        rb.mass = 5; // easier sucked into portal

        // Switch layer so you doesn't kick out player
        gameObject.layer = LayerMask.NameToLayer("Collide With Nothing");

        // Anim
        transform.DOScale(Vector3.zero, shrinkDur).SetEase(ease);
        var dir = Random.Range(0, 2) > 0 ? 1 : -1;
        transform.DORotate(new Vector3(0,0,transform.rotation.z + objectRotSpeed * dir), 1f).SetLoops(-1);
    }


    private float getPercentageBetweenVectors(Vector2 vMid, Vector2 v1, Vector2 v2)
    {
        float totalLength = Vector2.Distance(v1, v2);
        float midToStartLength = Vector2.Distance(vMid, v1);

        return midToStartLength / totalLength;
    }
}
