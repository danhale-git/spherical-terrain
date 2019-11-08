using UnityEngine;
using Unity.Mathematics;
using Unity.Collections;

public class Sphere : MonoBehaviour
{
    public float radius = 5;
    public float gridSize = 5;

    const float radians = 6.283f;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if(gridSize < 1)
            gridSize = 1;

        float thetaIncrement = (gridSize / radius) / math.PI;
        int pointCount = (int)(math.PI / thetaIncrement);

        for(int i = 0; i < pointCount; i++)
        {
            float theta = thetaIncrement * i + (thetaIncrement * 0.5f); 
            Lines(theta);
        }
    }

    void Lines(float theta)
    {   
        float phiRadius = radius * math.sin(theta);

        float phiIncrement = (gridSize / phiRadius) / math.PI;
        int pointCount = (int)(math.PI*2 / phiIncrement);

        for(int i = 0; i < pointCount; i++)
        {
            float phi = phiIncrement * i + (phiIncrement * 0.5f);
            Line(theta, phi);
        }
    }

    void Line(float theta, float phi)
    {
        float3 position = PositionOnSphere(theta, phi);
        Debug.DrawLine(float3.zero + (position*0.8f), position, new Color(position.x, position.y, position.z));
    }

    float3 PositionOnSphere(float theta, float phi)
    {
        float degTheta = theta;
        float degPhi = phi;
        float x = radius * math.sin(theta) * math.cos(phi);
        float y = radius * math.cos(theta);
        float z = radius * math.sin(theta) * math.sin(phi);

        return new float3(x, y, z);
    }
}
