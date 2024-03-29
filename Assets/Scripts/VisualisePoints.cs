﻿using UnityEngine;
using Unity.Mathematics;
using System.Collections.Generic;
using UnityEditor;
using UnityVoronoi;
using Unity.Collections;

public struct PositionWrapper : IBowyerWatsonPoint
{
    public PositionWrapper(float3 worldPosition, float3 triangulatePosition)
    {
        this.worldPosition = worldPosition;
        this.triangulatePosition = triangulatePosition;
    }
    public float3 worldPosition;
    float3 triangulatePosition;
    public float3 GetBowyerWatsonPoint()
    {
        return triangulatePosition;
    }
} 

public class VisualisePoints : MonoBehaviour
{
    public bool showSphere = true;
    public bool cameraTrackCursorOnSphere = false;
    public bool showGrid = true;

    public float radius = 5;
    public float pointDistance = 5;
    public float jitter = 0;
    
    float gridSize;

    const float radians = 6.283f;

    PlotPoints plot;
    BowyerWatson<PositionWrapper> bowyerWatson;

    int2 gridSelect = new int2(0, 0);
    List<int2> adjacent = new List<int2>();

    void InputValues()
    {
        pointDistance = math.clamp(pointDistance, 2, 100);
        radius = math.clamp(radius, 0, 15);
        jitter = math.clamp(jitter, 0, 15);

        gridSize = radius * 2;

        if(Input.GetKeyDown(KeyCode.UpArrow))
        {
            int previousY = gridSelect.y;
            gridSelect.y = WrapIndex(gridSelect.y += 1, plot.radianOffset.Length);
            gridSelect.x = VerticalAdjacent(gridSelect.x, previousY, gridSelect.y);
            adjacent = plot.FindAllAdjacent(gridSelect);
            ArrowKeyInput();
        }
        if(Input.GetKeyDown(KeyCode.DownArrow))
        {
            int previousY = gridSelect.y;
            gridSelect.y = WrapIndex(gridSelect.y -= 1, plot.radianOffset.Length);
            gridSelect.x = VerticalAdjacent(gridSelect.x, previousY, gridSelect.y);
            adjacent = plot.FindAllAdjacent(gridSelect);
            ArrowKeyInput();
        }
        if(Input.GetKeyDown(KeyCode.LeftArrow))
        {
            gridSelect.x = WrapIndex(gridSelect.x -= 1, plot.radianOffset[gridSelect.y].Length);
            ArrowKeyInput();
        }
        if(Input.GetKeyDown(KeyCode.RightArrow))
        {
            gridSelect.x = WrapIndex(gridSelect.x += 1, plot.radianOffset[gridSelect.y].Length);
            ArrowKeyInput();
        }

    }

    void ArrowKeyInput()
    {
        adjacent = plot.FindAllAdjacent(gridSelect);

        if(cameraTrackCursorOnSphere)
            MoveCameraWithCursor();
    }

    void MoveCameraWithCursor()
    {
        Camera camera = SceneView.lastActiveSceneView.camera;
        camera.transform.position = -(plot.GetPosition(gridSelect) * radius) + new float3(0, 0, 1);
        camera.transform.LookAt(float3.zero);

        SceneView.lastActiveSceneView.AlignViewToObject(camera.transform);
        SceneView.lastActiveSceneView.Repaint();
    }

    void Update()
    {
        InputValues();

        plot.PlotSphere(radius, pointDistance, jitter);

        if(showSphere)
            DrawPointsInSphere();

        if(showGrid)
            DrawGrid();

        PointTesselation();
    }

    void PointTesselation()
    {
        int scale = 7;
        var pointsToTriangulate = new List<PositionWrapper>();
        float3 up = new float3(0, 0, 0.5f);
        float3 right = new float3(0.5f, 0, 0);

        int centerRingLength = plot.radianOffset[gridSelect.y].Length;
        float2 center = plot.radianOffset[gridSelect.y][gridSelect.x];
        float3 center3 = new float3(center.x, 0, center.y) * scale;
        float3 centerWorld = plot.worldOffset[gridSelect.y][gridSelect.x];
        
        pointsToTriangulate.Add(new PositionWrapper(centerWorld, center3));

        //Debug.DrawLine(center3 + up, center3 - up, Color.green);
        //Debug.DrawLine(center3 + right, center3 - right, Color.green);
        for(int i = 0; i < adjacent.Count; i++)
        {
            float2 radians = plot.radianOffset[adjacent[i].y][adjacent[i].x];
            float3 radians3 = new float3(radians.x, 0, radians.y) * scale;
            float3 world = plot.worldOffset[adjacent[i].y][adjacent[i].x];
            int ringLength = plot.radianOffset[adjacent[i].y].Length;

            //float2 normalised = math.unlerp(0, ringLength, radians);
            //float2 interpolated = radians;// = math.lerp(0, centerRingLength, normalised);
            //float3 interpolated3 = new float3(interpolated.x, 0, interpolated.y) * scale;
            //pointsToTriangulate.Add(new PositionWrapper(interpolated3));

            pointsToTriangulate.Add(new PositionWrapper(world, radians3));
            
            //Debug.DrawLine(radians3 + up, radians3 - up, Color.red);
            //Debug.DrawLine(radians3 + right, radians3 - right, Color.red);
        }

        NativeArray<PositionWrapper> triangulateArray = new NativeArray<PositionWrapper>(pointsToTriangulate.Count, Allocator.Persistent);
        for(int i = 0; i < triangulateArray.Length; i++)
            triangulateArray[i] = pointsToTriangulate[i];

        var triangles = bowyerWatson.Triangulate(triangulateArray);
        for(int i = 0; i < triangles.Length; i++)
            DrawTriangle(triangles[i], Color.white);

        triangulateArray.Dispose();
        triangles.Dispose();
    }

    void DrawTriangle(BowyerWatson<PositionWrapper>.Triangle triangle, Color color)
    {
        //Debug.DrawLine(triangle.a.pos, triangle.b.pos, color);
        //Debug.DrawLine(triangle.b.pos, triangle.c.pos, color);
        //Debug.DrawLine(triangle.c.pos, triangle.a.pos, color);

        Debug.DrawLine(triangle.a.pointObject.worldPosition, triangle.b.pointObject.worldPosition, color);
        Debug.DrawLine(triangle.b.pointObject.worldPosition, triangle.c.pointObject.worldPosition, color);
        Debug.DrawLine(triangle.c.pointObject.worldPosition, triangle.a.pointObject.worldPosition, color);
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
                else if(gridSelect.Equals(new int2(p, t)))
                {
                    color = Color.white;
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
}
