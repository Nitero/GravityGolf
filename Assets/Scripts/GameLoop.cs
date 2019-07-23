using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameLoop : MonoBehaviour
{
    [SerializeField] private GameObject respawnDust;
    [SerializeField] private GameObject deadDust;
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private int maxShots;

    private int remainingShots;
    private float timer;
    private bool timerStarted;

    private UiManager ui;
    private BallVisuals ballVis;
    private Screenshake screenshake;
    private GameObject player;

    void Start()
    {
        ui = FindObjectOfType<UiManager>();
        ballVis = FindObjectOfType<BallVisuals>();
        screenshake = FindObjectOfType<Screenshake>();
        player = GameObject.FindGameObjectWithTag("Player");

        resetLevel();
    }

    void Update()
    {
        if (timerStarted)
        {
            timer += Time.deltaTime;
            //uiManager.timeDisplay.text = timer;
        }
    }

    public void startTimer()
    {
        timerStarted = true;
    }

    public void didShot()
    {
        remainingShots--;
        ui.displayShots(remainingShots);
    }


    public void resetLevel()
    {
        //Reset environment
        var dynamicObjs = FindObjectsOfType<DynamicObject>();
        foreach (DynamicObject d in dynamicObjs)
            d.reset();


        //Reset shots
        remainingShots = maxShots;
        ui.displayShots(remainingShots);
    }

    public int getShots()
    {
        return remainingShots;
    }


    public void respawnPlayer(bool diedOnBounds, bool diedInHole)
    {
        if(!diedInHole)
        {
            if (!diedOnBounds)
            {
                Instantiate(respawnDust, player.transform.position, respawnDust.transform.rotation);
                screenshake.AddShake(Vector2.up, 0.5f);
            }
            else
            {
                Vector3 pos = Camera.main.WorldToViewportPoint(player.transform.position);
                var rot = 0;
                if (pos.x <= 0)
                {
                    rot = 90;
                    pos.x = 0;
                    ballVis.boundShake(new Vector2(1, 0), player.GetComponent<BallMovement>().getMagnitude());
                }
                if (pos.y <= 0)
                {
                    pos.y = 0;
                    ballVis.boundShake(new Vector2(0, 1), player.GetComponent<BallMovement>().getMagnitude());
                }
                if (pos.x >= 1)
                {
                    rot = -90;
                    pos.x = 1;
                    ballVis.boundShake(new Vector2(1, 0), player.GetComponent<BallMovement>().getMagnitude());
                }
                if (pos.y >= 1)
                {
                    rot = 180;
                    pos.y = 1;
                    ballVis.boundShake(new Vector2(0, 1), player.GetComponent<BallMovement>().getMagnitude());
                }
                Instantiate(deadDust, Camera.main.ViewportToWorldPoint(pos), Quaternion.Euler(0, 0, rot));
            }
        }
        else
            screenshake.AddShake(Vector2.up, 0.5f);


        var dragInds = FindObjectsOfType<LineRenderer>();
        foreach (LineRenderer d in dragInds)
            Destroy(d);

        var orgPos = player.GetComponent<BallMovement>().getOrgPos();
        Destroy(player);
        player = Instantiate(playerPrefab, orgPos, Quaternion.identity);
        player.GetComponent<BallMovement>().notInTut();

        Instantiate(respawnDust, player.transform.position, respawnDust.transform.rotation);
    }
}
