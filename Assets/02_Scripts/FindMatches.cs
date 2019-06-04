using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class FindMatches : MonoBehaviour
{
    private Board board;
    public List<GameObject> currentMatches = new List<GameObject>();

    // Start is called before the first frame update
    void Start()
    {
        board = FindObjectOfType<Board>();
    }

    public void FindAllMatches()
    {
        StartCoroutine(FindAllMatchesCo());
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
                                LineBombProcessing(leftBead, currentBead, rightBead, i, j);

                                if (!currentMatches.Contains(leftBead))
                                {
                                    currentMatches.Add(leftBead);
                                }
                                BeadComponentMatched(leftBead, true);
                                if (!currentMatches.Contains(rightBead))
                                {
                                    currentMatches.Add(rightBead);
                                }
                                BeadComponentMatched(rightBead, true);
                                if (!currentMatches.Contains(currentBead))
                                {
                                    currentMatches.Add(currentBead);
                                }
                                BeadComponentMatched(currentBead, true);
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
                                LineBombProcessing(upBead, currentBead, downBead, i, j);

                                if (!currentMatches.Contains(upBead))
                                {
                                    currentMatches.Add(upBead);
                                }
                                BeadComponentMatched(upBead, true);
                                if (!currentMatches.Contains(downBead))
                                {
                                    currentMatches.Add(downBead);
                                }
                                BeadComponentMatched(downBead, true);
                                if (!currentMatches.Contains(currentBead))
                                {
                                    currentMatches.Add(currentBead);
                                }
                                BeadComponentMatched(currentBead, true);
                            }
                        }
                    }
                }
            }
        }
    }

    List<GameObject> GetColumnPieces(int _column)
    {
        List<GameObject> beads = new List<GameObject>();
        for(int i = 0; i < board.height; i++)
        {
            if(board.allBeads[_column, i] != null)
            {
                beads.Add(board.allBeads[_column, i]);
                BeadComponentMatched(board.allBeads[_column, i], true);
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
                BeadComponentMatched(board.allBeads[i, _row], true);
            }
        }
        return beads;
    }

    private void LineBombProcessing(GameObject _bead1, GameObject _bead2, GameObject _bead3, int _i, int _j)
    {
        if (_bead1.GetComponent<Bead>().isColumnBomb)
        {
            //left
            currentMatches.Union(GetColumnPieces(_i - 1));
        }

        if (_bead2.GetComponent<Bead>().isColumnBomb)
        {
            //current
            currentMatches.Union(GetColumnPieces(_i));
        }

        if (_bead3.GetComponent<Bead>().isColumnBomb)
        {
            //right
            currentMatches.Union(GetColumnPieces(_i + 1));
        }

        if (_bead1.GetComponent<Bead>().isRowBomb)
        { 
            //up
            currentMatches.Union(GetRowPieces(_j + 1));
        }

        if (_bead2.GetComponent<Bead>().isRowBomb)
        {
            //current
            currentMatches.Union(GetRowPieces(_j));
        }

        if (_bead3.GetComponent<Bead>().isRowBomb)
        {
            //down
            currentMatches.Union(GetRowPieces(_j - 1));
        }
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
                int typeOfBomb = Random.Range(0, 100);
                if(typeOfBomb < 50)
                {
                    //Make a row bomb
                    board.currentBead.MakeRowBomb();
                }
                else
                {
                    //Make a column bomb
                    board.currentBead.MakeColumnBomb();
                }
            }
            //Is the other piece matched?
            else if(board.currentBead.otherBead != null)
            {

            }
        }
    }

    private void BeadComponentMatched(GameObject _obj, bool _matched)
    {
        _obj.GetComponent<Bead>().isMatched = _matched;
    }
}
