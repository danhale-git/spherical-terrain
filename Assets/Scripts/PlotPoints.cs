using UnityEngine;
using Unity.Mathematics;
using System.Collections.Generic;


public struct PlotPoints
{
    public float3[][] positions;
    public float[][] xAngles;
    public float[] yAngles;

    float radius;
    float pointDistance;

    public void PlotSphere(float radius, float pointDistance)
    {
        this.radius = radius;
        this.pointDistance = pointDistance;
        PlotHorizontalRings();
    }

    void PlotHorizontalRings()
    {
        float yIncrement = (pointDistance / radius) / math.PI;
        int pointCount = (int)(math.PI / yIncrement);

        positions = new float3[pointCount][];
        xAngles = new float[pointCount][];
        yAngles = new float[pointCount];

        for(int i = 0; i < pointCount; i++)
        {
            float yAngle = AngleInRadians(yIncrement, i);

            PlotRingPoints(yAngle, i);
        }
    }

    void PlotRingPoints(float yAngle, int yIndex)
    {   
        float ringRadius = radius * math.sin(yAngle);
        float xIncrement = ( pointDistance / ringRadius ) / math.PI;
        int pointCount = (int)(math.PI*2 / xIncrement);

        positions[yIndex] = new float3[pointCount];
        xAngles[yIndex] = new float[pointCount];
        yAngles[yIndex] = yAngle;

        for(int i = 0; i < pointCount; i++)
        {
            float xAngle = AngleInRadians(xIncrement, i);
            xAngles[yIndex][i] = xAngle;

            float3 position = PositionOnSphere(yAngle, xAngle);
            positions[yIndex][i] = position;
        }
    }

    float AngleInRadians(float increment, int count)
    {
        return increment * count + (increment * 0.5f);
    }

    float3 PositionOnSphere(float yAngle, float xAngle)
    {
        float x = radius * math.sin(yAngle) * math.cos(xAngle);
        float y = radius * math.cos(yAngle);
        float z = radius * math.sin(yAngle) * math.sin(xAngle);

        return new float3(x, y, z);
    }

    public List<int2> FindAllAdjacent(int2 index)
    {
        List<int2> adjacent = new List<int2>();
        adjacent.Add(WrapXIndex(index + new int2(1, 0)));
        adjacent.Add(WrapXIndex(index + new int2(-1, 0)));
        adjacent.AddRange(FindAdjacentVertical(index, +1));
        adjacent.AddRange(FindAdjacentVertical(index, -1));

        return adjacent;
    }

    List<int2> FindAdjacentVertical(int2 index, int yOffset)
    {
        int2 otherIndex = WrapYIndex(new int2(0, index.y+yOffset));
        otherIndex.x = VerticalNeighbour(index.x, index.y, otherIndex.y);
        otherIndex = WrapXIndex(otherIndex);

        int xLength = xAngles[index.y].Length;
        int xLengthOther = xAngles[otherIndex.y].Length;

        float2 bounds = GetBounds(index.x, xLength);
        float2 otherBounds = GetBounds(otherIndex.x, xLengthOther);

        bool2 inBounds = InBounds(otherBounds, bounds);
        bool left = inBounds.x;
        bool right = inBounds.y;

        List<int2> adjacent = new List<int2>();
        adjacent.Add(otherIndex);

        int2 cursor = otherIndex;

        while(left)
        {
            cursor.x -= 1;
            adjacent.Add(WrapXIndex(cursor));

            float2 cursorBounds = GetBounds(cursor.x, xLengthOther);
            bool2 cursorIn = InBounds(cursorBounds, bounds);
            left = cursorIn.x;
        }

        cursor = otherIndex;

        while(right)
        {
            cursor.x += 1;
            adjacent.Add( WrapXIndex(cursor) );

            float2 cursorBounds = GetBounds(cursor.x, xLengthOther);
            bool2 cursorIn = InBounds(cursorBounds, bounds);
            right = cursorIn.y;
        }

        return adjacent;
    }

    int2 WrapYIndex(int2 index)
    {
        int length = yAngles.Length;
        if(index.y > length-1)
            return new int2(index.x, index.y - length);
        if(index.y < 0)
            return new int2(index.x, index.y + length);
        return index;
    }

    int2 WrapXIndex(int2 index)
    {
        int length = xAngles[index.y].Length;
        if(index.x > length-1)
            return new int2(index.x - length, index.y);
        if(index.x < 0)
            return new int2(index.x + length, index.y);
        return index;
    }

    bool2 InBounds(float2 check, float2 against)
    {
        bool xResult = (check.x >= against.x && check.x <= against.y);
        bool yResult = (check.y >= against.x && check.y <= against.y);

        return new bool2(xResult, yResult);
    }

    float2 GetBounds(int index, int length)
    {
        float increment = math.unlerp(0, length, 1);
        float position = math.unlerp(0, length, index);

        return new float2(position, position + increment);
    }

    int VerticalNeighbour(int xIndex, int yIndex, int yIndexOther)
    {
        Debug.Log(xAngles.Length+" "+yIndexOther);
        int length = xAngles[yIndex].Length;
        int otherLength = xAngles[yIndexOther].Length;

        float normalized = math.unlerp(0, length-1, (float)xIndex);
        int interpolated = (int)math.round(math.lerp(0, otherLength-1, normalized));

        return interpolated;
    } 
}
