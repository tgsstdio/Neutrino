using glTFLoader.Schema;
using Magnesium;
using System;

namespace Neutrino.UnitTests
{
    public class GltfSampler
    {
        public MgSamplerAddressMode AddressModeU { get; }
        public MgSamplerAddressMode AddressModeV { get; }
        public MgFilter MinFilter { get; }
        public MgFilter MagFilter { get; }
        public MgSamplerMipmapMode MipmapMode { get; }

        public GltfSampler(Sampler src)
        {
            AddressModeU = GetAddressModeU(src.WrapS);
            AddressModeV = GetAddressModeV(src.WrapT);
            MinFilter = GetMinFilter(src.MinFilter);
            MagFilter = GetMagFilter(src.MagFilter);
            MipmapMode = GetMipmapMode(src.MinFilter);
        }

        private MgSamplerAddressMode GetAddressModeV(Sampler.WrapTEnum wrapT)
        {
            switch (wrapT)
            {
                case Sampler.WrapTEnum.CLAMP_TO_EDGE:
                    return MgSamplerAddressMode.CLAMP_TO_EDGE;
                case Sampler.WrapTEnum.MIRRORED_REPEAT:
                    return MgSamplerAddressMode.MIRRORED_REPEAT;
                case Sampler.WrapTEnum.REPEAT:
                    return MgSamplerAddressMode.REPEAT;
                default:
                    throw new NotSupportedException();
            }
        }

        private MgSamplerAddressMode GetAddressModeU(Sampler.WrapSEnum wrapS)
        {
            switch (wrapS)
            {
                case Sampler.WrapSEnum.CLAMP_TO_EDGE:
                    return MgSamplerAddressMode.CLAMP_TO_EDGE;
                case Sampler.WrapSEnum.MIRRORED_REPEAT:
                    return MgSamplerAddressMode.MIRRORED_REPEAT;
                case Sampler.WrapSEnum.REPEAT:
                    return MgSamplerAddressMode.REPEAT;
                default:
                    throw new NotSupportedException();
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
                        throw new NotSupportedException();
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
                        throw new NotSupportedException();
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
                switch (magFilter.Value)
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
    }
    
}
