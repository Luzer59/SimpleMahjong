using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class MahjongMapData
{
    public MahjongMapData()
    {

    }

    public MahjongMapData(int pieceCount, int pieceSize)
    {
        this.pieceCount = pieceCount;
        this.pieceSize = pieceSize;
        pieceHorizontalIndex = new int[pieceCount];
        pieceVerticalIndex = new int[pieceCount];
        pieceHeightIndex = new int[pieceCount];
    }

    public int pieceCount;
    public int pieceSize;
    public int[] pieceHorizontalIndex;
    public int[] pieceVerticalIndex;
    public int[] pieceHeightIndex;
}
