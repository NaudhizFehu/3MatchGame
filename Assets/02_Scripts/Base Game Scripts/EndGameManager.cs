using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum GameType
{
    Moves,
    Time,
}

[System.Serializable]
public class EndGameRequirements
{
    public GameType gameType;
    public int counterValue;
}

public class EndGameManager : MonoBehaviour
{
    private static EndGameManager endGameManager = null;

    public static EndGameManager instance
    {
        get
        {
            if(endGameManager == null)
            {
                EndGameManager tmpinst = FindObjectOfType(typeof(EndGameManager)) as EndGameManager;
                endGameManager = tmpinst;
            }
            return endGameManager;
        }
    }

    public GameObject youWinPanel;
    public GameObject tryAgainPanel;
    public GameObject moveText;
    public GameObject timeText;
    public Text counter;
    public EndGameRequirements requirements;
    public int currentCounterValue;
    private float timerSeconds = 1;
    private Board board;

    // Start is called before the first frame update
    void Start()
    {
        board = FindObjectOfType<Board>();
        SetupGame();
    }

    private void SetupGame()
    {
        currentCounterValue = requirements.counterValue;
        moveText.SetActive(requirements.gameType == GameType.Moves);
        timeText.SetActive(requirements.gameType == GameType.Time);
        insertCounterText();
    }

    public void DecreaseCounterValue()
    {
        if(board.currentState != GameState.pause)
        {
            currentCounterValue--;
            if(currentCounterValue <= 0)
            {
                LoseGame();
            }
            insertCounterText();
        }
    }

    public void WinGame()
    {
        youWinPanel.SetActive(true);
        currentCounterValue = 0;
        board.currentState = GameState.win;
        FadePanelController fade = FindObjectOfType<FadePanelController>();
        fade.GameOver();
    }

    public void LoseGame()
    {
        tryAgainPanel.SetActive(true);
        board.currentState = GameState.lose;
        currentCounterValue = 0;
        FadePanelController fade = FindObjectOfType<FadePanelController>();
        fade.GameOver();
    }

    private void insertCounterText()
    {
        counter.text = currentCounterValue.ToString();
    }

    // Update is called once per frame
    void Update()
    {
        if(requirements.gameType == GameType.Time && currentCounterValue > 0)
        {
            timerSeconds -= Time.deltaTime;
            if(timerSeconds <= 0)
            {
                timerSeconds = 1;
                DecreaseCounterValue();
            }
        }
    }
}
