using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class UiManager : MonoBehaviour
{

    [SerializeField] private TextMeshProUGUI shotDisplay;
    [SerializeField] private TextMeshProUGUI timeDisplay;
    [Space]
    [SerializeField] private Image greyBackground;
    [SerializeField] private GameObject levelSelect;
    [SerializeField] private TextMeshProUGUI lvlDisplay;
    [Space]
    [Header("Tutorial")]
    [SerializeField] private GameObject tutorialParent;
    [SerializeField] private GameObject tutorialScreenBlock;
    [SerializeField] private GameObject tutMouse;
    [SerializeField] private GameObject tutClick;

    private bool sceneSelectToggled;

    void Awake()
    {
        // Before loading this scene, won the other scene or switched via menu?
        if (PlayerPrefs.HasKey("sceneSelectToggled"))
            sceneSelectToggled = PlayerPrefs.GetInt("sceneSelectToggled") > 0 ? true : false;
        else
            PlayerPrefs.SetInt("sceneSelectToggled", 0);

        levelSelect.SetActive(sceneSelectToggled);
    }



    

    public void displayShots(int remainingShots)
    {
        shotDisplay.text = "Shots: " + remainingShots;
    }


    public void updateLevelDisplay(string lvlString, int maxLvl)
    {
        string lvlStringMax = maxLvl.ToString();
        if (maxLvl < 9) lvlStringMax = "0" + lvlStringMax;
        lvlDisplay.text = lvlString + " / " + lvlStringMax;
    }

    public void toggleLevelSelect()
    {
        if (PlayerPrefs.GetInt("sceneSelectToggled") == 1) PlayerPrefs.SetInt("sceneSelectToggled", 0);
        else if (PlayerPrefs.GetInt("sceneSelectToggled") == 0) PlayerPrefs.SetInt("sceneSelectToggled", 1);
        sceneSelectToggled = PlayerPrefs.GetInt("sceneSelectToggled") > 0 ? true : false;
        levelSelect.SetActive(sceneSelectToggled);
        FindObjectOfType<Slowmotion>().toggleSceneSelectPause(PlayerPrefs.GetInt("sceneSelectToggled"));
    }


    // Mouse moving to show how to drag
    public void playtutorialAnimation()
    {
        if(!sceneSelectToggled)
            StartCoroutine(tutorialAnim());
    }

    private IEnumerator tutorialAnim()
    {
        tutorialParent.SetActive(true);

        var startPos = new Vector2(75, 50);
        var endPos = new Vector2(-75, -50);
        var clickOffset = new Vector2(-24, 24);

        var cursor = tutMouse.GetComponent<RectTransform>();
        var cursorColor = cursor.GetComponent<Image>();
        var click = tutClick.GetComponent<RectTransform>();
        var clickColor = click.GetComponent<Image>();

        var lightGray = new Color(0.75f,0.75f,0.75f,1);

        // Prepare UI

        clickColor.color = Color.clear;
        cursor.DOAnchorPos(startPos, 1f);
        click.DOAnchorPos(startPos + clickOffset, 1f);
        click.DOScale(new Vector2(0,0), 0f); // or start with Vector2(2,2) but then have a short delay before click appears


        yield return new WaitForSeconds(1f);


        // Start mouse drag anim

        Sequence seq = DOTween.Sequence().SetLoops(-1);
        seq.Append(cursor.DOShakeScale(0.25f, 0.75f, 10));
        seq.Append(cursor.DOScale(new Vector2(0.75f, 0.75f), 0.25f));//seq.Join //seq.Insert  //cursor.DOPunchScale(new Vector2(-0.5f, -0.5f), 1f, 3)
        //seq.Insert(0, cursorColor.DOColor(lightGray, 0.5f));
        seq.Insert(0, clickColor.DOColor(Color.white, 0.5f));
        seq.Insert(0, click.DOScale(Vector2.one, 0.5f));


        seq.Append(cursor.DOAnchorPos(endPos, 1.25f).SetEase(Ease.InOutCubic));
        seq.Join(click.DOAnchorPos(endPos + clickOffset, 1.25f).SetEase(Ease.InOutCubic));

        seq.Append(cursor.DOScale(Vector2.one, 0.5f));
        seq.Join(cursorColor.DOColor(Color.white, 0.5f));
        seq.Join(click.DOScale(new Vector2(2,2), 0.5f));
        seq.Join(clickColor.DOColor(Color.clear, 0.5f));

        seq.AppendInterval(0.5f);
        seq.Append(cursor.DOAnchorPos(startPos, 0.75f).SetEase(Ease.InOutSine));
        seq.AppendInterval(0.25f);

        // TODO: use more .setEase()


        yield return new WaitForSeconds(2.5f);

        // Now player can use input

        tutorialScreenBlock.GetComponent<Image>().DOColor(Color.clear, 0.5f);
        yield return new WaitForSeconds(0.5f);
        tutorialScreenBlock.SetActive(false);
        FindObjectOfType<BallMovement>().notInTut();
    }
}
