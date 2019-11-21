using UnityEngine;
using Unity.Mathematics;
using System.Collections.Generic;
using UnityEditor;
using UnityVoronoi;
using Unity.Collections;



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
    BowyerWatson<PlotPoints.Point> bowyerWatson;
    VoronoiCellGenerator<PlotPoints.Point> voronoi;

    int2 gridSelect = new int2(0, 0);
    List<int2> adjacentIndices = new List<int2>();

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
            ArrowKeyInput();
        }
        if(Input.GetKeyDown(KeyCode.DownArrow))
        {
            int previousY = gridSelect.y;
            gridSelect.y = WrapIndex(gridSelect.y -= 1, plot.radianOffset.Length);
            gridSelect.x = VerticalAdjacent(gridSelect.x, previousY, gridSelect.y);
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
        adjacentIndices.Clear();
        var adjacent = plot.FindAllAdjacent(gridSelect);
        adjacentIndices.AddRange(adjacent.ToArray());
        adjacent.Dispose();

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

    void Start()
    {
        plot.PlotSphere(radius, pointDistance, jitter);

        DrawAllCells();
    }

    void Update()
    {
        InputValues();

        plot.PlotSphere(radius, pointDistance, jitter);

        if(showSphere)
            DrawPointsInSphere();

        if(showGrid)
            DrawGrid();
    }

    void DrawAllCells()
    {
        if(!plot.plotted)
            return;

        for(int r = 1; r < plot.radianOffset.Length-1; r++)
        {
            for(int p = 1; p < plot.radianOffset[r].Length-1; p++)
            {
                int2 index = new int2(p, r);
                AddVoronoiCell(index);
            }
        }
    }

    void AddVoronoiCell(int2 index)
    {
        NativeList<int2> adjacent = plot.FindAllAdjacent(index);
        PlotPoints.Point centerPos;
        var unwrapped = plot.UnwrapPoints(adjacent, index, out centerPos);

        VoronoiCell cell = voronoi.GetVoronoiVertices(unwrapped, centerPos);
        DrawVoronoiCell(cell);

        unwrapped.Dispose();
        adjacent.Dispose();
    }

    void DrawVoronoiCell(VoronoiCell cell)
    {
        for(int i = 0; i < cell.vertices.Length; i++)
        {
            int next = i < cell.vertices.Length-1 ? i+1 : 0;
            Debug.DrawLine(cell.vertices[i], cell.vertices[next], Color.white, 200);
        }
    }

    void DrawPointsInSphere()
    {
        for(int t = 0; t < plot.worldOffset.Length; t++)
        {
            for(int p = 0; p < plot.worldOffset[t].Length; p++)
            {
                float3 position = plot.worldOffset[t][p];
                Color color = new Color(position.x, position.y, position.z, 0.3f);

                Debug.DrawLine(
                    float3.zero + (position*0.95f),
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
            else if(adjacentIndices.Contains(new int2(i, yIndex)))
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
