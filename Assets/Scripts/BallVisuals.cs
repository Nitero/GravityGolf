using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class BallVisuals : MonoBehaviour
{
    private bool inSpawnAnim;
    private Rigidbody2D rb;
    private BallMovement movement;
    private SpriteRenderer sprite;
    private TrailRenderer trail;
    private Tween shakeTween;
    private Tween blinkTween;
    private Quaternion fixedRot;
    private HitStop hitStop;
    private Screenshake screenshake;


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




    void LateUpdate()
    {
        // Rotate sprite into the velocity direction
        Vector3 vectorToTarget = rb.velocity;
        float angle = Mathf.Atan2(vectorToTarget.y, vectorToTarget.x) * Mathf.Rad2Deg;
        Quaternion q = Quaternion.AngleAxis(angle - 90, Vector3.forward);
        visualBall.rotation = q;


        // Deform ball scale via velocity if not just hit a wall
        if (!inSpawnAnim && (shakeTween == null || !shakeTween.IsPlaying()))
        {
            //credit: https://github.com/grapefrukt/juicy-breakout/blob/master/src/com/grapefrukt/games/juicy/gameobjects/Ball.as

            var scaleY = 1 + (rb.velocity.magnitude - minVel) / (maxVel - minVel) * velMultiY;
            var scaleX = 1 - (rb.velocity.magnitude - minVel) / (maxVel - minVel) * velMultiX;

            visualBall.localScale = new Vector2(scaleX, scaleY);

            if(!movement.isInsidePortal())
                trail.startWidth = scaleX;

            //TODO: accurate ground hit shake
        }

    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(rb.velocity.magnitude >= dustThreshHold)
            deathDustFX(collision.transform.GetComponent<Collider2D>().bounds.ClosestPoint(transform.position));

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


    public void deathDustFX(Vector3 hitPoint)
    {
        var dirPointToPlayer = hitPoint - transform.position;
        var part = Instantiate(dustPartic, hitPoint, dustPartic.transform.rotation); // Quaternion.Euler(-90 + dirPointToPlayer.z, -90,0));//Quaternion.Euler(transform.rotation.eulerAngles.z - 90, -90, 0));//dustPartic.transform.rotation);

        float angle = Mathf.Atan2(dirPointToPlayer.y, dirPointToPlayer.x) * Mathf.Rad2Deg;
        Quaternion q = Quaternion.AngleAxis(angle + 90, Vector3.forward);
        part.transform.rotation = q;
    }


    // Uppon hitting wall
    public void shakeScale(float vel)
    {
        if (shakeTween == null || !shakeTween.IsPlaying())
        {
            visualBall.localScale = Vector2.one;

            var scale = 1 - (vel / 50f);
            if (scale >= collShakeMax) scale = collShakeMax;
            if (scale <= collShakeMin) scale = collShakeMin;

            shakeTween = visualBall.DOPunchScale(Vector2.one * vel * collisionScaleVelMulti, 0.1f);
            //visualBall.DOShakeScale(0.2f, 0.1f); //* dmgShakeAm
        }
    }

    // Uppon hitting wall fast
    private void blink(float vel)
    {
        if (blinkTween == null || !blinkTween.IsPlaying())
        {
            blinkTween = sprite.DOColor(Color.grey, 0.1f).SetEase(Ease.Flash, 4, 0);
        }
    }

    public void setSpriteEnabled(bool b)
    {
        sprite.enabled = b;
    }

    // Coming out of portal
    public void spawnAnim()
    {
        inSpawnAnim = true;
        sprite.transform.localScale = Vector2.zero;
        setSpriteEnabled(true);

        var seq = DOTween.Sequence();
        seq.Append(sprite.transform.DOScale(Vector2.one, 0.25f));
        seq.Append(sprite.transform.DOPunchScale(Vector2.one * 0.5f, 0.1f));

        seq.AppendCallback(() => GetComponent<BallMovement>().notInTut());
        seq.AppendCallback(() => inSpawnAnim = false);
        seq.AppendCallback(() => GameObject.Find("Click Protection Total").SetActive(false));

    }
}
