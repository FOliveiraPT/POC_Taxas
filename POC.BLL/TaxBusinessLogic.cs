using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Linq;
using System.Xml.XPath;
using Microsoft.CSharp;
using POC.BLL.DataModel;
using POC.BLL.DataModel.Enums;
using POC.Common;
using POC.DAL;
using POC.DataBase;

namespace POC.BLL
{
    public class TaxBusinessLogic
    {
        /// <summary>
        /// Method that will return via webservice call, the DataModel representating a Tax.
        /// </summary>
        /// <param name="orderTypeId">The orderTypeId to wich the order refers too.</param>
        /// <param name="channelId">The identification of the channel that is calling the function.</param>
        /// <param name="orderList">The xml representation of the order.</param>
        /// <param name="forView">A flag validation to validate the calculation or not of a formula.</param>
        /// <returns>A ServiceTaxResults object.</returns>
        public ServiceTaxResults GetTax(int orderTypeId, int channelId, string orderList, bool forView = false)
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

                                foreach (var item in elegibleItems)
                                {
                                    taxResult.Unidades = 1;
                                    if (item.FormulaId.HasValue)
                                    {
                                        var formulaResult = ParseFormula(GetFormula(item.FormulaId.Value), orderList);

                                        if (formulaResult.GetType().Equals(typeof(Int32)))
                                        {
                                            taxResult.Unidades = Convert.ToInt32(formulaResult);
                                            taxResult.Valor_Unitário = Convert.ToDouble(tax.VALUE.Value);
                                            taxValue = CalculateValue(Convert.ToDecimal(taxResult.Unidades) * tax.VALUE.Value, discount);
                                        }
                                        else if (formulaResult.GetType().Equals(typeof(Decimal)))
                                        {
                                            taxResult.Valor_Unitário = Convert.ToDouble(formulaResult);
                                            taxValue = CalculateValue(formulaResult, 0, discount);
                                        }
                                        break;
                                    }
                                    else
                                    {
                                        taxResult.Unidades = ConvertToInt(item.FormItemValue);
                                        taxResult.Valor_Unitário = Convert.ToDouble(tax.VALUE.Value);
                                        taxValue = CalculateValue(Convert.ToDecimal(taxResult.Unidades) * tax.VALUE.Value, discount);
                                    }
                                }
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

        /// <summary>
        /// Method that will get the elements that can be used on the calculation of the tax.
        /// </summary>
        /// <param name="orderList">string representation of the XML.</param>
        /// <param name="taxCondList">the collection of the tax conditions, including the restrictions.</param>
        /// <param name="passedOnAllConditions">flag that will be returned, indicating that this tax can be calculated.</param>
        /// <returns>A collection of ElegibleItems.</returns>
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

            #endregion First Step

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

            #endregion Second Step

            return elegibleItems;
        }

        /// <summary>
        /// Method that parses the string representation of a formula, to a C# code
        /// that will allow the execution of that same formula.
        /// </summary>
        /// <param name="formula">Formula object that will be considered.</param>
        /// <param name="xml">A string that contains the xml representation.</param>
        /// <returns>The result value of the formula parsing.</returns>
        private Object ParseFormula(FORMULAS formula, string xml)
        {
            Object returnedValue = null;

            XDocument xDoc = XDocument.Parse(xml);
            var oper = GetOperator(formula.FORMULA);
            var functionCode = new StringBuilder();
            switch (oper.VALUETYPE_ID)
            {
                case (int)EnumTax.OperatorValueTypes.Date:
                    var xElementFirst = xDoc.XPathSelectElements(String.Format("//Object/Field[Name = '{0}']", StringEnum.GetStringValue(EnumTax.XMLFieldNames.FromDate)))
                        .Descendants("Value").SingleOrDefault();
                    var xElementSecond = xDoc.XPathSelectElements(String.Format("//Object/Field[Name = '{0}']", StringEnum.GetStringValue(EnumTax.XMLFieldNames.ToDate)))
                        .Descendants("Value").SingleOrDefault();

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
                                                    xElementFirst,
                                                    xElementSecond,
                                                    (int)((EnumTax.Formula)Enum.Parse(typeof(EnumTax.Formula), formula.FORMULA))));
                    functionCode.Append("){");
                    functionCode.Append(String.Format("numberOfDays = ((Convert.ToDateTime(\"{0}\")) - (Convert.ToDateTime(\"{1}\"))).Days;", xElementFirst, xElementSecond));
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
                    //TODO
                    break;

                case (int)EnumTax.OperatorValueTypes.Numeric:
                    //TODO
                    break;
            }

            return returnedValue;
        }

        /// <summary>
        /// Method that finds an element on a xml file, according to the parameters defined.
        /// </summary>
        /// <param name="option">Type of enum to be used to get the field of the xml file.</param>
        /// <param name="xDoc">XML file.</param>
        /// <param name="fieldName">The name of the element.</param>
        /// <param name="id">The id that matches the numeric value on the enum selection.</param>
        /// <returns>The value of the element.</returns>
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

        /// <summary>
        /// Method that makes an arithmetic operation.
        /// </summary>
        /// <param name="originalTaxValue">Value of the tax.</param>
        /// <param name="discount">Discount to apply.</param>
        /// <returns>Value of tax after discount.</returns>
        private decimal CalculateValue(decimal originalTaxValue, double? discount)
        {
            return discount.HasValue ? CalculateDiscount(originalTaxValue, discount.Value) : originalTaxValue;
        }

        /// <summary>
        /// Method that makes an arithmetic operation.
        /// </summary>
        /// <param name="formulaResult">According to the formulaResult type, the calculation will be different.</param>
        /// <param name="originalTaxValue">Value of the tax.</param>
        /// <param name="discount">Discount to apply.</param>
        /// <returns>Value of tax after discount.</returns>
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

        /// <summary>
        /// Method that makes an arithmetic operation.
        /// </summary>
        /// <param name="taxValue">Value of the tax.</param>
        /// <param name="discount">Discount to apply.</param>
        /// <returns>Value of discount to be applied.</returns>
        private decimal CalculateDiscount(decimal taxValue, double discount)
        {
            return taxValue - (taxValue * Convert.ToDecimal(discount));
        }

        #endregion Math Operations

        #region DAL Access

        /// <summary>
        /// Method that returns a collection of TaxCond objects
        /// </summary>
        /// <param name="taxId">Value to be used as a filter on the ID field.</param>
        /// <returns>A collection of TAXCONDS</returns>
        private IEnumerable<TAXCONDS> GetTaxCondsByTaxId(int taxId)
        {
            using (var locator = new GenericRepository<TAXCONDS>())
            {
                return locator.Find(c => c.TAX_ID == taxId);
            }
        }

        /// <summary>
        /// Method that returns a operator object.
        /// </summary>
        /// <param name="operatorName">The name of the operator.</param>
        /// <returns>The operator that matches the condition.</returns>
        private OPERATORS GetOperator(string operatorName)
        {
            using (var locator = new GenericRepository<OPERATORS>())
            {
                return locator.Single(c => c.OPERATOR.Equals(operatorName));
            }
        }

        /// <summary>
        /// Method that returns a formula object.
        /// </summary>
        /// <param name="idFormula">Value to be used as a filter on the ID field.</param>
        /// <returns>The formula that matches the condition.</returns>
        private FORMULAS GetFormula(int idFormula)
        {
            using (var locator = new GenericRepository<FORMULAS>())
            {
                return locator.Single(c => c.ID == idFormula);
            }
        }

        /// <summary>
        /// Method that returns a tax collection.
        /// </summary>
        /// <param name="orderTypeId">OrderTypeId to be used as a filter.</param>
        /// <param name="channelId">Identification of the channel that is calling the method.</param>
        /// <returns>A collection of TAXES/returns>
        private IEnumerable<TAXES> GetTaxes(int orderTypeId, int channelId)
        {
            using (var locator = new GenericRepository<TAXES>())
            {
                return locator.Find(c => c.ORDERTYPE_ID == orderTypeId
                    && c.CHANNEL_ID == channelId);
            }
        }

        /// <summary>
        /// Method that returns a value refering to the discount to be made.
        /// </summary>
        /// <param name="taxId">Identification of the Tax.</param>
        /// <returns>The value of the discount to be applied.</returns>
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

        /// <summary>
        /// Method that returns the existance of a Tax.
        /// </summary>
        /// <param name="orderTypeId">OrderTypeId to be used as a filter.</param>
        /// <returns>Flag that returns a bool result</returns>
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

        #endregion DAL Access

        #region Compilation at runtime
        /// <summary>
        /// Method that allows the creation of a method at runtime, 
        /// that can be called from other members of the class, exclusively.
        /// </summary>
        /// <param name="script">C# code</param>
        /// <returns>MethodInfo that will be called from the other members.</returns>
        private MethodInfo CreateFunction(string script)
        {
            Assembly t = Compile(script);

            CSharpCodeProvider provider = new CSharpCodeProvider();
            CompilerResults results = provider.CompileAssemblyFromSource(new CompilerParameters(), script);

            Type binaryFunction = results.CompiledAssembly.GetType("ConsoleApplication.DateFunctionValidation");
            return binaryFunction.GetMethod("Function");
        }

        /// <summary>
        /// Compiles at runtime a given C# script code.
        /// </summary>
        /// <param name="script">C# code</param>
        /// <returns>An assembly to be used</returns>
        private Assembly Compile(string script)
        {
            CompilerParameters options = new CompilerParameters();
            options.GenerateExecutable = false;
            options.GenerateInMemory = true;
            options.ReferencedAssemblies.Add("System.dll");

            Microsoft.CSharp.CSharpCodeProvider provider = new Microsoft.CSharp.CSharpCodeProvider();
            CompilerResults result = provider.CompileAssemblyFromSource(options, script);

            using (StringWriter sw = new StringWriter())
            {
                // Check the compiler results for errors
                foreach (CompilerError ce in result.Errors)
                {
                    if (ce.IsWarning) continue;
                    sw.WriteLine("{0}({1},{2}: error {3}: {4}", ce.FileName, ce.Line, ce.Column, ce.ErrorNumber, ce.ErrorText);
                }
                // If there were errors, raise an exception...
                string errorText = sw.ToString();
                if (errorText.Length > 0)
                    throw new ApplicationException(errorText);
            }
            return result.CompiledAssembly;
        }

        #endregion Compilation at runtime

        /// <summary>
        /// Method that converts a string to an int, taking on consideration the existance of previous known cases
        /// that have strings attached to numerical values.
        /// </summary>
        /// <param name="value">string value that will be converted.</param>
        /// <returns>the value converted.</returns>
        private int ConvertToInt(string value)
        {
            string[] arrayStringExceptions = new string[] { "m2" };
            var splitted = value.Split(arrayStringExceptions, StringSplitOptions.None).FirstOrDefault();

            return Convert.ToInt32(splitted != String.Empty ? splitted : value);
        }
    }
}