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
                                    /*
                                    EnumTax.ElegibleItemType.Formula:
                                        taxValue = CalculateFormula();*/
                                    break;
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
            using (var locator = new GenericRepository<TAX>())
            {
                return locator.Find(
                    c => c.ORDERTYPE_ID == orderTypeId
                    && !c.END_DATE.HasValue
                    ).Count() > 0;
            }
        }

        private IEnumerable<TAX> GetTaxes(int orderTypeId)
        {
            using (var locator = new GenericRepository<TAX>())
            {
                return locator.Find(c => c.ORDERTYPE_ID == orderTypeId);
            }
        }


        private int GetTaxId(int orderTypeId)
        {
            using (var locator = new GenericRepository<TAX>())
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

            using (var locator = new GenericRepository<TAXCOND>())
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

        private List<FORMULAS> GetFormulas(int taxcondId)
        {
            var listFormulas = new List<FORMULAS>();
            using (var locatorTaxCond = new GenericRepository<TAXCOND>())
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
            using (var locator = new GenericRepository<TAX>())
            {
                var tax = locator.Single(c => c.ID == taxId);

                using (var loc = new GenericRepository<DISCOUNT>())
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

        private decimal CalculateFormula()
        {

            return 0;
        }
    }
}
