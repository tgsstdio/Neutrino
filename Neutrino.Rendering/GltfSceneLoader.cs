using glTFLoader;
using glTFLoader.Schema;
using Magnesium;
using Magnesium.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace Neutrino
{
    public class GltfSceneLoader : IGltfSceneLoader
    {
        private readonly IMgGraphicsConfiguration mConfiguration;
        private readonly MgOptimizedStorageBuilder mBuilder;
        public GltfSceneLoader(
            IMgGraphicsConfiguration config,
            MgOptimizedStorageBuilder builder
        ) {
            mConfiguration = config;
            mBuilder = builder;
        }

        public void Load(string modelFilePath)
        {
            var model = Interface.LoadModel(modelFilePath);
            var baseDir = Path.GetDirectoryName(modelFilePath);

            var buffers = ExtractBuffers(model, baseDir);

            var request = new MgStorageBlockAllocationRequest();

            ExtractCameras(model.Cameras, request);
        }

        public static void ExtractCameras(glTFLoader.Schema.Camera[] cameras, MgStorageBlockAllocationRequest request)
        {
            const int CAMERA_BUCKET_SIZE = 16;
            if (cameras != null)
            {
                var noOfBuckets = cameras.Length / CAMERA_BUCKET_SIZE;
                var remainder = cameras.Length % CAMERA_BUCKET_SIZE;

                for (var i = 0; i < noOfBuckets; i += 1)
                {
                    InsertCameraBucket(request, CAMERA_BUCKET_SIZE);
                }

                if (remainder > 0)
                {
                    InsertCameraBucket(request, CAMERA_BUCKET_SIZE);
                }
            }
        }

        private static void InsertCameraBucket(MgStorageBlockAllocationRequest request, int CAMERA_BUCKET_SIZE)
        {
            int UBO_STRIDE = Marshal.SizeOf(typeof(CameraUBO));
            ulong UBO_BUCKET_SIZE = (ulong)(UBO_STRIDE * CAMERA_BUCKET_SIZE);
            request.Insert(new MgStorageBlockAllocationInfo
            {
                Usage = MgBufferUsageFlagBits.UNIFORM_BUFFER_BIT,
                MemoryPropertyFlags = MgMemoryPropertyFlagBits.HOST_VISIBLE_BIT,
                Size = UBO_BUCKET_SIZE,
                ElementByteSize = 0,
            });
        }

        private static List<byte[]> ExtractBuffers(glTFLoader.Schema.Gltf model, string baseDir)
        {
            var buffers = new List<byte[]>();
            foreach (var selectedBuffer in model.Buffers)
            {
                if (selectedBuffer.Uri.StartsWith("data:") && DataURL.FromUri(selectedBuffer.Uri, out DataURL output))
                {
                    if (output.Data.Length != selectedBuffer.ByteLength)
                    {
                        throw new InvalidDataException($"The specified length of embedded data chunk ({selectedBuffer.ByteLength}) is not equal to the actual length of the data chunk ({output.Data.Length}).");
                    }
                    buffers.Add(output.Data);
                }
                else
                {
                    // OPEN FILE
                    string additionalFilePath = System.IO.Path.Combine(baseDir, selectedBuffer.Uri);
                    using (var fs = File.Open(additionalFilePath, FileMode.Open))
                    using (var br = new BinaryReader(fs))
                    {
                        // ONLY READ SPECIFIED CHUNK SIZE
                        buffers.Add(br.ReadBytes(selectedBuffer.ByteLength));
                    }
                }
            }
            return buffers;
        }

  
    }
}
