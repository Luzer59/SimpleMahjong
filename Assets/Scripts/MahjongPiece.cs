using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MahjongPiece : MonoBehaviour
{
    private List<MahjongPiece> piecesAbove;
    private List<MahjongPiece> piecesBellow;

    private void Awake()
    {
        piecesAbove = new List<MahjongPiece>();
        piecesBellow = new List<MahjongPiece>();
    }
}
