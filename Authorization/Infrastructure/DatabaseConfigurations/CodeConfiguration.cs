using Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.DatabaseConfigurations;
internal class CodeConfiguration : IEntityTypeConfiguration<Code>
{
  public void Configure(EntityTypeBuilder<Code> builder)
  {
    builder.HasOne(code => code.Client);
    builder.HasOne(code => code.CodeType);

    builder
      .HasIndex(code => code.Value)
      .IsUnique(true);

    builder.ToTable("Codes");
  }
}
