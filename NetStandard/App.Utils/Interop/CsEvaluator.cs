using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Text;
using System.Reflection;
using System.Collections.Generic;
using Microsoft.CSharp;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.IO;
using System.Collections;

namespace App.Utils
{
    /// <summary>
    /// C# ���ʽ������
    /// From: http://www.codeproject.com/csharp/runtime_eval.asp
    /// netcore ʹ�� Roslyn ���б��루��� CodeDom��
    /// </summary>
    public class CsEvaluator : Evaluator
    {
        /// <summary>CSharp ���ʽ��ֵ</summary>
        /// <param name="expression">CSharp ���ʽ���磺2.5, DateTime.Now</param>
        public override object Eval(string expression)
        {
            // ����
            var text = string.Format(@"
                using System;
                public class Calculator
                {{
                    public static object Evaluate() {{ return {0}; }}
                }}", expression);

            // �������ɳ���
            var tree = SyntaxFactory.ParseSyntaxTree(text);
            var compilation = CSharpCompilation.Create(
                "calc.dll",
                new[] { tree },
                options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary),
                references: new[] { 
                    MetadataReference.CreateFromFile(typeof(object).Assembly.Location)
                });
            Assembly compiledAssembly;
            using (var stream = new MemoryStream())
            {
                var compileResult = compilation.Emit(stream);
                compiledAssembly = Assembly.Load(stream.GetBuffer());
            }

            // �÷���ִ�з���
            var calculatorClass = compiledAssembly.GetType("Calculator");
            var evaluateMethod = calculatorClass.GetMethod("Evaluate");
            return evaluateMethod.Invoke(null, null);
        }
    }
}

