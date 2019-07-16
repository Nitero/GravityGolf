using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameLoop : MonoBehaviour
{
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private int maxShots;

    private int remainingShots;
    private float timer;
    private bool timerStarted;

    private UiManager ui;
    private GameObject player;

    void Start()
    {
        ui = FindObjectOfType<UiManager>();
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


    public void respawnPlayer()
    {

        var dragInds = FindObjectsOfType<LineRenderer>();
        foreach (LineRenderer d in dragInds)
            Destroy(d);

        var orgPos = player.GetComponent<BallMovement>().getOrgPos();
        Destroy(player);
        player = Instantiate(playerPrefab, orgPos, Quaternion.identity);
        player.GetComponent<BallMovement>().notInTut();

    }
}
