using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using POC.BLL.DataModel.Enums;

namespace POC.BLL.DataModel
{
    public class CalculableItem
    {
        public EnumTax.OperationType Operation { get; set; }
        public int NumberOccurences { get; set; }
        public double ValueByOccurences { get; set; }
    }
}
