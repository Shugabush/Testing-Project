using UnityEngine;

namespace SolarSystem
{
    public class ColorGenerator
    {
        ColorSettings settings;
        Texture2D texture;
        const int textureResolution = 50;
        INoiseFilter biomeNoiseFilter;

        public void UpdateSettings(ColorSettings settings)
        {
            this.settings = settings;
            int actualResolution = textureResolution * (settings.useOcean ? 2 : 1);
            if (texture == null || texture.height != settings.biomeColorSettings.biomes.Length || texture.width != actualResolution)
            {
                texture = new Texture2D(actualResolution, settings.biomeColorSettings.biomes.Length, TextureFormat.RGBA32, false);
            }
            biomeNoiseFilter = NoiseFilterFactory.CreateNoiseFilter(settings.biomeColorSettings.noise);
        }

        public void UpdateElevation(MinMax elevationMinMax)
        {
            settings.planetMaterial.SetVector("_ElevationMinMax", new Vector4(elevationMinMax.Min, elevationMinMax.Max));
        }

        public float BiomePercentFromPoint(Vector3 pointOnUnitSphere)
        {
            float heightPercent = (pointOnUnitSphere.y + 1f) / 2f;
            heightPercent += (biomeNoiseFilter.Evaluate(pointOnUnitSphere) - settings.biomeColorSettings.noiseOffset) * settings.biomeColorSettings.noiseStrength;
            float biomeIndex = 0;
            int numBiomes = settings.biomeColorSettings.biomes.Length;
            float blendRange = settings.biomeColorSettings.blendAmount / 2f + 0.001f;

            for (int i = 0; i < numBiomes; i++)
            {
                float dst = heightPercent - settings.biomeColorSettings.biomes[i].startHeight;
                float weight = Mathf.InverseLerp(-blendRange, blendRange, dst);
                biomeIndex *= 1f - weight;
                biomeIndex += i * weight;
            }

            return biomeIndex / Mathf.Max(1f, numBiomes - 1f);
        }

        public void UpdateColors()
        {
            int colorSize = textureResolution * (settings.useOcean ? 2 : 1);
            Color[] colors = new Color[colorSize * settings.biomeColorSettings.biomes.Length];
            int colorIndex = 0;
            foreach (var biome in settings.biomeColorSettings.biomes)
            {
                for (int i = 0; i < colorSize; i++)
                {
                    Color gradientCol;
                    if (i < textureResolution && settings.useOcean)
                    {
                        gradientCol = settings.oceanColor.Evaluate(i / (textureResolution - 1f));
                    }
                    else
                    {
                        gradientCol = biome.gradient.Evaluate((i - (settings.useOcean ? textureResolution : 0f)) / (textureResolution - 1f));
                    }
                    Color tintColor = biome.tint;
                    colors[colorIndex] = gradientCol * (1f - biome.tintPercent) + tintColor * biome.tintPercent;
                    colorIndex++;
                }
            }
            texture.SetPixels(colors);
            texture.Apply();
            UpdateTexture();
        }

        public void UpdateTexture()
        {
            if (settings != null && settings.planetMaterial != null)
            {
                settings.planetMaterial.SetInt("_UseOcean", settings.useOcean ? 1 : 0);
            }
            if (texture != null)
            {
                settings.planetMaterial.mainTexture = texture;
            }
        }
    }
}