using glTFLoader.Schema;
using Magnesium;
using System;

namespace Neutrino
{
    public enum GltfElementType
    {
        Half,
        Float,
        ByteNorm,
        UshortNorm,
        Byte,
        SByte,
        Ushort,
        Uint,
        Short,
    }

    public class GltfAccessor
    {
        public GltfAccessor(Accessor srcAccessor)
        {
            BufferView = srcAccessor.BufferView;
            ViewOffset = srcAccessor.ByteOffset;
            Format = DetermineFormat(
                srcAccessor.Type,
                srcAccessor.ComponentType);
            ElementType = DetermineElementType(srcAccessor.ComponentType);
            ElementByteSize = DetermineByteSize(srcAccessor.ComponentType);
            NoOfComponents = DetermineNoOfComponents(srcAccessor.Type);
            ElementCount = srcAccessor.Count;
            TotalByteSize = (ulong)(ElementByteSize * NoOfComponents * ElementCount);
        }

        public int? BufferView { get; private set; }
        public int ViewOffset { get; private set; }
        public MgFormat Format { get; private set; }
        public GltfElementType ElementType { get; private set; }
        public uint ElementByteSize { get; private set; }
        public uint NoOfComponents { get; private set; }
        public int ElementCount { get; private set; }
        public ulong TotalByteSize { get; private set; }

        private static GltfElementType DetermineElementType(Accessor.ComponentTypeEnum componentType)
        {
            switch (componentType)
            {
                case Accessor.ComponentTypeEnum.BYTE:
                    return GltfElementType.SByte;
                case Accessor.ComponentTypeEnum.UNSIGNED_BYTE:
                    return GltfElementType.Byte;
                case Accessor.ComponentTypeEnum.SHORT:
                    return GltfElementType.Short;
                case Accessor.ComponentTypeEnum.UNSIGNED_SHORT:
                    return GltfElementType.Ushort;
                case Accessor.ComponentTypeEnum.UNSIGNED_INT:
                    return GltfElementType.Uint;
                case Accessor.ComponentTypeEnum.FLOAT:
                    return GltfElementType.Float;
                default:
                    throw new NotSupportedException();
            }
        }

        private static uint DetermineByteSize(Accessor.ComponentTypeEnum componentType)
        {
            switch (componentType)
            {
                case Accessor.ComponentTypeEnum.BYTE:
                    return 1U;
                case Accessor.ComponentTypeEnum.UNSIGNED_BYTE:
                    return 1U;
                case Accessor.ComponentTypeEnum.SHORT:
                    return 2U;
                case Accessor.ComponentTypeEnum.UNSIGNED_SHORT:
                    return 2U;
                case Accessor.ComponentTypeEnum.UNSIGNED_INT:
                    return 4U;
                case Accessor.ComponentTypeEnum.FLOAT:
                    return 4U;
                default:
                    throw new NotSupportedException();
            }
        }

        private static uint DetermineNoOfComponents(Accessor.TypeEnum type)
        {
            switch (type)
            {
                case Accessor.TypeEnum.SCALAR:
                    return 1U;
                case Accessor.TypeEnum.VEC2:
                    return 2U;
                case Accessor.TypeEnum.VEC3:
                    return 3U;
                case Accessor.TypeEnum.VEC4:
                    return 4U;
                case Accessor.TypeEnum.MAT2:
                    return 4U;
                case Accessor.TypeEnum.MAT3:
                    return 9U;
                case Accessor.TypeEnum.MAT4:
                    return 16U;
                default:
                    throw new NotSupportedException();
            }
        }

        private static MgFormat DetermineFormat(Accessor.TypeEnum type, Accessor.ComponentTypeEnum componentType)
        {
            if (type == Accessor.TypeEnum.SCALAR)
            {
                switch (componentType)
                {
                    case Accessor.ComponentTypeEnum.BYTE:
                        return MgFormat.R8_SINT;
                    case Accessor.ComponentTypeEnum.UNSIGNED_BYTE:
                        return MgFormat.R8_UINT;
                    case Accessor.ComponentTypeEnum.FLOAT:
                        return MgFormat.R32_SFLOAT;
                    case Accessor.ComponentTypeEnum.SHORT:
                        return MgFormat.R16_SINT;
                    case Accessor.ComponentTypeEnum.UNSIGNED_SHORT:
                        return MgFormat.R16_UINT;
                    case Accessor.ComponentTypeEnum.UNSIGNED_INT:
                        return MgFormat.R32_UINT;
                    default:
                        throw new NotSupportedException();
                }
            }
            else if (type == Accessor.TypeEnum.VEC2)
            {
                switch (componentType)
                {
                    case Accessor.ComponentTypeEnum.BYTE:
                        return MgFormat.R8G8_SINT;
                    case Accessor.ComponentTypeEnum.UNSIGNED_BYTE:
                        return MgFormat.R8G8_UINT;
                    case Accessor.ComponentTypeEnum.FLOAT:
                        return MgFormat.R32G32_SFLOAT;
                    case Accessor.ComponentTypeEnum.SHORT:
                        return MgFormat.R16G16_SINT;
                    case Accessor.ComponentTypeEnum.UNSIGNED_SHORT:
                        return MgFormat.R16G16_UINT;
                    case Accessor.ComponentTypeEnum.UNSIGNED_INT:
                        return MgFormat.R32G32_UINT;
                    default:
                        throw new NotSupportedException();
                }
            }
            else if (type == Accessor.TypeEnum.VEC3)
            {
                switch (componentType)
                {
                    case Accessor.ComponentTypeEnum.BYTE:
                        return MgFormat.R8G8B8_SINT;
                    case Accessor.ComponentTypeEnum.UNSIGNED_BYTE:
                        return MgFormat.R8G8B8_UINT;
                    case Accessor.ComponentTypeEnum.FLOAT:
                        return MgFormat.R32G32B32_SFLOAT;
                    case Accessor.ComponentTypeEnum.SHORT:
                        return MgFormat.R16G16B16_SINT;
                    case Accessor.ComponentTypeEnum.UNSIGNED_SHORT:
                        return MgFormat.R16G16B16_UINT;
                    case Accessor.ComponentTypeEnum.UNSIGNED_INT:
                        return MgFormat.R32G32B32_UINT;
                    default:
                        throw new NotSupportedException();
                }
            }
            else if (type == Accessor.TypeEnum.VEC4)
            {
                switch (componentType)
                {
                    case Accessor.ComponentTypeEnum.BYTE:
                        return MgFormat.R8G8B8A8_SINT;
                    case Accessor.ComponentTypeEnum.UNSIGNED_BYTE:
                        return MgFormat.R8G8B8A8_UINT;
                    case Accessor.ComponentTypeEnum.FLOAT:
                        return MgFormat.R32G32B32A32_SFLOAT;
                    case Accessor.ComponentTypeEnum.SHORT:
                        return MgFormat.R16G16B16A16_SINT;
                    case Accessor.ComponentTypeEnum.UNSIGNED_SHORT:
                        return MgFormat.R16G16B16A16_UINT;
                    case Accessor.ComponentTypeEnum.UNSIGNED_INT:
                        return MgFormat.R32G32B32A32_UINT;
                    default:
                        throw new NotSupportedException();
                }
            }
            else
            {
                throw new NotSupportedException();
            }

        }
    }
}
