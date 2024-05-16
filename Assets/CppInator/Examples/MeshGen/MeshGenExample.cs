using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using AOT;
using CppInator.Runtime;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace CppInator.Examples
{
    public class MeshGenExample : MonoBehaviour
    {
        [DllImport("__Internal")]
        static extern void GenerateMeshExample(int requestId, int seed, MeshGenCallback cb);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        delegate void MeshGenCallback(int requestId, int vertexCount, int indexCount, IntPtr vertices, IntPtr normals,
            IntPtr uvs, IntPtr indices);

        [MonoPInvokeCallback(typeof(MeshGenCallback))]
        static void ReceivedMeshData(int requestId, int vertexCount, int indexCount, IntPtr vertices, IntPtr normals,
            IntPtr uvs, IntPtr indices)
        {
            var verts = MarshalExtensions.PtrToArray<Vector3>(vertices, vertexCount);
            var norms = MarshalExtensions.PtrToArray<Vector3>(normals, vertexCount);
            var uv = MarshalExtensions.PtrToArray<Vector2>(uvs, vertexCount);
            var ind = MarshalExtensions.PtrToArray<int>(indices, indexCount);

            if (_callbacksData.TryGetValue(requestId, out var mesh))
            {
                mesh.Clear();

                mesh.SetVertices(verts);
                mesh.SetNormals(norms);
                mesh.SetUVs(0, uv);
                mesh.SetIndices(ind, MeshTopology.Triangles, 0);
                mesh.UploadMeshData(false);

                _callbacksData.Remove(requestId);
                _timer.Stop();

                int minPossibleMs = Mathf.FloorToInt(Time.deltaTime * 1000);
                Debug.Log($"Mesh generation took {_timer.ElapsedMilliseconds}ms (min possible time: {minPossibleMs})");

                _timer.Reset();
                _isRunning = false;
            }
        }

        static readonly Dictionary<int, Mesh> _callbacksData = new();

        static int _requestId;

        [SerializeField] private int _seed = 69;

        [SerializeField] private MeshFilter _meshFilter;

        static bool _isRunning;

        static readonly Stopwatch _timer = new();

        private void QueueMeshGen(Mesh mesh)
        {
            _isRunning = true;
            _timer.Start();
            _callbacksData.Add(_requestId, mesh);

            Native.Invoke(GenerateMeshExample, _requestId++, _seed, (MeshGenCallback)ReceivedMeshData);
        }

        private void Start()
        {
            var mesh = new Mesh();
            _meshFilter.sharedMesh = mesh;

            QueueMeshGen(mesh);
        }

        private void Update()
        {
            if (_isRunning) return;

            if (Input.GetKeyDown(KeyCode.Space))
                QueueMeshGen(_meshFilter.sharedMesh);
        }
    }
}