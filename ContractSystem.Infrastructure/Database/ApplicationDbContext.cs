using System.Linq.Expressions;
using ContractSystem.Application.Auth;
using ContractSystem.Domain;
using ContractSystem.Domain.Identity;
using ContractSystem.Domain.Business;
using ContractSystem.Domain.Nomencladores;
using ContractSystem.Domain.Contratos;
using Microsoft.EntityFrameworkCore;

namespace ContractSystem.Infrastructure.Database;

/// <summary>
/// Contexto de Entity Framework Core para la aplicación.
/// Soporta SQL Server y SQLite según la configuración.
/// Aplica filtro global de soft delete a todas las entidades que implementan <see cref="ISoftDeletable"/>:
/// por defecto las consultas no devuelven registros con <see cref="ISoftDeletable.DeletedAt"/> distinto de null.
/// Use <see cref="IgnoreQueryFilters"/> en la consulta cuando necesite incluir también los registros borrados.
///
/// Automáticamente rellena los campos de auditoría (IAuditable) en SaveChanges/SaveChangesAsync.
/// </summary>
public sealed class ApplicationDbContext : DbContext, IApplicationDbContext
{
    private readonly IAuthContext? _authContext;

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, IAuthContext authContext)
        : base(options)
    {
        _authContext = authContext;
    }

    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Rol> Roles => Set<Rol>();
    public DbSet<UsuarioRol> UsuarioRoles => Set<UsuarioRol>();
    public DbSet<Permiso> Permisos => Set<Permiso>();
    public DbSet<RolPermiso> RolPermisos => Set<RolPermiso>();
    public DbSet<UsuarioPermiso> UsuarioPermisos => Set<UsuarioPermiso>();
    public DbSet<BusinessInfo> BusinessInfos => Set<BusinessInfo>();
    public DbSet<UnidadMedida> UnidadesMedida => Set<UnidadMedida>();
    public DbSet<Tercero> Terceros => Set<Tercero>();
    public DbSet<ContactoTercero> ContactosTercero => Set<ContactoTercero>();
    public DbSet<ProductoServicio> ProductosServicios => Set<ProductoServicio>();
    public DbSet<PlantillaDocumento> PlantillasDocumento => Set<PlantillaDocumento>();
    public DbSet<Contrato> Contratos => Set<Contrato>();
    public DbSet<ModificacionDocumento> ModificacionesDocumento => Set<ModificacionDocumento>();
    public DbSet<ConfiguracionNumeracion> ConfiguracionesNumeracion => Set<ConfiguracionNumeracion>();
    public DbSet<ContadorNumeracion> ContadoresNumeracion => Set<ContadorNumeracion>();
    public DbSet<Anexo> Anexos => Set<Anexo>();
    public DbSet<LineaDetalle> LineasDetalle => Set<LineaDetalle>();
    public DbSet<DocumentoAdjunto> DocumentosAdjuntos => Set<DocumentoAdjunto>();
    public DbSet<HistorialCambio> HistorialCambios => Set<HistorialCambio>();
    public DbSet<Factura> Facturas => Set<Factura>();

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        StampAuditFields();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        StampAuditFields();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    private void StampAuditFields()
    {
        var usuario = _authContext?.NombreParaMostrar ?? _authContext?.NombreUsuario ?? "Sistema";
        var ahora = DateTime.UtcNow;

        foreach (var entry in ChangeTracker.Entries<IAuditable>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.FechaCreacion = ahora;
                    entry.Entity.CreadoPor = usuario;
                    break;

                case EntityState.Modified:
                    entry.Entity.FechaModificacion = ahora;
                    entry.Entity.ModificadoPor = usuario;
                    // No sobreescribir FechaCreacion/CreadoPor en edición
                    entry.Property(nameof(IAuditable.FechaCreacion)).IsModified = false;
                    entry.Property(nameof(IAuditable.CreadoPor)).IsModified = false;
                    break;
            }
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        ApplySoftDeleteQueryFilters(modelBuilder);
    }

    /// <summary>
    /// Aplica el filtro de soft delete a todas las entidades que implementan <see cref="ISoftDeletable"/>.
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
