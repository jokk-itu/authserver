using Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.DatabaseConfigurations;
internal class NonceConfiguration : IEntityTypeConfiguration<Nonce>
{
  public void Configure(EntityTypeBuilder<Nonce> builder)
  {
    builder
      .Property(x => x.Value)
      .IsRequired();
  }
}
