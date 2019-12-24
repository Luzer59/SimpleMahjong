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
    private int pieceSize = 1;

    [SerializeField]
    private MahjongPiece piecePrefab;

    private Cell[][,] map;

#if UNITY_EDITOR
    [SerializeField]
    private bool debug = false;
    [SerializeField]
    private float debugSpacing = 0.2f;
    [SerializeField]
    private GameObject cellDebugPrefab;
    [SerializeField]
    private Material cellDebugPrefabFilledMat;
    [SerializeField]
    private Material cellDebugPrefabEmptyMat;
#endif

    private class Cell
    {
        public Cell()
        {

        }

        public MahjongPiece upLeft;
        public MahjongPiece upRight;
        public MahjongPiece downLeft;
        public MahjongPiece downRight;
        public MahjongPiece center;

#if UNITY_EDITOR
        public GameObject upLeftDebug;
        public GameObject upRightDebug;
        public GameObject downLeftDebug;
        public GameObject downRightDebug;
        public GameObject centerDebug;
#endif
    }

    private class RandomMahjongRow
    {
        public RandomMahjongRow(List<RandomMahjongRow> lowerRows)
        {
            this.lowerRows = lowerRows;
            pieces = new List<MahjongPiece>();
            piecesTaken = new List<bool>();
            piecesTakenCount = 0;
        }

        private List<RandomMahjongRow> lowerRows;
        private List<MahjongPiece> pieces;
        private List<bool> piecesTaken;
        private int piecesTakenCount;

        public void Add(MahjongPiece piece)
        {
            pieces.Add(piece);
            piecesTaken.Add(false);
        }

        public void RemoveEqual(List<MahjongPiece> list)
        {
            for (int i = 0; i < pieces.Count; i++)
            {
                list.Remove(pieces[i]);
            }
        }

        public MahjongPiece Take()
        {
            List<MahjongPiece> input;
            List<int> valid = new List<int>();
            for (int i = 0; i < pieces.Count; i++)
            {
                if (piecesTakenCount != 0)
                {
                    if (piecesTaken[i])
                        continue;

                    if (pieces.Count > 1)
                    {
                        if (i == 0)
                        {
                            if (!piecesTaken[i + 1])
                                continue;
                        }
                        else if (i == pieces.Count - 1)
                        {
                            if (!piecesTaken[i - 1])
                                continue;
                        }
                        else
                        {
                            if (!piecesTaken[i + 1] && !piecesTaken[i - 1])
                                continue;
                        }
                    }
                }

                input = pieces[i].piecesBellow.ToList();
                for (int p = 0; p < lowerRows.Count; p++)
                {
                    lowerRows[p].RemoveEqual(input);
                    if (input.Count == 0)
                        break;
                }

                valid.Add(i);
            }

            int random = Random.Range(0, valid.Count);
            piecesTaken[valid[random]] = true;
            piecesTakenCount++;

            return pieces[valid[random]];
        }
    }

    private void Awake()
    {
        map = new Cell[heightSize][,];
        for (int i = 0; i < heightSize; i++)
        {
            map[i] = new Cell[horizontalSize, verticalSize];
            for (int p = 0; p < horizontalSize; p++)
            {
                for (int u = 0; u < verticalSize; u++)
                {
                    map[i][p, u] = new Cell();
#if UNITY_EDITOR
                    if (debug && i == 0)
                    {
                        map[i][p, u].upLeftDebug = Instantiate(cellDebugPrefab, GridToWorldPoint(new Vector3Int(p, u, i)) + new Vector3(-horizontalSpacing * debugSpacing, 0f, verticalSpacing * debugSpacing), Quaternion.identity, transform);
                        map[i][p, u].upRightDebug = Instantiate(cellDebugPrefab, GridToWorldPoint(new Vector3Int(p, u, i)) + new Vector3(horizontalSpacing * debugSpacing, 0f, verticalSpacing * debugSpacing), Quaternion.identity, transform);
                        map[i][p, u].downLeftDebug = Instantiate(cellDebugPrefab, GridToWorldPoint(new Vector3Int(p, u, i)) + new Vector3(-horizontalSpacing * debugSpacing, 0f, -verticalSpacing * debugSpacing), Quaternion.identity, transform);
                        map[i][p, u].downRightDebug = Instantiate(cellDebugPrefab, GridToWorldPoint(new Vector3Int(p, u, i)) + new Vector3(horizontalSpacing * debugSpacing, 0f, -verticalSpacing * debugSpacing), Quaternion.identity, transform);
                        map[i][p, u].centerDebug = Instantiate(cellDebugPrefab, GridToWorldPoint(new Vector3Int(p, u, i)), Quaternion.identity, transform);
                    }
#endif
                }
            }
        }
    }

    private Vector3Int WorldPointToGrid(Vector3 worldPoint)
    {
        Vector3 localPoint = transform.InverseTransformPoint(worldPoint);
        float horLimitPos = (horizontalSize - 1) / 2f * horizontalSpacing;
        float verLimitPos = (verticalSize - 1) / 2f * verticalSpacing;
        return new Vector3Int(
            Mathf.RoundToInt(Mathf.InverseLerp(-horLimitPos, horLimitPos, localPoint.x) * (horizontalSize - 1)),
            Mathf.RoundToInt(Mathf.InverseLerp(verLimitPos, -verLimitPos, localPoint.z) * (verticalSize - 1)),
            0);
    }

    private Vector3Int WorldPointToHighestGridCenter(Vector3 worldPoint)
    {
        Vector3Int index = WorldPointToGrid(worldPoint);
        for (int i = 0; i < heightSize; i++)
        {
            if (map[i][index.x, index.y].center)
            {
                index.z = i;
            }
            else
            {
                break;
            }
        }
        return index;
    }

    private Vector3 GridToWorldPoint(Vector3Int index)
    {
        return transform.TransformPoint(new Vector3(
            -((horizontalSize - 1) / 2f * horizontalSpacing) + horizontalSpacing * index.x, 
            index.z * heightSpacing, 
            (verticalSize - 1) / 2f * verticalSpacing - verticalSpacing * index.y));
    }

    public void LoadMap(MahjongMapData data)
    {
        if (data.pieceCount % 2 != 0)
        {
            Debug.LogError("Non-even map size");
            return;
        }

        UnloadMap();

        for (int i = 0; i < data.pieceCount; i++)
        {
            TryPlace(new Vector3Int(data.pieceHorizontalIndex[i], data.pieceVerticalIndex[i], data.pieceHeightIndex[i]), false);
        }
    }

    public void InitPieces(int maxPieceTypes)
    {
        List<RandomMahjongRow> rows = new List<RandomMahjongRow>();
        int lastHeightStartIndex = 0;
        int lastHeightEndIndex = 0;
        bool rowActive = false;
        int totalPieces = 0;

        // Find rows
        for (int u = 0; u < heightSize; u++)
        {
            for (int p = 0; p < verticalSize; p++)
            {
                for (int i = 0; i < horizontalSize; i++)
                {
                    if (map[u][i, p].center && !rowActive)
                    {
                        rows.Add(new RandomMahjongRow(rows.GetRange(lastHeightStartIndex, lastHeightEndIndex - lastHeightStartIndex)));
                        rowActive = true;
                    }
                    else
                    {
                        rowActive = false;
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
        for (int i = 0; i < totalPieces; i++)
        {
            MahjongPiece piece = rows[Random.Range(0, rows.Count)].Take();
            piece.SetType(currentPieceType);
            if (i % 2 == 1)
            {
                currentPieceType++;
                if (currentPieceType > maxPieceTypes - 1)
                {
                    currentPieceType = 0;
                }
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
                    if (map[u][i, p].center)
                    {
                        map[u][i, p].center.Remove();
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
                    if (map[u][i, p].center)
                    {
                        pieces.Add(new Vector3Int(i, p, u));
                    }
                }
            }
        }
        MahjongMapData data = new MahjongMapData(pieces.Count);
        for (int i = 0; i < data.pieceCount; i++)
        {
            data.pieceHorizontalIndex[i] = pieces[i].x;
            data.pieceVerticalIndex[i] = pieces[i].y;
            data.pieceHeightIndex[i] = pieces[i].z;
        }
        return data;
    }

    public bool GetPossiblePieceIndex(Vector3 worldPoint, out Vector3Int index, out HashSet<MahjongPiece> piecesBellow)
    {
        index = WorldPointToGrid(worldPoint);
        piecesBellow = new HashSet<MahjongPiece>();

        if (CheckOutOfMap(index, pieceSize))
            return false;

        for (int i = 0; i < heightSize; i++)
        {
            index.z = i;
            if (GetOccupyingPieces(index, out bool filled, out HashSet<MahjongPiece> pieces))
            {
                if (filled)
                {
                    // Fully filled, may be possible on higher level
                    piecesBellow = pieces;
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
                // Not filled, valid position
                return true;
            }
        }
        // Height cap reached
        return false;
    }

    public bool TryPlace(Vector3Int index, bool needSupport)
    {
        if (!GetOccupyingPieces(index, out bool filled, out HashSet<MahjongPiece> pieces))
        {
            HashSet<MahjongPiece> piecesBellow = new HashSet<MahjongPiece>();
            filled = true;
            if (needSupport && index.z > 0)
            {
                GetOccupyingPieces(new Vector3Int(index.x, index.y, index.z - 1), out filled, out piecesBellow);
            }
            if (filled)
            {
                GetSideBlockingPieces(index, out HashSet<MahjongPiece> piecesLeft, out HashSet<MahjongPiece> piecesRight);
                HashSet<MahjongPiece> piecesAbove = new HashSet<MahjongPiece>();
                if (index.z < heightSize - 1)
                {
                    GetOccupyingPieces(index, out filled, out piecesAbove);
                }
                MahjongPiece newPiece = Instantiate(piecePrefab);
                newPiece.Place(GridToWorldPoint(index), piecesAbove, piecesBellow, piecesLeft, piecesRight);
                SetCellState(index, newPiece);
                return true;
            }
        }
        return false;
    }

    public bool TryPlace(Vector3 worldPoint, bool needSupport)
    {
        if (GetPossiblePieceIndex(worldPoint, out Vector3Int index, out HashSet<MahjongPiece> piecesBellow))
        {
            return TryPlace(index, needSupport);
        }
        return false;
    }

    public bool TryRemove(Vector3Int index, bool sideBlocking)
    {
        if ((sideBlocking && map[index.z][index.x, index.y].center.AreSidesBlocked()) || map[index.z][index.x, index.y].center.IsAboveBlocked())
            return false;

        map[index.z][index.x, index.y].center.Remove();
        SetCellState(index, null);
        return true;
    }

    public bool TryRemove(MahjongPiece piece, bool sideBlocking)
    {
        return TryRemove(WorldPointToHighestGridCenter(piece.GetWorldPoint()), sideBlocking);
    }

    private void SetCellState(Vector3Int index, MahjongPiece piece)
    {
        for (int i = -pieceSize; i < 1 + pieceSize; i++)
        {
            for (int p = -pieceSize; p < 1 + pieceSize; p++)
            {
                if (i == -pieceSize)
                {
                    // Left side
                    if (p == -pieceSize)
                    {
                        // Upper side
                        map[index.z][index.x + i, index.y + p].downRight = piece;
#if UNITY_EDITOR
                        if (debug && index.z == 0)
                        {
                            map[index.z][index.x + i, index.y + p].downRightDebug.GetComponent<MeshRenderer>().material = piece ? cellDebugPrefabFilledMat : cellDebugPrefabEmptyMat;
                        }
#endif
                    }
                    else if (p == pieceSize)
                    {
                        // Lower side
                        map[index.z][index.x + i, index.y + p].upRight = piece;
#if UNITY_EDITOR
                        if (debug && index.z == 0)
                        {
                            map[index.z][index.x + i, index.y + p].upRightDebug.GetComponent<MeshRenderer>().material = piece ? cellDebugPrefabFilledMat : cellDebugPrefabEmptyMat;
                        }
#endif
                    }
                    else
                    {
                        // Middle
                        map[index.z][index.x + i, index.y + p].downRight = piece;
                        map[index.z][index.x + i, index.y + p].upRight = piece;
#if UNITY_EDITOR
                        if (debug && index.z == 0)
                        {
                            map[index.z][index.x + i, index.y + p].downRightDebug.GetComponent<MeshRenderer>().material = piece ? cellDebugPrefabFilledMat : cellDebugPrefabEmptyMat;
                            map[index.z][index.x + i, index.y + p].upRightDebug.GetComponent<MeshRenderer>().material = piece ? cellDebugPrefabFilledMat : cellDebugPrefabEmptyMat;
                        }
#endif
                    }
                }
                else if (i == pieceSize)
                {
                    // Right side
                    if (p == -pieceSize)
                    {
                        // Upper side
                        map[index.z][index.x + i, index.y + p].downLeft = piece;
#if UNITY_EDITOR
                        if (debug && index.z == 0)
                        {
                            map[index.z][index.x + i, index.y + p].downLeftDebug.GetComponent<MeshRenderer>().material = piece ? cellDebugPrefabFilledMat : cellDebugPrefabEmptyMat;
                        }
#endif
                    }
                    else if (p == pieceSize)
                    {
                        // Lower side
                        map[index.z][index.x + i, index.y + p].upLeft = piece;
#if UNITY_EDITOR
                        if (debug && index.z == 0)
                        {
                            map[index.z][index.x + i, index.y + p].upLeftDebug.GetComponent<MeshRenderer>().material = piece ? cellDebugPrefabFilledMat : cellDebugPrefabEmptyMat;
                        }
#endif
                    }
                    else
                    {
                        // Middle
                        map[index.z][index.x + i, index.y + p].downLeft = piece;
                        map[index.z][index.x + i, index.y + p].upLeft = piece;
#if UNITY_EDITOR
                        if (debug && index.z == 0)
                        {
                            map[index.z][index.x + i, index.y + p].downLeftDebug.GetComponent<MeshRenderer>().material = piece ? cellDebugPrefabFilledMat : cellDebugPrefabEmptyMat;
                            map[index.z][index.x + i, index.y + p].upLeftDebug.GetComponent<MeshRenderer>().material = piece ? cellDebugPrefabFilledMat : cellDebugPrefabEmptyMat;
                        }
#endif
                    }
                }
                else
                {
                    // Middle
                    if (p == -pieceSize)
                    {
                        // Upper side
                        map[index.z][index.x + i, index.y + p].downRight = piece;
                        map[index.z][index.x + i, index.y + p].downLeft = piece;
#if UNITY_EDITOR
                        if (debug && index.z == 0)
                        {
                            map[index.z][index.x + i, index.y + p].downRightDebug.GetComponent<MeshRenderer>().material = piece ? cellDebugPrefabFilledMat : cellDebugPrefabEmptyMat;
                            map[index.z][index.x + i, index.y + p].downLeftDebug.GetComponent<MeshRenderer>().material = piece ? cellDebugPrefabFilledMat : cellDebugPrefabEmptyMat;
                        }
#endif
                    }
                    else if (p == pieceSize)
                    {
                        // Lower side
                        map[index.z][index.x + i, index.y + p].upRight = piece;
                        map[index.z][index.x + i, index.y + p].upLeft = piece;
#if UNITY_EDITOR
                        if (debug && index.z == 0)
                        {
                            map[index.z][index.x + i, index.y + p].upRightDebug.GetComponent<MeshRenderer>().material = piece ? cellDebugPrefabFilledMat : cellDebugPrefabEmptyMat;
                            map[index.z][index.x + i, index.y + p].upLeftDebug.GetComponent<MeshRenderer>().material = piece ? cellDebugPrefabFilledMat : cellDebugPrefabEmptyMat;
                        }
#endif
                    }
                    else
                    {
                        // Middle
                        map[index.z][index.x + i, index.y + p].upRight = piece;
                        map[index.z][index.x + i, index.y + p].upLeft = piece;
                        map[index.z][index.x + i, index.y + p].downRight = piece;
                        map[index.z][index.x + i, index.y + p].downLeft = piece;
                        map[index.z][index.x + i, index.y + p].center = piece;
#if UNITY_EDITOR
                        if (debug && index.z == 0)
                        {
                            map[index.z][index.x + i, index.y + p].upRightDebug.GetComponent<MeshRenderer>().material = piece ? cellDebugPrefabFilledMat : cellDebugPrefabEmptyMat;
                            map[index.z][index.x + i, index.y + p].upLeftDebug.GetComponent<MeshRenderer>().material = piece ? cellDebugPrefabFilledMat : cellDebugPrefabEmptyMat;
                            map[index.z][index.x + i, index.y + p].downRightDebug.GetComponent<MeshRenderer>().material = piece ? cellDebugPrefabFilledMat : cellDebugPrefabEmptyMat;
                            map[index.z][index.x + i, index.y + p].downLeftDebug.GetComponent<MeshRenderer>().material = piece ? cellDebugPrefabFilledMat : cellDebugPrefabEmptyMat;
                            map[index.z][index.x + i, index.y + p].centerDebug.GetComponent<MeshRenderer>().material = piece ? cellDebugPrefabFilledMat : cellDebugPrefabEmptyMat;
                        }
#endif
                    }
                }
            }
        }
    }

    private bool GetOccupyingPieces(Vector3Int index, out bool completelyFilled, out HashSet<MahjongPiece> pieces)
    {
        Cell cell;
        pieces = new HashSet<MahjongPiece>();
        bool found = false;
        int cellsLeft = (1 + pieceSize * 2) * (1 + pieceSize * 2);
        for (int i = -pieceSize; i < 1 + pieceSize; i++)
        {
            for (int p = -pieceSize; p < 1 + pieceSize; p++)
            {
                cell = map[index.z][index.x + i, index.y + p];
                if (i == -pieceSize)
                {
                    // Left side
                    if (p == -pieceSize)
                    {
                        // Upper side
                        if (cell.downRight)
                        {
                            pieces.Add(cell.downRight);
                            cellsLeft--;
                            found = true;
                        }
                    }
                    else if (p == pieceSize)
                    {
                        // Lower side
                        if (cell.upRight)
                        {
                            pieces.Add(cell.upRight);
                            cellsLeft--;
                            found = true;
                        }
                    }
                    else
                    {
                        // Middle
                        if (cell.downRight)
                        {
                            pieces.Add(cell.downRight);
                            cellsLeft--;
                            found = true;
                        }
                        else if (cell.upRight)
                        {
                            pieces.Add(cell.upRight);
                            cellsLeft--;
                            found = true;
                        }
                    }
                }
                else if (i == pieceSize)
                {
                    // Right side
                    if (p == -pieceSize)
                    {
                        // Upper side
                        if (cell.downLeft)
                        {
                            pieces.Add(cell.downLeft);
                            cellsLeft--;
                            found = true;
                        }
                    }
                    else if (p == pieceSize)
                    {
                        // Lower side
                        if (cell.upLeft)
                        {
                            pieces.Add(cell.upLeft);
                            cellsLeft--;
                            found = true;
                        }
                    }
                    else
                    {
                        // Middle
                        if (cell.downLeft)
                        {
                            pieces.Add(cell.downLeft);
                            cellsLeft--;
                            found = true;
                        }
                        else if (cell.upLeft)
                        {
                            pieces.Add(cell.upLeft);
                            cellsLeft--;
                            found = true;
                        }
                    }
                }
                else
                {
                    // Middle
                    if (p == -pieceSize)
                    {
                        // Upper side
                        if (cell.downRight)
                        {
                            pieces.Add(cell.downRight);
                            cellsLeft--;
                            found = true;
                        }
                        else if (cell.downLeft)
                        {
                            pieces.Add(cell.downLeft);
                            cellsLeft--;
                            found = true;
                        }
                    }
                    else if (p == pieceSize)
                    {
                        // Lower side
                        if (cell.upRight)
                        {
                            pieces.Add(cell.upRight);
                            cellsLeft--;
                            found = true;
                        }
                        else if (cell.upLeft)
                        {
                            pieces.Add(cell.upLeft);
                            cellsLeft--;
                            found = true;
                        }
                    }
                    else
                    {
                        // Middle
                        if (cell.upRight)
                        {
                            pieces.Add(cell.upRight);
                            cellsLeft--;
                            found = true;
                        }
                        else if (cell.downRight)
                        {
                            pieces.Add(cell.downRight);
                            cellsLeft--;
                            found = true;
                        }
                        else if (cell.upLeft)
                        {
                            pieces.Add(cell.upLeft);
                            cellsLeft--;
                            found = true;
                        }
                        else if (cell.downLeft)
                        {
                            pieces.Add(cell.downLeft);
                            cellsLeft--;
                            found = true;
                        }
                    }
                }
            }
        }
        completelyFilled = cellsLeft == 0;
        return found;
    }

    private bool GetSideBlockingPieces(Vector3Int index, out HashSet<MahjongPiece> left, out HashSet<MahjongPiece> right)
    {
        Cell cell;
        left = new HashSet<MahjongPiece>();
        right = new HashSet<MahjongPiece>();
        bool leftFound = false;
        bool rightFound = false;
        for (int i = -pieceSize; i < 1 + pieceSize; i += pieceSize)
        {
            for (int p = -pieceSize; p < 1 + pieceSize; p++)
            {
                cell = map[index.z][index.x + i, index.y + p];
                if (i == -pieceSize)
                {
                    // Left side
                    if (p == -pieceSize)
                    {
                        // Upper side
                        if (cell.downLeft)
                        {
                            left.Add(cell.downLeft);
                            leftFound = true;
                        }
                    }
                    else if (p == pieceSize)
                    {
                        // Lower side
                        if (cell.upLeft)
                        {
                            left.Add(cell.upLeft);
                            leftFound = true;
                        }
                    }
                    else
                    {
                        // Middle
                        if (cell.downLeft)
                        {
                            left.Add(cell.downLeft);
                            leftFound = true;
                        }
                        else if (cell.upLeft)
                        {
                            left.Add(cell.upLeft);
                            leftFound = true;
                        }
                    }
                }
                else if (i == pieceSize)
                {
                    // Right side
                    if (p == -pieceSize)
                    {
                        // Upper side
                        if (cell.downRight)
                        {
                            right.Add(cell.downRight);
                            rightFound = true;
                        }
                    }
                    else if (p == pieceSize)
                    {
                        // Lower side
                        if (cell.upRight)
                        {
                            right.Add(cell.upRight);
                            rightFound = true;
                        }
                    }
                    else
                    {
                        // Middle
                        if (cell.downRight)
                        {
                            right.Add(cell.downRight);
                            rightFound = true;
                        }
                        else if (cell.upRight)
                        {
                            right.Add(cell.upRight);
                            rightFound = true;
                        }
                    }
                }
            }
        }
        return leftFound && rightFound;
    }

    private bool CheckOutOfMap(Vector3Int index, int radius)
    {
        return (index.x - radius < 0) || (index.x + radius > horizontalSize - 1) || (index.y - radius < 0) || (index.y + radius > verticalSize - 1);
    }
}
