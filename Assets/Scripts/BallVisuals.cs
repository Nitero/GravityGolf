using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class BallVisuals : MonoBehaviour
{
    private Rigidbody2D rb;
    private SpriteRenderer sprite;
    private Tween shakeTween;
    private Tween blinkTween;
    private Quaternion fixedRot;
    private bool inSpawnAnim;
    private TrailRenderer trail;
    private HitStop hitStop;
    private Screenshake screenshake;
    private BallMovement movement;


    [SerializeField] private GameObject dustPartic;
    [SerializeField] private Transform visualBall;
    //[SerializeField] private float velocityDeform = 0.2f;
    [SerializeField] private float minVel = 0;
    [SerializeField] private float maxVel = 10;
    [SerializeField] private float velMultiX = .2f;
    [SerializeField] private float velMultiY = .3f;
    [Space]
    [SerializeField] private float dustThreshHold = 0.25f;
    [SerializeField] private float collisionShakeVelMulti = 0.01f;
    [SerializeField] private float collisionScaleVelMulti = 10f;
    [SerializeField] private float collisionScaleThreshold = 2f;
    [SerializeField] private float collShakeMin = 0.25f;
    [SerializeField] private float collShakeMax = 0.9f;
    [Space]
    //[SerializeField] private float collisionVelColBrightMulti = 10f;
    [SerializeField] private float collisionBlinkHitstopThreshold = 2f;
    


    void Awake()
    {
        fixedRot = transform.rotation;

        rb = GetComponent<Rigidbody2D>();
        movement = GetComponent<BallMovement>();
        sprite = visualBall.GetComponent<SpriteRenderer>();
        trail = GetComponentInChildren<TrailRenderer>();
        hitStop = FindObjectOfType<HitStop>();
        screenshake = FindObjectOfType<Screenshake>();
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
        if (!inSpawnAnim && (shakeTween == null || !shakeTween.IsPlaying()))
        {
            //var deform = rb.velocity.normalized;
            ////var locVel = transform.InverseTransformDirection(rb.velocity);
            ////var deform = locVel.normalized; //(for when the object to deform rotates... here is is axis alligned)
            //deform *= velocityDeform;

            //deform.x = Mathf.Abs(deform.x);
            //deform.y = Mathf.Abs(deform.y);

            //visualBall.localScale = Vector2.one + deform;



            // Y always points to velocity dir, so don't need velocity direction, just magnitude 
            /*
            var currSpeed = rb.velocity.magnitude;


            float Strength = 15f;
            float Dampening = 0.3f;
            float VelocityStretch = 0.25f;
            float _squash = 0;
            float _squashVelocity = 0;

            //Calculate the desired squash amount based on the current Y axis velocity.
            //float targetSquash = -Mathf.Abs(velocity.y) * VelocityStretch;
            float targetSquash = currSpeed * VelocityStretch;

            //Adjust the squash velocity.
            //_squashVelocity += (targetSquash - _squash) * Strength * Time.deltaTime;

            //Apply dampening to the squash velocity.
            //_squashVelocity = ((_squashVelocity / Time.deltaTime) * (1f - Dampening)) * Time.deltaTime;

            //Apply the velocity to the squash value.
            // _squash += _squashVelocity;


            if (targetSquash <= 1)
                targetSquash = 1;
            visualBall.localScale = new Vector2(1, targetSquash);//_squash);
            */

            //TODO: accurate ground


            //https://github.com/grapefrukt/juicy-breakout/blob/master/src/com/grapefrukt/games/juicy/gameobjects/Ball.as

            var scaleY = 1 + (rb.velocity.magnitude - minVel) / (maxVel - minVel) * velMultiY;
            var scaleX = 1 - (rb.velocity.magnitude - minVel) / (maxVel - minVel) * velMultiX;

            visualBall.localScale = new Vector2(scaleX, scaleY);

            if(!movement.isInsidePortal())
                trail.startWidth = scaleX;
        }

    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(rb.velocity.magnitude >= dustThreshHold)
            dust(collision.transform.GetComponent<Collider2D>().bounds.ClosestPoint(transform.position));


        if (rb.velocity.magnitude >= collisionScaleThreshold)
            shakeScale(rb.velocity.magnitude);


        if (rb.velocity.magnitude >= collisionBlinkHitstopThreshold)
        {
            hitStop.Stop(0.05f); // 0.5 too much, 0.1 nothing?
            blink(rb.velocity.magnitude);
        }

        shake(rb.velocity.normalized, rb.velocity.magnitude * collisionShakeVelMulti);
    }

    public void shake(Vector2 dir, float am)
    {
        screenshake.AddShake(dir, am);
    }

    public void boundShake(Vector2 dir, float ballVel)
    {
        screenshake.AddShake(dir, ballVel * collisionShakeVelMulti);
    }


    public void dust(Vector3 hitPoint)
    {
        var dirPointToPlayer = hitPoint - transform.position;
        var part = Instantiate(dustPartic, hitPoint, dustPartic.transform.rotation); // Quaternion.Euler(-90 + dirPointToPlayer.z, -90,0));//Quaternion.Euler(transform.rotation.eulerAngles.z - 90, -90, 0));//dustPartic.transform.rotation);

        float angle = Mathf.Atan2(dirPointToPlayer.y, dirPointToPlayer.x) * Mathf.Rad2Deg;
        Quaternion q = Quaternion.AngleAxis(angle + 90, Vector3.forward);
        part.transform.rotation = q;
        //part.transform.eulerAngles = new Vector3(part.transform.rotation.x, -90,0);
    }


    public void shakeScale(float vel)
    {
        if (shakeTween == null || !shakeTween.IsPlaying())
        {
            visualBall.localScale = Vector2.one;

            var scale = 1 - (vel / 50f);
            if (scale >= collShakeMax) scale = collShakeMax;
            if (scale <= collShakeMin) scale = collShakeMin;

            //shakeTween = visualBall.DOShakeScale(0.2f, scale); //* dmgShakeAm
            shakeTween = visualBall.DOPunchScale(Vector2.one * vel * collisionScaleVelMulti, 0.1f); //* dmgShakeAm
            //visualBall.DOShakeScale(0.2f, 0.1f); //* dmgShakeAm
        }
    }

    private void blink(float vel)
    {
        if (blinkTween == null || !blinkTween.IsPlaying())
        {
            /*
            var orgColor = sprite.color;
            var brightColor = sprite.color;
            brightColor.r += vel;
            brightColor.g += vel;
            brightColor.b += vel;

            blinkTween = DOTween.Sequence();
            blinkTween.Append(sprite.DOColor(brightColor, 0.1f));
            */

            //blinkTween = sprite.DOColor(Color.grey, 0.25f).SetEase(Ease.Flash, 4, 1);
            blinkTween = sprite.DOColor(Color.grey, 0.1f).SetEase(Ease.Flash, 4, 0);
        }
    }

    public void setSprite(bool b)
    {
        sprite.enabled = b;
    }

    public void spawnAnim()
    {
        inSpawnAnim = true;
        sprite.transform.localScale = Vector2.zero;
        setSprite(true);

        var seq = DOTween.Sequence();
        seq.Append(sprite.transform.DOScale(Vector2.one, 0.25f));
        seq.Append(sprite.transform.DOPunchScale(Vector2.one * 0.5f, 0.1f));

        //seq.AppendInterval(0.25f); //so that no false inpit

        seq.AppendCallback(() => GetComponent<BallMovement>().notInTut());
        seq.AppendCallback(() => inSpawnAnim = false);
        seq.AppendCallback(() => GameObject.Find("Click Protection Total").SetActive(false));

    }
}
