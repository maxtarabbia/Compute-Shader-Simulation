using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseControls : MonoBehaviour
{
    GPUSimulation GPUS;



    Vector2 mouseOffset;
    Vector2 LastmousePos;
    bool isHolding;

    public float mouseMovementSensitivity;
    public float mouseScrollSensitivity;

    void Start()
    {
        GPUS = GetComponent<GPUSimulation>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            GPUS.Exit();
        if (Input.GetKeyDown(KeyCode.D))
        {
            GPUS.DebugMode = !GPUS.DebugMode;
            GPUS.RunSim();
        }
        CheckMousePos();
        MoveNoise();
        ScaleNoise();
    }
    public void ScaleNoise()
    {
        GPUS.scale += Input.mouseScrollDelta.y * mouseScrollSensitivity;
    }
    public void MoveNoise()
    {
        if (Input.GetMouseButton(0))
        {
            if (!isHolding)
            {
                isHolding = true;
            }
            else
            {
                mouseOffset += (LastmousePos - (Vector2)Input.mousePosition) * mouseMovementSensitivity * 0.01f;
                GPUS.NoiseOffset = mouseOffset;
            }

        }
        else
        {
            isHolding = false;
        }
        LastmousePos = Input.mousePosition;
    }
    public void CheckMousePos()
    {
        if(screenToField(Input.mousePosition).x < GPUS.resolution && screenToField(Input.mousePosition).x > 0)
            addpointsBetween(screenToField(Input.mousePosition), screenToField(LastmousePos),5);
    }
    public void addpointsBetween(Vector2 point1, Vector2 point2, int count)
    {

        float col = (Time.time / 5) % 1;
        GPUS.ForceAddPoint(new Vector2Int((int)point1.x, (int)point1.y), col);
        Vector2 coord;
        for(int i = 0; i < count; i++)
        { 
            coord = Vector2.Lerp(point1, point2, (float)i/((float)count+1));
            GPUS.ForceAddPoint(new Vector2Int((int)coord.x,(int)coord.y), col);
        }
    }
    public Vector2 screenToField(Vector2 point)
    {
        Vector2 mouse = new(point.x - (Screen.width / 2) + (Screen.height / 2), point.y);
        Vector2 RelativePos = mouse / Screen.height * GPUS.resolution;
        return RelativePos;
    }
}
