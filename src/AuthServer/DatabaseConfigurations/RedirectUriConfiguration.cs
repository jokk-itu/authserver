using AuthServer.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuthServer.DatabaseConfigurations;

internal sealed class RedirectUriConfiguration : IEntityTypeConfiguration<RedirectUri>
{
	public void Configure(EntityTypeBuilder<RedirectUri> builder)
	{
		builder
			.Property(x => x.Uri)
			.HasMaxLength(255)
			.IsRequired();

		builder
			.HasOne(x => x.Client)
			.WithMany(x => x.RedirectUris)
			.IsRequired()
			.OnDelete(DeleteBehavior.Cascade);
	}
}