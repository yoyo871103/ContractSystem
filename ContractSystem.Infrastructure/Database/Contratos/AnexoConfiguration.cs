using ContractSystem.Domain.Contratos;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ContractSystem.Infrastructure.Database.Contratos;

internal sealed class AnexoConfiguration : IEntityTypeConfiguration<Anexo>
{
    public void Configure(EntityTypeBuilder<Anexo> builder)
    {
        builder.ToTable("Anexos");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Nombre).HasMaxLength(256).IsRequired();
        builder.Property(e => e.CondicionesEntrega).HasMaxLength(4000);
        builder.Property(e => e.CostosAsociados).HasMaxLength(4000);
        builder.Property(e => e.Orden);

        builder.HasIndex(e => e.ContratoId);

        builder.HasOne(e => e.Contrato)
            .WithMany(c => c.Anexos)
            .HasForeignKey(e => e.ContratoId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
