using iLynx.UI.Controls;
using System;

namespace iLynx.UI.TestBench
{
    public static class Program
    {
        static void Main(string[] args)
        {
            var window = new Window(800, 400, "UI TestBench");
            window.Run();
        }
    }
}
