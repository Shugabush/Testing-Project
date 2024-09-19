using UnityEngine;

namespace SolarSystem
{
    public abstract class CelestialBodyShape : ScriptableObject
    {
        public bool randomize;
        public int seed;
        public ComputeShader heightMapCompute;

        public bool perturbVertices;
        public ComputeShader perturbCompute;
        [Range(0f, 1f)]
        public float perturbStrength = 0.7f;

        public event System.Action OnSettingChanged;

        ComputeBuffer heightBuffer;

        public virtual float[] CalculateHeights(ComputeBuffer vertexBuffer)
        {
            Debug.Log(System.Environment.StackTrace);
            // Set data
            SetShapeData();
            heightMapCompute.SetInt("numVertices", vertexBuffer.count);
            heightMapCompute.SetBuffer(0, "vertices", vertexBuffer);

            // Run
            ComputeHelper.Run(heightMapCompute, vertexBuffer.count);

            float[] heights = new float[vertexBuffer.count];
            heightBuffer.GetData(heights);
            return heights;
        }

        public virtual void ReleaseBuffers()
        {
            ComputeHelper.Release(heightBuffer);
        }

        protected virtual void SetShapeData()
        {

        }

        protected virtual void OnValidate()
        {
            OnSettingChanged?.Invoke();
        }
    }
}