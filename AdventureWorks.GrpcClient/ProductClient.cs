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
using System.Threading.Tasks;
using Grpc.Core;
using AdventureWorks.GrpcClient.Protos.Product;

namespace AdventureWorks.GrpcClient
{
    public class ProductClient
    {
        private readonly Channel _channel;
        private readonly ProductService.ProductServiceClient _client;

        public ProductClient(string address)
        {
            _channel = new Channel(address, ChannelCredentials.Insecure);
            _client = new ProductService.ProductServiceClient(_channel);
        }

        public async Task<ProductDto> GetProductAsync(int productId)
        {
            var req = new GetProductRequest { ProductId = productId };
            return await _client.GetProductAsync(req);
        }

        public async Task<PagedProducts> SearchProductsAsync(string query, int page, int size)
        {
            var req = new SearchProductsRequest
            {
                Query = query ?? "",
                Page = page,
                Size = size
            };

            return await _client.SearchProductsAsync(req);
        }

        public async Task<ProductInventoryDto> GetProductWithInventoryAsync(int productId)
        {
            var req = new GetProductRequest { ProductId = productId };
            return await _client.GetProductWithInventoryAsync(req);
        }

        public async Task ShutdownAsync() => await _channel.ShutdownAsync();
    }
}
