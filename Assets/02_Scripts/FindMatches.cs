using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class FindMatches : MonoBehaviour
{
    private Board board;
    public List<GameObject> currentMatches = new List<GameObject>();
    //public Dictionary<string, List<GameObject>> Dic_currentMatches = new Dictionary<string, List<GameObject>>();

    // Start is called before the first frame update
    void Start()
    {
        board = FindObjectOfType<Board>();

        //for(int i = 0; i < board.beads.Length; i++)
        //{
        //    Dic_currentMatches.Add(board.beads[i].tag, new List<GameObject>());
        //}
    }

    public void FindAllMatches()
    {
        StartCoroutine(FindAllMatchesCo());
    }

    private List<GameObject> IsRowBomb(Bead _bead1, Bead _bead2, Bead _bead3)
    {
        List<GameObject> currentBeads = new List<GameObject>();
        if (_bead1.isRowBomb)
        {
            currentMatches.Union(GetRowPieces(_bead1.row));
        }
        if (_bead2.isRowBomb)
        {
            currentMatches.Union(GetRowPieces(_bead2.row));
        }
        if (_bead3.isRowBomb)
        {
            currentMatches.Union(GetRowPieces(_bead3.row));
        }
        return currentBeads;
    }

    private List<GameObject> IsColumnBomb(Bead _bead1, Bead _bead2, Bead _bead3)
    {
        List<GameObject> currentBeads = new List<GameObject>();
        if (_bead1.isColumnBomb)
        {
            currentMatches.Union(GetColumnPieces(_bead1.column));
        }
        if (_bead2.isColumnBomb)
        {
            currentMatches.Union(GetColumnPieces(_bead2.column));
        }
        if (_bead3.isColumnBomb)
        {
            currentMatches.Union(GetColumnPieces(_bead3.column));
        }
        return currentBeads;
    }

    private List<GameObject> IsAdjacentBomb(Bead _bead1, Bead _bead2, Bead _bead3)
    {
        List<GameObject> currentBeads = new List<GameObject>();
        if (_bead1.isAdjacentBomb)
        {
            currentMatches.Union(GetAdjacentPieces(_bead1.column, _bead1.row));
        }
        if (_bead2.isAdjacentBomb)
        {
            currentMatches.Union(GetAdjacentPieces(_bead2.column, _bead2.row));
        }
        if (_bead3.isAdjacentBomb)
        {
            currentMatches.Union(GetAdjacentPieces(_bead3.column, _bead3.row));
        }
        return currentBeads;
    }

    private void AddToListAndMatch(GameObject _bead)
    {
        if(_bead.tag != "NonNormalBead")
        {
            if (!currentMatches.Contains(_bead))
            {
                currentMatches.Add(_bead);
                //Dic_currentMatches[upBead.tag].Add(upBead);
            }
            GetBeadComponent(_bead).isMatched = true;
        }
    }

    private void GetNearbyPieces(GameObject _bead1, GameObject _bead2, GameObject _bead3)
    {
        AddToListAndMatch(_bead1);
        AddToListAndMatch(_bead2);
        AddToListAndMatch(_bead3);
    }

    private IEnumerator FindAllMatchesCo()
    {
        yield return new WaitForSeconds(0.2f);
        for(int i = 0; i < board.width; i++)
        {
            for(int j = 0; j < board.height; j++)
            {
                GameObject currentBead = board.allBeads[i, j];
                if(currentBead != null)
                {
                    if(i > 0 && i < board.width - 1)
                    {
                        GameObject leftBead = board.allBeads[i - 1, j];
                        GameObject rightBead = board.allBeads[i + 1, j];
                        if(leftBead != null && rightBead != null)
                        {
                            if(leftBead.tag == currentBead.tag && rightBead.tag == currentBead.tag)
                            {
                                currentMatches.Union(IsRowBomb(GetBeadComponent(leftBead), GetBeadComponent(currentBead), GetBeadComponent(rightBead)));

                                currentMatches.Union(IsColumnBomb(GetBeadComponent(leftBead), GetBeadComponent(currentBead), GetBeadComponent(rightBead)));

                                currentMatches.Union(IsAdjacentBomb(GetBeadComponent(leftBead), GetBeadComponent(currentBead), GetBeadComponent(rightBead)));

                                GetNearbyPieces(leftBead, currentBead, rightBead);
                            }
                        }
                    }

                    if (j > 0 && j < board.height - 1)
                    {
                        GameObject upBead = board.allBeads[i, j + 1];
                        GameObject downBead = board.allBeads[i, j - 1];
                        if (upBead != null && downBead != null)
                        {
                            if (upBead.tag == currentBead.tag && downBead.tag == currentBead.tag)
                            {
                                currentMatches.Union(IsColumnBomb(GetBeadComponent(upBead), GetBeadComponent(currentBead), GetBeadComponent(downBead)));

                                currentMatches.Union(IsRowBomb(GetBeadComponent(upBead), GetBeadComponent(currentBead), GetBeadComponent(downBead)));

                                currentMatches.Union(IsAdjacentBomb(GetBeadComponent(upBead), GetBeadComponent(currentBead), GetBeadComponent(downBead)));

                                GetNearbyPieces(upBead, currentBead, downBead);
                            }
                        }
                    }
                }
            }
        }
    }
    
    private Bead GetBeadComponent(GameObject _bead)
    {
        return _bead.GetComponent<Bead>();
    }

    public void MatchPiecesOfColor(string _tag)
    {
        for(int i = 0; i < board.width; i++)
        {
            for(int j = 0; j < board.height; j++)
            {
                //Check if that piece exists
                if(board.allBeads[i, j] != null)
                {
                    //Check the tag on the Bead
                    if(board.allBeads[i, j].tag == _tag)
                    {
                        //Set that bead to be matched
                        board.allBeads[i, j].GetComponent<Bead>().isMatched = true;
                    }
                }
            }
        }
    }

    List<GameObject> GetAdjacentPieces(int _column, int _row)
    {
        List<GameObject> beads = new List<GameObject>();
        for(int i = _column - 1; i <= _column + 1; i++)
        {
            for(int j = _row - 1; j <= _row + 1; j++)
            {
                if(i >= 0 && i < board.width && j >= 0 && j < board.height)
                {
                    beads.Add(board.allBeads[i, j]);
                    GetBeadComponent(board.allBeads[i, j]).isMatched = true;
                }
            }
        }
        return beads;
    }

    List<GameObject> GetColumnPieces(int _column)
    {
        List<GameObject> beads = new List<GameObject>();
        for(int i = 0; i < board.height; i++)
        {
            if(board.allBeads[_column, i] != null)
            {
                beads.Add(board.allBeads[_column, i]);
                GetBeadComponent(board.allBeads[_column, i]).isMatched = true;
            }
        }
        return beads;
    }

    List<GameObject> GetRowPieces(int _row)
    {
        List<GameObject> beads = new List<GameObject>();
        for (int i = 0; i < board.width; i++)
        {
            if (board.allBeads[i, _row] != null)
            {
                beads.Add(board.allBeads[i, _row]);
                GetBeadComponent(board.allBeads[i, _row]).isMatched = true;
            }
        }
        return beads;
    }


    public void CheckBombs()
    {
        //Did the player move something?
        if(board.currentBead != null)
        {
            //Is the piece they moved matched?
            if(board.currentBead.isMatched)
            {
                //make it unmatched
                board.currentBead.isMatched = false;

                //Decide what kind of bomb to make
                board.currentBead.MakeBomb(board.currentBead.direction == BeadMoveDirection.Right || board.currentBead.direction == BeadMoveDirection.Left ? BombType.Row : BombType.Column);

                //if(board.currentBead.direction == BeadMoveDirection.Right || board.currentBead.direction == BeadMoveDirection.Left)
                //{
                //    board.currentBead.MakeBomb(BombType.Row);
                //}
                //else if(board.currentBead.direction != BeadMoveDirection.None)
                //{
                //    board.currentBead.MakeBomb(BombType.Column);
                //}
            }
            //Is the other piece matched?
            else if(board.currentBead.otherBead != null)
            {
                Bead otherBead = board.currentBead.otherBead.GetComponent<Bead>();
                //Is the other Bead matched?
                if(otherBead.isMatched)
                {
                    //Make it unmatched
                    otherBead.isMatched = false;
                    //Decide what kind of bomb to make
                    otherBead.MakeBomb(otherBead.direction == BeadMoveDirection.Right || otherBead.direction == BeadMoveDirection.Left ? BombType.Row : BombType.Column);

                    //if (otherBead.direction == BeadMoveDirection.Right || otherBead.direction == BeadMoveDirection.Left)
                    //{
                    //    otherBead.MakeRowBomb();
                    //}
                    //else if (otherBead.direction != BeadMoveDirection.None)
                    //{
                    //    otherBead.MakeColumnBomb();
                    //}
                }
            }
        }
    }

}
