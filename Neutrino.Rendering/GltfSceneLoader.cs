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
        private readonly IPbrEffectPath mPbrEffectPath;

        public GltfSceneLoader(
            IMgGraphicsConfiguration config,
            MgOptimizedStorageBuilder builder,
            IPbrEffectPath pbrEffectPath
        ) {
            mConfiguration = config;
            mBuilder = builder;
            mPbrEffectPath = pbrEffectPath;
        }

        public GltfScene Load(IMgDevice device, IMgEffectFramework framework, string modelFilePath)
        {
            var model = Interface.LoadModel(modelFilePath);
            var baseDir = Path.GetDirectoryName(modelFilePath);

            var buffers = ExtractBuffers(model, baseDir);

            var request = new MgStorageBlockAllocationRequest();

            const int MAX_NO_OF_CAMERAS = 1;
            var cameraAllocationInfo = new GltfBucketAllocationInfo<CameraUBO>
            {
                BucketSize = MAX_NO_OF_CAMERAS,
                Usage = MgBufferUsageFlagBits.UNIFORM_BUFFER_BIT,
                MemoryPropertyFlags = MgMemoryPropertyFlagBits.HOST_VISIBLE_BIT,
            };

            var cameraSlots = cameraAllocationInfo.Prepare(model.Cameras != null ? model.Cameras.Length : 0, request);

            var accessors = ExtractAccessors(model);

            var bufferViews = ExtractBufferViews(model);

            const int MAX_NO_OF_MATERIALS = 16;

            var materialAllocationInfo = new GltfBucketAllocationInfo<MaterialUBO>
            {
                BucketSize = MAX_NO_OF_MATERIALS,
                Usage = MgBufferUsageFlagBits.UNIFORM_BUFFER_BIT,
                MemoryPropertyFlags = MgMemoryPropertyFlagBits.HOST_VISIBLE_BIT,
            };

            var images = ExtractImages(baseDir, model.Images, bufferViews, buffers);

            var samplers = ExtractSamplers(device, model.Samplers);

            const int MAX_NO_OF_TEXTURES = 16;
           // var textures = AllocateTextures(MAX_NO_OF_TEXTURES, model.Textures, images);

            var materialSlots = materialAllocationInfo.Prepare(model.Materials != null ? model.Materials.Length : 0, request);

            var materialChunks = ExtractMaterials(materialSlots.BucketSize, model.Materials);

            var meshes = ExtractMeshes(model, accessors, materialSlots);
            PadMeshes(meshes);

            var meshLocations = AllocateMeshes(request, meshes, accessors, bufferViews);

            var nodes = ExtractNodes(model, cameraSlots);

            var pDsCreateInfo = new MgDescriptorSetLayoutCreateInfo
            {
                Bindings = new[]
                {
                    // CAMERA
                    new MgDescriptorSetLayoutBinding
                    {
                        Binding = 0,
                        DescriptorType = MgDescriptorType.UNIFORM_BUFFER,
                        DescriptorCount = MAX_NO_OF_CAMERAS,
                        StageFlags = MgShaderStageFlagBits.VERTEX_BIT,
                    },
                    // MATERIALS
                    new MgDescriptorSetLayoutBinding
                    {
                        Binding = 1,
                        DescriptorType = MgDescriptorType.UNIFORM_BUFFER,
                        DescriptorCount = MAX_NO_OF_MATERIALS,
                        StageFlags = MgShaderStageFlagBits.FRAGMENT_BIT,
                    },
                    // TEXTURES
                    new MgDescriptorSetLayoutBinding
                    {
                        Binding = 2,
                        DescriptorType = MgDescriptorType.COMBINED_IMAGE_SAMPLER,
                        DescriptorCount = MAX_NO_OF_TEXTURES,
                        StageFlags = MgShaderStageFlagBits.FRAGMENT_BIT,
                    },
                }
            };

            var err = device.CreateDescriptorSetLayout(pDsCreateInfo, null, out IMgDescriptorSetLayout dsLayout);
            if (err != Result.SUCCESS)
                throw new InvalidOperationException("CreatePipelineLayout failed");

            var pCreateInfo = new MgPipelineLayoutCreateInfo
            {
                SetLayouts = new[]
                {
                    dsLayout,
                }
            };

            err = device.CreatePipelineLayout(pCreateInfo, null, out IMgPipelineLayout layout);
            if (err != Result.SUCCESS)
                throw new InvalidOperationException("CreatePipelineLayout failed");

            var pbrEffect = new EffectPipelineDictionary();
            var pbrFactory = new PbrEffectVariantFactory(mPbrEffectPath);

            var instanceDrawGroups = new Dictionary<GltfInstancedGroupKey, GltfInstanceDrawGroup>();
            foreach (var node in nodes)
            {
                if (node.Mesh.HasValue)
                {
                    var options = new EffectVariantOptions
                    {

                    };

                    var meshIndex = node.Mesh.Value;
                    var mesh = meshes[meshIndex];

                    options.FrontFace =
                        node.IsMirrored
                        ? MgFrontFace.CLOCKWISE
                        : MgFrontFace.COUNTER_CLOCKWISE;

                    foreach (var primitive in mesh.Primitives)
                    {
                        options.Topology = primitive.Topology;

                        var materialItem = materialChunks[primitive.Material.BucketIndex].Items[primitive.Material.Offset];

                        options.CullMode =
                            materialItem.DoubleSided
                            ? MgCullModeFlagBits.NONE
                            : MgCullModeFlagBits.BACK_BIT;

                        var key = new EffectVariantKey
                        {
                            Definition = PerVertexDefinitionEncoder.Encode(primitive.FinalDefinition),
                            Options = EffectVariantEncoder.Encode(options),
                        };

                        if (!pbrEffect.TryGetValue(key, out EffectVariant found))
                        {
                            var vertexInput = new PerVertexInputPipelineState(primitive.FinalDefinition);
                            found = pbrFactory.Initialize(device, layout, framework.Renderpass, vertexInput, options);

                            pbrEffect.Add(key, found);
                        }

                        var groupKey = new GltfInstancedGroupKey
                        {
                            MeshIndex = meshIndex,
                            CameraSlotIndex = 0,
                            TextureSlotIndex = 0,
                            MaterialSlotIndex = primitive.Material.StorageIndex,

                            VariantKey = key,
                        };

                        if (!instanceDrawGroups.TryGetValue(groupKey, out GltfInstanceDrawGroup drawGroup))
                        {
                            drawGroup = new GltfInstanceDrawGroup
                            {
                                GroupKey = groupKey,
                                Variant = found,
                                Members = new List<GltfInstancedDraw>(),
                            };

                            instanceDrawGroups.Add(groupKey, drawGroup);
                        }

                        var instancedDraw = new GltfInstancedDraw
                        {
                            Key = key,
                            GroupKey = groupKey,
                            Instance = new PerInstance
                            {
                                Translation = node.Transform.ExtractTranslation(),
                                Scale = node.Transform.ExtractScale(),
                                Rotation = new TkVector4(1, 1, 1, 1), // TODO 
                                MaterialIndex = (uint)primitive.Material.Offset,
                            },
                        };

                        drawGroup.Members.Add(instancedDraw);
                    }
                }
            }

            var stride = Marshal.SizeOf(typeof(PerInstance));

            var perInstances = new List<GltfInstanceRenderGroup>();
            foreach (var group in instanceDrawGroups.Values)
            {
                var slotInfo = new MgStorageBlockAllocationInfo
                {
                    MemoryPropertyFlags = MgMemoryPropertyFlagBits.HOST_VISIBLE_BIT,
                    Usage = MgBufferUsageFlagBits.VERTEX_BUFFER_BIT,
                    ElementByteSize = (uint)stride,
                    Size = (ulong)(group.Members.Count * stride),
                };

                var instanceGroup = new GltfInstanceRenderGroup
                {
                    Variant = group.Variant,
                    StorageIndex = request.Insert(slotInfo),
                    Members = group.Members.ToArray(),
                };
                perInstances.Add(instanceGroup);
            }

            var storage = mBuilder.Build(
                new MgOptimizedStorageCreateInfo
                {
                    Allocations = request.ToArray()
                }
            );

            // MAP DATA

            var finalMaterials = new List<GltfMaterialInfo>();

            for (var i = 0; i < materialChunks.Length; i += 1)
            {
                var slot = materialSlots.Slots[i];
                var chunk = materialChunks[i];
                for(var j = 0; j < chunk.Items.Length; j += 1)
                {                    
                    var values = chunk.Items[j];
                    var mat = new GltfMaterialInfo
                    {                        
                        StorageIndex = slot,                        

                        BucketIndex = i,
                        SetOffset = j,
                        Values = values,
                    };
                }
            }

            return new GltfScene
            {
                //Cameras = new []
                //{

                //},
                DescriptorSets = new IMgDescriptorSet[]
                {

                },
                Effects = new []
                {
                    new Effect
                    {
                        DescriptorSetLayout = dsLayout,
                        Layout = layout,
                        Variants = pbrEffect,
                    }
                },
                //Meshes = new[]
                //{

                //},
                Materials = finalMaterials.ToArray(),
                //Nodes = new []
                //{

                //},
                PerInstances = perInstances.ToArray(),
                Samplers = samplers,
                Storage = storage,
                //Textures = new []
                //{

                //},
            };
        }

        //private GltfTextureContainer AllocateTextures(int bucketSize, Texture[] textures, GltfImageData[] images)
        //{           
        //    return new GltfTextureContainer
        //    {
        //        BucketSize = bucketSize,                
        //    };
        //}

        private IMgSampler[] ExtractSamplers(IMgDevice device, Sampler[] samplers)
        {
            var noOfSamplers = samplers != null ? samplers.Length : 0;

            var output = new IMgSampler[noOfSamplers];
            for (var i = 0; i < noOfSamplers; i += 1)
            {
                var src = samplers[i];

                var createInfo = new MgSamplerCreateInfo
                {                    
                    AddressModeU = GetAddressModeU(src.WrapS),
                    AddressModeV = GetAddressModeV(src.WrapT),
                    MinFilter = GetMinFilter(src.MinFilter),
                    MagFilter = GetMagFilter(src.MagFilter),
                    MipmapMode = GetMipmapMode(src.MinFilter),
                };

                var err = device.CreateSampler(createInfo, null, out IMgSampler pSampler);
                if (err != Result.SUCCESS)
                    throw new InvalidOperationException();
                output[i] = pSampler;
            }
            return output;
        }

        private MgSamplerAddressMode GetAddressModeV(Sampler.WrapTEnum wrapT)
        {
            switch(wrapT)
            {
                case Sampler.WrapTEnum.CLAMP_TO_EDGE:
                    return MgSamplerAddressMode.CLAMP_TO_EDGE;
                case Sampler.WrapTEnum.MIRRORED_REPEAT:
                    return MgSamplerAddressMode.MIRRORED_REPEAT;
                case Sampler.WrapTEnum.REPEAT:
                    return MgSamplerAddressMode.REPEAT;
                default:
                    throw new InvalidOperationException();
            }
        }

        private MgSamplerAddressMode GetAddressModeU(Sampler.WrapSEnum wrapS)
        {
            switch(wrapS)
            {
                case Sampler.WrapSEnum.CLAMP_TO_EDGE:
                    return MgSamplerAddressMode.CLAMP_TO_EDGE;
                case Sampler.WrapSEnum.MIRRORED_REPEAT:
                    return MgSamplerAddressMode.MIRRORED_REPEAT;
                case Sampler.WrapSEnum.REPEAT:
                    return MgSamplerAddressMode.REPEAT;
                default:
                    throw new InvalidOperationException();
            }
        }

        private static MgSamplerMipmapMode GetMipmapMode(Sampler.MinFilterEnum? minFilter)
        {
            if (minFilter.HasValue)
            {
                switch (minFilter.Value)
                {
                    case Sampler.MinFilterEnum.LINEAR:
                    case Sampler.MinFilterEnum.NEAREST_MIPMAP_LINEAR:
                    case Sampler.MinFilterEnum.LINEAR_MIPMAP_LINEAR:
                        return MgSamplerMipmapMode.LINEAR;

                    case Sampler.MinFilterEnum.NEAREST:    
                    case Sampler.MinFilterEnum.NEAREST_MIPMAP_NEAREST:
                    case Sampler.MinFilterEnum.LINEAR_MIPMAP_NEAREST:
                        return MgSamplerMipmapMode.NEAREST;
                    default:
                        throw new InvalidOperationException();
                }
            }
            else
            {
                return MgSamplerMipmapMode.NEAREST;
            }
        }

        private static MgFilter GetMinFilter(Sampler.MinFilterEnum? minFilter)
        {
            if (minFilter.HasValue)
            {
                switch (minFilter.Value)
                {
                    case Sampler.MinFilterEnum.LINEAR:
                    case Sampler.MinFilterEnum.LINEAR_MIPMAP_LINEAR:
                    case Sampler.MinFilterEnum.LINEAR_MIPMAP_NEAREST:
                        return MgFilter.LINEAR;

                    case Sampler.MinFilterEnum.NEAREST:
                    case Sampler.MinFilterEnum.NEAREST_MIPMAP_NEAREST:
                    case Sampler.MinFilterEnum.NEAREST_MIPMAP_LINEAR:
                        return MgFilter.NEAREST;
                    default:
                        throw new InvalidOperationException();
                }
            }        
            else
            {
                return MgFilter.LINEAR;
            }
        }

        private static MgFilter GetMagFilter(Sampler.MagFilterEnum? magFilter)
        {
            if (magFilter.HasValue)
            {
                switch(magFilter.Value)
                {
                    case Sampler.MagFilterEnum.LINEAR:
                        return MgFilter.LINEAR;
                    case Sampler.MagFilterEnum.NEAREST:
                        return MgFilter.NEAREST;
                    default:
                        throw new NotSupportedException();
                }
            }
            else
            {
                return MgFilter.LINEAR;
            }
        }

        public class GltfMaterialInfo
        {
            public uint? SourceIndex { get; set; }

            public int StorageIndex { get; set; }

            public int SetIndex { get; set; }

            public int BucketIndex { get; set; }
            public int SetOffset { get; set; }

            public GltfMaterialCapsule Values { get; set; }
        }

        public class GltfInstanceDrawGroup
        {
            public GltfInstancedGroupKey GroupKey { get; set; }
            public List<GltfInstancedDraw> Members { get; set; }
            public EffectVariant Variant { get; internal set; }
        }

        public class GltfInstanceRenderGroup
        {
            public GltfInstancedGroupKey GroupKey { get; set; }
            public EffectVariant Variant { get; internal set; }
            public int StorageIndex { get; set; }
            public GltfInstancedDraw[] Members { get; set; }
        }

        private GltfImageData[] ExtractImages(string baseDir, Image[] images, GltfBufferView[] bufferViews, List<byte[]> buffers)
        {
            var count = images != null ? images.Length : 0;

            var output = new GltfImageData[count];

            for (var i = 0; i < count; i += 1)
            {
                var currentImage = images[i];

                byte[] src = null;
                ulong srcOffset = 0;
                ulong srcLength = 0UL;
                string userDefMimeType = null;
                if (currentImage.BufferView.HasValue)
                {
                    var view = bufferViews[currentImage.BufferView.Value];
                    src = buffers[view.BufferIndex];
                    srcOffset = (ulong)view.BufferOffset;
                    srcLength = (ulong)view.ByteLength;
                }
                else if (!string.IsNullOrWhiteSpace(currentImage.Uri))
                {
                    if (DataURL.FromUri(currentImage.Uri, out DataURL urlData))
                    {
                        userDefMimeType = urlData.MediaType;
                        src = urlData.Data;
                        srcOffset = 0UL;
                        srcLength = (ulong)urlData.Data.Length;
                    }
                    else
                    {
                        // OPEN FILE
                        userDefMimeType = Path.GetExtension(currentImage.Uri).ToUpperInvariant();

                        string additionalFilePath = System.IO.Path.Combine(baseDir, currentImage.Uri);
                        using (var fs = File.Open(additionalFilePath, FileMode.Open))
                        using (var ms = new MemoryStream())
                        {
                            fs.CopyTo(ms);
                            src = ms.ToArray();
                            srcOffset = 0UL;
                            srcLength = (ulong)src.Length;
                        }
                    }
                }

                GltfImageMimeType mimeType =
                    currentImage.MimeType.HasValue
                    ? GetImageTypeFromJSON(currentImage.MimeType.Value)
                    : GetImageTypeFromStr(userDefMimeType);

                var temp = new GltfImageData
                {
                    Name = currentImage.Name,
                    MimeType = mimeType,
                    Source = src,
                    SrcOffset = srcOffset,
                    SrcLength = srcLength,
                };

                output[i] = temp;
            }

            return output;
        }

        private GltfImageMimeType GetImageTypeFromStr(string userDefMimeType)
        {
            switch(userDefMimeType)
            {
                case "":
                default:
                    throw new InvalidOperationException("image type invalid");                
                case ".PNG":
                case "image/png":
                    return GltfImageMimeType.PNG;
                case ".JPG":
                case ".JPEG":
                case "image/jpeg":
                    return GltfImageMimeType.JPEG;
            }
        }

        private static GltfImageMimeType GetImageTypeFromJSON(Image.MimeTypeEnum mimeType)
        {
            switch (mimeType)
            {
                case Image.MimeTypeEnum.image_jpeg:
                    return GltfImageMimeType.JPEG;
                case Image.MimeTypeEnum.image_png:
                    return GltfImageMimeType.PNG;
                default:
                    throw new InvalidOperationException("image type invalid");
            }
        }

        //public GltfTextureInfo[] ExtractTextures(uint bucketSize, Texture[] textures, Sampler[] samples, Image[] images)
        //{
        //    var count = textures != null ? textures.Length : 0;
        //    var noOfBuckets = (count / bucketSize) + 1;

        //    var chunks = new GltfTextureInfo[noOfBuckets];

        //    for (var i = 0; i < count; i += 1)
        //    {
        //        var current = textures[i];

        //        current.Source.

        //        var info = new GltfTextureInfo
        //        {
                    
        //        };

        //        chunks[i] = info;
        //    }

        //    return chunks;
        //}

        private GltfMaterialChunk[] ExtractMaterials(int bucketSize, Material[] materials)
        {
            var noOfMaterials = materials != null ? materials.Length : 0;
            var noOfBuckets = (noOfMaterials / bucketSize) + 1;

            var chunks = new GltfMaterialChunk[noOfBuckets];

            var k = 0;
            for(var i =0; i < noOfBuckets; i += 1)
            {
                chunks[i] = new GltfMaterialChunk
                {
                    Items = new GltfMaterialCapsule[bucketSize],
                };

                for (var j = 0; j < bucketSize; j += 1)
                {
                    if (i == 0 && j == 0)
                    {
                        chunks[i].Items[j] = new GltfMaterialCapsule
                        {
                            DoubleSided = false,
                            UBO = new MaterialUBO
                            {
                                BaseTexture = 0,
                                BaseTextureTexCoords = 0,
                                BaseColorFactor = new MgVec4f(1f, 1f, 1f, 1f),
                                MetallicFactor = 1f,
                                RoughnessFactor = 1f,

                                NormalTexture = 0,
                                NormalTexCoords = 0,
                                NormalScale = 1f,

                                OcclusionTexture = 0,
                                OcclusionTexCoords = 0,
                                OcclusionStrength = 1f,

                                EmissiveFactor = new Color3f { R = 0f, G = 0f, B = 0f },
                                AlphaCutoff = 0.5f,

                                // OPAQUE
                                A = 1.0f, //  A * vec4(fragColor.rgb, 1f))
                                B = 0f, //  + B * vec4(step(alphacutoff, fragColor.a) * fragColor.rgb, 1f)
                                C = 0f, //  + C * vec4(fragColor.rgb, fragColor.a)
                            }
                        };
                    }
                    else
                    {
                        if (k < noOfMaterials)
                        {
                            var src = materials[k];

                            var temp = new GltfMaterialCapsule
                            {              
                                DoubleSided = src.DoubleSided,
                            };

                            var baseColorFactor = new MgVec4f(1f, 1f, 1f, 1f);
                            if (src.PbrMetallicRoughness != null)
                            {
                                baseColorFactor = new MgVec4f
                                {
                                    X = src.PbrMetallicRoughness.BaseColorFactor[0],
                                    Y = src.PbrMetallicRoughness.BaseColorFactor[1],
                                    Z = src.PbrMetallicRoughness.BaseColorFactor[2],
                                    W = src.PbrMetallicRoughness.BaseColorFactor[3],
                                };
                            }

                            ushort normalTexCoords = 0;
                            ushort normalTexSetOffset = 0;
                            float normalScale = 1f;
                            if (src.NormalTexture != null)
                            {
                                normalScale = src.NormalTexture.Scale;
                                normalTexSetOffset = (ushort) src.NormalTexture.Index;
                                normalTexCoords = (ushort) src.NormalTexture.TexCoord;
                            }

                            temp.UBO = new MaterialUBO
                            {
                                BaseColorFactor = baseColorFactor,
                                EmissiveFactor = (src.EmissiveFactor != null)
                                    ? new Color3f {
                                        R = src.EmissiveFactor[0],
                                        G = src.EmissiveFactor[1],
                                        B = src.EmissiveFactor[2]
                                    }
                                    : new Color3f { R = 0f, G = 0f, B = 0f },
                                NormalScale = normalScale,
                                NormalTexCoords = normalTexCoords,
                                NormalTexture = normalTexSetOffset,
                            };


                            chunks[i].Items[j] = temp;

                            k += 1;
                        }
                    }
                }
            }

            // Default - chunk 0, slot 0


            return chunks;
        }

        PerVertexDefinition DEFAULT_PADDING = new PerVertexDefinition
        {
            Position = PerVertexPositionType.Float3,
            Normal = PerVertexNormalType.Float3,
            Tangent = PerVertexTangentType.Float4,
            TexCoords0 = PerVertexTexCoordsType.Float2,
            TexCoords1 = PerVertexTexCoordsType.Float2,
            Color0 = PerVertexColorType.FloatRGBA,
            Color1 = PerVertexColorType.FloatRGBA,
            Joints0 = PerVertexJointType.Byte4,
            Joints1 = PerVertexJointType.Byte4,
            Weights0 = PerVertexWeightsType.Float4,
            Weights1 = PerVertexWeightsType.Float4,
        };

        private void PadMeshes(GltfMesh[] meshes)
        {
            var noOfMeshes = meshes.Length;
            for (var i = 0; i < noOfMeshes; i += 1)
            {
                var currentMesh = meshes[i];

                foreach (var primitive in currentMesh.Primitives)
                {
                    var modelDefinition = primitive.InitialDefinition;
                    primitive.FinalDefinition = new PerVertexDefinition {
                        Position =
                            (modelDefinition.Position == PerVertexPositionType.None)
                            ? DEFAULT_PADDING.Position
                            : modelDefinition.Position,
                        Normal =
                            (modelDefinition.Normal == PerVertexNormalType.None)
                            ? DEFAULT_PADDING.Normal
                            : modelDefinition.Normal,
                        Tangent =
                            (modelDefinition.Tangent == PerVertexTangentType.None)
                            ? DEFAULT_PADDING.Tangent
                            : modelDefinition.Tangent,
                        TexCoords0 = (modelDefinition.TexCoords0 == PerVertexTexCoordsType.None)
                            ? DEFAULT_PADDING.TexCoords0
                            : modelDefinition.TexCoords0,
                        TexCoords1 = (modelDefinition.TexCoords1 == PerVertexTexCoordsType.None)
                            ? DEFAULT_PADDING.TexCoords1
                            : modelDefinition.TexCoords1,
                        Color0 = (modelDefinition.Color0 == PerVertexColorType.None)
                            ? DEFAULT_PADDING.Color0
                            : modelDefinition.Color0,
                        Color1 = (modelDefinition.Color1 == PerVertexColorType.None)
                            ? DEFAULT_PADDING.Color1
                            : modelDefinition.Color1,
                        Joints0 = (modelDefinition.Joints0 == PerVertexJointType.None)
                            ? DEFAULT_PADDING.Joints0
                            : modelDefinition.Joints0,
                        Joints1 = (modelDefinition.Joints1 == PerVertexJointType.None)
                            ? DEFAULT_PADDING.Joints1
                            : modelDefinition.Joints1,
                        Weights0 = (modelDefinition.Weights0 == PerVertexWeightsType.None)
                            ? DEFAULT_PADDING.Weights0
                            : modelDefinition.Weights0,
                        Weights1 = (modelDefinition.Weights1 == PerVertexWeightsType.None)
                            ? DEFAULT_PADDING.Weights1
                            : modelDefinition.Weights1,
                    };
                }
            }
        }

        private GltfPrimitiveStorageLocation[] AllocateMeshes(MgStorageBlockAllocationRequest request, GltfMesh[] meshes, GltfAccessor[] accessors, GltfBufferView[] bufferViews)
        {
            var locations = new List<GltfPrimitiveStorageLocation>();
            foreach(var mesh in meshes)
            {
                foreach (var primitive in mesh.Primitives)
                {
                    var locator = primitive.VertexLocations;

                    var finalLocation = new GltfPrimitiveStorageLocation { };

                    finalLocation.CopyOperations = GenerateCopyOps(primitive.VertexCount, request, accessors, bufferViews, locator, finalLocation);
                    locations.Add(finalLocation);
                }
            }
            return locations.ToArray();
        }

        private static GltfInterleavedOperation[] GenerateCopyOps(uint vertexCount, MgStorageBlockAllocationRequest request, GltfAccessor[] accessors, GltfBufferView[] bufferViews, IPerVertexDataLocator locator, GltfPrimitiveStorageLocation finalLocation)
        {
            var totalSize = 0UL;
            var vertexFields = new int?[]
            {
                locator.Position,
                locator.Normal,
                locator.Tangent,
                locator.TexCoords0,
                locator.TexCoords1,
                locator.Color0,
                locator.Color1,
                locator.Joints0,
                locator.Joints1,
                locator.Weights0,
                locator.Weights1,
            };

            var paddingByteStride = new uint[]
            {
                12U,
                12U,
                16U,
                8U,
                8U,
                16U,
                16U,
                4U,
                4U,
                16U,
                16U,
            };

            var copyOps = new List<GltfInterleavedOperation>();

            var vertexBufferStride = 0U;
            for (var i =0; i < vertexFields.Length; i += 1)
            {
                var field = vertexFields[i];
                if (field.HasValue)
                {
                    var selected = accessors[field.Value];
                    var op = CreateCopyOp(copyOps, selected, bufferViews);
                    vertexBufferStride += op.ByteStride;
                    totalSize += selected.TotalByteSize;
                }
                else
                {
                    vertexBufferStride += paddingByteStride[i];
                    totalSize += vertexCount * paddingByteStride[i];
                }
            }

            foreach (var op in copyOps)
            {
                op.DstStride = vertexBufferStride;
            }

            var vertexInfo = new MgStorageBlockAllocationInfo
            {
                MemoryPropertyFlags = MgMemoryPropertyFlagBits.HOST_COHERENT_BIT,
                Usage = MgBufferUsageFlagBits.VERTEX_BUFFER_BIT,
                ElementByteSize = vertexBufferStride,
                Size = totalSize,
            };

            finalLocation.Vertex = request.Insert(vertexInfo);

            if (locator.Indices.HasValue)
            {
                var selected = accessors[locator.Indices.Value];
                var op = CreateCopyOp(copyOps, selected, bufferViews);
                op.DstStride = selected.ElementByteSize;

                var indexInfo = new MgStorageBlockAllocationInfo
                {
                    MemoryPropertyFlags = MgMemoryPropertyFlagBits.HOST_COHERENT_BIT,
                    Usage = MgBufferUsageFlagBits.INDEX_BUFFER_BIT,
                    Size = selected.TotalByteSize,
                    ElementByteSize = selected.ElementByteSize,
                };

                finalLocation.Index = request.Insert(indexInfo);
            }

            return copyOps.ToArray();
        }

        private static GltfInterleavedOperation CreateCopyOp(List<GltfInterleavedOperation> destination, GltfAccessor selected, GltfBufferView[] bufferViews)
        {
            if (!selected.BufferView.HasValue)
                throw new InvalidOperationException("unable to locate bufferview");

            var view = bufferViews[selected.BufferView.Value];

            var op = new GltfInterleavedOperation
            {
                BufferIndex = view.BufferIndex,                
                Count = selected.ElementCount,
                SrcOffset = (ulong)(view.BufferOffset + selected.ViewOffset),
                TotalSize = (uint) (selected.ElementCount * selected.NoOfComponents * selected.ElementByteSize),
            };

            op.ByteStride =
                (view.ByteStride.HasValue)
                ? (uint) view.ByteStride.Value
                : selected.NoOfComponents * selected.ElementByteSize;                    

            destination.Add(op);
            return op;
        }

        private GltfMesh[] ExtractMeshes(Gltf model, GltfAccessor[] accessors, GltfBucketContainer materials)
        {
            var noOfItems = model.Meshes != null ? model.Meshes.Length : 0;
            var output = new GltfMesh[noOfItems];

            for (var i = 0; i < noOfItems; i += 1)
            {
                var result = new GltfMesh(model.Meshes[i], accessors, materials);
                output[i] = result;
            }
            return output;
        }

        private GltfAccessor[] ExtractAccessors(Gltf model)
        {
            var noOfItems = model.Accessors != null ? model.Accessors.Length : 0;
            var output = new GltfAccessor[noOfItems];

            for (var i = 0; i < noOfItems; i += 1)
            {
                output[i] = new GltfAccessor(model.Accessors[i]);                 
            }

            return output;
        }

        private static GltfBufferView[] ExtractBufferViews(glTFLoader.Schema.Gltf model)
        {
            var noOfItems = model.BufferViews != null ? model.BufferViews.Length : 0;
            var output = new GltfBufferView[noOfItems];
            for (var i = 0; i < noOfItems; i += 1)
            {
                output[i] = new GltfBufferView(model.BufferViews[i]);
            }
            return output;
        }


        private GtlfNodeInfo[] ExtractNodes(Gltf model, GltfBucketContainer cameras)
        {
            var noOfNodes = model.Nodes != null ? model.Nodes.Length : 0;
            var allNodes = new GtlfNodeInfo[noOfNodes];

            for (var i = 0; i < noOfNodes; i += 1)
            {
                var srcNode = model.Nodes[i];
                var destNode = new GtlfNodeInfo
                {
                    Name = srcNode.Name,
                    NodeIndex = i,
                    CameraAllocation = cameras.GetAllocation(srcNode.Camera),
                    Children = srcNode.Children ?? (new int[] { }),
                    Transform = GenerateTransform(srcNode), 
                    Mesh = srcNode.Mesh,
                };

                destNode.IsMirrored = destNode.Transform.Determinant < 0;

                // TODO: meshes

                allNodes[i] = destNode;
            }

            LinkToParents(allNodes);
            return allNodes;
        }

        public static TkMatrix4 GenerateTransform(Node srcNode)
        {
           if (srcNode.Matrix != null && srcNode.Matrix.Length == 16)
           {
                var src = srcNode.Matrix;
                return new TkMatrix4(
                    src[0],   src[1],  src[2],  src[3],
                    src[4],   src[5],  src[6],  src[7],
                    src[8],   src[9], src[10], src[11],
                    src[12], src[13], src[14], src[15]
                );
           }
           else
           {
                var firstOp =
                    (srcNode.Scale != null && srcNode.Scale.Length == 3)
                    ? TkMatrix4.CreateScale(srcNode.Scale[0], srcNode.Scale[1], srcNode.Scale[2])
                    : TkMatrix4.CreateScale(1, 1, 1);
                var secondOp =
                    (srcNode.Rotation != null && srcNode.Rotation.Length == 4)
                    ? TkMatrix4.Identity // TODO QUATERNION
                    : TkMatrix4.Identity;
                var thirdOp =
                    (srcNode.Translation != null && srcNode.Translation.Length == 3)
                    ? TkMatrix4.CreateTranslation(srcNode.Translation[0], srcNode.Translation[1], srcNode.Translation[2])
                    : TkMatrix4.CreateTranslation(0, 0, 0);

                // T * (R * S)
                TkMatrix4.Mult(ref secondOp, ref firstOp, out TkMatrix4 result);
                return TkMatrix4.Mult(thirdOp, result);
           }
        }

        public static void LinkToParents(GtlfNodeInfo[] allNodes)
        {
            foreach (var srcParentNode in allNodes)
            {
                var noOfChildren = srcParentNode.Children != null ? srcParentNode.Children.Length : 0;

                if (srcParentNode.Children != null)
                {
                    foreach (var childIndex in srcParentNode.Children)
                    {
                        var child = allNodes[childIndex];
                        child.Parent = srcParentNode.NodeIndex;
                    }
                }
            }
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
                        throw new InvalidDataException(
                            string.Format(
                                "The specified length of embedded data chunk ({0}) is not equal to the actual length of the data chunk ({1}).",
                                selectedBuffer.ByteLength,
                                output.Data.Length)
                        );
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
