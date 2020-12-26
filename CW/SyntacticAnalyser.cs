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
        public IDictionary<int, string> Analyze(IEnumerable<Lexem> lexems)
        {
            var lexemsList = (lexems as List<Lexem> ?? throw new ArgumentNullException(nameof(lexems)))
                .Where(lex =>lex.LexemType != LexemType.Comment)
                .ToList();

            var errors = new Dictionary<int, string>();
            var negErrorsCounter = -1;

            if (lexemsList.Count() < 7)
                throw new Exception("The program cannot contain less than 7 lexems");

            if (!(lexemsList[0].LexemType == LexemType.KeyWord && lexemsList[0].KeyWordType == KeyWordType.Program))
                errors.Add(lexemsList[0].LineIndex, "The program must start with a keyword 'program'");

            if (!(lexemsList[1].LexemType == LexemType.Unrecognized || lexemsList[1].LexemType == LexemType.Identifier))
                errors.Add(lexemsList[1].LineIndex, "You did not specify a program name");

            if (!(lexemsList[2].LexemType == LexemType.Operator && String.Equals(lexemsList[2].Operator, ";")))
                errors.Add(lexemsList[2].LineIndex, "The program name must be followed by a delimiter ';'");

            if(!(lexemsList[3].LexemType == LexemType.KeyWord && lexemsList[3].KeyWordType == KeyWordType.Var))
                errors.Add(lexemsList[3].LineIndex, "The program must include a block of variables 'var'");

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
                errors.Add(negErrorsCounter--, "The program does not contain the end of the block of variables");

            // Checking the correctness of the block of variables
            var identifiers = new Dictionary<string, string>();
            delIndex = delIndex == -1 ? lexemsList.Count() : delIndex;
            if (lexemsList.Skip(4).Take(delIndex - 4).Any(lex => !(lex.LexemType == LexemType.KeyWord && lex.KeyWordType == KeyWordType.Integer ||
                 lex.LexemType == LexemType.Identifier ||
                 lex.LexemType == LexemType.Operator && String.Equals(lex.Operator, ":=") ||
                 lex.LexemType == LexemType.NumericConstant
                 )))
                errors.Add(negErrorsCounter--, "The variable block is not specified correctly");
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
                            errors.Add(lexemsList[i].LineIndex, "Incorrect sequence of operators");
                        isKeyWord = true;
                        continue;
                    }
                    if (lexemsList[i].LexemType == LexemType.Identifier)
                    {
                        if (!isKeyWord || isOperator || isValue)
                            errors.Add(lexemsList[i].LineIndex, "Incorrect sequence of operators");
                        if (identifiers.ContainsKey(lexemsList[i].IdentifierName))
                            errors.Add(lexemsList[i].LineIndex, "Identifire already exists");
                        else
                            identifiers.Add(lexemsList[i].IdentifierName, lexemsList[i].IdentifierName);
                        isIdentifier = true;
                        continue;
                    }
                    if (lexemsList[i].LexemType == LexemType.Operator)
                    {
                        if (!isKeyWord || !isIdentifier || isValue)
                            errors.Add(lexemsList[i].LineIndex, "Incorrect sequence of operators");
                        isOperator = true;
                        continue;
                    }
                    if (lexemsList[i].LexemType == LexemType.NumericConstant)
                    {
                        if (!isKeyWord || !isIdentifier || !isOperator)
                            errors.Add(lexemsList[i].LineIndex, "Incorrect sequence of operators");
                        isValue = true;
                        continue;
                    }
                }
                if (!(isKeyWord && isIdentifier && isOperator && isValue))
                    errors.Add(negErrorsCounter--, "The variable block is not specified correctly");
            }

            if(delIndex == lexemsList.Count())
            {
                errors.Add(negErrorsCounter--, "The program does not contain a keyword 'start'");
            }
            else
            {
                if(!(lexemsList[delIndex+1].LexemType == LexemType.KeyWord && lexemsList[3].KeyWordType == KeyWordType.Start))
                    errors.Add(lexemsList[delIndex + 1].LineIndex, "The program does not contain a keyword 'start'");
            }
            
            if(!(lexemsList.Last().LexemType == LexemType.KeyWord && lexemsList.Last().KeyWordType == KeyWordType.Finish))
                errors.Add(lexemsList.Last().LineIndex, "The program does not contain a keyword 'finish'");

            if(delIndex != lexemsList.Count())
            {
                var afterStartIndex = delIndex + 1;
                var finishIndex = lexemsList.Count();

                for(int i = afterStartIndex; i< finishIndex; i++)
                {
                    if(lexemsList[i].LexemType == LexemType.KeyWord && lexemsList[i].KeyWordType == KeyWordType.If)
                    {
                        if(lexemsList.ElementAtOrDefault(i + 1) is null)
                            errors.Add(lexemsList.ElementAtOrDefault(i + 1).LineIndex, "After if there must be a numeric constant or identifier");
                        else
                        {
                            if (!(lexemsList[i + 1].LexemType == LexemType.Identifier && identifiers.ContainsKey(lexemsList[i + 1].IdentifierName) ||
                                lexemsList[i + 1].LexemType == LexemType.NumericConstant))
                                errors.Add(lexemsList[i + 1].LineIndex, "Variable not declared");
                        }
                        if (lexemsList.ElementAtOrDefault(i + 2) is null)
                            errors.Add(lexemsList.ElementAtOrDefault(i + 1).LineIndex, "After numeric or variable there must be a comparison");
                        else
                        {
                            if (!(lexemsList[i+2].LexemType == LexemType.KeyWord && lexemsList[i + 2].KeyWordType == KeyWordType.Eg ||
                                lexemsList[i + 2].LexemType == LexemType.KeyWord && lexemsList[i + 2].KeyWordType == KeyWordType.Ne ||
                                lexemsList[i+2].LexemType ==LexemType.Operator && String.Equals(lexemsList[i+2].Operator, ">") ||
                                lexemsList[i + 2].LexemType == LexemType.Operator && String.Equals(lexemsList[i + 2].Operator, "<")))
                                errors.Add(lexemsList[i + 1].LineIndex, "After numeric or variable there must be a comparison");
                        }
                    }
                }
                
            }

            return errors;
        }
    }
}
