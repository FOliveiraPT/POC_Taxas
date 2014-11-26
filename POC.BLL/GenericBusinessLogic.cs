using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using POC.DataBase;
using POC.Common;
using POC.DAL;
using POC.BLL.DataModel;
using POC.BLL.DataModel.Enums;
using System.Xml.Linq;
using System.Reflection;
using Microsoft.CSharp;
using System.CodeDom.Compiler;
using System.IO;

namespace POC.BLL
{
    public class GenericBusinessLogic
    {
        public ServiceTaxResults GetTax(int orderTypeId, int channelId, string orderDate, string orderList, bool forView = false)
        {
            try
            {
                ServiceTaxResults results = new ServiceTaxResults();

                // Verificar se tem Tax associado
                if (HasTax(orderTypeId))
                {
                    foreach (var tax in GetTaxes(orderTypeId))
                    {
                        var discount = GetDiscount(tax.ID);
                        decimal taxValue = 0;
                        bool passedOnAllConditions = true;
                        var elegibleItems = GetElegibleItems(tax.ID, orderTypeId, orderList, forView, out passedOnAllConditions);

                        if (!passedOnAllConditions)
                            break;

                        //Caso seja uma taxa de valor fixo
                        if (elegibleItems.Count == 0)
                        {
                            taxValue = discount.HasValue ? CalculateDiscount(tax.VALUE.Value, discount.Value) : tax.VALUE.Value;
                            results.taxResults.Add(new GetTax_Result()
                            {
                                Taxa = tax.NAME,
                                TAX_DISCOUNT = discount,
                                TOTAL = Convert.ToDouble(taxValue)
                            });
                        }
                        else
                        {
                            foreach (var item in elegibleItems)
                            {
                                //Neste caso o valor da taxa é "fixo", devido a ter passado em todas as comparações
                                //na relação campo/valor.
                                if (item.ItemType.CompareTo(EnumTax.ElegibleItemType.Field) == 0)
                                {
                                    taxValue = discount.HasValue ? CalculateDiscount(tax.VALUE.Value, discount.Value) : tax.VALUE.Value;
                                    results.taxResults.Add(new GetTax_Result()
                                    {
                                        Taxa = tax.NAME,
                                        TAX_DISCOUNT = discount,
                                        TOTAL = Convert.ToDouble(taxValue)
                                    });
                                    break;
                                }
                                else if (item.ItemType.CompareTo(EnumTax.ElegibleItemType.CalculatedField) == 0)
                                {
                                    taxValue = tax.VALUE.Value * Decimal.Parse(item.FormItemValue);
                                    taxValue = discount.HasValue ? CalculateDiscount(tax.VALUE.Value, discount.Value) : tax.VALUE.Value;
                                    results.taxResults.Add(new GetTax_Result()
                                    {
                                        Taxa = tax.NAME,
                                        TAX_DISCOUNT = discount,
                                        TOTAL = Convert.ToDouble(taxValue),
                                        Unidades = Double.Parse(item.FormItemValue),
                                        Valor_Unitário = Convert.ToDouble(tax.VALUE.Value)
                                    });
                                }
                                else
                                {
                                    //Calcular Formula
                                    taxValue = (RetrieveFormulaResult(ParseFormula(GetFormula(item.FormulaId.Value), orderList)));
                                }
                            }
                        }
                    }
                }
                return results;
            }
            catch (ArgumentException ae)
            {
                //Log.LogException(ae, message: "The date format is invalid, please write this format 'yyyyMMdd'");
                throw new Exception("The date format is invalid, please write this format 'yyyyMMdd'");
            }
            catch (Exception ex)
            {
                //Log.LogException(ex, message: string.Format("GetTax with OrderType: {0}, Channel: {1}, Date: {2}, XML: {3}", idOrderType, idChannel, orderDate, orderList));
                throw;
            }
        }


        private bool HasTax(int orderTypeId)
        {
            using (var locator = new GenericRepository<TAXES>())
            {
                return locator.Find(
                    c => c.ORDERTYPE_ID == orderTypeId
                    && !c.END_DATE.HasValue
                    ).Count() > 0;
            }
        }

        private IEnumerable<TAXES> GetTaxes(int orderTypeId)
        {
            using (var locator = new GenericRepository<TAXES>())
            {
                return locator.Find(c => c.ORDERTYPE_ID == orderTypeId);
            }
        }


        private int GetTaxId(int orderTypeId)
        {
            using (var locator = new GenericRepository<TAXES>())
            {
                return locator.Single(
                    c => c.ORDERTYPE_ID == orderTypeId
                    && !c.END_DATE.HasValue
                    ).ID;
            }
        }

        private List<ElegibleItem> GetElegibleItems(int taxId, int orderTypeId, string orderList, bool forView, out bool passedOnAllConditions)
        {
            XDocument xDoc = XDocument.Parse(orderList);
            var elegibleItems = new List<ElegibleItem>();
            passedOnAllConditions = true;

            using (var locator = new GenericRepository<TAXCONDS>())
            {
                // Verificar se tem TaxCond 
                var taxCondList = locator.Find(c => c.TAX_ID == taxId);

                foreach (var item in taxCondList)
                {
                    //      Se sim:
                    //          Verificar se tem TaxExclusions
                    if (!GetExclusionsByTaxCondID(item.ID, forView))
                        continue;

                    if (!item.FORMULA_ID.HasValue &&
                            (
                                !string.IsNullOrEmpty(item.FORM_ITEM_NAME) ||
                                !string.IsNullOrWhiteSpace(item.FORM_ITEM_NAME)
                            )
                        )
                    {
                        var elements = xDoc.Descendants(item.FORM_ITEM_NAME);

                        if (!string.IsNullOrEmpty(item.FORM_ITEM_VALUE) ||
                                !string.IsNullOrWhiteSpace(item.FORM_ITEM_VALUE))
                        {
                            if (item.VALUE_IS_EQUAL)
                                elements = elements.Where(c => c.Value.Equals(item.FORM_ITEM_VALUE));
                            else
                                elements = elements.Where(c => !(c.Value.Equals(item.FORM_ITEM_VALUE)));

                            //Neste caso temos uma condição que não passou nas validações, logo não é calculada a taxa
                            if (elements.Count() == 0)
                            {
                                passedOnAllConditions = false;
                                break;
                            }

                            elegibleItems.Add(new ElegibleItem()
                                    {
                                        ItemType = EnumTax.ElegibleItemType.Field,
                                        FormItemValue = elements.SingleOrDefault().Value
                                    }
                                );
                        }
                        else
                        {
                            ElegibleItem elegibleItem = null;
                            var enumerator = Enum.GetValues(typeof(EnumTax.XMLFieldNames)).GetEnumerator();
                            while (enumerator.MoveNext())
                            {
                                if (StringEnum.GetStringValue((EnumTax.XMLFieldNames)enumerator.Current) == item.FORM_ITEM_NAME)
                                {
                                    elegibleItem = new ElegibleItem()
                                        {
                                            ItemType = EnumTax.ElegibleItemType.CalculatedField,
                                            XmlFieldName = (EnumTax.XMLFieldNames)enumerator.Current,
                                            FormItemValue = elements.SingleOrDefault().Value
                                        };
                                    break;
                                }
                            }

                            if (elegibleItem == null)
                                passedOnAllConditions = false;

                            elegibleItems.Add(elegibleItem);
                        }
                    }
                    else if (item.FORMULA_ID.HasValue)
                    {
                        elegibleItems.Add(
                            new ElegibleItem()
                            {
                                ItemType = EnumTax.ElegibleItemType.Formula,
                                FormulaId = item.FORMULA_ID,
                            }
                        );
                    }
                }
            }


            return elegibleItems;
        }

        private bool GetExclusionsByTaxCondID(int taxCondId, bool forView)
        {
            using (var locator = new GenericRepository<TAXEXCLUSIONS>())
            {
                var collection = locator.Find(c => c.TAXCOND_ID == taxCondId);

                if (forView == true)
                    collection = collection.Where(c => c.FOR_VIEW == true);

                return collection.Count() > 0;
            }
        }

        private bool HasTaxExclusion(int ordertypeId, bool forView)
        {
            using (var locator = new GenericRepository<TAXEXCLUSIONS>())
            {
                return locator.Find(c => c.ORDERTYPE_ID == ordertypeId && c.FOR_VIEW == forView).Count() > 0;
            }
        }

        private OPERATORS GetOperator(string operatorName)
        {
            using (var locator = new GenericRepository<OPERATORS>())
            {
                return locator.Single(c => c.OPERATOR.Equals(operatorName));
            }
        }

        private FORMULAS GetFormula(int idFormula)
        {
            using (var locator = new GenericRepository<FORMULAS>())
            {
                return locator.Single(c => c.ID == idFormula);
            }
        }

        private List<FORMULAS> GetFormulas(int taxcondId)
        {
            var listFormulas = new List<FORMULAS>();
            using (var locatorTaxCond = new GenericRepository<TAXCONDS>())
            {
                var formulaId = locatorTaxCond.Single(c => c.ID == taxcondId).FORMULA_ID;

                if (formulaId.HasValue)
                {
                    using (var locatorFormula = new GenericRepository<FORMULAS>())
                    {
                        listFormulas.AddRange(locatorFormula.Find(c => c.ID == formulaId.Value));
                    }
                }
            }

            return listFormulas;
        }

        private double? GetDiscount(int taxId)
        {
            double? discount = null;
            using (var locator = new GenericRepository<TAXES>())
            {
                var tax = locator.Single(c => c.ID == taxId);

                using (var loc = new GenericRepository<DISCOUNTS>())
                {
                    discount = loc.Single(c => c.ID == tax.DISCOUNT_ID).VALUE;
                }
            }

            return discount;
        }

        private decimal CalculateDiscount(decimal taxValue, double discount)
        {
            return taxValue - (taxValue * Convert.ToDecimal(discount));
        }

        private IEnumerable<Object> ParseFormula(FORMULAS formula, string xml)
        {
            XDocument xDoc = XDocument.Parse(xml);
            //var operatorArray = new char[] { '+', '*', '/', '-' };
            //foreach (var op in operatorArray)
            //{
            //    //Devolve um objecto que terá uma coleção dos valores e operadores para o cálculo
            //    formula.FORMULA.Split(op);
            //}

            if (!CheckMathOperatorExistance(formula.FORMULA))
            {
                var oper = GetOperator(formula.FORMULA);
                var functionCode = new StringBuilder();

                switch (oper.VALUETYPE_ID)
                {
                    case (int)EnumTax.OperatorValueTypes.Date:
                        //ALTERAR PARA XPATH
                        var tempElement = xDoc.Descendants("Field").Elements("Name").SingleOrDefault(x => x.Value == "De").NextNode;
                        var elementInitialDate = (string)((XElement)tempElement);
                        tempElement = xDoc.Descendants("Field").Elements("Name").SingleOrDefault(x => x.Value == "até").NextNode;
                        var elementFinalDate = (string)((XElement)tempElement);

                        functionCode.Append(@"using System;
                                        namespace ConsoleApplication{
                                        public class DateFunctionValidation
                                        {
                                            public static int Function()
                                            {
	                                            var numberOfDays = 0;
	
	                                            if(");
                        functionCode.Append(String.Format(StringEnum.GetStringValue(
                                                        (EnumTax.Formula)Enum.Parse(typeof(EnumTax.Formula), formula.FORMULA)),
                                                        elementFinalDate,
                                                        elementInitialDate,
                                                        (int)((EnumTax.Formula)Enum.Parse(typeof(EnumTax.Formula), formula.FORMULA))));
                        functionCode.Append("){");
                        functionCode.Append(String.Format("numberOfDays = ((Convert.ToDateTime(\"{0}\")) - (Convert.ToDateTime(\"{1}\"))).Days;", elementFinalDate, elementInitialDate));
                        functionCode.Append("}");
                        functionCode.Append("return numberOfDays;");
                        functionCode.Append("}");
                        functionCode.Append("}");
                        functionCode.Append("}");

                        var delegatedFunction = (Func<int>)Delegate.CreateDelegate(typeof(Func<int>), CreateFunction(functionCode.ToString()));
                        delegatedFunction();


                        //Já se vê o que fazer com os dias.
                        break;
                    case (int)EnumTax.OperatorValueTypes.Monetary:
//                        functionCode = @"using System;
//                                public class DateFunctionValidation
//                                {
//                                    public static bool Function()
//                                    {
//                                        return {0};
//                                    }
//                                }";
                        break;
                    case (int)EnumTax.OperatorValueTypes.Numeric:
//                        functionCode = @"using System;
//                                public class DateFunctionValidation
//                                {
//                                    public static bool Function()
//                                    {
//                                        return {0};
//                                    }
//                                }";
                        break;
                }


            }

            return null;
        }

        private bool CheckMathOperatorExistance(string formula)
        {
            bool exist = false;
            var operatorArray = new char[] { '+', '*', '/', '-' };

            foreach (var item in operatorArray)
            {
                if (formula.Contains(item))
                {
                    exist = true;
                    break;
                }
            }

            return exist;
        }

        private decimal RetrieveFormulaResult(Object o)//IEnumerable<OPERATORS> operators)
        {
            return 0;
        }

        #region Compile - Código visto por fontes externas
        MethodInfo CreateFunction(string script)
        {
            Assembly t = Compile(script);

            CSharpCodeProvider provider = new CSharpCodeProvider();
            CompilerResults results = provider.CompileAssemblyFromSource(new CompilerParameters(), script);

            Type binaryFunction = results.CompiledAssembly.GetType("ConsoleApplication.DateFunctionValidation");
            return binaryFunction.GetMethod("Function");
        }

        Assembly Compile(string script)
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