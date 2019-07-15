using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShotsAndTimer : MonoBehaviour
{
    [SerializeField] private int maxShots;
    [SerializeField] private TextMeshProUGUI shotDisplay;
    [SerializeField] private TextMeshProUGUI timeDisplay;

    private float timer;
    private int remainingShots;
    private bool timerStarted;

    void Start()
    {
        remainingShots = maxShots;
        shotDisplay.text = "Shots: " + remainingShots;
    }


    void Update()
    {
        if(timerStarted)
        {
            timer += Time.deltaTime;
            //timeDisplay.text = timer;
        }
    }

    public void startTimer()
    {
        timerStarted = true;
    }

    public void didShot()
    {
        remainingShots--;
        shotDisplay.text = "Shots: " + remainingShots;
    }

    public int getShots()
    {
        return remainingShots;
    }
}
