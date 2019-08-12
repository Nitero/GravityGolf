using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.EventSystems;

public class BallMovement : MonoBehaviour
{
    [SerializeField] private bool showRealTrajectory;
    [SerializeField] private float keptVelocityOnShot = 1f; // 0 means complete reset, 1 means keep old vel and add new
    [SerializeField] private Vector2 shootStrMinMax;
    [Space]
    [Header("Input")]
    [SerializeField] private Vector2 dragDistMinMax; //above doesn't do more strength, below and it can be released to abort
    [SerializeField] private float dragDistOutBeforeCanRelease = 1f; 
    [SerializeField] private float dragReleaseDist = 0.5f; 
    //[SerializeField] private float dragToStrMulti = ;
    private float shootStrength;
    [SerializeField] private bool useMouse = true;

    [SerializeField] private GameObject dragIndication;
    [SerializeField] private Vector2 dragLineMinMaxWidth;
    [SerializeField] private float dragIndicationSnapDur = 0.25f;
    [SerializeField] private AnimationCurve dragIndicationSnap;

    [SerializeField] private float joystickOffset = 3;

    [SerializeField] private GameObject trajectory;
    [SerializeField] private int trajectoryVerts = 30;



    private Rigidbody2D rb;
    private LineRenderer dragLine;
    private GameLoop mainLogic;
    private Slowmotion slowmotion;
    private BallVisuals visuals;

    private Vector2 aimDir = Vector2.zero;
    private Vector2 originalPos;
    private bool canRelease;
    private bool insidePortal;
    private bool inTutorial = true;
    private LineRenderer line;

    private bool wasTouchingLastFixed;
    private bool isTouching;
    private Vector2 touchA;
    private Vector2 touchB;
    private Transform dragOutter;
    private Transform dragInner;
    private SpriteRenderer[] dragCrossParts = new SpriteRenderer[2];

    private Vector2 joystickOutterStart;
    private Vector2 joystickInnerStart;

    void Start()
    {
        dragIndication = Instantiate(dragIndication);
        dragLine = dragIndication.GetComponent<LineRenderer>();
        dragOutter = dragIndication.transform.GetChild(0);
        dragInner = dragIndication.transform.GetChild(1);
        dragCrossParts[0] = dragInner.transform.GetChild(0).GetComponent<SpriteRenderer>();
        dragCrossParts[1] = dragInner.transform.GetChild(1).GetComponent<SpriteRenderer>();
        dragIndication.SetActive(false);

        rb = GetComponent<Rigidbody2D>();
        line = trajectory.GetComponent<LineRenderer>();
        slowmotion = FindObjectOfType<Slowmotion>();
        mainLogic = FindObjectOfType<GameLoop>();

        originalPos = transform.position;
    }

    private void Awake()
    {
        visuals = GetComponent<BallVisuals>();
    }


    void Update()
    {
        //TODO: put this into a seperate input script (+ move trajectory and user drag indication to UI)

        if (useMouse)
        {
            if (Input.GetMouseButtonDown(0) && !IsPointerOverUIObject())
            {
                var mousePos = Input.mousePosition;
                touchA = mousePos;

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
        else if (Input.touchCount > 0) //Touch input (only one finger)
        {
            Touch t = Input.GetTouch(0);

            if (t.phase == TouchPhase.Began && !IsPointerOverUIObject())
            {
                touchA = t.position;

                isTouching = true;
            }
            if (t.phase == TouchPhase.Moved && isTouching)
            {
                isTouching = true;
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



        if (!insidePortal && isTouching && mainLogic.getShots() > 0)
        {
            float dragDist = Vector2.Distance(Camera.main.ScreenToWorldPoint(touchB), Camera.main.ScreenToWorldPoint(touchA));
            dragDist = Mathf.Clamp(dragDist, dragDistMinMax.x, dragDistMinMax.y);
            shootStrength = dragDist.Remap(dragDistMinMax.x, dragDistMinMax.y, shootStrMinMax.x, shootStrMinMax.y);

            if(dragDist >= dragDistOutBeforeCanRelease) canRelease = true;

            // Could reset now by lifting finger
            if (canRelease && dragDist <= dragReleaseDist)
            {
                foreach (SpriteRenderer s in dragCrossParts)
                    s.transform.DOScaleX(3, 0.2f).SetEase(Ease.OutCubic);

                hideTrajectory();
            }
            // Could not
            else
            {
                foreach (SpriteRenderer s in dragCrossParts)
                    s.transform.DOScaleX(0, 0.2f);

                if (showRealTrajectory)
                    drawTrajectory(transform.position, -aimDir * (shootStrength / rb.mass) + rb.velocity);
                else
                    drawTrajectory(transform.position, -aimDir * (shootStrength / rb.mass));
            }
                
            
            showDragIndication();
            //slowmotion.setDragging(true);
        }
        else
        {
            hideTrajectory();
            //slowmotion.setDragging(false);
        }
    }


    private void FixedUpdate()
    {
        if (inTutorial) return;
        

        if (!insidePortal)
        {
            // Doing any input
            if (isTouching && mainLogic.getShots() > 0)
            {
                var dragOffset = Camera.main.ScreenToWorldPoint(touchB) - Camera.main.ScreenToWorldPoint(touchA);

                aimDir = dragOffset.normalized; 

                wasTouchingLastFixed = true;
            }
            else
            {
                // released after drag
                if (wasTouchingLastFixed && mainLogic.getShots() > 0)
                {
                    float dragDist = Vector2.Distance(Camera.main.ScreenToWorldPoint(touchB), Camera.main.ScreenToWorldPoint(touchA));
                    dragDist = Mathf.Clamp(dragDist, dragDistMinMax.x, dragDistMinMax.y);

                    // didn't drag far enough, reset shot
                    if (dragDist <= dragReleaseDist)
                    {
                        hideDragIndicationInstant();
                    }
                    else
                    {
                        rb.velocity = rb.velocity * keptVelocityOnShot;
                        rb.gravityScale = 1;
                        wasTouchingLastFixed = false;
                        rb.AddForce(-aimDir * shootStrength, ForceMode2D.Impulse);

                        mainLogic.didShot();

                        visuals.deathDustFX((Vector2)transform.position /* + aimDir*/);

                        // Doesnt feel right
                        //if(shootStrength == shootStrMinMax.y)
                        //    FindObjectOfType<HitStop>().Stop(0.025f);

                        StartCoroutine(hideDragIndication());
                    }

                    canRelease = false;
                }
            }
        }
    }



    public void hideTrajectory()
    {
        trajectory.GetComponent<LineRenderer>().SetVertexCount(0);
    }


    public void drawTrajectory(Vector2 startPos, Vector2 startVelocity)  //credit: https://answers.unity.com/questions/606720/drawing-projectile-trajectory.html?childToView=606837#comment-606837
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

            // TODO: wall reflect https://www.youtube.com/watch?v=GttdLYKEJAM

            RaycastHit2D hit = Physics2D.Raycast(pos, nextPos, Vector2.Distance(pos, nextPos), LayerMask.GetMask("Solid Objects")); // ~(LayerMask.GetMask("Player")) everything but player
            if (hit.collider != null)
            {
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


        //credit: https://answers.unity.com/questions/733592/dotted-line-with-line-renderer.html https://gamedev.stackexchange.com/questions/118814/unity-make-dotted-line-renderer
        distance += Vector3.Distance(startPos, pos);
        line.materials[0].mainTextureScale = new Vector3(distance, 1, 1);
    }



    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.tag == "KillBounds")
            diedBounds();
    }

    public float getMagnitude()
    {
        return rb.velocity.magnitude;
    }

    public void didHitGoal(float shrinkDur, Ease ease, bool isGoal)
    {
        insidePortal = true;

        rb.gravityScale = 0;

        // Is safe in goal, don't be affected by anything (collider still needed to be sucked in)
        gameObject.layer = LayerMask.NameToLayer("Collide With Nothing");


        hideDragIndicationInstant();

        transform.DOScale(Vector3.zero, shrinkDur).SetEase(ease);

        GetComponentInChildren<TrailRenderer>().DOResize(0, 0, shrinkDur);

        if (isGoal)
        {
            Invoke("trappedInPortalGoalShake", 1.75f);
            Invoke("nextLvl", shrinkDur);
        }
        else
        {
            Invoke("trappedInPortalBadShake", 1.75f);
            Invoke("died", shrinkDur);
        }
    }

    private void trappedInPortalBadShake()
    {
        visuals.shake(Random.insideUnitCircle, 0.5f);
    }
    private void trappedInPortalGoalShake()
    {
        visuals.shake(Random.insideUnitCircle, 0.4f);
    }

    public bool isInsidePortal()
    {
        return insidePortal;
    }

    private void nextLvl()
    {
        FindObjectOfType<LevelManager>().nextLvl();
    }


    // Show circles connected with a line under finger & start drag pos
    private void showDragIndication()
    {
        var inner = Camera.main.ScreenToWorldPoint(touchA);
        var outter = Camera.main.ScreenToWorldPoint(touchB);

        // Clamp max drag distance
        if(Vector2.Distance(inner, outter) > dragDistMinMax.magnitude)
        {
            var dir = outter - inner;
            outter = inner + dir.normalized * dragDistMinMax.magnitude;
        }

        dragInner.position = new Vector3(inner.x, inner.y, 0);
        dragOutter.position = new Vector3(outter.x, outter.y, 0);
        dragLine.SetPosition(0, dragInner.position);
        dragLine.SetPosition(1, dragInner.position + 0.5f * (dragOutter.position - dragInner.position));
        dragLine.SetPosition(2, dragOutter.position);
        float dragDist = Vector2.Distance(dragInner.position, dragOutter.position);
        dragLine.startWidth = dragDist.Remap(dragDistMinMax.x, dragDistMinMax.y, dragLineMinMaxWidth.y, dragLineMinMaxWidth.x);
        dragIndication.SetActive(true);
    }

    // Do quick animations of circles snapping together
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

        dragIndication.SetActive(false);
    }

    public void hideDragIndicationInstant()
    {
        dragIndication.SetActive(false);
    }


    private bool IsPointerOverUIObject() //credit: https://answers.unity.com/questions/1073979/android-touches-pass-through-ui-elements.html
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

    private void died() //bad practise: gets invoked, thus duplicates below with other params
    {
        //only effect on respawn pos, died in portal

        Destroy(dragIndication);
        mainLogic.resetLevel();
        mainLogic.respawnPlayer(false, true);
    }

    private void diedBounds()
    {
        //effect on the edge of camera where died

        Destroy(dragIndication);
        mainLogic.resetLevel();
        mainLogic.respawnPlayer(true, false);
    }

    public void restarted()
    {
        //just effect on the ball and former position

        Destroy(dragIndication);
        mainLogic.resetLevel();
        mainLogic.respawnPlayer(false, false);
    }

    

    public void notInTut()
    {
        inTutorial = false;
    }

    public void hideSprite()
    {
        visuals.setSpriteEnabled(false);
    }


    public void spawnFromPortal(Vector2 spawnPos)
    {
        transform.position = spawnPos;
        visuals.spawnAnim();
        rb.AddForce(Vector2.up * 20, ForceMode2D.Impulse);
    }
}



public static class ExtensionMethods
{

    public static float Remap(this float value, float from1, float to1, float from2, float to2)
    {
        return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
    }

}