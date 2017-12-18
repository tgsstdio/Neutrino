namespace GltfDemo
{
    public interface IGLDeviceMemoryTypeMap
    {
        uint DetermineTypeIndex(GLDeviceMemoryTypeFlagBits category);
        GLDeviceMemoryTypeInfo[] MemoryTypes { get; }
    }
}
