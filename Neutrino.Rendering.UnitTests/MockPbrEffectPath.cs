using System.IO;

namespace Neutrino.UnitTests
{
    internal class MockPbrEffectPath : IPbrEffectPath
    {
        public string FragPath { get; set; }
        public string VertPath { get; set; }

        public Stream OpenFragmentShader()
        {
            return File.Open(FragPath, FileMode.Open, FileAccess.Read);
        }

        public Stream OpenVertexShader()
        {
            return File.Open(VertPath, FileMode.Open, FileAccess.Read);
        }
    }
}