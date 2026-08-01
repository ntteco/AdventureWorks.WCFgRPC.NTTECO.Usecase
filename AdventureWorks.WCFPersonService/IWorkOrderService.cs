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

namespace AdventureWorks.WCFWorkOrderService
{
    [ServiceContract(Namespace = "http://schemas.adventureworks.local/WCFWorkOrderService")]
    public interface IWorkOrderService
    {
        [OperationContract]
        [FaultContract(typeof(NotFoundFault))]
        WorkOrderDto GetWorkOrder(GetWorkOrderRequest request);

        [OperationContract]
        [FaultContract(typeof(ValidationFault))]
        PagedWorkOrdersDto ListWorkOrdersByProduct(ListWorkOrdersRequest request);

        [OperationContract]
        [FaultContract(typeof(ValidationFault))]
        [FaultContract(typeof(NotFoundFault))]
        WorkOrderDto CreateWorkOrder(WorkOrderCreateDto request);
    }

    [DataContract]
    public class GetWorkOrderRequest
    {
        [DataMember] public int WorkOrderId { get; set; }
    }

    [DataContract]
    public class ListWorkOrdersRequest
    {
        [DataMember] public int ProductId { get; set; }
        [DataMember] public int Page { get; set; }
        [DataMember] public int Size { get; set; }
    }

    [DataContract]
    public class WorkOrderCreateDto
    {
        [DataMember] public int ProductID { get; set; }
        [DataMember] public int OrderQty { get; set; }
        [DataMember] public DateTime StartDate { get; set; }
        [DataMember] public DateTime DueDate { get; set; }
        [DataMember] public short? ScrapReasonID { get; set; }
    }

    [DataContract]
    public class WorkOrderDto
    {
        [DataMember] public int WorkOrderID { get; set; }
        [DataMember] public int ProductID { get; set; }
        [DataMember] public int OrderQty { get; set; }
        [DataMember] public int StockedQty { get; set; } // computed: OrderQty - ScrappedQty
        [DataMember] public short ScrappedQty { get; set; }
        [DataMember] public DateTime StartDate { get; set; }
        [DataMember] public DateTime? EndDate { get; set; }
        [DataMember] public DateTime DueDate { get; set; }
        [DataMember] public short? ScrapReasonID { get; set; }
        [DataMember] public DateTime ModifiedDate { get; set; }
    }

    [DataContract]
    public class PagedWorkOrdersDto
    {
        [DataMember] public List<WorkOrderDto> Items { get; set; }
        [DataMember] public int Page { get; set; }
        [DataMember] public int Size { get; set; }
        [DataMember] public int TotalCount { get; set; }
    }
}
