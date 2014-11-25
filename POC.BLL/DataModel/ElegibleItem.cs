using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace POC.BLL.DataModel
{
    public class ElegibleItem
    {
        public POC.BLL.DataModel.Enums.EnumTax.ElegibleItemType ItemType { get; set; }
        public POC.BLL.DataModel.Enums.EnumTax.XMLFieldNames? XmlFieldName { get; set; }
        public string FormItemName { get; set; }
        public string FormItemValue { get; set; }
        public int? FormulaId { get; set; }
    }
}
