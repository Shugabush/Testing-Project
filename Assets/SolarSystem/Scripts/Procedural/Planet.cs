using UnityEngine;

namespace SolarSystem
{
    [SelectionBase, ExecuteInEditMode]
    public class Planet : MonoBehaviour
    {
        public Transform meshParent;

        [Range(2, 256)]
        public int resolution = 10;
        public bool autoUpdate = true;
        public enum FaceRenderMask { All, Top, Bottom, Left, Right, Front, Back }
        public FaceRenderMask faceRenderMask;

        public ShapeSettings shapeSettings;
        public ColorSettings colorSettings;

        [HideInInspector]
        public bool shapeSettingsFoldout;
        [HideInInspector]
        public bool colorSettingsFoldout;

        ShapeGenerator shapeGenerator = new ShapeGenerator();
        ColorGenerator colorGenerator = new ColorGenerator();

        [SerializeField, HideInInspector]
        MeshFilter[] meshFilters;
        MeshRenderer[] meshRenderers;
        TerrainFace[] terrainFaces;

        void Initialize()
        {
            if (meshParent == null)
            {
                meshParent = transform;
            }
            shapeGenerator.UpdateSettings(shapeSettings);
            colorGenerator.UpdateSettings(colorSettings);

            if (meshFilters == null || meshFilters.Length == 0)
            {
                meshFilters = new MeshFilter[6];
            }
            if (meshRenderers == null || meshRenderers.Length == 0)
            {
                meshRenderers = new MeshRenderer[6];
            }
            terrainFaces = new TerrainFace[6];

            Vector3[] directions = { Vector3.up, Vector3.down, Vector3.left, Vector3.right, Vector3.forward, Vector3.back };

            for (int i = 0; i < 6; i++)
            {
                if (meshFilters[i] == null)
                {
                    GameObject meshObj = new GameObject("mesh");
                    meshObj.transform.parent = meshParent;

                    meshFilters[i] = meshObj.AddComponent<MeshFilter>();
                    
                }
                if (meshFilters[i].sharedMesh == null)
                {
                    meshFilters[i].sharedMesh = new Mesh();
                }
                if (meshRenderers[i] == null)
                {
                    meshRenderers[i] = meshFilters[i].gameObject.GetComponent<MeshRenderer>();
                    if (meshRenderers[i] == null)
                    {
                        meshRenderers[i] = meshFilters[i].gameObject.AddComponent<MeshRenderer>();
                    }
                    meshRenderers[i].sharedMaterial = colorSettings.planetMaterial;
                }

                terrainFaces[i] = new TerrainFace(shapeGenerator, meshFilters[i].sharedMesh, resolution, directions[i]);
                bool renderFace = faceRenderMask == FaceRenderMask.All || (int)faceRenderMask - 1 == i;
                meshFilters[i].gameObject.SetActive(renderFace);
            }
        }

        void Update()
        {
            colorGenerator.UpdateTexture();
        }

        public void GeneratePlanet()
        {
            Initialize();
            GenerateMesh();
            GenerateColors();
        }

        public void OnShapeSettingsUpdated()
        {
            if (autoUpdate)
            {
                Initialize();
                GenerateMesh();
            }
        }

        public void OnColorSettingsUpdated()
        {
            if (autoUpdate)
            {
                Initialize();
                GenerateColors();
            }
        }

        void GenerateMesh()
        {
            if (gameObject.activeInHierarchy)
            {
                for (int i = 0; i < terrainFaces.Length; i++)
                {
                    if (meshFilters[i].gameObject.activeSelf)
                    {
                        terrainFaces[i].ConstructMesh();
                    }
                }

                colorGenerator.UpdateElevation(shapeGenerator.elevationMinMax);
            }
        }

        void GenerateColors()
        {
            colorGenerator.UpdateColors();
            for (int i = 0; i < 6; i++)
            {
                if (meshRenderers[i].gameObject.activeSelf)
                {
                    meshRenderers[i].sharedMaterial = colorSettings.planetMaterial;
                    terrainFaces[i].UpdateUVs(colorGenerator);
                }
            }
        }
    }
}