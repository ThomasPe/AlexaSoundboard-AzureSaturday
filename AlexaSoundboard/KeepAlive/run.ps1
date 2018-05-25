Write-Output "KeepAlive trigger function executed at:$(get-date)";
$Response = Invoke-WebRequest https://alexasoundboardskill.azurewebsites.net/api/alexa-soundboard -Method POST
Write-Output $Response.StatusCode