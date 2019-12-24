using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class MahjongMapData
{
    public MahjongMapData()
    {

    }

    public MahjongMapData(int count)
    {
        pieceCount = count;
        pieceHorizontalIndex = new int[count];
        pieceVerticalIndex = new int[count];
        pieceHeightIndex = new int[count];
    }

    public int pieceCount;
    public int[] pieceHorizontalIndex;
    public int[] pieceVerticalIndex;
    public int[] pieceHeightIndex;
}
