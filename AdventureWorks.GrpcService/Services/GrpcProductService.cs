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
using ProductProto = AdventureWorks.GrpcService.Protos.Product;

namespace AdventureWorks.GrpcService.Services
{
    public class GrpcProductService : ProductProto.ProductService.ProductServiceBase
    {
        private readonly IDbContextFactory<AdventureWorks2025Context> _dbFactory;
        private readonly ILogger<GrpcProductService> _logger;

        public GrpcProductService(IDbContextFactory<AdventureWorks2025Context> dbFactory, ILogger<GrpcProductService> logger)
        {
            _dbFactory = dbFactory;
            _logger = logger;
        }

        public override async Task<ProductProto.ProductDto> GetProduct(ProductProto.GetProductRequest request, ServerCallContext context)
        {
            if (request == null) throw new RpcException(new Status(StatusCode.InvalidArgument, "Request is null"));

            await using var db = _dbFactory.CreateDbContext();
            var entity = await db.Products.AsNoTracking()
                                          .SingleOrDefaultAsync(p => p.ProductID == request.ProductId);

            if (entity == null) throw new RpcException(new Status(StatusCode.NotFound, $"Product/{request.ProductId} not found"));

            return MapToProto(entity);
        }

        public override async Task<ProductProto.PagedProducts> SearchProducts(ProductProto.SearchProductsRequest request, ServerCallContext context)
        {
            if (request == null) throw new RpcException(new Status(StatusCode.InvalidArgument, "Request is null"));

            var page = request.Page <= 0 ? 1 : request.Page;
            var size = request.Size <= 0 ? 25 : request.Size;

            await using var db = _dbFactory.CreateDbContext();
            IQueryable<Product> q = db.Products.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(request.Query))
            {
                var qstr = request.Query.Trim();
                q = q.Where(p =>
                    (p.Name != null && EF.Functions.Like(p.Name, $"%{qstr}%")) ||
                    (p.ProductNumber != null && EF.Functions.Like(p.ProductNumber, $"%{qstr}%")));
            }

            var total = await q.CountAsync();
            var skip = (page - 1) * size;

            var items = await q.OrderBy(p => p.ProductID)
                               .Skip(skip)
                               .Take(size)
                               .ToListAsync();

            var result = new ProductProto.PagedProducts
            {
                Page = page,
                Size = size,
                TotalCount = (int)total
            };

            result.Items.AddRange(items.Select(MapToProto));
            return result;
        }

        public override async Task<ProductProto.ProductInventoryDto> GetProductWithInventory(ProductProto.GetProductRequest request, ServerCallContext context)
        {
            if (request == null) throw new RpcException(new Status(StatusCode.InvalidArgument, "Request is null"));

            await using var db = _dbFactory.CreateDbContext();
            var entity = await db.Products.AsNoTracking()
                                          .SingleOrDefaultAsync(p => p.ProductID == request.ProductId);

            if (entity == null) throw new RpcException(new Status(StatusCode.NotFound, $"Product/{request.ProductId} not found"));

            // If you have a mapped Inventory DbSet, prefer LINQ SumAsync:
            // var inventoryLevel = await db.Inventories.Where(i => i.ProductID == entity.ProductID).SumAsync(i => (int?)i.Quantity) ?? 0;
            // Otherwise use a typed keyless result (InventoryLevel) as shown below.

            var inv = await db.Set<InventoryLevel>()
                              .FromSqlRaw("SELECT SUM(Quantity) AS Qty FROM Inventory WHERE ProductID = {0}", entity.ProductID)
                              .AsNoTracking()
                              .FirstOrDefaultAsync();

            var inventoryLevel = inv?.Qty ?? 0;
            var warehouseInfo = await GetWarehouseInfoForProduct(db, entity.ProductID);

            return new ProductProto.ProductInventoryDto
            {
                Product = MapToProto(entity),
                InventoryLevel = inventoryLevel,
                WarehouseInfo = warehouseInfo
            };
        }

        #region Helpers and mapping

        // Keyless holder for scalar SQL result
        private class InventoryLevel
        {
            public int? Qty { get; set; }
        }

        private static Task<string> GetWarehouseInfoForProduct(AdventureWorks2025Context db, int productId)
        {
            return Task.FromResult("DefaultWarehouse");
        }

        private static ProductProto.ProductDto MapToProto(Product e)
        {
            // Adjust casts below if your POCO properties are nullable (decimal? or double?)
            return new ProductProto.ProductDto
            {
                ProductId = e.ProductID,
                Name = e.Name ?? string.Empty,
                ProductNumber = e.ProductNumber ?? string.Empty,
                MakeFlag = e.MakeFlag,
                FinishedGoodsFlag = e.FinishedGoodsFlag,
                Color = e.Color ?? string.Empty,
                SafetyStockLevel = e.SafetyStockLevel,
                ReorderPoint = e.ReorderPoint,

                // If StandardCost is non-nullable decimal:
                StandardCost = (double)e.StandardCost,

                // If ListPrice is non-nullable decimal:
                ListPrice = (double)e.ListPrice,

                Size = e.Size ?? string.Empty,
                SizeUnitMeasureCode = e.SizeUnitMeasureCode ?? string.Empty,
                WeightUnitMeasureCode = e.WeightUnitMeasureCode ?? string.Empty,

                // If Weight is non-nullable decimal:
                Weight = e.Weight==null? 0 :(double)e.Weight,

                DaysToManufacture = e.DaysToManufacture,
                ProductLine = e.ProductLine ?? string.Empty,
                ClassCode = e.Class ?? string.Empty,
                Style = e.Style ?? string.Empty,
                ProductSubcategoryId = e.ProductSubcategoryID ?? 0,
                ProductModelId = e.ProductModelID ?? 0,
                SellStartDate = ToTimestampSafe(e.SellStartDate),
                SellEndDate = ToTimestampSafe(e.SellEndDate),
                DiscontinuedDate = ToTimestampSafe(e.DiscontinuedDate),
                Rowguid = e.RowGuid.ToString(),
                ModifiedDate = ToTimestampNonNull(e.ModifiedDate)
            };
        }

        // Timestamp helpers
        private static Timestamp? ToTimestampSafe(DateTime? dt)
        {
            if (!dt.HasValue) return null;
            var utc = DateTime.SpecifyKind(dt.Value, DateTimeKind.Utc);
            return Timestamp.FromDateTime(utc);
        }

        private static Timestamp ToTimestampNonNull(DateTime dt)
        {
            var utc = DateTime.SpecifyKind(dt, DateTimeKind.Utc);
            return Timestamp.FromDateTime(utc);
        }

        #endregion
    }
}
