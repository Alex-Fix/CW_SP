using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CW
{
    public class Lexem
    {
        public LexemType LexemType { get; set; }
        public string UnrecognizedText { get; set; }

        public KeyWordType KeyWordType { get; set; }
        public string KeyWord { get; set; }

        public string Operator { get; set; }

        public string CommentText { get; set; }

        public short Value { get; set; }

        public string IdentifierName { get; set; }

        public int LineIndex { get; set; }

        public string Description { get; set; }
    }

    public enum LexemType
    {
        Unrecognized,
        KeyWord,
        Identifier,
        NumericConstant,
        Comment,
        Operator
    }

    public enum KeyWordType
    {
        Program,
        Var,
        Start,
        Finish,
        Get,
        Put,
        If,
        Then,
        Goto,
        Div,
        Mod,
        Eg,
        Ne,
        Not,
        And,
        Or,
        Integer
    }
}
