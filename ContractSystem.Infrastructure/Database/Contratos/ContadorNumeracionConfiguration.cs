using ContractSystem.Domain.Contratos;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ContractSystem.Infrastructure.Database.Contratos;

internal sealed class ContadorNumeracionConfiguration : IEntityTypeConfiguration<ContadorNumeracion>
{
    public void Configure(EntityTypeBuilder<ContadorNumeracion> builder)
    {
        builder.ToTable("ContadoresNumeracion");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Anio);
        builder.Property(e => e.UltimoNumero).IsRequired();

        // Índice único: solo un contador por año (o uno global con Anio=null)
        builder.HasIndex(e => e.Anio).IsUnique();
    }
}
