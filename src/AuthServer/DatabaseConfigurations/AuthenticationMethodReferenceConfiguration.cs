using AuthServer.Constants;
using AuthServer.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuthServer.DatabaseConfigurations;

internal sealed class AuthenticationMethodReferenceConfiguration : IEntityTypeConfiguration<AuthenticationMethodReference>
{
    private sealed record AuthenticationMethodReferenceSeed(int Id, string Name);

    public void Configure(EntityTypeBuilder<AuthenticationMethodReference> builder)
    {
        builder
            .Property(x => x.Name)
            .HasMaxLength(255)
            .IsRequired();

        builder
            .HasIndex(x => x.Name)
            .IsUnique();

        builder.HasData(new AuthenticationMethodReferenceSeed(1, AuthenticationMethodReferenceConstants.Password),
            new AuthenticationMethodReferenceSeed(2, AuthenticationMethodReferenceConstants.MultiFactorAuthentication),
            new AuthenticationMethodReferenceSeed(3, AuthenticationMethodReferenceConstants.Sms),
            new AuthenticationMethodReferenceSeed(4, AuthenticationMethodReferenceConstants.Face),
            new AuthenticationMethodReferenceSeed(5, AuthenticationMethodReferenceConstants.Fingerprint),
            new AuthenticationMethodReferenceSeed(6, AuthenticationMethodReferenceConstants.Geo),
            new AuthenticationMethodReferenceSeed(7, AuthenticationMethodReferenceConstants.Iris),
            new AuthenticationMethodReferenceSeed(8, AuthenticationMethodReferenceConstants.KnowledgeBasedAuthentication),
            new AuthenticationMethodReferenceSeed(9, AuthenticationMethodReferenceConstants.MultipleChannelAuthentication),
            new AuthenticationMethodReferenceSeed(10, AuthenticationMethodReferenceConstants.OneTimePassword),
            new AuthenticationMethodReferenceSeed(11, AuthenticationMethodReferenceConstants.PersonalIdentificationNumber),
            new AuthenticationMethodReferenceSeed(12, AuthenticationMethodReferenceConstants.ProofOfPossessionHardwareKey),
            new AuthenticationMethodReferenceSeed(13, AuthenticationMethodReferenceConstants.ProofOfPossessionKey),
            new AuthenticationMethodReferenceSeed(14, AuthenticationMethodReferenceConstants.ProofOfPossessionSoftwareKey),
            new AuthenticationMethodReferenceSeed(15, AuthenticationMethodReferenceConstants.Retina),
            new AuthenticationMethodReferenceSeed(16, AuthenticationMethodReferenceConstants.RiskBasedAuthentication),
            new AuthenticationMethodReferenceSeed(17, AuthenticationMethodReferenceConstants.SmartCard),
            new AuthenticationMethodReferenceSeed(18, AuthenticationMethodReferenceConstants.TelephoneCall),
            new AuthenticationMethodReferenceSeed(19, AuthenticationMethodReferenceConstants.User),
            new AuthenticationMethodReferenceSeed(20, AuthenticationMethodReferenceConstants.Voice),
            new AuthenticationMethodReferenceSeed(21, AuthenticationMethodReferenceConstants.WindowsIntegratedAuthentication));
    }
}