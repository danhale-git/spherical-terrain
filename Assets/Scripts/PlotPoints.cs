using UnityEngine;
using Unity.Mathematics;
using Unity.Collections;
using UnityVoronoi;
using System.Collections.Generic;

public struct PlotPoints
{
    public bool plotted;

    public float3[][] worldOffset;
    public float2[][] radianOffset;

    float radius;
    float pointDistance;
    float jitter;

    Unity.Mathematics.Random random;

    public struct Point : IBowyerWatsonPoint
    {
        float3 worldPosition;
        float3 triangulatePosition;
        
        public Point(float3 worldPosition, float3 triangulatePosition)
        {
            this.worldPosition = worldPosition;
            this.triangulatePosition = triangulatePosition;
        }
        public float3 GetBowyerWatsonPoint()
        {
            return triangulatePosition;
        }
        public float3 GetWorldPoint()
        {
            return worldPosition;
        }
    } 

    public void PlotSphere(float radius, float pointDistance, float jitter)
    {
        this.radius = radius;
        this.pointDistance = pointDistance;
        this.jitter = jitter;
        random = new Unity.Mathematics.Random(1234);

        PlotHorizontalRings();

        plotted = true;
    }

    public float3 GetPosition(int2 index)
    {
        return worldOffset[index.y][index.x];
    }

    void PlotHorizontalRings()
    {
        float yIncrement = (pointDistance / radius) / math.PI;
        int pointCount = (int)(math.PI / yIncrement);

        worldOffset = new float3[pointCount][];
        radianOffset = new float2[pointCount][];

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

        worldOffset[yIndex] = new float3[pointCount];
        radianOffset[yIndex] = new float2[pointCount];

        for(int i = 0; i < pointCount; i++)
        {
            float xAngle = AngleInRadians(xIncrement, i);
            float2 offset = new float2(xAngle, yAngle);
            offset += random.NextFloat2(0, jitter);
            radianOffset[yIndex][i] = offset;

            float3 position = PositionOnSphere(offset);
            worldOffset[yIndex][i] = position;
        }
    }

    float AngleInRadians(float increment, int count)
    {
        return increment * count + (increment * 0.5f);
    }

    float3 PositionOnSphere(float2 radians)
    {
        float x = radius * math.sin(radians.y) * math.cos(radians.x);
        float y = radius * math.cos(radians.y);
        float z = radius * math.sin(radians.y) * math.sin(radians.x);

        return new float3(x, y, z);
    }

    public NativeList<int2> FindAllAdjacent(int2 index)
    {
        NativeList<int2> adjacent = new NativeList<int2>(Allocator.Persistent);
        adjacent.Add(WrapXIndex(index + new int2(1, 0)));
        adjacent.Add(WrapXIndex(index + new int2(-1, 0)));
        NativeList<int2> above = FindAdjacentVertical(index, +1);
        adjacent.AddRange(above);
        above.Dispose();
        NativeList<int2> below = FindAdjacentVertical(index, -1);
        adjacent.AddRange(below);
        below.Dispose();

        return adjacent;
    }

    NativeList<int2> FindAdjacentVertical(int2 index, int yOffset)
    {
        int2 startIndex = WrapYIndex(new int2(0, index.y+yOffset));
        startIndex.x = VerticalNeighbour(index.x, index.y, startIndex.y);
        startIndex = WrapXIndex(startIndex);

        float2 bounds = GetBounds(index);
        float2 otherBounds = GetBounds(startIndex);

        NativeList<int2> adjacent = new NativeList<int2>(Allocator.Temp);
        adjacent.Add(startIndex);

        int2 leftCursor = startIndex;
        int2 rightCursor = startIndex;

        while(true)
        {
            leftCursor.x -= 1;
            bool left = InBounds(leftCursor, index);
            adjacent.Add(WrapXIndex(leftCursor));
            if(!left)
                break;

            //adjacent.Add(WrapXIndex(leftCursor));
        }

        while(true)
        {
            rightCursor.x += 1;
            bool right = InBounds(rightCursor, index);
            adjacent.Add(WrapXIndex(rightCursor));
            if(!right)
                break;

            //adjacent.Add(WrapXIndex(rightCursor));
        }

        return adjacent;
    }

    int2 WrapYIndex(int2 index)
    {
        int length = radianOffset.Length;
        if(index.y > length-1)
            return new int2(index.x, index.y - length);
        if(index.y < 0)
            return new int2(index.x, index.y + length);
        return index;
    }

    int2 WrapXIndex(int2 index)
    {
        int length = radianOffset[index.y].Length;
        if(index.x > length-1)
            return new int2(index.x - length, index.y);
        if(index.x < 0)
            return new int2(index.x + length, index.y);
        return index;
    }

    bool InBounds(int2 checkIndex, int2 againstIndex)
    {
        float2 check = GetBounds(checkIndex);
        float checkPoint = GetPoint(checkIndex);
        float2 against = GetBounds(againstIndex);


        bool left =     check.x >= against.x && check.x <= against.y;
        bool right =    check.y >= against.x && check.y <= against.y;
        bool overlap =  check.x <= against.x && check.y >= against.y;

        bool point = checkPoint >= against.x && checkPoint <= against.y;
        
        return (left || right || overlap || point);
    }

    float2 GetBounds(int2 index)
    {
        int length = radianOffset[index.y].Length;

        float increment = math.unlerp(0, length, 1);
        float position = math.unlerp(0, length, index.x);

        return new float2(position, position + increment);
    }

    float GetPoint(int2 index)
    {
        int length = radianOffset[index.y].Length;

        float increment = math.unlerp(0, length, 1);
        float position = math.unlerp(0, length, index.x);

        return position + (increment*0.5f);
    }

    int VerticalNeighbour(int xIndex, int yIndex, int yIndexOther)
    {
        int length = radianOffset[yIndex].Length;
        int otherLength = radianOffset[yIndexOther].Length;

        float normalized = math.unlerp(0, length-1, (float)xIndex);
        int interpolated = (int)math.round(math.lerp(0, otherLength-1, normalized));

        return interpolated;
    }

    public NativeArray<Point> UnwrapPoints(NativeArray<int2> adjacent, int2 center, out Point centerPos)
    {
        var pointsToTriangulate = new NativeList<Point>(Allocator.Temp);

        float2 centerRadians = radianOffset[center.y][center.x];
        float3 center3 = new float3(centerRadians.x, 0, centerRadians.y);
        float3 centerWorld = worldOffset[center.y][center.x];
        
        centerPos = new Point(centerWorld, center3);
        pointsToTriangulate.Add(centerPos);

        for(int i = 0; i < adjacent.Length; i++)
        {
            float2 radians = radianOffset[adjacent[i].y][adjacent[i].x];
            float3 radians3 = new float3(radians.x, 0, radians.y);
            float3 world = worldOffset[adjacent[i].y][adjacent[i].x];

            pointsToTriangulate.Add(new Point(world, radians3));
        }

        NativeArray<Point> triangulateArray = new NativeArray<Point>(pointsToTriangulate.Length, Allocator.Persistent);
        for(int i = 0; i < triangulateArray.Length; i++)
            triangulateArray[i] = pointsToTriangulate[i];

        pointsToTriangulate.Dispose();

        return triangulateArray;
    }
}
