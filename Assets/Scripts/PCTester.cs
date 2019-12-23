using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PCTester : MonoBehaviour
{
    new public Camera camera;
    public MahjongPieceMap map;

    public MahjongPiece piecePrefab;
    public MahjongPiece currentPiece;

    private void Update()
    {
        if (!currentPiece)
        {
            currentPiece = Instantiate(piecePrefab);
            currentPiece.Hide();
        }

        if (Input.GetMouseButtonDown(0))
        {
            Ray cameraRay = camera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(cameraRay, out RaycastHit hit))
            {
                if (map.TryPlace(hit.point, currentPiece))
                {
                    currentPiece = null;
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
