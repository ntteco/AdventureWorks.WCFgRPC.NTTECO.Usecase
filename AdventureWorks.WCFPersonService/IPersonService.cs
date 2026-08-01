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
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.ServiceModel;

namespace AdventureWorks.WCFServices
{
    // Service contract
    [ServiceContract(Namespace = "http://schemas.adventureworks.local/WCFPersonService")]
    public interface IPersonService
    {
        [OperationContract]
        [FaultContract(typeof(NotFoundFault))]
        PersonDto GetPerson(GetPersonRequest request);

        [OperationContract]
        [FaultContract(typeof(ValidationFault))]
        PagedPeopleDto ListPeople(ListPeopleRequest request);

        [OperationContract]
        [FaultContract(typeof(NotFoundFault))]
        PersonXmlDto GetPersonRawXml(GetPersonRequest request);
    }

    // Requests
    [DataContract]
    public class GetPersonRequest
    {
        [DataMember] public int PersonId { get; set; }
    }

    [DataContract]
    public class ListPeopleRequest
    {
        [DataMember] public int Page { get; set; }
        [DataMember] public int Size { get; set; }
        [DataMember] public string Filter { get; set; } // simple server-side filter (e.g., "lastName:Smith")
    }

    //// DTOs (full PersonDto already provided; included here for completeness)
    //[DataContract]
    //public class PersonDto
    //{
    //    [DataMember] public int BusinessEntityID { get; set; }
    //    [DataMember] public string PersonType { get; set; }
    //    [DataMember] public int NameStyle { get; set; }
    //    [DataMember] public string Title { get; set; }
    //    [DataMember] public string FirstName { get; set; }
    //    [DataMember] public string MiddleName { get; set; }
    //    [DataMember] public string LastName { get; set; }
    //    [DataMember] public string Suffix { get; set; }
    //    [DataMember] public int EmailPromotion { get; set; }
    //    [DataMember] public string AdditionalContactInfoRaw { get; set; } // xml
    //    [DataMember] public string DemographicsRaw { get; set; }          // xml
    //    [DataMember] public Guid RowGuid { get; set; }
    //    [DataMember] public DateTime ModifiedDate { get; set; }

    //    // Convenience/derived fields
    //    [DataMember] public string DemographicsSummary { get; set; }
    //    [DataMember] public string PrimaryContact { get; set; }
    //}

    [DataContract]
    public class PersonListItemDto
    {
        [DataMember] public int BusinessEntityID { get; set; }
        [DataMember] public string FirstName { get; set; }
        [DataMember] public string LastName { get; set; }
        [DataMember] public string PrimaryContact { get; set; }
        [DataMember] public string DemographicsSummary { get; set; }
    }

    [DataContract]
    public class PersonXmlDto
    {
        [DataMember] public int BusinessEntityID { get; set; }
        [DataMember] public string AdditionalContactInfoRaw { get; set; }
        [DataMember] public string DemographicsRaw { get; set; }
        [DataMember] public Guid RowGuid { get; set; }
        [DataMember] public DateTime ModifiedDate { get; set; }
    }

    // Paged result for people
    [DataContract]
    public class PagedPeopleDto
    {
        [DataMember] public List<PersonListItemDto> Items { get; set; }
        [DataMember] public int Page { get; set; }
        [DataMember] public int Size { get; set; }
        [DataMember] public int TotalCount { get; set; }
    }

    // Fault contracts
    [DataContract]
    public class ValidationFault
    {
        [DataMember] public List<ValidationError> Errors { get; set; }
    }

    [DataContract]
    public class ValidationError
    {
        [DataMember] public string Field { get; set; }
        [DataMember] public string Message { get; set; }
    }

    [DataContract]
    public class NotFoundFault
    {
        [DataMember] public string Resource { get; set; }
        [DataMember] public string Message { get; set; }
    }
}
