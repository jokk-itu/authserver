function GetInitialToken([Parameter(Mandatory)][string]$Uri) {
    $response = Invoke-WebRequest -Uri $Uri -Method 'GET'
    $token = ConvertFrom-Json $response.Content | Select -Expand 'access_token'
    return $token
}

function PostClient([Parameter()][string]$Uri, [Parameter(Mandatory)][string]$Token) {
    $body = @{
        'redirect_uris' = @('http://localhost:5002/callback')
        'response_types' = @('code')
        'grant_types' = @('authorization_code', 'refresh_token')
        'application_type' = 'web'
        'contacts' = @('test@gmail.com')
        'client_name' = 'webapp'
        'policy_uri' = 'http://localhost:5002/policy'
        'tos_uri' = 'http://localhost:5002/tos'
        'subject_type' = 'public'
        'token_endpoint_auth_method' = 'client_secret_post'
        'scope' = 'openid profile email phone offline_access weather:read identityprovider:read'
    }
    $headers = @{
        'Authorization' = "Bearer $($Token)"
    }
    $response = Invoke-WebRequest -Uri $Uri -Method 'POST' -Headers $headers -Body ($body | ConvertTo-Json) -ContentType 'application/json'
    return $response.Content
}
$token = GetInitialToken 'http://localhost:5000/connect/client/initial-token'

PostClient 'http://localhost:5000/connect/client/register' $token