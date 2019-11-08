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
        //Lines(radians * 0.3f);
        //return;

        lineCount = 0;

        float hemiCircumference = math.PI;

        float phiIncrement = gridSize / hemiCircumference;
        int pointCount = (int)(hemiCircumference / phiIncrement);

        for(int i = 0; i < pointCount; i++)
        {
            float theta = phiIncrement * i; 
            Lines(theta);

            if(lineCount > 1000)
                return;
        }

        //Debug.DrawLine(float3.zero, PositionOnSphere());   
        //float3 drop = new float3(0, -0.1f, 0);
        //Debug.DrawLine(float3.zero + drop, PositionOnSphere() + drop);   
    }

    void Lines(float theta)
    {   
        float phiRadius = radius * math.sin(theta);
        float phiCircumference = ((phiRadius * 2) * math.PI) / radius;

        float phiIncrement = gridSize / phiCircumference;
        int pointCount = (int)(phiCircumference / phiIncrement);

        for(int i = 0; i < pointCount; i++)
        {
            float phi = (radians / pointCount) * i;
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
