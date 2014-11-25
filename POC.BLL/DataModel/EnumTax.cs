using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using POC.Common;

namespace POC.BLL.DataModel.Enums
{
    public class EnumTax
    {
        public enum OperationType
        {
            Multiply,
            Compare
        }

        public enum ElegibleItemType
        {
            Field,
            CalculatedField,
            Formula
        }

        public enum XMLFieldNames
        {
            [StringValue("--")]
            NoOption = 0,
            [StringValue("Área de Construção")]
            ConstructionArea = 1,
            [StringValue("Finalidade das cópias")]
            CopyReason = 2,
            [StringValue("NULL")]
            NoText = 3,
            [StringValue("Número de equipamentos:")]
            NumberOfEquipment = 4,
            [StringValue("Número de lotes")]
            NumberOfAllotment = 5,
            [StringValue("Objecto do pedido")]
            OrderObject = 6,
            [StringValue("Operação Urbanística")]
            UrbanisticOperation = 7,
            [StringValue("Tipo de Obra")]
            ConstructionType = 8,
            [StringValue("Tipo de Pedido")]
            OrderType = 9,
            [StringValue("Tipo Requerimento")]
            RequirementType = 10,
            [StringValue("Área de Construção")]
            ConstructionArea_2 = 11,
            [StringValue("Objecto do Pedido")]
            OrderObject_2 = 12,
            [StringValue("Operação Urbanística")]
            UrbanisticOperation_2 = 13,
            [StringValue("Produtos alimentares e bebidas")]
            FoodAndBeverage = 14,
            [StringValue("Tipo de Aedido")]
            OrderType_2 = 15,
            [StringValue("Tipo de certidão a emitir")]
            CertificateTypeToEmit = 16,
            [StringValue("Tipo de Operação")]
            OperationType = 17,
            [StringValue("Tipo de pedido:")]
            OrderType_3 = 18,
            [StringValue("Tipo de produtos de venda: ")]
            ProductsTypeForSale = 19,
            [StringValue("Tipo de requisição:")]
            RequisitionType = 20
        }
        /*
        public enum XMLFieldValues
        {
            TESTE
        }*/
    }
}