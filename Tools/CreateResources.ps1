function GetInitialToken([Parameter(Mandatory)][string]$Uri) {
    $response = Invoke-WebRequest -Uri $Uri -Method 'GET'
    $token = ConvertFrom-Json $response.Content | Select -Expand 'access_token'
    return $token
}

function PostResource([Parameter()][string]$Uri, [Parameter(Mandatory)][string]$Token, [Parameter(Mandatory)][string]$Name, [Parameter(Mandatory)][string]$Scope) {
    $body = @{
        'resource_name' = $Name
        'scope' = $Scope
    }
    $headers = @{
        'Authorization' = "Bearer $($Token)"
    }
    $response = Invoke-WebRequest -Uri $Uri -Method 'POST' -Headers $headers -Body ($body | ConvertTo-Json) -ContentType 'application/json'
    return $response.Content
}
$token = GetInitialToken 'http://localhost:5000/connect/resource/initial-token'

PostResource 'http://localhost:5000/connect/resource/register' $token 'weather' 'weather:read'

PostResource 'http://localhost:5000/connect/resource/register' $token 'identityprovider' 'identityprovider:userinfo'