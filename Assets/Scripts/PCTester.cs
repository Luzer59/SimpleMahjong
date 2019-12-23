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
                if (map.CheckValidMapLocation(hit.point, out Vector3 validPoint))
                {
                    print("Valid hit " + hit.point + "  " + validPoint);
                    pointer.transform.position = validPoint;
                }
                else
                {
                    print("Invalid hit " + hit.point);
                }
            }
        }
    }
}
