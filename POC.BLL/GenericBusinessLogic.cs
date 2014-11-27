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
using System.Xml.XPath;
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

                // Check if has tax associated
                if (HasTax(orderTypeId))
                {
                    foreach (var tax in GetTaxes(orderTypeId, channelId))
                    {
                        var taxResult = new GetTax_Result();
                        var discount = GetDiscount(tax.ID);
                        decimal taxValue = 0;

                        if (tax.FORMULA_ID.HasValue)
                        {
                            var formulaResult = ParseFormula(GetFormula(tax.FORMULA_ID.Value), orderList);

                            if (formulaResult.GetType().Equals(typeof(Int32)))
                            {
                                taxResult.Unidades = Convert.ToInt32(formulaResult);
                                taxResult.Valor_Unitário = Convert.ToDouble(tax.VALUE.Value);
                            }

                            taxValue = CalculateValue(taxResult, tax.VALUE.Value, discount);
                        }
                        else
                        {
                            //Check if it has TaxConds
                            var taxConds = GetTaxCondsByTaxId(tax.ID);

                            //If has TaxConds then will check for exclusions and calculate the Tax
                            if (taxConds.Count() > 0)
                            {
                                bool passedOnAllConditions = true;
                                var elegibleItems = GetElegibleItems(orderList, taxConds, out passedOnAllConditions);

                                //If doesn't pass on all conditions due to restrictions or not found elements,
                                //no tax value will be defined
                                if (!passedOnAllConditions)
                                    break;

                                //foreach (var item in elegibleItems)
                                //{
                                //    if (item.FormulaId.HasValue)
                                //    {
                                //        break;
                                //    }
                                //    else
                                //    {
                                //        taxResult.Unidades = Convert.ToInt32(item.FormItemValue);
                                //        taxResult.Valor_Unitário = Convert.ToDouble(tax.VALUE.Value);


                                //    }
                                //}
                            }
                            else
                            {
                                //If it doesn't have conditions, make the operations to get the final value
                                taxValue = CalculateValue(tax.VALUE.Value, discount);
                                taxResult.Unidades = 1; //Default value
                                taxResult.Valor_Unitário = Convert.ToDouble(tax.VALUE.Value); //Default value
                            }
                        }

                        taxResult.Taxa = tax.NAME;
                        taxResult.TAX_DISCOUNT = discount;
                        taxResult.TOTAL = Convert.ToDouble(taxValue);
                        results.taxResults.Add(taxResult);
                        /*


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
                        }*/
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

        private List<ElegibleItem> GetElegibleItems(string orderList, IEnumerable<TAXCONDS> taxCondList, out bool passedOnAllConditions)
        {
            XDocument xDoc = XDocument.Parse(orderList);
            var elegibleItems = new List<ElegibleItem>();
            passedOnAllConditions = true;

            #region First Step
            //First we need to check if there are exclusions, and if they are caught!
            using (var locator = new GenericRepository<TAXEXCLUSIONS>())
            {
                foreach (var item in taxCondList)
                {
                    var taxExclusion = locator.Single(c => c.TAXCOND_ID == item.ID);
                    if (taxExclusion != null)
                    {
                        var resultXPath = XPathString(EnumTax.XPathOptions.TaxExclusionField, xDoc, null, taxExclusion.ID);
                        if (!String.IsNullOrEmpty(resultXPath) || !String.IsNullOrWhiteSpace(resultXPath))
                        {
                            passedOnAllConditions = false;
                            break;
                        }
                    }
                }
            }
            #endregion

            #region Second Step
            //Now will validate if the tax cond are present, if it isn't, no result will be returned!
            //We will iterate the taxCondList to test the conditions, if one condition is not matched,
            //then we will break the cycle and send passedAllConditions to false
            foreach (var item in taxCondList)
            {
                var resultXPath = XPathString(EnumTax.XPathOptions.XmlFormField, xDoc, item.FORM_ITEM_NAME);

                //Test if the form_item_value is equal in file and database
                if (!String.IsNullOrEmpty(item.FORM_ITEM_VALUE) || !String.IsNullOrWhiteSpace(item.FORM_ITEM_VALUE))
                {
                    if (!(item.VALUE_IS_EQUAL == resultXPath.Equals(item.FORM_ITEM_VALUE)))
                    {
                        passedOnAllConditions = false;
                        break;
                    }

                    if (item.FORMULA_ID.HasValue)
                    {
                        elegibleItems.Add(new ElegibleItem()
                            {
                                FormItemName = item.FORM_ITEM_NAME,
                                FormItemValue = item.FORM_ITEM_VALUE,
                                FormulaId = item.FORMULA_ID.Value
                            }
                        );
                    }
                }
                else
                {
                    elegibleItems.Add(new ElegibleItem()
                            {
                                FormItemName = item.FORM_ITEM_NAME,
                                FormItemValue = resultXPath
                            }
                         );
                }
            }
            #endregion

            return elegibleItems;
        }

        private Object ParseFormula(FORMULAS formula, string xml)
        {
            Object returnedValue = null;

            XDocument xDoc = XDocument.Parse(xml);
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
                    functionCode.Append(String.Format("else { numberOfDays = Enum.Parse(typeof(EnumTax.Formula), formula.FORMULA) == EnumTax.Formula.Q46 ? ((Convert.ToDateTime(\"{0}\")) - (Convert.ToDateTime(\"{1}\"))).Days - {0} : {0};}", (int)((EnumTax.Formula)Enum.Parse(typeof(EnumTax.Formula), formula.FORMULA))));
                    functionCode.Append("return numberOfDays;");
                    functionCode.Append("}");
                    functionCode.Append("}");
                    functionCode.Append("}");

                    var delegatedFunction = (Func<int>)Delegate.CreateDelegate(typeof(Func<int>), CreateFunction(functionCode.ToString()));
                    returnedValue = delegatedFunction();
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

            return returnedValue;
        }

        private string XPathString(POC.BLL.DataModel.Enums.EnumTax.XPathOptions option, XDocument xDoc, string fieldName, int id = 0)
        {
            XElement xElement = null;

            switch (option)
            {
                case EnumTax.XPathOptions.TaxExclusionField:
                    xElement = xDoc.XPathSelectElements(
                            StringEnum.GetStringValue((EnumTax.TaxExclusions)Enum.Parse(typeof(EnumTax.TaxExclusions), id.ToString()))
                        )
                        .Descendants("Value").SingleOrDefault();
                    break;
                case EnumTax.XPathOptions.XmlFormField:
                    xElement = xDoc.XPathSelectElements(String.Format("//Object/Field[Name = '{0}']", fieldName))
                        .Descendants("Value").SingleOrDefault();
                    break;
            }

            if (xElement != null)
                return xElement.Value;

            return string.Empty;
        }

        #region Math Operations
        private decimal CalculateValue(decimal originalTaxValue, double? discount)
        {
            return discount.HasValue ? CalculateDiscount(originalTaxValue, discount.Value) : originalTaxValue;
        }

        private decimal CalculateValue(object formulaResult, decimal originalTaxValue, double? discount)
        {
            decimal taxResultValue, taxValue = 0;
            if (formulaResult.GetType().Equals(typeof(Int32)))
            {
                taxResultValue = Convert.ToDecimal(formulaResult) * originalTaxValue;
                taxValue = discount.HasValue ? CalculateDiscount(taxResultValue, discount.Value) : taxResultValue;
            }
            else if (formulaResult.GetType().Equals(typeof(Decimal)))
            {
                taxValue = discount.HasValue ? CalculateDiscount(Convert.ToDecimal(formulaResult), discount.Value) : Convert.ToDecimal(formulaResult);
            }

            return taxValue;
        }

        private decimal CalculateDiscount(decimal taxValue, double discount)
        {
            return taxValue - (taxValue * Convert.ToDecimal(discount));
        }
        #endregion

        #region DAL Access
        private IEnumerable<TAXCONDS> GetTaxCondsByTaxId(int taxId)
        {
            using (var locator = new GenericRepository<TAXCONDS>())
            {
                return locator.Find(c => c.TAX_ID == taxId);
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

        private IEnumerable<TAXES> GetTaxes(int orderTypeId, int channelId)
        {
            using (var locator = new GenericRepository<TAXES>())
            {
                return locator.Find(c => c.ORDERTYPE_ID == orderTypeId
                    && c.CHANNEL_ID == channelId);
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
        #endregion

        #region Compilation at runtime
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