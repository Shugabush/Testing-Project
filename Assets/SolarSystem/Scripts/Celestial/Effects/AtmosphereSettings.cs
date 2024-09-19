using UnityEngine;
using static UnityEngine.Mathf;

namespace SolarSystem
{
    [CreateAssetMenu(menuName = "Celestial Body/Atmosphere")]
    public class AtmosphereSettings : ScriptableObject
    {
        public bool enabled = true;
        public Shader atmosphereShader;
        public ComputeShader opticalDepthCompute;
        public int textureSize = 256;

        public int inScatteringPoints = 10;
        public int opticalDepthPoints = 10;
        public float densityFalloff = 0.25f;

        public Vector3 wavelengths = new Vector3(700f, 530f, 460f);

        public Vector4 testParams = new Vector4(7f, 1.26f, 0.1f, 3f);
        public float scatteringStrength = 20f;
        public float intensity = 1f;

        public float ditherStrength = 0.8f;
        public float ditherScale = 4f;
        public Texture2D blueNoise;

        [Range(0f, 1f)]
        public float atmosphereScale = 0.5f;

        [Header("Test")]
        public float timeOfDay;
        public float sunDst = 1f;

        RenderTexture opticalDepthTexture;
        bool settingsUpToDate;

        public void SetProperties(Material material, float bodyRadius)
        {
            if (!settingsUpToDate || !Application.isPlaying)
            {
                var sun = GameObject.Find("Test Sun");
                if (sun != null)
                {
                    sun.transform.position = new Vector3(Cos(timeOfDay), Sin(timeOfDay));
                    sun.transform.LookAt(Vector3.zero);
                }

                float atmosphereRadius = (1f + atmosphereScale) * bodyRadius;

                material.SetVector("params", testParams);
                material.SetInt("numInScatteringPoints", inScatteringPoints);
                material.SetInt("numOpticalDepthPoints", opticalDepthPoints);
                material.SetFloat("atmosphereRadius", atmosphereRadius);
                material.SetFloat("planetRadius", bodyRadius);
                material.SetFloat("densityFalloff", densityFalloff);
            }
        }
    }
}