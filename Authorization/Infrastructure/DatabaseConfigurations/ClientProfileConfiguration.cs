using Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.DatabaseConfigurations;
internal class ClientProfileConfiguration : IEntityTypeConfiguration<ClientProfile>
{
  public void Configure(EntityTypeBuilder<ClientProfile> builder)
  {
    builder
      .HasIndex(clientProfile => clientProfile.Name)
      .IsUnique(true);

    builder.HasData(
      new ClientProfile 
      {
        Id = 1,
        Name = Domain.Constants.ClientProfileConstants.WebApplication
      },
      new ClientProfile
      {
        Id = 2,
        Name = Domain.Constants.ClientProfileConstants.UserAgentApplication
      },
      new ClientProfile
      {
        Id = 3,
        Name = Domain.Constants.ClientProfileConstants.NativeApplication
      });

    builder.ToTable("ClientProfiles");
  }
}
