using UnityEngine;

namespace SolarSystem
{
    public class NBodySimulation : MonoBehaviour
    {
        CelestialBody[] bodies;

        void Awake()
        {
            bodies = FindObjectsByType<CelestialBody>(FindObjectsInactive.Include, FindObjectsSortMode.InstanceID);
            Time.fixedDeltaTime = Universe.physicsTimeStep;
        }

        void FixedUpdate()
        {
            for (int i = 0; i < bodies.Length; i++)
            {
                bodies[i].UpdateVelocity(bodies, Universe.physicsTimeStep);
            }

            for (int i = 0; i < bodies.Length; i++)
            {
                bodies[i].UpdatePosition(Universe.physicsTimeStep);
            }
        }
    }
}