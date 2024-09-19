using UnityEngine;

namespace SolarSystem
{
    [CreateAssetMenu(menuName = "Celestial Body/Settings Holder")]
    public class CelestialBodySettings : ScriptableObject
    {
        public CelestialBodyShape shape;
        public CelestialBodyShading shading;
    }
}