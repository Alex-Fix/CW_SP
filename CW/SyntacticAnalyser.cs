using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CW
{
    public class SyntacticAnalyser
    {
        /// <summary>
        /// Method for analyze syntactic
        /// </summary>
        /// <param name="lexems"></param>
        /// <returns></returns>
        public IEnumerable<ErrorModel> Analyze(IEnumerable<Lexem> lexems)
        {
            var lexemsList = (lexems as List<Lexem> ?? throw new ArgumentNullException(nameof(lexems)))
                .Where(lex =>lex.LexemType != LexemType.Comment)
                .ToList();

            var errors = new List<ErrorModel>();
            var negErrorsCounter = -1;

            if (lexemsList.Count() < 7)
                throw new Exception("The program cannot contain less than 7 lexems");

            if (!(lexemsList[0].LexemType == LexemType.KeyWord && lexemsList[0].KeyWordType == KeyWordType.Program))
                errors.Add(new ErrorModel(lexemsList[0].LineIndex, "The program must start with a keyword 'program'"));

            if (!(lexemsList[1].LexemType == LexemType.Unrecognized || lexemsList[1].LexemType == LexemType.Identifier))
                errors.Add(new ErrorModel(lexemsList[1].LineIndex, "You did not specify a program name"));

            if (!(lexemsList[2].LexemType == LexemType.Operator && String.Equals(lexemsList[2].Operator, ";")))
                errors.Add(new ErrorModel(lexemsList[2].LineIndex, "The program name must be followed by a delimiter ';'"));

            if(!(lexemsList[3].LexemType == LexemType.KeyWord && lexemsList[3].KeyWordType == KeyWordType.Var))
                errors.Add(new ErrorModel(lexemsList[3].LineIndex, "The program must include a block of variables 'var'"));

            // Find the end of a block of variables
            int delIndex = -1;
            for(int i = 4; i< lexemsList.Count(); i++)
            {
                if(lexemsList[i].LexemType == LexemType.Operator && String.Equals(lexemsList[i].Operator, ";"))
                {
                    delIndex = i;
                    break;
                }
            }

            if (delIndex == -1)
                errors.Add(new ErrorModel(negErrorsCounter--, "The program does not contain the end of the block of variables"));

            // Checking the correctness of the block of variables
            var identifiers = new Dictionary<string, string>();
            delIndex = delIndex == -1 ? lexemsList.Count() : delIndex;
            if (lexemsList.Skip(4).Take(delIndex - 4).Any(lex => !(lex.LexemType == LexemType.KeyWord && lex.KeyWordType == KeyWordType.Integer ||
                 lex.LexemType == LexemType.Identifier ||
                 lex.LexemType == LexemType.Operator && String.Equals(lex.Operator, ":=") ||
                 lex.LexemType == LexemType.NumericConstant
                 )))
                errors.Add(new ErrorModel(negErrorsCounter--, "The variable block is not specified correctly"));
            else
            {
                var isKeyWord = false;
                var isIdentifier = false;
                var isOperator = false;
                var isValue = false;
                for (int i = 4; i < delIndex; i++)
                {
                    if(lexemsList[i].LexemType == LexemType.KeyWord)
                    {
                        if(isKeyWord && isIdentifier && isOperator && isValue)
                        {
                            isKeyWord = false;
                            isIdentifier = false;
                            isOperator = false;
                            isValue = false;
                        }
                        if (isIdentifier || isOperator || isValue)
                            errors.Add(new ErrorModel(lexemsList[i].LineIndex, "Incorrect sequence of operators"));
                        isKeyWord = true;
                        continue;
                    }
                    if (lexemsList[i].LexemType == LexemType.Identifier)
                    {
                        if (!isKeyWord || isOperator || isValue)
                            errors.Add(new ErrorModel(lexemsList[i].LineIndex, "Incorrect sequence of operators"));
                        if (identifiers.ContainsKey(lexemsList[i].IdentifierName))
                            errors.Add(new ErrorModel(lexemsList[i].LineIndex, "Identifire already exists"));
                        else
                            identifiers.Add(lexemsList[i].IdentifierName, lexemsList[i].IdentifierName);
                        isIdentifier = true;
                        continue;
                    }
                    if (lexemsList[i].LexemType == LexemType.Operator)
                    {
                        if (!isKeyWord || !isIdentifier || isValue)
                            errors.Add(new ErrorModel(lexemsList[i].LineIndex, "Incorrect sequence of operators"));
                        isOperator = true;
                        continue;
                    }
                    if (lexemsList[i].LexemType == LexemType.NumericConstant)
                    {
                        if (!isKeyWord || !isIdentifier || !isOperator)
                            errors.Add(new ErrorModel(lexemsList[i].LineIndex, "Incorrect sequence of operators"));
                        isValue = true;
                        continue;
                    }
                }
                if (!(isKeyWord && isIdentifier && isOperator && isValue))
                    errors.Add(new ErrorModel(negErrorsCounter--, "The variable block is not specified correctly"));
            }

            if(delIndex == lexemsList.Count())
            {
                errors.Add(new ErrorModel(negErrorsCounter--, "The program does not contain a keyword 'start'"));
            }
            else
            {
                if(!(lexemsList[delIndex+1].LexemType == LexemType.KeyWord && lexemsList[delIndex+1].KeyWordType == KeyWordType.Start))
                    errors.Add(new ErrorModel(lexemsList[delIndex + 1].LineIndex, "The program does not contain a keyword 'start'"));
            }
            
            if(!(lexemsList.Last().LexemType == LexemType.KeyWord && lexemsList.Last().KeyWordType == KeyWordType.Finish))
                errors.Add(new ErrorModel(lexemsList.Last().LineIndex, "The program does not contain a keyword 'finish'"));

            if(delIndex != lexemsList.Count())
            {
                var afterStartIndex = delIndex + 2;
                var finishIndex = lexemsList.Count()-1;

                for(int i = afterStartIndex; i< finishIndex; i++)
                {
                    if(lexemsList[i].LexemType == LexemType.KeyWord && lexemsList[i].KeyWordType == KeyWordType.If)
                    {
                        if(lexemsList.ElementAtOrDefault(i + 1) is null)
                            errors.Add(new ErrorModel(negErrorsCounter--, "After if there must be a numeric constant or identifier"));
                        else
                        {
                            if (!(lexemsList[i + 1].LexemType == LexemType.Identifier && identifiers.ContainsKey(lexemsList[i + 1].IdentifierName) ||
                                lexemsList[i + 1].LexemType == LexemType.NumericConstant))
                                errors.Add(new ErrorModel(lexemsList[i + 1].LineIndex, "Variable not declared"));
                        }
                        if (lexemsList.ElementAtOrDefault(i + 2) is null)
                            errors.Add(new ErrorModel(negErrorsCounter--, "After numeric or variable there must be a comparison"));
                        else
                        {
                            if (!(lexemsList[i+2].LexemType == LexemType.KeyWord && lexemsList[i + 2].KeyWordType == KeyWordType.Eg ||
                                lexemsList[i + 2].LexemType == LexemType.KeyWord && lexemsList[i + 2].KeyWordType == KeyWordType.Ne ||
                                lexemsList[i+2].LexemType ==LexemType.Operator && String.Equals(lexemsList[i+2].Operator, ">") ||
                                lexemsList[i + 2].LexemType == LexemType.Operator && String.Equals(lexemsList[i + 2].Operator, "<")))
                                errors.Add(new ErrorModel(lexemsList[i + 2].LineIndex, "After numeric or variable there must be a comparison"));
                        }
                        if (lexemsList.ElementAtOrDefault(i + 3) is null)
                            errors.Add(new ErrorModel(negErrorsCounter--, "After condition there must be a numeric constant or identifier"));
                        else
                        {
                            if (!(lexemsList[i + 3].LexemType == LexemType.Identifier && identifiers.ContainsKey(lexemsList[i + 3].IdentifierName) ||
                                lexemsList[i + 3].LexemType == LexemType.NumericConstant))
                                errors.Add(new ErrorModel(lexemsList[i + 3].LineIndex, "Variable not declared"));
                        }
                        if (lexemsList.ElementAtOrDefault(i + 4) is null)
                            errors.Add(new ErrorModel(negErrorsCounter--, "The numeric must be followed by a keyword 'then'"));
                        else
                        {
                            if (!(lexemsList[i+4].LexemType == LexemType.KeyWord && lexemsList[i + 4].KeyWordType == KeyWordType.Then))
                                errors.Add(new ErrorModel(lexemsList[i + 4].LineIndex, "After numeric or variable there must be a 'then'"));
                        }
                        if (lexemsList.ElementAtOrDefault(i + 5) is null)
                            errors.Add(new ErrorModel(negErrorsCounter--, "After the keyword 'then' should be 'goto'"));
                        else
                        {
                            if (!(lexemsList[i+5].LexemType == LexemType.KeyWord && lexemsList[i + 5].KeyWordType == KeyWordType.Goto))
                                errors.Add(new ErrorModel(lexemsList[i + 5].LineIndex, "After the keyword 'then' should be 'goto'"));
                        }
                        if (lexemsList.ElementAtOrDefault(i + 6) is null)
                            errors.Add(new ErrorModel(negErrorsCounter--, "After the keyword 'goto' should be numeric constant"));
                        else
                        {
                            if (!(lexemsList[i+6].LexemType == LexemType.NumericConstant && lexemsList[i+6].Value >= lexemsList[afterStartIndex].LineIndex && lexemsList[i + 6].Value< lexemsList[finishIndex].LineIndex))
                                errors.Add(new ErrorModel(lexemsList[i + 6].LineIndex, "After the keyword 'goto' should be numeric constant"));
                        }
                        if (lexemsList.ElementAtOrDefault(i + 7) is null)
                            errors.Add(new ErrorModel(negErrorsCounter--, "After the keyword 'goto' should be operator ';'"));
                        else
                        {
                            if (!(lexemsList[i + 7].LexemType == LexemType.Operator && String.Equals(lexemsList[i + 7].Operator, ";")))
                                errors.Add(new ErrorModel(lexemsList[i + 7].LineIndex, "After the keyword 'goto' should be operator ';'"));
                        }
                        i += 7;
                        continue;
                    }
                    if(lexemsList[i].LexemType == LexemType.KeyWord && (lexemsList[i].KeyWordType == KeyWordType.Put || lexemsList[i].KeyWordType == KeyWordType.Get))
                    {
                        if (lexemsList.ElementAtOrDefault(i + 1) is null)
                            errors.Add(new ErrorModel(negErrorsCounter--, $"After the keyword '{lexemsList[i].KeyWord}' should be identifier"));
                        else
                        {
                            if (!(lexemsList[i + 1].LexemType == LexemType.Identifier && identifiers.ContainsKey(lexemsList[i + 1].IdentifierName)))
                                errors.Add(new ErrorModel(lexemsList[i + 1].LineIndex, "Variable not declared"));
                        }
                        i++;
                        continue;
                    }
                    if (lexemsList[i].LexemType == LexemType.KeyWord && lexemsList[i].KeyWordType == KeyWordType.Goto)
                    {
                        if (lexemsList.ElementAtOrDefault(i + 1) is null)
                            errors.Add(new ErrorModel(negErrorsCounter--, "After the keyword 'goto' should be numeric constant"));
                        else
                        {
                            if (!(lexemsList[i + 1].LexemType == LexemType.NumericConstant && lexemsList[i + 1].Value >= lexemsList[afterStartIndex].LineIndex && lexemsList[i + 1].Value < lexemsList[finishIndex].LineIndex))
                                errors.Add(new ErrorModel(lexemsList[i + 1].LineIndex, "After the keyword 'goto' should be numeric constant"));
                        }
                        i++;
                        continue;
                    }
                    if(lexemsList[i].LexemType == LexemType.Identifier)
                    {
                        if (lexemsList.ElementAtOrDefault(i + 1) is null)
                            errors.Add(new ErrorModel(negErrorsCounter--, "After the identifier should be assignment operator"));
                        else
                        {
                            if(!(lexemsList[i+1].LexemType == LexemType.Operator && String.Equals(lexemsList[i+1].Operator, ":=")))
                                errors.Add(new ErrorModel(lexemsList[i + 1].LineIndex, "After the identifier should be assignment operator"));
                        }
                        if (lexemsList.ElementAtOrDefault(i + 2) is null)
                            errors.Add(new ErrorModel(negErrorsCounter--, "After assignment operator must be numeric constant or identifier"));
                        else
                        {
                            if (!(lexemsList[i + 2].LexemType == LexemType.Identifier && identifiers.ContainsKey(lexemsList[i + 2].IdentifierName) ||
                                lexemsList[i + 2].LexemType == LexemType.NumericConstant))
                                errors.Add(new ErrorModel(lexemsList[i + 2].LineIndex, "Variable not declared or After assignment operator must be numeric constant or identifier"));
                        }
                        if (lexemsList.ElementAtOrDefault(i + 3) is null)
                            errors.Add(new ErrorModel(negErrorsCounter--, "After numeric constant or identifier should be arithmetic or logic keyword or operator"));
                        else
                        {
                            if (!(lexemsList[i + 3].LexemType == LexemType.KeyWord && lexemsList[i + 3].KeyWordType == KeyWordType.Div ||
                                lexemsList[i + 3].LexemType == LexemType.KeyWord && lexemsList[i + 3].KeyWordType == KeyWordType.Mod ||
                                lexemsList[i + 3].LexemType == LexemType.KeyWord && lexemsList[i + 3].KeyWordType == KeyWordType.And ||
                                lexemsList[i + 3].LexemType == LexemType.KeyWord && lexemsList[i + 3].KeyWordType == KeyWordType.Or ||
                                lexemsList[i + 3].LexemType == LexemType.Operator && String.Equals(lexemsList[i+3].Operator, "++") ||
                                lexemsList[i + 3].LexemType == LexemType.Operator && String.Equals(lexemsList[i + 3].Operator, "--") ||
                                lexemsList[i + 3].LexemType == LexemType.Operator && String.Equals(lexemsList[i + 3].Operator, "**")
                                ))
                                errors.Add(new ErrorModel(lexemsList[i + 3].LineIndex, "After numeric constant or identifier should be arithmetic or logic keyword or operator"));
                        }
                        if (lexemsList.ElementAtOrDefault(i + 4) is null)
                            errors.Add(new ErrorModel(negErrorsCounter--, "After assignment operator must be numeric constant or identifier"));
                        else
                        {
                            if (!(lexemsList[i + 4].LexemType == LexemType.Identifier && identifiers.ContainsKey(lexemsList[i + 4].IdentifierName) ||
                                lexemsList[i + 4].LexemType == LexemType.NumericConstant))
                                errors.Add(new ErrorModel(lexemsList[i + 2].LineIndex, "Variable not declared or After assignment operator must be numeric constant or identifier"));
                        }
                        i += 4;
                        continue;
                    }
                    errors.Add(new ErrorModel(lexemsList[i].LineIndex, "Incorrect lexem"));
                }
                
            }

            return errors;
        }
    }
}
