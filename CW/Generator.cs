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
                        code += "call DivProc\n";
                        code += "mov " + lexemsList[i].IdentifierName + ",eax\n";
                    }
                    else if(lexemsList[i + 3].LexemType == LexemType.KeyWord && lexemsList[i + 3].KeyWordType == KeyWordType.Mod)
                    {
                        code += "call ModProc\n";
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
                        code += "call MulProc\n";
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
            code += "MulProc proc\n";
code += "cmp eax, 0\n";
            code += "je ZeroMul\n";
            code += "jl L_N\n";
            code += "cmp ebx, 0\n";
            code += "je ZeroMul\n";
            code += "jl R_N\n";
            code += "jmp NormCase\n";
            code += "L_N:\n";
            code += "cmp ebx, 0\n";
            code += "je ZeroMul\n";
            code += "jl LR_N\n";
            code += "mov ecx, ebx\n";
            code += "mov ebx, eax\n";
            code += "L_NLoop:\n";
            code += "cmp ecx, 1\n";
            code += "jle EndMul\n";
            code += "add eax, ebx\n";
            code += "dec ecx\n";
            code += "jmp L_NLoop\n";
            code += "R_N:\n";
            code += "cmp eax, 0\n";
            code += "je ZeroMul\n";
            code += "jl LR_N\n";
            code += "mov edx, eax\n";
            code += "xor eax, eax\n";
            code += "sub eax, edx\n";
            code += "mov edx, ebx\n";
            code += "xor ebx, ebx\n";
            code += "sub ebx, edx\n";
            code += "mov ecx, ebx\n";
            code += "mov ebx, eax\n";
            code += "R_NLoop:\n";
            code += "cmp ecx, 1\n";
            code += "jle EndMul\n";
            code += "add eax, ebx\n";
            code += "dec ecx\n";
            code += "jmp R_NLoop\n";
            code += "LR_N:\n";
            code += "mov edx, eax\n";
            code += "xor eax, eax\n";
            code += "sub eax, edx\n";
            code += "mov edx, ebx\n";
            code += "xor ebx, ebx\n";
            code += "sub ebx, edx\n";
            code += "mov ecx, ebx\n";
            code += "mov ebx, eax\n";
            code += "LR_NLoop:\n";
            code += "cmp ecx, 1\n";
            code += "jle EndMul\n";
            code += "add eax, ebx\n";
            code += "dec ecx\n";
            code += "jmp LR_NLoop\n";
            code += "jmp EndMul\n";
            code += "NormCase:\n";
            code += "mov ecx, ebx\n";
            code += "mov ebx, eax\n";
            code += "NormCaseLoop:\n";
            code += "cmp ecx, 1\n";
            code += "jle EndMul\n";
            code += "add eax, ebx\n";
            code += "dec ecx\n";
            code += "jmp NormCaseLoop\n";
            code += "ZeroMul:\n";
            code += "xor eax,eax\n";
            code += "EndMul:\n";
            code += "ret\n";
            code += "MulProc endp\n";
            code += "DivProc proc" + "\n";
            code += "xor ecx, ecx" + "\n";
            code += "cmp eax, 0" + "\n";
            code += "jl Rem_neg" + "\n";
            code += "cmp ebx, 0" + "\n";
            code += "je Div_z" + "\n";
            code += "jl Div_neg" + "\n";
            code += "jmp Norm_case" + "\n";
            code += "Rem_neg:" + "\n";
            code += "cmp ebx, 0" + "\n";
            code += "je Div_z" + "\n";
            code += "jl Rem_Div_neg" + "\n";
            code += "mov edx, ebx" + "\n";
            code += "xor ebx, ebx" + "\n";
            code += "sub ebx, edx" + "\n";
            code += "Rem_neg_loop:" + "\n";
            code += "cmp eax, ebx" + "\n";
            code += "jg Div_end" + "\n";
            code += "sub eax, ebx" + "\n";
            code += "dec ecx" + "\n";
            code += "jmp Rem_neg_loop" + "\n";
            code += "Div_neg:" + "\n";
            code += "cmp eax, 0" + "\n";
            code += "jl Rem_Div_neg" + "\n";
            code += "mov edx, ebx" + "\n";
            code += "xor ebx, ebx" + "\n";
            code += "sub ebx, edx" + "\n";
            code += "Div_neg_loop:" + "\n";
            code += "cmp eax, ebx" + "\n";
            code += "jl Div_end" + "\n";
            code += "sub eax, ebx" + "\n";
            code += "dec ecx" + "\n";
            code += "jmp Div_neg_loop" + "\n";
            code += "Rem_Div_neg:" + "\n";
            code += "mov edx, eax" + "\n";
            code += "xor eax, eax" + "\n";
            code += "sub eax, edx" + "\n";
            code += "mov edx, ebx" + "\n";
            code += "xor ebx, ebx" + "\n";
            code += "sub ebx, edx" + "\n";
            code += "Rem_Div_neg_loop:" + "\n";
            code += "cmp eax, ebx" + "\n";
            code += "jl Div_end" + "\n";
            code += "sub eax, ebx" + "\n";
            code += "inc ecx" + "\n";
            code += "jmp Rem_Div_neg_loop" + "\n";
            code += "Norm_case:" + "\n";
            code += "cmp eax, ebx" + "\n";
            code += "jl Div_end" + "\n";
            code += "sub eax, ebx" + "\n";
            code += "inc ecx" + "\n";
            code += "jmp Norm_case" + "\n";
            code += "Div_z:" + "\n";
            code += "mov ecx, 0" + "\n";
            code += "Div_end:" + "\n";
            code += "mov eax, ecx" + "\n";
            code += "ret\n";
            code += "DivProc endp" + "\n\r";
            code += "ModProc proc" + "\n";
            code += "xor ecx, ecx" + "\n";
            code += "cmp eax, 0" + "\n";
            code += "jl ModRem_neg" + "\n";
            code += "cmp ebx, 0" + "\n";
            code += "je ModDiv_z" + "\n";
            code += "jl ModDiv_neg" + "\n";
            code += "jmp ModNorm_case" + "\n";
            code += "ModRem_neg:" + "\n";
            code += "cmp ebx, 0" + "\n";
            code += "je ModDiv_z" + "\n";
            code += "jl ModRem_Div_neg" + "\n";
            code += "mov edx, ebx" + "\n";
            code += "xor ebx, ebx" + "\n";
            code += "sub ebx, edx" + "\n";
            code += "ModRem_neg_loop:" + "\n";
            code += "cmp eax, ebx" + "\n";
            code += "jg ModDiv_end" + "\n";
            code += "sub eax, ebx" + "\n";
            code += "dec ecx" + "\n";
            code += "jmp ModRem_neg_loop" + "\n";
            code += "ModDiv_neg:" + "\n";
            code += "cmp eax, 0" + "\n";
            code += "jl ModRem_Div_neg" + "\n";
            code += "mov edx, ebx" + "\n";
            code += "xor ebx, ebx" + "\n";
            code += "sub ebx, edx" + "\n";
            code += "ModDiv_neg_loop:" + "\n";
            code += "cmp eax, ebx" + "\n";
            code += "jl ModDiv_end" + "\n";
            code += "sub eax, ebx" + "\n";
            code += "dec ecx" + "\n";
            code += "jmp ModDiv_neg_loop" + "\n";
            code += "ModRem_Div_neg:" + "\n";
            code += "mov edx, eax" + "\n";
            code += "xor eax, eax" + "\n";
            code += "sub eax, edx" + "\n";
            code += "mov edx, ebx" + "\n";
            code += "xor ebx, ebx" + "\n";
            code += "sub ebx, edx" + "\n";
            code += "ModRem_Div_neg_loop:" + "\n";
            code += "cmp eax, ebx" + "\n";
            code += "jl ModDiv_end" + "\n";
            code += "sub eax, ebx" + "\n";
            code += "inc ecx" + "\n";
            code += "jmp ModRem_Div_neg_loop" + "\n";
            code += "ModNorm_case:" + "\n";
            code += "cmp eax, ebx" + "\n";
            code += "jl ModDiv_end" + "\n";
            code += "sub eax, ebx" + "\n";
            code += "inc ecx" + "\n";
            code += "jmp ModNorm_case" + "\n";
            code += "ModDiv_z:" + "\n";
            code += "mov ecx, 0" + "\n";
            code += "ModDiv_end:" + "\n";
            code += "ret\n";
            code += "ModProc endp" + "\n\r";
            code += "OutInt proc " + "\n";
            code += "mov edx, eax\n";
            code += "and edx, 80000000h\n";
            code += "cmp edx, 80000000h\n";
            code += "jne oi_1\n";
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
