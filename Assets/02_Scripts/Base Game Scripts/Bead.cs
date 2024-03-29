﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BeadMoveDirection
{
    None = 0,
    Right,
    Up,
    Left,
    Down,
}

public enum BombType
{
    Row = 0,
    Column,
    Color,
    Adjacent,
}

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
    public GameObject otherBead;
    private Vector2 firstTouchPosition = Vector2.zero;
    private Vector2 finalTouchPosition = Vector2.zero;
    private Vector2 tempPosition;

    [Header("Swipe Stuff")]
    public float swipeAngle = 0;
    public float swipeResist = 1f;
    public BeadMoveDirection direction;

    [Header("Powerup Stuff")]
    public bool isColorBomb;
    public bool isColumnBomb;
    public bool isRowBomb;
    public bool isAdjacentBomb;
    public GameObject rowArrow;
    public GameObject columnArrow;
    private GameObject arrow;
    public GameObject colorBomb;
    public GameObject adjacentBomb;

    private GameObject bomb;

    //test
    public int nArrow = 0;

    // Start is called before the first frame update
    void Start()
    {
        isColumnBomb = false;
        isRowBomb = false;
        isColorBomb = false;
        isAdjacentBomb = false;

        direction = BeadMoveDirection.None;
        //FindObjectOfType보다 FindWithTag로 찾는것이 더 빠르다.
        board = GameObject.FindWithTag("Board").GetComponent<Board>();
        //board = FindObjectOfType<Board>();
        findMatches = FindObjectOfType<FindMatches>();
    }

    //This is for testing and Debug only.
    private void OnMouseOver()
    {
        if(Input.GetMouseButtonDown(1))
        {
            nArrow++;
            if (nArrow > 4) nArrow = 0;
            //nArrow = nArrow > 2 ? 0 : nArrow++;

            GameObject originalObj = null;
            if(arrow != null)
            {
                Destroy(arrow);
                arrow = null;
            }
            switch (nArrow)
            {
                case 0:
                    isRowBomb = false;
                    isColumnBomb = false;
                    isColorBomb = false;
                    isAdjacentBomb = false;
                    originalObj = null;
                    break;

                case 1:
                    isRowBomb = true;
                    isColumnBomb = false;
                    isColorBomb = false;
                    isAdjacentBomb = false;
                    originalObj = rowArrow;
                    break;

                case 2:
                    isRowBomb = false;
                    isColumnBomb = true;
                    isColorBomb = false;
                    isAdjacentBomb = false;
                    originalObj = columnArrow;
                    break;

                case 3:
                    isRowBomb = false;
                    isColumnBomb = false;
                    isColorBomb = true;
                    isAdjacentBomb = false;
                    originalObj = colorBomb;
                    break;

                case 4:
                    isRowBomb = false;
                    isColumnBomb = false;
                    isColorBomb = false;
                    isAdjacentBomb = true;
                    originalObj = adjacentBomb;
                    break;
            }

            if(nArrow != 0)
            {
                arrow = Instantiate(originalObj, transform.position, Quaternion.identity);
                arrow.transform.parent = this.transform;
                arrow.transform.localPosition = Vector3.zero;
            }
            
        }
    }

    // Update is called once per frame
    void Update()
    {
        //if(isMatched)
        //{
        //    SpriteRenderer mySprite = GetComponent<SpriteRenderer>();
        //    mySprite.color = new Color(1f, 1f, 1f, 0.2f);
        //}

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
                findMatches.FindAllMatches();
            }
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
                findMatches.FindAllMatches();
            }
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
        //섞은 구슬과 같은 색의 구슬을 모두 제거함
        if(isColorBomb)
        {
            //This piece is a color bomb, and the other piece is the color to destroy
            findMatches.MatchPiecesOfColor(otherBead.tag);
            isMatched = true;
        }
        else if(otherBead.GetComponent<Bead>().isColorBomb)
        {
            //The other piece is a color bomb, and this piece has the color to destroy
            findMatches.MatchPiecesOfColor(this.gameObject.tag);
            otherBead.GetComponent<Bead>().isMatched = true;
        }
        yield return new WaitForSeconds(0.3f);
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
                board.currentBead = null;
                board.currentState = GameState.move;
            }
            //매칭이 되었다면 제거한다.
            else
            {
                if(EndGameManager.instance.requirements.gameType == GameType.Moves)
                {
                    EndGameManager.instance.DecreaseCounterValue();
                }
                board.DestroyMatches();
            }
            //otherBead = null;
        }
       
    }

    private void OnMouseDown()
    {
        //Destory the hint
        HintManager.instance.DestroyHint();



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
            board.currentState = GameState.wait;//외부로 드래그할 시 생기는 버그 Fix
            swipeAngle = Mathf.Atan2(finalTouchPosition.y - firstTouchPosition.y, finalTouchPosition.x - firstTouchPosition.x) * 180 / Mathf.PI;
            MovePieces();
            board.currentBead = this;
        }
        else
        {
            board.currentState = GameState.move;
        }
    }

    //MovePieces함수 리팩토링
    private void MovePiecesActual(Vector2 _direction)
    {
        otherBead = board.allBeads[column + (int)_direction.x, row + (int)_direction.y];
        previousColumn = column;
        previousRow = row;
        if(board.lockTiles[column, row] == null && board.lockTiles[column + (int)_direction.x, row + (int)_direction.y] == null)
        {
            if(otherBead != null)
            {
                otherBead.GetComponent<Bead>().column += -1 * (int)_direction.x;
                otherBead.GetComponent<Bead>().row += -1 * (int)_direction.y;
                column += (int)_direction.x;
                row += (int)_direction.y;
                StartCoroutine(CheckMoveCo());
            }
            else
            {
                board.currentState = GameState.move;
            }
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
            MovePiecesActual(Vector2.right);
        }
        else if (swipeAngle > 45 && swipeAngle <= 135 && row < board.height - 1)
        {
            //Up Swipe
            MovePiecesActual(Vector2.up);
        }
        else if ((swipeAngle > 135 || swipeAngle <= -135) && column > 0)
        {
            //Left Swipe
            MovePiecesActual(Vector2.left);
        }
        else if (swipeAngle < -45 && swipeAngle >= -135 && row > 0)
        {
            //Down Swipe
            MovePiecesActual(Vector2.down);
        }
        //else
        //{
            board.currentState = GameState.move;
        //}
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

    public void MakeBomb(BombType _type)
    {
        GameObject createBombObj = null;
        //폭탄 문양이 겹치지 않도록 초기화
        if(bomb != null)
        {
            Destroy(bomb);
            bomb = null;
            isRowBomb = false;
            isColumnBomb = false;
            isColorBomb = false;
            isAdjacentBomb = false;
        }
        switch(_type)
        {
            case BombType.Row:
                if(!isColumnBomb && !isColorBomb && !isAdjacentBomb)
                {
                    isRowBomb = true;
                    createBombObj = rowArrow;
                }
                break;

            case BombType.Column:
                if (!isRowBomb && !isColorBomb && !isAdjacentBomb)
                {
                    isColumnBomb = true;
                    createBombObj = columnArrow;
                }
                break;

            case BombType.Color:
                if (!isColumnBomb && !isRowBomb && !isAdjacentBomb)
                {
                    isColorBomb = true;
                    createBombObj = colorBomb;
                    this.tag = "Color";
                }
                break;

            case BombType.Adjacent:
                if (!isColumnBomb && !isColorBomb && !isRowBomb)
                {
                    isAdjacentBomb = true;
                    createBombObj = adjacentBomb;
                }
                break;
        }
        bomb = Instantiate(createBombObj, transform.position, Quaternion.identity);
        bomb.transform.parent = this.transform;
    }
}
