using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace POC.BLL.DataModel
{
    public class GetTax_Result
    {
        public string Taxa { get; set; }
        public double? Valor_Unitário { get; set; }
        public double? Unidades { get; set; }
        public double? TOTAL { get; set; }
        public double? TAX_DISCOUNT { get; set; }
    }
}