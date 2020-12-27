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
        public IEnumerable<Lexem> ParseFile(string path)
        {
            var lexems = new List<Lexem>();
            using(var reader = new StreamReader(path))
            {
                int lineIndex = 0;
                while (!reader.EndOfStream)
                {
                    ParseLine(reader.ReadLine(), lexems, lineIndex);
                    lineIndex++;
                }
            }
            return lexems;
        }

        private void ParseLine(string line, IEnumerable<Lexem> lexems, int lineIndex)
        {
            if (line is null)
                throw new ArgumentNullException(nameof(line));

            var lexemsList = lexems as List<Lexem> ?? throw new ArgumentNullException(nameof(lexems));
            line += "\n\n";
            int i = 0;

            while(line[i] != '\n')
            {
                if(line[i]== '#')
                {
                    if(line[i+1] == '#')
                        lexemsList.Add(ParseComment(line, i, lineIndex));
                    else
                    {
                        lexemsList.Add(new Lexem
                        {
                            LexemType = LexemType.Unrecognized,
                            Operator = "#" + line[i + 1],
                            LineIndex = lineIndex,
                            Description = "Нерозпізнана лексема"
                        });
                    }
                    break;
                }
                if(line[i]>='a' && line[i] <= 'z')
                {
                    lexemsList.Add(ParseKeyWord(line, ref i, lineIndex));
                    continue;
                }
                if(line[i] >= 'A' && line[i] <= 'Z')
                {
                    lexemsList.Add(ParseIdentifier(line, ref i, lineIndex));
                    continue;
                }
                if((line[i] >= '0' && line[i] <= '9') || (line[i] == '-' && line[i + 1] >= '0' && line[i + 1] <= '9'))
                {
                    lexemsList.Add(ParseNumericConstant(line, ref i, lineIndex));
                    continue;
                }
                if(line[i] == ';')
                {
                    lexemsList.Add(new Lexem
                    {
                        LexemType = LexemType.Operator,
                        Operator = ";",
                        Description = "Розділювач",
                        LineIndex = lineIndex
                    });
                    i++;
                    continue;
                }
                if (line[i] == ':')
                {
                    if(line[i+1] == '=')
                    {
                        lexemsList.Add(new Lexem
                        {
                            LexemType = LexemType.Operator,
                            Operator = ":=",
                            Description = "Присвоєння",
                            LineIndex = lineIndex
                        });
                    }
                    else
                    {
                        lexemsList.Add(new Lexem
                        {
                            LexemType = LexemType.Unrecognized,
                            UnrecognizedText = ":" + line[i + 1],
                            LineIndex = lineIndex,
                            Description = "Нерозпізнана лексема"
                        });
                    }
                    i+=2;
                    continue;
                }
                if(line[i] == '+')
                {
                    if (line[i + 1] == '+')
                    {
                        lexemsList.Add(new Lexem
                        {
                            LexemType = LexemType.Operator,
                            Operator = "++",
                            Description = "Арифметичний оператор",
                            LineIndex = lineIndex
                        });
                    }
                    else
                    {
                        lexemsList.Add(new Lexem
                        {
                            LexemType = LexemType.Unrecognized,
                            UnrecognizedText = "+" + line[i + 1],
                            LineIndex = lineIndex,
                            Description = "Нерозпізнана лексема"
                        });
                    }
                    i+=2;
                    continue;
                }
                if (line[i] == '-')
                {
                    if (line[i + 1] == '-')
                    {
                        lexemsList.Add(new Lexem
                        {
                            LexemType = LexemType.Operator,
                            Operator = "--",
                            Description = "Арифметичний оператор",
                            LineIndex = lineIndex
                        });
                    }
                    else
                    {
                        lexemsList.Add(new Lexem
                        {
                            LexemType = LexemType.Unrecognized,
                            UnrecognizedText = "-" + line[i + 1],
                            LineIndex = lineIndex,
                            Description = "Нерозпізнана лексема"
                        });
                    }
                    i += 2;
                    continue;
                }
                if (line[i] == '*')
                {
                    if (line[i + 1] == '*')
                    {
                        lexemsList.Add(new Lexem
                        {
                            LexemType = LexemType.Operator,
                            Operator = "**",
                            Description = "Арифметичний оператор",
                            LineIndex = lineIndex
                        });
                    }
                    else
                    {
                        lexemsList.Add(new Lexem
                        {
                            LexemType = LexemType.Unrecognized,
                            UnrecognizedText = "*" + line[i + 1],
                            LineIndex = lineIndex,
                            Description = "Нерозпізнана лексема"
                        });
                    }
                    i += 2;
                    continue;
                }
                if(line[i] == '>')
                {
                    lexemsList.Add(new Lexem
                    {
                        LexemType = LexemType.Operator,
                        Operator = ">",
                        Description = "Логічний оператор",
                        LineIndex = lineIndex
                    });
                    i++;
                    continue;
                }
                if (line[i] == '<')
                {
                    lexemsList.Add(new Lexem
                    {
                        LexemType = LexemType.Operator,
                        Operator = "<",
                        Description = "Логічний оператор",
                        LineIndex = lineIndex
                    });
                    i++;
                    continue;
                }
                if(line[i] == ' ' || line[i] == '\t' || line[i] == '\n')
                {
                    i++;
                    continue;
                }
                lexemsList.Add(new Lexem
                {
                    LexemType = LexemType.Unrecognized,
                    LineIndex = lineIndex,
                    Description = "Нерозпізнана лексема",
                    UnrecognizedText = line[i].ToString()
                });
                i++;
            }
        }

        private Lexem ParseComment(string line, int i, int lineIndex)
        {
            if (line is null)
                throw new ArgumentNullException(nameof(line));

            var lexem = new Lexem();
            lexem.LexemType = LexemType.Comment;
            lexem.LineIndex = lineIndex;
            lexem.CommentText = line.Substring(i + 2, line.Length - i -3);
            lexem.Operator = "##";
            lexem.Description = "Коментар";
            return lexem;
        }

        private Lexem ParseKeyWord(string line, ref int i, int lineIndex)
        {
            if (line is null)
                throw new ArgumentNullException(nameof(line));

            var endPosition = i;
            var isCorrect = true;
            while(line[endPosition] !=' ' && line[endPosition]!='\t' && line[endPosition] != '\n')
            {
                if (!(line[endPosition] >= 'a' && line[endPosition] <= 'z' || line[endPosition] == '_' || line[endPosition] == '1' || line[endPosition] == '6'))
                    isCorrect = false;
                endPosition++;
            }
            var keyWord = line.Substring(i, endPosition - i);
            i = endPosition;
            if (!isCorrect)
                return new Lexem
                {
                    LexemType = LexemType.Unrecognized,
                    LineIndex = lineIndex,
                    Description = "Нерозпізнана лексема",
                    UnrecognizedText = keyWord
                };
            var availableKeyWords = new List<string> { "program", "var", "start", "finish", "get", "put", "if", "then", "goto", "div", "mod", "eg", "ne", "not", "and", "or", "integer16_t" };
            foreach(var kw in availableKeyWords)
            {
                if (kw == keyWord)
                {
                    KeyWordType keyWordType = KeyWordType.Program;
                    switch (kw)
                    {
                        case "program":
                            keyWordType = KeyWordType.Program;
                            break;
                        case "var":
                            keyWordType = KeyWordType.Var;
                            break;
                        case "start":
                            keyWordType = KeyWordType.Start;
                            break;
                        case "finish":
                            keyWordType = KeyWordType.Finish;
                            break;
                        case "get":
                            keyWordType = KeyWordType.Get;
                            break;
                        case "put":
                            keyWordType = KeyWordType.Put;
                            break;
                        case "if":
                            keyWordType = KeyWordType.If;
                            break;
                        case "then":
                            keyWordType = KeyWordType.Then;
                            break;
                        case "goto":
                            keyWordType = KeyWordType.Goto;
                            break;
                        case "div":
                            keyWordType = KeyWordType.Div;
                            break;
                        case "mod":
                            keyWordType = KeyWordType.Mod;
                            break;
                        case "eg":
                            keyWordType = KeyWordType.Eg;
                            break;
                        case "ne":
                            keyWordType = KeyWordType.Ne;
                            break;
                        case "not":
                            keyWordType = KeyWordType.Not;
                            break;
                        case "and":
                            keyWordType = KeyWordType.And;
                            break;
                        case "or":
                            keyWordType = KeyWordType.Or;
                            break;
                        case "integer16_t":
                            keyWordType = KeyWordType.Integer;
                            break;
                    }
                    return new Lexem
                    {
                        LexemType = LexemType.KeyWord,
                        LineIndex = lineIndex,
                        Description = "Ключове слово",
                        KeyWord = kw,
                        KeyWordType = keyWordType
                    };
                }  
            }

            return new Lexem
            {
                LexemType = LexemType.Unrecognized,
                LineIndex = lineIndex,
                Description = "Нерозпізнана лексема",
                UnrecognizedText = keyWord
            };
        }

        private Lexem ParseIdentifier(string line, ref int i, int lineIndex)
        {
            if (line is null)
                throw new ArgumentNullException(nameof(line));

            var endPosition = i;
            var isCorrect = true;
            while(line[endPosition] != ' ' && line[endPosition] != '\t' && line[endPosition] != '\n')
            {
                if (!(line[endPosition] >= 'A' && line[endPosition] <= 'Z'))
                    isCorrect = false;
                endPosition++;
            }
            var identifier = line.Substring(i, endPosition - i);
            i = endPosition;
            if (!isCorrect)
                return new Lexem
                {
                    LexemType = LexemType.Unrecognized,
                    LineIndex = lineIndex,
                    Description = "Нерозпізнана лексема",
                    UnrecognizedText = identifier
                };
            if(identifier.Length>8)
                identifier = identifier.Substring(0, 8);
            return new Lexem
            {
                LexemType = LexemType.Identifier,
                IdentifierName = identifier,
                LineIndex = lineIndex,
                Description = "Ідентифікатор"
            };
        }

        private Lexem ParseNumericConstant(string line, ref int i, int lineIndex)
        {
            if (line is null)
                throw new ArgumentNullException(nameof(line));

            var endPosition = i;
            var isCorrect = true;
            var wasMinus = false;
            while (line[endPosition] != ' ' && line[endPosition] != '\t' && line[endPosition] != '\n')
            {
                if (!((line[endPosition] >= '0' && line[endPosition] <= '9') || line[endPosition] == '-'))
                    isCorrect = false;
                if (line[endPosition] == '-')
                {
                    wasMinus = true;
                    endPosition++;
                    continue;
                }
                    
                if (line[endPosition] == '-' && wasMinus)
                    isCorrect = false;
                endPosition++;
            }
            var numericConstantStr = line.Substring(i, endPosition - i);
            i = endPosition;
            if (!isCorrect)
                return new Lexem
                {
                    LexemType = LexemType.Unrecognized,
                    LineIndex = lineIndex,
                    Description = "Нерозпізнана лексема",
                    UnrecognizedText = numericConstantStr
                };
            short numericConstant;
            if(!short.TryParse(numericConstantStr, out numericConstant))
                return new Lexem
                {
                    LexemType = LexemType.Unrecognized,
                    LineIndex = lineIndex,
                    Description = "Нерозпізнана лексема",
                    UnrecognizedText = numericConstantStr
                };
            return new Lexem
            {
                LexemType = LexemType.NumericConstant,
                LineIndex = lineIndex,
                Description = "Числова константа",
                Value = numericConstant
            };
        }
    }
}
