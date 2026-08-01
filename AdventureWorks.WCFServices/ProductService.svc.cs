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
using AdventureWorks.WCFEntities;
using AdventureWorks.WCFProductService; // DTOs, requests, faults
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;

namespace AdventureWorks.WCFProductService
{
    public class ProductService : IProductService
    {
        public ProductDto GetProduct(GetProductRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            using (var ctx = new AdventureWorks2025Context())
            {
                var entity = ctx.Products
                                .SingleOrDefault(p => p.ProductID == request.ProductId);

                if (entity == null)
                {
                    var nf = new NotFoundFault
                    {
                        Resource = $"Product/{request.ProductId}",
                        Message = "Product not found."
                    };
                    throw new FaultException<NotFoundFault>(nf, new FaultReason(nf.Message));
                }

                return MapToProductDto(entity);
            }
        }

        public ProductInventoryDto GetProductWithInventory(GetProductRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            using (var ctx = new AdventureWorks2025Context())
            {
                var entity = ctx.Products
                                .SingleOrDefault(p => p.ProductID == request.ProductId);

                if (entity == null)
                {
                    var nf = new NotFoundFault
                    {
                        Resource = $"Product/{request.ProductId}",
                        Message = "Product not found."
                    };
                    throw new FaultException<NotFoundFault>(nf, new FaultReason(nf.Message));
                }

                // Simple inventory calculation for demo: sum StockedQty from WorkOrders for this product.
                // If WorkOrders are not present or StockedQty is null, fallback to 0.
                int inventoryLevel = 0;
                try
                {
                    inventoryLevel = ctx.WorkOrders
                                       .Where(w => w.ProductID == request.ProductId)
                                       .Select(w => (int?)w.StockedQty)
                                       .ToList()
                                       .Where(v => v.HasValue)
                                       .Sum(v => v.Value);
                }
                catch
                {
                    // If anything goes wrong (e.g., computed column not available), fall back to 0.
                    inventoryLevel = 0;
                }

                var dto = new ProductInventoryDto
                {
                    Product = MapToProductDto(entity),
                    InventoryLevel = inventoryLevel,
                    WarehouseInfo = $"DefaultWarehouse" // placeholder for demo
                };

                return dto;
            }
        }

        public PagedProductsDto SearchProducts(SearchProductsRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            // Basic validation
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
                IQueryable<Product> q = ctx.Products;

                if (!string.IsNullOrWhiteSpace(request.Query))
                {
                    var qTrim = request.Query.Trim();
                    q = q.Where(p => p.Name.Contains(qTrim) || p.ProductNumber.Contains(qTrim));
                }

                var total = q.Count();

                var skip = (request.Page - 1) * request.Size;
                var items = q.OrderBy(p => p.ProductID)
                             .Skip(skip)
                             .Take(request.Size)
                             .ToList()
                             .Select(MapToProductDto)
                             .ToList();

                return new PagedProductsDto
                {
                    Items = items,
                    Page = request.Page,
                    Size = request.Size,
                    TotalCount = total
                };
            }
        }

        #region Mapping helpers

        private static ProductDto MapToProductDto(Product e)
        {
            if (e == null) return null;

            return new ProductDto
            {
                ProductID = e.ProductID,
                Name = e.Name,
                ProductNumber = e.ProductNumber,
                MakeFlag = e.MakeFlag,
                FinishedGoodsFlag = e.FinishedGoodsFlag,
                Color = e.Color,
                SafetyStockLevel = e.SafetyStockLevel,
                ReorderPoint = e.ReorderPoint,
                StandardCost = e.StandardCost,
                ListPrice = e.ListPrice,
                Size = e.Size,
                SizeUnitMeasureCode = e.SizeUnitMeasureCode,
                WeightUnitMeasureCode = e.WeightUnitMeasureCode,
                Weight = e.Weight,
                DaysToManufacture = e.DaysToManufacture,
                ProductLine = e.ProductLine,
                Class = e.Class,
                Style = e.Style,
                ProductSubcategoryID = e.ProductSubcategoryID,
                ProductModelID = e.ProductModelID,
                SellStartDate = e.SellStartDate,
                SellEndDate = e.SellEndDate,
                DiscontinuedDate = e.DiscontinuedDate,
                RowGuid = e.RowGuid,
                ModifiedDate = e.ModifiedDate
            };
        }

        #endregion
    }
}
