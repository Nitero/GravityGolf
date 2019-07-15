using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Goal : MonoBehaviour
{
    [SerializeField] private float ballLessMagnitudeStop = 0.5f;

    [Header("Animation")]
    [SerializeField] private float ballShrinkDur = 1f;
    [SerializeField] private Ease ballShrinkEase = Ease.Linear;

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
            }
        }
    }
}
