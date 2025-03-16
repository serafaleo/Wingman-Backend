using Microsoft.EntityFrameworkCore;
using Wingman.Api.Features.Aircrafts.Models;
using Wingman.Api.Features.Auth.Models;
using Wingman.Api.Features.Flights.Models;

namespace Wingman.Api.Core.Migration;

public class MigrationContext : DbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<Aircraft> Aircrafts { get; set; }
    public DbSet<Flight> Flights { get; set; }

    public MigrationContext(DbContextOptions<MigrationContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(MigrationContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
