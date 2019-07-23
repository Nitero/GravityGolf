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


    void Update()
    {
        
    }


    private IEnumerator startPortalAnim()
    {
        //portal.transform.DORotate(new Vector3(0,0,transform.rotation.z + 1), portalRotateSpeed).SetLoops(-1);
        for (int i = 0; i < spriteParent.childCount; i++)
        {
            //portal.transform.DOShakeScale(portalScaleDur, portalScaleStr, portalScaleVib, portalScaleRand); //SetLoops(-1)
            //portal.transform.DOPunchScale(punchVec, portalScaleDur, portalScaleVib, portalScaleElast).SetLoops(-1);
            Sequence seq = DOTween.Sequence().SetLoops(-1);
            seq.Append(spriteParent.GetChild(i).transform.DOScale(spriteParent.GetChild(i).transform.localScale * scaleDivider, portalScaleShrinkDur).SetEase(Ease.InOutSine));
            seq.Append(spriteParent.GetChild(i).transform.DOScale(spriteParent.GetChild(i).transform.localScale, portalScaleExpandDur).SetEase(Ease.Linear));

            yield return new WaitForSeconds(scaleOffset);
        }

        //visualParent.GetChild(2).transform.domovearound(visualParent.GetChild(i).transform.localScale * scaleDivider, portalScaleShrinkDur).SetEase(Ease.InOutSine));
    }

    private void LateUpdate()
    {
        for (int i = 0; i < spriteParent.childCount; i++)
        {
            spriteParent.GetChild(i).Rotate(new Vector3(0, 0, -spinSpeeds[i] * Time.deltaTime));

            Vector3 axis = new Vector3(0, 0, -1);
            transform.RotateAround(spriteParent.GetChild(i).transform.position, axis, Time.deltaTime * circleSpeeds[i]);
        }
    }




    private IEnumerator reactivateTrigger(float delay, Collider2D portal, Collider2D other)
    {
        yield return new WaitForSeconds(delay);
        if(other != null)
            Physics2D.IgnoreCollision(other, portal, false);
    }

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
                    ball.didHitGoal(ballShrinkDur, ballShrinkEase, isGoal);



                    //print("GOAL");

                    //TODO: konfetti !?


                    Invoke("suckEffect", ballShrinkDur / 1.25f);
                }
            }

            var obj = collision.GetComponent<DynamicObject>();
            if (obj != null)
            {
                if (obj.canBeSucked && obj.getMagnitude() < objLessMagnitudeStop)
                {
                    obj.hitGoal(objShrinkDur, objShrinkEase, objRotSpeed);

                    Invoke("suckEffect", objShrinkDur / 2);
                }
            }
        }    
    }

    private void suckEffectLess() //TODO: more dynamic depending on speed
    {
        spriteParent.parent.DOKill(); //prevent permanent deform... better would be preserve original scale and go back to it when no tweens anymore

        spriteParent.parent.DOShakeScale(0.25f, 0.5f, 10);
        //visualParent.DOPunchScale(new Vector2(-0.5f, -0.5f), 1f, 3);
    }

    private void suckEffect()
    {
        spriteParent.parent.DOKill(); //prevent permanent deform... better would be preserve original scale and go back to it when no tweens anymore

        spriteParent.parent.DOShakeScale(0.25f, 0.75f, 10);
        //visualParent.DOPunchScale(new Vector2(-0.5f, -0.5f), 1f, 3);
    }


    public void spawnAnim()
    {
        //GameObject.Find("Click Protection Total").SetActive(true);
        GameObject.Find("Canvas").transform.GetChild(GameObject.Find("Canvas").transform.childCount-1).gameObject.SetActive(true); //bad workaround
        FindObjectOfType<TrailRenderer>().enabled = false;

        var player = GameObject.FindGameObjectWithTag("Player").GetComponent<BallMovement>();
        //player.transform.position = transform.position;
        player.hideSprite();

        /*
        var seq = DOTween.Sequence();
        seq.AppendInterval(0.5f);
        seq.Append(visualParent.DOScale(Vector2.one * 3, .5f));
        seq.Append(visualParent.DOShakeScale(.25f)); //shake?
        seq.Append(visualParent.DOScale(Vector2.one * 1.5f, .5f));
        seq.AppendInterval(0.5f);
        seq.Append(visualParent.DOScale(Vector2.zero, .25f));
        seq.Append(visualParent.DOPunchScale(-Vector2.one * 1f, .25f));
        */

        var seq = DOTween.Sequence();
        seq.AppendInterval(0.5f);
        seq.Append(spriteParent.parent.DOPunchScale(Vector2.one * 0.5f, .5f)); //shake?
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
