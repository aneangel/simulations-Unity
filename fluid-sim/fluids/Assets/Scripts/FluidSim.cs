using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class FluidSimulation : MonoBehaviour
{
    public int particleCount = 1000;
    public float particleMass = 1f;
    public float smoothingRadius = 1f;
    public float targetDensity = 1000f;
    public float pressureConstant = 200f;
    public float viscosityConstant = 0.01f;
    public Vector3 gravity = new Vector3(0, -9.81f, 0);
    public Vector3 containerSize = new Vector3(10f, 10f, 10f);

    private List<FluidParticle> particles;
    private GameObject particlePrefab;

    void Start()
    {
        InitializeParticles();
    }

    void Update()
    {
        CalculateDensityAndPressure();
        CalculateForces();
        UpdateParticles();
    }

    void InitializeParticles()
    {
        particles = new List<FluidParticle>();
        particlePrefab = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        particlePrefab.transform.localScale = Vector3.one * 0.1f;

        for (int i = 0; i < particleCount; i++)
        {
            Vector3 position = new Vector3(
                Random.Range(-containerSize.x / 2, containerSize.x / 2),
                Random.Range(-containerSize.y / 2, containerSize.y / 2),
                Random.Range(-containerSize.z / 2, containerSize.z / 2)
            );

            GameObject particleObject = Instantiate(particlePrefab, position, Quaternion.identity);
            FluidParticle particle = particleObject.AddComponent<FluidParticle>();
            particle.Position = position;
            particles.Add(particle);
        }

        Destroy(particlePrefab);
    }

    void CalculateDensityAndPressure()
    {
        foreach (FluidParticle particle in particles)
        {
            particle.Density = 0f;
            foreach (FluidParticle neighbor in particles)
            {
                float distance = Vector3.Distance(particle.Position, neighbor.Position);
                if (distance < smoothingRadius)
                {
                    particle.Density += particleMass * SmoothingKernel(distance, smoothingRadius);
                }
            }
            particle.Pressure = pressureConstant * (particle.Density - targetDensity);
        }
    }

    void CalculateForces()
    {
        foreach (FluidParticle particle in particles)
        {
            Vector3 pressureForce = Vector3.zero;
            Vector3 viscosityForce = Vector3.zero;

            foreach (FluidParticle neighbor in particles)
            {
                if (particle == neighbor) continue;

                Vector3 direction = neighbor.Position - particle.Position;
                float distance = direction.magnitude;

                if (distance < smoothingRadius)
                {
                    // Pressure force
                    float avgPressure = (particle.Pressure + neighbor.Pressure) / 2f;
                    pressureForce += -direction.normalized * avgPressure * particleMass / neighbor.Density * SmoothingKernelDerivative(distance, smoothingRadius);

                    // Viscosity force
                    viscosityForce += viscosityConstant * particleMass * (neighbor.Velocity - particle.Velocity) / neighbor.Density * SmoothingKernel(distance, smoothingRadius);
                }
            }

            Vector3 gravityForce = gravity * particle.Density;
            particle.Force = pressureForce + viscosityForce + gravityForce;
        }
    }

    void UpdateParticles()
    {
        foreach (FluidParticle particle in particles)
        {
            particle.UpdatePosition(Time.deltaTime);
            
            // Boundary conditions
            particle.Position = new Vector3(
                Mathf.Clamp(particle.Position.x, -containerSize.x / 2, containerSize.x / 2),
                Mathf.Clamp(particle.Position.y, -containerSize.y / 2, containerSize.y / 2),
                Mathf.Clamp(particle.Position.z, -containerSize.z / 2, containerSize.z / 2)
            );

            particle.transform.position = particle.Position;
        }
    }

    float SmoothingKernel(float distance, float radius)
    {
        if (distance >= radius) return 0f;
        float volume = Mathf.PI * Mathf.Pow(radius, 4) / 6f;
        return (radius - distance) * (radius - distance) / volume;
    }

    float SmoothingKernelDerivative(float distance, float radius)
    {
        if (distance >= radius) return 0f;
        float volume = Mathf.PI * Mathf.Pow(radius, 4) / 6f;
        return -2 * (radius - distance) / volume;
    }
}