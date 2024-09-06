using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class FluidParticle : MonoBehaviour
{
    public Vector3 Position { get; set; }
    public Vector3 Velocity { get; set; }
    public Vector3 Force { get; set; }
    public float Density { get; set; }
    public float Pressure { get; set; }

    public void UpdatePosition(float deltaTime)
    {
        Velocity += Force * deltaTime;
        Position += Velocity * deltaTime;
    }
}
