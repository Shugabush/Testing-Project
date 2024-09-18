using UnityEngine;

namespace SolarSystem
{
    [ExecuteInEditMode, RequireComponent(typeof(Rigidbody))]
    public class CelestialBody : MonoBehaviour
    {
        public Color pathColor = Color.white;
        public CelestialBody parentBody;
        public float radius;
        public float surfaceGravity;
        public Vector3 initialVelocity;
        public bool move = true;
        Vector3 currentVelocity;
        Rigidbody rb;
        LineRenderer lr;
        Planet planet;
        public LineRenderer Lr
        {
            get
            {
                if (lr == null)
                {
                    lr = GetComponentInChildren<LineRenderer>();
                }
                return lr;
            }
        }
        public Rigidbody Rb
        {
            get
            {
                if (rb == null)
                {
                    rb = GetComponent<Rigidbody>();
                }
                return rb;
            }
        }
        public float Mass { get; private set; }

        void Awake()
        {
            if (planet == null)
            {
                planet = GetComponent<Planet>();
            }
            currentVelocity = initialVelocity;
            if (Application.isPlaying)
            {
                Lr.enabled = true;
                Lr.positionCount = 1;
                Lr.useWorldSpace = parentBody == null;
                if (parentBody != null)
                {
                    Lr.transform.SetParent(parentBody.transform, false);
                }
            }
        }

        public void UpdateVelocity(CelestialBody[] allBodies, float timeStep)
        {
            if (!move) return;
            if (parentBody != null)
            {
                float sqrDst = (parentBody.rb.position - rb.position).sqrMagnitude;
                Vector3 forceDir = (parentBody.rb.position - rb.position).normalized;
                Vector3 force = forceDir * Universe.gravitationalConstant * Mass * parentBody.Mass / sqrDst;
                Vector3 acceleration = force / Mass;
                currentVelocity += acceleration * timeStep;
            }
            else
            {
                foreach (var otherBody in allBodies)
                {
                    if (otherBody != this)
                    {
                        float sqrDst = (otherBody.rb.position - rb.position).sqrMagnitude;
                        Vector3 forceDir = (otherBody.rb.position - rb.position).normalized;
                        Vector3 force = forceDir * Universe.gravitationalConstant * Mass * otherBody.Mass / sqrDst;
                        Vector3 acceleration = force / Mass;
                        currentVelocity += acceleration * timeStep;
                    }
                }
            }
        }

        void OnValidate()
        {
            RecalculateMass();
        }

        public void RecalculateMass()
        {
            Mass = surfaceGravity * radius * radius / Universe.gravitationalConstant;
            Rb.mass = Mass;
        }

        public void UpdatePosition(float timeStep)
        {
            rb.position += currentVelocity * timeStep;
            Lr.positionCount++;
            const int maxPositionCount = 1000;
            if (Lr.positionCount > maxPositionCount)
            {
                for (int i = 1; i < Lr.positionCount; i++)
                {
                    Lr.SetPosition(i - 1, Lr.GetPosition(i));
                }
                Lr.positionCount = maxPositionCount;
            }
            Lr.SetPosition(Lr.positionCount - 1, Lr.useWorldSpace ? rb.position : parentBody.transform.InverseTransformPoint(rb.position));
        }
    }
}
