/*
 * Copyright (c) 2026 NTTECO.
 *
 * This source code is provided for demonstration and educational purposes only.
 * It is offered "as is", without warranty of any kind, express or implied.
 *
 * The WCF and gRPC code in this repository is intentionally minimal and may not
 * represent production-ready implementations.
 *
 * **The purpose of this repository is to illustrate the development effort and
 * complexity involved in modernizing WCF services to gRPC. NTTECO is referenced
 * only as a modernization usecase category — not as a migration tool, converter,
 * or automation framework.**
 *
 * NTTECO is the Metadata-Driven Object Graph Authority Platform. No NTTECO
 * implementation is included in this repository.
 *
 * For more information about NTTECO, visit https://ntteco.com.
 */
using AdventureWorks.WCFServices;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.ServiceModel;

namespace AdventureWorks.WCFProductService
{
    [ServiceContract(Namespace = "http://schemas.adventureworks.local/WCFProductService")]
    public interface IProductService
    {
        [OperationContract]
        [FaultContract(typeof(NotFoundFault))]
        ProductDto GetProduct(GetProductRequest request);

        [OperationContract]
        [FaultContract(typeof(ValidationFault))]
        PagedProductsDto SearchProducts(SearchProductsRequest request);

        [OperationContract]
        [FaultContract(typeof(NotFoundFault))]
        ProductInventoryDto GetProductWithInventory(GetProductRequest request);
    }

    [DataContract]
    public class GetProductRequest
    {
        [DataMember] public int ProductId { get; set; }
    }

    [DataContract]
    public class SearchProductsRequest
    {
        [DataMember] public string Query { get; set; }
        [DataMember] public int Page { get; set; }
        [DataMember] public int Size { get; set; }
    }

    [DataContract]
    public class ProductDto
    {
        [DataMember] public int ProductID { get; set; }
        [DataMember] public string Name { get; set; }
        [DataMember] public string ProductNumber { get; set; }
        [DataMember] public bool MakeFlag { get; set; }
        [DataMember] public bool FinishedGoodsFlag { get; set; }
        [DataMember] public string Color { get; set; }
        [DataMember] public short SafetyStockLevel { get; set; }
        [DataMember] public short ReorderPoint { get; set; }
        [DataMember] public decimal StandardCost { get; set; }
        [DataMember] public decimal ListPrice { get; set; }
        [DataMember] public string Size { get; set; }
        [DataMember] public string SizeUnitMeasureCode { get; set; }
        [DataMember] public string WeightUnitMeasureCode { get; set; }
        [DataMember] public decimal? Weight { get; set; }
        [DataMember] public int DaysToManufacture { get; set; }
        [DataMember] public string ProductLine { get; set; }
        [DataMember] public string Class { get; set; }
        [DataMember] public string Style { get; set; }
        [DataMember] public int? ProductSubcategoryID { get; set; }
        [DataMember] public int? ProductModelID { get; set; }
        [DataMember] public DateTime SellStartDate { get; set; }
        [DataMember] public DateTime? SellEndDate { get; set; }
        [DataMember] public DateTime? DiscontinuedDate { get; set; }
        [DataMember] public Guid RowGuid { get; set; }
        [DataMember] public DateTime ModifiedDate { get; set; }
    }

    [DataContract]
    public class ProductInventoryDto
    {
        [DataMember] public ProductDto Product { get; set; }
        [DataMember] public int InventoryLevel { get; set; }
        [DataMember] public string WarehouseInfo { get; set; }
    }

    [DataContract]
    public class PagedProductsDto
    {
        [DataMember] public List<ProductDto> Items { get; set; }
        [DataMember] public int Page { get; set; }
        [DataMember] public int Size { get; set; }
        [DataMember] public int TotalCount { get; set; }
    }
}
