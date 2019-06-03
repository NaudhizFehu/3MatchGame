using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
                                if(!currentMatches.Contains(leftBead))
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

    private void BeadComponentMatched(GameObject _obj, bool _matched)
    {
        _obj.GetComponent<Bead>().isMatched = _matched;
    }
}
