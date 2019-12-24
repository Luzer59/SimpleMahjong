using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MahjongLevelBuilderController : MonoBehaviour
{
    [SerializeField]
    private MahjongPieceMap map;

    public void SaveMap()
    {
        FileIO.Save("level", map.ConvertLoadedMap());
    }

    public void LoadMap()
    {
        map.LoadMap(FileIO.Load<MahjongMapData>("level"));
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
        {
            SaveMap();
        }
        if (Input.GetKeyDown(KeyCode.L))
        {
            LoadMap();
        }

        if (Input.GetMouseButtonDown(0))
        {
            Ray cameraRay = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(cameraRay, out RaycastHit hit))
            {
                if (map.TryPlace(hit.point, true))
                {
                    print("Valid hit " + hit.point);
                }
                else
                {
                    print("Invalid hit " + hit.point);
                }
            }
        }
        if (Input.GetMouseButtonDown(1))
        {
            Ray cameraRay = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(cameraRay, out RaycastHit hit))
            {
                MahjongPiece piece = hit.collider.GetComponentInParent<MahjongPiece>();
                if (piece && map.TryRemove(piece, false))
                {
                    print("Valid hit " + hit.point);
                }
                else
                {
                    print("Invalid hit " + hit.point);
                }
            }
        }
    }
}
