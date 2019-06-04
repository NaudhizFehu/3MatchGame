using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResetBtn : MonoBehaviour
{
    private Board board;

    // Start is called before the first frame update
    void Start()
    {
        board = FindObjectOfType<Board>();
    }

    private void OnMouseUpAsButton()
    {
        board.Resetbead();
    }
}
