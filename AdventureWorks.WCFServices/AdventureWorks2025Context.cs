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
using AdventureWorks.WCFEntities;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;

namespace AdventureWorks.WCFServices
{
    public class AdventureWorks2025Context : DbContext
    {
        // Use the connection string name defined in Web.config
        // Replace "AdventureWorks2025" with the exact name you add to Web.config
        public AdventureWorks2025Context()
           : base("name=AdventureWorks2025")
        {
            Database.SetInitializer<AdventureWorks2025Context>(null);
        }

        public DbSet<Person> People { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<WorkOrder> WorkOrders { get; set; }
    }
}
