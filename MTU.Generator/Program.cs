using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace MTU.Generator
{
    using Updater;

    class Program
    {
        static void Main(string[] args)
        {
            var generator = new MTUGenerator(@"S:\html\MTU\");

            if (generator.Generate(Path.Combine(Environment.CurrentDirectory, "ver.xml")))
                Console.WriteLine("XML generated successfully!");
            else
            {
                Console.WriteLine("Erro on generate XML!");
                Console.WriteLine(generator.LastError.Message);
                Console.WriteLine(generator.LastError.StackTrace);
            }

            Console.ReadLine();
        }
    }
}