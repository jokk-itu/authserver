using Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.DatabaseConfigurations;
public class NonceConfiguration : IEntityTypeConfiguration<Nonce>
{
  public void Configure(EntityTypeBuilder<Nonce> builder)
  {
    builder
      .HasIndex(nonce => nonce.Value)
      .IsUnique(true);

    builder.ToTable("Nonces");
  }
}
