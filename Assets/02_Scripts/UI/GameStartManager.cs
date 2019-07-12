using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStartManager : MonoBehaviour
{
    private static GameStartManager gameStartManager = null;

    public static GameStartManager instance
    {
        get
        {
            if(gameStartManager == null)
            {
                GameStartManager tmpinst = FindObjectOfType(typeof(GameStartManager)) as GameStartManager;
                gameStartManager = tmpinst;
            }
            return gameStartManager;
        }
    }

    public GameObject startPanel;
    public GameObject levelPanel;

    // Start is called before the first frame update
    void Start()
    {
        startPanel.SetActive(true);
        levelPanel.SetActive(false);
    }

    public void PlayGame()
    {
        startPanel.SetActive(false);
        levelPanel.SetActive(true);
    }

    public void Home()
    {
        startPanel.SetActive(true);
        levelPanel.SetActive(false);
    }
}
