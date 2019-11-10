using UnityEngine;
using Unity.Mathematics;


public struct PlotPoints
{
    public float3[][] positions;
    public float[][] xAngles;
    public float[] zAngles;

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
        float zIncrement = Increment(radius);
        int pointCount = (int)(math.PI / zIncrement);

        positions = new float3[pointCount][];
        xAngles = new float[pointCount][];
        zAngles = new float[pointCount];

        for(int i = 0; i < pointCount; i++)
        {
            float zAngle = AngleInRadians(zIncrement, i);

            PlotRingPoints(zAngle, i);
        }
    }

    void PlotRingPoints(float zAngle, int zIndex)
    {   
        float xIncrement = Increment(radius * math.sin(zAngle));
        int pointCount = (int)(math.PI*2 / xIncrement);

        positions[zIndex] = new float3[pointCount];
        xAngles[zIndex] = new float[pointCount];
        zAngles[zIndex] = zAngle;

        for(int i = 0; i < pointCount; i++)
        {
            float xAngle = AngleInRadians(xIncrement, i);
            xAngles[zIndex][i] = xAngle;

            float3 position = PositionOnSphere(zAngle, xAngle);
            positions[zIndex][i] = position;
        }
    }

    float Increment(float circleRadius)
    {
        return (pointDistance / circleRadius) / math.PI;
    }

    float AngleInRadians(float increment, int count)
    {
        return increment * count + (increment * 0.5f);
    }

    float3 PositionOnSphere(float zAngle, float xAngle)
    {
        float x = radius * math.sin(zAngle) * math.cos(xAngle);
        float y = radius * math.cos(zAngle);
        float z = radius * math.sin(zAngle) * math.sin(xAngle);

        return new float3(x, y, z);
    }
}
