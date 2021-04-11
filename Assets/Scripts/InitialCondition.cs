using Unity.Mathematics;

namespace NBodySimulator
{
    [System.Serializable]
    public struct InitialCondition
    {
        public int numberOfParticles;

        public float3 center;
        public float radius;
        public float3 shapeElipsoid;

        public float turbulenceStrength;
        public float3 turbulenceElipsoid;

        public float angularVelocity;
        public int seed;
    }
}
