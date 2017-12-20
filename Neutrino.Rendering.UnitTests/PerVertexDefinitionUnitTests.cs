using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Neutrino.UnitTests
{
    [TestClass]
    public class PerVertexDefinitionUnitTests
    {

        [TestMethod]
        public void Decode_0()
        {
            PerVertexDefinition expected = new PerVertexDefinition
            {
                Position = PerVertexPositionType.Float3,
                Normal = PerVertexNormalType.Float3,
                Color0 = PerVertexColorType.ByteUnormRGBA,
                Color1 = PerVertexColorType.ByteUnormRGB,
                Tangent = PerVertexTangentType.Float4,
                Weights0 = PerVertexWeightsType.UshortUnorm4,
                Weights1 = PerVertexWeightsType.Float4,
                Joints0 = PerVertexJointType.Ushort4,
                Joints1 = PerVertexJointType.Byte4,
                IndexType = PerVertexIndexType.Uint32,
            };

            var result = PerVertexDefinitionEncoder.Encode(expected);
            var actual = PerVertexDefinitionEncoder.Decode(result);

            TestResults(expected, actual);
        }

        [TestMethod]
        public void Decode_1()
        {
            PerVertexDefinition expected = new PerVertexDefinition
            {

            };

            var result = PerVertexDefinitionEncoder.Encode(expected);
            var actual = PerVertexDefinitionEncoder.Decode(result);

            TestResults(expected, actual);
        }

        [TestMethod]
        public void Decode_Position_0()
        {
            var expected = new PerVertexDefinition
            {
                Position = PerVertexPositionType.None,
            };

            var result = PerVertexDefinitionEncoder.Encode(expected);
            var actual = PerVertexDefinitionEncoder.Decode(result);

            TestResults(expected, actual);
        }

        [TestMethod]
        public void Decode_Position_1()
        {
            var expected = new PerVertexDefinition
            {
                Position = PerVertexPositionType.Half2,
            };

            var result = PerVertexDefinitionEncoder.Encode(expected);
            var actual = PerVertexDefinitionEncoder.Decode(result);

            TestResults(expected, actual);
        }

        [TestMethod]
        public void Decode_Position_2()
        {
            var expected = new PerVertexDefinition
            {
                Position = PerVertexPositionType.Half3,
            };

            var result = PerVertexDefinitionEncoder.Encode(expected);
            var actual = PerVertexDefinitionEncoder.Decode(result);

            TestResults(expected, actual);
        }

        [TestMethod]
        public void Decode_Position_3()
        {
            var expected = new PerVertexDefinition
            {
                Position = PerVertexPositionType.Float2,
            };

            var result = PerVertexDefinitionEncoder.Encode(expected);
            var actual = PerVertexDefinitionEncoder.Decode(result);

            TestResults(expected, actual);
        }

        [TestMethod]
        public void Decode_Position_4()
        {
            var expected = new PerVertexDefinition
            {
                Position = PerVertexPositionType.Float3,
            };

            var result = PerVertexDefinitionEncoder.Encode(expected);
            var actual = PerVertexDefinitionEncoder.Decode(result);

            TestResults(expected, actual);
        }

        [TestMethod]
        public void Decode_Normal_0()
        {
            var expected = new PerVertexDefinition
            {
                Normal = PerVertexNormalType.None,
            };

            var result = PerVertexDefinitionEncoder.Encode(expected);
            var actual = PerVertexDefinitionEncoder.Decode(result);

            TestResults(expected, actual);
        }

        [TestMethod]
        public void Decode_Normal_1()
        {
            var expected = new PerVertexDefinition
            {
                Normal = PerVertexNormalType.Half3,
            };

            var result = PerVertexDefinitionEncoder.Encode(expected);
            var actual = PerVertexDefinitionEncoder.Decode(result);

            TestResults(expected, actual);
        }

        [TestMethod]
        public void Decode_Normal_2()
        {
            var expected = new PerVertexDefinition
            {
                Normal = PerVertexNormalType.Float3,
            };

            var result = PerVertexDefinitionEncoder.Encode(expected);
            var actual = PerVertexDefinitionEncoder.Decode(result);

            TestResults(expected, actual);
        }

        [TestMethod]
        public void Decode_Tangents_0()
        {
            var expected = new PerVertexDefinition
            {
                Tangent = PerVertexTangentType.None,
            };

            var result = PerVertexDefinitionEncoder.Encode(expected);
            var actual = PerVertexDefinitionEncoder.Decode(result);

            TestResults(expected, actual);
        }

        [TestMethod]
        public void Decode_Tangents_1()
        {
            var expected = new PerVertexDefinition
            {
                Tangent = PerVertexTangentType.Half4,
            };

            var result = PerVertexDefinitionEncoder.Encode(expected);
            var actual = PerVertexDefinitionEncoder.Decode(result);

            TestResults(expected, actual);
        }

        [TestMethod]
        public void Decode_Tangents_2()
        {
            var expected = new PerVertexDefinition
            {
                Tangent = PerVertexTangentType.Float4,
            };

            var result = PerVertexDefinitionEncoder.Encode(expected);
            var actual = PerVertexDefinitionEncoder.Decode(result);

            TestResults(expected, actual);
        }

        [TestMethod]
        public void Decode_Color0_0()
        {
            var expected = new PerVertexDefinition
            {
                Color0 = PerVertexColorType.None,
            };

            var result = PerVertexDefinitionEncoder.Encode(expected);
            var actual = PerVertexDefinitionEncoder.Decode(result);

            TestResults(expected, actual);
        }


        [TestMethod]
        public void Decode_Color0_1()
        {
            var expected = new PerVertexDefinition
            {
                Color0 = PerVertexColorType.ByteUnormRGB,
            };

            var result = PerVertexDefinitionEncoder.Encode(expected);
            var actual = PerVertexDefinitionEncoder.Decode(result);

            TestResults(expected, actual);
        }

        [TestMethod]
        public void Decode_Color0_2()
        {
            var expected = new PerVertexDefinition
            {
                Color0 = PerVertexColorType.ByteUnormRGBA,
            };

            var result = PerVertexDefinitionEncoder.Encode(expected);
            var actual = PerVertexDefinitionEncoder.Decode(result);

            TestResults(expected, actual);
        }


        [TestMethod]
        public void Decode_Color0_3()
        {
            var expected = new PerVertexDefinition
            {
                Color0 = PerVertexColorType.UshortUnormRGB,
            };

            var result = PerVertexDefinitionEncoder.Encode(expected);
            var actual = PerVertexDefinitionEncoder.Decode(result);

            TestResults(expected, actual);
        }

        [TestMethod]
        public void Decode_Color0_4()
        {
            var expected = new PerVertexDefinition
            {
                Color0 = PerVertexColorType.UshortUnormRGBA,
            };

            var result = PerVertexDefinitionEncoder.Encode(expected);
            var actual = PerVertexDefinitionEncoder.Decode(result);

            TestResults(expected, actual);
        }

        [TestMethod]
        public void Decode_Color0_5()
        {
            var expected = new PerVertexDefinition
            {
                Color0 = PerVertexColorType.FloatRGB,
            };

            var result = PerVertexDefinitionEncoder.Encode(expected);
            var actual = PerVertexDefinitionEncoder.Decode(result);

            TestResults(expected, actual);
        }

        [TestMethod]
        public void Decode_Color0_6()
        {
            var expected = new PerVertexDefinition
            {
                Color0 = PerVertexColorType.FloatRGBA,
            };

            var result = PerVertexDefinitionEncoder.Encode(expected);
            var actual = PerVertexDefinitionEncoder.Decode(result);

            TestResults(expected, actual);
        }

        [TestMethod]
        public void Decode_Color1_0()
        {
            var expected = new PerVertexDefinition
            {
                Color1 = PerVertexColorType.None,
            };

            var result = PerVertexDefinitionEncoder.Encode(expected);
            var actual = PerVertexDefinitionEncoder.Decode(result);

            TestResults(expected, actual);
        }


        [TestMethod]
        public void Decode_Color1_1()
        {
            var expected = new PerVertexDefinition
            {
                Color1 = PerVertexColorType.ByteUnormRGB,
            };

            var result = PerVertexDefinitionEncoder.Encode(expected);
            var actual = PerVertexDefinitionEncoder.Decode(result);

            TestResults(expected, actual);
        }

        [TestMethod]
        public void Decode_Color1_2()
        {
            var expected = new PerVertexDefinition
            {
                Color1 = PerVertexColorType.ByteUnormRGBA,
            };

            var result = PerVertexDefinitionEncoder.Encode(expected);
            var actual = PerVertexDefinitionEncoder.Decode(result);

            TestResults(expected, actual);
        }


        [TestMethod]
        public void Decode_Color1_3()
        {
            var expected = new PerVertexDefinition
            {
                Color1 = PerVertexColorType.UshortUnormRGB,
            };

            var result = PerVertexDefinitionEncoder.Encode(expected);
            var actual = PerVertexDefinitionEncoder.Decode(result);

            TestResults(expected, actual);
        }

        [TestMethod]
        public void Decode_Color1_4()
        {
            var expected = new PerVertexDefinition
            {
                Color1 = PerVertexColorType.UshortUnormRGBA,
            };

            var result = PerVertexDefinitionEncoder.Encode(expected);
            var actual = PerVertexDefinitionEncoder.Decode(result);

            TestResults(expected, actual);
        }

        [TestMethod]
        public void Decode_Color1_5()
        {
            var expected = new PerVertexDefinition
            {
                Color1 = PerVertexColorType.FloatRGB,
            };

            var result = PerVertexDefinitionEncoder.Encode(expected);
            var actual = PerVertexDefinitionEncoder.Decode(result);

            TestResults(expected, actual);
        }

        [TestMethod]
        public void Decode_Color1_6()
        {
            var expected = new PerVertexDefinition
            {
                Color1 = PerVertexColorType.FloatRGBA,
            };

            var result = PerVertexDefinitionEncoder.Encode(expected);
            var actual = PerVertexDefinitionEncoder.Decode(result);

            TestResults(expected, actual);
        }

        [TestMethod]
        public void Decode_2()
        {
            PerVertexDefinition expected = new PerVertexDefinition
            {
                Position = PerVertexPositionType.Half3,
                Normal = PerVertexNormalType.None,
                Color0 = PerVertexColorType.FloatRGBA,
                Color1 = PerVertexColorType.None,
                Tangent = PerVertexTangentType.Float4,
                Weights0 = PerVertexWeightsType.UshortUnorm4,
                Weights1 = PerVertexWeightsType.ByteUnorm4,
                Joints0 = PerVertexJointType.Ushort4,
                Joints1 = PerVertexJointType.Byte4,
                IndexType = PerVertexIndexType.Uint16,
            };

            var result = PerVertexDefinitionEncoder.Encode(expected);
            var actual = PerVertexDefinitionEncoder.Decode(result);

            TestResults(expected, actual);
        }

        private static void TestResults(PerVertexDefinition expected, PerVertexDefinition actual)
        {
            Assert.AreEqual(expected.Position, actual.Position);
            Assert.AreEqual(expected.Normal, actual.Normal);
            Assert.AreEqual(expected.Tangent, actual.Tangent);
            Assert.AreEqual(expected.TexCoords0, actual.TexCoords0);
            Assert.AreEqual(expected.TexCoords1, actual.TexCoords1);
            Assert.AreEqual(expected.Color0, actual.Color0);
            Assert.AreEqual(expected.Color1, actual.Color1);
            Assert.AreEqual(expected.Joints0, actual.Joints0);
            Assert.AreEqual(expected.Joints1, actual.Joints1);
            Assert.AreEqual(expected.Weights0, actual.Weights0);
            Assert.AreEqual(expected.Weights1, actual.Weights1);
            Assert.AreEqual(expected.IndexType, actual.IndexType);
        }
    }
}
