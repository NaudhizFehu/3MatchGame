using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bead : MonoBehaviour
{
    [Header("Board Variables")]
    public int column;
    public int row;
    public int previousColumn;
    public int previousRow;
    public int targetX;
    public int targetY;
    public bool isMatched = false;

    private FindMatches findMatches;
    private Board board;
    private GameObject otherBead;
    private Vector2 firstTouchPosition;
    private Vector2 finalTouchPosition;
    private Vector2 tempPosition;
    public float swipeAngle = 0;
    public float swipeResist = 1f;
    

    // Start is called before the first frame update
    void Start()
    {
        board = FindObjectOfType<Board>();
        findMatches = FindObjectOfType<FindMatches>();
        //targetX = (int)transform.position.x;
        //targetY = (int)transform.position.y;
        //column = targetX;
        //row = targetY;
        //previousColumn = column;
        //previousRow = row;
    }

    // Update is called once per frame
    void Update()
    {
        //FindMatches();
        if(isMatched)
        {
            SpriteRenderer mySprite = GetComponent<SpriteRenderer>();
            mySprite.color = new Color(1f, 1f, 1f, 0.2f);
        }

        targetX = column;
        targetY = row;
        if(Mathf.Abs(targetX - transform.position.x) > 0.1f)
        {
            //Move Towards the target
            tempPosition = new Vector2(targetX, transform.position.y);
            transform.position = Vector2.Lerp(transform.position, tempPosition, 0.6f);
            if(board.allBeads[column, row] != this.gameObject)
            {
                board.allBeads[column, row] = this.gameObject;
            }
            findMatches.FindAllMatches();
        }
        else
        {
            //Directly set the position
            tempPosition = new Vector2(targetX, transform.position.y);
            transform.position = tempPosition;
        }

        if (Mathf.Abs(targetY - transform.position.y) > 0.1f)
        {
            //Move Towards the target
            tempPosition = new Vector2(transform.position.x, targetY);
            transform.position = Vector2.Lerp(transform.position, tempPosition, 0.6f);
            if (board.allBeads[column, row] != this.gameObject)
            {
                board.allBeads[column, row] = this.gameObject;
            }
            findMatches.FindAllMatches();
        }
        else
        {
            //Directly set the position
            tempPosition = new Vector2(transform.position.x, targetY);
            transform.position = tempPosition;
        }
    }

    public IEnumerator CheckMoveCo()
    {
        yield return new WaitForSeconds(0.5f);
        if(otherBead != null)
        {
            //매칭이 되지않았다면 이전 위치로 되돌린다.
            if (!isMatched && !otherBead.GetComponent<Bead>().isMatched)
            {
                otherBead.GetComponent<Bead>().column = column;
                otherBead.GetComponent<Bead>().row = row;
                column = previousColumn;
                row = previousRow;
                yield return new WaitForSeconds(0.5f);
                board.currentState = GameState.move;
            }
            //매칭이 되었다면 제거한다.
            else
            {
                board.DestroyMatches();
            }
            otherBead = null;
        }
       
    }

    private void OnMouseDown()
    {
        if(board.currentState == GameState.move)
        {
            firstTouchPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        }
    }

    private void OnMouseUp()
    {
        if(board.currentState == GameState.move)
        {
            finalTouchPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            CalculateAngle();
        }
    }

    private void CalculateAngle()
    {
        if(Mathf.Abs(finalTouchPosition.y - firstTouchPosition.y) > swipeResist ||
            Mathf.Abs(finalTouchPosition.x - firstTouchPosition.x) > swipeResist)
        {
            swipeAngle = Mathf.Atan2(finalTouchPosition.y - firstTouchPosition.y, finalTouchPosition.x - firstTouchPosition.x) * 180 / Mathf.PI;
            MovePieces();
            board.currentState = GameState.wait;
        }
        else
        {
            board.currentState = GameState.move;
        }
    }

    private void MovePieces()
    {
        if(swipeAngle > -45 && swipeAngle <= 45 && column < board.width - 1)
        {
            //Right Swipe
            otherBead = board.allBeads[column + 1, row];
            previousColumn = column;
            previousRow = row;
            otherBead.GetComponent<Bead>().column -= 1;
            column += 1;
        }
        else if (swipeAngle > 45 && swipeAngle <= 135 && row < board.height - 1)
        {
            //Up Swipe
            otherBead = board.allBeads[column, row + 1];
            previousColumn = column;
            previousRow = row;
            otherBead.GetComponent<Bead>().row -= 1;
            row += 1;
        }
        else if ((swipeAngle > 135 || swipeAngle <= -135) && column > 0)
        {
            //Left Swipe
            otherBead = board.allBeads[column - 1, row];
            previousColumn = column;
            previousRow = row;
            otherBead.GetComponent<Bead>().column += 1;
            column -= 1;
        }
        else if (swipeAngle < -45 && swipeAngle >= -135 && row > 0)
        {
            //Down Swipe
            otherBead = board.allBeads[column, row - 1];
            previousColumn = column;
            previousRow = row;
            otherBead.GetComponent<Bead>().row += 1;
            row -= 1;
        }
        StartCoroutine(CheckMoveCo());
    }

    private void FindMatches()
    {
        //가로
        if(column > 0 && column < board.width - 1)
        {
            GameObject leftBead1 = board.allBeads[column - 1, row];
            GameObject rightBead1 = board.allBeads[column + 1, row];
            if(leftBead1 != null && rightBead1 != null)
            {
                if(leftBead1.tag == this.gameObject.tag && rightBead1.tag == this.gameObject.tag)
                {
                    leftBead1.GetComponent<Bead>().isMatched = true;
                    rightBead1.GetComponent<Bead>().isMatched = true;
                    isMatched = true;
                }
            }
        }
        //세로
        if (row > 0 && row < board.height - 1)
        {
            GameObject upBead1 = board.allBeads[column, row + 1];
            GameObject downBead1 = board.allBeads[column, row - 1];
            if(upBead1 != null && downBead1 != null)
            {
                if (upBead1.tag == this.gameObject.tag && downBead1.tag == this.gameObject.tag)
                {
                    upBead1.GetComponent<Bead>().isMatched = true;
                    downBead1.GetComponent<Bead>().isMatched = true;
                    isMatched = true;
                }
            }
        }
    }
}
