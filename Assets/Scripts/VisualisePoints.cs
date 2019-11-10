using UnityEngine;
using Unity.Mathematics;
using System.Collections.Generic;

public class VisualisePoints : MonoBehaviour
{
    public bool showSphere = true;
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
            gridSelect.y = WrapIndex(gridSelect.y += 1, plot.yAngles.Length);
            gridSelect.x = VerticalAdjacent(gridSelect.x, previousY, gridSelect.y);
            adjacent = plot.FindAllAdjacent(gridSelect);
        }
        if(Input.GetKeyDown(KeyCode.DownArrow))
        {
            int previousY = gridSelect.y;
            gridSelect.y = WrapIndex(gridSelect.y -= 1, plot.yAngles.Length);
            gridSelect.x = VerticalAdjacent(gridSelect.x, previousY, gridSelect.y);
            adjacent = plot.FindAllAdjacent(gridSelect);
        }
        if(Input.GetKeyDown(KeyCode.LeftArrow))
        {
            gridSelect.x = WrapIndex(gridSelect.x -= 1, plot.xAngles[gridSelect.y].Length);
            adjacent = plot.FindAllAdjacent(gridSelect);
        }
        if(Input.GetKeyDown(KeyCode.RightArrow))
        {
            gridSelect.x = WrapIndex(gridSelect.x += 1, plot.xAngles[gridSelect.y].Length);
            adjacent = plot.FindAllAdjacent(gridSelect);
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
        int length = plot.xAngles[yIndex].Length;
        int otherLength = plot.xAngles[yIndexOther].Length;

        float normalized = math.unlerp(0, length-1, (float)xIndex);
        int interpolated = (int)math.round(math.lerp(0, otherLength-1, normalized));

        return interpolated;
    } 

    float WrapBounds(float bound)
    {
        if(bound > 1)
            return bound - 1f;
        if(bound < 0)
            return bound + 1f;
        return bound;
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
        for(int t = 0; t < plot.positions.Length; t++)
        {
            for(int p = 0; p < plot.positions[t].Length; p++)
            {
                DrawLine(plot.positions[t][p]);
            }
        }
    }

    void DrawGrid()
    {
        float rowHeight = gridSize / plot.yAngles.Length;
        for(int i = 0; i < plot.yAngles.Length; i++)
        {
            DrawGridRow(rowHeight, i);
        }
    }

    void DrawGridRow(float rowHeight, int yIndex)
    {
        float sizeInGrid = gridSize / plot.xAngles[yIndex].Length;
        for(int i = 0; i < plot.xAngles[yIndex].Length; i++)
        {
            float3 start = new float3(sizeInGrid * i, 0, rowHeight * yIndex);
            float3 offset = new float3(sizeInGrid, 0, 0);
            float3 vert = new float3(0, 0, rowHeight/2);

            float3 pos = plot.positions[yIndex][i];
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

    void DrawLine(float3 position)
    {
        Debug.DrawLine(
            float3.zero + (position*0.8f),
            position,
            new Color(position.x, position.y, position.z));
    }
}
