using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CW
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                if (args.Length != 1)
                    throw new ArgumentException("Incorrect arguments");
                if (args[0].Substring(args[0].Length - 4) != ".p72")
                    throw new ArgumentException("Incorrect file extension");
                if (!Directory.GetFiles(Directory.GetCurrentDirectory()).Any(f => String.Equals(f, $"{Directory.GetCurrentDirectory()}\\{args[0]}")))
                    throw new ArgumentException("Such a file does not exist in the current directory");
                Parser parser = new Parser();
                var lexems = parser.ParseFile($"{Directory.GetCurrentDirectory()}\\{args[0]}");
                SyntacticAnalyser analyser = new SyntacticAnalyser();
                var errors = analyser.Analyze(lexems);
                if (errors.Any())
                {
                    using(var writer = new StreamWriter($"{Directory.GetCurrentDirectory()}\\{args[0].Substring(0, args[0].Length - 4) + "Errors.txt"}"))
                    {
                        foreach (var error in errors)
                            writer.WriteLine($"Line: {error.Key}. Error: {error.Value}");
                    }
                    foreach (var error in errors)
                        Console.WriteLine($"Line: {error.Key}. Error: {error.Value}");
                    throw new Exception($"\nCount of errors: {errors.Count()}. You can see all errors in file '{args[0].Substring(0, args[0].Length - 4) + "Errors.txt"}'");
                }

            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }
            Console.ReadKey();
        }
    }
}
