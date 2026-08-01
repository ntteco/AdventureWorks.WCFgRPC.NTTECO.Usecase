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
    [Table("Person", Schema = "Person")]
    public class Person
    {
        [Key]
        [Column("BusinessEntityID")]
        public int BusinessEntityID { get; set; }

        [Column("PersonType", TypeName = "nchar")]
        [MaxLength(2)]
        public string PersonType { get; set; }

        [Column("NameStyle")]
        public int NameStyle { get; set; }

        [Column("Title")]
        [MaxLength(8)]
        public string Title { get; set; }

        [Column("FirstName")]
        [MaxLength(50)]
        public string FirstName { get; set; }

        [Column("MiddleName")]
        [MaxLength(50)]
        public string MiddleName { get; set; }

        [Column("LastName")]
        [MaxLength(50)]
        public string LastName { get; set; }

        [Column("Suffix")]
        [MaxLength(10)]
        public string Suffix { get; set; }

        [Column("EmailPromotion")]
        public int EmailPromotion { get; set; }

        // XML columns stored as string
        [Column("AdditionalContactInfo", TypeName = "xml")]
        public string AdditionalContactInfo { get; set; }

        [Column("Demographics", TypeName = "xml")]
        public string Demographics { get; set; }

        [Column("rowguid")]
        public Guid RowGuid { get; set; }

        [Column("ModifiedDate")]
        public DateTime ModifiedDate { get; set; }
    }
}
