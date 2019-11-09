using UnityEngine;
using Unity.Mathematics;

public class Sphere : MonoBehaviour
{
    public bool showSphere = true;
    public float radius = 5;
    public float pointDistance = 5;
    
    public bool showGrid = true;
    float gridSize;

    const float radians = 6.283f;

    float3[][] positions;
    float[][] phis;
    float[] thetas;

    void InputValues()
    {
        pointDistance = math.clamp(pointDistance, 2, 100);
        radius = math.clamp(radius, 0, 15);

        gridSize = radius * 2;
    }

    void Update()
    {
        InputValues();

        PlotHorizontalRings();

        if(showSphere)
            DrawPointsInSphere();

        if(showGrid)
            DrawGrid();
    }   

    void PlotHorizontalRings()
    {
        float thetaIncrement = (pointDistance / radius) / math.PI;
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
        float ringRadius = radius * math.sin(theta);
        float phiIncrement = (pointDistance / ringRadius) / math.PI;
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

    float AngleInRadians(float increment, int count)
    {
        return increment * count + (increment * 0.5f);
    }

    void DrawPointsInSphere()
    {
        for(int t = 0; t < positions.Length; t++)
        {
            for(int p = 0; p < positions[t].Length; p++)
            {
                DrawLine(positions[t][p]);
            }
        }
    }

    void DrawGrid()
    {
        float rowHeight = gridSize / thetas.Length;
        for(int i = 0; i < thetas.Length; i++)
        {
            DrawGridRow(rowHeight, i);
        }
    }

    void DrawGridRow(float rowHeight, int thetaIndex)
    {
        float sizeInGrid = gridSize / phis[thetaIndex].Length;
        for(int i = 0; i < phis[thetaIndex].Length; i++)
        {
            float3 start = new float3(sizeInGrid * i, 0, rowHeight * thetaIndex);
            float3 offset = new float3(sizeInGrid, 0, 0);
            float3 vert = new float3(0, 0, rowHeight/2);

            float3 pos = positions[thetaIndex][i];
            Color color = new Color(pos.x, pos.y, pos.z);
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

    float3 PositionOnSphere(float theta, float phi)
    {
        float x = radius * math.sin(theta) * math.cos(phi);
        float y = radius * math.cos(theta);
        float z = radius * math.sin(theta) * math.sin(phi);

        return new float3(x, y, z);
    }
}
