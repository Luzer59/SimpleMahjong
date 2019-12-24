using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PCTester : MonoBehaviour
{
    new public Camera camera;
    public MahjongPieceMap map;

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray cameraRay = camera.ScreenPointToRay(Input.mousePosition);
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
    }

    /*public int pressID = -1;
    public bool pressed = false;
    public GameInput.InputType type = GameInput.InputType.Normal;

    private void Update()
    {
        int id;
        if (!pressed && GameInput.instance.GetPressDown(out id))
        {
            pressID = id;
            pressed = true;
        }
        else if (pressed && GameInput.instance.GetPressUp(out id, out GameInput.InputType type))
        {
            if (id == pressID)
            {
                this.type = type;
                pressID = -1;
                pressed = false;
            }
        }
    }*/
}
