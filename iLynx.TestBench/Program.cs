using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using iLynx.Common;
using iLynx.Common.Threading;
using iLynx.Serialization;

// ReSharper disable LocalizableElement

namespace iLynx.TestBench
{
    class Program
    {
        public static readonly Random Rand = new Random();
        public static void Main(string[] args)
        {
            var menu = new ConsoleMenu();
            menu.AddMenuItem('1', "Run some BinarySerializerService tests", RunSerializerTests);
            menu.AddMenuItem('2', "Run tests on RuntimeHelper", RunRuntimeHelper);
            menu.AddMenuItem('3', "Run more BinarySerializerService tests", RunMoreSerializerTests);
            menu.AddMenuItem('4', "Run \"Primitive\" serialization test", TestPrimitiveSerialization);
            menu.AddMenuItem('5', "Run Multi-Dimensional array serialization test", TestMultiDimensionalArray);
            menu.AddMenuItem('6', "Run Tcp connection tests", () => new TcpConnectionTests().Run());
            menu.AddMenuItem('7', "Run timerservice tests", RunTimerServiceTests);
            menu.AddMenuItem('8', "Run BinarySerializerService 'Visualization'", RunSerializerVisualization);
            menu.AddMenuItem('w', "Run the WPF test window", StartDialog);
            menu.ShowMenu();
        }

        private class VisualizationClass
        {
            public string Name { get; set; }
        }

        private static readonly ISerializerService SerializerService = new BinarySerializerService();

        private static void RunSerializerVisualization()
        {
            const string testString = "abcdefghijklmnopqrstuvwxyz";
            Console.WriteLine("Serializing: {0}", testString);
            var instance = new VisualizationClass {Name = testString};
            byte[] buffer;
            using (var stream = new MemoryStream())
            {
                SerializerService.Serialize(instance, stream);
                buffer = stream.ToArray();
            }
            var ascii = Encoding.ASCII.GetString(buffer);
            Console.WriteLine("Input string Length: {0}", testString.Length);
            Console.WriteLine("Output byte length:  {0}", buffer.Length);
            Console.WriteLine("Byte Output: {0}", buffer.ToString(":"));
            Console.WriteLine("ASCII: {0}", ascii);

            Console.ReadKey();
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
                SerializerService.Serialize(item, target);
                target.Position = 0;
                var result = SerializerService.Deserialize<ArrayClass>(target);
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
                SerializerService.Serialize(5, memStream);
                memStream.Position = 0;
                var result = SerializerService.Deserialize<int>(memStream);
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
            //var serializer = new BinarySerializerService(new ConsoleLogger());
            var lastUpdate = DateTime.Now - TimeSpan.FromSeconds(10);
            var serializeSw = new Stopwatch();
            var deserializeSw = new Stopwatch();
            long totalBytes = 0;
            var i = 0;
            var errors = 0;
            var totalAverages = new double[2];
            var started = DateTime.Now;
            RuntimeCommon.DefaultLogger.Log(LogLevel.Information, null, string.Format("Started Test at {0}", started));
            foreach (var random in CreateSomething(count))
            {
                ++i;
                TestObject other;
                using (var memoryStream = new MemoryStream())
                {
                    serializeSw.Start();
                    SerializerService.Serialize(random, memoryStream);
                    serializeSw.Stop();
                    totalBytes += memoryStream.Length;
                    memoryStream.Seek(0, SeekOrigin.Begin);
                    deserializeSw.Start();
                    other = SerializerService.Deserialize<TestObject>(memoryStream);
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
            RuntimeCommon.DefaultLogger.Log(LogLevel.Information, null, "Completed Test run at {0}");
            RuntimeCommon.DefaultLogger.Log(LogLevel.Information, null, string.Format("Average Serialize Speed:   {0} MiB/s", totalAverages[0] / count));
            RuntimeCommon.DefaultLogger.Log(LogLevel.Information, null, string.Format("Average Deserialize Speed: {0} MiB/s", totalAverages[1] / count));
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
}
// ReSharper restore LocalizableElement