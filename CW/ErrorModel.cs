using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CW
{
    public class ErrorModel
    {
        public int LineIndex { get; set; }
        public string ErrorText { get; set; }
        public ErrorModel(int index, string text)
        {
            this.LineIndex = index;
            this.ErrorText = text;
        }
    }
}
