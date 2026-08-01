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
using Microsoft.EntityFrameworkCore;
using AdventureWorks.WCFEntities;

namespace AdventureWorks.GrpcService.Data
{
    public class AdventureWorks2025Context : DbContext
    {
        public AdventureWorks2025Context(DbContextOptions<AdventureWorks2025Context> options)
            : base(options) { }

        public DbSet<Person> People { get; set; } = null!;
        public DbSet<Product> Products { get; set; } = null!;
        public DbSet<WorkOrder> WorkOrders { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // If your POCOs have [Table] attributes, EF Core will honor them.
            // Map computed column for WorkOrder.StockedQty if DB defines it as computed.
            modelBuilder.Entity<WorkOrder>(b =>
            {
                b.Property(w => w.StockedQty)
                 .ValueGeneratedOnAddOrUpdate()
                 .HasComputedColumnSql("([OrderQty] - [ScrappedQty])", stored: true);
            });

            base.OnModelCreating(modelBuilder);
        }
    }
}
