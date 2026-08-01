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
using AdventureWorks.WCFServices;       // shared Faults (ValidationFault, NotFoundFault)
using AdventureWorks.WCFEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;

namespace AdventureWorks.WCFWorkOrderService
{
    public class WorkOrderService : IWorkOrderService
    {
        // Create is intentionally not implemented for this demo
        public WorkOrderDto CreateWorkOrder(WorkOrderCreateDto request)
        {
            throw new NotImplementedException();
        }

        public WorkOrderDto GetWorkOrder(GetWorkOrderRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            using (var ctx = new AdventureWorks2025Context())
            {
                var entity = ctx.WorkOrders
                                .SingleOrDefault(w => w.WorkOrderID == request.WorkOrderId);

                if (entity == null)
                {
                    var nf = new NotFoundFault
                    {
                        Resource = $"WorkOrder/{request.WorkOrderId}",
                        Message = "WorkOrder not found."
                    };
                    throw new FaultException<NotFoundFault>(nf, new FaultReason(nf.Message));
                }

                return MapToWorkOrderDto(entity);
            }
        }

        public PagedWorkOrdersDto ListWorkOrdersByProduct(ListWorkOrdersRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            // Basic validation for paging
            if (request.Page <= 0 || request.Size <= 0)
            {
                var vf = new ValidationFault
                {
                    Errors = new List<ValidationError>
                    {
                        new ValidationError { Field = "Page/Size", Message = "Page and Size must be positive integers." }
                    }
                };
                throw new FaultException<ValidationFault>(vf, new FaultReason("Invalid paging parameters."));
            }

            using (var ctx = new AdventureWorks2025Context())
            {
                IQueryable<WorkOrder> q = ctx.WorkOrders.Where(w => w.ProductID == request.ProductId);

                var total = q.Count();

                var skip = (request.Page - 1) * request.Size;
                var items = q.OrderBy(w => w.WorkOrderID)
                             .Skip(skip)
                             .Take(request.Size)
                             .ToList()
                             .Select(MapToWorkOrderDto)
                             .ToList();

                return new PagedWorkOrdersDto
                {
                    Items = items,
                    Page = request.Page,
                    Size = request.Size,
                    TotalCount = total
                };
            }
        }

        #region Mapping helper

        private static WorkOrderDto MapToWorkOrderDto(WorkOrder e)
        {
            if (e == null) return null;

            return new WorkOrderDto
            {
                WorkOrderID = e.WorkOrderID,
                ProductID = e.ProductID,
                OrderQty = e.OrderQty,
                StockedQty = e.StockedQty,      // computed column, read-only
                ScrappedQty = e.ScrappedQty,
                StartDate = e.StartDate,
                EndDate = e.EndDate,
                DueDate = e.DueDate,
                ScrapReasonID = e.ScrapReasonID,
                ModifiedDate = e.ModifiedDate
            };
        }

        #endregion
    }
}
