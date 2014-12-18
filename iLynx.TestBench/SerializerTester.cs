using System;
using System.IO;
using iLynx.Serialization;

namespace iLynx.TestBench
{
    public class SerializerTester
    {
        public static void NullTests()
        {
            var obj = TestObject.MakeRandom();
            obj.Array = null;
            obj.AString = null;
            var failures = 0;
            var serializer = new BinarySerializerService();
            for (var i = 0; i < 50000000; ++i)
            {
                var pass = true;
                using (var memStream = new MemoryStream())
                {
                    serializer.Serialize(obj, memStream);
                    memStream.Position = 0;
                    var result = serializer.Deserialize<TestObject>(memStream);
                    pass &= result.Compare(obj, false);
                }
                Program.WriteCenter(string.Format("Test: {0}", i + 1), 2);
                Program.WriteCenter(string.Format("{0}", pass ? "PASS" : "FAIL"),  1);
                failures += pass ? 0 : 1;
                Program.WriteCenter(string.Format("Failures: {0}", failures), 0);
            }
            Console.ReadKey();
            Console.Clear();
        }
    }
}
