using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameState
{
    wait = 0,
    move,
    win,
    lose,
    pause,
}

public enum TileKind
{
    Breakable,
    Blank,
    Lock,
    Concrete,
    Slime,
    Normal,
}

public enum BeadMatchType
{
    None = 0,
    Color,
    Adjacent,
    Colomn_Row,
}

[System.Serializable]
public class MatchType
{
    public BeadMatchType type;
    public string color;
}

[System.Serializable]
public class TileType
{
    public int x;
    public int y;
    public TileKind tileKind;
}

public class Board : MonoBehaviour
{
    [Header("Scriptable Object Stuff")]
    public World world;
    public int level;

    public GameState currentState = GameState.move;

    [Header("Board Dimensions")]
    public int width;//가로 갯수
    public int height;//세로 갯수
    public int offSet;

    [Header("Prefabs")]
    public GameObject tilePrefab;//타일 프리펩
    public GameObject breakableTilePrefab;
    public GameObject lockTilePrefab;
    public GameObject concreteTilePrefab;
    public GameObject slimeTilePrefab;
    public GameObject[] beads;
    public GameObject destroyEffect;

    [Header("Layout")]
    public TileType[] boardLayout;
    private bool[,] blankSpaces;
    private BackgroundTile[,] breakableTiles;
    public BackgroundTile[,] lockTiles;
    private BackgroundTile[,] concreteTiles;
    private BackgroundTile[,] slimeTiles;
    public GameObject[,] allBeads;

    [Header("Match Stuff")]
    public MatchType matchType;
    public Bead currentBead;
    private FindMatches findMatches;
    public int basePieceValue = 20;
    private int streakValue = 1;
    public float refillDelay = 0.5f;
    public int[] scoreGoals;
    private bool makeSlime = true;

    private void Awake()
    {
        if(PlayerPrefs.HasKey("Current Level"))
        {
            level = PlayerPrefs.GetInt("Current Level");
        }
        if(world != null)
        {
            if(level < world.levels.Length)
            {
                if(world.levels[level] != null)
                {
                    width = world.levels[level].width;
                    height = world.levels[level].height;

                    boardLayout = world.levels[level].boardLayout;
                    beads = world.levels[level].beads;
                    scoreGoals = world.levels[level].scoreGoals;

                    GoalManager.instance.levelGoals = world.levels[level].levelGoals;
                    GoalManager.instance.ResetNumberCollected();

                    EndGameManager.instance.requirements = world.levels[level].endGameRequirements;
                }
            }
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        breakableTiles = new BackgroundTile[width, height];
        findMatches = FindObjectOfType<FindMatches>();
        blankSpaces = new bool[width, height];//타일 세팅
        allBeads = new GameObject[width, height];//구슬 세팅
        lockTiles = new BackgroundTile[width, height];
        concreteTiles = new BackgroundTile[width, height];
        slimeTiles = new BackgroundTile[width, height];
        SetUp();//타일 배치
    }

    //비어있는 타일 생성
    public void GenerateBlankSpaces()
    {
        for(int i = 0; i < boardLayout.Length; i++)
        {
            if(boardLayout[i].tileKind == TileKind.Blank)
            {
                blankSpaces[boardLayout[i].x, boardLayout[i].y] = true;
            }
        }
    }

    //젤리포장지 타일 생성
    public void GenerateBreakableTiles()
    {
        //Look at all the tiles in the layout
        for(int i = 0; i < boardLayout.Length; i++)
        {
            //if a tile is a "Jelly" tile
            if(boardLayout[i].tileKind == TileKind.Breakable)
            {
                //Create a "Jelly" tile t that position;
                Vector3 tempPosition = new Vector3(boardLayout[i].x, boardLayout[i].y, 0.1f);
                GameObject tile = Instantiate(breakableTilePrefab, tempPosition, Quaternion.identity);
                breakableTiles[boardLayout[i].x, boardLayout[i].y] = tile.GetComponent<BackgroundTile>();
            }
        }
    }

    private void GenerateLockTiles()
    {
        //Look at all the tiles in the layout
        for (int i = 0; i < boardLayout.Length; i++)
        {
            //if a tile is a "Lock" tile
            if (boardLayout[i].tileKind == TileKind.Lock)
            {
                //Create a "Lock" tile t that position;
                Vector3 tempPosition = new Vector3(boardLayout[i].x, boardLayout[i].y, 0.1f);
                GameObject tile = Instantiate(lockTilePrefab, tempPosition, Quaternion.identity);
                lockTiles[boardLayout[i].x, boardLayout[i].y] = tile.GetComponent<BackgroundTile>();
            }
        }
    }

    private void GenerateConcreteTiles()
    {
        //Look at all the tiles in the layout
        for (int i = 0; i < boardLayout.Length; i++)
        {
            //if a tile is a "Concrete" tile
            if (boardLayout[i].tileKind == TileKind.Concrete)
            {
                //Create a "Concrete" tile t that position;
                Vector3 tempPosition = new Vector3(boardLayout[i].x, boardLayout[i].y, 0.1f);
                GameObject tile = Instantiate(concreteTilePrefab, tempPosition, Quaternion.identity);
                concreteTiles[boardLayout[i].x, boardLayout[i].y] = tile.GetComponent<BackgroundTile>();
            }
        }
    }

    private void GenerateSlimeTiles()
    {
        //Look at all the tiles in the layout
        for (int i = 0; i < boardLayout.Length; i++)
        {
            //if a tile is a "Slime" tile
            if (boardLayout[i].tileKind == TileKind.Slime)
            {
                //Create a "Slime" tile t that position;
                Vector3 tempPosition = new Vector3(boardLayout[i].x, boardLayout[i].y, 0.1f);
                GameObject tile = Instantiate(slimeTilePrefab, tempPosition, Quaternion.identity);
                slimeTiles[boardLayout[i].x, boardLayout[i].y] = tile.GetComponent<BackgroundTile>();
            }
        }
    }

    //타일 배치
    private void SetUp()
    {
        GenerateBlankSpaces();
        GenerateBreakableTiles();
        GenerateLockTiles();
        GenerateConcreteTiles();
        GenerateSlimeTiles();
        //가로 세팅
        for (int i = 0; i < width; i++)
        {
            //세로 세팅
            for(int j = 0; j < height; j++)
            {
                if(!blankSpaces[i, j] && !concreteTiles[i, j] && !slimeTiles[i,j])
                {
                    //위치 조정
                    Vector2 tempPosition = new Vector2(i, j + offSet);
                    Vector2 tilePosition = new Vector3(i, j, 0.01f);
                    //생성
                    GameObject backgroundTile = Instantiate(tilePrefab, tilePosition, Quaternion.identity) as GameObject;
                    backgroundTile.transform.parent = this.transform;
                    backgroundTile.name = string.Format(Define.Vec2Name, i, j);
                    

                    //구슬을 생성하고 배치한다.
                    makeBeadSetup(i, j, tempPosition);
                }
            }
        }

        if(isDeadlocked())
        {
            ShuffleBoard();
        }
    }

    private void makeBeadSetup(int _column, int _row, Vector2 _pos)
    {
        int beadToUse = Random.Range(0, beads.Length);

        //시작부터 구슬이 터지지않도록 재배치한다.
        int maxIterarions = 0;
        while (MatchesAt(_column, _row, beads[beadToUse]) && maxIterarions < 100)
        {
            beadToUse = Random.Range(0, beads.Length);
            maxIterarions++;
        }
        maxIterarions = 0;

        GameObject bead = Instantiate(beads[beadToUse], _pos, Quaternion.identity) as GameObject;
        bead.GetComponent<Bead>().column = _column;
        bead.GetComponent<Bead>().row = _row;
        bead.transform.parent = this.transform;
        bead.name = string.Format(Define.Vec2Name, _column, _row);
        allBeads[_column, _row] = bead;
    }

    //매치가 되었는지 검사한다.
    private bool MatchesAt(int _column, int _row, GameObject _piece)
    {
        if(_column > 1 && _row > 1)
        {
            if(allBeads[_column - 1, _row] != null && allBeads[_column - 2, _row] != null)
            {
                if(allBeads[_column - 1, _row].tag == _piece.tag && allBeads[_column - 2, _row].tag == _piece.tag)
                {
                    return true;
                }
            }
            if (allBeads[_column, _row - 1] != null && allBeads[_column, _row - 2] != null)
            {
                if (allBeads[_column, _row - 1].tag == _piece.tag && allBeads[_column, _row - 2].tag == _piece.tag)
                {
                    return true;
                }
            }
        }
        else if(_column <= 1 || _row <= 1)
        {
            if (_column > 1)
            {
                if(allBeads[_column - 1, _row] != null && allBeads[_column - 2, _row] != null)
                {
                    if (allBeads[_column - 1, _row].tag == _piece.tag && allBeads[_column - 2, _row].tag == _piece.tag)
                    {
                        return true;
                    }
                }
            }
            if (_row > 1)
            {
                if(allBeads[_column, _row - 1] != null && allBeads[_column, _row - 2] != null)
                {
                    if(allBeads[_column, _row - 1].tag == _piece.tag && allBeads[_column, _row - 2].tag == _piece.tag)
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }

    private MatchType ColumnOrRow()
    {
        //Make a copy of the current matches
        List<GameObject> matchCopy = findMatches.currentMatches as List<GameObject>;

        matchType.type = BeadMatchType.None;
        matchType.color = "";

        //Cycle through all of match Copy and decide if a bomb needs to be made
        for(int i = 0; i < matchCopy.Count; i++)
        {
            //Store this bead
            Bead thisBead = matchCopy[i].GetComponent<Bead>();
            string color = matchCopy[i].tag;
            int column = thisBead.column;
            int row = thisBead.row;
            int columnMatch = 0;
            int rowMatch = 0;

            //Cycle throught the rest of the pieces and compare
            for(int j = 0;  j < matchCopy.Count; j++)
            {
                //Store the next bead
                Bead nextBead = matchCopy[j].GetComponent<Bead>();
                if (nextBead == thisBead)
                    continue;

                if(nextBead.column == thisBead.column && nextBead.tag == color)
                {
                    columnMatch++;
                }

                if (nextBead.row == thisBead.row && nextBead.tag == color)
                {
                    rowMatch++;
                }
            }
            //return 1 if it's a color bomb
            //return 2 if adjacent bomb
            //return 3 if column or row bomb
            if(columnMatch == 4 || rowMatch == 4)
            {
                matchType.type = BeadMatchType.Color;
                matchType.color = color;
                return matchType;
            }
            else if(columnMatch == 2 && rowMatch == 2)
            {
                matchType.type = BeadMatchType.Adjacent;
                matchType.color = color;
                return matchType;
            }
            else if(columnMatch == 3 || rowMatch == 3)
            {
                matchType.type = BeadMatchType.Colomn_Row;
                matchType.color = color;
                return matchType;
            }
        }

        matchType.type = BeadMatchType.None;
        matchType.color = "";
        return matchType;
    }

    private void CheckToMakeBombs()
    {
        //How many objects are in findMatches currentMatches?
        if(findMatches.currentMatches.Count > 3)
        {
            //What type of match?
            MatchType typeOfMatch = ColumnOrRow();
            switch(typeOfMatch.type)
            {
                case BeadMatchType.Color:
                    //make Color Bomb
                    //is the current bead matched?
                    if (currentBead != null && currentBead.isMatched && currentBead.tag == typeOfMatch.color)
                    {
                        currentBead.isMatched = false;
                        currentBead.MakeBomb(BombType.Color);
                    }
                    else
                    {
                        if (currentBead.otherBead != null)
                        {
                            Bead otherBead = currentBead.otherBead.GetComponent<Bead>();
                            if (otherBead.isMatched && otherBead.tag == typeOfMatch.color)
                            {
                                otherBead.isMatched = false;
                                otherBead.MakeBomb(BombType.Color);
                            }
                        }
                    }
                    break;

                case BeadMatchType.Adjacent:
                    //make Adjacent Bomb
                    //is the current bead matched?
                    if (currentBead != null && currentBead.isMatched && currentBead.tag == typeOfMatch.color)
                    {
                        currentBead.isMatched = false;
                        currentBead.MakeBomb(BombType.Adjacent);
                    }
                    else
                    {
                        if (currentBead.otherBead != null)
                        {
                            Bead otherBead = currentBead.otherBead.GetComponent<Bead>();
                            if (otherBead.isMatched && otherBead.tag == typeOfMatch.color)
                            {
                                otherBead.isMatched = false;
                                otherBead.MakeBomb(BombType.Adjacent);
                            }
                        }
                    }
                    break;

                case BeadMatchType.Colomn_Row:
                    findMatches.CheckBombs(typeOfMatch);
                    break;
            }
        }
    }

    private void BombRow(int _row)
    {
        for(int i = 0; i < width; i++)
        {
            if(concreteTiles[i,_row])
            {
                concreteTiles[i, _row].TakeDamage(1);
                if(concreteTiles[i, _row].hitPoints <= 0)
                {
                    concreteTiles[i, _row] = null;
                }
            }
        }
    }

    private void BombColumn(int _column)
    {
        for (int i = 0; i < width; i++)
        {
            for(int j = 0; j < height; j++)
            {
                if (concreteTiles[i, i])
                {
                    concreteTiles[_column, i].TakeDamage(1);
                    if (concreteTiles[_column, i].hitPoints <= 0)
                    {
                        concreteTiles[_column, i] = null;
                    }
                }
            }
        }
    }

    //매치가 되었을때 해당 구슬을 제거한다.
    private void DestroyMatchesAt(int _column, int _row)
    {
        if(allBeads[_column, _row].GetComponent<Bead>().isMatched)
        {
            //Does a tile need to break?
            if(breakableTiles[_column, _row] != null)
            {
                //if it does, give one damage
                breakableTiles[_column, _row].TakeDamage(1);
                if(breakableTiles[_column, _row].hitPoints <= 0)
                {
                    breakableTiles[_column, _row] = null;
                }
            }
            if (lockTiles[_column, _row] != null)
            {
                //if it does, give one damage
                lockTiles[_column, _row].TakeDamage(1);
                if (lockTiles[_column, _row].hitPoints <= 0)
                {
                    lockTiles[_column, _row] = null;
                }
            }
            DamageConcrete(_column, _row);
            DamageSlime(_column, _row);

            GoalManager.instance.CompareGoal(allBeads[_column, _row].tag.ToString());
            GoalManager.instance.UpdateGoals();

            //Does the sound manager exist?
            SoundManager.instance.PlayRandomDestoryNoise();

            GameObject Eff = Instantiate(destroyEffect, allBeads[_column, _row].transform.position + Define.FrontPos, Quaternion.identity);
            Eff.GetComponent<ParticleSystemRenderer>().material = Resources.Load(Define.destroyEffPath[BeadColorIndex(allBeads[_column, _row].tag)]) as Material;
            Destroy(Eff, 1f);
            Destroy(allBeads[_column, _row]);
            ScoreManager.instance.IncreaseScore(basePieceValue * streakValue);
            allBeads[_column, _row] = null;
        }
    }

    private void DamageConcrete(int _column, int _row)
    {
        if(_column > 0)
        {
            if(concreteTiles[_column - 1, _row])
            {
                concreteTiles[_column - 1, _row].TakeDamage(1);
                if (concreteTiles[_column - 1, _row].hitPoints <= 0)
                {
                    concreteTiles[_column - 1, _row] = null;
                }
            }
        }
        if (_column < width - 1)
        {
            if (concreteTiles[_column + 1, _row])
            {
                concreteTiles[_column + 1, _row].TakeDamage(1);
                if (concreteTiles[_column + 1, _row].hitPoints <= 0)
                {
                    concreteTiles[_column + 1, _row] = null;
                }
            }
        }
        if (_row > 0)
        {
            if (concreteTiles[_column, _row - 1])
            {
                concreteTiles[_column, _row - 1].TakeDamage(1);
                if (concreteTiles[_column, _row - 1].hitPoints <= 0)
                {
                    concreteTiles[_column, _row - 1] = null;
                }
            }
        }
        if (_row < height - 1)
        {
            if (concreteTiles[_column, _row + 1])
            {
                concreteTiles[_column, _row + 1].TakeDamage(1);
                if (concreteTiles[_column, _row + 1].hitPoints <= 0)
                {
                    concreteTiles[_column, _row + 1] = null;
                }
            }
        }
    }

    private void DamageSlime(int _column, int _row)
    {
        if (_column > 0)
        {
            if (slimeTiles[_column - 1, _row])
            {
                slimeTiles[_column - 1, _row].TakeDamage(1);
                if (slimeTiles[_column - 1, _row].hitPoints <= 0)
                {
                    slimeTiles[_column - 1, _row] = null;
                }
                makeSlime = false;
            }
        }
        if (_column < width - 1)
        {
            if (slimeTiles[_column + 1, _row])
            {
                slimeTiles[_column + 1, _row].TakeDamage(1);
                if (slimeTiles[_column + 1, _row].hitPoints <= 0)
                {
                    slimeTiles[_column + 1, _row] = null;
                }
                makeSlime = false;
            }
        }
        if (_row > 0)
        {
            if (slimeTiles[_column, _row - 1])
            {
                slimeTiles[_column, _row - 1].TakeDamage(1);
                if (slimeTiles[_column, _row - 1].hitPoints <= 0)
                {
                    slimeTiles[_column, _row - 1] = null;
                }
                makeSlime = false;
            }
        }
        if (_row < height - 1)
        {
            if (slimeTiles[_column, _row + 1])
            {
                slimeTiles[_column, _row + 1].TakeDamage(1);
                if (slimeTiles[_column, _row + 1].hitPoints <= 0)
                {
                    slimeTiles[_column, _row + 1] = null;
                }
                makeSlime = false;
            }
        }
    }

    private int BeadColorIndex(string _tag)
    {
        for(int i = 0; i < beads.Length; i++)
        {
            if(_tag == beads[i].tag)
            {
                return i;
            }
        }
        return 0;
    }


    public void  DestroyMatches()
    {
        if (findMatches.currentMatches.Count >= 4)
        {
            CheckToMakeBombs();
        }
        findMatches.currentMatches.Clear();

        for (int i = 0; i < width; i++)
        {
            for(int j = 0; j < height; j++)
            {
                if(allBeads[i, j] != null)
                {
                    DestroyMatchesAt(i, j);
                }
            }
        }
        StartCoroutine(DecreaseRowCo2());
    }

    private IEnumerator DecreaseRowCo2()
    {
        for(int i = 0; i < width; i++)
        {
            for(int j = 0; j < height; j++)
            {
                //if the current spot isn't blank or empty
                if(!blankSpaces[i, j] && allBeads[i,j] == null && !concreteTiles[i,j] && !slimeTiles[i,j])
                {
                    //loop from the space above to the top of the column.
                    for(int k = j + 1; k < height; k++)
                    {
                        //if a bead is found
                        if(allBeads[i, k] != null)
                        {
                            //move that bead to this empty space
                            allBeads[i, k].GetComponent<Bead>().row = j;
                            //get that spot to be null
                            allBeads[i, k] = null;
                            //break out of the loop
                            break;
                        }
                    }
                }
            }
        }
        yield return new WaitForSeconds(refillDelay * 0.5f);
        StartCoroutine(FillBoardCo());
    }

    //매치가되어 구슬이 사라졌다면 새로운 구슬을 재배치한다.
    private IEnumerator DecreaseRowCo()
    {
        int nullCount = 0;
        for(int i = 0; i < width; i++)
        {
            for(int j = 0; j < height; j++)
            {
                if(allBeads[i, j] == null)
                {
                    nullCount++;
                }
                else if(nullCount > 0)
                {
                    allBeads[i, j].GetComponent<Bead>().row -= nullCount;
                    allBeads[i, j] = null;
                }
            }
            nullCount = 0;
        }
        yield return new WaitForSeconds(refillDelay * 0.5f);
        StartCoroutine(FillBoardCo());
    }

    private void RefillBoard()
    {
        for(int i = 0; i < width; i++)
        {
            for(int j = 0; j < height; j++)
            {
                if(allBeads[i, j] == null && !blankSpaces[i, j] && !concreteTiles[i,j] && !slimeTiles[i,j])
                {
                    Vector2 tempPosition = new Vector2(i, j + offSet);
                    int beadToUse = Random.Range(0, beads.Length);
                    int maxIterations = 0;
                    while(MatchesAt(i, j, beads[beadToUse]) && maxIterations < 100)
                    {
                        maxIterations++;
                        beadToUse = Random.Range(0, beads.Length);
                    }
                    maxIterations = 0;

                    GameObject piece = Instantiate(beads[beadToUse], tempPosition, Quaternion.identity);
                    allBeads[i, j] = piece;
                    piece.GetComponent<Bead>().column = i;
                    piece.GetComponent<Bead>().row = j;
                    //piece.transform.parent = this.transform;
                    //piece.name = string.Format(Define.Vec2Name, i, j);
                }
            }
        }
    }

    private bool MatchesOnBoard()
    {
        findMatches.FindAllMatches();

        for(int i = 0; i < width; i++)
        {
            for(int j = 0; j < height; j++)
            {
                if(allBeads[i, j] != null)
                {
                    if(allBeads[i, j].GetComponent<Bead>().isMatched)
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }

    private IEnumerator FillBoardCo()
    {
        yield return new WaitForSeconds(refillDelay);
        RefillBoard();
        yield return new WaitForSeconds(refillDelay);
        while (MatchesOnBoard())
        {
            streakValue++;
            DestroyMatches();
            yield break;
        }
        currentBead = null;
        CheckToMakeSlime();
        if (isDeadlocked())
        {
            StartCoroutine(ShuffleBoard());
            //ShuffleBoard();
            //string str = "<color=red>Deadlocked!!!</color>";
            //Debug.Log(str);
        }
        yield return new WaitForSeconds(refillDelay);

        //매칭이 가능한 갯수 표기
        //int remainderMatch = isDeadlocked();
        //if (remainderMatch == 0)
        //{
        //    Debug.Log("Deadlocked!!!");
        //}
        //Debug.Log(string.Format("remainder match : {0}", remainderMatch));
        System.GC.Collect();
        if(currentState != GameState.pause)
            currentState = GameState.move;
        makeSlime = true;
        streakValue = 1;
    }

    private void CheckToMakeSlime()
    {
        //Check the slime tiles array
        for(int i=0; i < width; i++)
        {
            for(int j =0; j < height; j++)
            {
                if(slimeTiles[i,j] != null && makeSlime)
                {
                    //Call another method to make a new slime
                    MakeNewSlime();
                }
            }
        }
    }

    private Vector2 CheckForAdjacent(int _column, int _row)
    {
        if(allBeads[_column + 1, _row] && _column < width - 1)
        {
            return Vector2.right;
        }
        if (allBeads[_column - 1, _row] && _column > 0)
        {
            return Vector2.left;
        }
        if (allBeads[_column, _row + 1] && _row < height - 1)
        {
            return Vector2.up;
        }
        if (allBeads[_column, _row - 1] && _row > 0)
        {
            return Vector2.down;
        }
        return Vector2.zero;
    }

    private void MakeNewSlime()
    {
        bool isslime = false;
        int loops = 0;
        while(!isslime && loops < (width * height))
        {
            int newX = Random.Range(1, width-1);
            int newY = Random.Range(1, height-1);
            if(slimeTiles[newX, newY])
            {
                Vector2 adjacent = CheckForAdjacent(newX, newY);
                if(adjacent != Vector2.zero)
                {
                    Destroy(allBeads[newX + (int)adjacent.x, newY + (int)adjacent.y]);
                    Vector2 tempPosition = new Vector2(newX + (int)adjacent.x, newY + (int)adjacent.y);
                    GameObject tile = Instantiate(slimeTilePrefab, tempPosition, Quaternion.identity);
                    slimeTiles[newX + (int)adjacent.x, newY + (int)adjacent.y] = tile.GetComponent<BackgroundTile>();
                    isslime = true;
                }
            }
            loops++;
        }
    }

    private void SwitchPieces(int _column, int _row, Vector2 _direction)
    {
        //Take the second piece and save it in a holder
        GameObject holder = allBeads[_column + (int)_direction.x, _row + (int)_direction.y] as GameObject;
        //switching the first bead to be the second position
        allBeads[_column + (int)_direction.x, _row + (int)_direction.y] = allBeads[_column, _row];
        //Set the first bead to be the second bead
        allBeads[_column, _row] = holder;
    }

    private bool CheckForMatches()
    {
        for(int i = 0; i < width; i++)
        {
            for(int j = 0; j < height; j++)
            {
                if(allBeads[i, j] != null)
                {
                    //Make sure that one and two to the right are in the board
                    if(i < width - 2)
                    {
                        //Check if the beads to the rifht and two to the right exist
                        //GameObject bead1 = allBeads[i + 1, j];
                        //GameObject bead2 = allBeads[i + 2, j];
                        if(allBeads[i + 1, j] != null && allBeads[i + 2, j] != null)
                        {
                            if(allBeads[i + 1, j].tag == allBeads[i, j].tag && allBeads[i + 2, j].tag == allBeads[i, j].tag)
                            {
                                return true;
                            }
                        }
                    }

                    if(j < height - 2)
                    {
                        //Check if the beads above exist
                        if (allBeads[i, j + 1] != null && allBeads[i, j + 2] != null)
                        {
                            if (allBeads[i, j + 1].tag == allBeads[i, j].tag && allBeads[i, j + 2].tag == allBeads[i, j].tag)
                            {
                                return true;
                            }
                        }
                    }
                }
            }
        }
        return false;
    }

    public bool SwitchAndCheck(int _column, int _row, Vector2 _direction)
    {
        if(!blankSpaces[_column,_row] || !concreteTiles[_column,_row] || !slimeTiles[_column,_row])
        {
            SwitchPieces(_column, _row, _direction);
            if(CheckForMatches())
            {
                SwitchPieces(_column, _row, _direction);
                return true;
            }
            SwitchPieces(_column, _row, _direction);
        }
        return false;
    }

    private bool isDeadlocked()
    {
        for(int i = 0; i < width; i++)
        {
            for(int j = 0; j < height; j++)
            {
                if(allBeads[i,j] != null)
                {
                    if(i < width - 1)
                    {
                        if(SwitchAndCheck(i, j, Vector2.right))
                        {
                            //Matching++;
                            return false;
                        }
                    }

                    if(j < height - 1)
                    {
                        if(SwitchAndCheck(i, j, Vector2.up))
                        {
                            //Matching++;
                            return false;
                        }
                    }
                }
            }
        }
        return true;
    }

    //DeadLock 발생시 셔플
    private IEnumerator ShuffleBoard()
    {
        yield return new WaitForSeconds(0.5f);
        //Create a list game objects
        List<GameObject> newBoard = new List<GameObject>();
        //Add every piece to this list
        for(int i = 0; i < width; i++)
        {
            for(int j = 0; j < height; j++)
            {
                if(allBeads[i, j] != null)
                {
                    newBoard.Add(allBeads[i, j]);
                }
            }
        }
        yield return new WaitForSeconds(0.5f);
        //for every spot on the board
        for(int i = 0; i < width; i++)
        {
            for(int j = 0; j < height; j++)
            {
                //if this spot shouldn't be blank
                if(!blankSpaces[i,j] && !concreteTiles[i,j] && !slimeTiles[i,j])
                {
                    //Pick a random number
                    int pieceToUse = Random.Range(0, newBoard.Count);
                    int maxIterarions = 0;
                    while (MatchesAt(i, j, newBoard[pieceToUse]) && maxIterarions < 100)
                    {
                        pieceToUse = Random.Range(0, newBoard.Count);
                        maxIterarions++;
                    }
                    //Make a container for the piece
                    maxIterarions = 0;
                    Bead piece = newBoard[pieceToUse].GetComponent<Bead>();
                    //Assign the column, row the piece
                    piece.column = i;
                    piece.row = j;
                    //Fill in the beads array with this new piece
                    allBeads[i, j] = newBoard[pieceToUse];
                    //Remove it from the list
                    newBoard.Remove(newBoard[pieceToUse]);
                }
            }
        }

        //Check if it's still deadlocked
        if(isDeadlocked())
        {
            ShuffleBoard();
        }
    }

    //TestCode
    public void Resetbead()
    {
        ShuffleBoard();
    }
}
