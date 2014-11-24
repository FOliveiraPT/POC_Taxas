using System.Collections.Generic;

namespace POC.BLL.DataModel
{
    [System.Serializable]
    public class ServiceTaxResults
    {
        public List<GetTax_Result> taxResults { get; set; }
    }
}