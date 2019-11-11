﻿using UnityEngine;
using Unity.Mathematics;
using System.Collections.Generic;
using UnityEditor;

public class VisualisePoints : MonoBehaviour
{
    public bool showSphere = true;
    public bool cameraTrackCursorOnSphere = false;
    public float radius = 5;
    public float pointDistance = 5;
    
    public bool showGrid = true;
    float gridSize;

    const float radians = 6.283f;

    PlotPoints plot;

    int2 gridSelect = new int2(0, 0);
    List<int2> adjacent = new List<int2>();

    void InputValues()
    {
        pointDistance = math.clamp(pointDistance, 2, 100);
        radius = math.clamp(radius, 0, 15);

        gridSize = radius * 2;

        if(Input.GetKeyDown(KeyCode.UpArrow))
        {
            int previousY = gridSelect.y;
            gridSelect.y = WrapIndex(gridSelect.y += 1, plot.radianOffset.Length);
            gridSelect.x = VerticalAdjacent(gridSelect.x, previousY, gridSelect.y);
            adjacent = plot.FindAllAdjacent(gridSelect);

            if(cameraTrackCursorOnSphere)
                MoveCameraWithCursor();
        }
        if(Input.GetKeyDown(KeyCode.DownArrow))
        {
            int previousY = gridSelect.y;
            gridSelect.y = WrapIndex(gridSelect.y -= 1, plot.radianOffset.Length);
            gridSelect.x = VerticalAdjacent(gridSelect.x, previousY, gridSelect.y);
            adjacent = plot.FindAllAdjacent(gridSelect);

            if(cameraTrackCursorOnSphere)
                MoveCameraWithCursor();
        }
        if(Input.GetKeyDown(KeyCode.LeftArrow))
        {
            gridSelect.x = WrapIndex(gridSelect.x -= 1, plot.radianOffset[gridSelect.y].Length);
            adjacent = plot.FindAllAdjacent(gridSelect);

            if(cameraTrackCursorOnSphere)
                MoveCameraWithCursor();
        }
        if(Input.GetKeyDown(KeyCode.RightArrow))
        {
            gridSelect.x = WrapIndex(gridSelect.x += 1, plot.radianOffset[gridSelect.y].Length);
            adjacent = plot.FindAllAdjacent(gridSelect);

            if(cameraTrackCursorOnSphere)
                MoveCameraWithCursor();
        }

    }

    void MoveCameraWithCursor()
    {
        Camera camera = SceneView.lastActiveSceneView.camera;
        camera.transform.position = -(plot.GetPosition(gridSelect) * radius);
        camera.transform.LookAt(float3.zero);

        SceneView.lastActiveSceneView.AlignViewToObject(camera.transform);
        SceneView.lastActiveSceneView.Repaint();
    }

    int WrapIndex(int index, int length)
    {
        int lastIndex = length-1;
        if(index > lastIndex)
            return index - length;
        if(index < 0)
            return index + length;
        return index;
    }
    int VerticalAdjacent(int xIndex, int yIndex, int yIndexOther)
    {
        int length = plot.radianOffset[yIndex].Length;
        int otherLength = plot.radianOffset[yIndexOther].Length;

        float normalized = math.unlerp(0, length-1, (float)xIndex);
        int interpolated = (int)math.round(math.lerp(0, otherLength-1, normalized));

        return interpolated;
    }

    void Update()
    {
        InputValues();

        plot.PlotSphere(radius, pointDistance);

        if(showSphere)
            DrawPointsInSphere();

        if(showGrid)
            DrawGrid();
    }   

    void DrawPointsInSphere()
    {
        for(int t = 0; t < plot.worldOffset.Length; t++)
        {
            for(int p = 0; p < plot.worldOffset[t].Length; p++)
            {
                float3 position = plot.worldOffset[t][p];
                Color color = new Color(position.x, position.y, position.z, 0.3f);

                if(adjacent.Contains(new int2(p, t)))
                {
                    color = new Color(color.r, color.g, color.b) * 1.25f;
                }

                Debug.DrawLine(
                    float3.zero + (position*0.8f),
                    position,
                    color);
            }
        }
    }

    void DrawGrid()
    {
        float3 zOffset = new float3(0, 0, radius);        
        float rowHeight = gridSize / plot.radianOffset.Length;
        for(int i = 0; i < plot.radianOffset.Length; i++)
        {
            DrawGridRow(rowHeight, i, zOffset);
        }
    }

    void DrawGridRow(float rowHeight, int yIndex, float3 zOffset)
    {
        float sizeInGrid = gridSize / plot.radianOffset[yIndex].Length;
        for(int i = 0; i < plot.radianOffset[yIndex].Length; i++)
        {
            float3 start = new float3(sizeInGrid * i, 0, rowHeight * yIndex) + zOffset;
            float3 offset = new float3(sizeInGrid, 0, 0);
            float3 vert = new float3(0, 0, rowHeight/2);

            float3 pos = plot.worldOffset[yIndex][i];
            Color color = new Color(pos.x, pos.y, pos.z);
            
            if(yIndex == gridSelect.y && i == gridSelect.x)
                color = new Color(1-color.r, 1-color.g, 1-color.b);
            else if(adjacent.Contains(new int2(i, yIndex)))
                color = new Color(1-color.r, 1-color.g, 1-color.b) * 2;
                
            Debug.DrawLine(start + vert, start - vert, color);
            Debug.DrawLine(start+offset + vert, start+offset - vert, color);
            Debug.DrawLine(start + vert, start+offset - vert, color);
            Debug.DrawLine(start - vert, start+offset + vert, color);
        }
    }
}
