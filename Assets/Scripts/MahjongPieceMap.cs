using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MahjongPieceMap : MonoBehaviour
{
    [SerializeField]
    private int horizontalSize;
    [SerializeField]
    private int verticalSize;
    [SerializeField]
    private int heightSize;
    [SerializeField]
    private float horizontalSpacing;
    [SerializeField]
    private float verticalSpacing;
    [SerializeField]
    private float heightSpacing;
    [SerializeField]
    private int pieceSize = 1;

    private CellData[][,] map;

    private class CellData
    {
        public List<MahjongPiece> pieces = new List<MahjongPiece>();
    }

    private void Awake()
    {
        map = new CellData[heightSize][,];
        for (int i = 0; i < heightSize; i++)
        {
            map[i] = new CellData[horizontalSize, verticalSize];
        }
    }

    private void Update()
    {
        Vector3 point;
        for (int i = 0; i < horizontalSize; i++)
        {
            for (int p = 0; p < verticalSize; p++)
            {
                point = new Vector3(-(horizontalSize - 1) / 2f * horizontalSpacing + horizontalSpacing * i, 0f, (verticalSize - 1) / 2f * verticalSpacing - verticalSpacing * p);
                Debug.DrawLine(transform.position + point, transform.position + point + transform.up, Color.green);
            }
        }
    }

    private int[] WorldPointToGrid(Vector3 worldPoint)
    {
        Vector3 localPoint = transform.InverseTransformPoint(worldPoint);
        float horLimitPos = (horizontalSize - 1) / 2f * horizontalSpacing;
        float verLimitPos = (verticalSize - 1) / 2f * verticalSpacing;
        return new int[] {
            Mathf.RoundToInt(Mathf.InverseLerp(-horLimitPos, horLimitPos, localPoint.x) * (horizontalSize - 1)),
            Mathf.RoundToInt(Mathf.InverseLerp(verLimitPos, -verLimitPos, localPoint.z) * (verticalSize - 1)) };
    }

    private Vector3 GridToWorldPoint(int[] index)
    {
        return transform.TransformPoint(new Vector3(
            -((horizontalSize - 1) / 2f * horizontalSpacing) + horizontalSpacing * index[0], 
            0f, 
            (verticalSize - 1) / 2f * verticalSpacing - verticalSpacing * index[1]));
    }

    public bool CanPlace(Vector3 worldPoint)
    {
        int[] index = WorldPointToGrid(worldPoint);
        bool filled;
        for (int i = 0; i < heightSize; i++)
        {
            if (GetAnyOccupyingPieces(index, i, out filled))
            {
                if (filled)
                {
                    // Fully filled, may be possible on higher level
                    continue;
                }
                else
                {
                    // Only partially filled, no possible solutions on higher levels
                    return false;
                }
            }
            else
            {
                // No filled, valid position
                return true;
            }
        }
        // Height cap reached
        return false;
    }

    public bool TryPlace(Vector3 worldPoint, MahjongPiece piece)
    {
        if (CanPlace(worldPoint))
        {

        }
        return false;
    }

    public bool CanRemove(Vector3 worldPoint)
    {

    }

    public bool TryRemove(Vector3 worldPoint)
    {
        if (CanRemove(worldPoint))
        {

        }
        return false;
    }

    private HashSet<MahjongPiece> GetOccupyingPieces(int[] index, int height, out bool completelyFilled)
    {
        CellData cell;
        HashSet<MahjongPiece> foundPieces = new HashSet<MahjongPiece>();
        int cellsLeft = (1 + pieceSize * 2) * (1 + pieceSize * 2);
        for (int i = -pieceSize; i < 1 + pieceSize; i++)
        {
            for (int p = -pieceSize; p < 1 + pieceSize; p++)
            {
                cell = map[height][index[0] + i, index[1] + p];
                if (cell.pieces.Count > 0)
                {
                    cellsLeft--;
                    for (int u = 0; u < cell.pieces.Count; u++)
                    {
                        foundPieces.Add(cell.pieces[u]);
                    }
                }
            }
        }
        completelyFilled = cellsLeft == 0;
        return foundPieces;
    }

    private bool GetAnyOccupyingPieces(int[] index, int height, out bool completelyFilled)
    {
        CellData cell;
        bool found = false;
        int cellsLeft = (1 + pieceSize * 2) * (1 + pieceSize * 2);
        for (int i = -pieceSize; i < 1 + pieceSize; i++)
        {
            for (int p = -pieceSize; p < 1 + pieceSize; p++)
            {
                cell = map[height][index[0] + i, index[1] + p];
                if (cell.pieces.Count > 0)
                {
                    cellsLeft--;
                    found = true;
                }
            }
        }
        completelyFilled = cellsLeft == 0;
        return found;
    }
}
