using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameState
{
    wait = 0,
    move,
}

public enum TileKind
{
    Breakable,
    Blank,
    Normal,

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
    public GameState currentState = GameState.move;
    public int width;//가로 갯수
    public int height;//세로 갯수
    public int offSet;
    public GameObject tilePrefab;//타일 프리펩
    public GameObject breakableTilePrefab;
    public TileType[] boardLayout;
    private bool[,] blankSpaces;
    private BackgroundTile[,] breakableTiles;
    public GameObject[] beads;
    public GameObject destroyEffect;
    public GameObject[,] allBeads;
    public Bead currentBead;
    private FindMatches findMatches;
    public int basePieceValue = 20;
    private int streakValue = 1;
    private ScoreManager scoreManager;
    public float refillDelay = 0.5f;

    //test
    private ResetBtn m_resetBtn;

    // Start is called before the first frame update
    void Start()
    {
        scoreManager = FindObjectOfType<ScoreManager>();
        breakableTiles = new BackgroundTile[width, height];
        findMatches = FindObjectOfType<FindMatches>();
        blankSpaces = new bool[width, height];//타일 세팅
        allBeads = new GameObject[width, height];//구슬 세팅
        SetUp();//타일 배치
        //m_resetBtn = makeResetBtn();

    }

    private void LateUpdate()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
    }

    private ResetBtn makeResetBtn()
    {
        GameObject btn = Instantiate(Resources.Load(string.Format("02_Prefabs/03_UI/ResetBtn"))) as GameObject;
        btn.transform.parent = this.transform;
        btn.transform.localPosition = new Vector3(3f, -1.5f, 0f);
        return btn.GetComponent<ResetBtn>();
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
    
    //타일 배치
    private void SetUp()
    {
        GenerateBlankSpaces();
        GenerateBreakableTiles();
        //가로 세팅
        for (int i = 0; i < width; i++)
        {
            //세로 세팅
            for(int j = 0; j < height; j++)
            {
                if(!blankSpaces[i, j])
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

    private bool ColumnOrRow()
    {
        int numberHorizontal = 0;
        int numberVertical = 0;
        Bead firstPiece = findMatches.currentMatches[0].GetComponent<Bead>();
        if(firstPiece != null)
        {
            foreach(GameObject currentPiece in findMatches.currentMatches)
            {
                Bead bead = currentPiece.GetComponent<Bead>();
                if(bead.row == firstPiece.row)
                {
                    numberHorizontal++;
                }
                if(bead.column == firstPiece.column)
                {
                    numberVertical++;
                }
            }
        }

        return (numberHorizontal == 5 || numberVertical == 5);
    }

    private void CheckToMakeBombs()
    {
        //Column, Row Bomb
        if(findMatches.currentMatches.Count == 4 || findMatches.currentMatches.Count == 7)
        {
            findMatches.CheckBombs();
        }

        //Color, Adjacent Bomb
        if(findMatches.currentMatches.Count == 5 || findMatches.currentMatches.Count == 8)
        {
            if(ColumnOrRow())
            {
                //make Color Bomb
                //is the current bead matched?
                if(currentBead != null)
                {
                    if(currentBead.isMatched)
                    {
                        if(!currentBead.isColorBomb)
                        {
                            currentBead.isMatched = false;
                            currentBead.MakeBomb(BombType.Color);
                        }
                    }
                    else
                    {
                        if(currentBead.otherBead != null)
                        {
                            Bead otherBead = currentBead.otherBead.GetComponent<Bead>();
                            if(otherBead.isMatched)
                            {
                                if (!otherBead.isColorBomb)
                                {
                                    otherBead.isMatched = false;
                                    otherBead.MakeBomb(BombType.Color);
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                //make Adjacent Bomb
                //is the current bead matched?
                if (currentBead != null)
                {
                    if (currentBead.isMatched)
                    {
                        if (!currentBead.isAdjacentBomb)
                        {
                            currentBead.isMatched = false;
                            currentBead.MakeBomb(BombType.Adjacent);
                        }
                    }
                    else
                    {
                        if (currentBead.otherBead != null)
                        {
                            Bead otherBead = currentBead.otherBead.GetComponent<Bead>();
                            if(otherBead.isMatched)
                            {
                                if (!otherBead.isAdjacentBomb)
                                {
                                    otherBead.isMatched = false;
                                    otherBead.MakeBomb(BombType.Adjacent);
                                }
                            }
                        }
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
            currentState = GameState.wait;

            if(findMatches.currentMatches.Count >= 4)
            {
                CheckToMakeBombs();
            }

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

            GameObject Eff = Instantiate(destroyEffect, allBeads[_column, _row].transform.position + Define.FrontPos, Quaternion.identity);
            Eff.GetComponent<ParticleSystemRenderer>().material = Resources.Load(Define.destroyEffPath[BeadColorIndex(allBeads[_column, _row].tag)]) as Material;
            Destroy(Eff, 1f);
            Destroy(allBeads[_column, _row]);
            scoreManager.IncreaseScore(basePieceValue * streakValue);
            allBeads[_column, _row] = null;

            currentState = GameState.move;
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


    public void DestroyMatches()
    {
        for(int i = 0; i < width; i++)
        {
            for(int j = 0; j < height; j++)
            {
                if(allBeads[i, j] != null)
                {
                    DestroyMatchesAt(i, j);
                }
            }
        }
        findMatches.currentMatches.Clear();
        StartCoroutine(DecreaseRowCo2());
    }

    private IEnumerator DecreaseRowCo2()
    {
        for(int i = 0; i < width; i++)
        {
            for(int j = 0; j < height; j++)
            {
                //if the current spot isn't blank or empty
                if(!blankSpaces[i, j] && allBeads[i,j] == null)
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
                if(allBeads[i, j] == null && !blankSpaces[i, j])
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
                    piece.GetComponent<Bead>().column = i;
                    piece.GetComponent<Bead>().row = j;
                    piece.transform.parent = this.transform;
                    piece.name = string.Format(Define.Vec2Name, i, j);
                    allBeads[i, j] = piece;
                }
            }
        }
    }

    private bool MatchesOnBoard()
    {
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
        RefillBoard();
        yield return new WaitForSeconds(refillDelay);
        while(MatchesOnBoard())
        {
            streakValue++;
            DestroyMatches();
            yield return new WaitForSeconds(2 * refillDelay);
        }
        findMatches.currentMatches.Clear();
        currentBead = null;

        if (isDeadlocked())
        {
            ShuffleBoard();
            string str = "<color=red>Deadlocked!!!</color>";
            Debug.Log(str);
        }
        yield return new WaitForSeconds(refillDelay);

        //매칭이 가능한 갯수 표기
        //int remainderMatch = isDeadlocked();
        //if (remainderMatch == 0)
        //{
        //    Debug.Log("Deadlocked!!!");
        //}
        //Debug.Log(string.Format("remainder match : {0}", remainderMatch));
        currentState = GameState.move;
        streakValue = 1;
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
        SwitchPieces(_column, _row, _direction);
        if(CheckForMatches())
        {
            SwitchPieces(_column, _row, _direction);
            return true;
        }
        SwitchPieces(_column, _row, _direction);
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
    private void ShuffleBoard()
    {
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
        //for every spot on the board
        for(int i = 0; i < width; i++)
        {
            for(int j = 0; j < height; j++)
            {
                //if this spot shouldn't be blank
                if(!blankSpaces[i,j])
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
        //모든 구슬 제거
        for(int i = 0; i < width; i++)
        {
            for(int j = 0; j < height; j++)
            {
                Destroy(allBeads[i, j]);
                allBeads[i, j] = null;
            }
        }

        //재세팅
        GenerateBlankSpaces();
        GenerateBreakableTiles();
        //가로 세팅
        for (int i = 0; i < width; i++)
        {
            //세로 세팅
            for (int j = 0; j < height; j++)
            {
                if (!blankSpaces[i, j])
                {
                    //위치 조정
                    Vector2 tempPosition = new Vector2(i, j + offSet);


                    //구슬을 생성하고 배치한다.
                    makeBeadSetup(i, j, tempPosition);
                }
            }
        }
    }
}
