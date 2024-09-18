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
    using UnityEditor;
    using UnityEngine;

    namespace Kino
    {
        // Class used for drawing the brightness response curve
        public class BloomGraphDrawer
        {
            #region Public Methods

            // Update internal state with a given bloom instance.
            public void Prepare(BloomEffect bloom)
            {

                rangeX = 6;
                rangeY = 1.5f;

                threshold = bloom.ThresholdLinear;
                knee = bloom.softKnee * threshold + 1e-5f;

                // Intensity is capped to prevent sampling errors.
                intensity = Mathf.Min(bloom.Intensity, 10);
            }

            // Draw the graph at the current position.
            public void DrawGraph()
            {
                rectGraph = GUILayoutUtility.GetRect(128, 80);

                // Background
                DrawRect(0, 0, rangeX, rangeY, 0.1f, 0.4f);

                // Soft-knee range
                DrawRect(threshold - knee, 0, threshold + knee, rangeY, 0.25f, -1);

                // Horizontal lines
                for (var i = 1; i < rangeY; i++)
                    DrawLine(0, i, rangeX, i, 0.4f);

                // Vertical lines
                for (var i = 1; i < rangeX; i++)
                    DrawLine(i, 0, i, rangeY, 0.4f);

                // Label
                Handles.Label(
                    PointInRect(0, rangeY) + Vector3.right,
                    "Brightness Response (linear)", EditorStyles.miniLabel
                );

                // Threshold line
                DrawLine(threshold, 0, threshold, rangeY, 0.6f);

                // Response curve
                var vcount = 0;
                while (vcount < curveResolution)
                {
                    var x = rangeX * vcount / (curveResolution - 1);
                    var y = ResponseFunction(x);
                    if (y < rangeY)
                    {
                        curveVertices[vcount++] = PointInRect(x, y);
                    }
                    else
                    {
                        if (vcount > 1)
                        {
                            // Extend the last segment to the top edge of the rect.
                            var v1 = curveVertices[vcount - 2];
                            var v2 = curveVertices[vcount - 1];
                            var clip = (rectGraph.y - v1.y) / (v2.y - v1.y);
                            curveVertices[vcount - 1] = v1 + (v2 - v1) * clip;
                        }
                        break;
                    }
                }

                if (vcount > 1)
                {
                    Handles.color = Color.white * 0.9f;
                    Handles.DrawAAPolyLine(2.0f, vcount, curveVertices);
                }
            }

            #endregion

            #region Response Function

            float threshold;
            float knee;
            float intensity;

            float ResponseFunction(float x)
            {
                var rq = Mathf.Clamp(x - threshold + knee, 0, knee * 2);
                rq = rq * rq * 0.25f / knee;
                return Mathf.Max(rq, x - threshold) * intensity;
            }

            #endregion

            #region Graph Functions

            // Number of vertices in curve
            const int curveResolution = 96;

            // Vertex buffers
            Vector3[] rectVertices = new Vector3[4];
            Vector3[] lineVertices = new Vector3[2];
            Vector3[] curveVertices = new Vector3[curveResolution];

            Rect rectGraph;
            float rangeX;
            float rangeY;

            // Transform a point into the graph rect.
            Vector3 PointInRect(float x, float y)
            {
                x = Mathf.Lerp(rectGraph.x, rectGraph.xMax, x / rangeX);
                y = Mathf.Lerp(rectGraph.yMax, rectGraph.y, y / rangeY);
                return new Vector3(x, y, 0);
            }

            // Draw a line in the graph rect.
            void DrawLine(float x1, float y1, float x2, float y2, float grayscale)
            {
                lineVertices[0] = PointInRect(x1, y1);
                lineVertices[1] = PointInRect(x2, y2);
                Handles.color = Color.white * grayscale;
                Handles.DrawAAPolyLine(2.0f, lineVertices);
            }

            // Draw a rect in the graph rect.
            void DrawRect(float x1, float y1, float x2, float y2, float fill, float line)
            {
                rectVertices[0] = PointInRect(x1, y1);
                rectVertices[1] = PointInRect(x2, y1);
                rectVertices[2] = PointInRect(x2, y2);
                rectVertices[3] = PointInRect(x1, y2);

                Handles.DrawSolidRectangleWithOutline(
                    rectVertices,
                    fill < 0 ? Color.clear : Color.white * fill,
                    line < 0 ? Color.clear : Color.white * line
                );
            }

            #endregion
        }
    }
}