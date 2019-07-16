using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;

public class LevelManager : MonoBehaviour
{
    [SerializeField] private float levelCameraTrans = 0.25f;
    [SerializeField] private int maxLvl;
    //[SerializeField] private bool debugging = true;
    private int lvl;

    private bool inLevelTransition;

    private UiManager uiManager;
    private BallMovement player;

    private Camera cam;

    void Awake() //Start
    {
        cam = Camera.main;
        uiManager = FindObjectOfType<UiManager>();
        player = FindObjectOfType<BallMovement>();

        var sceneName = SceneManager.GetActiveScene().name;
        var lvlString = sceneName.Substring(sceneName.Length - 2);
        lvl = int.Parse(lvlString);

        uiManager.updateLevelDisplay(lvlString, maxLvl);


        if (lvl == 0) uiManager.tutorialAnimation();
        else player.notInTut();

        // if go back to first scene, don't show tutorial again, but still be able to move
        if(lvl == 0 && PlayerPrefs.HasKey("sceneSelectToggled") && PlayerPrefs.GetInt("sceneSelectToggled") == 1)
            player.notInTut();


        if (lvl != 0 && lvl != maxLvl)
        {
            var lastLvl = PlayerPrefs.GetInt("lastLvl");
            
            if(lastLvl > lvl) cam.transform.position = new Vector3(17,0,-10);
            else cam.transform.position = new Vector3(-17, 0, -10);

            cam.transform.DOMoveX(0, levelCameraTrans).SetEase(Ease.OutCubic).SetUpdate(true);
        }

        PlayerPrefs.SetInt("lastLvl", lvl);
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
        if (inLevelTransition || lvl == maxLvl) return;

        lvl++;
        if (lvl > maxLvl) lvl = maxLvl;


        Sequence seq = DOTween.Sequence().SetUpdate(true); //move camera even in timescale 0
        seq.Append(cam.transform.DOMoveX(17, levelCameraTrans).SetEase(Ease.InCubic));
        seq.AppendCallback(() => loadLevel());
    }

    public void lastLvl()
    {
        if (inLevelTransition || lvl == 0) return;

        lvl--;
        if (lvl < 0) lvl = 0;


        Sequence seq = DOTween.Sequence().SetUpdate(true); //move camera even in timescale 0
        seq.Append(cam.transform.DOMoveX(-17, levelCameraTrans).SetEase(Ease.InCubic));
        seq.AppendCallback(() => loadLevel());
    }

    private void loadLevel()
    {
        inLevelTransition = true;

        string lvlString = lvl.ToString();
        if (lvl < 10) lvlString = "0" + lvlString;
        SceneManager.LoadScene("level" + lvlString);
        //return lvlString;
    }

}
