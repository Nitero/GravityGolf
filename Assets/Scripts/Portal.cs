using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Portal : MonoBehaviour
{

    [SerializeField] private bool isGoal = true;
    [SerializeField] private bool doesSuck = true;
    [SerializeField] private bool doesTeleport;
    [SerializeField] private Transform teleportConnection;
    [SerializeField] private float canTPbackDelay = 0.25f;
    [Space]
    [SerializeField] private float ballLessMagnitudeStop = 0.5f;
    [SerializeField] private float objLessMagnitudeStop = 2f; // objects are heavier...

    [Header("Animation")]
    [SerializeField] private float ballShrinkDur = 1f;
    [SerializeField] private Ease ballShrinkEase = Ease.Linear;
    [SerializeField] private float objShrinkDur = 1f;
    [SerializeField] private Ease objShrinkEase = Ease.Linear;
    [SerializeField] private int objRotSpeed = 180;

    [Space]

    [SerializeField] private float spinSpeed = 1f;
    [SerializeField] private float shrinkDur = 1f;
    [SerializeField] private Ease shrinkEase = Ease.Linear;
    [Space]
    [SerializeField] private float[] spinSpeeds = new float[] {45, 90, 180 };
    [SerializeField] private float[] circleSpeeds = new float[] {1, 2, 5 };
    [SerializeField] private float portalScaleShrinkDur = 1.25f;
    [SerializeField] private float portalScaleExpandDur = 2f;
    [SerializeField] private float scaleDivider = 0.7f;
    [SerializeField] private float scaleOffset = 0.25f;
    [SerializeField] private int portalScaleVib = 0;
    [SerializeField] private float portalScaleElast = 10;
    [Space]
    [SerializeField] private Transform spriteParent;

    private BallVisuals ballVisuals;

    void Start()
    {
        StartCoroutine(startPortalAnim());
        ballVisuals = FindObjectOfType<BallVisuals>();
    }

    // Start scaling portal
    private IEnumerator startPortalAnim()
    {
        for (int i = 0; i < spriteParent.childCount; i++)
        {
            Sequence seq = DOTween.Sequence().SetLoops(-1);
            seq.Append(spriteParent.GetChild(i).transform.DOScale(spriteParent.GetChild(i).transform.localScale * scaleDivider, portalScaleShrinkDur).SetEase(Ease.InOutSine));
            seq.Append(spriteParent.GetChild(i).transform.DOScale(spriteParent.GetChild(i).transform.localScale, portalScaleExpandDur).SetEase(Ease.Linear));

            yield return new WaitForSeconds(scaleOffset);
        }
    }


    // Start spinning portal
    private void LateUpdate()
    {
        for (int i = 0; i < spriteParent.childCount; i++)
        {
            spriteParent.GetChild(i).Rotate(new Vector3(0, 0, -spinSpeeds[i] * Time.deltaTime));

            Vector3 axis = new Vector3(0, 0, -1);
            transform.RotateAround(spriteParent.GetChild(i).transform.position, axis, Time.deltaTime * circleSpeeds[i]);
        }
    }



    // Don't instantly go back through teleportation portal
    private IEnumerator reactivateTrigger(float delay, Collider2D portal, Collider2D other)
    {
        yield return new WaitForSeconds(delay);
        if(other != null)
            Physics2D.IgnoreCollision(other, portal, false);
    }


    // Teleport portal is instant
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!doesSuck) return;

        if (doesTeleport)
        {
            var vel = collision.GetComponent<BallMovement>().getMagnitude();
            if (vel >= 1f)
            {
                suckEffect();
                teleportConnection.GetComponent<Portal>().suckEffect();

                ballVisuals.boundShake(collision.GetComponent<Rigidbody2D>().velocity.normalized, vel);
            }

            collision.transform.position = teleportConnection.position;
            collision.GetComponentInChildren<TrailRenderer>().Clear();
            Physics2D.IgnoreCollision(collision, teleportConnection.GetComponent<Collider2D>());
            StartCoroutine(reactivateTrigger(canTPbackDelay, teleportConnection.GetComponent<Collider2D>(), collision));
        }
    }

    // Non teleport portals wait for thing to lose speed
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (!doesSuck) return;

        if(!doesTeleport)
        {
            var ball = collision.GetComponent<BallMovement>();
            if (ball != null)
            {
                if (ball.getMagnitude() < ballLessMagnitudeStop)
                {
                    // Player safely inside goal, can't jump out anymore

                    ball.didHitGoal(ballShrinkDur, ballShrinkEase, isGoal);

                    Invoke("suckEffect", ballShrinkDur / 1.25f);
                }
            }

            var obj = collision.GetComponent<DynamicObject>();
            if (obj != null)
            {
                if (obj.canBeSucked && obj.getMagnitude() < objLessMagnitudeStop)
                {
                    // Object safely inside goal, can't jump out anymore

                    obj.hitPortal(objShrinkDur, objShrinkEase, objRotSpeed);

                    Invoke("suckEffect", objShrinkDur / 2);
                }
            }
        }    
    }


    // Swallowed object/ player
    private void suckEffect()
    {
        spriteParent.parent.DOKill(); //prevent permanent deform

        spriteParent.parent.DOShakeScale(0.25f, 0.75f, 10);
        //visualParent.DOPunchScale(new Vector2(-0.5f, -0.5f), 1f, 3);
    }

    // Swallow by teleport portal, less strong for less annoying
    private void suckEffectLess()
    {
        spriteParent.parent.DOKill(); //prevent permanent deform

        spriteParent.parent.DOShakeScale(0.25f, 0.5f, 10);

        //TODO: more dynamic depending on speed
    }


    // Spit out player
    public void spawnAnim()
    {
        GameObject.Find("Canvas").transform.GetChild(GameObject.Find("Canvas").transform.childCount-1).gameObject.SetActive(true); //bad workaround
        FindObjectOfType<TrailRenderer>().enabled = false;

        var player = GameObject.FindGameObjectWithTag("Player").GetComponent<BallMovement>();
        player.hideSprite();

        var seq = DOTween.Sequence();
        seq.AppendInterval(0.5f);
        seq.Append(spriteParent.parent.DOPunchScale(Vector2.one * 0.5f, .5f)); //+ screen shake?
        seq.InsertCallback(0.6f, () => player.spawnFromPortal(transform.position));
        //seq.Join(visualParent.DOScale(Vector2.one * 1.5f, .5f));
        seq.AppendInterval(0.5f);
        seq.Append(spriteParent.parent.DOShakeScale(.25f));
        seq.Join(spriteParent.parent.DOScale(Vector2.zero, .25f));

        seq.AppendCallback(() => FindObjectOfType<TrailRenderer>().enabled = true);
        seq.AppendInterval(2f);
        seq.AppendCallback(() => Destroy(gameObject));
    }
}
