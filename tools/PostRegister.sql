USE AuthServerTestIdentityProvider

DECLARE @ClientId UNIQUEIDENTIFIER = '91f61e84-a5d8-4896-9310-fc69c6e2829a'

update Client
set
	ClientUri = 'https://localhost:7226',
	BackchannelLogoutUri = 'https://localhost:7226/signout-oidc'
where Id = @ClientId

update RedirectUri
set Uri = 'https://localhost:7226/signin-oidc'
where ClientId = @ClientId

update
set Uri = 'https://localhost:7226/signout-callback-oidc'
where ClientId = @ClientId