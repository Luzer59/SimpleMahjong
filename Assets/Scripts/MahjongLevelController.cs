using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MahjongLevelController : MonoBehaviour
{
    [SerializeField]
    private MahjongPiece[] piecePrefabs;

    private MahjongPieceMap map;

    private void Start()
    {
        // game loop init here (for now)
        GeneratePieceIDs();
        LoadLevelLayout();
        AssignPiecesToMap();
    }

    private void GeneratePieceIDs()
    {
        int id = 0;
        for (int i = 0; i < piecePrefabs.Length; i++)
        {
            piecePrefabs[i].id = id;
            id++;
        }
    }

    private void LoadLevelLayout()
    {
        // load json here
    }

    private void AssignPiecesToMap()
    {

    }
}
