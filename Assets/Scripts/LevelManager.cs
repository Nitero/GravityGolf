using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    [SerializeField] private Image greyBackground; //fade this !!!
    [SerializeField] private GameObject levelSelect;
    [SerializeField] private TextMeshProUGUI lvlDisplay;


    [SerializeField] private int maxLvl;
    //[SerializeField] private bool debugging = true;
    private int lvl;
    private bool sceneSelectToggled;

    void Start()
    { 
        var sceneName = SceneManager.GetActiveScene().name;
        var lvlString = sceneName.Substring(sceneName.Length - 2);
        lvl = int.Parse(lvlString);

        string lvlStringMax = maxLvl.ToString();
        if (maxLvl < 9) lvlStringMax = "0" + lvlStringMax;
        lvlDisplay.text = lvlString + " / " + lvlStringMax;


        if (PlayerPrefs.HasKey("sceneSelectToggled"))
            sceneSelectToggled = PlayerPrefs.GetInt("sceneSelectToggled") > 0 ? true : false;     
        else
            PlayerPrefs.SetInt("sceneSelectToggled", 0);

        //activate or deactivate it
        levelSelect.SetActive(sceneSelectToggled);
    }


    void Update()
    {
        
    }


    public void restart()
    {
        string lvlString = lvl.ToString();
        if (lvl < 9) lvlString = "0" + lvlString;
        SceneManager.LoadScene("level" + lvlString);
    }

    public void nextLvl()
    {
        lvl++;
        if (lvl > maxLvl) lvl = maxLvl;

        loadLevelAndDisplayString();
    }

    public void lastLvl()
    {
        lvl--;
        if (lvl < 0) lvl = 0;

        loadLevelAndDisplayString();
    }

    private void loadLevelAndDisplayString()
    {
        string lvlString = lvl.ToString();
        if (lvl < 9) lvlString = "0" + lvlString;
        SceneManager.LoadScene("level" + lvlString);

        string lvlStringMax = maxLvl.ToString();
        if (maxLvl < 9) lvlStringMax = "0" + lvlStringMax;
        lvlDisplay.text = lvlString + " / " + lvlStringMax;
    }



    public void toggleLevelSelect()
    {
        //levelSelect.SetActive(!levelSelect.active);

        if (PlayerPrefs.GetInt("sceneSelectToggled") == 1) PlayerPrefs.SetInt("sceneSelectToggled", 0);
        else if (PlayerPrefs.GetInt("sceneSelectToggled") == 0) PlayerPrefs.SetInt("sceneSelectToggled", 1);
        sceneSelectToggled = PlayerPrefs.GetInt("sceneSelectToggled") > 0 ? true : false;
        levelSelect.SetActive(sceneSelectToggled);
        FindObjectOfType<Slowmotion>().toggleSceneSelectPause(PlayerPrefs.GetInt("sceneSelectToggled"));
    }
}
