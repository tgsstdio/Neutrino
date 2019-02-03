using GLSLSyntaxAST.Preprocessor;
using Neutrino;
using System;
using System.IO;

namespace NeutrinoBatch
{
    class Program
    {
        class NumDefine
        {
            public string Name;
            public int Value;
        }

        static void Main(string[] args)
        {
            // 1 1st argument - template file 
            // 2 2nd arg - shader file output
            // 3 3rd argument - json configuration                               

            // 2 read meta data
            Console.WriteLine("Hello World!");

            const string modelFile = "Data/Triangle.gltf";
            var metadata = LoadModelFile(modelFile);

            // 2 match defines to model data shape

            // var numDefines = new NumDefine[0];

            NumDefine[] numDefines = SetupDefines(metadata);

            var pasteTokens = new PasteToken[]
            {
                new PasteToken { Name = "IS_VERT_POS_TYPE", Value = "vec3"},
            };

            // 3 load tempate into pre-processor

            var templateFile = "Data/Template.txt";

            // 4 output shader file
            var output = TransformTemplate(templateFile, numDefines, pasteTokens);

            var dest = "out2.glsl";
            using (var sw = new StreamWriter(dest, false))
            {
                sw.Write(output);
            }
        }

        private static NumDefine[] SetupDefines(MgtfModelMetaFile metadata)
        {
            return new NumDefine[]
            {
                new NumDefine { Name = "NO_OF_CAMERAS", Value = 1},
                new NumDefine { Name = "NO_OF_LIGHTS", Value = 1},
                new NumDefine { Name = "NO_OF_MATERIALS", Value = 1},

                new NumDefine { Name = "IS_VERT_POS", Value = 1 },
                new NumDefine { Name = "IS_VERT_POS_ORDER", Value = 0 }
            };
        }

        private static string TransformTemplate(
            string filePath,
            NumDefine[] defines, 
            PasteToken[] attributeTypes
        )
        {
            var debug = new InfoSinkBase(SinkType.String);
            var info = new InfoSinkBase(SinkType.String);
            var infoSink = new InfoSink(info, debug);
            var intermediate = new GLSLIntermediate();
            var symbols = new SymbolLookup();
            symbols.SetPreambleManually(Profile.CoreProfile);

            foreach (var x in defines)
            {
                symbols.DefineAs(x.Name, x.Value);
            }

            foreach (var t in attributeTypes)
            {
                symbols.AddPasteToken(t.Name, t.Value);
            }

            var preprocessor = new Standalone(infoSink, intermediate, symbols);
            preprocessor.Run(filePath, out string result);
            return result;
        }

        private static MgtfModelMetaFile LoadModelFile(string modelFile)
        {
            var loader = new Loader();
            var dataLoader = new DataLoader();

            using (var fs = File.Open(modelFile, FileMode.Open))
            {
                // load model file
                var model = glTFLoader.Interface.LoadModel(fs);
                // load meta data
                return loader.LoadMetaData(model);
                // load data
                // var data = dataLoader.LoadData(".", model);
            }
        }

        private class PasteToken
        {
            public string Name { get; set; }
            public string Value { get; set; }
        }
    }
}
