﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{
    private static PauseManager pauseManager;

    public static PauseManager instance
    {
        get
        {
            if(pauseManager == null)
            {
                PauseManager tmpinst = FindObjectOfType(typeof(PauseManager)) as PauseManager;
                pauseManager = tmpinst;
            }
            return pauseManager;
        }
    }

    public GameObject pausePanel;
    private Board board;
    public bool paused = false;
    public Image soundButton;
    public Sprite musicOnSprite;
    public Sprite musicOffSprite;

    // Start is called before the first frame update
    void Start()
    {
        // In Player Prefs, the "Sound" key is for sound
        // if sound == 0, then mute, if sound == 1, then unmute
        if(PlayerPrefs.HasKey("Sound"))
        {
            if(PlayerPrefs.GetInt("Sound") == 0)
            {
                soundButton.sprite = musicOffSprite;
            }
            else
            {
                soundButton.sprite = musicOnSprite;
            }
        }
        else
        {
            soundButton.sprite = musicOnSprite;
        }

        pausePanel.SetActive(false);
        board = GameObject.FindWithTag("Board").GetComponent<Board>();
    }

    // Update is called once per frame
    void Update()
    {
        if(paused && !pausePanel.activeInHierarchy)
        {
            pausePanel.SetActive(true);
            board.currentState = GameState.pause;
        }
        if(!paused && pausePanel.activeInHierarchy)
        {
            pausePanel.SetActive(false);
            board.currentState = GameState.move;
        }
    }

    public void SoundButton()
    {
        if (PlayerPrefs.HasKey("Sound"))
        {
            if (PlayerPrefs.GetInt("Sound") == 0)
            {
                soundButton.sprite = musicOnSprite;
                PlayerPrefs.SetInt("Sound", 1);
                SoundManager.instance.adjustVolume();
            }
            else
            {
                soundButton.sprite = musicOffSprite;
                PlayerPrefs.SetInt("Sound", 0);
                SoundManager.instance.adjustVolume();
            }
        }
        else
        {
            soundButton.sprite = musicOffSprite;
            PlayerPrefs.SetInt("Sound", 0);
            SoundManager.instance.adjustVolume();
        }
    }

    public void PauseGame()
    {
        paused = !paused;
    }

    public void ExitGame()
    {
        SceneManager.LoadScene("Splash");
    }
}
