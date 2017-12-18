using System;
using System.Collections.Generic;

namespace GltfDemo
{
    public class OrdinaryEffect : IEffect
    {
        private Dictionary<string, uint> mLocations;
        public OrdinaryEffect()
        {
            mLocations = InitializeLocations();
        }

        private static Dictionary<string, uint> InitializeLocations()
        {
            var reservedAttributeNames = new[]
            {
                "POSITION",
                "NORMAL",
                "COLOR_0",
                "TANGENT",
                "TEXCOORD_0",
                "TEXCOORD_1",
            };

            var shaderLocations = new Dictionary<string, uint>();
            for (var i = 0U; i < reservedAttributeNames.Length; i += 1)
            {
                shaderLocations.Add(reservedAttributeNames[i], i);
            }

            return shaderLocations;
        }

        public IReadOnlyDictionary<string, uint> GetShaderAttributeLocations()
        {
            return mLocations;
        }
    }
}
