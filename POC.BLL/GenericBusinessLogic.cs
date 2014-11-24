using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using POC.DataBase;
using POC.DAL;
using POC.BLL.DataModel;

namespace POC.BLL
{
    public class GenericBusinessLogic
    {
        public ServiceTaxResults GetTax(int ordertypeId, int channelId, string orderDate, string orderList)
        {
            try
            {
                ServiceTaxResults tax = new ServiceTaxResults();

                return tax;

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

        private bool HasTax(int ordertypeId)
        {
             using (var locator = new GenericRepository<TAX>())
            {
                return locator.Find(c => c.ORDERTYPE_ID == ordertypeId).Count() > 0;
             }
        }

        private bool HasTaxExclusion(int ordertypeId)
        {
            using (var locator = new GenericRepository<TAXEXCLUSIONS>())
            {
                return locator.Find(c => c.ORDERTYPE_ID == ordertypeId).Count() > 0;
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

        private double GetTaxResult() 
        {
            return 0;
        }

    }
}
