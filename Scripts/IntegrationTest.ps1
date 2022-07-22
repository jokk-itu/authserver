<#
    .SYNOPSIS
        dd

    .DESCRIPTION
        dd

    .PARAMETER AuthorizationServerUri
        dd
#>

param (
  [Parameter()][string]$AuthorizationServerUri="http://localhost:5000"
)


try {
    $ResetPasswordBody = @{
      Username = "jokk"
      Password = "Password12!"
    }
    $ResetPasswordResponse = Invoke-WebRequest -Uri $AuthorizationServerUri/connect/v1/account/reset/password -Body ($ResetPasswordBody | ConvertTo-Json) -Method POST -ContentType "application/json"
    Write-Host "Request to ResetPassword Completed with $($ResetPasswordResponse.StatusCode)" 
}
catch [System.Net.WebException] {
  Write-Host "Exception details: "
  $e = $_.Exception
  Write-Host ("`tMessage: " + $e.Message)
  Write-Host ("`tStatus code: " + $e.Response.StatusCode)
  Write-Host ("`tStatus description: " + $e.Response.StatusDescription)
  Write-Host "`tResponse: " -NoNewline
  $memStream = $e.Response.GetResponseStream()
  $readStream = New-Object System.IO.StreamReader($memStream)
  while ($readStream.Peek() -ne -1) {
    Write-Host $readStream.ReadLine()
  }
  $readStream.Dispose();
}
catch {
  Write-Host "An unknown error occured `n $_"
}