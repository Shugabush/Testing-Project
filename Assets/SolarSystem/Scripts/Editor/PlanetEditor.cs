#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace SolarSystem
{
    [CustomEditor(typeof(Planet))]
    public class PlanetEditor : Editor
    {
        Planet planet;
        Editor shapeEditor;
        Editor colorEditor;

        public override void OnInspectorGUI()
        {
            using (var check = new EditorGUI.ChangeCheckScope())
            {
                base.OnInspectorGUI();
                if (check.changed)
                {
                    planet.GeneratePlanet();
                }
            }

            if (GUILayout.Button("Generate Planet"))
            {
                planet.GeneratePlanet();
            }
            if (GUILayout.Button("Generate All Planets"))
            {
                Planet[] planets = FindObjectsByType<Planet>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
                foreach (var planet in planets)
                {
                    planet.GeneratePlanet();
                }
            }

            DrawSettingsEditor(planet.shapeSettings, planet.OnShapeSettingsUpdated, ref planet.shapeSettingsFoldout, ref shapeEditor);
            DrawSettingsEditor(planet.colorSettings, planet.OnColorSettingsUpdated, ref planet.colorSettingsFoldout, ref colorEditor);
        }

        void DrawSettingsEditor(Object settings, System.Action onSettingsUpdated, ref bool foldout, ref Editor editor)
        {
            if (settings != null)
            {
                foldout = EditorGUILayout.InspectorTitlebar(foldout, settings);
                using (var check = new EditorGUI.ChangeCheckScope())
                {
                    if (foldout)
                    {
                        CreateCachedEditor(settings, null, ref editor);
                        editor.OnInspectorGUI();
                        if (check.changed)
                        {
                            onSettingsUpdated?.Invoke();
                        }
                    }
                }
            }
        }

        void OnEnable()
        {
            planet = (Planet)target;
        }
    }
}
#endif