﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
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
                            writer.WriteLine($"Line: {error.LineIndex+1}. Error: {error.ErrorText}");
                    }
                    foreach (var error in errors)
                        Console.WriteLine($"Line: {error.LineIndex+1}. Error: {error.ErrorText}");
                    throw new Exception($"\nCount of errors: {errors.Count()}. You can see all errors in file '{args[0].Substring(0, args[0].Length - 4) + "Errors.txt"}'");
                }
                Generator generator = new Generator();
                var code = generator.Generate(lexems);
                if (string.IsNullOrEmpty(code))
                    throw new ArgumentNullException(nameof(code));
                using(var writer = new StreamWriter($"{Directory.GetCurrentDirectory()}\\{args[0].Substring(0, args[0].Length - 4) + "Assembler.asm"}"))
                {
                    writer.Write(code);
                }
                if(Directory.GetFiles(Directory.GetCurrentDirectory()).Contains($"{Directory.GetCurrentDirectory()}\\ml.exe") &&
                    Directory.GetFiles(Directory.GetCurrentDirectory()).Contains($"{Directory.GetCurrentDirectory()}\\link.exe"))
                {
                    Process.Start($"{Directory.GetCurrentDirectory()}\\ml.exe", $"/c /Zd /coff {args[0].Substring(0, args[0].Length - 4) + "Assembler.asm"}").WaitForExit();
                    Process.Start($"{Directory.GetCurrentDirectory()}\\link.exe", $"/SUBSYSTEM:CONSOLE {args[0].Substring(0, args[0].Length - 4) + "Assembler.obj"}").WaitForExit();
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
