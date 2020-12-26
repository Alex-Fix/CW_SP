using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CW
{
    public class Parser
    {
        public ParserResult ParseFile(string path)
        {
            var parserResults = new List<ParserResult>();
            using(var reader = new StreamReader(path))
            {
                while (!reader.EndOfStream)
                    ParseLine(reader.ReadLine(), parserResults);
                    
            }
            return parserResults;
        }

        private void 
    }
}
