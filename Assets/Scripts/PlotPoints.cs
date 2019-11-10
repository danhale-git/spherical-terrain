using UnityEngine;
using Unity.Mathematics;


public struct PlotPoints
{
    public float3[][] positions;
    public float[][] phis;
    public float[] thetas;

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
        float thetaIncrement = Increment(radius);
        int pointCount = (int)(math.PI / thetaIncrement);

        positions = new float3[pointCount][];
        phis = new float[pointCount][];
        thetas = new float[pointCount];

        for(int i = 0; i < pointCount; i++)
        {
            float theta = AngleInRadians(thetaIncrement, i);

            PlotRingPoints(theta, i);
        }
    }

    void PlotRingPoints(float theta, int thetaIndex)
    {   
        float phiIncrement = Increment(radius * math.sin(theta));
        int pointCount = (int)(math.PI*2 / phiIncrement);

        positions[thetaIndex] = new float3[pointCount];
        phis[thetaIndex] = new float[pointCount];
        thetas[thetaIndex] = theta;

        for(int i = 0; i < pointCount; i++)
        {
            float phi = AngleInRadians(phiIncrement, i);
            phis[thetaIndex][i] = phi;

            float3 position = PositionOnSphere(theta, phi);
            positions[thetaIndex][i] = position;
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

    float3 PositionOnSphere(float theta, float phi)
    {
        float x = radius * math.sin(theta) * math.cos(phi);
        float y = radius * math.cos(theta);
        float z = radius * math.sin(theta) * math.sin(phi);

        return new float3(x, y, z);
    }
}
