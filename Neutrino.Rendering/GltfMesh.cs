using glTFLoader.Schema;
using Magnesium;
using System;

namespace Neutrino
{
    public class GltfMesh
    {
        public string Name { get; set; }
        public float[] Weights { get; }
        public GltfMeshPrimitive[] Primitives { get; private set; }

        public GltfMesh(Mesh mesh, GltfAccessor[] accessors, GltfBucketContainer materials)
        {
            Name = mesh.Name;
            Weights = mesh.Weights;
            Primitives = InitializePrimitives(mesh, accessors, materials);
        }

        private static GltfMeshPrimitive[] InitializePrimitives(
            Mesh mesh, 
            GltfAccessor[] accessors,
            GltfBucketContainer materials)
        {
            var noOfItems = mesh.Primitives != null ? mesh.Primitives.Length : 0;
            var output = new GltfMeshPrimitive[noOfItems];

            for (var i = 0; i < noOfItems; i += 1)
            {
                var srcPrimitive = mesh.Primitives[i];

                var vertexLocations = new PerVertexDataLocator(
                    srcPrimitive.Indices, 
                    srcPrimitive.Attributes);
                var definition = PerVertexDefinitionEncoder.Extract(
                    vertexLocations,
                    accessors);

                var temp = new GltfMeshPrimitive
                {
                    Topology = DetermineTopology(srcPrimitive.Mode),
                    VertexLocations = vertexLocations,
                    InitialDefinition = definition,
                    Material = materials.GetAllocation(srcPrimitive.Material),
                    VertexCount =
                        vertexLocations.Position.HasValue
                        ? (uint) accessors[vertexLocations.Position.Value].ElementCount
                        : 0U
                };                               

                output[i] = temp;
            }

            return output;
        }        

        private static MgPrimitiveTopology DetermineTopology(MeshPrimitive.ModeEnum mode)
        {
            switch (mode) {
                case MeshPrimitive.ModeEnum.LINES:
                    return MgPrimitiveTopology.LINE_LIST;
                case MeshPrimitive.ModeEnum.LINE_STRIP:
                    return MgPrimitiveTopology.LINE_STRIP;
                case MeshPrimitive.ModeEnum.POINTS:
                    return MgPrimitiveTopology.POINT_LIST;
                case MeshPrimitive.ModeEnum.TRIANGLES:
                    return MgPrimitiveTopology.TRIANGLE_LIST;
                case MeshPrimitive.ModeEnum.TRIANGLE_FAN:
                    return MgPrimitiveTopology.TRIANGLE_FAN;
                case MeshPrimitive.ModeEnum.TRIANGLE_STRIP:
                    return MgPrimitiveTopology.TRIANGLE_STRIP;
                default:
                    throw new NotSupportedException();
            }
        }
    }
}
