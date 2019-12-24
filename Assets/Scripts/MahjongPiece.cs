using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class MahjongPiece : MonoBehaviour
{
    public int id;

    [SerializeField]
    private GameObject visuals;

    private HashSet<MahjongPiece> piecesAbove;
    private HashSet<MahjongPiece> piecesBellow;
    private HashSet<MahjongPiece> piecesLeft;
    private HashSet<MahjongPiece> piecesRight;

    private void Awake()
    {
        piecesAbove = new HashSet<MahjongPiece>();
        piecesBellow = new HashSet<MahjongPiece>();
        piecesLeft = new HashSet<MahjongPiece>();
        piecesRight = new HashSet<MahjongPiece>();
    }

    public void AddPieceAbove(MahjongPiece piece)
    {
        piecesAbove.Add(piece);
    }

    public void RemovePieceAbove(MahjongPiece piece)
    {
        piecesAbove.Remove(piece);
    }

    public void AddPieceBellow(MahjongPiece piece)
    {
        piecesAbove.Add(piece);
    }

    public void RemovePieceBellow(MahjongPiece piece)
    {
        piecesAbove.Remove(piece);
    }

    public void AddPieceLeft(MahjongPiece piece)
    {
        piecesAbove.Add(piece);
    }

    public void RemovePieceLeft(MahjongPiece piece)
    {
        piecesAbove.Remove(piece);
    }

    public void AddPieceRight(MahjongPiece piece)
    {
        piecesAbove.Add(piece);
    }

    public void RemovePieceRight(MahjongPiece piece)
    {
        piecesAbove.Remove(piece);
    }

    public bool AreSidesBlocked()
    {
        return piecesLeft.Count > 0 && piecesRight.Count > 0;
    }

    public bool IsAboveBlocking()
    {
        return piecesAbove.Count > 0;
    }

    public Vector3 GetWorldPoint()
    {
        return transform.position;
    }

    public void Show()
    {
        visuals.SetActive(true);
    }

    public void Hide()
    {
        visuals.SetActive(false);
    }

    public void Place(Vector3 worldPoint, HashSet<MahjongPiece> above, HashSet<MahjongPiece> bellow, HashSet<MahjongPiece> left, HashSet<MahjongPiece> right)
    {
        transform.position = worldPoint;
        Show();

        MahjongPiece[] inputArray = above.ToArray();
        for (int i = 0; i < inputArray.Length; i++)
        {
            AddPieceAbove(inputArray[i]);
            inputArray[i].AddPieceBellow(this);
        }
        inputArray = bellow.ToArray();
        for (int i = 0; i < inputArray.Length; i++)
        {
            AddPieceBellow(inputArray[i]);
            inputArray[i].AddPieceAbove(this);
        }
        inputArray = left.ToArray();
        for (int i = 0; i < inputArray.Length; i++)
        {
            AddPieceLeft(inputArray[i]);
            inputArray[i].AddPieceRight(this);
        }
        inputArray = right.ToArray();
        for (int i = 0; i < inputArray.Length; i++)
        {
            AddPieceRight(inputArray[i]);
            inputArray[i].AddPieceLeft(this);
        }
    }

    public void Remove()
    {
        Hide();

        MahjongPiece[] inputArray = piecesAbove.ToArray();
        for (int i = 0; i < inputArray.Length; i++)
        {
            RemovePieceAbove(inputArray[i]);
            inputArray[i].RemovePieceAbove(this);
        }
        inputArray = piecesBellow.ToArray();
        for (int i = 0; i < inputArray.Length; i++)
        {
            RemovePieceBellow(inputArray[i]);
            inputArray[i].RemovePieceAbove(this);
        }
        inputArray = piecesLeft.ToArray();
        for (int i = 0; i < inputArray.Length; i++)
        {
            RemovePieceLeft(inputArray[i]);
            inputArray[i].RemovePieceLeft(this);
        }
        inputArray = piecesRight.ToArray();
        for (int i = 0; i < inputArray.Length; i++)
        {
            RemovePieceRight(inputArray[i]);
            inputArray[i].RemovePieceRight(this);
        }
    }
}
