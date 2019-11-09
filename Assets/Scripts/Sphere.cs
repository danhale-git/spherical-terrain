using UnityEngine;
using Unity.Mathematics;

public class Sphere : MonoBehaviour
{
    public float radius = 5;
    public float gridSize = 5;

    const float radians = 6.283f;

    void Update()
    {
        gridSize = math.clamp(gridSize, 1, 100);
        radius = math.clamp(radius, 0, 20);

        PlotPointsOnSphere();
    }   

    void PlotPointsOnSphere()
    {
        float thetaIncrement = (gridSize / radius) / math.PI;
        int pointCount = (int)(math.PI / thetaIncrement);

        for(int i = 0; i < pointCount; i++)
        {
            float theta = thetaIncrement * i + (thetaIncrement * 0.5f); 
            PlotHorizontalRing(theta);
        }
    }

    void PlotHorizontalRing(float theta)
    {   
        float ringRadius = radius * math.sin(theta);

        float phiIncrement = (gridSize / ringRadius) / math.PI;
        int pointCount = (int)(math.PI*2 / phiIncrement);

        for(int i = 0; i < pointCount; i++)
        {
            float phi = phiIncrement * i + (phiIncrement * 0.5f);
            DrawLine(theta, phi);
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
