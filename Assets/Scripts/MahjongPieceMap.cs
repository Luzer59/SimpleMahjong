using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

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
    private int pieceSize = 2; // TODO: Use to scale spacing

    [SerializeField]
    private MahjongPiece piecePrefab;

    private Cell[,,] map; // horizontal, vertical, height
    private Dictionary<int, Vector3Int> pieceLeadIndexes;
    private Dictionary<int, Vector3Int> availablePieceLeadIndexes;

    private class Cell
    {
        public Cell()
        {
            isLeftBlocked = false;
            isRightBlocked = false;
            isTopBlocked = false;
        }

        public bool isLeftBlocked;
        public bool isRightBlocked;
        public bool isTopBlocked;
        public Vector3Int leadIndex;
        public MahjongPiece piece;
    }

    private void Awake()
    {
        pieceLeadIndexes = new Dictionary<int, Vector3Int>();
        availablePieceLeadIndexes = new Dictionary<int, Vector3Int>();
        map = new Cell[horizontalSize, verticalSize, heightSize];
        for (int i = 0; i < horizontalSize; i++)
        {
            for (int p = 0; p < verticalSize; p++)
            {
                for (int u = 0; u < heightSize; u++)
                {
                    map[i, p, u] = new Cell();
                }
            }
        }
    }

    public Vector3Int WorldPointToGrid(Vector3 worldPoint, bool highest)
    {
        Vector3 localPoint = transform.InverseTransformPoint(worldPoint);
        float horLimitPos = (horizontalSize - 1) / 2f * horizontalSpacing;
        float verLimitPos = (verticalSize - 1) / 2f * verticalSpacing;
        Vector3Int index = new Vector3Int(
            Mathf.RoundToInt(Mathf.InverseLerp(-horLimitPos, horLimitPos, localPoint.x) * (horizontalSize - 1)),
            Mathf.RoundToInt(Mathf.InverseLerp(verLimitPos, -verLimitPos, localPoint.z) * (verticalSize - 1)),
            0);
        if (highest)
        {
            for (int i = 0 ; i < heightSize; i++)
            {
                if (!map[index.x, index.y, i].piece)
                {
                    index.z = i;
                    break;
                }
            }
        }
        return index;
    }

    public Vector3 GridToWorldPoint(Vector3Int index)
    {
        return transform.TransformPoint(new Vector3(
            -((horizontalSize - 1) / 2f * horizontalSpacing) + horizontalSpacing * index.x, 
            index.z * heightSpacing, 
            (verticalSize - 1) / 2f * verticalSpacing - verticalSpacing * index.y));
    }

    public Vector3Int PieceToIndex(MahjongPiece piece)
    {
        pieceLeadIndexes.TryGetValue(piece.GetId(), out Vector3Int index);
        return index;
    }

    public MahjongPiece IndexToPiece(Vector3Int index)
    {
        return map[index.x, index.y, index.z].piece;
    }

    public void LoadMap(MahjongMapData data)
    {
        if (data.pieceCount % 2 != 0)
        {
            Debug.LogError("Non-even map piece size. Cancelling...");
            return;
        }

        UnloadMap();

        for (int i = 0; i < data.pieceCount; i++)
        {
            TryPlace(new Vector3Int(data.pieceHorizontalIndex[i], data.pieceVerticalIndex[i], data.pieceHeightIndex[i]));
        }
    }

    public void InitPieces(int maxPieceTypes)
    {
        /*//List<RandomMahjongRow> rows = new List<RandomMahjongRow>();
        int lastHeightStartIndex = 0;
        int lastHeightEndIndex = 0;
        bool rowActive = false;
        int totalPieces = 0;
        int tryCount = 10;

        // Find rows
        for (int u = 0; u < heightSize; u++)
        {
            for (int p = 0; p < verticalSize; p++)
            {
                for (int i = 0; i < horizontalSize; i++)
                {
                    if (!map[i, p, u].center)
                    {
                        rowActive = false;
                    }
                    else if (!rowActive)
                    {
                        rows.Add(new RandomMahjongRow(rows.GetRange(lastHeightStartIndex, lastHeightEndIndex - lastHeightStartIndex)));
                        rowActive = true;
                    }
                    if (rowActive)
                    {
                        rows[rows.Count - 1].Add(map[u][i, p].center);
                        totalPieces++;
                        i += Mathf.Max(0, 1 + (pieceSize - 1) * 2);
                    }
                }
            }
            lastHeightStartIndex = lastHeightEndIndex + 1;
            lastHeightEndIndex = rows.Count - 1;
        }

        Debug.Log("Found " + rows.Count + " rows with total " + totalPieces + " pieces");

        int currentPieceType = 0;
        List<List<MahjongPiece>> valid = new List<List<MahjongPiece>>();
        int rnd;
        bool flag; 
        int counter;

        for (int j = 0; j < tryCount; j++)
        {
            int lastRow = -1;
            flag = false;

            for (int i = 0; i < totalPieces; i++)
            {
                flag = false;
                valid.Clear();
                counter = 0;

                for (int p = 0; p < rows.Count; p++)
                {
                    valid.Add(rows[p].GetValid(false));
                }

                string debug = "";

                for (int p = 0; p < valid.Count; p++)
                {
                    counter += valid[p].Count;
                    debug += "Row " + p + ": " + valid[p].Count + ", ";
                }

                if (counter == 0)
                {
                    if (j < tryCount - 1)
                    {
                        Debug.LogWarning("No valid initialization positions. Retrying " + (tryCount - 1 - j) + " times");
                    }
                    else
                    {
                        Debug.LogError("No valid initialization positions. Map invalid.");
                    }
                    break;
                }

                rnd = Random.Range(0, counter - 1);
                counter = 0;

                for (int p = 0; p < valid.Count; p++)
                {
                    if (flag)
                        break;

                    for (int u = 0; u < valid[p].Count; u++)
                    {
                        if (rnd == counter)
                        {
                            //Debug.Log("Row: " + p + ", Piece: " + u + ", Type: " + currentPieceType);
                            valid[p][u].SetVariation(currentPieceType);
                            rows[p].Take(valid[p][u]);
                            lastRow = p;
                            flag = true;
                            break;
                        }
                        counter++;
                    }
                }

                if (i % 2 == 1)
                {
                    currentPieceType++;
                    if (currentPieceType > maxPieceTypes - 1)
                    {
                        currentPieceType = 0;
                    }
                }

                if (i == totalPieces - 1)
                {
                    flag = true;
                }
            }

            if (flag)
                break;
        }*/
    }

    public void UnloadMap()
    {
        for (int i = 0; i < horizontalSize; i++)
        {
            for (int p = 0; p < verticalSize; p++)
            {
                for (int u = 0; u < heightSize; u++)
                {
                    if (map[i, p, u].piece)
                    {
                        map[i, p, u].piece.Remove();
                    }
                }
            }
        }
    }

    public MahjongMapData ConvertLoadedMap()
    {
        List<Vector3Int> pieces = new List<Vector3Int>();
        for (int i = 0; i < horizontalSize; i++)
        {
            for (int p = 0; p < verticalSize; p++)
            {
                for (int u = 0; u < heightSize; u++)
                {
                    if (map[i, p, u].piece)
                    {
                        pieces.Add(new Vector3Int(i, p, u));
                    }
                }
            }
        }
        MahjongMapData data = new MahjongMapData(pieces.Count, pieceSize);
        for (int i = 0; i < data.pieceCount; i++)
        {
            data.pieceHorizontalIndex[i] = pieces[i].x;
            data.pieceVerticalIndex[i] = pieces[i].y;
            data.pieceHeightIndex[i] = pieces[i].z;
        }
        return data;
    }

    public bool TryPlace(Vector3Int leadIndex)
    {
        if (!IsOccupied(leadIndex, pieceSize))
        {
            IsOccupied(leadIndex - new Vector3Int(0, 0, 1), pieceSize, out bool filled);

            if (filled)
            {
                MahjongPiece newPiece = Instantiate(piecePrefab);
                newPiece.Place(GridToWorldPoint(leadIndex));
                pieceLeadIndexes.Add(newPiece.GetId(), leadIndex);
                SetCellState(leadIndex, newPiece);
                return true;
            }
        }
        return false;
    }

    public bool TryRemove(Vector3Int index, bool sideBlocking)
    {
        if (!map[index.x, index.y, index.z].piece)
            return false;

        /*if ((sideBlocking && AreSidesBlocked(map[index.x, index.y, index.z].leadIndex)) || IsOccupied(map[index.x, index.y, index.z].leadIndex + new Vector3Int(0, 0, 1), pieceSize))
            return false;*/
        if (!IsPieceAvailable(index, sideBlocking))
            return false;

        pieceLeadIndexes.Remove(map[index.x, index.y, index.z].piece.GetId());
        map[index.x, index.y, index.z].piece.Remove();
        SetCellState(map[index.x, index.y, index.z].leadIndex, null);
        return true;
    }

    public bool IsPieceAvailable(Vector3Int index, bool sideBlocking)
    {
        if (!map[index.x, index.y, index.z].piece)
            return false;

        index = map[index.x, index.y, index.z].leadIndex;
        bool leftBlocked = false;
        bool rightBlocked = false;

        for (int i = 0; i < pieceSize; i++)
        {
            for (int p = 0; p < pieceSize; p++)
            {
                if (map[index.x + i, index.y + p, index.z].isTopBlocked)
                {
                    return false;
                }

                if (i == 0 && !leftBlocked)
                {
                    if (map[index.x + i, index.y + p, index.z].isLeftBlocked)
                    {
                        leftBlocked = true;
                        if (leftBlocked && rightBlocked)
                        {
                            return false;
                        }
                    }
                }
                else if (i == pieceSize - 1 && !rightBlocked)
                {
                    if (map[index.x + i, index.y + p, index.z].isRightBlocked)
                    {
                        rightBlocked = true;
                        if (leftBlocked && rightBlocked)
                        {
                            return false;
                        }
                    }
                }
            }
        }

        return true;
    }

    private void CalcAvailablePieces()
    {
        CalcAvailablePieces(pieceLeadIndexes);
    }

    private void CalcAvailablePieces(Dictionary<int, Vector3Int> uniqueIndexes)
    {
        availablePieceLeadIndexes.Clear();
        List<Vector3Int> indexes = uniqueIndexes.Values.ToList();
        for (int i = 0; i < indexes.Count; i++)
        {
            if (IsPieceAvailable(indexes[i], true))
            {
                availablePieceLeadIndexes.Add(map[indexes[i].x, indexes[i].y, indexes[i].z].piece.GetId(), indexes[i]);
            }
        }
    }

    private void SetCellState(Vector3Int leadIndex, MahjongPiece piece)
    {
        Dictionary<int, Vector3Int> afflictedPieces = new Dictionary<int, Vector3Int>();
        afflictedPieces.Add(piece.GetId(), leadIndex);

        for (int i = 0; i < pieceSize; i++)
        {
            for (int p = 0; p < pieceSize; p++)
            {
                map[leadIndex.x + i, leadIndex.y + p, leadIndex.z].piece = piece;
                if (piece)
                {
                    map[leadIndex.x + i, leadIndex.y + p, leadIndex.z].leadIndex = leadIndex;
                    if (leadIndex.z > 0)
                    {
                        map[leadIndex.x + i, leadIndex.y + p, leadIndex.z - 1].isTopBlocked = true;
                        afflictedPieces.Add(map[leadIndex.x + i, leadIndex.y + p, leadIndex.z - 1].piece.GetId(), new Vector3Int(leadIndex.x + i, leadIndex.y + p, leadIndex.z - 1));
                    }

                    if (i == 0)
                    {
                        if (!IsOutOfMap(new Vector3Int(leadIndex.x + i - 1, leadIndex.y + p, leadIndex.z)) && map[leadIndex.x + i - 1, leadIndex.y + p, leadIndex.z].piece)
                        {
                            map[leadIndex.x + i, leadIndex.y + p, leadIndex.z].isLeftBlocked = true;
                            map[leadIndex.x + i - 1, leadIndex.y + p, leadIndex.z].isRightBlocked = true;
                            afflictedPieces.Add(map[leadIndex.x + i - 1, leadIndex.y + p, leadIndex.z - 1].piece.GetId(), new Vector3Int(leadIndex.x + i - 1, leadIndex.y + p, leadIndex.z));
                        }
                    }
                    else if (i == pieceSize - 1)
                    {
                        if (!IsOutOfMap(new Vector3Int(leadIndex.x + i + 1, leadIndex.y + p, leadIndex.z)) && map[leadIndex.x + i + 1, leadIndex.y + p, leadIndex.z].piece)
                        {
                            map[leadIndex.x + i, leadIndex.y + p, leadIndex.z].isRightBlocked = true;
                            map[leadIndex.x + i + 1, leadIndex.y + p, leadIndex.z].isLeftBlocked = true;
                            afflictedPieces.Add(map[leadIndex.x + i + 1, leadIndex.y + p, leadIndex.z - 1].piece.GetId(), new Vector3Int(leadIndex.x + i + 1, leadIndex.y + p, leadIndex.z));
                        }
                    }
                }
                else
                {
                    map[leadIndex.x + i, leadIndex.y + p, leadIndex.z].leadIndex = default;
                    if (leadIndex.z > 0)
                    {
                        map[leadIndex.x + i, leadIndex.y + p, leadIndex.z - 1].isTopBlocked = false;
                        afflictedPieces.Add(map[leadIndex.x + i, leadIndex.y + p, leadIndex.z - 1].piece.GetId(), new Vector3Int(leadIndex.x + i, leadIndex.y + p, leadIndex.z - 1));
                    }

                    if (i == 0)
                    {
                        if (!IsOutOfMap(new Vector3Int(leadIndex.x + i - 1, leadIndex.y + p, leadIndex.z)) && map[leadIndex.x + i - 1, leadIndex.y + p, leadIndex.z].piece)
                        {
                            map[leadIndex.x + i, leadIndex.y + p, leadIndex.z].isLeftBlocked = false;
                            map[leadIndex.x + i - 1, leadIndex.y + p, leadIndex.z].isRightBlocked = false;
                            afflictedPieces.Add(map[leadIndex.x + i - 1, leadIndex.y + p, leadIndex.z - 1].piece.GetId(), new Vector3Int(leadIndex.x + i - 1, leadIndex.y + p, leadIndex.z));
                        }
                    }
                    else if (i == pieceSize - 1)
                    {
                        if (!IsOutOfMap(new Vector3Int(leadIndex.x + i + 1, leadIndex.y + p, leadIndex.z)) && map[leadIndex.x + i + 1, leadIndex.y + p, leadIndex.z].piece)
                        {
                            map[leadIndex.x + i, leadIndex.y + p, leadIndex.z].isRightBlocked = false;
                            map[leadIndex.x + i + 1, leadIndex.y + p, leadIndex.z].isLeftBlocked = false;
                            afflictedPieces.Add(map[leadIndex.x + i + 1, leadIndex.y + p, leadIndex.z - 1].piece.GetId(), new Vector3Int(leadIndex.x + i + 1, leadIndex.y + p, leadIndex.z));
                        }
                    }
                }
            }
        }

        CalcAvailablePieces(afflictedPieces);
    }

    private bool IsOccupied(Vector3Int leadIndex, int radius, out bool completelyFilled, out List<Vector3Int> pieces)
    {
        pieces = new List<Vector3Int>();
        completelyFilled = false;
        Vector3Int currentIndex;
        if (leadIndex.z >= heightSize || leadIndex.z < 0)
            return false;
        
        for (int i = 0; i < radius; i++)
        {
            for (int p = 0; p < radius; p++)
            {
                currentIndex = new Vector3Int(leadIndex.x + i, leadIndex.y + p, leadIndex.z);
                if (!IsOutOfMap(currentIndex) && map[leadIndex.x + i, leadIndex.y + p, leadIndex.z].piece)
                {
                    pieces.Add(new Vector3Int(i, p, leadIndex.z));
                }
            }
        }

        if (pieces.Count == radius * radius)
        {
            completelyFilled = true;
        }
        else
        {
            completelyFilled = false;
        }

        return pieces.Count > 0;
    }

    private bool IsOccupied(Vector3Int leadIndex, int radius, out bool completelyFilled)
    {
        int count = 0;
        completelyFilled = false;
        Vector3Int currentIndex;
        if (leadIndex.z >= heightSize || leadIndex.z < 0)
            return false;

        for (int i = 0; i < radius; i++)
        {
            for (int p = 0; p < radius; p++)
            {
                currentIndex = new Vector3Int(leadIndex.x + i, leadIndex.y + p, leadIndex.z);
                if (!IsOutOfMap(currentIndex) && map[leadIndex.x + i, leadIndex.y + p, leadIndex.z].piece)
                {
                    count++;
                }
            }
        }

        if (count == radius * radius)
        {
            completelyFilled = true;
        }
        else
        {
            completelyFilled = false;
        }

        return count > 0;
    }

    private bool IsOccupied(Vector3Int leadIndex, int radius)
    {
        int count = 0; 
        Vector3Int currentIndex;
        if (leadIndex.z >= heightSize || leadIndex.z < 0)
            return false;

        for (int i = 0; i < radius; i++)
        {
            for (int p = 0; p < radius; p++)
            {
                currentIndex = new Vector3Int(leadIndex.x + i, leadIndex.y + p, leadIndex.z);
                if (!IsOutOfMap(currentIndex) && map[leadIndex.x + i, leadIndex.y + p, leadIndex.z].piece)
                {
                    count++;
                }
            }
        }

        return count > 0;
    }

    private bool AreSidesBlocked(Vector3Int leadIndex, out List<Vector3Int> pieces)
    {
        bool leftBlocked = false;
        bool rightBlocked = false;
        pieces = new List<Vector3Int>();

        if (!IsOutOfMap(leadIndex + new Vector3Int(-1, 0, 0)))
        {
            for (int i = 0; i < pieceSize; i++)
            {
                if (map[leadIndex.x - 1, leadIndex.y + i, leadIndex.z].piece)
                {
                    leftBlocked = true;
                    pieces.Add(new Vector3Int(leadIndex.x - 1, leadIndex.y + i, leadIndex.z));
                }
            }
        }

        if (!IsOutOfMap(leadIndex + new Vector3Int(pieceSize, 0, 0)))
        {
            for (int i = 0; i < pieceSize; i++)
            {
                if (map[leadIndex.x + pieceSize, leadIndex.y + i, leadIndex.z].piece)
                {
                    leftBlocked = true;
                    pieces.Add(new Vector3Int(leadIndex.x + pieceSize, leadIndex.y + i, leadIndex.z));
                }
            }
        }

        return leftBlocked && rightBlocked;
    }

    private bool AreSidesBlocked(Vector3Int leadIndex)
    {
        bool leftBlocked = false;
        bool rightBlocked = false;

        if (!IsOutOfMap(leadIndex + new Vector3Int(-1, 0, 0)))
        {
            for (int i = 0; i < pieceSize; i++)
            {
                if (map[leadIndex.x - 1, leadIndex.y + i, leadIndex.z].piece)
                {
                    leftBlocked = true;
                }
            }
        }

        if (!IsOutOfMap(leadIndex + new Vector3Int(pieceSize, 0, 0)))
        {
            for (int i = 0; i < pieceSize; i++)
            {
                if (map[leadIndex.x + pieceSize, leadIndex.y + i, leadIndex.z].piece)
                {
                    leftBlocked = true;
                }
            }
        }

        return leftBlocked && rightBlocked;
    }

    private bool IsOutOfMap(Vector3Int index)
    {
        return (index.x < 0) || (index.x > horizontalSize - 1) || (index.y < 0) || (index.y > verticalSize - 1);
    }
}
