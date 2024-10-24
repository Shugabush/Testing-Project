using UnityEngine;

namespace SolarSystem
{
    [CreateAssetMenu(menuName = "Celestial Body/Ocean")]
    public class OceanSettings : ScriptableObject
    {
        public float depthMultiplier = 10f;
        public float alphaMultiplier = 70f;
        public Color colA, colB;
        public Color specularCol = Color.white;
        [Header("Waves")]
        public Texture2D waveNormalA;
        public Texture2D waveNormalB;
        [Range(0f, 1f)]
        public float waveStrength = 0.15f;
        public float waveScale = 15f;
        public float waveSpeed = 0.5f;

        [Range(0f, 1f)]
        public float smoothness = 0.92f;
        public Vector4 testParams;

        public void SetProperties(Material material, int seed, bool randomize)
        {
            material.SetFloat("depthMultiplier", depthMultiplier);
            material.SetFloat("alphaMultiplier", alphaMultiplier);

            material.SetTexture("waveNormalA", waveNormalA);
            material.SetTexture("waveNormalB", waveNormalB);
            material.SetFloat("waveStrength", waveStrength);
            material.SetFloat("waveNormalScale", waveScale);
            material.SetFloat("waveSpeed", waveSpeed);
            material.SetFloat("smoothness", smoothness);
            material.SetVector("params", testParams);

            if (randomize)
            {
                var random = new PRNG(seed);
                var randomColA = Color.HSVToRGB(random.Value(), random.Range(0.6f, 0.8f), random.Range(0.65f, 1f));
                var randomColB = ColorHelper.TweakHSV(randomColA, random.SignedValue() * 0.2f, random.SignedValue() * 0.2f, random.Range(-0.5f, -0.4f));

                material.SetColor("colA", randomColA);
                material.SetColor("colB", randomColB);
                material.SetColor("specularCol", Color.white);
            }
            else
            {
                material.SetColor("colA", colA);
                material.SetColor("colB", colB);
                material.SetColor("specularCol", specularCol);
            }
        }
    }
}