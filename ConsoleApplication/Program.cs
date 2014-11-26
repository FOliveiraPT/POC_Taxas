using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using POC.BLL;
using System.Xml.Linq;
using POC.Common;
using POC.BLL.DataModel.Enums;
using Microsoft.CSharp;
using System.CodeDom.Compiler;
using System.Reflection;
using System.IO;

namespace ConsoleApplication
{
    class Program
    {
        static void Main(string[] args)
        {
            var t = TesteDinamico();
        }

        static int TesteDinamico()
        {
            var formula = "Q15";
            XDocument xDoc = XDocument.Load("Teste.xml");

            //ALTERAR PARA XPATH
            var tempElement = xDoc.Descendants("Field").Elements("Name").SingleOrDefault(x => x.Value == "De").NextNode;
            var elementInitialDate = (string)((XElement)tempElement);
            tempElement = xDoc.Descendants("Field").Elements("Name").SingleOrDefault(x => x.Value == "até").NextNode;
            var elementFinalDate = (string)((XElement)tempElement);

            var functionCode = new StringBuilder();
            functionCode.Append(@"using System;
                namespace ConsoleApplication{
                                        public class DateFunctionValidation
                                        {
                                            public static int Function()
                                            {
	                                            var numberOfDays = 0;
	
	                                            if(");
            functionCode.Append(String.Format(StringEnum.GetStringValue(
                                            (EnumTax.Formula)Enum.Parse(typeof(EnumTax.Formula), formula)),
                                            elementFinalDate,
                                            elementInitialDate,
                                            (int)((EnumTax.Formula)Enum.Parse(typeof(EnumTax.Formula), formula))));
            functionCode.Append("){");
            functionCode.Append(String.Format("numberOfDays = ((Convert.ToDateTime(\"{0}\")) - (Convert.ToDateTime(\"{1}\"))).Days;", elementFinalDate, elementInitialDate));
            functionCode.Append("}");
            functionCode.Append("return numberOfDays;");
            functionCode.Append("}");
            functionCode.Append("}");
            functionCode.Append("}");

            var delegatedFunction = (Func<int>)Delegate.CreateDelegate(typeof(Func<int>), CreateFunction(functionCode.ToString()));
            return delegatedFunction();
        }

        static MethodInfo CreateFunction(string script)
        {
            Assembly t = Compile(script);

            CSharpCodeProvider provider = new CSharpCodeProvider();
            CompilerResults results = provider.CompileAssemblyFromSource(new CompilerParameters(), script);

            Type binaryFunction = results.CompiledAssembly.GetType("ConsoleApplication.DateFunctionValidation");
            return binaryFunction.GetMethod("Function");
        }

        #region Compile - Código visto por fontes externas
        static Assembly Compile(string script)
        {
            CompilerParameters options = new CompilerParameters();
            options.GenerateExecutable = false;
            options.GenerateInMemory = true;
            options.ReferencedAssemblies.Add("System.dll");

            Microsoft.CSharp.CSharpCodeProvider provider = new Microsoft.CSharp.CSharpCodeProvider();
            CompilerResults result = provider.CompileAssemblyFromSource(options, script);

            // Check the compiler results for errors
            StringWriter sw = new StringWriter();
            foreach (CompilerError ce in result.Errors)
            {
                if (ce.IsWarning) continue;
                sw.WriteLine("{0}({1},{2}: error {3}: {4}", ce.FileName, ce.Line, ce.Column, ce.ErrorNumber, ce.ErrorText);
            }
            // If there were errors, raise an exception...
            string errorText = sw.ToString();
            if (errorText.Length > 0)
                throw new ApplicationException(errorText);

            return result.CompiledAssembly;
        }
        #endregion
    }
}
