using ContractSystem.Domain.Contratos;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ContractSystem.Infrastructure.Database.Contratos;

internal sealed class ContratoConfiguration : IEntityTypeConfiguration<Contrato>
{
    public void Configure(EntityTypeBuilder<Contrato> builder)
    {
        builder.ToTable("Contratos");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Numero)
            .HasMaxLength(128)
            .IsRequired();

        builder.Property(e => e.Objeto)
            .HasMaxLength(2048);

        builder.Property(e => e.TipoDocumento)
            .IsRequired();

        builder.Property(e => e.Rol)
            .IsRequired();

        builder.Property(e => e.Estado)
            .IsRequired();

        builder.Property(e => e.FechaFirma);
        builder.Property(e => e.FechaEntradaVigor);
        builder.Property(e => e.FechaVigencia);

        builder.Property(e => e.Duracion)
            .HasMaxLength(256);

        builder.Property(e => e.Ejecutado);
        builder.Property(e => e.FechaEjecucion);

        builder.Property(e => e.ValorTotal)
            .HasColumnType("decimal(18,2)");

        builder.Property(e => e.CondicionesEntrega)
            .HasMaxLength(4000);

        builder.Property(e => e.CostosAsociados)
            .HasMaxLength(4000);

        builder.Property(e => e.EsModificacionGenerales);

        builder.Property(e => e.FechaCreacion);
        builder.Property(e => e.UsuarioCreacionId);
        builder.Property(e => e.DeletedAt);

        // Índices
        builder.HasIndex(e => e.Numero);
        builder.HasIndex(e => e.TipoDocumento);
        builder.HasIndex(e => e.Estado);
        builder.HasIndex(e => e.Rol);
        builder.HasIndex(e => e.FechaFirma);
        builder.HasIndex(e => e.FechaVigencia);
        builder.HasIndex(e => e.TerceroId);
        builder.HasIndex(e => e.ContratoPadreId);

        // Relación jerárquica (auto-referencia)
        builder.HasOne(e => e.ContratoPadre)
            .WithMany(e => e.Hijos)
            .HasForeignKey(e => e.ContratoPadreId)
            .OnDelete(DeleteBehavior.Restrict);

        // Relación con BusinessInfo (Mi Empresa)
        builder.HasOne(e => e.MiEmpresa)
            .WithMany()
            .HasForeignKey(e => e.MiEmpresaId)
            .OnDelete(DeleteBehavior.SetNull);

        // Relación con Tercero (Contraparte)
        builder.HasOne(e => e.Tercero)
            .WithMany()
            .HasForeignKey(e => e.TerceroId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
