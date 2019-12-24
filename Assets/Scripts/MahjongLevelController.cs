using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MahjongLevelController : MonoBehaviour
{
    private MahjongPieceMap map;

    private void Start()
    {
        // game loop init here (for now)
        LoadLevelLayout();
        AssignPiecesToMap();
    }

    private void LoadLevelLayout()
    {
        // load json here
    }

    private void AssignPiecesToMap()
    {

    }
}
