using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using POC.DataBase;
using POC.DAL;

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

    }
}
