using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "World", menuName = "Level")]
public class Level : ScriptableObject
{
    [Header("Board Dimensions")]
    public int width;
    public int height;

    [Header("Staring Tiles")]
    public TileType[] boardLayout;

    [Header("Available Beads")]
    public GameObject[] beads;

    [Header("Score Goals")]
    public int[] scoreGoals;

    [Header("End Gmae Requirements")]
    public EndGameRequirements endGameRequirements;
    public BlankGoal[] levelGoals;
}
