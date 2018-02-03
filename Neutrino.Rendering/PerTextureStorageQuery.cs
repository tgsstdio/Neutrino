using Magnesium;
using System;

namespace Neutrino
{
    // REPLACEMENT FOR PRE-TRANSFORMS IN OPTIMAL STORAGE
    public class PerTextureStorageQuery
    {
        private MgPhysicalDeviceProperties mProperties;
        public PerTextureStorageQuery(MgPhysicalDeviceProperties properties)
        {
            mProperties = properties;
        }

        public MgBufferUsageFlagBits GetAppropriateBufferUsage()
        {
            if (
                mProperties.Limits.MaxPerStageDescriptorStorageBuffers > 0
                // SHOULD ALWAYS BE TRUE 
                && mProperties.Limits.MaxUniformBufferRange < mProperties.Limits.MaxStorageBufferRange
                )
            {
                return MgBufferUsageFlagBits.STORAGE_BUFFER_BIT;
            }
            else
            {
                if (mProperties.Limits.MaxPerStageDescriptorUniformBuffers == 0)
                    throw new InvalidOperationException("both storage and uniform buffers are unavailable");

                return MgBufferUsageFlagBits.UNIFORM_BUFFER_BIT;
            }
        }

        public uint GetElementRange(
            MgBufferUsageFlagBits usage,
            uint texturesPerMaterial,
            uint sizeOfBlock
        )
        {
            var countPerMaxTextures =
                mProperties.Limits.MaxPerStageDescriptorSampledImages
                / texturesPerMaterial;

            var maxBufferSize =
                (usage == MgBufferUsageFlagBits.STORAGE_BUFFER_BIT)
                ? mProperties.Limits.MaxStorageBufferRange
                : mProperties.Limits.MaxUniformBufferRange;

            var countPerBlockSize =
                maxBufferSize / sizeOfBlock;

            return Math.Min(countPerBlockSize, countPerMaxTextures);
        }

        public PerTextureStorageSettings GetHighestSettings(
            uint texturesPerMaterial,
            uint sizeOfBlock)
        {
            var usage = GetAppropriateBufferUsage();
            var elementRange = GetElementRange(usage, texturesPerMaterial, sizeOfBlock);
            var minAlignment = (usage == MgBufferUsageFlagBits.STORAGE_BUFFER_BIT)
                ? mProperties.Limits.MinStorageBufferOffsetAlignment
                : mProperties.Limits.MinUniformBufferOffsetAlignment;

            return new PerTextureStorageSettings
            {
                Usage = usage,
                ElementRange = elementRange,
                MinimumAlignment = minAlignment,
                AllocationSize = sizeOfBlock * elementRange,
            };
        }
    }
}
