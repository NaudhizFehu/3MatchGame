using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelButton : MonoBehaviour
{
    [Header("Active Stuff")]
    public bool isActive;
    public Sprite activeSprite;
    public Sprite lockedSprite;
    private Image buttonImage;
    private Button myButton;
    private int starsActive;

    [Header("Level UI")]
    public Image[] stars;
    public Text levelText;
    public int level;
    public GameObject confirmPanel;

    private GameData gameData;

    // Start is called before the first frame update
    void Start()
    {
        gameData = FindObjectOfType<GameData>();
        buttonImage = GetComponent<Image>();
        myButton = GetComponent<Button>();
        LoadData();
        ShowLevel();
        ActivateStars();
        DecideSprite();
    }

    private void LoadData()
    {
        //Is GameData present?
        if(gameData != null)
        {
            //Decide if the level is active
            isActive = gameData.saveData.isActive[level - 1];

            //Decide how many stars to activate
            starsActive = gameData.saveData.stars[level - 1];
        }
    }

    private void ActivateStars()
    {
        for(int i = 0; i< starsActive; i++)
        {

            stars[i].enabled = true;
        }
    }

    private void DecideSprite()
    {
        buttonImage.sprite = isActive ? activeSprite : lockedSprite;
        myButton.enabled = isActive;
        levelText.enabled = isActive;
    }

    private void ShowLevel()
    {
        levelText.text = level.ToString();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ConfirmPanel(int _level)
    {
        confirmPanel.GetComponent<ConfirmPanel>().level = _level;
        confirmPanel.SetActive(true);
    }
}
