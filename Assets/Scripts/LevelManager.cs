using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;

public class LevelManager : MonoBehaviour
{
    [SerializeField] private Vector2 portalSpawnOffset;
    [SerializeField] private GameObject portalSpawnPrefab;
    [SerializeField] private float levelCameraTrans = 0.25f;
    [SerializeField] private int maxLvl;
    //[SerializeField] private bool debugging = true;
    private int lvl;

    private bool inLevelTransition;

    private Camera cam;
    private UiManager uiManager;
    private BallMovement player;
    private Screenshake screenShake;


    void Awake()
    {
        cam = Camera.main;
        uiManager = FindObjectOfType<UiManager>();
        screenShake = FindObjectOfType<Screenshake>();
        player = FindObjectOfType<BallMovement>();

        var sceneName = SceneManager.GetActiveScene().name;
        var lvlString = sceneName.Substring(sceneName.Length - 2);
        lvl = int.Parse(lvlString);

        uiManager.updateLevelDisplay(lvlString, maxLvl);


        if (lvl == 0) uiManager.playtutorialAnimation();
        else if(PlayerPrefs.GetInt("sceneSelectToggled") == 1) player.notInTut();


        // If go back to first scene, don't show tutorial again, but still be able to move
        if(lvl == 0 && PlayerPrefs.HasKey("sceneSelectToggled") && PlayerPrefs.GetInt("sceneSelectToggled") == 1)
            player.notInTut();

        // Do a camera animation if changed level from last load
        if (lvl != 0 && lvl != maxLvl)
        {
            var lastLvl = PlayerPrefs.GetInt("lastLvl");
            
            if(lastLvl > lvl) cam.transform.position = new Vector3(17,0,-10);
            else cam.transform.position = new Vector3(-17, 0, -10);

            screenShake.setInTransition(true);

            Sequence seq = DOTween.Sequence().SetUpdate(true); 
            cam.transform.DOMoveX(0, levelCameraTrans).SetEase(Ease.OutCubic).SetUpdate(true);
            seq.AppendCallback(() => screenShake.setInTransition(false));
        }

        // Progressed one level normally
        if(lvl != 0 && (PlayerPrefs.HasKey("lastLvl") && PlayerPrefs.GetInt("lastLvl") != lvl && PlayerPrefs.GetInt("sceneSelectToggled") == 0))
        {
            Vector2 playerPos = GameObject.FindGameObjectWithTag("Player").transform.position;
            var portal = Instantiate(portalSpawnPrefab, playerPos + portalSpawnOffset, Quaternion.identity);
            portal.GetComponent<Portal>().spawnAnim();
        }

        PlayerPrefs.SetInt("lastLvl", lvl);
    }





    // ------------------ Called from UI buttons ------------------ //

    public void restart()
    {
        player.restarted();
    }

    public void nextLvl()
    {
        if (inLevelTransition || lvl == maxLvl) return;

        lvl++;
        if (lvl > maxLvl) lvl = maxLvl;

        screenShake.setInTransition(true);

        Sequence seq = DOTween.Sequence().SetUpdate(true); //move camera even in timescale 0
        seq.Append(cam.transform.DOMoveX(17, levelCameraTrans).SetEase(Ease.InCubic));
        seq.AppendCallback(() => loadLevel());
    }

    public void lastLvl()
    {
        if (inLevelTransition || lvl == 0) return;

        lvl--;
        if (lvl < 0) lvl = 0;

        screenShake.setInTransition(true);

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
    }

}
