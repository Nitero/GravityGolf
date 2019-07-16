using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Goal : MonoBehaviour
{
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

    void Start()
    {
        
    }


    void Update()
    {
        
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        var ball = collision.GetComponent<BallMovement>();
        if (ball != null)
        {
            if (ball.getMagnitude() < ballLessMagnitudeStop)
            {
                ball.didHitGoal(ballShrinkDur, ballShrinkEase);

                print("GOAL");

                //TODO: konfetti !?

                Invoke("suckEffect", ballShrinkDur/2);
            }
        }

        var obj = collision.GetComponent<DynamicObject>();
        if (obj != null)
        {
            if (obj.canBeSucked && obj.getMagnitude() < objLessMagnitudeStop)
            {
                obj.hitGoal(objShrinkDur, objShrinkEase, objRotSpeed);

                Invoke("suckEffect", objShrinkDur/2);
            }
        }
    }

    private void suckEffect()
    {
        transform.DOShakeScale(0.25f, 0.75f, 10);
        //transform.DOPunchScale(new Vector2(-0.5f, -0.5f), 1f, 3);
    }
}
