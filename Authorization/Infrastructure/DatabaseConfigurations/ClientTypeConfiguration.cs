using Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.DatabaseConfigurations;
internal class ClientTypeConfiguration : IEntityTypeConfiguration<ClientType>
{
  public void Configure(EntityTypeBuilder<ClientType> builder)
  {
    builder.HasData(
      new ClientType
      {
        Id = 1,
        Name = Domain.Constants.ClientTypeConstants.Confidential
      },
      new ClientType
      {
        Id = 2,
        Name = Domain.Constants.ClientTypeConstants.Public
      });

    builder.ToTable("ClientTypes");
  }
}
