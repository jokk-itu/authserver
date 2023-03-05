function GetInitialToken([Parameter(Mandatory)][string]$Uri) {
    $response = Invoke-WebRequest -Uri $Uri -Method 'GET'
    $token = ConvertFrom-Json $response.Content | Select -Expand 'access_token'
    return $token
}

function PostClient([Parameter()][string]$Uri, [Parameter(Mandatory)][string]$Token, [Parameter(Mandatory)]$Body) {
    $headers = @{
        'Authorization' = "Bearer $($Token)"
    }
    $response = Invoke-WebRequest -Uri $Uri -Method 'POST' -Headers $headers -Body ($body | ConvertTo-Json) -ContentType 'application/json'
    return $response.Content
}

function CreateWebApp() {
    $token = GetInitialToken 'https://localhost:5000/connect/client/initial-token'
    $body = @{
        'redirect_uris' = @('https://localhost:5002/callback')
        'response_types' = @('code')
        'grant_types' = @('authorization_code', 'refresh_token')
        'application_type' = 'web'
        'contacts' = @('test@gmail.com')
        'client_name' = 'webapp'
        'policy_uri' = 'https://localhost:5002/policy'
        'tos_uri' = 'https://localhost:5002/tos'
        'subject_type' = 'public'
        'token_endpoint_auth_method' = 'client_secret_post'
        'scope' = 'openid profile email phone offline_access weather:read identityprovider:userinfo'
        'client_uri' = 'https://localhost:5002'
        'logo_uri' = 'https://www.gravatar.com/avatar/b6920d9083c8e76685bcc8db34b8c9bb?d=identicon'
        'initiate_login_uri' = 'https://localhost:5002/login'
        'default_max_age' = '3600'
    }
    PostClient 'https://localhost:5000/connect/client/register' $token $body
}

function CreateWorker() {
    $token = GetInitialToken 'https://localhost:5000/connect/client/initial-token'
    $body = @{
        'grant_types' = @('client_credentials')
        'client_name' = 'worker'
        'token_endpoint_auth_method' = 'client_secret_post'
        'scope' = 'weather:read'
    }
    PostClient 'https://localhost:5000/connect/client/register' $token $body
}

function CreateWasm() {
    $token = GetInitialToken 'https://localhost:5000/connect/client/initial-token'
    $body = @{
        'redirect_uris' = @('https://localhost:5003/callback')
        'response_types' = @('code')
        'grant_types' = @('authorization_code', 'refresh_token')
        'application_type' = 'web'
        'contacts' = @('test@gmail.com')
        'client_name' = 'wasm'
        'policy_uri' = 'https://localhost:5003/policy'
        'tos_uri' = 'https://localhost:5003/tos'
        'subject_type' = 'public'
        'token_endpoint_auth_method' = 'client_secret_post'
        'scope' = 'openid profile email phone offline_access weather:read identityprovider:userinfo'
        'client_uri' = 'https://localhost:5003'
        'logo_uri' = 'https://www.gravatar.com/avatar/7c15733998c9b7862e44dc6364646286?d=identicon'
        'initiate_logo_uri' = 'https://localhost:5003/login'
    }
    PostClient 'https://localhost:5000/connect/client/register' $token $body
}

function CreateHybrid() {
    $token = GetInitialToken 'https://localhost:5000/connect/client/initial-token'
    $body = @{
        'redirect_uris' = @('hybridapp://callback')
        'response_types' = @('code')
        'grant_types' = @('authorization_code', 'refresh_token')
        'application_type' = 'native'
        'contacts' = @('test@gmail.com')
        'client_name' = 'hybridapp'
        'subject_type' = 'public'
        'token_endpoint_auth_method' = 'none'
        'scope' = 'openid profile email phone offline_access weather:read identityprovider:userinfo'
        'logo_uri' = 'https://www.gravatar.com/avatar/30bc082fbe72e3c7a5ac3316095e106d?d=identicon'
    }
    PostClient 'https://localhost:5000/connect/client/register' $token $body
}

CreateWebApp
CreateWorker
CreateWasm
CreateHybrid