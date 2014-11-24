using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace POC.BLL.DataModel
{
    public class CalculableItem
    {
        public POC.BLL.DataModel.Enum.OperationType Operation { get; set; }
        public int NumberOccurences { get; set; }
        public double ValueByOccurences { get; set; }
    }
}
