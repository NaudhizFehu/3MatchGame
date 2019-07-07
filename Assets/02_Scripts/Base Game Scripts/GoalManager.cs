using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GoalType
{

}

[System.Serializable]
public class BlankGoal
{
    public int numberNeeded;
    public int numberCollected;
    public Sprite goalSprite;
    public string matchValue;
}

public class GoalManager : MonoBehaviour
{
    private static GoalManager goalManager = null;
    
    public static GoalManager instance
    {
        get
        {
            if(goalManager == null)
            {
                GoalManager tmpinst = FindObjectOfType(typeof(GoalManager)) as GoalManager;
                goalManager = tmpinst;
            }
            return goalManager;
        }
    }

    public BlankGoal[] levelGoals;
    public List<GoalPanel> currentGoals = new List<GoalPanel>();
    public GameObject goalPrefab;
    public GameObject goalIntroParent;
    public GameObject goalGameParent;

    // Start is called before the first frame update
    void Start()
    {
        SetupIntroGoals();
    }

    public void ResetNumberCollected()
    {
        for(int i = 0; i < levelGoals.Length; i++)
        {
            levelGoals[i].numberCollected = 0;
        }
    }

    private void SetupIntroGoals()
    {
        for(int i = 0; i < levelGoals.Length; i++)
        {
            //Create a new Goal Panel at the goalIntroParent position
            GameObject goal = Instantiate(goalPrefab, goalIntroParent.transform.position, Quaternion.identity);
            goal.transform.SetParent(goalIntroParent.transform);
            goal.transform.localScale = Vector3.one;//하지않으면 스케일이 변경된다
            //Set the image and text of the goal
            GoalPanel panel = goal.GetComponent<GoalPanel>();
            panel.thisSprite = levelGoals[i].goalSprite;
            panel.thisString = "0/" + levelGoals[i].numberNeeded;

            GameObject Gamegoal = Instantiate(goalPrefab, goalGameParent.transform.position, Quaternion.identity);
            Gamegoal.transform.SetParent(goalGameParent.transform);
            Gamegoal.transform.localScale = new Vector3(0.5f, 1f, 1f);//하지않으면 스케일이 변경된다
            panel = Gamegoal.GetComponent<GoalPanel>();
            currentGoals.Add(panel);
            panel.thisSprite = levelGoals[i].goalSprite;
            panel.thisString = "0/" + levelGoals[i].numberNeeded;
        }
    }

    public void UpdateGoals()
    {
        int goalsCompleted = 0;
        for(int i = 0; i < levelGoals.Length; i++)
        {
            currentGoals[i].thisText.text = string.Format("{0}/{1}", levelGoals[i].numberCollected, levelGoals[i].numberNeeded);
            if(levelGoals[i].numberCollected >= levelGoals[i].numberNeeded)
            {
                goalsCompleted++;
                currentGoals[i].thisText.text = string.Format("{0}/{1}", levelGoals[i].numberNeeded, levelGoals[i].numberNeeded);
            }
        }
        if(goalsCompleted >= levelGoals.Length)
        {
            EndGameManager.instance.WinGame();
        }
    }

    public void CompareGoal(string _goalToCompare)
    {
        for(int i = 0; i < levelGoals.Length; i++)
        {
            if(_goalToCompare == levelGoals[i].matchValue)
            {
                levelGoals[i].numberCollected++;
            }
        }
    }
}
