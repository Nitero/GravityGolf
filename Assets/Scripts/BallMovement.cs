﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.EventSystems;

public class BallMovement : MonoBehaviour
{
    [SerializeField] private Vector2 shootStrMinMax;
    [SerializeField] private Vector2 dragDistMinMax; //above doesn't do more strength, below and it can be released to abort
    //[SerializeField] private float dragToStrMulti = ;
    private float shootStrength;
    [SerializeField] private bool useMouse = true;

    private Rigidbody2D rb;
    private Vector2 aimDir = Vector2.zero;

    private Vector2 originalPos;
    private bool hitGoal;
    private LineRenderer line;

    private bool wasTouchingLastFixed;
    private bool isTouching;
    private Vector2 touchA;
    private Vector2 touchB;
    [SerializeField] private GameObject dragIndication;
    [SerializeField] private Vector2 dragLineMinMaxWidth;
    [SerializeField] private float dragIndicationSnapDur = 0.25f;
    [SerializeField] private AnimationCurve dragIndicationSnap;
    private LineRenderer dragLine;
    private Transform dragOutter;
    private Transform dragInner;
    [SerializeField] private float joystickOffset = 3;
    private Vector2 joystickOutterStart;
    private Vector2 joystickInnerStart;

    [SerializeField] private GameObject trajectory;
    [SerializeField] private int trajectoryVerts = 30;

    private GameLoop mainLogic;
    private Slowmotion slowmotion;

    void Start()
    {
        dragIndication = Instantiate(dragIndication);
        dragLine = dragIndication.GetComponent<LineRenderer>();
        dragOutter = dragIndication.transform.GetChild(0);
        dragInner = dragIndication.transform.GetChild(1);
        dragIndication.SetActive(false);

        rb = GetComponent<Rigidbody2D>();
        line = trajectory.GetComponent<LineRenderer>();
        slowmotion = FindObjectOfType<Slowmotion>();
        mainLogic = FindObjectOfType<GameLoop>();

        //rb.gravityScale = 0;
        originalPos = transform.position;
    }


    void Update()
    {
        if (useMouse)
        {
            if (Input.GetMouseButtonDown(0) && !IsPointerOverUIObject())
            {
                var mousePos = Input.mousePosition;
                touchA = mousePos;
                // -> mouse to world doesnt work since player moves in world... also bcz they are in canvas

                //joystickInner.position = touchA;
                //joystickOutter.position = touchA;
                isTouching = true;
            }
            if (Input.GetMouseButton(0) && isTouching)
            {
                isTouching = true;
                var mousePos = Input.mousePosition;
                touchB = mousePos;
            }
            else
                isTouching = false;
        }

        /*
        else if (Input.touchCount > 0) //Touch input (only one finger)
        {
            Touch t = Input.GetTouch(0);

            //int id = t.fingerId;

            if (t.phase == TouchPhase.Began)
            {
                touchA = t.position;

                dragInner.position = touchA;
                dragOutter.position = touchA;
                isTouching = true;
            }
            if (t.phase == TouchPhase.Moved)
            {
                touchB = t.position;
            }
            if (t.phase == TouchPhase.Stationary)
            {

            }
            if (t.phase == TouchPhase.Ended)
            {
                isTouching = false;
            }
        }
        */



        if (isTouching && mainLogic.getShots() > 0 && !hitGoal)
        {
            float dragDist = Vector2.Distance(Camera.main.ScreenToWorldPoint(touchB), Camera.main.ScreenToWorldPoint(touchA));
            shootStrength = dragDist.Remap(dragDistMinMax.x, dragDistMinMax.y, shootStrMinMax.x, shootStrMinMax.y);
            //trajectoryVerts

            drawTrajectory(transform.position, -aimDir * (shootStrength / rb.mass));

            slowmotion.setDragging(true);
        }
        else
        {
            hideTrajectory();

            slowmotion.setDragging(false);
        }
    }

    private void FixedUpdate()
    {
        //var direction = Vector2.zero;

        

        if (!hitGoal)
        {
            // Mouse & Touch input
            if (isTouching && mainLogic.getShots() > 0)
            {
                var offset = Camera.main.ScreenToWorldPoint(touchB) - Camera.main.ScreenToWorldPoint(touchA);

                aimDir = offset.normalized; //faster direction change bcz normalization
                                            //direction = Vector2.ClampMagnitude(offset, 1); //smooth movement: if drag far go faster, if not go less

                wasTouchingLastFixed = true;

                //Smooth button on non using joystick for acceleration too, even though its normalized for actual physic throttle
                //direction = Vector2.ClampMagnitude(offset, 1);
                //dragInner.position = new Vector2(touchA.x + direction.x * joystickOffset, touchA.y + direction.y * joystickOffset);

                showDragIndication();
            }
            else
            {

                if (wasTouchingLastFixed && mainLogic.getShots() > 0)
                {
                    rb.gravityScale = 1;
                    wasTouchingLastFixed = false;
                    rb.AddForce(-aimDir * shootStrength, ForceMode2D.Impulse);

                    mainLogic.didShot();

                    StartCoroutine(hideDragIndication());
                }

                //joystickInner.position = joystickInnerStart;
                //joystickOutter.position = joystickOutterStart;
            }
        }
    }



    public void hideTrajectory()
    {
        trajectory.GetComponent<LineRenderer>().SetVertexCount(0);
    }

    public void drawTrajectory(Vector2 startPos, Vector2 startVelocity)  //https://answers.unity.com/questions/606720/drawing-projectile-trajectory.html?childToView=606837#comment-606837
    {
        var distance = 0f;

        var verts = trajectoryVerts;
        line.SetVertexCount(verts);

        var pos = startPos;
        var vel = startVelocity;
        var grav = new Vector2(Physics.gravity.x, Physics.gravity.y);
        for (var i = 0; i < verts; i++)
        {
            line.SetPosition(i, new Vector3(pos.x, pos.y, 0));

            var nextPos = pos + vel * Time.fixedDeltaTime;

            // wall reflect https://www.youtube.com/watch?v=GttdLYKEJAM
            RaycastHit2D hit = Physics2D.Raycast(pos, nextPos, Vector2.Distance(pos, nextPos), LayerMask.GetMask("Solid Objects")); // ~(LayerMask.GetMask("Player")) everything but player
            if (hit.collider != null)
            {
                //TODO: limit ammount of reflections to one ??????

                //just quit, don't reflect (use this if did one reflection already)
                //line.SetVertexCount(i);
                //break;

                //reflect
                vel = Vector2.Reflect(vel, hit.normal); //does this work? get stuck in ground?
                //pos = hit.point;// + Vector2.up;
                pos = hit.collider.ClosestPoint(hit.point);
                //pos = hit.collider.ClosestPoint(hit.point) + vel + grav * Time.fixedDeltaTime;

                //fix distance
                distance += Vector3.Distance(startPos, pos);
                startPos = pos;
            }
            else
            {
                vel = vel + grav * Time.fixedDeltaTime;//Physics.deltaf;
                pos = nextPos;//Physics.fixedDeltaTime;
            }

        }

        //https://answers.unity.com/questions/733592/dotted-line-with-line-renderer.html
        //https://gamedev.stackexchange.com/questions/118814/unity-make-dotted-line-renderer
        distance += Vector3.Distance(startPos, pos);
        line.materials[0].mainTextureScale = new Vector3(distance, 1, 1);
    }

    /*
    public void drawTrajectory(Vector2 startPos, Vector2 startVelocity)  //https://answers.unity.com/questions/606720/drawing-projectile-trajectory.html?childToView=606837#comment-606837
    {
        var verts = trajectoryVerts;
        line.SetVertexCount(verts);

        var pos = startPos;
        var vel = startVelocity;
        var grav = new Vector2(Physics.gravity.x, Physics.gravity.y);
        for (var i = 0; i < verts; i++)
        {
            line.SetPosition(i, new Vector3(pos.x, pos.y, 0));

            var nextPos = pos + vel * Time.fixedDeltaTime;

            // walls https://www.youtube.com/watch?v=GttdLYKEJAM
            RaycastHit2D hit = Physics2D.Raycast(pos, nextPos, Vector2.Distance(pos, nextPos), LayerMask.GetMask("Solid Objects")); // ~(LayerMask.GetMask("Player")) everything but player
            if (hit.collider != null)
            {
                //just quit, don't reflect (use this if did one reflection already)
                line.SetVertexCount(i);
                break;
            }

            vel = vel + grav * Time.fixedDeltaTime;//Physics.deltaf;
            pos = nextPos;//Physics.fixedDeltaTime;
        }

        //https://answers.unity.com/questions/733592/dotted-line-with-line-renderer.html
        //https://gamedev.stackexchange.com/questions/118814/unity-make-dotted-line-renderer
        var distance = Vector3.Distance(startPos, pos);
        line.materials[0].mainTextureScale = new Vector3(distance, 1, 1);
    }*/

    //version with drag: https://tech.spaceapegames.com/2016/07/05/trajectory-prediction-with-unity-physics/
    /*public void drawTraject(Vector2 startPos, Vector2 velocity)
    {
        var steps = trajectoryVerts;
        var pos = startPos;
        line.SetVertexCount(steps);

        Vector2[] results = new Vector2[steps];

        float timestep = Time.fixedDeltaTime / Physics2D.velocityIterations;
        Vector2 gravityAccel = Physics2D.gravity * rb.gravityScale * timestep * timestep;
        float drag = 1f - timestep * rb.drag;
        Vector2 moveStep = velocity * timestep;

        for (int i = 0; i < steps; ++i)
        {
            moveStep += gravityAccel;
            moveStep *= drag;
            pos += moveStep;
            results[i] = pos;
        }

        for (var i = 0; i < steps; i++)
        {
            line.SetPosition(i, new Vector3(results[i].x, results[i].y, 0));
        }

        var distance = Vector3.Distance(startPos, pos);
        line.materials[0].mainTextureScale = new Vector3(distance, 1, 1);
    }*/



    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.tag == "KillBounds")
            died();
    }

    public float getMagnitude()
    {
        return rb.velocity.magnitude;
    }

    public void didHitGoal(float shrinkDur, Ease ease)
    {
        rb.gravityScale = 0;
        hitGoal = true;

        //GetComponent<Collider2D>().enabled = false; //doesn't work bcz goal can't suck you in anymore
        gameObject.layer = LayerMask.NameToLayer("Collide With Nothing");

        slowmotion.hitGoal(true);

        hideDragIndicationInstant();

        transform.DOScale(Vector3.zero, shrinkDur).SetEase(ease);
        Invoke("nextLvl", shrinkDur);
    }

    private void nextLvl()
    {
        FindObjectOfType<LevelManager>().nextLvl();
    }


    private void showDragIndication()
    {
        dragIndication.SetActive(true);
        dragInner.position = Camera.main.ScreenToWorldPoint(touchA);
        dragOutter.position = Camera.main.ScreenToWorldPoint(touchB);
        dragInner.position = new Vector3(dragInner.position.x, dragInner.position.y, 0);
        dragOutter.position = new Vector3(dragOutter.position.x, dragOutter.position.y, 0);
        dragLine.SetPosition(0, dragInner.position);
        dragLine.SetPosition(1, dragInner.position + 0.5f * (dragOutter.position - dragInner.position));//Vector2.Lerp(dragInner.position, dragOutter.position, (dragInner.position - dragOutter.position).magnitude));
        dragLine.SetPosition(2, dragOutter.position);
        float dragDist = Vector2.Distance(dragInner.position, dragOutter.position);
        dragLine.startWidth = dragDist.Remap(dragDistMinMax.x, dragDistMinMax.y, dragLineMinMaxWidth.y, dragLineMinMaxWidth.x);
    }

    private IEnumerator hideDragIndication()
    {
        var startPos = dragOutter.position;

        float timer = 0;

        while(timer <= dragIndicationSnapDur)
        {
            dragOutter.position = Vector2.Lerp(startPos, dragInner.position, dragIndicationSnap.Evaluate(timer.Remap(0, dragIndicationSnapDur, 0, 1)));

            dragLine.SetPosition(1, dragInner.position + 0.5f * (dragOutter.position - dragInner.position));
            dragLine.SetPosition(2, dragOutter.position);
            float dragDist = Vector2.Distance(dragInner.position, dragOutter.position);
            dragLine.startWidth = dragDist.Remap(dragDistMinMax.x, dragDistMinMax.y, dragLineMinMaxWidth.y, dragLineMinMaxWidth.x);

            timer += Time.deltaTime;

            yield return null;
        }

        //yield return new WaitForSeconds(dragIndicationSnapDur);

        dragIndication.SetActive(false);
    }

    public void hideDragIndicationInstant()
    {
        dragIndication.SetActive(false);
    }


    private bool IsPointerOverUIObject() //https://answers.unity.com/questions/1073979/android-touches-pass-through-ui-elements.html
    {
        PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
        eventDataCurrentPosition.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
        return results.Count > 0;
    }



    public Vector2 getOrgPos()
    {
        return originalPos;
    }

    private void died()
    {
        //effect on the edge of camera where died
        //screenshake

        Destroy(dragIndication);
        mainLogic.resetLevel();
        mainLogic.respawnPlayer();
    }

    public void restarted()
    {
        //effect on the ball

        Destroy(dragIndication);
        mainLogic.resetLevel();
        mainLogic.respawnPlayer();
    }


    /*
    private void died()
    {
        //effect on the edge of camera where died
        //screenshake

        resetPlayerPosAndOrient();
        mainLogic.resetShots();
    }

    public void restarted()
    {
        //effect on the ball

        resetPlayerPosAndOrient();
        mainLogic.resetShots();
    }

    private void resetPlayerPosAndOrient()
    {
        // order is important
        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0;
        transform.eulerAngles = Vector3.zero;
        transform.position = originalPos;

        rb.gravityScale = 0; //without this is more fun as there is small bounce, but when resetting when fly into portal then ball rolls after reset... maybe use drag or disable collider for a sec with yield return null
    }
    */
}



public static class ExtensionMethods
{

    public static float Remap(this float value, float from1, float to1, float from2, float to2)
    {
        return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
    }

}