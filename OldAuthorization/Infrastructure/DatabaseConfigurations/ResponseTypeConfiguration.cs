using Domain;
using Domain.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.DatabaseConfigurations;
internal class ResponseTypeConfiguration : IEntityTypeConfiguration<ResponseType>
{
  public void Configure(EntityTypeBuilder<ResponseType> builder)
  {
    builder.HasData(new ResponseType
    {
      Id = 1,
      Name = ResponseTypeConstants.Code
    });

    builder
      .Property(x => x.Name)
      .IsRequired();
  }
}
