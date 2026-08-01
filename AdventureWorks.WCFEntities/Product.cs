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
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AdventureWorks.WCFEntities
{
    [Table("Product", Schema = "Production")]
    public class Product
    {
        [Key]
        [Column("ProductID")]
        public int ProductID { get; set; }

        [Column("Name")]
        [MaxLength(100)]
        public string Name { get; set; }

        [Column("ProductNumber")]
        [MaxLength(25)]
        public string ProductNumber { get; set; }

        [Column("MakeFlag")]
        public bool MakeFlag { get; set; }

        [Column("FinishedGoodsFlag")]
        public bool FinishedGoodsFlag { get; set; }

        [Column("Color")]
        [MaxLength(15)]
        public string Color { get; set; }

        [Column("SafetyStockLevel")]
        public short SafetyStockLevel { get; set; }

        [Column("ReorderPoint")]
        public short ReorderPoint { get; set; }

        [Column("StandardCost", TypeName = "money")]
        public decimal StandardCost { get; set; }

        [Column("ListPrice", TypeName = "money")]
        public decimal ListPrice { get; set; }

        [Column("Size")]
        [MaxLength(5)]
        public string Size { get; set; }

        [Column("SizeUnitMeasureCode")]
        [MaxLength(3)]
        public string SizeUnitMeasureCode { get; set; }

        [Column("WeightUnitMeasureCode")]
        [MaxLength(3)]
        public string WeightUnitMeasureCode { get; set; }

        [Column("Weight", TypeName = "decimal")]
        public decimal? Weight { get; set; }

        [Column("DaysToManufacture")]
        public int DaysToManufacture { get; set; }

        [Column("ProductLine")]
        [MaxLength(2)]
        public string ProductLine { get; set; }

        [Column("Class")]
        [MaxLength(2)]
        public string Class { get; set; }

        [Column("Style")]
        [MaxLength(2)]
        public string Style { get; set; }

        [Column("ProductSubcategoryID")]
        public int? ProductSubcategoryID { get; set; }

        [Column("ProductModelID")]
        public int? ProductModelID { get; set; }

        [Column("SellStartDate")]
        public DateTime SellStartDate { get; set; }

        [Column("SellEndDate")]
        public DateTime? SellEndDate { get; set; }

        [Column("DiscontinuedDate")]
        public DateTime? DiscontinuedDate { get; set; }

        [Column("rowguid")]
        public Guid RowGuid { get; set; }

        [Column("ModifiedDate")]
        public DateTime ModifiedDate { get; set; }
    }
}
