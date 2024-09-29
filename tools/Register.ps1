$body = @{
	client_name = 'test-client'
	grant_types = 'authorization_code','refresh_token'
	scope = 'address openid authserver:userinfo phone email profile'
	redirect_uris = ,'https://testclient.demo.authserver.dk/signin-oidc'
	post_logout_redirect_uris = ,'https://testclient.demo.authserver.dk/signout-callback-oidc'
	token_endpoint_auth_method = 'client_secret_post'
	backchannel_logout_uri = 'https://testclient.demo.authserver.dk/signout-oidc'
} | ConvertTo-Json

$response = Invoke-WebRequest `
	-Method POST `
	-Uri 'https://localhost:7254/connect/register' `
	-Body $body `
	-ContentType 'application/json'

$response.Content | Write-Host