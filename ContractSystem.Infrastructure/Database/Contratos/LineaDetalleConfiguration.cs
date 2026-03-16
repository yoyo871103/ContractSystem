using ContractSystem.Domain.Contratos;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ContractSystem.Infrastructure.Database.Contratos;

internal sealed class LineaDetalleConfiguration : IEntityTypeConfiguration<LineaDetalle>
{
    public void Configure(EntityTypeBuilder<LineaDetalle> builder)
    {
        builder.ToTable("LineasDetalle");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Tipo).IsRequired();
        builder.Property(e => e.Concepto).HasMaxLength(512).IsRequired();
        builder.Property(e => e.Descripcion).HasMaxLength(2048);
        builder.Property(e => e.Cantidad).HasColumnType("decimal(18,4)");
        builder.Property(e => e.UnidadMedidaTexto).HasMaxLength(64);
        builder.Property(e => e.PrecioUnitario).HasColumnType("decimal(18,4)");
        builder.Property(e => e.ImporteTotal).HasColumnType("decimal(18,2)");
        builder.Property(e => e.EsCopiaDeOriginal);
        builder.Property(e => e.Orden);

        builder.HasIndex(e => e.ContratoId);
        builder.HasIndex(e => e.AnexoId);

        builder.HasOne(e => e.Contrato)
            .WithMany()
            .HasForeignKey(e => e.ContratoId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.Anexo)
            .WithMany(a => a.Lineas)
            .HasForeignKey(e => e.AnexoId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
