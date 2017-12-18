using Magnesium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GltfDemo
{
    class OptimizedMeshBinder
    {
        void InitPipeline()
        {
            MgOptimizedMesh mesh = null;

            // LOOK LIKE IT IS WILL BE EASIER TO GET THIS INFO FROM GLTF MODEL FILE
            var attributes = new List<MgVertexInputAttributeDescription>();
            var firstBinding = 0U;
            foreach(var attr in mesh.Allocations)
            {
                var elem = new MgVertexInputAttributeDescription
                {
                    Location = attr.Index, // FIXED
                    Binding = firstBinding + attr.BlockIndex,
                    Format = MgFormat.UNDEFINED, // NEED 
                    Offset = (uint) attr.ByteOffset,
                };
                attributes.Add(elem);
            }

            var bindings = new List<MgVertexInputBindingDescription>();
            for(var i = 0U; i < mesh.Instances.Length; i += 1)
            {
                var current = mesh.Instances[i];

                var stride = 0U;
                foreach (var attrIndex in current.PackingOrder)
                {
                    var attr = mesh.Allocations[attrIndex];
                    // BUFFERVIEW IS A BETTER MATCH
                    if ((attr.Usage & MgBufferUsageFlagBits.VERTEX_BUFFER_BIT) == MgBufferUsageFlagBits.VERTEX_BUFFER_BIT)
                    {
                        // ELEMENT COUNT
                        stride += 0U;
                    }
                }

                var elem = new MgVertexInputBindingDescription
                {
                    Binding = firstBinding + i,
                    InputRate = MgVertexInputRate.VERTEX,
                    Stride = stride,
                };
                bindings.Add(elem);
            }

            IMgDevice device = null;

            IMgPipeline[] pPipelines;
            var pCreateInfos = new[] {
                new MgGraphicsPipelineCreateInfo{
                    InputAssemblyState = new MgPipelineInputAssemblyStateCreateInfo
                    {
                        Topology = MgPrimitiveTopology.TRIANGLE_LIST,
                    },
                    VertexInputState = new MgPipelineVertexInputStateCreateInfo
                    {
                        VertexBindingDescriptions = bindings.ToArray(),
                        VertexAttributeDescriptions = attributes.ToArray(),
                    }                    
                }
            };
            device.CreateGraphicsPipelines(null, pCreateInfos, null, out pPipelines);
        }
    }
}
