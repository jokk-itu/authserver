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
        Name = Domain.Constants.ClientProfileConstants.WebApplication
      },
      new ClientProfile
      {
        Name = Domain.Constants.ClientProfileConstants.UserAgentApplication
      },
      new ClientProfile
      {
        Name = Domain.Constants.ClientProfileConstants.NativeApplication
      });

    builder.ToTable("ClientProfiles");
  }
}
