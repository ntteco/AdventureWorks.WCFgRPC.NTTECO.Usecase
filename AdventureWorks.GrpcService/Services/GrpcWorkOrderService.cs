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
using System.Linq;
using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Google.Protobuf.WellKnownTypes;
using AdventureWorks.GrpcService.Data;
using AdventureWorks.WCFEntities;
using WorkOrderProto = AdventureWorks.GrpcService.Protos.WorkOrder;

namespace AdventureWorks.GrpcService.Services
{
    public class GrpcWorkOrderService : WorkOrderProto.WorkOrderService.WorkOrderServiceBase
    {
        private readonly IDbContextFactory<AdventureWorks2025Context> _dbFactory;
        private readonly ILogger<GrpcWorkOrderService> _logger;

        public GrpcWorkOrderService(IDbContextFactory<AdventureWorks2025Context> dbFactory, ILogger<GrpcWorkOrderService> logger)
        {
            _dbFactory = dbFactory;
            _logger = logger;
        }

        public override async Task<WorkOrderProto.WorkOrderDto> GetWorkOrder(WorkOrderProto.GetWorkOrderRequest request, ServerCallContext context)
        {
            if (request == null) throw new RpcException(new Status(StatusCode.InvalidArgument, "Request is null"));

            await using var db = _dbFactory.CreateDbContext();
            var entity = await db.WorkOrders.AsNoTracking()
                                           .SingleOrDefaultAsync(w => w.WorkOrderID == request.WorkOrderId);

            if (entity == null) throw new RpcException(new Status(StatusCode.NotFound, $"WorkOrder/{request.WorkOrderId} not found"));

            return MapToProto(entity);
        }

        public override async Task<WorkOrderProto.PagedWorkOrders> ListWorkOrdersByProduct(WorkOrderProto.ListWorkOrdersRequest request, ServerCallContext context)
        {
            if (request == null) throw new RpcException(new Status(StatusCode.InvalidArgument, "Request is null"));

            var page = request.Page <= 0 ? 1 : request.Page;
            var size = request.Size <= 0 ? 25 : request.Size;

            await using var db = _dbFactory.CreateDbContext();
            IQueryable<WorkOrder> q = db.WorkOrders.AsNoTracking();

            if (request.ProductId > 0)
            {
                q = q.Where(w => w.ProductID == request.ProductId);
            }

            var total = await q.CountAsync();
            var skip = (page - 1) * size;

            // Materialize before mapping
            var items = await q.OrderBy(w => w.WorkOrderID)
                               .Skip(skip)
                               .Take(size)
                               .ToListAsync();

            var result = new WorkOrderProto.PagedWorkOrders
            {
                Page = page,
                Size = size,
                TotalCount = (int)total
            };

            // PagedWorkOrders.items is WorkOrderDto in your proto
            result.Items.AddRange(items.Select(MapToProto));
            return result;
        }

        public override async Task<WorkOrderProto.WorkOrderDto> CreateWorkOrder(WorkOrderProto.WorkOrderCreateDto request, ServerCallContext context)
        {
            if (request == null) throw new RpcException(new Status(StatusCode.InvalidArgument, "Request is null"));
            if (request.ProductId <= 0) throw new RpcException(new Status(StatusCode.InvalidArgument, "ProductId must be positive"));
            if (request.OrderQty <= 0) throw new RpcException(new Status(StatusCode.InvalidArgument, "OrderQty must be positive"));

            await using var db = _dbFactory.CreateDbContext();

            // Ensure product exists
            var productExists = await db.Products.AsNoTracking().AnyAsync(p => p.ProductID == request.ProductId);
            if (!productExists) throw new RpcException(new Status(StatusCode.NotFound, $"Product/{request.ProductId} not found"));

            var now = DateTime.UtcNow;

            // Convert incoming timestamps to DateTime? safely
            var startDate = FromTimestampSafe(request.StartDate);
            var dueDate = FromTimestampSafe(request.DueDate);

            // If your WorkOrder POCO has non-nullable DateTime fields, choose a sensible fallback.
            // Here we use DateTime.UtcNow as fallback for required DB columns; adjust if you prefer default(DateTime) or to require the client to send timestamps.
            var entity = new WorkOrder
            {
                ProductID = request.ProductId,
                OrderQty = request.OrderQty,
                // WorkOrderCreateDto does not include ScrappedQty or EndDate in your proto, so we don't set them here.
                StartDate = startDate ?? DateTime.UtcNow,
                DueDate = dueDate ?? DateTime.UtcNow,
                ScrapReasonID = request.ScrapReasonId == 0 ? null : (short?)request.ScrapReasonId,
                ModifiedDate = now
            };

            db.WorkOrders.Add(entity);

            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Error creating WorkOrder for ProductId {ProductId}", request.ProductId);
                throw new RpcException(new Status(StatusCode.Internal, "Failed to create work order"));
            }

            // Reload to get DB-populated/computed columns (StockedQty, WorkOrderID, etc.)
            var created = await db.WorkOrders.AsNoTracking().SingleOrDefaultAsync(w => w.WorkOrderID == entity.WorkOrderID);
            return MapToProto(created ?? entity);
        }

        #region Mapping helpers

        private static WorkOrderProto.WorkOrderDto MapToProto(WorkOrder e)
        {
            return new WorkOrderProto.WorkOrderDto
            {
                WorkOrderId = e.WorkOrderID,
                ProductId = e.ProductID,
                OrderQty = e.OrderQty,
                StockedQty = e.StockedQty,
                ScrappedQty = e.ScrappedQty,
                StartDate = ToTimestampSafe(e.StartDate),
                EndDate = ToTimestampSafe(e.EndDate),
                DueDate = ToTimestampSafe(e.DueDate),
                // ScrapReasonID in POCO is often short? so cast safely to int for proto
                ScrapReasonId = e.ScrapReasonID.HasValue ? (int)e.ScrapReasonID.Value : 0,
                ModifiedDate = ToTimestampNonNull(e.ModifiedDate)
            };
        }

        // Convert nullable DateTime -> Timestamp?
        private static Timestamp? ToTimestampSafe(DateTime? dt)
        {
            if (!dt.HasValue) return null;
            var utc = DateTime.SpecifyKind(dt.Value, DateTimeKind.Utc);
            return Timestamp.FromDateTime(utc);
        }

        // Convert non-nullable DateTime -> Timestamp
        private static Timestamp ToTimestampNonNull(DateTime dt)
        {
            var utc = DateTime.SpecifyKind(dt, DateTimeKind.Utc);
            return Timestamp.FromDateTime(utc);
        }

        // Convert incoming Timestamp -> DateTime?
        private static DateTime? FromTimestampSafe(Timestamp? ts)
        {
            if (ts == null) return null;
            return ts.ToDateTime();
        }

        #endregion
    }
}
