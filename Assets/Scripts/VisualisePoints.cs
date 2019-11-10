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

    PlotPoints plot;

    void InputValues()
    {
        pointDistance = math.clamp(pointDistance, 2, 100);
        radius = math.clamp(radius, 0, 15);

        gridSize = radius * 2;
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

   /* void GetAdjacency()
    {
        for(int t = 0; t < thetas.Length; t++)
        {
            if(t == 0 || t == thetas.Length-1)
                continue;

            

            float boundSize = Increment(radius * math.sin(thetas[t])) * 0.5f;
            float nextBoundSize = Increment(radius * math.sin(thetas[t+1])) * 0.5f;
            float prevBoundSize = Increment(radius * math.sin(thetas[t-1])) * 0.5f;

            for(int p = 0; p < plot.phis[t].Length; p++)
            {
                float boundsStart = plot.phis[t][p] - boundSize;
                float boundsEnd = plot.phis[t][p] + boundSize;

                //Lerp to find closes index is adjacent row.
                // Start at that index and horizontal flood fill both ways, checking bounds as you go
            }
        }
    }

    int ClosestAdjacentPhi(int thetaIndex, int phiIndex, int otherThetaIndex)
    {
        int length = plot.phis[thetaIndex].Length;
        int otherLength = plot.phis[otherThetaIndex].Length;
        Debug.Log(length+" "+otherLength);

        float normalized = math.unlerp(0, length-1, phiIndex);
        int interpolated = (int)math.round(math.lerp(0, otherLength-1, normalized));

        return interpolated;
    } */

    void DrawGrid()
    {
        float rowHeight = gridSize / plot.thetas.Length;
        for(int i = 0; i < plot.thetas.Length; i++)
        {
            DrawGridRow(rowHeight, i);
        }
    }

    void DrawGridRow(float rowHeight, int thetaIndex)
    {
        float sizeInGrid = gridSize / plot.phis[thetaIndex].Length;
        for(int i = 0; i < plot.phis[thetaIndex].Length; i++)
        {
            float3 start = new float3(sizeInGrid * i, 0, rowHeight * thetaIndex);
            float3 offset = new float3(sizeInGrid, 0, 0);
            float3 vert = new float3(0, 0, rowHeight/2);

            float3 pos = plot.positions[thetaIndex][i];
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
}
