using Magnesium;
using System;

namespace Neutrino
{
    // REPLACEMENT FOR PRE-TRANSFORMS IN OPTIMAL STORAGE
    public class PerMaterialTextureStorageQuery
    {
        private MgPhysicalDeviceLimits mLimits;
        public PerMaterialTextureStorageQuery(MgPhysicalDeviceLimits limits)
        {
            mLimits = limits;
        }

        public uint GetMaxNoOfCombinedImageSamplers()
        {
            return mLimits.MaxPerStageDescriptorSampledImages;
        }

        public MgBufferUsageFlagBits GetAppropriateBufferUsage()
        {
            if (
                mLimits.MaxPerStageDescriptorStorageBuffers > 0
                // SHOULD ALWAYS BE TRUE 
                && mLimits.MaxUniformBufferRange < mLimits.MaxStorageBufferRange
                )
            {
                return MgBufferUsageFlagBits.STORAGE_BUFFER_BIT;
            }
            else
            {
                if (mLimits.MaxPerStageDescriptorUniformBuffers == 0)
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
                mLimits.MaxPerStageDescriptorSampledImages
                / texturesPerMaterial;

            var maxBufferSize =
                (usage == MgBufferUsageFlagBits.STORAGE_BUFFER_BIT)
                ? mLimits.MaxStorageBufferRange
                : mLimits.MaxUniformBufferRange;

            var countPerBlockSize =
                maxBufferSize / sizeOfBlock;

            return Math.Min(countPerBlockSize, countPerMaxTextures);
        }

        public PerMaterialStorageSettings GetHighestSettings(
            uint texturesPerMaterial,
            uint sizeOfBlock)
        {
            var usage = GetAppropriateBufferUsage();
            var elementRange = GetElementRange(usage, texturesPerMaterial, sizeOfBlock);
            var minAlignment = (usage == MgBufferUsageFlagBits.STORAGE_BUFFER_BIT)
                ? mLimits.MinStorageBufferOffsetAlignment
                : mLimits.MinUniformBufferOffsetAlignment;

            return new PerMaterialStorageSettings
            {
                Usage = usage,
                ElementRange = elementRange,
                MinimumAlignment = minAlignment,
                AllocationSize = sizeOfBlock * elementRange,
            };
        }
    }
}
