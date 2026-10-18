using Microsoft.CSharp;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Reflection;

namespace DynCode
{
    public enum CodeLanguage
    {
        CSharp,
        FSharp
    }

    public class CodeHelper
    {
        public static object HelperFunction(string classCode, string mainClass, object[] requiredAssemblies, CodeLanguage language = CodeLanguage.CSharp)
        {
            CodeDomProvider provider = GetProvider(language);

            CompilerParameters parameters = new CompilerParameters
            {
                GenerateExecutable = false,       // Create a dll
                GenerateInMemory = true,          // Create it in memory
                WarningLevel = 3,                 // Default warning level
                CompilerOptions = "/optimize",    // Optimize code
                TreatWarningsAsErrors = false     // Better be false to avoid break in warnings
            };

            //----------------
            // Add basic referenced assemblies
            parameters.ReferencedAssemblies.Add("system.dll");
            parameters.ReferencedAssemblies.Add("System.Windows.Forms.dll");
            parameters.ReferencedAssemblies.Add("System.Core.dll");

            if (language == CodeLanguage.FSharp)
            {
                parameters.ReferencedAssemblies.Add("mscorlib.dll");
                parameters.ReferencedAssemblies.Add("FSharp.Core.dll");
            }

            //----------------
            // Add all extra assemblies required
            if (requiredAssemblies != null)
            {
                foreach (var extraAsm in requiredAssemblies)
                {
                    if (extraAsm is string asmPath && !string.IsNullOrWhiteSpace(asmPath))
                    {
                        parameters.ReferencedAssemblies.Add(asmPath);
                    }
                }
            }

            //--------------------
            // Try to compile the code received
            CompilerResults results = provider.CompileAssemblyFromSource(parameters, classCode);

            //--------------------
            // If the compilation returned error, then return the CompilerErrorCollection class with the errors to the caller
            if (results.Errors != null && results.Errors.Count != 0)
            {
                return results.Errors;
            }

            //--------------------
            // Return the created class instance to caller
            return results.CompiledAssembly.CreateInstance(mainClass);
        }

        private static CodeDomProvider GetProvider(CodeLanguage language)
        {
            switch (language)
            {
                case CodeLanguage.CSharp:
                    return new CSharpCodeProvider(new Dictionary<string, string> { { "CompilerVersion", "v4.0" } });

                case CodeLanguage.FSharp:
                    return GetFSharpProvider();

                default:
                    throw new NotSupportedException(string.Format("Language '{0}' is not supported.", language));
            }
        }

        private static CodeDomProvider GetFSharpProvider()
        {
            // 1. Try finding FSharpCodeProvider via reflection from FSharp.Compiler.CodeDom
            Type fsharpType = Type.GetType("FSharp.Compiler.CodeDom.FSharpCodeProvider, FSharp.Compiler.CodeDom", false)
                ?? Type.GetType("Microsoft.FSharp.Compiler.CodeDom.FSharpCodeProvider, FSharp.Compiler.CodeDom", false);

            if (fsharpType == null)
            {
                try
                {
                    Assembly asm = Assembly.Load("FSharp.Compiler.CodeDom");
                    if (asm != null)
                    {
                        fsharpType = asm.GetType("FSharp.Compiler.CodeDom.FSharpCodeProvider")
                            ?? asm.GetType("Microsoft.FSharp.Compiler.CodeDom.FSharpCodeProvider");
                    }
                }
                catch
                {
                    // Ignore assembly load failure and try CodeDom registration below
                }
            }

            if (fsharpType != null)
            {
                return (CodeDomProvider)Activator.CreateInstance(fsharpType);
            }

            // 2. Try configured CodeDom providers ("fs" or "F#")
            try
            {
                if (CodeDomProvider.IsDefinedLanguage("fs"))
                {
                    return CodeDomProvider.CreateProvider("fs");
                }
                if (CodeDomProvider.IsDefinedLanguage("F#"))
                {
                    return CodeDomProvider.CreateProvider("F#");
                }
            }
            catch { }

            throw new InvalidOperationException(
                "F# CodeDom provider was not found. Please ensure 'FSharp.Compiler.CodeDom' NuGet package or F# CodeDom provider is installed and referenced."
            );
        }
    }
}