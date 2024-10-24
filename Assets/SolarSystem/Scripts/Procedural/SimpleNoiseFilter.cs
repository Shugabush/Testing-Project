using UnityEngine;

namespace SolarSystem
{
    public class SimpleNoiseFilter : INoiseFilter
    {
        NoiseSettings.SimpleNoiseSettings settings;
        Noise noise = new Noise();

        public SimpleNoiseFilter(NoiseSettings.SimpleNoiseSettings settings)
        {
            this.settings = settings;
        }

        public float Evaluate(Vector3 point)
        {
            float noiseValue = 0f;
            float frequency = settings.baseRoughness;
            float amplitude = 1f;

            for (int i = 0; i < settings.numLayers; i++)
            {
                float v = noise.Evaluate(point * frequency + settings.center);
                noiseValue += (v + 1) * 0.5f * amplitude;
                frequency *= settings.roughness;
                amplitude *= settings.persistance;
            }
            noiseValue = noiseValue - settings.minValue;
            return noiseValue * settings.strength;
        }
    }
}