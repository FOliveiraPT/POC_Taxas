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
        public TAX GetTax(int orderType, int channelID)
        {
            using (var locator = new GenericRepository<TAX>())
            {
                /*
                 * TERÁ DE SE PEGAR NOS CAMPOS DAS TAX COND E CONSOANTE O OPERADOR, FAZER O TESTE NO VALUE
                 * PODERÁ SE USAR AS QUERY DINÂMICAS
                 */
                var teste = locator.First(c => c.TAX_ORDERTYPEID == 48);
                return teste;
            }
        }

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
            return false;
        }

        private bool HasTaxExclusion(int ordertypeId)
        {
            return false;
        }

        private List<FORMULAS> GetFormulas(int taxcondId)
        {
            return null;
        }

        private double GetDiscount(int taxId)
        {
            return 0;
        }

        private double GetTaxResult() 
        {
            return 0;
        }

    }
}
