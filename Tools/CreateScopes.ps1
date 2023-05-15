function GetInitialToken([Parameter(Mandatory)][string]$Uri) {
    $response = Invoke-WebRequest -Uri $Uri -Method 'GET'
    $token = ConvertFrom-Json $response.Content | Select -Expand 'access_token'
    return $token
}

function PostScope([Parameter(Mandatory)][string]$Uri, [Parameter(Mandatory)][string]$Token, [Parameter(Mandatory)][string]$ScopeName) {
    $body = @{
        'scope_name' = $ScopeName
    }
    $headers = @{
        'Authorization' = "Bearer $($Token)"
    }
    $response = Invoke-WebRequest -Uri $Uri -Method 'POST' -Headers $headers -Body ($body | ConvertTo-Json) -ContentType 'application/json'
    return $response.Content
}

$token = GetInitialToken 'https://localhost:5000/connect/scope/initial-token'
PostScope 'https://localhost:5000/connect/scope/register' $token 'weather:read'
PostScope 'https://localhost:5000/connect/scope/register' $token 'identityprovider:userinfo'