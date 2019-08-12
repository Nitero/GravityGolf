using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Slowmotion : MonoBehaviour
{
    /*
    [SerializeField] private float minTimeDragging = 0.2f;
    [SerializeField] private float slowSpeed = 0.01f;
    [SerializeField] private AnimationCurve interpolateIn;
    [SerializeField] private AnimationCurve interpolateOut;
    private AnimationCurve interpolateCurrent;


    private float timer; //If 1 then full slow, if 0 then no slow
    private int timerDir = -1;
    private bool enteredGoal = true;
    private bool dragging;


    void Start()
    {
       
    }


     void Update()
    {
        if (dragging && enteredGoal)
        {
            timerDir = 1;
            interpolateCurrent = interpolateIn;
        }
        else
        {
            timerDir = -1;
            interpolateCurrent = interpolateOut;
        }

        timer += timerDir * Time.deltaTime * slowSpeed * interpolateCurrent.Evaluate(timer);

        if (timer <= 0) timer = 0;
        if (timer >= 1) timer = 1;

        Time.timeScale = timer.Remap(0, 1, 1, minTimeDragging);
        print(Time.timeScale);
    }

    public void setDragging(bool b)
    {
        dragging = b;
    }
    public void hitGoal(bool b)
    {
        enteredGoal = b;
    }*/


    public void toggleSceneSelectPause(int t)
    {
        if (t == 0) Time.timeScale = 1;
        if (t == 1) Time.timeScale = 0;
    }

}
