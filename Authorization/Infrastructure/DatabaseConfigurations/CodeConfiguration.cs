using Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.DatabaseConfigurations;
internal class CodeConfiguration : IEntityTypeConfiguration<Code>
{
  public void Configure(EntityTypeBuilder<Code> builder)
  {
    builder.HasOne(code => code.Client);
    builder
      .Property(code => code.CodeType)
      .HasConversion<string>();

    builder
      .Property(code => code.Value)
      .HasMaxLength(int.MaxValue);    

    builder.ToTable("Codes");
  }
}
