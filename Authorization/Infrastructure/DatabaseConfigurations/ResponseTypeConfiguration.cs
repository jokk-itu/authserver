using Domain;
using Domain.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.DatabaseConfigurations;
public class ResponseTypeConfiguration : IEntityTypeConfiguration<ResponseType>
{
  public void Configure(EntityTypeBuilder<ResponseType> builder)
  {
    builder.HasData(new ResponseType
    {
      Id = 1,
      Name = ResponseTypeConstants.Code
    });

    builder.ToTable("ResponseTypes");
  }
}
