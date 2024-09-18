using UnityEngine;

namespace SolarSystem
{
    class VirtualBody
    {
        public CelestialBody body;
        public Vector3 position;
        public Vector3 velocity;
        public float mass;

        public VirtualBody(CelestialBody body)
        {
            this.body = body;
            position = body.transform.position;
            velocity = body.initialVelocity;
            mass = body.Mass;
        }
    }
}