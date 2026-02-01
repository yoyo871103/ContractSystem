using System.Linq.Expressions;
using InventorySystem.Domain;
using InventorySystem.Domain.Identity;
using Microsoft.EntityFrameworkCore;

namespace InventorySystem.Infrastructure.Database;

/// <summary>
/// Contexto de Entity Framework Core para la aplicación.
/// Soporta SQL Server y SQLite según la configuración.
/// Aplica filtro global de soft delete a todas las entidades que implementan <see cref="ISoftDeletable"/>:
/// por defecto las consultas no devuelven registros con <see cref="ISoftDeletable.DeletedAt"/> distinto de null.
/// Use <see cref="IgnoreQueryFilters"/> en la consulta cuando necesite incluir también los registros borrados.
/// </summary>
public sealed class ApplicationDbContext : DbContext, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Rol> Roles => Set<Rol>();
    public DbSet<UsuarioRol> UsuarioRoles => Set<UsuarioRol>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        ApplySoftDeleteQueryFilters(modelBuilder);
    }

    /// <summary>
    /// Aplica el filtro de soft delete a todas las entidades que implementan <see cref="ISoftDeletable"/>.
    /// Las consultas sobre esos DbSet excluyen por defecto los registros con <see cref="ISoftDeletable.DeletedAt"/> != null.
    /// Para incluir también los borrados, use <c>.IgnoreQueryFilters()</c> en la consulta.
    /// </summary>
    private static void ApplySoftDeleteQueryFilters(ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(ISoftDeletable).IsAssignableFrom(entityType.ClrType))
            {
                var parameter = Expression.Parameter(entityType.ClrType, "e");
                var property = Expression.Property(parameter, nameof(ISoftDeletable.DeletedAt));
                var nullConstant = Expression.Constant(null, typeof(DateTimeOffset?));
                var condition = Expression.Equal(property, nullConstant);
                var lambda = Expression.Lambda(condition, parameter);
                modelBuilder.Entity(entityType.ClrType).HasQueryFilter(lambda);
            }
        }
    }
}
