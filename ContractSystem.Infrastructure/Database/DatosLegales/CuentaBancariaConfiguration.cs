using ContractSystem.Domain.DatosLegales;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ContractSystem.Infrastructure.Database.DatosLegales;

internal sealed class CuentaBancariaConfiguration : IEntityTypeConfiguration<CuentaBancaria>
{
    public void Configure(EntityTypeBuilder<CuentaBancaria> builder)
    {
        builder.ToTable("CuentasBancarias");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Numero).HasMaxLength(64).IsRequired();
        builder.Property(e => e.Moneda).HasMaxLength(10).IsRequired();
        builder.Property(e => e.Banco).HasMaxLength(128).IsRequired();
        builder.Property(e => e.Sucursal).HasMaxLength(128);
        builder.Property(e => e.DireccionSucursal).HasMaxLength(256);
        builder.Property(e => e.Titular).HasMaxLength(256);

        builder.HasOne(e => e.BusinessInfo)
            .WithMany(b => b.CuentasBancarias)
            .HasForeignKey(e => e.BusinessInfoId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.Tercero)
            .WithMany(t => t.CuentasBancarias)
            .HasForeignKey(e => e.TerceroId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
