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

    public List<MahjongPiece> availableDebug = new List<MahjongPiece>();

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

        availableDebug = availablePieceLeadIndexes.Values.ToArray().Select(ind => map[ind.x, ind.y, ind.z].piece).ToList();
    }

    public void InitPieces(int maxPieceTypes)
    {
        Dictionary<int, Vector3Int> tempAvailablePieceIndexes = new Dictionary<int, Vector3Int>(availablePieceLeadIndexes);
        Cell[,,] tempMap = new Cell[map.GetLength(0), map.GetLength(1), map.GetLength(2)];
        System.Array.Copy(map, tempMap, map.GetLength(0) * map.GetLength(1) * map.GetLength(2));
        List<MahjongPiece>[] resultTypeList = new List<MahjongPiece>[maxPieceTypes];
        int currentType = 0;
        int index;
        List<Vector3Int> values;

        for (int i = 0; i < maxPieceTypes; i++)
        {
            resultTypeList[i] = new List<MahjongPiece>();
        }

        for (int i = 0; i < pieceLeadIndexes.Count / 2; i++)
        {
            values = tempAvailablePieceIndexes.Values.ToList();

            index = Random.Range(0, values.Count);
            resultTypeList[currentType].Add(tempMap[values[index].x, values[index].y, values[index].z].piece);
            SetCellState(tempMap, tempAvailablePieceIndexes, values[index], null);
            values.RemoveAt(index);

            index = Random.Range(0, values.Count);
            resultTypeList[currentType].Add(tempMap[values[index].x, values[index].y, values[index].z].piece);
            SetCellState(tempMap, tempAvailablePieceIndexes, values[index], null);

            currentType++;
            if (currentType >= maxPieceTypes)
            {
                currentType = 0;
            }
        }

        for (int i = 0; i < maxPieceTypes; i++)
        {
            for (int p = 0; p < resultTypeList[i].Count; p++)
            {
                resultTypeList[i][p].SetVariation(i);
            }
        }
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
        pieceLeadIndexes.Clear();
        availablePieceLeadIndexes.Clear();
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
        return TryPlace(map, availablePieceLeadIndexes, pieceLeadIndexes, leadIndex);
    }

    private bool TryPlace(Cell[,,] map, Dictionary<int, Vector3Int> available, Dictionary<int, Vector3Int> all, Vector3Int leadIndex)
    {
        if (OccupiedPieces(leadIndex, pieceSize) == 0 && OccupiedPieces(leadIndex - new Vector3Int(0, 0, 1), pieceSize) == pieceSize * pieceSize)
        {
            MahjongPiece newPiece = Instantiate(piecePrefab);
            newPiece.Place(GridToWorldPoint(leadIndex));
            all.Add(newPiece.GetId(), leadIndex);
            SetCellState(map, available, leadIndex, newPiece);
            return true;
        }
        return false;
    }

    public bool TryRemove(Vector3Int index, bool sideBlocking)
    {
        return TryRemove(map, availablePieceLeadIndexes, pieceLeadIndexes, index, sideBlocking);
    }

    private bool TryRemove(Cell[,,] map, Dictionary<int, Vector3Int> available, Dictionary<int, Vector3Int> all,  Vector3Int index, bool sideBlocking)
    {
        if (!map[index.x, index.y, index.z].piece)
            return false;

        if (!IsPieceAvailable(index, sideBlocking))
            return false;

        MahjongPiece piece = map[index.x, index.y, index.z].piece;
        SetCellState(map, available, map[index.x, index.y, index.z].leadIndex, null);
        all.Remove(map[index.x, index.y, index.z].piece.GetId());
        piece.Remove();
        return true;
    }

    public bool IsPieceAvailable(Vector3Int index, bool sideBlocking)
    {
        return IsPieceAvailable(map, index, sideBlocking);
    }

    private bool IsPieceAvailable(Cell[,,] map, Vector3Int index, bool sideBlocking)
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

    private void CalcAvailablePieces(Cell[,,] map, Dictionary<int, Vector3Int> available, Dictionary<int, Vector3Int> changed)
    {
        List<Vector3Int> indexes = changed.Values.ToList();
        for (int i = 0; i < indexes.Count; i++)
        {
            // TODO: Fix available list

            if (IsPieceAvailable(map, indexes[i], true))
            {
                if (!available.ContainsKey(map[indexes[i].x, indexes[i].y, indexes[i].z].piece.GetId()))
                {
                    available.Add(map[indexes[i].x, indexes[i].y, indexes[i].z].piece.GetId(), indexes[i]);
                }
            }
            else
            {
                if (available.ContainsKey(map[indexes[i].x, indexes[i].y, indexes[i].z].piece.GetId()))
                {
                    available.Remove(map[indexes[i].x, indexes[i].y, indexes[i].z].piece.GetId());
                }
            }
        }
    }

    private void SetCellState(Cell[,,] map, Dictionary<int, Vector3Int> available, Vector3Int leadIndex, MahjongPiece piece)
    {
        Dictionary<int, Vector3Int> afflictedPieces = new Dictionary<int, Vector3Int>();
        if (piece)
        {
            afflictedPieces.Add(piece.GetId(), leadIndex);
        }
        else
        {
            afflictedPieces.Add(map[leadIndex.x, leadIndex.y, leadIndex.z].piece.GetId(), leadIndex);
        }

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
                        if (!afflictedPieces.ContainsKey(map[leadIndex.x + i, leadIndex.y + p, leadIndex.z - 1].piece.GetId()))
                        {
                            afflictedPieces.Add(map[leadIndex.x + i, leadIndex.y + p, leadIndex.z - 1].piece.GetId(), new Vector3Int(leadIndex.x + i, leadIndex.y + p, leadIndex.z - 1));
                        }
                    }

                    if (i == 0)
                    {
                        if (!IsOutOfMap(new Vector3Int(leadIndex.x + i - 1, leadIndex.y + p, leadIndex.z)) && map[leadIndex.x + i - 1, leadIndex.y + p, leadIndex.z].piece)
                        {
                            map[leadIndex.x + i, leadIndex.y + p, leadIndex.z].isLeftBlocked = true;
                            map[leadIndex.x + i - 1, leadIndex.y + p, leadIndex.z].isRightBlocked = true;
                            if (!afflictedPieces.ContainsKey(map[leadIndex.x + i - 1, leadIndex.y + p, leadIndex.z].piece.GetId()))
                            {
                                afflictedPieces.Add(map[leadIndex.x + i - 1, leadIndex.y + p, leadIndex.z].piece.GetId(), new Vector3Int(leadIndex.x + i - 1, leadIndex.y + p, leadIndex.z));
                            }
                        }
                    }
                    else if (i == pieceSize - 1)
                    {
                        if (!IsOutOfMap(new Vector3Int(leadIndex.x + i + 1, leadIndex.y + p, leadIndex.z)) && map[leadIndex.x + i + 1, leadIndex.y + p, leadIndex.z].piece)
                        {
                            map[leadIndex.x + i, leadIndex.y + p, leadIndex.z].isRightBlocked = true;
                            map[leadIndex.x + i + 1, leadIndex.y + p, leadIndex.z].isLeftBlocked = true;
                            if (!afflictedPieces.ContainsKey(map[leadIndex.x + i + 1, leadIndex.y + p, leadIndex.z].piece.GetId()))
                            {
                                afflictedPieces.Add(map[leadIndex.x + i + 1, leadIndex.y + p, leadIndex.z].piece.GetId(), new Vector3Int(leadIndex.x + i + 1, leadIndex.y + p, leadIndex.z));
                            }
                        }
                    }
                }
                else
                {
                    map[leadIndex.x + i, leadIndex.y + p, leadIndex.z].leadIndex = default;
                    if (leadIndex.z > 0)
                    {
                        map[leadIndex.x + i, leadIndex.y + p, leadIndex.z - 1].isTopBlocked = false;
                        if (!afflictedPieces.ContainsKey(map[leadIndex.x + i, leadIndex.y + p, leadIndex.z - 1].piece.GetId()))
                        {
                            afflictedPieces.Add(map[leadIndex.x + i, leadIndex.y + p, leadIndex.z - 1].piece.GetId(), new Vector3Int(leadIndex.x + i, leadIndex.y + p, leadIndex.z - 1));
                        }
                    }

                    if (i == 0)
                    {
                        if (!IsOutOfMap(new Vector3Int(leadIndex.x + i - 1, leadIndex.y + p, leadIndex.z)) && map[leadIndex.x + i - 1, leadIndex.y + p, leadIndex.z].piece)
                        {
                            map[leadIndex.x + i, leadIndex.y + p, leadIndex.z].isLeftBlocked = false;
                            map[leadIndex.x + i - 1, leadIndex.y + p, leadIndex.z].isRightBlocked = false;
                            if (!afflictedPieces.ContainsKey(map[leadIndex.x + i - 1, leadIndex.y + p, leadIndex.z].piece.GetId()))
                            {
                                afflictedPieces.Add(map[leadIndex.x + i - 1, leadIndex.y + p, leadIndex.z].piece.GetId(), new Vector3Int(leadIndex.x + i - 1, leadIndex.y + p, leadIndex.z));
                            }
                        }
                    }
                    else if (i == pieceSize - 1)
                    {
                        if (!IsOutOfMap(new Vector3Int(leadIndex.x + i + 1, leadIndex.y + p, leadIndex.z)) && map[leadIndex.x + i + 1, leadIndex.y + p, leadIndex.z].piece)
                        {
                            map[leadIndex.x + i, leadIndex.y + p, leadIndex.z].isRightBlocked = false;
                            map[leadIndex.x + i + 1, leadIndex.y + p, leadIndex.z].isLeftBlocked = false;
                            if (!afflictedPieces.ContainsKey(map[leadIndex.x + i + 1, leadIndex.y + p, leadIndex.z].piece.GetId()))
                            {
                                afflictedPieces.Add(map[leadIndex.x + i + 1, leadIndex.y + p, leadIndex.z].piece.GetId(), new Vector3Int(leadIndex.x + i + 1, leadIndex.y + p, leadIndex.z));
                            }
                        }
                    }
                }
            }
        }

        CalcAvailablePieces(map, available, afflictedPieces);
    }

    private int OccupiedPieces(Vector3Int leadIndex, int radius, out List<Vector3Int> results)
    {
        results = new List<Vector3Int>();
        Vector3Int currentIndex;
        if (leadIndex.z >= heightSize || leadIndex.z < 0)
            return pieceSize * pieceSize;
        
        for (int i = 0; i < radius; i++)
        {
            for (int p = 0; p < radius; p++)
            {
                currentIndex = new Vector3Int(leadIndex.x + i, leadIndex.y + p, leadIndex.z);
                if (!IsOutOfMap(currentIndex) && map[leadIndex.x + i, leadIndex.y + p, leadIndex.z].piece)
                {
                    results.Add(new Vector3Int(i, p, leadIndex.z));
                }
            }
        }

        return results.Count;
    }

    private int OccupiedPieces(Vector3Int leadIndex, int radius)
    {
        int count = 0; 
        Vector3Int currentIndex;
        if (leadIndex.z >= heightSize || leadIndex.z < 0)
            return pieceSize * pieceSize;

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

        return count;
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
