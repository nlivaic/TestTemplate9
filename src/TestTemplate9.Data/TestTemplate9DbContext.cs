using MassTransit;
using Microsoft.EntityFrameworkCore;
using TestTemplate9.Core.Entities;

namespace TestTemplate9.Data
{
    public class TestTemplate9DbContext : DbContext
    {
        public TestTemplate9DbContext(DbContextOptions options)
            : base(options)
        {
        }

        public DbSet<Foo> Foos { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.AddInboxStateEntity();
            modelBuilder.AddOutboxMessageEntity();
            modelBuilder.AddOutboxStateEntity();
        }
    }
}
