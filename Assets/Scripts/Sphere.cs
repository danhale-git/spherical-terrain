using UnityEngine;
using Unity.Mathematics;

public class Sphere : MonoBehaviour
{
    public float radius = 5;
    public float gridSize = 5;

    const float radians = 6.283f;

    float[][] phis;
    float[] thetas;
    
    void ClampInputValues()
    {
        gridSize = math.clamp(gridSize, 2, 100);
        radius = math.clamp(radius, 0, 15);
    }

    void Update()
    {
        ClampInputValues();

        PlotHorizontalRings();

        DrawPointsInSphere();
    }   

    void PlotHorizontalRings()
    {
        float thetaIncrement = (gridSize / radius) / math.PI;
        int pointCount = (int)(math.PI / thetaIncrement);

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
        float ringRadius = radius * math.sin(theta);
        float phiIncrement = (gridSize / ringRadius) / math.PI;
        int pointCount = (int)(math.PI*2 / phiIncrement);

        phis[thetaIndex] = new float[pointCount];
        thetas[thetaIndex] = theta;

        for(int i = 0; i < pointCount; i++)
        {
            float phi = AngleInRadians(phiIncrement, i);

            phis[thetaIndex][i] = phi;
        }
    }

    float AngleInRadians(float increment, int count)
    {
        return increment * count + (increment * 0.5f);
    }

    void DrawPointsInSphere()
    {
        for(int t = 0; t < thetas.Length; t++)
            for(int p = 0; p < phis[t].Length; p++)
            {
                DrawLine(thetas[t], phis[t][p]);
            }
    }

    void DrawGridRow(float[] points)
    {
        for(int i = 0; i < points.Length; i++)
        {

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
}
