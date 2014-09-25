using System;
using System.Collections.Generic;

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
}