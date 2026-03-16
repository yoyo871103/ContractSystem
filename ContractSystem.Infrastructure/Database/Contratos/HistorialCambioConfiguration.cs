using ContractSystem.Domain.Contratos;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ContractSystem.Infrastructure.Database.Contratos;

public class HistorialCambioConfiguration : IEntityTypeConfiguration<HistorialCambio>
{
    public void Configure(EntityTypeBuilder<HistorialCambio> builder)
    {
        builder.ToTable("HistorialCambios");

        builder.HasKey(h => h.Id);

        builder.Property(h => h.FechaHora).IsRequired();
        builder.Property(h => h.UsuarioNombre).HasMaxLength(200);
        builder.Property(h => h.Descripcion).HasMaxLength(2000).IsRequired();
        builder.Property(h => h.TipoCambio).IsRequired();

        builder.HasOne(h => h.Contrato)
            .WithMany()
            .HasForeignKey(h => h.ContratoId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(h => h.ContratoId);
        builder.HasIndex(h => h.FechaHora);
    }
}
