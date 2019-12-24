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
    }
}
