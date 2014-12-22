using System;
using System.IO;
using System.Xml;
using iLynx.Serialization.Xml;
using NUnit.Framework;

namespace iLynx.Tests
{
    public class XmlPrimitivesFixture
    {
        [TestFixture]
        public class ArraySerializerFixture
        {
            [Test]
            public void GivenAnArrayOfValuesWhenSerializeCalledThenStreamPositionAdvances()
            {
                var array = new[] { 1, 2, 3, 4, 5, 6 };
                var target = new XmlPrimitives.ArraySerializer(array.GetType());

                using (var memorStream = new MemoryStream())
                {
                    using (var writer = XmlWriter.Create(memorStream))
                    {
                        target.Serialize(array, writer);
                    }
                    Assert.IsTrue(memorStream.Position > 0);
                }
            }

            [Test]
            public void GivenASerializedArrayOfValuesWhenDeserializeCalledThenValidArrayIsReturned()
            {
                var array = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 0 };
                var target = new XmlPrimitives.ArraySerializer(array.GetType());
                using (var memoryStream = new MemoryStream())
                {
                    using (var writer = XmlWriter.Create(memoryStream, new XmlWriterSettings { ConformanceLevel = ConformanceLevel.Fragment }))
                        target.Serialize(array, writer);
                    memoryStream.Position = 0;
                    Array result;
                    using (var reader = XmlReader.Create(memoryStream, new XmlReaderSettings { ConformanceLevel = ConformanceLevel.Fragment }))
                        result = target.Deserialize(reader);

                    CollectionAssert.AreEqual(array, result);
                }
            }
        }
    }
}
