
using UnityEngine;
using UnityEngine.Experimental.Rendering;

namespace SolarSystem
{
    public static class ComputeHelper
    {
        /// <summary>
        /// <para>Subscribe to this event to be notified when buffers created in edit mode should be released</para>
        /// <para>(i. e before script compilation occurs, and when exitting edit mode)</para>
        /// </summary>
        public static event System.Action ShouldReleaseEditModeBuffers;

        /// <summary>
        /// <para>Convenience method for dispatching a compute shader</para>
        /// <para>It calculates the number of thread groups based on the number of iterations needed.</para>
        /// </summary>
        public static void Run(ComputeShader cs, int numIterationsX, int numIterationsY = 1, int numIterationsZ = 1, int kernelIndex = 0)
        {
            Vector3Int threadGroupSizes = GetThreadGroupSizes(cs, kernelIndex);
            int numGroupsX = Mathf.CeilToInt(numIterationsX / (float)threadGroupSizes.x);
            int numGroupsY = Mathf.CeilToInt(numIterationsY / (float)threadGroupSizes.y);
            int numGroupsZ = Mathf.CeilToInt(numIterationsZ / (float)threadGroupSizes.z);
            cs.Dispatch(kernelIndex, numGroupsX, numGroupsY, numGroupsZ);
        }

        /// <summary>
        /// <para>Only run compute shaders if this is true</para>
        /// <para>This is only relevant for compute shaders that run outside of playmode</para>
        /// </summary>
        public static bool CanRunEditModeCompute
        {
            get { return CheckIfCanRunInEditMode(); }
        }

        /// <summary>
        /// <para>Set all values from settings object on the shader. Note, variable names must be an exact match in the shader.</para>
        /// <para>Settings object can be any class/struct containing vectors/ints/floats/bools</para>
        /// </summary>
        public static void SetParams(System.Object settings, ComputeShader shader, string variableNamePrefix = "", string variableNameSuffix = "")
        {
            var fields = settings.GetType().GetFields();
            foreach (var field in fields)
            {
                var fieldType = field.FieldType;
                string shaderVariableName = variableNamePrefix + field.Name + variableNameSuffix;

                if (fieldType == typeof(Vector4) || fieldType == typeof(Vector3) || fieldType == typeof(Vector2))
                {
                    shader.SetVector(shaderVariableName, (Vector4)field.GetValue(settings));
                }
                else if (fieldType == typeof(int))
                {
                    shader.SetInt(shaderVariableName, (int)field.GetValue(settings));
                }
                else if (field.FieldType == typeof(float))
                {
                    shader.SetFloat(shaderVariableName, (float)field.GetValue(settings));
                }
                else
                {
                    Debug.Log($"Type {fieldType} not implemented");
                }
            }
        }

        public static void CreateStructuredBuffer<T>(ref ComputeBuffer buffer, int count)
        {
            int stride = System.Runtime.InteropServices.Marshal.SizeOf(typeof(T));
            bool createNewBuffer = buffer == null || !buffer.IsValid() || buffer.count != count || buffer.stride != stride;
            if (createNewBuffer)
            {
                Release(buffer);
                buffer = new ComputeBuffer(count, stride);
            }
        }

        public static void CreateStructuredBuffer<T>(ref ComputeBuffer buffer, T[] data)
        {
            CreateStructuredBuffer<T>(ref buffer, data.Length);
            buffer.SetData(data);
        }

        // Test

        public static void CreateAndSetBuffer<T>(ref ComputeBuffer buffer, T[] data, ComputeShader cs, string nameID, int kernelIndex = 0)
        {
            int stride = System.Runtime.InteropServices.Marshal.SizeOf(typeof(T));
            CreateStructuredBuffer<T>(ref buffer, data.Length);
            buffer.SetData(data);
            cs.SetBuffer(kernelIndex, nameID, buffer);
        }

        public static ComputeBuffer CreateAndSetBuffer<T>(int length, ComputeShader cs, string nameID, int kernelIndex = 0)
        {
            ComputeBuffer buffer = null;
            CreateAndSetBuffer<T>(ref buffer, length, cs, nameID, kernelIndex);
            return buffer;
        }

        public static void CreateAndSetBuffer<T>(ref ComputeBuffer buffer, int length, ComputeShader cs, string nameID, int kernelIndex = 0)
        {
            CreateStructuredBuffer<T>(ref buffer, length);
            cs.SetBuffer(kernelIndex, nameID, buffer);
        }

        /// <summary>
        /// Released supplied buffer/s if not null
        /// </summary>
        public static void Release(params ComputeBuffer[] buffers)
        {
            foreach (var buffer in buffers)
            {
                if (buffer != null)
                {
                    buffer.Release();
                }
            }
        }

        public static Vector3Int GetThreadGroupSizes(ComputeShader compute, int kernelIndex = 0)
        {
            uint x, y, z;
            compute.GetKernelThreadGroupSizes(kernelIndex, out x, out y, out z);
            return new Vector3Int((int)x, (int)y, (int)z);
        }

        public static void CreateRenderTexture(ref RenderTexture texture, int size, FilterMode filterMode = FilterMode.Bilinear, GraphicsFormat format = GraphicsFormat.R16G16B16A16_SFloat)
        {
            CreateRenderTexture(ref texture, size, size, filterMode, format);
        }

        public static void CreateRenderTexture(ref RenderTexture texture, int width, int height, FilterMode filterMode = FilterMode.Bilinear, GraphicsFormat format = GraphicsFormat.R16G16B16A16_SFloat)
        {
            if (texture == null || !texture.IsCreated() || texture.width != width || texture.height != height || texture.graphicsFormat != format)
            {
                if (texture != null)
                {
                    texture.Release();
                }
                texture = new RenderTexture(width, height, 0);
                texture.graphicsFormat = format;
                texture.enableRandomWrite = true;

                texture.autoGenerateMips = false;
                texture.Create();
            }
            texture.wrapMode = TextureWrapMode.Clamp;
            texture.filterMode = filterMode;
        }

        // https://cmwdexint.com/2017/12/04/computeshader-setfloats/
        public static float[] PackFloats(params float[] values)
        {
            float[] packed = new float[values.Length * 4];
            for (int i = 0; i < values.Length; i++)
            {
                packed[i * 4] = values[i];
            }
            return packed;
        }

        // Editor Helpers

#if UNITY_EDITOR
        static UnityEditor.PlayModeStateChange playModeState;
#endif

        static ComputeHelper()
        {
            // Monitor play mode state
            UnityEditor.EditorApplication.playModeStateChanged -= MonitorPlayModeState;
            UnityEditor.EditorApplication.playModeStateChanged += MonitorPlayModeState;
            // Monitor script compilation
            UnityEditor.Compilation.CompilationPipeline.compilationStarted -= OnCompilationStarted;
            UnityEditor.Compilation.CompilationPipeline.compilationStarted += OnCompilationStarted;
        }

        static void MonitorPlayModeState(UnityEditor.PlayModeStateChange state)
        {
            playModeState = state;
            if (state == UnityEditor.PlayModeStateChange.ExitingEditMode)
            {
                ShouldReleaseEditModeBuffers?.Invoke();
            }
        }

        static void OnCompilationStarted(System.Object obj)
        {
            ShouldReleaseEditModeBuffers?.Invoke();
        }

        static bool CheckIfCanRunInEditMode()
        {
            bool isCompilingOrExitingEditMode = false;
#if UNITY_EDITOR
            isCompilingOrExitingEditMode |= UnityEditor.EditorApplication.isCompiling;
            isCompilingOrExitingEditMode |= playModeState == UnityEditor.PlayModeStateChange.ExitingEditMode;
#endif

            return !isCompilingOrExitingEditMode;
        }
    }
}