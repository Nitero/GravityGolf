using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    [SerializeField] private int maxLvl;
    //[SerializeField] private bool debugging = true;
    private int lvl;

    private UiManager uiManager;
    private BallMovement player;

    void Start()
    {
        uiManager = FindObjectOfType<UiManager>();
        player = FindObjectOfType<BallMovement>();

        var sceneName = SceneManager.GetActiveScene().name;
        var lvlString = sceneName.Substring(sceneName.Length - 2);
        lvl = int.Parse(lvlString);

        uiManager.updateLevelDisplay(lvlString, maxLvl);


        if (lvl == 0) uiManager.tutorialAnimation();
    }


    void Update()
    {
        
    }


    public void restart()
    {
        player.restarted();

        /*string lvlString = lvl.ToString();
        if (lvl < 9) lvlString = "0" + lvlString;
        SceneManager.LoadScene("level" + lvlString);*/
    }

    public void nextLvl()
    {
        lvl++;
        if (lvl > maxLvl) lvl = maxLvl;

        var lvlString = loadLevel();
        uiManager.updateLevelDisplay(lvlString, maxLvl);
    }

    public void lastLvl()
    {
        lvl--;
        if (lvl < 0) lvl = 0;

        var lvlString = loadLevel();
        uiManager.updateLevelDisplay(lvlString, maxLvl);
    }

    private string loadLevel()
    {
        string lvlString = lvl.ToString();
        if (lvl < 9) lvlString = "0" + lvlString;
        SceneManager.LoadScene("level" + lvlString);
        return lvlString;
    }

}
