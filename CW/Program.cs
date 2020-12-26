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
                if (args[1].Substring(args[0].Length - 4) != ".p72")
                    throw new ArgumentException("Incorrect file extension");
                if (!Directory.GetFiles(Directory.GetCurrentDirectory()).Any(f => String.Equals(f, args[0])))
                    throw new ArgumentException("Such a file does not exist in the current directory");
                Parser parser = new Parser();
                var parserResult = parser.ParseFile($"{Directory.GetCurrentDirectory()}\\{args[0]}");

            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}
