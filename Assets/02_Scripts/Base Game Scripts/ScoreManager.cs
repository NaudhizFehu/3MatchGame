using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreManager : MonoBehaviour
{
    private static ScoreManager scoreManager = null;

    public static ScoreManager instance
    {
        get
        {
            if(scoreManager == null)
            {
                ScoreManager tmpinst = FindObjectOfType(typeof(ScoreManager)) as ScoreManager;
                scoreManager = tmpinst;
            }
            return scoreManager;
        }
    }

    private Board board;
    public Text scoreText;
    public int score;
    public Image scoreBar;
    private GameData gameData;
    private int numberStars;

    // Start is called before the first frame update
    void Start()
    {
        board = FindObjectOfType<Board>();
        gameData = FindObjectOfType<GameData>();
        scoreBar.fillAmount = 0f;
    }

    // Update is called once per frame
    void Update()
    {
        scoreText.text = score.ToString();
    }

    public void IncreaseScore(int _amountToIncrease)
    {
        score += _amountToIncrease;
        for(int i = 0; i < board.scoreGoals.Length; i++)
        {
            if(score > board.scoreGoals[i] && numberStars < i + 1)
            {
                numberStars++;
            }
        }
        if(gameData != null)
        {
            int highScore = gameData.saveData.highScores[board.level];
            if(score > highScore && board.currentState == GameState.win)
            {
                gameData.saveData.highScores[board.level] = score;
            }

            int currentStars = gameData.saveData.stars[board.level];
            if(numberStars > currentStars)
            {
                gameData.saveData.stars[board.level] = numberStars;
            }

            gameData.Save();
        }
        UpdataBar();
    }

    private void UpdataBar()
    {
        if (board != null && scoreBar != null)
        {
            int length = board.scoreGoals.Length;
            scoreBar.fillAmount = (float)score / (float)board.scoreGoals[length - 1];
        }
    }
}
