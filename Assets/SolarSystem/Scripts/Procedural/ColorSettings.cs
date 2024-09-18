using UnityEngine;

namespace SolarSystem
{
    [CreateAssetMenu()]
    public class ColorSettings : ScriptableObject
    {
        public Material planetMaterial;
        public BiomeColorSettings biomeColorSettings;
        public Gradient oceanColor;
        public bool useOcean = true;

        [System.Serializable]
        public class BiomeColorSettings
        {
            public Biome[] biomes;
            public NoiseSettings noise;
            public float noiseOffset;
            public float noiseStrength;
            public float blendAmount;

            [System.Serializable]
            public class Biome
            {
                public Gradient gradient;
                public Color tint;
                [Range(0f, 1f)]
                public float startHeight;
                [Range(0f, 1f)]
                public float tintPercent;
            }
        }
    }
}