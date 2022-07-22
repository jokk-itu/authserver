<#
    .SYNOPSIS
        .
    .DESCRIPTION
        .
    .PARAMETER KibanaUrl
       
    .PARAMETER ElasticUrl
       
    .PARAMETER Index
       
    .PARAMETER Policy
      
#>

param (
    [Parameter()][string]$KibanaUrl="http://localhost:5601",
    [Parameter()][string]$ElasticUrl="http://localhost:9200",
    [Parameter(Mandatory=$true)][string]$Index,
    [Parameter(Mandatory=$true)][string]$Policy
)