using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class BallVisuals : MonoBehaviour
{
    private Rigidbody2D rb;
    private SpriteRenderer sprite;
    private Tween shakeTween;
    private Quaternion fixedRot;

    [SerializeField] private Transform visualBall;
    [SerializeField] private float velocityDeform = 0.2f;
    [SerializeField] private float collisionShakeVelMulti = 10f;
    [SerializeField] private float collisionShakeThreshold = 2f;
    [Space]
    [SerializeField] private float collShakeMin = 0.25f;
    [SerializeField] private float collShakeMax = 0.9f;


    void Awake()
    {
        fixedRot = transform.rotation;

        rb = GetComponent<Rigidbody2D>();
        sprite = visualBall.GetComponent<SpriteRenderer>();
    }

    void Update()
    {

    }

    void LateUpdate()
    {
        //fix rotation of sprite, parent with collider can still roll
        //visualBall.transform.rotation = fixedRot;

        //nevermind, rotate sprite into the velocity direction
        //Vector3 vectorToTarget = targetTransform.position - transform.position;
        Vector3 vectorToTarget = rb.velocity;
        float angle = Mathf.Atan2(vectorToTarget.y, vectorToTarget.x) * Mathf.Rad2Deg;
        Quaternion q = Quaternion.AngleAxis(angle - 90, Vector3.forward);
        visualBall.rotation = q;// Quaternion.Slerp(transform.rotation, q, Time.deltaTime * speed);



        // PAUSE THIS WHEN SCALE PLAYER BY DAMAGE (func below)
        if (shakeTween == null || !shakeTween.IsPlaying())
        {
            /*
            var deformMargin = rb.velocity.magnitude / maxVelocity;
            if (deformMargin >= 1) deformMargin = 1;
            if (deformMargin <= 0) deformMargin = 0;
            print(deformMargin);
            transform.localScale = new Vector2(1 - velocityDeform * deformMargin, 1 + velocityDeform * deformMargin);
            */


            var deform = rb.velocity.normalized;
            //var locVel = transform.InverseTransformDirection(rb.velocity);
            //var deform = locVel.normalized; //(for when the object to deform rotates... here is is axis alligned)
            deform *= velocityDeform;


            deform.x = Mathf.Abs(deform.x);
            deform.y = Mathf.Abs(deform.y);

            visualBall.localScale = Vector2.one + deform;
            //visuals.localScale = Vector2.one + new Vector2(transform.up.x * deform.x, transform.up.y * deform.y);
        }

    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        return;

        if (rb.velocity.magnitude >= collisionShakeThreshold)
            shakeScale(rb.velocity.magnitude * collisionShakeVelMulti);


    }



    public void shakeScale(float vel)
    {
        if (shakeTween == null || !shakeTween.IsPlaying())
        {
            var scale = 1 - (vel / 50f);
            if (scale >= collShakeMax) scale = collShakeMax;
            if (scale <= collShakeMin) scale = collShakeMin;

            shakeTween = visualBall.DOShakeScale(1 - scale, scale); //* dmgShakeAm
        }
    }
}
