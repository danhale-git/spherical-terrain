using UnityEngine;
using Unity.Mathematics;

public class Sphere : MonoBehaviour
{
    public float radius = 5;
    public float gridSize = 5;

    const float radians = 6.283f;

    float[][] phis;
    float[] thetas;

    void Update()
    {
        gridSize = math.clamp(gridSize, 2, 100);
        radius = math.clamp(radius, 0, 15);

        PlotHorizontalRings();
        DrawPoints();
    }   

    void PlotHorizontalRings()
    {
        float thetaIncrement = (gridSize / radius) / math.PI;
        int pointCount = (int)(math.PI / thetaIncrement);

        phis = new float[pointCount][];
        thetas = new float[pointCount];

        for(int i = 0; i < pointCount; i++)
        {
            float theta = thetaIncrement * i + (thetaIncrement * 0.5f);

            PlotRingPoints(theta, i);
        }
    }

    void PlotRingPoints(float theta, int thetaIndex)
    {   
        float ringRadius = radius * math.sin(theta);
        float phiIncrement = (gridSize / ringRadius) / math.PI;
        int pointCount = (int)(math.PI*2 / phiIncrement);

        phis[thetaIndex] = new float[pointCount];
        thetas[thetaIndex] = theta;

        for(int i = 0; i < pointCount; i++)
        {
            float phi = phiIncrement * i + (phiIncrement * 0.5f);

            phis[thetaIndex][i] = phi;
        }
    }

    void DrawPoints()
    {
        for(int t = 0; t < thetas.Length; t++)
            for(int p = 0; p < phis[t].Length; p++)
            {
                DrawLine(thetas[t], phis[t][p]);
            }
    }

    void DrawLine(float theta, float phi)
    {
        float3 position = PositionOnSphere(theta, phi);
        Debug.DrawLine(
            float3.zero + (position*0.8f),
            position, new Color(position.x,
            position.y, position.z));
    }

    float3 PositionOnSphere(float theta, float phi)
    {
        float x = radius * math.sin(theta) * math.cos(phi);
        float y = radius * math.cos(theta);
        float z = radius * math.sin(theta) * math.sin(phi);

        return new float3(x, y, z);
    }

    public int Flatten2D(int x, int z, int size)
    {
        return (z * size) + x;
    }

    public int2 Unflatten2D(int index, int size)
    {
        int x = index % size;
        int z = index / size;

        return new int2(x, z);
    }
}
