using System.Collections;
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
            piecesAbove.Add(inputArray[i]);
            inputArray[i].piecesBellow.Add(this);
        }
        inputArray = bellow.ToArray();
        for (int i = 0; i < inputArray.Length; i++)
        {
            piecesBellow.Add(inputArray[i]);
            inputArray[i].piecesAbove.Add(this);
        }
        inputArray = left.ToArray();
        for (int i = 0; i < inputArray.Length; i++)
        {
            piecesLeft.Add(inputArray[i]);
            inputArray[i].piecesRight.Add(this);
        }
        inputArray = right.ToArray();
        for (int i = 0; i < inputArray.Length; i++)
        {
            piecesRight.Add(inputArray[i]);
            inputArray[i].piecesLeft.Add(this);
        }
    }

    public void Remove()
    {
        MahjongPiece[] inputArray = piecesAbove.ToArray();
        for (int i = 0; i < inputArray.Length; i++)
        {
            piecesAbove.Remove(inputArray[i]);
            inputArray[i].piecesBellow.Remove(this);
        }
        inputArray = piecesBellow.ToArray();
        for (int i = 0; i < inputArray.Length; i++)
        {
            piecesBellow.Remove(inputArray[i]);
            inputArray[i].piecesAbove.Remove(this);
        }
        inputArray = piecesLeft.ToArray();
        for (int i = 0; i < inputArray.Length; i++)
        {
            piecesLeft.Remove(inputArray[i]);
            inputArray[i].piecesRight.Remove(this);
        }
        inputArray = piecesRight.ToArray();
        for (int i = 0; i < inputArray.Length; i++)
        {
            piecesRight.Remove(inputArray[i]);
            inputArray[i].piecesLeft.Remove(this);
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
            activeTypeObject.transform.localPosition = Vector3.zero;
            activeTypeObject.transform.localRotation = Quaternion.identity;
        }
    }
}
