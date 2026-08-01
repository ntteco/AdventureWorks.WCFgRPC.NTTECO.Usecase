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
using System.Threading.Tasks;
using Grpc.Core;
using Google.Protobuf.WellKnownTypes;
using AdventureWorks.GrpcClient.Protos.WorkOrder;

namespace AdventureWorks.GrpcClient
{
    public class WorkOrderClient
    {
        private readonly Channel _channel;
        private readonly WorkOrderService.WorkOrderServiceClient _client;

        public WorkOrderClient(string address)
        {
            _channel = new Channel(address, ChannelCredentials.Insecure);
            _client = new WorkOrderService.WorkOrderServiceClient(_channel);
        }

        public async Task<WorkOrderDto> GetWorkOrderAsync(int id)
        {
            return await _client.GetWorkOrderAsync(new GetWorkOrderRequest { WorkOrderId = id });
        }

        public async Task<PagedWorkOrders> ListWorkOrdersByProductAsync(int productId, int page, int size)
        {
            return await _client.ListWorkOrdersByProductAsync(new ListWorkOrdersRequest
            {
                ProductId = productId,
                Page = page,
                Size = size
            });
        }

        public async Task<WorkOrderDto> CreateWorkOrderAsync(
            int productId,
            int orderQty,
            DateTime? startDate,
            DateTime? dueDate,
            int scrapReasonId)
        {
            var req = new WorkOrderCreateDto
            {
                ProductId = productId,
                OrderQty = orderQty,
                ScrapReasonId = scrapReasonId
            };

            if (startDate.HasValue)
                req.StartDate = Timestamp.FromDateTime(DateTime.SpecifyKind(startDate.Value, DateTimeKind.Utc));

            if (dueDate.HasValue)
                req.DueDate = Timestamp.FromDateTime(DateTime.SpecifyKind(dueDate.Value, DateTimeKind.Utc));

            return await _client.CreateWorkOrderAsync(req);
        }

        public async Task ShutdownAsync() => await _channel.ShutdownAsync();
    }
}
