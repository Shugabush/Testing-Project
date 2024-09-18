namespace SolarSystem
{
    //
    // Kino/Bloom v2 - Bloom filter for Unity
    //
    // Copyright (C) 2015, 2016 Keijiro Takahashi
    //
    // Permission is hereby granted, free of charge, to any person obtaining a copy
    // of this software and associated documentation files (the "Software"), to deal
    // in the Software without restriction, including without limitation the rights
    // to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
    // copies of the Software, and to permit persons to whom the Software is
    // furnished to do so, subject to the following conditions:
    //
    // The above copyright notice and this permission notice shall be included in
    // all copies or substantial portions of the Software.
    //
    // THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
    // IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
    // FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
    // AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
    // LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
    // OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
    // THE SOFTWARE.
    //
    using UnityEngine;
    using UnityEditor;

    namespace Kino
    {
        [CanEditMultipleObjects]
        [CustomEditor(typeof(BloomEffect))]
        public class BloomEditor : Editor
        {
            BloomGraphDrawer graph;

            SerializedProperty threshold;
            SerializedProperty softKnee;
            SerializedProperty radius;
            SerializedProperty intensity;
            SerializedProperty highQuality;
            SerializedProperty antiFlicker;

            static GUIContent textThreshold = new GUIContent("Threshold (gamma)");

            void OnEnable()
            {
                graph = new BloomGraphDrawer();
                threshold = serializedObject.FindProperty("threshold");
                softKnee = serializedObject.FindProperty("softKnee");
                radius = serializedObject.FindProperty("radius");
                intensity = serializedObject.FindProperty("intensity");
                highQuality = serializedObject.FindProperty("highQuality");
                antiFlicker = serializedObject.FindProperty("antiFlicker");
            }

            public override void OnInspectorGUI()
            {
                serializedObject.Update();

                if (!serializedObject.isEditingMultipleObjects)
                {
                    EditorGUILayout.Space();
                    graph.Prepare((BloomEffect)target);
                    graph.DrawGraph();
                    EditorGUILayout.Space();
                }

                EditorGUILayout.PropertyField(threshold, textThreshold);
                EditorGUILayout.PropertyField(softKnee);
                EditorGUILayout.PropertyField(intensity);
                EditorGUILayout.PropertyField(radius);
                EditorGUILayout.PropertyField(highQuality);
                EditorGUILayout.PropertyField(antiFlicker);

                serializedObject.ApplyModifiedProperties();
            }
        }
    }

}