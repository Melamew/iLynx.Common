using System;
using iLynx.Common;

namespace iLynx.TestBench
{
    internal class TestObject
    {
        public int ThisIsaField;
        public int AndThisIsAProperty { get; set; }

        public string AString { get; set; }
        public ulong AndALargeNumber { get; set; }

        public string[] Array { get; set; }

        public Program.ClassProperty ClassProperty { get; set; }

        public static TestObject MakeRandom()
        {
            var largeBuffer = new byte[8];
            Program.Rand.NextBytes(largeBuffer);
            return new TestObject
            {
                ThisIsaField = Program.Rand.Next(Int32.MinValue, Int32.MaxValue),
                AndThisIsAProperty = Program.Rand.Next(Int32.MinValue, Int32.MaxValue),
                AString = String.Format("{0}Blah{1}".RepeatString(Program.Rand.Next(1, 200)), Program.Rand.Next(0, 2), Program.Rand.Next(-5000, 5000)),
                AndALargeNumber = BitConverter.ToUInt64(largeBuffer, 0),
                Array = "Something".Repeat(Program.Rand.Next(1, 20)),
                ClassProperty = new Program.ClassProperty(),
            };
        }

        public override string ToString()
        {
            return
                String.Format(
                    "RandomClass1 Current Values:{0}ThisIsAField: {1}{0}AndThisIsAProperty: {2}{0}AString: {3}{0}AndALargeNumber: {4}",
                    Environment.NewLine, ThisIsaField, AndThisIsAProperty, AString, AndALargeNumber);
        }

        public bool Compare(TestObject other, bool verbose)
        {
            var result = ThisIsaField == other.ThisIsaField;
            result &= AndThisIsAProperty == other.AndThisIsAProperty;
            result &= AString == other.AString;
            result &= AndALargeNumber == other.AndALargeNumber;
            result &= (Array == null && other.Array == null) || (Array != null && other.Array != null);

            if (null != Array && null != other.Array)
            {
                for (var i = 0; i < Array.Length && i < other.Array.Length; ++i)
                    result &= Array[i] == other.Array[i];
            }
            if (verbose)
            {
                Console.WriteLine("ThisIsAField:");
                Console.WriteLine("Me: {0}, Them: {1}", ThisIsaField, other.ThisIsaField);
                Console.WriteLine("AndThisIsAProperty:");
                Console.WriteLine("Me: {0}, Them: {1}", AndThisIsAProperty, other.AndThisIsAProperty);
                Console.WriteLine("AString:");
                Console.WriteLine("Me: {0}, Them: {1}", AString, other.AString);
                Console.WriteLine("AndALargeNumber:");
                Console.WriteLine("Me: {0}, Them: {1}", AndALargeNumber, other.AndALargeNumber);
                Console.WriteLine("Array:");
                Console.WriteLine("Me: {0}, Them: {1}", Array.CombineToString(), other.Array.CombineToString());
            }
            result &= ClassProperty.Compare(other.ClassProperty, verbose);
            return result;
        }
    }
}