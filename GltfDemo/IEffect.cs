using System.Collections.Generic;

namespace GltfDemo
{
    public interface IEffect
    {
        IReadOnlyDictionary<string, uint> GetShaderAttributeLocations();
    }    
}
