using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CW
{
    public class Generator
    {
        public string Generate(IEnumerable<Lexem> lexems)
        {
            var lexemsList = (lexems as List<Lexem> ?? throw new ArgumentNullException(nameof(lexems))).Where(el => el.LexemType != LexemType.Comment).ToList();

            var path =  " Libs\\";

            var code = GenerateBegin(path);

            code += GenerateData(lexemsList);

            code += GenerateCode(lexemsList);

            code += GenerateEndProgram();

            return code;
        }

        private string GenerateBegin(string path)
        {
            if (path is null)
                throw new ArgumentNullException(nameof(path));

            var code = "";
            code += ".586" + "\n";
            code += ".model flat, STDCALL" + "\n";
            code += "option casemap :none" + "\n\r";
            code += "include" + path + "user32.inc" + "\n";
            code += "include" + path + "windows.inc" + "\n";
            code += "include " + path + "macros.asm " + "\n";
            code += "include" + path + "masm32.inc" + "\n";
            code += "include" + path + "gdi32.inc" + "\n";
            code += "include" + path + "kernel32.inc" + "\n\r";
            code += "includelib" + path + "user32.lib" + "\n";
            code += "includelib" + path + "masm32.lib" + "\n";
            code += "includelib" + path + "gdi32.lib" + "\n";
            code += "includelib" + path + "kernel32.lib" + "\n\r";
            return code;
        }

        private string GenerateData(IEnumerable<Lexem> lexems)
        {
            var lexemsList = lexems as List<Lexem> ?? throw new ArgumentException(nameof(lexems));
            var code = "";

            code += ".data" + "\n";

            code += "buff_ db 11 Dup(?),0" + "\n";
            code += "stdInHandle_0 dd ?" + "\n";
            code += "bytesRead_0	dd ?" + "\n";
            code += "minus_ db '-',0" + "\n";
            code += "size_mes dd ($-minus_-1)" + "\n";
            code += "consoleHandle_0 dd 0" + "\n";
            code += "bytesWrite_0 dd ?" + "\n";
            code += "consoleHandle_1 dd 0" + "\n";
            code += "bytesWrite_1 dd ?" + "\n";

            var valIndex = lexemsList.FindIndex(el => el.LexemType == LexemType.KeyWord && el.KeyWordType == KeyWordType.Var);
            var sepIndex = 0;
            for(int i = valIndex; i< lexemsList.Count(); i++)
            {
                if(lexemsList[i].LexemType == LexemType.Operator && lexemsList[i].Operator == ";")
                {
                    sepIndex = i;
                    break;
                }
            }
            for(int i = valIndex + 1; i< sepIndex; i+=4)
            {
                code += lexemsList[i + 1].IdentifierName + " dd " + lexemsList[i + 3].Value.ToString() + "\n";
            }

            code += "\r";

            return code;
        }

        private string GenerateCode(IEnumerable<Lexem> lexems)
        {
            var lexemsList = lexems as List<Lexem> ?? throw new ArgumentException(nameof(lexems));
            var code = "";

            code += ".code\n";
            code += "start_st:\n";

            var startIndex = lexemsList.FindIndex(el => el.LexemType == LexemType.KeyWord && el.KeyWordType == KeyWordType.Start) + 1;
            for(int i = startIndex; i < lexemsList.Count();)
            {
                code += "Mark_" + (lexemsList[i].LineIndex+1).ToString() + ":\n";
                if(lexemsList[i].LexemType == LexemType.KeyWord && lexemsList[i].KeyWordType == KeyWordType.Get)
                {
                    code += "call InputInt\n";
                    code += "mov " + lexemsList[i + 1].IdentifierName + ",eax\n\r";
                    i += 2;
                    continue;
                }
                if(lexemsList[i].LexemType == LexemType.KeyWord && lexemsList[i].KeyWordType == KeyWordType.Put)
                {
                    code += "mov eax, " + lexemsList[i + 1].IdentifierName + "\n";
                    code += "call OutInt\n\r";
                    i += 2;
                    continue;
                }
                if(lexemsList[i].LexemType == LexemType.Identifier)
                {
                    if(lexemsList[i + 2].LexemType == LexemType.Identifier)
                    {
                        code += "mov eax, " + lexemsList[i + 2].IdentifierName + "\n";
                    }
                    else
                    {
                        code += "mov eax, " + lexemsList[i + 2].Value.ToString() + "\n";
                    }
                    if (lexemsList[i + 4].LexemType == LexemType.Identifier)
                    {
                        code += "mov ebx, " + lexemsList[i + 4].IdentifierName + "\n";
                    }
                    else
                    {
                        code += "mov ebx, " + lexemsList[i + 4].Value.ToString() + "\n";
                    }
                        
                    if(lexemsList[i+3].LexemType == LexemType.KeyWord && lexemsList[i + 3].KeyWordType == KeyWordType.Div)
                    {
                        code += "idiv bx\n";
                        code += "xor edx, edx\n";
                        code += "mov dx, ax\n";
                        code += "mov " + lexemsList[i].IdentifierName + ",edx\n";
                    }
                    else if(lexemsList[i + 3].LexemType == LexemType.KeyWord && lexemsList[i + 3].KeyWordType == KeyWordType.Mod)
                    {
                        code += "idiv bx\n";
                        code += "xor eax, eax\n";
                        code += "mov ax, dx\n";
                        code += "mov " + lexemsList[i].IdentifierName + ",eax\n";
                    }
                    else if(lexemsList[i + 3].LexemType == LexemType.KeyWord && lexemsList[i + 3].KeyWordType == KeyWordType.And)
                    {
                        code += "and eax, ebx\n";
                        code += "mov " + lexemsList[i].IdentifierName + ",eax\n";
                    }
                    else if(lexemsList[i + 3].LexemType == LexemType.KeyWord && lexemsList[i + 3].KeyWordType == KeyWordType.Or)
                    {
                        code += "xor eax, ebx\n";
                        code += "mov " + lexemsList[i].IdentifierName + ",eax\n";
                    }
                    else if(lexemsList[i + 3].LexemType == LexemType.Operator && lexemsList[i + 3].Operator == "++")
                    {
                        code += "add eax, ebx\n";
                        code += "mov " + lexemsList[i].IdentifierName + ",eax\n";
                    }
                    else if (lexemsList[i + 3].LexemType == LexemType.Operator && lexemsList[i + 3].Operator == "--")
                    {
                        code += "sub eax, ebx\n";
                        code += "mov " + lexemsList[i].IdentifierName + ",eax\n";
                    }
                    else if (lexemsList[i + 3].LexemType == LexemType.Operator && lexemsList[i + 3].Operator == "**")
                    {
                        code += "imul bx\n";
                        code += "mov " + lexemsList[i].IdentifierName + ",eax\n";
                    }
                    i += 5;
                    continue;
                }
                if(lexemsList[i].LexemType == LexemType.KeyWord && lexemsList[i].KeyWordType == KeyWordType.Goto)
                {
                    code += "jmp Mark_" + lexemsList[i + 1].Value.ToString() + "\n";
                    i += 2;
                    continue;
                }
                if (lexemsList[i].LexemType == LexemType.KeyWord && lexemsList[i].KeyWordType == KeyWordType.If)
                {
                    if (lexemsList[i + 1].LexemType == LexemType.Identifier)
                    {
                        code += "mov eax, " + lexemsList[i + 1].IdentifierName + "\n";
                    }
                    else
                    {
                        code += "mov eax, " + lexemsList[i + 1].Value.ToString() + "\n";
                    }
                    if (lexemsList[i + 3].LexemType == LexemType.Identifier)
                    {
                        code += "mov ebx, " + lexemsList[i + 3].IdentifierName + "\n";
                    }
                    else
                    {
                        code += "mov ebx, " + lexemsList[i + 3].Value.ToString() + "\n";
                    }
                    code += "cmp eax, ebx\n";
                    if (lexemsList[i + 2].LexemType == LexemType.KeyWord && lexemsList[i + 2].KeyWordType == KeyWordType.Eg)
                    {
                        code += "je Mark_" + lexemsList[i + 6].Value.ToString() + "\n";
                    }
                    else if(lexemsList[i + 2].LexemType == LexemType.KeyWord && lexemsList[i + 2].KeyWordType == KeyWordType.Ne)
                    {
                        code += "jne Mark_" + lexemsList[i + 6].Value.ToString() + "\n";
                    }
                    else if(lexemsList[i + 2].LexemType == LexemType.Operator && lexemsList[i+2].Operator == ">")
                    {
                        code += "jg Mark_" + lexemsList[i + 6].Value.ToString() + "\n";
                    }
                    else if (lexemsList[i + 2].LexemType == LexemType.Operator && lexemsList[i + 2].Operator == "<")
                    {
                        code += "jl Mark_" + lexemsList[i + 6].Value.ToString() + "\n";
                    }
                    i += 8;
                    continue;
                }
                i++;
            }

            return code;
        }


        private string GenerateEndProgram()
        {
            var code = "";
            code += "push 0" + "\n";
            code += "call ExitProcess" + "\n\r";
            code += "OutInt proc " + "\n";
            code += "test    eax, eax" + "\n";
            code += "jns     oi_1" + "\n";
            code += "push eax" + "\n";
            code += "invoke GetStdHandle, STD_OUTPUT_HANDLE" + "\n";
            code += "mov consoleHandle_0,eax" + "\n";
            code += "invoke WriteConsole, consoleHandle_0, ADDR minus_, size_mes, ADDR bytesWrite_0, 0" + "\n";
            code += "pop  eax" + "\n";
            code += "neg     eax" + "\n";
            code += "oi_1:    xor     ecx, ecx" + "\n";
            code += "mov     ebx, 10 " + "\n";
            code += "oi_2:    xor     edx,edx" + "\n";
            code += "div     ebx" + "\n";
            code += "push    edx" + "\n";
            code += "inc     ecx" + "\n";
            code += "test    eax, eax" + "\n";
            code += "jnz     oi_2" + "\n";
            code += "mov esi, offset buff_" + "\n";
            code += "invoke GetStdHandle, STD_OUTPUT_HANDLE" + "\n";
            code += "mov consoleHandle_1,eax" + "\n";
            code += "oi_3: 	pop     edx" + "\n";
            code += "add dl,'0'" + "\n";
            code += "mov [esi],dl" + "\n";
            code += "inc esi" + "\n";
            code += "mov minus_,dl" + "\n";
            code += "mov ebx,ecx" + "\n";
            code += "invoke WriteConsole, consoleHandle_1, ADDR minus_, 1, ADDR bytesWrite_1, 0" + "\n";
            code += "mov ecx,ebx" + "\n";
            code += "loop    oi_3" + "\n";
            code += "mov dl,13" + "\n";
            code += "mov minus_,dl" + "\n";
            code += "invoke WriteConsole, consoleHandle_1, ADDR minus_, 1, ADDR bytesWrite_1, 0" + "\n";
            code += "mov dl,10" + "\n";
            code += "mov minus_,dl" + "\n";
            code += "invoke WriteConsole, consoleHandle_1, ADDR minus_, 1, ADDR bytesWrite_1, 0" + "\n";
            code += "mov  eax, ecx" + "\n";
            code += "ret" + "\n";
            code += "OutInt endp " + "\n\r";
            code += "InputInt proc" + "\n";
            code += "invoke GetStdHandle, STD_INPUT_HANDLE" + "\n";
            code += "mov stdInHandle_0,eax" + "\n";
            code += "invoke ReadConsole, stdInHandle_0, ADDR buff_, 11, ADDR bytesRead_0, 0" + "\n";
            code += "mov esi,offset buff_" + "\n";
            code += "cmp byte ptr [esi],\"-\"" + "\n";
            code += "jnz ii_1" + "\n";
            code += "mov edi,1" + "\n";
            code += "inc esi" + "\n";
            code += "ii_1:    xor eax,eax" + "\n";
            code += "xor ecx,ecx" + "\n";
            code += "mov ebx,10" + "\n";
            code += "ii_2:    mov cl,[esi]" + "\n";
            code += "cmp cl,0dh" + "\n";
            code += "jz end_in" + "\n";
            code += "cmp cl,'0'" + "\n";
            code += "jl er_" + "\n";
            code += "cmp cl,'9'" + "\n";
            code += "ja er_" + "\n";
            code += "sub cl,'0'" + "\n";
            code += "mul ebx" + "\n";
            code += "add eax,ecx" + "\n";
            code += "inc esi" + "\n";
            code += "jmp ii_2" + "\n";
            code += "er_: push 0" + "\n";
            code += "call ExitProcess" + "\n";
            code += "end_in:" + "\n";
            code += "cmp edi,1" + "\n";
            code += "jnz ii_3" + "\n";
            code += "neg eax" + "\n";
            code += "ii_3:" + "\n";
            code += "ret" + "\n";
            code += "InputInt endp" + "\n\r";
            code += "end start_st" + "\n";
            return code;
        }
    }
}
