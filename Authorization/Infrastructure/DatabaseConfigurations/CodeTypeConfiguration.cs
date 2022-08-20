using Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.DatabaseConfigurations;
public class CodeTypeConfiguration : IEntityTypeConfiguration<CodeType>
{
  public void Configure(EntityTypeBuilder<CodeType> builder)
  {
    builder.HasData(
      new CodeType 
      {
        Name = Domain.Constants.CodeTypeConstants.AuthorizationCode
      },
      new CodeType 
      {
        Name = Domain.Constants.CodeTypeConstants.DeviceCode
      });

    builder.ToTable("CodeTypes");
  }
}
