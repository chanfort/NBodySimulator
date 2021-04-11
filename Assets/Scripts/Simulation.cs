using UnityEngine;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Jobs;
using Unity.Burst;

namespace NBodySimulator
{
    public class Simulation : MonoBehaviour
    {
        public static Simulation active;

        int nTotal = 0;
        ParticleSystem pSystem;
        public List<InitialCondition> initialConditionsSet = new List<InitialCondition>();
        public List<RuntimeCondition> runtimeConditionsSet = new List<RuntimeCondition>();

        NativeArray<double3> positions;
        NativeArray<double3> positionsCopy;
        NativeArray<double3> velocities;
        NativeArray<double> masses;

        public List<Color> palette = new List<Color>();
        ParticleSystem.Particle[] particles = new ParticleSystem.Particle[0];

        void Awake()
        {
            active = this;
        }

        void Start()
        {
            pSystem = GetComponent<ParticleSystem>();

            List<InitialCondition> conditions = new List<InitialCondition>();

            for (int i = 0; i < initialConditionsSet.Count; i++)
            {
                conditions.Add(initialConditionsSet[i]);
            }

            for (int i = 0; i < conditions.Count; i++)
            {
                ConditionsController.active.Add(conditions[i]);
            }

            ConditionsController.active.StartAll();
        }

        void Update()
        {
            for (int i = 0; i < nTotal; i++)
            {
                positions[i] = positionsCopy[i];

                float px = (float)positions[i].x;
                float py = (float)positions[i].y;
                float pz = (float)positions[i].z;

                particles[i].position = new Vector3(px, py, pz);
                particles[i].startColor = 0.5f * ColorFromPalette((float)(masses[i]) / 120.0f, palette) + new Color(0.5f, 0.5f, 0.5f, 1f);
            }

            pSystem.SetParticles(particles, particles.Length);
            UpdateStep();
        }


        public void UpdateStep()
        {
            new CalculateStep
            {
                positions = positions,
                positionsCopy = positionsCopy,
                velocities = velocities,
                masses = masses,
                nTotal = nTotal
            }.Schedule(nTotal, System.Environment.ProcessorCount).Complete();
        }

        [BurstCompile]
        struct CalculateStep : IJobParallelFor
        {
            [NativeDisableParallelForRestriction] [ReadOnly] public NativeArray<double3> positions;
            [NativeDisableParallelForRestriction] [WriteOnly] public NativeArray<double3> positionsCopy;
            [NativeDisableParallelForRestriction] public NativeArray<double3> velocities;
            [NativeDisableParallelForRestriction] [ReadOnly] public NativeArray<double> masses;
            public int nTotal;

            public void Execute(int i)
            {
                double3 dv = 0.0;
                double3 posi = positions[i];

                for (int j = 0; j < nTotal; j++)
                {
                    if (i != j)
                    {
                        double3 r = positions[j] - posi;

                        double rSq = math.lengthsq(r) + 0.01;
                        double rSqrt = math.sqrt(rSq);

                        dv += masses[j] * r / (rSq * rSqrt);
                    }
                }

                double timestep = 0.5f;

                velocities[i] += timestep * 0.000001f * dv;
                positionsCopy[i] = positions[i] + timestep * velocities[i];
            }
        }

        public void AddInitialConditions(List<InitialCondition> initialConditions)
        {
            int nTotalOld = nTotal;
            int n = nTotalOld;

            for (int j = 0; j < initialConditions.Count; j++)
            {
                runtimeConditionsSet.Add(
                    new RuntimeCondition
                    {
                        startIndex = n,
                        numberOfParticles = initialConditions[j].numberOfParticles
                    }
                );

                for (int i = 0; i < initialConditions[j].numberOfParticles; i++)
                {
                    n++;
                }
            }

            nTotal = n;

            if (!positions.IsCreated || positions.Length == 0)
            {
                positions = new NativeArray<double3>(n, Allocator.Persistent);
                positionsCopy = new NativeArray<double3>(n, Allocator.Persistent);
                velocities = new NativeArray<double3>(n, Allocator.Persistent);
                masses = new NativeArray<double>(n, Allocator.Persistent);

                particles = new ParticleSystem.Particle[n];
            }
            else
            {
                positions = NativeArrayUtils.Append(positions, new NativeArray<double3>(nTotal - nTotalOld, Allocator.Persistent));
                positionsCopy = NativeArrayUtils.Append(positionsCopy, new NativeArray<double3>(nTotal - nTotalOld, Allocator.Persistent));
                velocities = NativeArrayUtils.Append(velocities, new NativeArray<double3>(nTotal - nTotalOld, Allocator.Persistent));
                masses = NativeArrayUtils.Append(masses, new NativeArray<double>(nTotal - nTotalOld, Allocator.Persistent));

                particles = NativeArrayUtils.Append(particles, new ParticleSystem.Particle[nTotal - nTotalOld]);
            }

            n = 0;

            for (int j = 0; j < initialConditions.Count; j++)
            {
                UnityEngine.Random.InitState(initialConditions[j].seed);

                for (int i = 0; i < initialConditions[j].numberOfParticles; i++)
                {
                    float3 randPos = ElipsoidRandom(initialConditions[j].shapeElipsoid, initialConditions[j].center, initialConditions[j].radius);

                    int k = n + nTotalOld;

                    positions[k] = randPos;
                    positionsCopy[k] = randPos;

                    float3 crossProd = initialConditions[j].angularVelocity * Vector3.Cross(Vector3.up, randPos);
                    velocities[k] = crossProd;

                    float3 turbulence = ElipsoidRandom(initialConditions[j].turbulenceElipsoid, float3.zero, initialConditions[j].turbulenceStrength);
                    velocities[k] += turbulence;

                    masses[k] = UnityEngine.Random.Range(0.1f, 100f);

                    particles[k].position = randPos;
                    particles[k].angularVelocity = 0f;
                    particles[k].rotation = 0f;
                    particles[k].velocity = new Vector3(0f, 0f, 0f);
                    particles[k].startLifetime = 10f;
                    particles[k].remainingLifetime = 10f - UnityEngine.Random.Range(0f, 1f);
                    particles[k].startSize = 0.1f;
                    particles[k].startColor = Color.white;

                    n++;
                }
            }

            pSystem.SetParticles(particles, nTotal);
        }

        public void RemoveRuntimeCondition(RuntimeCondition condition)
        {
            positions = NativeArrayUtils.RemoveRange(positions, condition.startIndex, condition.numberOfParticles);
            positionsCopy = NativeArrayUtils.RemoveRange(positionsCopy, condition.startIndex, condition.numberOfParticles);
            velocities = NativeArrayUtils.RemoveRange(velocities, condition.startIndex, condition.numberOfParticles);
            masses = NativeArrayUtils.RemoveRange(masses, condition.startIndex, condition.numberOfParticles);
            particles = NativeArrayUtils.RemoveRange(particles, condition.startIndex, condition.numberOfParticles);

            nTotal -= condition.numberOfParticles;
            pSystem.SetParticles(particles, nTotal);

            int conditionIndex = runtimeConditionsSet.IndexOf(condition);

            for (int i = conditionIndex + 1; i < runtimeConditionsSet.Count; i++)
            {
                runtimeConditionsSet[i].startIndex -= condition.numberOfParticles;
            }

            runtimeConditionsSet.Remove(condition);
        }

        private float3 ElipsoidRandom(float3 shapeElipsoid, float3 center, float radius)
        {
            float3 randPos = radius * UnityEngine.Random.insideUnitSphere;

            randPos = new float3(
                randPos.x * shapeElipsoid.x,
                randPos.y * shapeElipsoid.y,
                randPos.z * shapeElipsoid.z
            ) + center;

            return randPos;
        }

        public static Color ColorFromPalette(float value, List<Color> palette)
        {
            int n = palette.Count;

            int ilow = (int)(value * (n - 1));
            int ihigh = (int)(value * (n - 1) + 1f);

            Color clow = palette[ilow];
            Color chigh = palette[ihigh];

            float value1 = Mathf.Repeat(value * (n - 1), 1f);

            float r = Interpolate(value1, 0f, 1f, clow.r, chigh.r);
            float g = Interpolate(value1, 0f, 1f, clow.g, chigh.g);
            float b = Interpolate(value1, 0f, 1f, clow.b, chigh.b);

            return (new Color(r, g, b, 1f));
        }

        public static float Interpolate(float x, float x0, float x1, float y0, float y1)
        {
            return y0 + (y1 - y0) * (x - x0) / (x1 - x0);
        }

        void OnApplicationQuit()
        {
            if (positions.IsCreated)
            {
                positions.Dispose();
                positionsCopy.Dispose();
                velocities.Dispose();
                masses.Dispose();
            }
        }
    }
}
