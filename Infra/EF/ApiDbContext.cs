using Microsoft.EntityFrameworkCore;
using Domain.Entities;
using System.Reflection;
using NetDevPack.Security.Jwt.Store.EntityFrameworkCore;
using NetDevPack.Security.Jwt.Core.Model;

namespace Infra.EF;

public class ApiDbContext : DbContext, ISecurityKeyContext
{
    public ApiDbContext(DbContextOptions<ApiDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        modelBuilder.ApplyConfiguration(new KeyMaterialMap());
        base.OnModelCreating(modelBuilder);
    }

    public DbSet<KeyMaterial> SecurityKeys { get; set; }

    public DbSet<User> User { get; set; }
}
