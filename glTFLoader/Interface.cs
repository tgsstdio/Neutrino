using System;
using System.IO;
// using System.Runtime.Remoting.Messaging;
using System.Text;
using glTFLoader.Schema;
using Newtonsoft.Json;

namespace glTFLoader
{
    public static class Interface
    {
        const uint GLTF = 0x46546C67;
        const uint JSON = 0x4E4F534A;

        public static Gltf LoadModel(string filePath)
        {
            var path = Path.GetFullPath(filePath);
           // CallContext.LogicalSetData("UriRootPath", Path.GetDirectoryName(path));
            using (var fs = File.Open(path, FileMode.Open))
            {
                return LoadModel(fs);
            }
        }

        public static Gltf LoadModel(Stream fs)
        {
            bool binaryFile = false;

            using (BinaryReader binaryReader = new BinaryReader(fs))
            {
                uint magic = binaryReader.ReadUInt32();
                if (magic == GLTF)
                {
                    binaryFile = true;
                }

                fs.Seek(0, SeekOrigin.Begin);

                string fileData;
                if (binaryFile)
                {
                    fileData = ParseBinary(binaryReader, fs.Length);
                }
                else
                {
                    fileData = ParseText(binaryReader, fs.Length);
                }

                return JsonConvert.DeserializeObject<Gltf>(fileData);
            }
        }

        private static string ParseText(BinaryReader binaryReader, long streamLength)
        {
            return Encoding.UTF8.GetString(binaryReader.ReadBytes((int)streamLength));            
        }

        private static string ParseBinary(BinaryReader binaryReader, long streamLength)
        {
            uint magic = binaryReader.ReadUInt32();
            if (magic != GLTF)
            {
                throw new InvalidDataException($"Unexpected magic number: {magic}");
            }

            uint version = binaryReader.ReadUInt32();
            if (version != 2)
            {
                throw new NotImplementedException($"Unknown version number: {version}");
            }

            uint length = binaryReader.ReadUInt32();
            // DEV: not sure if compressed 
            long fileLength = streamLength;
            if (length != fileLength)
            {
                throw new InvalidDataException($"The specified length of the file ({length}) is not equal to the actual length of the file ({fileLength}).");
            }

            uint chunkLength = binaryReader.ReadUInt32();
            uint chunkFormat = binaryReader.ReadUInt32();
            if (chunkFormat != JSON)
            {
                throw new NotImplementedException($"The first chunk must be format 'JSON': {chunkFormat}");
            }

            return Encoding.UTF8.GetString(binaryReader.ReadBytes((int)chunkLength));            
        }

        public static string SerializeModel(Gltf model)
        {
            var json = JsonConvert.SerializeObject(model, Formatting.Indented);
            return json;
        }

        public static void SaveModel(Gltf model, string path)
        {
            File.WriteAllText(path, SerializeModel(model));
        }

    }



}
