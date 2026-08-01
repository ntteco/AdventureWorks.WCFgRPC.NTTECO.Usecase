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
using System.Runtime.Serialization;


namespace AdventureWorks.WCFServices
{
    [DataContract]
    public class PersonDto
    {
        [DataMember] public int BusinessEntityID { get; set; }
        [DataMember] public string PersonType { get; set; }           // nchar(2)
        [DataMember] public int NameStyle { get; set; }              // dbo.NameStyle (map to int/enum)
        [DataMember] public string Title { get; set; }               // nullable
        [DataMember] public string FirstName { get; set; }
        [DataMember] public string MiddleName { get; set; }          // nullable
        [DataMember] public string LastName { get; set; }
        [DataMember] public string Suffix { get; set; }              // nullable
        [DataMember] public int EmailPromotion { get; set; }
        [DataMember] public string AdditionalContactInfoRaw { get; set; } // xml as string
        [DataMember] public string DemographicsRaw { get; set; }          // xml as string
        [DataMember] public Guid RowGuid { get; set; }
        [DataMember] public DateTime ModifiedDate { get; set; }

        // Convenience/derived fields (service layer populates)
        [DataMember] public string DemographicsSummary { get; set; } // parsed summary
        [DataMember] public string PrimaryContact { get; set; }      // parsed preferred contact
    }
}
