using ContractSystem.Domain.Nomencladores;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ContractSystem.Infrastructure.Database.Nomencladores;

internal sealed class ProductoServicioConfiguration : IEntityTypeConfiguration<ProductoServicio>
{
    public void Configure(EntityTypeBuilder<ProductoServicio> builder)
    {
        builder.ToTable("nom_ProductosServicios");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Codigo)
            .HasMaxLength(64);

        builder.Property(e => e.Nombre)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(e => e.Descripcion)
            .HasMaxLength(1024);

        builder.Property(e => e.Tipo)
            .IsRequired();

        builder.Property(e => e.PrecioEstimado)
            .HasColumnType("decimal(18,2)");

        builder.Property(e => e.DeletedAt);

        builder.HasIndex(e => e.Codigo);
        builder.HasIndex(e => e.Tipo);

        builder.HasOne(e => e.UnidadMedida)
            .WithMany()
            .HasForeignKey(e => e.UnidadMedidaId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
