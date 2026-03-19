using ContractSystem.Domain.Contratos;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ContractSystem.Infrastructure.Database.Contratos;

public class FacturaConfiguration : IEntityTypeConfiguration<Factura>
{
    public void Configure(EntityTypeBuilder<Factura> builder)
    {
        builder.ToTable("Facturas");

        builder.HasKey(f => f.Id);

        builder.Property(f => f.Numero).HasMaxLength(100).IsRequired();
        builder.Property(f => f.Fecha).IsRequired();
        builder.Property(f => f.ImporteTotal).HasPrecision(18, 2).IsRequired();
        builder.Property(f => f.Descripcion).HasMaxLength(1000).IsRequired();

        builder.HasOne(f => f.Contrato)
            .WithMany(c => c.Facturas)
            .HasForeignKey(f => f.ContratoId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
