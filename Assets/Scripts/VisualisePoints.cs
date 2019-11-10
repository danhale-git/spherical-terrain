using UnityEngine;
using Unity.Mathematics;

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

    void InputValues()
    {
        pointDistance = math.clamp(pointDistance, 2, 100);
        radius = math.clamp(radius, 0, 15);

        gridSize = radius * 2;

        if(Input.GetKeyDown(KeyCode.UpArrow))
        {
            int previousY = gridSelect.y;
            gridSelect.y = WrapIndex(gridSelect.y += 1, plot.zAngles.Length);
            gridSelect.x = VerticalAdjacent(previousY, gridSelect.x, gridSelect.y);
        }
        if(Input.GetKeyDown(KeyCode.DownArrow))
        {
            int previousY = gridSelect.y;
            gridSelect.y = WrapIndex(gridSelect.y -= 1, plot.zAngles.Length);
            gridSelect.x = VerticalAdjacent(previousY, gridSelect.x, gridSelect.y);
        }
        if(Input.GetKeyDown(KeyCode.LeftArrow))
            gridSelect.x = WrapIndex(gridSelect.x -= 1, plot.xAngles[gridSelect.y].Length);
        if(Input.GetKeyDown(KeyCode.RightArrow))
            gridSelect.x = WrapIndex(gridSelect.x += 1, plot.xAngles[gridSelect.y].Length);

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
    }/* */

    int VerticalAdjacent(int zIndex, int xIndex, int zIndexOther)
    {
        int length = plot.xAngles[zIndex].Length;
        int otherLength = plot.xAngles[zIndexOther].Length;
        Debug.Log(length+" "+otherLength);

        float normalized = math.unlerp(0, length-1, (float)xIndex);
        int interpolated = (int)math.round(math.lerp(0, otherLength-1, normalized));

        return interpolated;
    } 

    void DrawGrid()
    {
        float rowHeight = gridSize / plot.zAngles.Length;
        for(int i = 0; i < plot.zAngles.Length; i++)
        {
            DrawGridRow(rowHeight, i);
        }
    }

    void DrawGridRow(float rowHeight, int zIndex)
    {
        float sizeInGrid = gridSize / plot.xAngles[zIndex].Length;
        for(int i = 0; i < plot.xAngles[zIndex].Length; i++)
        {
            float3 start = new float3(sizeInGrid * i, 0, rowHeight * zIndex);
            float3 offset = new float3(sizeInGrid, 0, 0);
            float3 vert = new float3(0, 0, rowHeight/2);

            float3 pos = plot.positions[zIndex][i];
            Color color = new Color(pos.x, pos.y, pos.z);
            
            if(zIndex == gridSelect.y && i == gridSelect.x)
                color = new Color(1-color.r, 1-color.g, 1-color.b);
                
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
