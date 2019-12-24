using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class GameInput : MonoBehaviour
{
    public static GameInput instance;

    public float minReqDragDist;
    public float minReqHoldTime;

    private enum InputState { Begin, Active, DragBegin, End }
    public enum InputType { Normal, Hold, Drag }

    private List<InputData> inputData = new List<InputData>();
    private List<InputData> removeInputs = new List<InputData>();

    private class InputData
    {
        public InputData(int id, Vector2 startPos)
        {
            this.id = id;
            state = InputState.Begin;
            type = InputType.Normal;
            deltaPos = Vector2.zero;
            this.startPos = startPos;
            time = 0f;
        }

        public int id;
        public InputState state;
        public InputType type;
        public Vector2 deltaPos;
        public Vector2 startPos;
        public float time;
    }

    public bool GetPressDown(out int id)
    {
        for (int i = 0; i < inputData.Count; i++)
        {
            if (inputData[i].state == InputState.Begin)
            {
                id = inputData[i].id;
                return true;
            }
        }
        id = 0;
        return false;
    }

    public bool GetPressUp(out int id, out InputType type)
    {
        for (int i = 0; i < inputData.Count; i++)
        {
            if (inputData[i].state == InputState.End)
            {
                id = inputData[i].id;
                type = inputData[i].type;
                return true;
            }
        }
        id = 0;
        type = InputType.Normal;
        return false;
    }

    public bool GetDragBegin(out int id)
    {
        for (int i = 0; i < inputData.Count; i++)
        {
            if (inputData[i].type == InputType.Drag && inputData[i].state == InputState.DragBegin)
            {
                id = inputData[i].id;
                return true;
            }
        }
        id = 0;
        return false;
    }

    public Vector2 GetPos(int id)
    {
        for (int i = 0; i < inputData.Count; i++)
        {
            if (id == inputData[i].id)
            {
                return inputData[i].startPos + inputData[i].deltaPos;
            }
        }
        return Vector2.zero;
    }

    private void Awake()
    {
        instance = this;
    }

    private void Update()
    {
        for (int i = 0; i < removeInputs.Count; i++)
        {
            inputData.Remove(removeInputs[i]);
        }
        removeInputs.Clear();

        Touch[] touches = Input.touches;
        InputData currentData;

        for (int i = 0; i < touches.Length; i++)
        {
            currentData = FindWithID(touches[i].fingerId);
            switch (touches[i].phase)
            {
                case TouchPhase.Began:
                    inputData.Add(new InputData(touches[i].fingerId, touches[i].position));
                    break;
                case TouchPhase.Moved:
                    currentData.deltaPos += touches[i].deltaPosition;
                    currentData.time += Time.unscaledDeltaTime;
                    currentData.state = InputState.Active;
                    if (currentData.deltaPos.sqrMagnitude >= minReqDragDist * minReqDragDist)
                    {
                        currentData.state = InputState.DragBegin;
                        currentData.type = InputType.Drag;
                    }
                    break;
                case TouchPhase.Stationary:
                    currentData.time += Time.unscaledDeltaTime;
                    currentData.state = InputState.Active;
                    if (currentData.type == InputType.Normal && inputData[touches[i].fingerId].time >= minReqHoldTime)
                    {
                        currentData.type = InputType.Hold;
                    }
                    break;
                case TouchPhase.Ended:
                    currentData.time += Time.unscaledDeltaTime;
                    currentData.state = InputState.End;
                    removeInputs.Add(currentData);
                    break;
                case TouchPhase.Canceled:
                    removeInputs.Add(currentData);
                    break;
                default:
                    break;
            }
        }
    }

    private InputData FindWithID(int id)
    {
        for (int i = 0; i < inputData.Count; i++)
        {
            if (inputData[i].id == id)
                return inputData[i];
        }
        return null;
    }
}
