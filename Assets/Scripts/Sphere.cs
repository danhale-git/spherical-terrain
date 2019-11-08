using UnityEngine;
using Unity.Mathematics;
using Unity.Collections;

public class Sphere : MonoBehaviour
{
    public float radius = 5;
    public float gridSize = 5;

    const float radians = 6.283f;

    public int lineCount = 0;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        lineCount = 0;

        float hemiCircumference = math.PI;

        float phiIncrement = (gridSize / radius) / hemiCircumference;
        int pointCount = (int)(hemiCircumference / phiIncrement);

        for(int i = 0; i < pointCount; i++)
        {
            float theta = phiIncrement * i + (phiIncrement * 0.5f); 
            Lines(theta);

            if(lineCount > 1000)
                return;
        }
    }

    void Lines(float theta)
    {   
        float phiRadius = radius * math.sin(theta);
        float phiCircumference = math.PI*2;

        float phiIncrement = (gridSize / phiRadius) / (phiCircumference*0.5f);
        int pointCount = (int)(phiCircumference / phiIncrement);

        for(int i = 0; i < pointCount; i++)
        {
            float phi = phiIncrement * i;
            float3 position = PositionOnSphere(theta, phi);
            Debug.DrawLine(float3.zero + (position*0.8f), position, new Color(position.x, position.y, position.z));
        }
        lineCount += pointCount;
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
