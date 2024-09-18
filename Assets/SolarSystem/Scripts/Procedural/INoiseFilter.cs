using UnityEngine;

namespace SolarSystem
{
    public interface INoiseFilter
    {
        float Evaluate(Vector3 point);
    }
}