using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class MahjongPiece : MonoBehaviour
{
    private int type;
    private int id;

    [SerializeField]
    private GameObject defaultTypePrefab;
    [SerializeField]
    private GameObject[] typePrefabs;

    [SerializeField]
    private GameObject activeTypeObject;

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

    public void Place(Vector3 worldPoint)
    {
        transform.position = worldPoint;
        id = Id.Get();
        SetVariation(-1);
    }

    public void Remove()
    {
        Destroy(gameObject);
    }

    public int GetId()
    {
        return id;
    }

    public int GetVariation()
    {
        return type;
    }

    public void SetVariation(int type)
    {
        this.type = type;

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
