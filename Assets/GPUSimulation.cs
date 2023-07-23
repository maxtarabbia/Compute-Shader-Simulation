using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Profiling;
using ImageExporting;
using System.Linq;

public class GPUSimulation : MonoBehaviour
{
    public ComputeShader computeShader;
    public int resolution;
    public float additivePixelPercent = 0.1f;

    public Cell[] data;

    public Texture2D texture;
    Sprite sprite;
    Color[] colors;

    public Gradient gradient;

    public float scale;

    public string outputPath;

    public bool outputFrames;

    public Vector2 NoiseOffset;
    public bool DebugMode;

    // Start is called before the first frame update
    void Start()
    {
        //Application.targetFrameRate = 60;
        RunSim();

    }

    // Update is called once per frame
    void Update()
    {

        Iterate();
    }

    public void RunSim()
    {
        //gameObject.transform.localScale = Vector3.one * resolution / 1000;
        texture = null;
        sprite = null;
        colors = null;
        InitializeData();
        //RunGPU();
        AssignTexture();
    }
    public void Iterate()
    {
        addPointsNew(additivePixelPercent/(float)100);
        RunGPU();
        AssignTexture();

        if (outputFrames)
        {
            Profiler.BeginSample("Write Output");
            ImageIO.WriteTextureToDisk(texture, outputPath, "_" + resolution + "p_Scale_6");
            Profiler.EndSample();

            if (Time.frameCount > 1000)
            {
                Exit();
            }
        }
    }

    public void Exit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
                    Application.Quit();
#endif
    }

    public void addPoints(float density)
    {
        Profiler.BeginSample("addPoints");
        for (int y = 0; y < resolution; y++)
        {
            for (int x = 0; x < resolution; x++)
            {
                if (UnityEngine.Random.value <= density)
                {
                    //data[x + resolution * y].color = new Vector4(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value, 1) * 3;
                    data[x + resolution * y].color = gradient.Evaluate(Random.value) * 2;
                }
            }
        }
        Profiler.EndSample();
    }
    public void addPointsNew(float density)
    {
        Profiler.BeginSample("addPointsNew");
        float totalPoints = resolution * resolution * density;

        for(int i = 0; i < totalPoints; i++)
        {
            int x = (int)(Random.value * resolution);
            int y = (int)(Random.value * resolution);
             
                ForceAddPoint(new Vector2Int(x, y), Random.value);
        }
        Profiler.EndSample();


    }
    public void ForceAddPoint(Vector2Int coord, float GradientTime)
    {
        Color col = gradient.Evaluate(GradientTime) * 2;
        col.a = 1;

        try
        {
            data[coord.x + resolution * coord.y].color = col;
            data[1 + coord.x + resolution * coord.y].color = col * 0.5f;
            data[-1 + coord.x + resolution * coord.y].color = col * 0.5f;
            data[coord.x + resolution * (coord.y + 1)].color = col * 0.5f;
            data[coord.x + resolution * (coord.y - 1)].color = col * 0.5f;
        }
        catch
        {
            //print("too close to edge");
        }
    }
    public void InitializeData()
    {
        if (resolution <= 0)
            resolution = 1;

        data = new Cell[resolution * resolution];
        for (int y = 0; y < resolution; y++)
        {
            for (int x = 0; x < resolution; x++)
            {
                Vector2 pos = new(x / (float)resolution, y / (float)resolution);
                Cell cell = data[x + resolution * y];
                cell.color = new Color(0, 0, 0, 1);
                cell.position = pos;
                data[x + resolution * y] = cell;
            }
        }
        addPointsNew(additivePixelPercent / (float)100);
    }
    public void RunGPU()
    {
        Profiler.BeginSample("Running Compute Shader");
        int Vec2Size = sizeof(float) * 2;
        int colorSize = sizeof(float) * 4;
        int totalSize = Vec2Size + colorSize;


        ComputeBuffer cellBuffer = new ComputeBuffer(data.Length, totalSize);
        cellBuffer.SetData(data);

        computeShader.SetFloat("scale", scale);
        //print(Time.frameCount);
        computeShader.SetBool("debugMode", DebugMode);
        computeShader.SetVector("noiseOffset", NoiseOffset);
        computeShader.SetFloat("z", -50 + Time.frameCount/2000f);
        computeShader.SetInt("size", resolution);
        computeShader.SetBuffer(0, "cells", cellBuffer);
        computeShader.SetFloat("resolution", data.Length);


        computeShader.Dispatch(0, data.Length / 64 + 1, 1, 1);
        //computeShader.Dispatch(0,data.Length/8 + 1,1,1);

        cellBuffer.GetData(data);

        cellBuffer.Dispose();
        Profiler.EndSample();
    }
    public void AssignTexture()
    {
        
        if (texture == null)
        {
            texture = new Texture2D(resolution, resolution, TextureFormat.RGBA32, false);
            texture.filterMode = FilterMode.Point;
        }
        colors ??= new Color[resolution * resolution];
        Profiler.BeginSample("Generating Texture Array");

        for (int y = 0; y < resolution * resolution; y++)
        {
            colors[y] = data[y].color;
        }
        Profiler.EndSample();
        Profiler.BeginSample("Applying Color Array");
        texture.SetPixels(colors);
        texture.Apply();

        if (sprite == null)
        {
            sprite = Sprite.Create(texture, new Rect(0, 0, resolution, resolution), new Vector2(0.5f, 0.5f),resolution);
            SpriteRenderer SR = GetComponent<SpriteRenderer>();
            SR.sprite = sprite;
        }

        Profiler.EndSample();
    }
    public Color Vec4ToCol(Vector4 v)
    {
        return new Color(v.x, v.y, v.z,v.w);
    }
    public Color32 Vec4ToCol32(Vector4 v)
    {
        return new Color32((byte)v.x, (byte)v.y, (byte)v.z, (byte)v.w);
    }
    public struct Cell
    {
        public Vector2 position;
        public Color color;
    }
}
