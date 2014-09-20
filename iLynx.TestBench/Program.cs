using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Windows;
using iLynx.Chatter.Infrastructure;
using iLynx.Common;
using iLynx.Common.Serialization;
using iLynx.Common.Threading;
using iLynx.Common.WPF;
using iLynx.Networking.ClientServer;
using iLynx.PubSub;
using iLynx.TestBench.ClientServerDemo;
using Microsoft.Practices.Unity;

// ReSharper disable LocalizableElement

namespace iLynx.TestBench
{
    public class ConsoleMenu
    {
        private readonly ConsoleKey exitKey;
        private readonly Dictionary<char, Tuple<string, Action>> callbackTable = new Dictionary<char, Tuple<string, Action>>(); 

        public ConsoleMenu(ConsoleKey exitKey = ConsoleKey.Escape)
        {
            this.exitKey = exitKey;
        }

        public void AddMenuItem(char key, string header, Action callback)
        {
            var item = new Tuple<string, Action>(header, callback);
            if (callbackTable.ContainsKey(key)) callbackTable[key] = item;
            else callbackTable.Add(key, item);
        }

        public void ShowMenu()
        {
            PrintOptions();
            while (true)
            {
                var keyInfo = Console.ReadKey();
                if (keyInfo.Key == exitKey) return;
                Console.Clear();
                Tuple<string, Action> tuple;
                if (callbackTable.TryGetValue(keyInfo.KeyChar, out tuple))
                    tuple.Item2.Invoke();
                PrintOptions();
            }
        }

        private void PrintOptions()
        {
            Console.Clear();
            foreach (var kvp in callbackTable)
                Console.WriteLine("Press {0} for: {1}", kvp.Key, kvp.Value.Item1);
            Console.WriteLine("Press {0} to exit this menu", exitKey);
        }
    }

    class Program
    {
        public static readonly Random Rand = new Random();
        public static void Main(string[] args)
        {
            var menu = new ConsoleMenu();
            menu.AddMenuItem('1', "Run some Serializer tests", RunSerializerTests);
            menu.AddMenuItem('2', "Run tests on RuntimeHelper", RunRuntimeHelper);
            menu.AddMenuItem('3', "Run more Serializer tests", RunMoreSerializerTests);
            menu.AddMenuItem('4', "Run \"Primitive\" serialization test", TestPrimitiveSerialization);
            menu.AddMenuItem('5', "Run Multi-Dimensional array serialization test", TestMultiDimensionalArray);
            menu.AddMenuItem('6', "Run Tcp connection tests", () => new TcpConnectionTests().Run());
            menu.AddMenuItem('7', "Run timerservice tests", RunTimerServiceTests);
            menu.AddMenuItem('w', "Run the WPF test window", StartDialog);
            menu.ShowMenu();
        }

        private static void RunTimerServiceTests()
        {
            var timerService = new SingleThreadedTimerService();
            var t1 = 0;
            var t2 = 0;
            const int count = 10;
            var timer1 = timerService.StartNew(() => Console.WriteLine("Timer1: {0}", t1++), 0, 500);
            var timer2 = timerService.StartNew(() => Console.WriteLine("Timer2: {0}", t2++), 0, 1000);

            while (true)
            {
                if (t1 >= count)
                    timerService.Stop(timer1);
                if (t2 >= count)
                    timerService.Stop(timer2);
                if (t1 >= count && t2 >= count) break;
            }
            
            Console.ReadKey();
        }

        private static void WriteTicks(int timer, int tickSoFar, int totalTicks, ITimerService timerService)
        {
            Console.WriteLine("Timer {0}: {1}/{2}", timer, tickSoFar, totalTicks);
            if (tickSoFar >= totalTicks)
                timerService.Stop(timer);
        }

        [STAThread]
        private static void StartDialog()
        {
            var threadObj = new Thread(() =>
            {
                var application = new TestApplication();
                application.Run();
            });
            threadObj.SetApartmentState(ApartmentState.STA);
            threadObj.Start();
            threadObj.Join();
        }

        private static void TestMultiDimensionalArray()
        {
            using (var target = new MemoryStream())
            {
                var item = new ArrayClass { Array = new int[5, 5, 5] };
                item.Array[2, 2, 2] = 5;
                Serializer.Serialize(item, target);
                target.Position = 0;
                var result = Serializer.Deserialize<ArrayClass>(target);
                Console.WriteLine("{0}", result.Array[2, 2, 2] == item.Array[2, 2, 2]);
            }
        }

        public class ArrayClass
        {
            public int[, ,] Array { get; set; }
        }

        private static void TestPrimitiveSerialization()
        {
            using (var memStream = new MemoryStream())
            {
                Serializer.Serialize(5, memStream);
                memStream.Position = 0;
                var result = Serializer.Deserialize<int>(memStream);
                WriteCenter(string.Format("Primitive Serialization: {0}", 5 == result ? "PASS" : "FAIL"), 0);
                Trace.WriteLine(result);
                Debug.Assert(5 == result);
            }
            Console.ReadKey();
        }

        private static void RunMoreSerializerTests()
        {
            SerializerTester.NullTests();
        }

        private static void RunRuntimeHelper()
        {
            const long val = 500000000000000;
            var target1 = new long[1];
            var buf = new byte[8];
            var source = new[] { val };
            Buffer.BlockCopy(source, 0, buf, 0, buf.Length);
            Array.Reverse(buf);
            Buffer.BlockCopy(buf, 0, target1, 0, buf.Length);

            source.SwapEndianness();

            Debug.Assert(source[0] == target1[0]);
        }

        private static void RunSerializerTests()
        {
            const long count = (long)(int.MaxValue * 0.5);
            //var serializer = new Serializer(new ConsoleLogger());
            var lastUpdate = DateTime.Now - TimeSpan.FromSeconds(10);
            var serializeSw = new Stopwatch();
            var deserializeSw = new Stopwatch();
            long totalBytes = 0;
            var i = 0;
            var errors = 0;
            var totalAverages = new double[2];
            var started = DateTime.Now;
            RuntimeCommon.DefaultLogger.Log(LoggingType.Information, null, string.Format("Started Test at {0}", started));
            foreach (var random in CreateSomething(count))
            {
                ++i;
                TestObject other;
                using (var memoryStream = new MemoryStream())
                {
                    serializeSw.Start();
                    Serializer.Serialize(random, memoryStream);
                    serializeSw.Stop();
                    totalBytes += memoryStream.Length;
                    memoryStream.Seek(0, SeekOrigin.Begin);
                    deserializeSw.Start();
                    other = Serializer.Deserialize<TestObject>(memoryStream);
                    deserializeSw.Stop();
                }
                if (!other.Compare(random, false))
                    errors++;
                if (DateTime.Now - lastUpdate <= TimeSpan.FromSeconds(1)) continue;
                Console.Clear();
                var avgSer = (totalBytes / 1024d / 1024) / serializeSw.Elapsed.TotalSeconds;
                var avgDes = (totalBytes / 1024d / 1024) / deserializeSw.Elapsed.TotalSeconds;
                WriteCenter(string.Format("{0}/{1}", i, count), 3);
                WriteCenter(string.Format("Average Serialize Speed  : {0:F2} MiB/s", avgSer), 2);
                WriteCenter(string.Format("Average Deserialize Speed: {0:F2} MiB/s", avgDes), 1);
                WriteCenter(string.Format("Remaining: {0}", count - i), 0);
                WriteCenter(string.Format("Errors: {0}", errors), -1);
                var itemsPerSecond = i / (DateTime.Now - started).TotalSeconds;
                WriteCenter(string.Format("~{0:F2} Items/Second", itemsPerSecond), -2);
                try { WriteCenter(string.Format("~{0} Time Remaining", TimeSpan.FromHours((count - i) / itemsPerSecond / 3600)), -3); }
                catch { WriteCenter(string.Format("TimeSpan Can't Handle The Truth"), -3); }
                //other.Compare(random, true);
                totalAverages[0] += avgSer;
                totalAverages[1] += avgDes;
                deserializeSw.Reset();
                serializeSw.Reset();
                totalBytes = 0;
                lastUpdate = DateTime.Now;
            }
            RuntimeCommon.DefaultLogger.Log(LoggingType.Information, null, "Completed Test run at {0}");
            RuntimeCommon.DefaultLogger.Log(LoggingType.Information, null, string.Format("Average Serialize Speed:   {0} MiB/s", totalAverages[0] / count));
            RuntimeCommon.DefaultLogger.Log(LoggingType.Information, null, string.Format("Average Deserialize Speed: {0} MiB/s", totalAverages[1] / count));
            //for (var i = 0; i < 10; ++i)
            //    Console.WriteLine("Pass: {0}", pass);
        }

        public static void WriteCenter(string str, int top)
        {
            Console.SetCursorPosition(Console.WindowWidth / 2 - str.Length / 2, Console.WindowHeight / 2 - top);
            Console.Write(str);
        }

        private static IEnumerable<TestObject> CreateSomething(long count)
        {
            while (count-- > 0)
                yield return TestObject.MakeRandom();
        }

        public class ClassProperty
        {
            public string SomeString { get; set; }
            public int SomeInt { get; set; }
            public ClassProperty()
            {
                SomeString = "A String";
                SomeInt = 548235;
            }

            public bool Compare(ClassProperty other, bool verbose)
            {
                var result = SomeString == other.SomeString;
                result &= SomeInt == other.SomeInt;
                if (verbose)
                {
                    Console.WriteLine("  SomeString:");
                    Console.WriteLine("  Me: {0}, Them: {1}", SomeString, other.SomeString);
                    Console.WriteLine("  SomeInt:");
                    Console.WriteLine("  Me: {0}, Them: {1}", SomeInt, other.SomeInt);
                }
                return result;
            }
        }
    }

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
// ReSharper restore LocalizableElement