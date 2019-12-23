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
                    if (debug)
                    {
                        map[i][p, u].upLeftDebug = Instantiate(cellDebugPrefab, GridToWorldPoint(new Vector3Int(p, u, i)) + new Vector3(-horizontalSpacing * debugSpacing, 0f, verticalSpacing * debugSpacing), Quaternion.identity, transform);
                        map[i][p, u].upRightDebug = Instantiate(cellDebugPrefab, GridToWorldPoint(new Vector3Int(p, u, i)) + new Vector3(horizontalSpacing * debugSpacing, 0f, verticalSpacing * debugSpacing), Quaternion.identity, transform);
                        map[i][p, u].downLeftDebug = Instantiate(cellDebugPrefab, GridToWorldPoint(new Vector3Int(p, u, i)) + new Vector3(-horizontalSpacing * debugSpacing, 0f, -verticalSpacing * debugSpacing), Quaternion.identity, transform);
                        map[i][p, u].downRightDebug = Instantiate(cellDebugPrefab, GridToWorldPoint(new Vector3Int(p, u, i)) + new Vector3(horizontalSpacing * debugSpacing, 0f, -verticalSpacing * debugSpacing), Quaternion.identity, transform);
                        map[i][p, u].centerDebug = Instantiate(cellDebugPrefab, GridToWorldPoint(new Vector3Int(p, u, i)), Quaternion.identity, transform);
                    }
                }
            }
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

    private Vector3 GridToWorldPoint(Vector3Int index)
    {
        return transform.TransformPoint(new Vector3(
            -((horizontalSize - 1) / 2f * horizontalSpacing) + horizontalSpacing * index.x, 
            index.z * heightSpacing, 
            (verticalSize - 1) / 2f * verticalSpacing - verticalSpacing * index.y));
    }

    public bool CanPlace(Vector3 worldPoint, out Vector3Int index, out HashSet<MahjongPiece> piecesBellow)
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

    public bool TryPlace(Vector3 worldPoint, MahjongPiece piece)
    {
        if (CanPlace(worldPoint, out Vector3Int index, out HashSet<MahjongPiece> piecesBellow))
        {
            GetSideBlockingPieces(index, out HashSet<MahjongPiece> piecesLeft, out HashSet<MahjongPiece> piecesRight);
            piece.Place(GridToWorldPoint(index), piecesBellow, piecesLeft, piecesRight);
            SetCellState(index, piece);
            return true;
        }
        return false;
    }

    public bool CanRemove(Vector3 worldPoint, bool sideBlocking = true)
    {
        Vector3Int index = WorldPointToGrid(worldPoint);

        if (map[index.z][index.x, index.y].center)
        {
            return CanRemove(map[index.z][index.x, index.y].center, sideBlocking);
        }

        return false;
    }

    public bool CanRemove(MahjongPiece piece, bool sideBlocking = true)
    {
        if ((sideBlocking && piece.AreSidesBlocked()) || piece.IsAboveBlocking())
            return false;

        return true;
    }

    public bool TryRemove(Vector3 worldPoint, bool sideBlocking = true)
    {
        if (CanRemove(worldPoint, sideBlocking))
        {
            Vector3Int index = WorldPointToGrid(worldPoint);
            map[index.z][index.x, index.y].center.Remove();
            SetCellState(index, null);
        }
        return false;
    }

    public bool TryRemove(MahjongPiece piece, bool sideBlocking = true)
    {
        if (CanRemove(piece, sideBlocking))
        {
            Vector3Int index = WorldPointToGrid(piece.GetWorldPoint());
            piece.Remove();
            SetCellState(index, null);
        }
        return false;
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
                        if (debug)
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
                        if (debug)
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
                        if (debug)
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
                        if (debug)
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
                        if (debug)
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
                        if (debug)
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
                        if (debug)
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
                        if (debug)
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
                        if (debug)
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
