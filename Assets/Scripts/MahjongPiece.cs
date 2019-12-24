﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class MahjongPiece : MonoBehaviour
{
    public int id;

    [SerializeField]
    private GameObject defaultTypePrefab;
    [SerializeField]
    private GameObject[] typePrefabs;

    [SerializeField]
    private GameObject activeTypeObject;

    public HashSet<MahjongPiece> piecesAbove;
    public HashSet<MahjongPiece> piecesBellow;
    public HashSet<MahjongPiece> piecesLeft;
    public HashSet<MahjongPiece> piecesRight;

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

    public bool IsAboveBlocked()
    {
        return piecesAbove.Count > 0;
    }

    public Vector3 GetWorldPoint()
    {
        return transform.position;
    }

    public bool IsSelectable()
    {
        return !(AreSidesBlocked() || IsAboveBlocked());
    }

    public void Show()
    {
        if (activeTypeObject)
            activeTypeObject.SetActive(true);
    }

    public void Hide()
    {
        if (activeTypeObject)
            activeTypeObject.SetActive(false);
    }

    public void Place(Vector3 worldPoint, HashSet<MahjongPiece> above, HashSet<MahjongPiece> bellow, HashSet<MahjongPiece> left, HashSet<MahjongPiece> right)
    {
        transform.position = worldPoint;
        SetType(-1);

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

        Destroy(gameObject);
    }

    public void SetType(int type)
    {
        id = type;

        if (activeTypeObject)
        {
            Destroy(activeTypeObject);
        }

        if (type == -1)
        {
            activeTypeObject = Instantiate(defaultTypePrefab, transform, false);
        }
        else
        {
            activeTypeObject = Instantiate(typePrefabs[type], transform, false);
        }
    }
}
