using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PCTester : MonoBehaviour
{
    new public Camera camera;
    public MahjongPieceMap map;
    public GameObject pointer;

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray cameraRay = camera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(cameraRay, out RaycastHit hit))
            {
                if (map.CanPlace(hit.point))
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
