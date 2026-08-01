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
    [Table("WorkOrder", Schema = "Production")]
    public class WorkOrder
    {
        [Key]
        [Column("WorkOrderID")]
        public int WorkOrderID { get; set; }

        [Column("ProductID")]
        public int ProductID { get; set; }

        [Column("OrderQty")]
        public int OrderQty { get; set; }

        // Computed column: map as DatabaseGeneratedOption.Computed
        [Column("StockedQty")]
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public int StockedQty { get; private set; }

        [Column("ScrappedQty")]
        public short ScrappedQty { get; set; }

        [Column("StartDate")]
        public DateTime StartDate { get; set; }

        [Column("EndDate")]
        public DateTime? EndDate { get; set; }

        [Column("DueDate")]
        public DateTime DueDate { get; set; }

        [Column("ScrapReasonID")]
        public short? ScrapReasonID { get; set; }

        [Column("ModifiedDate")]
        public DateTime ModifiedDate { get; set; }
    }
}
