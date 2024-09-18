using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions.Must;
using static UnityEngine.Rendering.DebugUI;

namespace SolarSystem
{
    [ExecuteInEditMode]
    public class OrbitDebugDisplay : MonoBehaviour
    {
        public int stepOffset = 0;
        public int numSteps = 1000;
        public float timeStep = 0.1f;
        public bool usePhysicsTimeStep;

        public bool relativeToBody;
        public CelestialBody centralBody;
        public float width = 100;
        public bool useThickLines;

        void Start()
        {
            if (Application.isPlaying)
            {
                HideOrbits();
            }
        }

        void Update()
        {
            if (!Application.isPlaying)
            {
                DrawOrbits();
            }
        }

        void DrawOrbits()
        {
            CelestialBody[] bodies = FindObjectsByType<CelestialBody>(FindObjectsInactive.Exclude, FindObjectsSortMode.InstanceID);
            Dictionary<CelestialBody, VirtualBody> virtualBodies = new Dictionary<CelestialBody, VirtualBody>();
            Vector3[][] drawPoints = new Vector3[bodies.Length][];
            CelestialBody referenceFrameBody = null;
            Vector3 referenceBodyInitialPosition = Vector3.zero;
            Dictionary<CelestialBody, Vector3> positions = new Dictionary<CelestialBody, Vector3>();

            numSteps = Mathf.Clamp(numSteps, 0, 25000);

            // Initialize virtual bodies (don't want to move the actual bodies)
            for (int i = 0; i < bodies.Length; i++)
            {
                VirtualBody newBody = new VirtualBody(bodies[i]);
                virtualBodies.Add(bodies[i], newBody);
                drawPoints[i] = new Vector3[numSteps];

                if (bodies[i] == centralBody && relativeToBody)
                {
                    referenceFrameBody = bodies[i];
                    referenceBodyInitialPosition = newBody.position;
                }
            }


            // Simulate
            for (int step = 0; step < stepOffset; step++)
            {
                Vector3 referenceBodyPosition = relativeToBody ? virtualBodies[referenceFrameBody].position : Vector3.zero;
                // Update velocities
                foreach (var vBody in virtualBodies.Values)
                {
                    vBody.velocity += CalculateAcceleration(vBody, virtualBodies) * timeStep;
                    if (!positions.TryAdd(vBody.body, vBody.position))
                    {
                        positions[vBody.body] = vBody.position;
                    }
                }
                // Update positions
                for (int i = 0; i < bodies.Length; i++)
                {
                    VirtualBody vBody = virtualBodies[bodies[i]];
                    Vector3 newPos = vBody.position + vBody.velocity * timeStep;
                    vBody.position = newPos;

                    if (bodies[i].parentBody != null)
                    {
                        // Offset newPos in the opposite direction of parentBody's movement (but only for drawing)
                        newPos -= positions[bodies[i].parentBody] - bodies[i].parentBody.Rb.position;
                    }
                    else if (relativeToBody)
                    {
                        if (bodies[i] == referenceFrameBody)
                        {
                            newPos = referenceBodyInitialPosition;
                        }
                        else
                        {
                            Vector3 referenceFrameOffset = referenceBodyPosition - referenceBodyInitialPosition;
                            newPos -= referenceFrameOffset;
                        }
                    }
                }
            }

            for (int step = 0; step < numSteps; step++)
            {
                Vector3 referenceBodyPosition = relativeToBody ? virtualBodies[referenceFrameBody].position : Vector3.zero;
                // Update velocities
                foreach (var vBody in virtualBodies.Values)
                {
                    vBody.velocity += CalculateAcceleration(vBody, virtualBodies) * timeStep;
                    if (!positions.TryAdd(vBody.body, vBody.position))
                    {
                        positions[vBody.body] = vBody.position;
                    }
                }
                // Update positions
                for (int i = 0; i < bodies.Length; i++)
                {
                    VirtualBody vBody = virtualBodies[bodies[i]];
                    Vector3 newPos = vBody.position + vBody.velocity * timeStep;
                    vBody.position = newPos;

                    if (bodies[i].parentBody != null)
                    {
                        // Offset newPos in the opposite direction of parentBody's movement (but only for drawing)
                        newPos -= positions[bodies[i].parentBody] - bodies[i].parentBody.Rb.position;
                    }
                    else if (relativeToBody)
                    {
                        if (bodies[i] == referenceFrameBody)
                        {
                            newPos = referenceBodyInitialPosition;
                        }
                        else
                        {
                            Vector3 referenceFrameOffset = referenceBodyPosition - referenceBodyInitialPosition;
                            newPos -= referenceFrameOffset;
                        }
                    }

                    drawPoints[i][step] = newPos;
                }
            }

            // Draw paths
            for (int bodyIndex = 0; bodyIndex < bodies.Length; bodyIndex++)
            {
                Color pathColor = bodies[bodyIndex].pathColor;
                LineRenderer lineRenderer = bodies[bodyIndex].Lr;

                if (useThickLines)
                {
                    lineRenderer.enabled = true;
                    lineRenderer.positionCount = drawPoints[bodyIndex].Length;
                    lineRenderer.SetPositions(drawPoints[bodyIndex]);
                    lineRenderer.startColor = pathColor;
                    lineRenderer.endColor = pathColor;
                    lineRenderer.widthMultiplier = width;
                }
                else
                {
                    for (int i = 0; i < drawPoints[bodyIndex].Length - 1; i++)
                    {
                        Debug.DrawLine(drawPoints[bodyIndex][i], drawPoints[bodyIndex][i + 1], pathColor);
                    }

                    // Hide renderer
                    lineRenderer.enabled = false;
                }
            }
        }

        Vector3 CalculateAcceleration(VirtualBody currentBody, Dictionary<CelestialBody, VirtualBody> virtualBodies)
        {
            Vector3 acceleration = Vector3.zero;
            if (currentBody.body.move)
            {
                if (currentBody.body.parentBody != null)
                {
                    VirtualBody parentBody = virtualBodies[currentBody.body.parentBody];
                    Vector3 forceDir = (parentBody.position - currentBody.position).normalized;
                    float sqrDst = (parentBody.position - currentBody.position).sqrMagnitude;
                    acceleration = forceDir * Universe.gravitationalConstant * parentBody.mass / sqrDst;
                }
                else
                {
                    foreach (var vBody in virtualBodies)
                    {
                        if (vBody.Key == currentBody.body) continue;

                        Vector3 forceDir = (vBody.Value.position - currentBody.position).normalized;
                        float sqrDst = (vBody.Value.position - currentBody.position).sqrMagnitude;
                        acceleration += forceDir * Universe.gravitationalConstant * vBody.Value.mass / sqrDst;
                    }
                }
            }
            return acceleration;
        }

        void HideOrbits()
        {
            CelestialBody[] bodies = FindObjectsByType<CelestialBody>(FindObjectsInactive.Exclude, FindObjectsSortMode.InstanceID);

            for (int bodyIndex = 0; bodyIndex < bodies.Length; bodyIndex++)
            {
                bodies[bodyIndex].Lr.positionCount = 0;
            }
        }

        void OnValidate()
        {
            if (usePhysicsTimeStep)
            {
                timeStep = Universe.physicsTimeStep;
            }
        }
    }
}