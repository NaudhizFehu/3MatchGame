using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameState
{
    wait = 0,
    move,
}

public class Board : MonoBehaviour
{
    public GameState currentState = GameState.move;
    public int width;//가로 갯수
    public int height;//세로 갯수
    public int offSet;
    public GameObject tilePrefab;//타일 프리펩
    private BackgroundTile[,] allTiles;//타일수
    public GameObject[] beads;
    public GameObject[,] allBeads;
    private FindMatches findMatches;

    //test
    public GameObject ResetBtnobj;
    private ResetBtn m_resetBtn;

    // Start is called before the first frame update
    void Start()
    {
        findMatches = FindObjectOfType<FindMatches>();
        allTiles = new BackgroundTile[width, height];//타일 세팅
        allBeads = new GameObject[width, height];//구슬 세팅
        SetUp();//타일 배치
        m_resetBtn = makeResetBtn();

    }

    private ResetBtn makeResetBtn()
    {
        GameObject btn = Instantiate(Resources.Load(string.Format("Button/ResetBtn"))) as GameObject;
        btn.transform.parent = this.transform;
        btn.transform.localPosition = new Vector3(3f, -1.5f, 0f);
        return btn.GetComponent<ResetBtn>();
    }
    
    //타일 배치
    private void SetUp()
    {
        //가로 세팅
        for (int i = 0; i < width; i++)
        {
            //세로 세팅
            for(int j = 0; j < height; j++)
            {
                //위치 조정
                Vector2 tempPosition = new Vector2(i, j + offSet);
                //생성
                GameObject backgroundTile = Instantiate(tilePrefab, tempPosition, Quaternion.identity) as GameObject;
                backgroundTile.transform.parent = this.transform;
                backgroundTile.name = string.Format(Define.Vec2Name, i, j);
                

                //구슬을 생성하고 배치한다.
                makeBeadSetup(i, j, tempPosition);
            }
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
            if(allBeads[_column - 1, _row].tag == _piece.tag && allBeads[_column - 2, _row].tag == _piece.tag)
            {
                return true;
            }
            if (allBeads[_column, _row - 1].tag == _piece.tag && allBeads[_column, _row - 2].tag == _piece.tag)
            {
                return true;
            }
        }
        else if(_column <= 1 || _row <= 1)
        {
            if (_column > 1)
            {
                if (allBeads[_column - 1, _row].tag == _piece.tag && allBeads[_column - 2, _row].tag == _piece.tag)
                {
                    return true;
                }
            }
            if (_row > 1)
            {
                if(allBeads[_column, _row - 1].tag == _piece.tag && allBeads[_column, _row - 2].tag == _piece.tag)
                {
                    return true;
                }
            }
        }
        return false;
    }

    //매치가 되었을때 해당 구슬을 제거한다.
    private void DestroyMatchesAt(int _column, int _row)
    {
        if(allBeads[_column, _row].GetComponent<Bead>().isMatched)
        {
            findMatches.currentMatches.Remove(allBeads[_column, _row]);
            Destroy(allBeads[_column, _row]);
            allBeads[_column, _row] = null;
        }
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
        StartCoroutine(DecreaseRowCo());
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
        yield return new WaitForSeconds(0.1f);
        StartCoroutine(FillBoardCo());
    }

    private void RefillBoard()
    {
        for(int i = 0; i < width; i++)
        {
            for(int j = 0; j < height; j++)
            {
                if(allBeads[i, j] == null)
                {
                    Vector2 tempPosition = new Vector2(i, j + offSet);
                    int beadToUse = Random.Range(0, beads.Length);
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
        yield return new WaitForSeconds(0.5f);
        while(MatchesOnBoard())
        {
            yield return new WaitForSeconds(0.5f);
            DestroyMatches();
        }
        yield return new WaitForSeconds(0.5f);
        currentState = GameState.move;
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
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                Vector2 tempPosition = new Vector2(i, j + offSet);
                makeBeadSetup(i, j, tempPosition);
            }
        }
    }
}
