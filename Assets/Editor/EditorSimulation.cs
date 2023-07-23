using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(GPUSimulation))]
public class MapGeneratoreditor : Editor
{
    public override void OnInspectorGUI()
    {

        GPUSimulation sim = (GPUSimulation)target;


        DrawDefaultInspector();

        if (GUILayout.Button("Generate"))
        {
            sim.RunSim();
        }
        if (GUILayout.Button("Iterate"))
        {
            sim.Iterate();
        }
    }
}